using System;
using KitforgeLabs.UIKit.Catalog.Confirm;
using KitforgeLabs.UIKit.Catalog.DailyLogin;
using KitforgeLabs.UIKit.Catalog.GameOver;
using KitforgeLabs.UIKit.Catalog.LevelComplete;
using KitforgeLabs.UIKit.Catalog.NotEnough;
using KitforgeLabs.UIKit.Catalog.Pause;
using KitforgeLabs.UIKit.Catalog.Reward;
using KitforgeLabs.UIKit.Catalog.Screens;
using KitforgeLabs.UIKit.Catalog.Settings;
using KitforgeLabs.UIKit.Catalog.Shop;
using KitforgeLabs.UIKit.Catalog.Toasts;
using KitforgeLabs.UIKit.Catalog.Tutorial;
using KitforgeLabs.UIKit.Core;
using KitforgeLabs.UIKit.Toast;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace KitforgeLabs.UIKit.Catalog.Tests.Smoke
{
    public class KitforgeRootSmokeTests
    {
        private const string KitforgeRootPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab";

        private GameObject _rootInstance;
        private Scene _scene;

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(KitforgeRootPath);
            Assert.IsNotNull(prefab, $"KitforgeRoot.prefab not found at {KitforgeRootPath}");
            _rootInstance = UnityEngine.Object.Instantiate(prefab);
            _rootInstance.name = "KitforgeRoot";
            SceneManager.MoveGameObjectToScene(_rootInstance, _scene);
        }

        [TearDown]
        public void TearDown()
        {
            if (_rootInstance != null) UnityEngine.Object.DestroyImmediate(_rootInstance);
            if (_scene.IsValid() && SceneManager.sceneCount > 1) EditorSceneManager.CloseScene(_scene, true);
            LogAssert.ignoreFailingMessages = false;
        }

        [TestCase(typeof(ConfirmPopup))]
        [TestCase(typeof(PausePopup))]
        [TestCase(typeof(TutorialPopup))]
        [TestCase(typeof(RewardPopup))]
        [TestCase(typeof(ShopPopup))]
        [TestCase(typeof(NotEnoughCurrencyPopup))]
        [TestCase(typeof(DailyLoginPopup))]
        [TestCase(typeof(LevelCompletePopup))]
        [TestCase(typeof(GameOverPopup))]
        [TestCase(typeof(SettingsPopup))]
        public void PopupOpens_WithNullDefaultServices(Type popupType)
        {
            var manager = UnityEngine.Object.FindAnyObjectByType<PopupManager>(FindObjectsInactive.Include);
            Assert.IsNotNull(manager, "PopupManager not found in KitforgeRoot");
            Assert.IsTrue(manager.IsRegistered(popupType), $"{popupType.Name} not registered in PopupManager._popupPrefabs");
            var method = typeof(PopupManager).GetMethod("Show").MakeGenericMethod(popupType);
            var result = method.Invoke(manager, new object[] { null, PopupPriority.Gameplay });
            Assert.IsNotNull(result, $"PopupManager.Show<{popupType.Name}>() returned null. Check Console for [PopupManager] warnings.");
        }

        [TestCase(typeof(LoadingScreen))]
        [TestCase(typeof(MainMenuScreen))]
        public void ScreenOpens_WithNullDefaultServices(Type screenType)
        {
            var manager = UnityEngine.Object.FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);
            Assert.IsNotNull(manager, "UIManager not found in KitforgeRoot");
            Assert.IsTrue(manager.IsRegistered(screenType), $"{screenType.Name} not registered in UIManager._screenPrefabs");
            var method = typeof(UIManager).GetMethod("Push").MakeGenericMethod(screenType);
            var result = method.Invoke(manager, new object[] { null });
            Assert.IsNotNull(result, $"UIManager.Push<{screenType.Name}>() returned null. Check Console for [UIManager] warnings.");
        }

        [TestCase(typeof(NotificationToast))]
        public void ToastOpens_WithNullDefaultServices(Type toastType)
        {
            var manager = UnityEngine.Object.FindAnyObjectByType<ToastManager>(FindObjectsInactive.Include);
            Assert.IsNotNull(manager, "ToastManager not found in KitforgeRoot");
            Assert.IsTrue(manager.IsRegistered(toastType), $"{toastType.Name} not registered in ToastManager._toastPrefabs");
            var method = typeof(ToastManager).GetMethod("Show").MakeGenericMethod(toastType);
            var result = method.Invoke(manager, new object[] { null, null });
            Assert.IsNotNull(result, $"ToastManager.Show<{toastType.Name}>() returned null. Check Console for [ToastManager] warnings.");
        }
    }
}
