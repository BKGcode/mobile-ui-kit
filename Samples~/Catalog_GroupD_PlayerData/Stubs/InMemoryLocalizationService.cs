using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupD
{
    /// <summary>
    /// In-memory <see cref="IUILocalizationService"/> sample stub. Inspector-editable initial
    /// language + available languages list. Awake validates and seeds the runtime state.
    /// On <c>SetLanguage</c>, raises <c>OnLanguageChanged</c> globally — buyer subscribes once
    /// from any popup or content code to drive the re-skin.
    /// <para>
    /// Kit does NOT translate strings (Non-goal #2 — BYO localization). This stub is the
    /// dispatch hub; buyer's localization layer (Unity Localization Package, I2, custom) listens
    /// to <c>OnLanguageChanged</c> and refreshes its own table.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class InMemoryLocalizationService : MonoBehaviour, IUILocalizationService
    {
        [Tooltip("ISO 639-1 codes the buyer's game supports (e.g. \"en\", \"es\", \"fr\"). Must contain at least one entry.")]
        [SerializeField] private List<string> _availableLanguages = new List<string> { "en", "es", "fr" };

        [Tooltip("Initial language code (must be in AvailableLanguages). Default \"en\".")]
        [SerializeField] private string _initialLanguage = "en";

        public string CurrentLanguage { get; private set; }
        public IReadOnlyList<string> AvailableLanguages => _availableLanguages;

        public event Action<string> OnLanguageChanged;

        private void Awake()
        {
            if (_availableLanguages == null || _availableLanguages.Count == 0)
            {
                Debug.LogError("[InMemoryLocalizationService] AvailableLanguages is empty. Add at least one ISO 639-1 code (e.g. \"en\") in the Inspector.", this);
                return;
            }
            if (!_availableLanguages.Contains(_initialLanguage))
            {
                Debug.LogWarning($"[InMemoryLocalizationService] InitialLanguage '{_initialLanguage}' not in AvailableLanguages. Falling back to '{_availableLanguages[0]}'.", this);
                _initialLanguage = _availableLanguages[0];
            }
            CurrentLanguage = _initialLanguage;
        }

        public void SetLanguage(string code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (!_availableLanguages.Contains(code))
                throw new ArgumentException($"Language code '{code}' is not in AvailableLanguages.", nameof(code));
            if (code == CurrentLanguage) return;
            CurrentLanguage = code;
            OnLanguageChanged?.Invoke(code);
        }
    }
}
