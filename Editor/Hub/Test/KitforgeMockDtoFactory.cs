using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Tutorial;
using KitforgeLabs.MobileUIKit.Editor.Hub.Catalog;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Hub.Test
{
    public static class KitforgeMockDtoFactory
    {
        public static object Create(KitforgeCatalogEntry entry) => CreateForComponent(entry.ComponentType, entry.DisplayName);

        public static object CreateForComponent(Type componentType, string displayName)
        {
            var dataType = KitforgeCatalogSnippetBuilder.ResolveDataTypeForComponent(componentType);
            var data = CreateInstanceSafe(dataType, displayName);
            if (data != null) ApplySmartDefaults(data, displayName);
            return data;
        }

        private static object CreateInstanceSafe(Type dataType, string displayName)
        {
            if (dataType == null) return null;
            try
            {
                return Activator.CreateInstance(dataType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KitforgeMockDtoFactory] Failed to create mock data for {displayName}: {ex.Message}");
                return null;
            }
        }

        private static void ApplySmartDefaults(object data, string displayName)
        {
            var type = data.GetType();
            SetIfStringField(type, data, "Title", $"Test {displayName}");
            SetIfStringField(type, data, "Subtitle", $"Mock {displayName}");
            SetIfStringField(type, data, "Message", "Mock spawn — click any button to dismiss.");
            ApplyTypeSpecificDefaults(data);
        }

        private static void ApplyTypeSpecificDefaults(object data)
        {
            switch (data)
            {
                case DailyLoginPopupData dl: PopulateDailyLoginCalendar(dl); break;
                case TutorialPopupData tut: PopulateTutorialSteps(tut); break;
            }
        }

        private static void PopulateDailyLoginCalendar(DailyLoginPopupData dto)
        {
            if (dto.RewardEntries != null && dto.RewardEntries.Length > 0) return;
            var entries = new DailyLoginRewardEntry[7];
            for (var i = 0; i < 7; i++)
            {
                var amount = (i + 1) * 100;
                var isBig = i == 6;
                entries[i] = new DailyLoginRewardEntry
                {
                    Label = $"Day {i + 1}",
                    IsBigReward = isBig,
                    AllowDouble = isBig,
                    Rewards = new[]
                    {
                        new RewardPopupData { Title = $"Day {i + 1} reward", Kind = RewardKind.Coins, Amount = amount },
                    },
                };
            }
            dto.RewardEntries = entries;
        }

        private static void PopulateTutorialSteps(TutorialPopupData dto)
        {
            if (dto.Steps != null && dto.Steps.Count > 0) return;
            dto.Steps = new List<TutorialStep>
            {
                new TutorialStep { Title = "Welcome", Body = "Mock tutorial step 1 — replace with your own copy." },
                new TutorialStep { Title = "Combo", Body = "Mock tutorial step 2 — drag a card to merge." },
                new TutorialStep { Title = "Score", Body = "Mock tutorial step 3 — match three to score." },
            };
        }

        private static void SetIfStringField(Type type, object data, string fieldName, string value)
        {
            var field = type.GetField(fieldName);
            if (field == null || field.FieldType != typeof(string)) return;
            field.SetValue(data, value);
        }
    }
}
