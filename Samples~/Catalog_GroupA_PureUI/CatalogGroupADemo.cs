using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.Confirm;
using KitforgeLabs.MobileUIKit.Catalog.Pause;
using KitforgeLabs.MobileUIKit.Catalog.Toasts;
using KitforgeLabs.MobileUIKit.Catalog.Tutorial;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupA
{
    public sealed class CatalogGroupADemo : MonoBehaviour
    {
        [SerializeField] private RectTransform _popupParent;
        [SerializeField] private RectTransform _toastParent;
        [SerializeField] private UIThemeConfig _theme;
        [SerializeField] private UIServices _services;

        [Header("Catalog prefabs (assigned by Build Group A Sample). Drop a prefab whose root has the corresponding catalog component.")]
        [SerializeField] private GameObject _confirmPrefab;
        [SerializeField] private GameObject _pausePrefab;
        [SerializeField] private GameObject _tutorialPrefab;
        [SerializeField] private GameObject _toastPrefab;

        [ContextMenu("Confirm — Neutral")]
        private void ShowConfirmNeutral() => SpawnConfirm(new ConfirmPopupData
        {
            Title = "Continue?",
            Message = "Pick up where you left off.",
            ConfirmLabel = "Continue",
            CancelLabel = "Cancel",
            Tone = ConfirmTone.Neutral
        });

        [ContextMenu("Confirm — Destructive")]
        private void ShowConfirmDestructive() => SpawnConfirm(new ConfirmPopupData
        {
            Title = "Delete save?",
            Message = "Your progress will be lost. This cannot be undone.",
            ConfirmLabel = "Delete",
            CancelLabel = "Cancel",
            Tone = ConfirmTone.Destructive
        });

        [ContextMenu("Confirm — Positive")]
        private void ShowConfirmPositive() => SpawnConfirm(new ConfirmPopupData
        {
            Title = "Claim reward?",
            Message = "Tap Claim to add 100 coins.",
            ConfirmLabel = "Claim",
            CancelLabel = "Later",
            Tone = ConfirmTone.Positive
        });

        [ContextMenu("Confirm — SingleButton")]
        private void ShowConfirmAlert() => SpawnConfirm(new ConfirmPopupData
        {
            Title = "Connection lost",
            Message = "Tap OK to retry.",
            ConfirmLabel = "OK",
            ShowCancel = false
        });

        [ContextMenu("Pause — Default")]
        private void ShowPauseDefault() => SpawnPause(new PausePopupData
        {
            Title = "Paused"
        });

        [ContextMenu("Pause — All Buttons")]
        private void ShowPauseAll() => SpawnPause(new PausePopupData
        {
            Title = "Paused",
            Subtitle = "World 3 — Level 12",
            ShowResume = true,
            ShowRestart = true,
            ShowSettings = true,
            ShowHome = true,
            ShowShop = true,
            ShowHelp = true,
            ShowQuit = true,
            ShowSoundToggle = true,
            ShowMusicToggle = true,
            ShowVibrationToggle = true
        });

        [ContextMenu("Tutorial — 3 Steps")]
        private void ShowTutorial3() => SpawnTutorial(BuildTutorialData(loop: false));

        [ContextMenu("Tutorial — Loop")]
        private void ShowTutorialLoop() => SpawnTutorial(BuildTutorialData(loop: true));

        [ContextMenu("Toast — Info")]
        private void ShowToastInfo() => SpawnToast(new NotificationToastData
        {
            Message = "Saved.",
            Severity = ToastSeverity.Info
        });

        [ContextMenu("Toast — Success")]
        private void ShowToastSuccess() => SpawnToast(new NotificationToastData
        {
            Message = "Reward claimed!",
            Severity = ToastSeverity.Success
        });

        [ContextMenu("Toast — Warning")]
        private void ShowToastWarning() => SpawnToast(new NotificationToastData
        {
            Message = "Connection unstable.",
            Severity = ToastSeverity.Warning
        });

        [ContextMenu("Toast — Error")]
        private void ShowToastError() => SpawnToast(new NotificationToastData
        {
            Message = "Purchase failed.",
            Severity = ToastSeverity.Error,
            DurationOverride = 6f
        });

        private void SpawnConfirm(ConfirmPopupData data)
        {
            var instance = SpawnPopup<ConfirmPopup>(_confirmPrefab, "ConfirmPopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private void SpawnPause(PausePopupData data)
        {
            var instance = SpawnPopup<PausePopup>(_pausePrefab, "PausePopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private void SpawnTutorial(TutorialPopupData data)
        {
            var instance = SpawnPopup<TutorialPopup>(_tutorialPrefab, "TutorialPopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private void SpawnToast(NotificationToastData data)
        {
            if (_toastPrefab == null) { LogPrefabMissing("NotificationToast"); return; }
            var go = Instantiate(_toastPrefab, _toastParent, false);
            var instance = go.GetComponent<NotificationToast>();
            if (instance == null) { LogComponentMissing("NotificationToast", go); return; }
            instance.Initialize(_theme, _services);
            instance.Bind(data);
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
            StartCoroutine(AutoDismissToast(instance));
        }

        private T SpawnPopup<T>(GameObject prefab, string name) where T : UIModuleBase
        {
            if (prefab == null) { LogPrefabMissing(name); return null; }
            var go = Instantiate(prefab, _popupParent, false);
            var instance = go.GetComponent<T>();
            if (instance == null) { LogComponentMissing(name, go); return null; }
            instance.Initialize(_theme, _services);
            return instance;
        }

        private void LogPrefabMissing(string name)
        {
            Debug.LogError($"[CatalogGroupADemo] {name} prefab not assigned. Run 'Tools/Kitforge/UI Kit/Build Group A Sample' to generate it.", this);
        }

        private void LogComponentMissing(string name, GameObject spawned)
        {
            Debug.LogError($"[CatalogGroupADemo] Spawned prefab is missing the {name} component on its root. Destroying the orphan instance.", this);
            Destroy(spawned);
        }

        private System.Collections.IEnumerator AutoDismissToast(NotificationToast toast)
        {
            yield return new WaitForSecondsRealtime(toast.DefaultDuration);
            if (toast != null && !toast.IsDismissing) toast.DismissNow();
        }

        private static TutorialPopupData BuildTutorialData(bool loop)
        {
            return new TutorialPopupData
            {
                Steps = new List<TutorialStep>
                {
                    new TutorialStep { Title = "Welcome", Body = "Tap Next to learn the basics." },
                    new TutorialStep { Title = "Move", Body = "Drag anywhere on the screen to move your character." },
                    new TutorialStep { Title = "Collect", Body = "Reach the goal to complete the level." }
                },
                ShowPrevious = true,
                ShowSkip = true,
                LoopBackToFirst = loop
            };
        }
    }
}
