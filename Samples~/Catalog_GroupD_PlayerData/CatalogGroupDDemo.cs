using KitforgeLabs.MobileUIKit.Catalog.Settings;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupD
{
    public sealed class CatalogGroupDDemo : MonoBehaviour
    {
        [SerializeField] private RectTransform _popupParent;
        [SerializeField] private UIThemeConfig _theme;
        [SerializeField] private UIServices _services;

        [Header("Catalog prefabs (assigned by Build Group D Sample).")]
        [SerializeField] private GameObject _settingsPrefab;

        [Header("Language picker options. If null/empty, picker auto-hides.")]
        [SerializeField] private LanguageOption[] _languageOptions;

        private void Awake()
        {
            if (_languageOptions == null || _languageOptions.Length == 0)
            {
                _languageOptions = new[]
                {
                    new LanguageOption { Code = "en", DisplayName = "English" },
                    new LanguageOption { Code = "es", DisplayName = "Español" },
                    new LanguageOption { Code = "fr", DisplayName = "Français" }
                };
            }
            if (_services != null && _services.Localization != null)
                _services.Localization.OnLanguageChanged += HandleLanguageChanged;
        }

        private void OnDestroy()
        {
            if (_services != null && _services.Localization != null)
                _services.Localization.OnLanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLanguageChanged(string code)
        {
            Debug.Log($"[CatalogGroupDDemo] Re-skin trigger: language changed to '{code}'. Buyer's localization layer would refresh content here.");
        }

        [ContextMenu("Settings — Show all (default DTO)")]
        private void ShowSettingsAll() => SpawnSettings(new SettingsPopupData
        {
            LanguageOptions = _languageOptions
        });

        [ContextMenu("Settings — Audio sliders only")]
        private void ShowSettingsAudioOnly() => SpawnSettings(new SettingsPopupData
        {
            ShowLanguagePicker = false,
            ShowNotificationsToggle = false,
            ShowHapticsToggle = false,
            LanguageOptions = _languageOptions
        });

        [ContextMenu("Settings — No language picker (hyper-casual)")]
        private void ShowSettingsNoLanguage() => SpawnSettings(new SettingsPopupData
        {
            ShowLanguagePicker = false,
            LanguageOptions = null
        });

        [ContextMenu("PlayerData — Print all kfmui.settings.* values")]
        private void PrintSettingsValues()
        {
            var pd = _services != null ? _services.PlayerData : null;
            if (pd == null) { Debug.LogError("[CatalogGroupDDemo] No PlayerData service wired on UIServices."); return; }
            Debug.Log($"[CatalogGroupDDemo] musicVolume={pd.GetFloat(SettingsPopup.KeyMusicVolume, 1f):F2} sfxVolume={pd.GetFloat(SettingsPopup.KeySfxVolume, 1f):F2} language='{pd.GetString(SettingsPopup.KeyLanguage, "")}' notifications={pd.GetBool(SettingsPopup.KeyNotifications, true)} haptics={pd.GetBool(SettingsPopup.KeyHaptics, true)}");
        }

        [ContextMenu("PlayerData — Print DailyLogin keys")]
        private void PrintDailyLoginValues()
        {
            var pd = _services != null ? _services.PlayerData : null;
            if (pd == null) { Debug.LogError("[CatalogGroupDDemo] No PlayerData service wired on UIServices."); return; }
            Debug.Log($"[CatalogGroupDDemo] DailyLogin currentDay={pd.GetInt(DailyLoginPersistence.KeyCurrentDay, 1)} lastClaimUtc='{pd.GetString(DailyLoginPersistence.KeyLastClaimUtc, "")}' doubledToday={pd.GetBool(DailyLoginPersistence.KeyDoubledToday, false)}");
        }

        [ContextMenu("PlayerData — Reset all 8 kit-side keys (restore defaults pattern)")]
        private void ResetAllKitKeys()
        {
            var pd = _services != null ? _services.PlayerData : null;
            if (pd == null) { Debug.LogError("[CatalogGroupDDemo] No PlayerData service wired on UIServices."); return; }
            pd.Delete(SettingsPopup.KeyMusicVolume);
            pd.Delete(SettingsPopup.KeySfxVolume);
            pd.Delete(SettingsPopup.KeyLanguage);
            pd.Delete(SettingsPopup.KeyNotifications);
            pd.Delete(SettingsPopup.KeyHaptics);
            pd.Delete(DailyLoginPersistence.KeyCurrentDay);
            pd.Delete(DailyLoginPersistence.KeyLastClaimUtc);
            pd.Delete(DailyLoginPersistence.KeyDoubledToday);
            pd.Save();
            pd.Reload();
            Debug.Log("[CatalogGroupDDemo] Reset 8 kit-side keys (5 settings + 3 dailylogin). Reload simulates app restart.");
        }

        [ContextMenu("Localization — SetLanguage 'en'")]
        private void SwitchToEnglish() => SwitchLanguage("en");

        [ContextMenu("Localization — SetLanguage 'es'")]
        private void SwitchToSpanish() => SwitchLanguage("es");

        [ContextMenu("Localization — SetLanguage 'fr'")]
        private void SwitchToFrench() => SwitchLanguage("fr");

        private void SwitchLanguage(string code)
        {
            var loc = _services != null ? _services.Localization : null;
            if (loc == null) { Debug.LogError("[CatalogGroupDDemo] No Localization service wired."); return; }
            loc.SetLanguage(code);
        }

        private void SpawnSettings(SettingsPopupData data)
        {
            var instance = SpawnPopup<SettingsPopup>(_settingsPrefab, "SettingsPopup");
            if (instance == null) return;
            instance.Bind(data);
            instance.OnMusicVolumeChanged += v => Debug.Log($"[Demo] Music volume → {v:F2}");
            instance.OnSfxVolumeChanged += v => Debug.Log($"[Demo] SFX volume → {v:F2}");
            instance.OnLanguageChanged += code => Debug.Log($"[Demo] Language → {code}");
            instance.OnNotificationsChanged += v => Debug.Log($"[Demo] Notifications → {v}");
            instance.OnHapticsChanged += v => Debug.Log($"[Demo] Haptics → {v}");
            instance.OnShow();
            instance.DismissRequested += _ => Destroy(instance.gameObject);
        }

        private T SpawnPopup<T>(GameObject prefab, string popupName) where T : UIModuleBase
        {
            if (prefab == null) { Debug.LogError($"[CatalogGroupDDemo] {popupName} prefab not assigned."); return null; }
            var go = Instantiate(prefab, _popupParent, false);
            var instance = go.GetComponent<T>();
            if (instance == null) { Debug.LogError($"[CatalogGroupDDemo] Spawned prefab missing {popupName} component."); Destroy(go); return null; }
            instance.Initialize(_theme, _services);
            return instance;
        }
    }
}
