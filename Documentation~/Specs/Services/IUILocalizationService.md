# IUILocalizationService

> Status: **CONFIRMED 2026-05-04** — L1-L7 locked.
> Targets M2 (`v0.7.0-alpha`). Spec-2 of UI Kit Services namespace.

## Purpose

Central dispatch for "active player language" + "language change" events. Provides a single subscription point that ALL kit popups + buyer's content code consume to trigger re-skin on language switch.

The kit does **NOT translate strings** — kit-side English literals are frozen at `v1.0.0-rc` per Non-goal #2 (BYO localization). The service exists purely as the dispatch hub for buyer's localization layer (Unity Localization Package, I2 Localization, custom JSON, etc.). When language changes, buyer's re-skin code runs and updates ALL strings in the game.

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| L1 | Surface | `CurrentLanguage` (string getter) + `AvailableLanguages` (`IReadOnlyList<string>`) + `event Action<string> OnLanguageChanged` + `SetLanguage(string)` | Minimal: 4 members. Mirrors common localization service patterns (Unity Localization, I2). |
| L2 | Persistence dependency | NO — service does NOT depend on `IPlayerDataService` | Decouples. Buyer wires "read from PlayerData on boot → call `SetLanguage`" themselves. Kit doesn't impose persistence policy on the localization service. |
| L3 | Implementation shipping at M2 close | `InMemoryLocalizationService` (Sample stub only) | Buyer's Runtime localization is custom (Unity Loc / I2 / JSON). No reasonable stock Runtime default beyond in-memory. Buyer writes ~20 lines to bridge to their localization system. |
| L4 | Constructor validation | Constructor takes (`currentCode`, `availableCodes`); throws `ArgumentException` if `currentCode ∉ availableCodes` OR `availableCodes` empty | Fail loud — silent fallback hides config bugs. Buyer detects misconfiguration at boot, not at first language switch. |
| L5 | `SetLanguage` with same code | No-op (no event emitted) | Avoid spurious re-skin triggers when buyer's code redundantly sets the same language. |
| L6 | `SetLanguage` with unknown code | Throws `ArgumentException` | Fail loud. Buyer's code should validate against `AvailableLanguages` before calling. |
| L7 | Event subscription pattern | Single global subscription per consumer (popup, screen, buyer content). Multi-popup re-skin propagates via the central event chain | Buyer subscribes once globally → all kit + buyer popups re-skin atomically. Don't create per-popup subscriptions that leak on Hide. |

## Surface

```csharp
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
```

4 members. `AvailableLanguages` is immutable post-construction (no add/remove methods — would complicate buyer wiring; if buyer needs dynamic languages, they re-construct the service).

## Implementation shipping at M2 close

### `InMemoryLocalizationService` (Samples + Tests)
- Constructor: `InMemoryLocalizationService(string currentCode, IReadOnlyList<string> availableCodes)`.
- Throws `ArgumentException` if `availableCodes` is null/empty OR `currentCode` is not contained.
- `SetLanguage(code)`: validates against `AvailableLanguages`; if same as `CurrentLanguage` → no-op; else updates + raises `OnLanguageChanged`.
- Path: `Samples~/Catalog_GroupD_PlayerData/Stubs/InMemoryLocalizationService.cs` (mirrors Group B/C stub layout).
- Used by `SettingsPopup` tests.

## Buyer integration patterns (M4 QUICKSTART entries)

### Pattern A — Bridge to Unity Localization Package (~20 lines)
Buyer implements `IUILocalizationService` wrapping `LocalizationSettings.SelectedLocale` and `LocalizationSettings.AvailableLocales`. `SetLanguage` calls `LocalizationSettings.SelectedLocale = locale`. Buyer subscribes to Unity Localization's locale change → emits `OnLanguageChanged` from the bridge impl.

### Pattern B — Boot synchronization (PlayerData → LocalizationService)
```csharp
// In buyer's bootstrapper, before showing first popup:
var savedLang = _playerData.GetString("kfmui.settings.language", "");
var initialLang = string.IsNullOrEmpty(savedLang) ? "en" : savedLang;
if (_localization.AvailableLanguages.Contains(initialLang))
{
    _localization.SetLanguage(initialLang); // raises OnLanguageChanged → buyer's re-skin runs
}
// else: keep service's constructor-time default
```

### Pattern C — SettingsPopup → re-skin chain (kit-internal)
SettingsPopup language picker change → calls `_localization.SetLanguage(code)` → service raises `OnLanguageChanged` → buyer's globally-subscribed re-skin handler refreshes all visible content. ALSO writes `kfmui.settings.language` to `IPlayerDataService` for cross-session persistence.

## Edge cases

- **`AvailableLanguages` empty at constructor**: throws `ArgumentException`. Service must always have ≥1 language.
- **`SetLanguage("")`**: allowed ONLY if `""` is contained in `AvailableLanguages` (some buyers reserve `""` for "follow system locale"). Otherwise throws.
- **Concurrent `SetLanguage` from multiple sources**: last writer wins. `OnLanguageChanged` raises per `Set` call (no coalescing). Buyer's re-skin handler must be idempotent.
- **Subscriber throws inside `OnLanguageChanged`**: Bug in buyer's handler. Service does not catch — exception propagates and may halt other subscribers. Documented as buyer responsibility.
- **Disposing**: no `IDisposable`. Service lives for app lifetime in stock impl. Buyer wraps in their own lifecycle if needed.

## Test strategy

- Tests use `InMemoryLocalizationService` exclusively.
- Each test instantiates a fresh service.
- Minimum 10 tests:
  - Constructor validation: empty available throws (1), current-not-in-available throws (1).
  - Surface coverage: `CurrentLanguage` getter (1), `AvailableLanguages` immutability (1), `SetLanguage` happy path raises event (1).
  - Edge cases: same-code no-op (1), unknown-code throws (1), event delivers new code (1), multi-subscriber receives (1), unsubscribe stops delivery (1).

## Out of scope

- String table / translation lookup (BYO localization — Non-goal #2).
- Async language load (synchronous switching only at v1.0.0-rc).
- Locale-aware formatting (numbers, dates, currency) — delegated to `System.Globalization`.
- Right-to-left layout switching (post-v1.0.0 — UI Toolkit territory).
- Dynamic add/remove of languages post-construction (re-construct the service).
- Hot-reload of language assets (buyer's localization layer concern).
