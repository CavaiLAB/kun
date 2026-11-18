"""工作流自测用的 YAML 子集读取器（S1-KUN-020）。只依赖标准库。

仓库里的 Python 工具只用标准库，没有 YAML 库。这里只读工作流里用到的写法：块状映射与块状列表（含「- 键: 值」）、
单行的流式列表 [a, 'b']、单引号与双引号标量、| 与 > 块标量、注释、锚点 &名字 与别名 *名字。
别的写法（流式映射、跨行的普通标量、标签等）直接报错，不猜。键一律按字符串读，on 不会变成 true。
"""
from __future__ import annotations

import re
from typing import Any, Dict, List, Optional, Tuple

KEY = re.compile(r"""^(?:"(?P<dq>[^"]*)"|'(?P<sq>[^']*)'|(?P<plain>[^\s'"#\[\]{},&*!|>%@`-][^#]*?))\s*:(?:\s+(?P<value>.*))?$""")
BLOCK = re.compile(r"^[|>][-+]?\d*$")
ANCHOR = re.compile(r"^&([^\s\[\]{},]+)(?:\s+(.*))?$")
ALIAS = re.compile(r"^\*([^\s\[\]{},]+)$")
BLOCK_LINE = -1  # 块标量的正文占一行，缩进记作 -1


class YamlError(ValueError):
    pass


def strip_comment(text: str) -> str:
    """去掉行尾注释：# 前面是空白且不在引号里。"""
    quote: Optional[str] = None
    index = 0
    while index < len(text):
        char = text[index]
        if quote:
            if char == quote:
                if quote == "'" and text[index + 1:index + 2] == "'":
                    index += 2
                    continue
                quote = None
            elif char == "\\" and quote == '"':
                index += 2
                continue
        elif char in "'\"" and (index == 0 or text[index - 1] in " \t[,:"):
            quote = char
        elif char == "#" and (index == 0 or text[index - 1] in " \t"):
            return text[:index].rstrip()
        index += 1
    return text.rstrip()


def scalar(text: str) -> str:
    text = text.strip()
    if len(text) >= 2 and text[0] == text[-1] == "'":
        return text[1:-1].replace("''", "'")
    if len(text) >= 2 and text[0] == text[-1] == '"':
        return re.sub(r"\\(.)", lambda match: {"n": "\n", "t": "\t"}.get(match.group(1), match.group(1)), text[1:-1])
    return text


def flow_list(text: str) -> List[str]:
    inner = text.strip()
    if not (inner.startswith("[") and inner.endswith("]")):
        raise YamlError(f"只认单行的流式列表：{text}")
    items: List[str] = []
    current = ""
    quote: Optional[str] = None
    for char in inner[1:-1]:
        if quote:
            current += char
            if char == quote:
                quote = None
        elif char in "'\"":
            quote = char
            current += char
        elif char == ",":
            items.append(current)
            current = ""
        else:
            current += char
    if current.strip():
        items.append(current)
    return [scalar(item) for item in items if item.strip()]


def block_scalar(indicator: str, body: str) -> str:
    """| 保留换行，> 把相邻的非空行折成一行；末尾换行按 - 去掉、+ 保留、缺省留一个。"""
    lines = body.split("\n")
    width = min((len(line) - len(line.lstrip(" ")) for line in lines if line.strip()), default=0)
    lines = [line[width:] for line in lines]
    while lines and not lines[-1].strip():
        lines.pop()
    if indicator.startswith(">"):
        folded: List[str] = []
        for line in lines:
            if folded and line.strip() and folded[-1].strip() and not line.startswith(" "):
                folded[-1] += " " + line
            else:
                folded.append(line)
        lines = folded
    text = "\n".join(lines)
    if "-" in indicator:
        return text
    return text + "\n" if text else text


def trailing_value(content: str) -> Optional[str]:
    """一行里「键:」或「- 」后面的值，用来认块标量。"""
    body = content[2:].strip() if content.startswith("- ") else content
    match = KEY.match(body)
    if match:
        return match.group("value")
    return body if content.startswith("- ") else None


class Loader:
    def __init__(self, text: str) -> None:
        self.lines: List[Tuple[int, str, int]] = []  # 缩进、去掉注释后的内容、原行号
        raw = text.splitlines()
        index = 0
        while index < len(raw):
            line = raw[index]
            content = line.lstrip(" ")
            if not content or content.startswith("#"):
                index += 1
                continue
            indent = len(line) - len(content)
            stripped = strip_comment(content)
            self.lines.append((indent, stripped, index + 1))
            index += 1
            value = trailing_value(stripped)
            if value is not None and BLOCK.match(value.strip()):
                # 块标量：后面缩进更深的行（含空行）原样收下，不当结构读。
                body: List[str] = []
                while index < len(raw) and (not raw[index].strip() or len(raw[index]) - len(raw[index].lstrip(" ")) > indent):
                    body.append(raw[index])
                    index += 1
                self.lines.append((BLOCK_LINE, "\n".join(body), index))
        self.anchors: Dict[str, Any] = {}
        self.position = 0

    def peek(self) -> Optional[Tuple[int, str, int]]:
        return self.lines[self.position] if self.position < len(self.lines) else None

    def value(self, text: Optional[str], indent: int) -> Any:
        """键或列表项后面的值；text 为空时值在下面更深缩进的行里。"""
        if text is None or not text.strip():
            line = self.peek()
            if line is None or line[0] < indent or line[0] == indent and not line[1].startswith("- "):
                return None
            return self.node(line[0])
        text = text.strip()
        anchor = ANCHOR.match(text)
        if anchor:
            node = self.value(anchor.group(2), indent)
            self.anchors[anchor.group(1)] = node
            return node
        alias = ALIAS.match(text)
        if alias:
            if alias.group(1) not in self.anchors:
                raise YamlError(f"别名 *{alias.group(1)} 前面没有对应的锚点。")
            return self.anchors[alias.group(1)]
        if BLOCK.match(text):
            line = self.peek()
            if line is None or line[0] != BLOCK_LINE:
                return ""
            self.position += 1
            return block_scalar(text, line[1])
        if text.startswith("["):
            return flow_list(text)
        if text.startswith("{"):
            raise YamlError(f"不认流式映射：{text}")
        return scalar(text)

    def node(self, indent: int) -> Any:
        line = self.peek()
        assert line is not None
        return self.sequence(indent) if line[1].startswith("- ") or line[1] == "-" else self.mapping(indent)

    def mapping(self, indent: int) -> Dict[str, Any]:
        result: Dict[str, Any] = {}
        while True:
            line = self.peek()
            if line is None or line[0] < indent or line[0] == indent and line[1].startswith("- "):
                return result
            if line[0] > indent:
                raise YamlError(f"第 {line[2]} 行缩进不对：{line[1]}")
            match = KEY.match(line[1])
            if not match:
                raise YamlError(f"第 {line[2]} 行不是「键: 值」：{line[1]}")
            key = next(group for group in (match.group("dq"), match.group("sq"), match.group("plain")) if group is not None).strip()
            self.position += 1
            if key in result:
                raise YamlError(f"第 {line[2]} 行键 {key} 重复。")
            result[key] = self.value(match.group("value"), indent)

    def sequence(self, indent: int) -> List[Any]:
        result: List[Any] = []
        while True:
            line = self.peek()
            if line is None or line[0] < indent or not (line[1].startswith("- ") or line[1] == "-"):
                return result
            if line[0] > indent:
                raise YamlError(f"第 {line[2]} 行缩进不对：{line[1]}")
            item = line[1][2:].strip() if line[1] != "-" else ""
            if item and KEY.match(item):
                # 「- 键: 值」：这一行当作缩进在键所在列的映射的第一行。
                column = line[0] + 2
                self.lines[self.position] = (column, item, line[2])
                result.append(self.mapping(column))
            else:
                self.position += 1
                result.append(self.value(item, indent + 1))


def load(text: str) -> Any:
    loader = Loader(text)
    first = loader.peek()
    if first is None:
        return None
    node = loader.node(first[0])
    rest = loader.peek()
    if rest is not None:
        raise YamlError(f"第 {rest[2]} 行没读到：{rest[1]}")
    return node


def pull_request_paths(document: Any) -> Optional[List[str]]:
    """工作流的 on.pull_request.paths；没有拉取请求触发或没写 paths 时返回 None（即不过滤）。"""
    triggers = (document or {}).get("on")
    if not isinstance(triggers, dict):
        return None
    pull_request = triggers.get("pull_request")
    if not isinstance(pull_request, dict) or "paths" not in pull_request:
        return None
    paths = pull_request["paths"]
    if not isinstance(paths, list) or not all(isinstance(path, str) for path in paths):
        raise YamlError("on.pull_request.paths 应是字符串列表。")
    return paths
