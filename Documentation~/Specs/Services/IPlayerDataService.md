# IPlayerDataService

> Status: **CONFIRMED 2026-05-04** — D1-D9 locked + Q1 resolved (retro-fit DailyLogin at M2).
> Targets M2 (`v0.7.0-alpha`). Backs `SettingsPopup` + DailyLoginPopup persistence and any future popup needing persistent state.

## Purpose

Persistence service for primitive key-value player data. Spec-1 of UI Kit Services namespace.

Backs `SettingsPopup` (volumes, language, toggles) at M2, and is the canonical persistence path for any kit popup needing cross-session state. Buyer with a custom save system implements the interface over their backing store; default implementations cover dev (`InMemory`) and prod (`PlayerPrefs`).

## Decisions to confirm

| # | Decision | Proposal | Rationale |
|---|---|---|---|
| D1 | Surface shape | Method-per-type primitives (`GetInt/SetInt/GetFloat/SetFloat/GetString/SetString/GetBool/SetBool/Has/Delete/Save/Reload`) | Mirrors `PlayerPrefs` API. Zero reflection, zero boxing, zero generic dispatch. Buyer with custom SaveSystem implements 12 trivial methods. |
| D2 | Generic `T Get<T>` rejected | NO — primitives only | Generic surface forces JSON layer (Newtonsoft dep) or runtime type-switch (smell + silent-fallback risk). Capability-gate fail: 0 use cases in M2 need non-primitive persistence. |
| D3 | Implementations shipping at M2 close | (a) `PlayerPrefsPlayerDataService` Runtime default; (b) `InMemoryPlayerDataService` Sample stub (test seam) | Buyer with custom SaveSystem implements interface themselves. Two stock impls cover dev (memory, hermetic tests) + prod (PlayerPrefs, persistent). |
| D4 | Key namespace | `kfmui.<scope>.<name>` (e.g. `kfmui.settings.musicVolume`) | Short (5 chars), unique across hypothetical Kitforge packages (`kfecon`, `kftutor`…), readable in PlayerPrefs registry browser. Avoids collision with buyer's existing keys. |
| D5 | Save semantics | `Set*` updates in-memory only; explicit `Save()` flushes to backing store; **Unity's PlayerPrefs lifecycle auto-flush on app pause/quit covers normal app lifecycle so buyer rarely calls `Save()` explicitly** — typical buyer pattern: 1 `Save()` call in `SettingsPopup.OnHide` (belt-and-braces flush before player navigates away) | Mirrors PlayerPrefs behavior (Set is fire-and-forget). Slider drag at 60fps = 60 in-memory writes, 0 disk flushes. Auto-save-on-Set rejected: ~5-50ms PlayerPrefs.Save() per call on Android = 300ms-3000ms frame stalls during slider drag (unacceptable). Manual `Save()` for "flush now" scenarios (pre-IAP transaction, before risky op, popup close belt-and-braces). |
| D6 | Reload semantics | `Reload()` discards in-memory cache, re-reads from store | Test scenario: simulate "app restart" without Domain Reload. PlayerPrefs impl just re-Gets on demand (no in-memory cache distinct from disk); InMemory impl resets to backing dictionary initial state. |
| D7 | Default values | Per-call `defaultValue` parameter (`int GetInt(string key, int defaultValue = 0)`) | Buyer controls per-callsite. Avoids global defaults registry (overengineered for 5 kit keys). |
| D8 | Kit-side keys frozen at `v1.0.0-rc` | YES — listed below; v1.x adds new keys only (additive), never renames/retypes/redefaults existing keys | Backwards compat for buyer upgrades. Buyer-side keys are buyer's responsibility (kit doesn't migrate them). |
| D9 | Restore defaults UX | OUT of `v1.0.0-rc` (kit-side); QUICKSTART recipe documents 5-line buyer pattern (`Delete` × N + `Save` + `Reload`) | Mobile market research (2026-05-04): top-grossing mobile titles (Royal Match, Candy Crush, Coin Master, Homescapes, Clash Royale) DO NOT ship "Restore defaults" — settings are minimal (3-5 toggles), per-setting reset suffices. Mid-core/console-port (Genshin, COD Mobile) ship it because surface is 50+ graphics settings. Our buyer's surface is 5 settings; reset button = UX hazard (accidental tap nukes prefs) more than UX win. |

## Surface

```csharp
namespace KitforgeLabs.MobileUIKit.Services
{
    public interface IPlayerDataService
    {
        int    GetInt(string key, int defaultValue = 0);
        void   SetInt(string key, int value);
        float  GetFloat(string key, float defaultValue = 0f);
        void   SetFloat(string key, float value);
        string GetString(string key, string defaultValue = "");
        void   SetString(string key, string value);
        bool   GetBool(string key, bool defaultValue = false);
        void   SetBool(string key, bool value);
        bool   Has(string key);
        void   Delete(string key);
        void   Save();
        void   Reload();
    }
}
```

12 methods total. No events. No async.

## Implementations shipping at M2 close

### `PlayerPrefsPlayerDataService` (Runtime, default)
- Wraps `UnityEngine.PlayerPrefs`. `GetBool` ↔ `GetInt(0|1)` mapping internal.
- `Save()` calls `PlayerPrefs.Save()` (forced flush to disk/registry).
- `Reload()` is no-op (PlayerPrefs has no in-memory cache distinct from disk; every `Get*` re-reads).
- Path: `Runtime/Services/PlayerPrefsPlayerDataService.cs`.
- Registered on `UIServices` MonoBehaviour at boot (Inspector ref).

### `InMemoryPlayerDataService` (Samples + Tests)
- Backing `Dictionary<string, object>`.
- `Save()` is no-op.
- `Reload()` clears the dictionary (resets to empty initial state — simulates "fresh app launch").
- Path: `Samples~/Catalog_GroupD_PlayerData/Stubs/InMemoryPlayerDataService.cs` (mirrors Group B/C stub layout).
- Used by every `IPlayerDataService` test (test seam — keeps EditMode tests hermetic; no PlayerPrefs touch).

## Kit-side canonical keys (frozen at `v1.0.0-rc`)

| Key | Type | Default | Owner | Notes |
|---|---|---|---|---|
| `kfmui.settings.musicVolume` | float | 1.0 | SettingsPopup | 0-1 range. 0 = muted. |
| `kfmui.settings.sfxVolume` | float | 1.0 | SettingsPopup | 0-1 range. 0 = muted. |
| `kfmui.settings.language` | string | `""` | SettingsPopup | Empty = system locale fallback. ISO 639-1 codes when set (`"en"`, `"es"`, …). |
| `kfmui.settings.notifications` | bool | true | SettingsPopup | Push notification opt-in. |
| `kfmui.settings.haptics` | bool | true | SettingsPopup | Vibration / haptic feedback opt-in. |
| `kfmui.dailylogin.lastClaimedDay` | int | -1 | DailyLoginPopup | 1-7 indexed day of last claim. `-1` = never claimed. Retro-fit at M2 — see Q1 resolved below. |
| `kfmui.dailylogin.streak` | int | 0 | DailyLoginPopup | Current consecutive-days streak counter. Resets on missed day. |
| `kfmui.dailylogin.lastSeenDay` | int | -1 | DailyLoginPopup | Day-of-year (or app-day index) of last popup view — drives "first time today" gating. `-1` = never seen. |

8 keys at `v1.0.0-rc` (5 settings + 3 dailylogin). All FROZEN — see migration policy below.

## Edge cases

- **Key not found**: `Get*` returns the per-call `defaultValue`. `Has(key) = false`.
- **Type mismatch on PlayerPrefs**: e.g. `GetString` on a key written via `SetInt` — Unity returns `""` silently. PlayerPrefs impl does NOT raise (preserves Unity behavior). InMemory impl throws `InvalidCastException` (test surface — fail loud during dev).
- **Empty key**: `Get*("")` allowed but logs warning once per session. PlayerPrefs accepts empty; InMemory accepts empty. Discouraged.
- **Null key**: throws `ArgumentNullException` — fast fail for dev errors.
- **`Delete` of non-existent key**: silent no-op (PlayerPrefs behavior).
- **Re-`Save()` with no Sets between**: PlayerPrefs idempotent (cheap noop after first flush). InMemory noop.

## Test strategy

- Tests use `InMemoryPlayerDataService` exclusively (no PlayerPrefs touch — keeps EditMode tests hermetic).
- Each test instantiates a fresh service (no shared state).
- Minimum 16 tests:
  - 12 happy-path: 1 per surface method.
  - 4 edge cases: default fallback, `Has` after `Delete`, `Reload` clears, type mismatch on InMemory throws.
- PlayerPrefs impl has 1 PlayMode smoke test wired via `[ContextMenu]` — NOT EditMode (PlayerPrefs is process-global state; would leak between tests).

## Buyer extension patterns (M4 QUICKSTART entries)

### Pattern 1 — Restore defaults (~5 lines)
```csharp
// In a Reset button click handler:
var keys = new[]
{
    "kfmui.settings.musicVolume",
    "kfmui.settings.sfxVolume",
    "kfmui.settings.language",
    "kfmui.settings.notifications",
    "kfmui.settings.haptics",
};
foreach (var key in keys) _playerData.Delete(key);
_playerData.Save();
_playerData.Reload(); // SettingsPopup re-binds defaults on next Show
```

### Pattern 2 — Custom SaveSystem integration
Buyer implements `IPlayerDataService` over their backing store (cloud, encrypted file, custom binary, …); assigns it on `UIServices` at boot replacing the default `PlayerPrefsPlayerDataService`. Twelve methods, all trivial wrappers.

### Pattern 3 — Custom keys
Buyer uses ANY prefix EXCEPT `kfmui.` (reserved). E.g. `mygame.player.coins`. Kit never reads buyer-prefixed keys.

## Migration policy

- Kit-side keys (table above) are FROZEN at `v1.0.0-rc`. New v1.x keys are **additive only** — no rename, no type change, no default change.
- v0.x → v1.0 buyers: dev-time data shipped before freeze is "as-is" — buyers running v0.x in production may lose data on v1.0 upgrade (acceptable for alpha→stable transition; documented in MIGRATION.md at M4).
- Buyer-side keys: buyer's responsibility. Kit does NOT migrate, validate, or version buyer-prefixed keys.

## Out of scope

- Async / IO-bound API (PlayerPrefs is sync; future v1.x could add `IPlayerDataServiceAsync` for cloud-sync impls — NOT `v1.0.0-rc`).
- Encryption (buyer adds via decorator pattern over `IPlayerDataService` if needed).
- Cloud sync (Game Center, Play Games, custom backend — buyer responsibility).
- Schema migration framework (kit keys frozen; buyer keys = buyer's domain).
- Generic `T Get<T>` surface (rejected — see D2).
- `OnKeyChanged` / `OnSaved` events (no use case in M2; SettingsPopup pulls on `OnShow`, pushes on slider change; runtime state propagation is the audio/locale services' job, not the persistence service's).

## Resolved questions (locked 2026-05-04)

### Q1 — DailyLoginPopup persistence retro-fit at M2 → ✅ **RETRO-FIT (option a)**

DailyLoginPopup (shipped M1, `v0.6.0-alpha`) currently consumes `IProgressionService` (Group C sample stub `InMemoryProgressionService`) for `lastClaimedDay` / `streak` / `lastSeenDay`. At M2 the `InMemoryProgressionService` stub gets retro-fitted to back those 3 fields with `IPlayerDataService.GetInt/SetInt`. Adds 3 keys to canonical list (rows above).

**Why retro-fit (lens: most common in mobile games + most comfortable for player and dev team)**:
- 100% of mobile games with daily-login persist streak. Without it the popup looks broken.
- Buyer evaluating the kit imports DailyLogin sample → presses Play → claims Day 1 → stops Play → re-presses Play. Without persistence: Day 1 still claimable. **Demo fails first-impression test** (per `/_checker as user` "fresh import smoke test" lens).
- Dev cost: ~30-60 min (3 calls in `InMemoryProgressionService`). Fits M2 budget.

**Risk gate**: if retro-fit ripples through `IProgressionService` interface (>1hr work), revert to v1.x additive deferral and document in CHANGELOG. Validate at M2 mid-point.

**Trade-off accepted**: `v1.0.0-rc` canonical list grows from 5 to 8 keys. Backwards-compat story unchanged (additive policy applies equally to 5 or 8 frozen keys).
