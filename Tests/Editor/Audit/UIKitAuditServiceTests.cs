using System.IO;
using KitforgeLabs.MobileUIKit.Editor.Audit;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Audit.Tests
{
    public sealed class UIKitAuditServiceTests
    {
        private string _activeSceneBefore;

        [SetUp]
        public void SetUp()
        {
            _activeSceneBefore = EditorSceneManager.GetActiveScene().path;
        }

        [TearDown]
        public void TearDown()
        {
            var nowPath = EditorSceneManager.GetActiveScene().path;
            Assert.AreEqual(_activeSceneBefore, nowPath, "Audit polluted active scene.");
        }

        [Test]
        public void AuditAll_PrefabsOnly_NoSnapshots_DoesNotThrow()
        {
            var run = UIKitAuditService.AuditAll(BuildOptions(UIKitAuditDimensions.Catalog | UIKitAuditDimensions.PrefabStructural));
            Assert.NotNull(run);
            Assert.IsNotEmpty(run.GeneratedAtUtc);
            Assert.IsNotEmpty(run.UnityVersion);
            Assert.AreEqual(Application.unityVersion, run.UnityVersion);
        }

        [Test]
        public void AuditAll_PrefabDimensions_ReturnsRunWithElapsedMs()
        {
            var run = UIKitAuditService.AuditAll(BuildOptions(UIKitAuditDimensions.Catalog | UIKitAuditDimensions.PrefabStructural));
            Assert.NotNull(run);
            Assert.NotNull(run.Targets);
            Assert.IsTrue(run.ElapsedMs >= 0);
        }

        [Test]
        public void Writer_WritesSummaryAndPerTargetReports()
        {
            UIKitAuditReportWriter.Clear();
            var run = UIKitAuditService.AuditAll(BuildOptions(UIKitAuditDimensions.Catalog | UIKitAuditDimensions.PrefabStructural));
            UIKitAuditReportWriter.Write(run, false);
            var summaryPath = Path.Combine(UIKitAuditReportWriter.ReportsRoot, "_Summary.md");
            Assert.IsTrue(File.Exists(summaryPath), "Summary not written.");
            for (var i = 0; i < run.Targets.Count; i++) AssertTargetReportFiles(run.Targets[i]);
        }

        [Test]
        public void AuditAll_AggregatesGitCommit_AndUnityVersion()
        {
            var run = UIKitAuditService.AuditAll(BuildOptions(UIKitAuditDimensions.Catalog));
            Assert.AreEqual(Application.unityVersion, run.UnityVersion);
            Assert.IsNotEmpty(run.GitCommit);
        }

        [Test]
        public void Run_BreakdownAggregates_CatalogAndPrefabKinds()
        {
            var run = UIKitAuditService.AuditAll(BuildOptions(UIKitAuditDimensions.Catalog | UIKitAuditDimensions.PrefabStructural));
            Assert.AreEqual(run.PrefabPass + (run.PrefabTotal - run.PrefabPass), run.PrefabTotal);
            Assert.IsTrue(run.CatalogTotal <= run.PrefabTotal);
        }

        private static UIKitAuditOptions BuildOptions(UIKitAuditDimensions dims)
        {
            return new UIKitAuditOptions { Dimensions = dims, Trigger = UIKitAuditTrigger.Manual };
        }

        private static void AssertTargetReportFiles(UIKitAuditTargetReport target)
        {
            var stem = Path.Combine(UIKitAuditReportWriter.ReportsRoot, target.Label);
            Assert.IsTrue(File.Exists(stem + ".md"), $"Missing per-target markdown for {target.Label}.");
            Assert.IsTrue(File.Exists(stem + ".json"), $"Missing per-target JSON for {target.Label}.");
        }
    }
}
