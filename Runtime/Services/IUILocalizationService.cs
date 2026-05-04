using System;
using System.Collections.Generic;

namespace KitforgeLabs.MobileUIKit.Services
{
    public interface IUILocalizationService
    {
        string CurrentLanguage { get; }
        IReadOnlyList<string> AvailableLanguages { get; }
        event Action<string> OnLanguageChanged;
        void SetLanguage(string code);
    }
}
