import io
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch
from urllib.error import HTTPError
import zipfile

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from release_bundle import bundle_record, resolve
from release_finish import version_section
from release_push import compare_package, payload, push_package


class ReleaseWorkflowTests(unittest.TestCase):
    def test_manifest_covers_the_entire_delivery_and_source(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            (root / "packages").mkdir()
            for name in ("packages/Kun.Core.2026.11.20.nupkg", "release-packages.json", "NuGet.config", "release-notes.md"):
                (root / name).write_text(name)
            def record(version="2026.11.20", sha="a" * 40, run="123"):
                return bundle_record(root, version, sha, run)
            expected = record()
            (root / "manifest.json").write_text(json.dumps(expected))
            self.assertEqual(record(), expected)
            self.assertNotEqual(record(run="124"), expected)
            self.assertNotEqual(record(sha="b" * 40), expected)
            self.assertNotEqual(record(version="2026.11.21"), expected)
            (root / "NuGet.config").write_text("changed")
            self.assertNotEqual(record()["sha256"], expected["sha256"])
            (root / "packages/extra.txt").touch()
            with self.assertRaises(ValueError):
                record()

    def test_resume_requires_published_workflow_main_and_exact_ci(self):
        sha = "a" * 40
        repo = "CavaiLAB/kun"
        original = {"workflow_id": 12, "head_repository": {"full_name": repo}, "event": "workflow_dispatch",
                    "head_branch": "main", "head_sha": sha, "status": "completed"}
        with tempfile.TemporaryDirectory() as folder:
            output = Path(folder) / "output"
            env = {"GITHUB_REPOSITORY": repo, "GITHUB_RUN_ID": "125", "GITHUB_SHA": "b" * 40, "GITHUB_OUTPUT": str(output)}
            def exercise(run, ci):
                responses = [json.dumps({"id": 12}), json.dumps(run), json.dumps(ci)]
                with patch.dict(os.environ, env), patch("release_bundle.command", side_effect=responses), patch.object(subprocess, "run"):
                    resolve("2026.11.20", "123")
            success = [{"event": "push", "headSha": sha}]
            exercise(original, success)
            self.assertEqual(output.read_text(), f"sha={sha}\nrun_id=123\nkind=stable\n")
            for changed in ({"workflow_id": 13}, {"head_repository": {"full_name": "example/kun"}},
                            {"event": "pull_request"}, {"head_branch": "topic"}, {"status": "in_progress"}):
                with self.subTest(changed=changed), self.assertRaises(ValueError):
                    exercise({**original, **changed}, success)
            for ci in ([], [{"event": "pull_request", "headSha": sha}], [{"event": "push", "headSha": "c" * 40}]):
                with self.subTest(ci=ci), self.assertRaises(ValueError):
                    exercise(original, ci)

    def test_symbol_upload_can_resume_after_the_main_package_exists(self):
        package = Path("Kun.Core.2026.11.20.nupkg")
        config = Path("NuGet.config")
        for feed, symbols, expected in (("nuget", True, 2), ("nuget", False, 1), ("github", True, 1)):
            with self.subTest(feed=feed, symbols=symbols), patch.dict(os.environ, NUGET_API_KEY="local", GITHUB_TOKEN="local"):
                with patch("release_push.subprocess.run") as run_cli:
                    push_package(package, config, feed, symbols)
            calls = [call.args[0] for call in run_cli.call_args_list]
            self.assertEqual(len(calls), expected)
            self.assertIn("--no-symbols", calls[0])
            self.assertEqual(calls[0][3], str(package))
            self.assertTrue(all("--skip-duplicate" in args for args in calls))
            if expected == 2:
                self.assertEqual(calls[1][3], str(package.with_suffix(".snupkg")))

    def test_registry_signature_does_not_change_package_payload(self):
        with tempfile.TemporaryDirectory() as folder:
            pack = Path(folder) / "a.nupkg"
            with zipfile.ZipFile(pack, "w") as archive:
                archive.writestr("a.nuspec", "manifest")
                archive.writestr("lib/net10.0/a.dll", b"assembly")
            original = payload(pack)
            with zipfile.ZipFile(pack, "a") as archive:
                archive.writestr(".signature.p7s", b"repository signature")
            self.assertEqual(original, payload(pack))
            with zipfile.ZipFile(pack, "w") as archive:
                archive.writestr("a.nuspec", "different")
                archive.writestr("lib/net10.0/a.dll", b"assembly")
            self.assertNotEqual(original, payload(pack))

    def test_pre_push_comparison_accepts_only_absent_or_identical_packages(self):
        identifier, version = "Kun.Core", "2026.11.20"
        base = "https://feed.example/packages/"
        with tempfile.TemporaryDirectory() as folder:
            file = Path(folder) / f"{identifier}.{version}.nupkg"
            def write_package(contents):
                with zipfile.ZipFile(file, "w") as archive:
                    archive.writestr("lib/net10.0/Kun.Core.dll", contents)
            write_package(b"previous build")
            other = file.read_bytes()
            write_package(b"approved build")
            for allow_missing in (True, False):
                with patch("release_push.read_url", return_value=io.BytesIO(file.read_bytes())) as read:
                    self.assertTrue(compare_package(base, identifier, version, file, allow_missing=allow_missing))
                    read.assert_called_once_with(base + "kun.core/2026.11.20/kun.core.2026.11.20.nupkg")
                with patch("release_push.read_url", return_value=io.BytesIO(other)), self.assertRaisesRegex(ValueError, identifier):
                    compare_package(base, identifier, version, file, allow_missing=allow_missing)
                for status in (401, 404, 503):
                    with self.subTest(allow_missing=allow_missing, status=status):
                        error = HTTPError(base, status, "test", {}, io.BytesIO())
                        with patch("release_push.read_url", side_effect=error):
                            if allow_missing and status == 404:
                                self.assertFalse(compare_package(base, identifier, version, file, allow_missing=True))
                            else:
                                with self.assertRaises(HTTPError):
                                    compare_package(base, identifier, version, file, allow_missing=allow_missing)

    def test_changelog_renders_the_release_date_without_rewriting_history(self):
        original = "# Kun\n\n## [Unreleased]\n\n- 变化\n\n## [2026.10.25] - 2026-10-25\n"
        result = version_section(original, "2026.11.2-hotfix.1")
        self.assertIn("## [2026.11.2-hotfix.1] - 2026-11-02\n\n- 变化", result)
        self.assertTrue(result.endswith("## [2026.10.25] - 2026-10-25\n"))
        with self.assertRaises(ValueError):
            version_section(original + "## [Unreleased]\n", "2026.11.2")
