using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Routing;
using NUnit.Framework;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Tests.EditMode
{
    public sealed class UIRouterTests
    {
        private GameObject _host;
        private UIRouter _router;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject(nameof(UIRouterTests));
            _router = _host.AddComponent<UIRouter>();
            _router.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void TransitionTo_DifferentState_ReturnsTrueAndUpdatesCurrent()
        {
            var changed = _router.TransitionTo(AppState.MainMenu);

            Assert.IsTrue(changed);
            Assert.AreEqual(AppState.MainMenu, _router.CurrentState);
        }

        [Test]
        public void TransitionTo_SameState_ReturnsFalse()
        {
            var changed = _router.TransitionTo(AppState.Loading);

            Assert.IsFalse(changed);
            Assert.AreEqual(AppState.Loading, _router.CurrentState);
        }

        [Test]
        public void TransitionTo_RaisesOnStateChangedWithPreviousAndNext()
        {
            AppState capturedPrev = default;
            AppState capturedNext = default;
            var calls = 0;
            _router.OnStateChanged += (prev, next) =>
            {
                if (calls == 0)
                {
                    capturedPrev = prev;
                    capturedNext = next;
                }
                calls++;
            };

            _router.TransitionTo(AppState.MainMenu);

            Assert.AreEqual(1, calls);
            Assert.AreEqual(AppState.Loading, capturedPrev);
            Assert.AreEqual(AppState.MainMenu, capturedNext);
        }

        [Test]
        public void TransitionTo_NestedFromCallback_IsBlockedByReentrancyGuard()
        {
            var nestedResult = true;
            _router.OnStateChanged += (prev, next) =>
            {
                if (next == AppState.MainMenu) nestedResult = _router.TransitionTo(AppState.Gameplay);
            };

            var changed = _router.TransitionTo(AppState.MainMenu);

            Assert.IsTrue(changed);
            Assert.IsFalse(nestedResult);
            Assert.AreEqual(AppState.MainMenu, _router.CurrentState);
        }

        [Test]
        public void IsValidPopup_NullType_ReturnsFalse()
        {
            Assert.IsFalse(_router.IsValidPopup(null));
        }

        [Test]
        public void IsValidPopup_NoRestrictions_ReturnsTrueForAnyType()
        {
            Assert.IsFalse(_router.HasPopupRestrictions);
            Assert.IsTrue(_router.IsValidPopup(typeof(FakePopupA)));
            Assert.IsTrue(_router.IsValidPopup(typeof(FakePopupB)));
        }

        [Test]
        public void RestrictPopupsTo_OnlyAllowedTypesPass()
        {
            _router.RestrictPopupsTo(new List<System.Type> { typeof(FakePopupA) });

            Assert.IsTrue(_router.HasPopupRestrictions);
            Assert.IsTrue(_router.IsValidPopup(typeof(FakePopupA)));
            Assert.IsFalse(_router.IsValidPopup(typeof(FakePopupB)));
        }

        [Test]
        public void RestrictPopupsTo_NullCollection_BlocksAllTypes()
        {
            _router.RestrictPopupsTo(null);

            Assert.IsTrue(_router.HasPopupRestrictions);
            Assert.IsFalse(_router.IsValidPopup(typeof(FakePopupA)));
        }

        [Test]
        public void ClearPopupRestrictions_RestoresOpenAllowList()
        {
            _router.RestrictPopupsTo(new List<System.Type> { typeof(FakePopupA) });
            _router.ClearPopupRestrictions();

            Assert.IsFalse(_router.HasPopupRestrictions);
            Assert.IsTrue(_router.IsValidPopup(typeof(FakePopupB)));
        }

        [Test]
        public void Initialize_CalledTwice_IsIdempotent()
        {
            var calls = 0;
            _router.OnStateChanged += (_, _) => calls++;

            _router.Initialize();
            _router.Initialize();

            Assert.AreEqual(0, calls);
            Assert.IsTrue(_router.IsInitialized);
        }

        private sealed class FakePopupA : UIModuleBase
        {
            public override void OnShow() { }
            public override void OnHide() { }
            protected override void BindUntyped(object data) { }
        }

        private sealed class FakePopupB : UIModuleBase
        {
            public override void OnShow() { }
            public override void OnHide() { }
            protected override void BindUntyped(object data) { }
        }
    }
}
