using KitforgeLabs.MobileUIKit.Editor.Audit;
using NUnit.Framework;
using UnityEditor.SceneManagement;

namespace KitforgeLabs.MobileUIKit.Audit.Tests
{
    public sealed class UIKitAuditDiscoveryTests
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
            Assert.AreEqual(_activeSceneBefore, nowPath, "Discovery polluted active scene.");
        }

        [Test]
        public void DiscoverAll_ReturnsList_NeverNull()
        {
            var all = UIKitAuditDiscovery.DiscoverAll();
            Assert.NotNull(all);
        }

        [Test]
        public void Filter_NoneDimension_ReturnsEmpty()
        {
            var all = UIKitAuditDiscovery.DiscoverAll();
            var filtered = UIKitAuditDiscovery.Filter(all, UIKitAuditDimensions.None);
            Assert.IsEmpty(filtered);
        }

        [Test]
        public void Filter_AllDimension_KeepsAll()
        {
            var all = UIKitAuditDiscovery.DiscoverAll();
            var filtered = UIKitAuditDiscovery.Filter(all, UIKitAuditDimensions.All);
            Assert.AreEqual(all.Count, filtered.Count);
        }

        [Test]
        public void Filter_ScenesOnly_RemovesPrefabs()
        {
            var all = UIKitAuditDiscovery.DiscoverAll();
            var filtered = UIKitAuditDiscovery.Filter(all, UIKitAuditDimensions.Scenes);
            for (var i = 0; i < filtered.Count; i++) Assert.AreEqual(UIKitAuditTargetKind.Scene, filtered[i].Kind);
        }

        [Test]
        public void Filter_CatalogOnly_RemovesNonCatalogPrefabsAndScenes()
        {
            var all = UIKitAuditDiscovery.DiscoverAll();
            var filtered = UIKitAuditDiscovery.Filter(all, UIKitAuditDimensions.Catalog);
            for (var i = 0; i < filtered.Count; i++)
            {
                Assert.AreEqual(UIKitAuditTargetKind.Prefab, filtered[i].Kind);
                Assert.IsTrue(filtered[i].IsCatalog);
            }
        }
    }
}
