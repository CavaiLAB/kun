"""发布前核对日期顺序、发布历史与两个正式包源。"""
import sys
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import release_feed as feed
import release_packages as release


class PreflightTests(unittest.TestCase):
    packages = [SimpleNamespace(id="Kun.Core", tier="kernel"), SimpleNamespace(id="Kun.Data", tier="platform")]

    def test_calendar_order_includes_numbered_hotfixes_and_ignores_beta_tags(self):
        tags = ["v0.2.0", "v2026.9.7", "v2026.9.7-hotfix.10", "v2026.9.7-hotfix.2", "v2026.10.1-beta.1"]
        changes = "## [Unreleased]\n\n## [2026.9.7-hotfix.10] - 2026-09-07\n"
        release.check_release_tags("2026.10.1", tags, changes)
        for candidate, body in [("2026.10.1-beta.1", changes), ("2026.10.1", changes.replace("hotfix.10", "hotfix.2")),
                                ("2026.10.1", "## [Unreleased]\n")]:
            with self.subTest(candidate=candidate, body=body), self.assertRaises(release.Failure):
                release.check_release_tags(candidate, tags, body)

    def test_checks_all_github_ids_and_kernel_nuget_ids(self):
        with patch.object(feed, "_read", side_effect=[None, {"versions": ["0.1.0", "2026.11.1"]}, []]) as get:
            feed.check_unpublished(self.packages, "2026.11.1-hotfix.1", "fixture")
        calls = [(call.args[0], call.args[1]) for call in get.call_args_list]
        self.assertEqual([(feed.GITHUB_API + "Kun.Core/versions?per_page=100&page=1", "fixture"),
                          (feed.NUGET_INDEX + "kun.core/index.json", None),
                          (feed.GITHUB_API + "Kun.Data/versions?per_page=100&page=1", "fixture")], calls)

    def test_rejects_existing_or_older_versions(self):
        for version, replies in [
            ("2026.11.1", [[], {"versions": ["2026.11.1"]}]),
            ("2026.11.1-beta.1", [[], {"versions": ["2026.11.1"]}]),
            ("2026.11.1-hotfix.2", [[], {"versions": ["2026.11.1-hotfix.10"]}]),
            ("2026.11.1", [None, None, [{"name": "old"}] * 100, [{"name": "2026.11.1"}]]),
        ]:
            with self.subTest(version=version), patch.object(feed, "_read", side_effect=replies), self.assertRaises(feed.FeedError):
                feed.check_unpublished(self.packages, version, "fixture")

    def test_resume_accepts_the_same_version_but_refuses_a_newer_release(self):
        with patch.object(feed, "_read", return_value={"versions": ["2026.11.1"]}):
            feed.check_unpublished(self.packages, "2026.11.1", "fixture", resume=True)
        with patch.object(feed, "_read", return_value={"versions": ["2026.11.2"]}), self.assertRaises(feed.FeedError):
            feed.check_unpublished(self.packages, "2026.11.1", "fixture", resume=True)

    def test_read_failure_or_missing_credentials_cannot_approve_publication(self):
        with self.assertRaises(feed.FeedError):
            feed.check_unpublished(self.packages, "2026.11.1", None)
        with patch.object(feed.time, "sleep"), patch.object(feed, "_read", side_effect=ConnectionError("e")) as read, self.assertRaises(feed.FeedError):
            feed.check_unpublished(self.packages, "2026.11.1", "fixture")
        self.assertEqual(read.call_count, 3)
        with patch.object(feed, "_read", return_value=[{}]), self.assertRaises(feed.FeedError):
            feed.check_unpublished(self.packages, "2026.11.1", "fixture")

    def test_transient_error_retries_only_the_interrupted_index(self):
        replies = [[], ConnectionError("connection interrupted"), {"versions": ["0.1.0"]}, []]
        with patch.object(feed.time, "sleep") as sleep, patch.object(feed, "_read", side_effect=replies) as read:
            feed.check_unpublished(self.packages, "2026.11.1", "fixture")
        urls = [call.args[0] for call in read.call_args_list]
        self.assertEqual(urls[1:3], [feed.NUGET_INDEX + "kun.core/index.json"] * 2)
        self.assertEqual(len(urls), 4)
        sleep.assert_called_once_with(1)


if __name__ == "__main__":
    unittest.main()
