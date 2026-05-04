using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers
{
    internal sealed class FakeLocalizationService : IUILocalizationService
    {
        private readonly List<string> _available;
        private string _current;

        public string CurrentLanguage => _current;
        public IReadOnlyList<string> AvailableLanguages => _available;

        public event Action<string> OnLanguageChanged;

        public FakeLocalizationService(string currentCode, IReadOnlyList<string> availableCodes)
        {
            if (availableCodes == null || availableCodes.Count == 0)
                throw new ArgumentException("availableCodes must contain at least one entry.", nameof(availableCodes));
            if (currentCode == null) throw new ArgumentNullException(nameof(currentCode));
            _available = new List<string>(availableCodes);
            if (!_available.Contains(currentCode))
                throw new ArgumentException($"currentCode '{currentCode}' is not in availableCodes.", nameof(currentCode));
            _current = currentCode;
        }

        public void SetLanguage(string code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (!_available.Contains(code))
                throw new ArgumentException($"Language code '{code}' is not in AvailableLanguages.", nameof(code));
            if (code == _current) return;
            _current = code;
            OnLanguageChanged?.Invoke(code);
        }
    }
}
