"""包源索引、分页与复查；测试只使用内存中的响应。"""
import sys
import unittest
from pathlib import Path
from unittest import mock

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import release_feed as feed


class ReleaseFeedTests(unittest.TestCase):
    version = "2026.12.1"
    name = "Kun.Data"
    record = {"repository": {"full_name": "CavaiLAB/kun"}, "visibility": "public"}

    def test_github_requires_public_ownership_and_follows_version_pages(self):
        read = mock.Mock(side_effect=[self.record, [{"name": "previous"}] * 100, [{"name": self.version}]])
        self.assertTrue(feed._available("github", self.name, self.version, read))
        page = feed.GITHUB_API + self.name + "/versions?per_page=100&page="
        self.assertEqual([mock.call(feed.GITHUB_API + self.name), mock.call(page + "1"), mock.call(page + "2")],
                         read.call_args_list)
        for record in [{**self.record, "visibility": "private"}, {**self.record, "repository": None},
                       {**self.record, "repository": {"full_name": "CavaiLAB/sample"}}]:
            with self.subTest(record=record), self.assertRaises(feed.FeedError):
                feed._available("github", self.name, self.version, mock.Mock(return_value=record))

    def test_github_absent_package_and_nuget_missing_version(self):
        self.assertFalse(feed._available("github", self.name, self.version, mock.Mock(return_value=None)))
        with mock.patch.object(feed, "_read", return_value={"versions": ["2026.11.30"]}) as read:
            with self.assertRaisesRegex(feed.FeedError, "Kun.Data"):
                feed.verify_feed("nuget", [self.name], self.version, 0)
            read.assert_called_once_with(feed.NUGET_INDEX + "kun.data/index.json", None, 30)
        with mock.patch.object(feed, "_read", return_value={"versions": [self.version]}):
            feed.verify_feed("nuget", [self.name], self.version, 0)

    def test_only_packages_still_missing_are_rechecked(self):
        replies = [True, ConnectionError("暂时不可用"), True]
        with mock.patch.object(feed, "_available", side_effect=replies) as check, mock.patch.object(feed.time, "sleep"):
            feed.verify_feed("github", ["Kun.A", "Kun.B"], self.version, 5, "unused")
        self.assertEqual(["Kun.A", "Kun.B", "Kun.B"], [call.args[1] for call in check.call_args_list])

    def test_empty_selection_and_malformed_index_fail(self):
        with self.assertRaisesRegex(feed.FeedError, "没有待核对"):
            feed.verify_feed("nuget", [], self.version, 0)
        with self.assertRaisesRegex(feed.FeedError, "形状"):
            feed._available("nuget", self.name, self.version, mock.Mock(return_value={"versions": "unexpected"}))


if __name__ == "__main__":
    unittest.main()
