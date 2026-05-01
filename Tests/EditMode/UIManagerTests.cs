using System.Reflection;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Theme;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KitforgeLabs.MobileUIKit.Tests.EditMode
{
    public sealed class UIManagerTests
    {
        private GameObject _host;
        private GameObject _screenRoot;
        private UIManager _manager;
        private UIThemeConfig _theme;
        private FakeScreen _prefabA;
        private FakeScreen _prefabB;
        private FakeScreen _prefabC;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject(nameof(UIManagerTests));
            _host.SetActive(false);
            _screenRoot = new GameObject("ScreenRoot");
            _screenRoot.transform.SetParent(_host.transform);
            _manager = _host.AddComponent<UIManager>();

            _theme = ScriptableObject.CreateInstance<UIThemeConfig>();
            _prefabA = CreatePrefab<FakeScreenA>("PrefabA");
            _prefabB = CreatePrefab<FakeScreenB>("PrefabB");
            _prefabC = CreatePrefab<FakeScreenC>("PrefabC");

            SetField(_manager, "_themeConfig", _theme);
            SetField(_manager, "_screenRoot", _screenRoot.transform);
            SetField(_manager, "_screenPrefabs", new UIModuleBase[] { _prefabA, _prefabB, _prefabC });

            _host.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            if (_theme != null) Object.DestroyImmediate(_theme);
        }

        [Test]
        public void Push_FirstScreen_BecomesCurrentAndIsActive()
        {
            var screen = _manager.Push<FakeScreenA>(data: "payload-a");

            Assert.IsNotNull(screen);
            Assert.AreSame(screen, _manager.Current);
            Assert.IsTrue(screen.gameObject.activeSelf);
            Assert.AreEqual(1, screen.ShowCount);
            Assert.AreEqual("payload-a", screen.LastData);
        }

        [Test]
        public void Push_Second_DeactivatesPreviousAndStacks()
        {
            var first = _manager.Push<FakeScreenA>();
            var second = _manager.Push<FakeScreenB>();

            Assert.AreSame(second, _manager.Current);
            Assert.IsTrue(second.gameObject.activeSelf);
            Assert.IsFalse(first.gameObject.activeSelf);
            Assert.AreEqual(0, first.HideCount, "Push must deactivate previous WITHOUT calling OnHide.");
        }

        [Test]
        public void Pop_RestoresPreviousAndCallsOnHide()
        {
            var first = _manager.Push<FakeScreenA>();
            var second = _manager.Push<FakeScreenB>();

            _manager.Pop();

            Assert.AreSame(first, _manager.Current);
            Assert.IsTrue(first.gameObject.activeSelf);
            Assert.IsFalse(second.gameObject.activeSelf);
            Assert.AreEqual(1, second.HideCount);
        }

        [Test]
        public void Pop_EmptyStack_NoOp()
        {
            Assert.DoesNotThrow(() => _manager.Pop());
            Assert.IsNull(_manager.Current);
        }

        [Test]
        public void Pop_LastScreen_ClearsCurrent()
        {
            _manager.Push<FakeScreenA>();
            _manager.Pop();

            Assert.IsNull(_manager.Current);
        }

        [Test]
        public void Replace_OnEmptyStack_PushesIncoming()
        {
            var replaced = _manager.Replace<FakeScreenA>(data: "p");

            Assert.IsNotNull(replaced);
            Assert.AreSame(replaced, _manager.Current);
            Assert.AreEqual("p", replaced.LastData);
        }

        [Test]
        public void Replace_DropsTopAndPushesIncoming_OnlyOneActive()
        {
            var first = _manager.Push<FakeScreenA>();
            var replacement = _manager.Replace<FakeScreenB>();

            Assert.AreSame(replacement, _manager.Current);
            Assert.AreEqual(1, first.HideCount);
            Assert.IsFalse(first.gameObject.activeSelf);

            _manager.Pop();
            Assert.IsNull(_manager.Current);
        }

        [Test]
        public void Replace_MissingPrefab_DoesNotPopExistingTop()
        {
            SetField(_manager, "_screenPrefabs", new UIModuleBase[] { _prefabA });
            var first = _manager.Push<FakeScreenA>();

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[UIManager\] No prefab registered for FakeScreenB"));
            var result = _manager.Replace<FakeScreenB>();

            Assert.IsNull(result);
            Assert.AreSame(first, _manager.Current, "Replace must not corrupt the stack when the incoming prefab is missing.");
            Assert.IsTrue(first.gameObject.activeSelf);
            Assert.AreEqual(0, first.HideCount);
        }

        [Test]
        public void PopToRoot_LeavesOneScreen()
        {
            _manager.Push<FakeScreenA>();
            _manager.Push<FakeScreenB>();
            _manager.Push<FakeScreenC>();

            _manager.PopToRoot();

            Assert.IsNotNull(_manager.Current);
            Assert.IsInstanceOf<FakeScreenA>(_manager.Current);
        }

        [Test]
        public void PopToRoot_OnSingleScreen_NoOp()
        {
            var only = _manager.Push<FakeScreenA>();

            _manager.PopToRoot();

            Assert.AreSame(only, _manager.Current);
            Assert.AreEqual(0, only.HideCount);
        }

        [Test]
        public void Push_TypeWithoutRegisteredPrefab_LogsErrorAndReturnsNull()
        {
            SetField(_manager, "_screenPrefabs", new UIModuleBase[] { _prefabA });

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"\[UIManager\] No prefab registered for FakeScreenB"));
            var result = _manager.Push<FakeScreenB>();

            Assert.IsNull(result);
            Assert.IsNull(_manager.Current);
        }

        [Test]
        public void Push_SameTypeTwice_ReusesCachedInstance()
        {
            var first = _manager.Push<FakeScreenA>();
            _manager.Pop();
            var second = _manager.Push<FakeScreenA>();

            Assert.AreSame(first, second, "ResolveScreen must reuse the cached instance per type.");
            Assert.AreEqual(2, first.ShowCount);
        }

        private FakeScreen CreatePrefab<T>(string name) where T : FakeScreen
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

        private abstract class FakeScreen : UIModuleBase
        {
            public object LastData;
            public int ShowCount;
            public int HideCount;

            public override void OnShow() { ShowCount++; }
            public override void OnHide() { HideCount++; }
            protected override void BindUntyped(object data) { LastData = data; }
        }

        private sealed class FakeScreenA : FakeScreen { }
        private sealed class FakeScreenB : FakeScreen { }
        private sealed class FakeScreenC : FakeScreen { }
    }
}
