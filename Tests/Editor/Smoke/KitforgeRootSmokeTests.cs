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
using UnityEngine;

namespace KitforgeLabs.UIKit.Catalog.Tests.Smoke
{
    public class KitforgeRootSmokeTests
    {
        private const string KitforgeRootPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab";

        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(KitforgeRootPath);
            Assert.IsNotNull(_prefab, $"KitforgeRoot.prefab not found at {KitforgeRootPath}");
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
        public void PopupPrefab_IsWiredInPopupManager(Type popupType)
        {
            var manager = _prefab.GetComponentInChildren<PopupManager>(true);
            Assert.IsNotNull(manager, "PopupManager not found in KitforgeRoot");
            Assert.IsTrue(manager.IsRegistered(popupType), $"{popupType.Name} is not registered in PopupManager._popupPrefabs[] — run Tools/KitforgeLabs/Test/Wire Catalog Into KitforgeRoot");
        }

        [TestCase(typeof(LoadingScreen))]
        [TestCase(typeof(MainMenuScreen))]
        public void ScreenPrefab_IsWiredInUIManager(Type screenType)
        {
            var manager = _prefab.GetComponentInChildren<UIManager>(true);
            Assert.IsNotNull(manager, "UIManager not found in KitforgeRoot");
            Assert.IsTrue(manager.IsRegistered(screenType), $"{screenType.Name} is not registered in UIManager._screenPrefabs[] — run Tools/KitforgeLabs/Test/Wire Catalog Into KitforgeRoot");
        }

        [TestCase(typeof(NotificationToast))]
        public void ToastPrefab_IsWiredInToastManager(Type toastType)
        {
            var manager = _prefab.GetComponentInChildren<ToastManager>(true);
            Assert.IsNotNull(manager, "ToastManager not found in KitforgeRoot");
            Assert.IsTrue(manager.IsRegistered(toastType), $"{toastType.Name} is not registered in ToastManager._toastPrefabs[] — run Tools/KitforgeLabs/Test/Wire Catalog Into KitforgeRoot");
        }
    }
}
