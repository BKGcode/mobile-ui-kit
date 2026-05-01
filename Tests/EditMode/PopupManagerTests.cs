using System.Reflection;
using KitforgeLabs.MobileUIKit.Core;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Tests.EditMode
{
    public sealed class PopupManagerTests
    {
        private GameObject _host;
        private GameObject _popupRoot;
        private PopupManager _manager;
        private FakePopup _prefabA;
        private FakePopup _prefabB;
        private FakePopup _prefabC;
        private FakePopup _prefabD;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject(nameof(PopupManagerTests));
            _popupRoot = new GameObject("PopupRoot");
            _popupRoot.transform.SetParent(_host.transform);
            _manager = _host.AddComponent<PopupManager>();

            _prefabA = CreatePrefab<FakePopupA>("PrefabA");
            _prefabB = CreatePrefab<FakePopupB>("PrefabB");
            _prefabC = CreatePrefab<FakePopupC>("PrefabC");
            _prefabD = CreatePrefab<FakePopupD>("PrefabD");

            SetField(_manager, "_popupRoot", _popupRoot.transform);
            SetField(_manager, "_popupPrefabs", new UIModuleBase[] { _prefabA, _prefabB, _prefabC, _prefabD });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void Show_FirstPopup_AddsToActiveStackAndReturnsInstance()
        {
            var popup = _manager.Show<FakePopupA>(data: "payload-a", priority: PopupPriority.Gameplay);

            Assert.IsNotNull(popup);
            Assert.AreEqual(1, _manager.ActiveCount);
            Assert.AreSame(popup, _manager.HighestPriorityPopup);
            Assert.AreEqual("payload-a", popup.LastData);
            Assert.IsTrue(popup.IsShownActive);
        }

        [Test]
        public void Show_HigherPriorityArrivesAfter_BecomesHighest()
        {
            _manager.Show<FakePopupA>(priority: PopupPriority.Meta);
            var modal = _manager.Show<FakePopupB>(priority: PopupPriority.Modal);

            Assert.AreEqual(2, _manager.ActiveCount);
            Assert.AreSame(modal, _manager.HighestPriorityPopup);
        }

        [Test]
        public void Show_BeyondMaxDepth_SamePriority_QueuesAndReturnsNull()
        {
            var first = _manager.Show<FakePopupA>(priority: PopupPriority.Gameplay);
            var second = _manager.Show<FakePopupB>(priority: PopupPriority.Gameplay);
            var third = _manager.Show<FakePopupC>(priority: PopupPriority.Gameplay);
            var fourth = _manager.Show<FakePopupD>(priority: PopupPriority.Gameplay);

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.IsNotNull(third);
            Assert.IsNull(fourth);
            Assert.AreEqual(3, _manager.ActiveCount);
        }

        [Test]
        public void Dismiss_DrainsPendingQueue()
        {
            _manager.Show<FakePopupA>(priority: PopupPriority.Gameplay);
            _manager.Show<FakePopupB>(priority: PopupPriority.Gameplay);
            var third = _manager.Show<FakePopupC>(priority: PopupPriority.Gameplay);
            var fourthInitial = _manager.Show<FakePopupD>(data: "queued-d", priority: PopupPriority.Gameplay);
            Assert.IsNull(fourthInitial);

            _manager.Dismiss(third);

            Assert.AreEqual(3, _manager.ActiveCount);
            Assert.IsTrue(_manager.IsShowing<FakePopupD>());
            Assert.IsFalse(_manager.IsShowing<FakePopupC>());
            var drained = (FakePopup)_manager.HighestPriorityPopup;
            Assert.IsInstanceOf<FakePopupD>(drained);
            Assert.AreEqual("queued-d", drained.LastData);
        }

        [Test]
        public void Show_HigherPriorityWhenFull_EvictsLowest()
        {
            _manager.Show<FakePopupA>(priority: PopupPriority.Meta);
            _manager.Show<FakePopupB>(priority: PopupPriority.Gameplay);
            _manager.Show<FakePopupC>(priority: PopupPriority.Gameplay);

            var modal = _manager.Show<FakePopupD>(priority: PopupPriority.Modal);

            Assert.IsNotNull(modal);
            Assert.AreEqual(3, _manager.ActiveCount);
            Assert.IsFalse(_manager.IsShowing<FakePopupA>(), "Lowest-priority popup (Meta) must be evicted.");
            Assert.IsTrue(_manager.IsShowing<FakePopupD>());
            Assert.AreSame(modal, _manager.HighestPriorityPopup);
        }

        [Test]
        public void Eviction_PreservesDataAndRestoresOnDismiss()
        {
            var aInstance = _manager.Show<FakePopupA>(data: "preserve-me", priority: PopupPriority.Meta);
            _manager.Show<FakePopupB>(priority: PopupPriority.Gameplay);
            _manager.Show<FakePopupC>(priority: PopupPriority.Gameplay);
            var modal = _manager.Show<FakePopupD>(priority: PopupPriority.Modal);
            Assert.IsFalse(_manager.IsShowing<FakePopupA>());

            aInstance.LastData = null;
            _manager.Dismiss(modal);

            Assert.IsTrue(_manager.IsShowing<FakePopupA>());
            Assert.AreEqual("preserve-me", aInstance.LastData);
        }

        [Test]
        public void DismissAll_ClearsActiveAndPending()
        {
            _manager.Show<FakePopupA>(priority: PopupPriority.Gameplay);
            _manager.Show<FakePopupB>(priority: PopupPriority.Gameplay);
            _manager.Show<FakePopupC>(priority: PopupPriority.Gameplay);
            _manager.Show<FakePopupD>(priority: PopupPriority.Gameplay);

            _manager.DismissAll();

            Assert.AreEqual(0, _manager.ActiveCount);
            Assert.IsNull(_manager.HighestPriorityPopup);

            var afterDismiss = _manager.Show<FakePopupA>(priority: PopupPriority.Gameplay);
            Assert.IsNotNull(afterDismiss, "DismissAll must also clear the pending queue (no leftover D).");
        }

        [Test]
        public void DispatchBackPressed_DelegatesToHighestPriorityPopup()
        {
            _manager.Show<FakePopupA>(priority: PopupPriority.Meta);
            var top = _manager.Show<FakePopupB>(priority: PopupPriority.Modal);

            var handled = _manager.DispatchBackPressed();

            Assert.IsTrue(handled);
            Assert.AreEqual(1, top.BackPressedCount);
            Assert.AreEqual(0, _prefabA.BackPressedCount);
        }

        [Test]
        public void DispatchBackPressed_NoActivePopups_ReturnsFalse()
        {
            Assert.IsFalse(_manager.DispatchBackPressed());
        }

        private FakePopup CreatePrefab<T>(string name) where T : FakePopup
        {
            var go = new GameObject(name);
            go.transform.SetParent(_host.transform);
            go.SetActive(false);
            return go.AddComponent<T>();
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field {fieldName} not found on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private abstract class FakePopup : UIModuleBase
        {
            public object LastData;
            public int ShowCount;
            public int HideCount;
            public int BackPressedCount;
            public bool IsShownActive => gameObject.activeSelf;

            public override void OnShow() { ShowCount++; }
            public override void OnHide() { HideCount++; }
            public override void OnBackPressed() { BackPressedCount++; }
            protected internal override void BindUntyped(object data) { LastData = data; }
        }

        private sealed class FakePopupA : FakePopup { }
        private sealed class FakePopupB : FakePopup { }
        private sealed class FakePopupC : FakePopup { }
        private sealed class FakePopupD : FakePopup { }
    }
}
