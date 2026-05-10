# Kitforge Mobile UI Kit — Cheatsheet

> One-page reference for game designers. **Goal**: spawn any of the 17 catalog elements in under 30 seconds without reading specs. For architectural contracts see [Specs/CATALOG.md](Specs/CATALOG.md). For per-element deep specs see [Specs/Catalog/](Specs/Catalog/).

---

## 1. Quick start (drop-and-play)

1. Drag `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab` into your scene.
2. Press **Play** → kit boots, `Theme_Default` applies, zero LogError.
3. Browse live demos: `Tools → Kitforge → UI Kit → Build Group [A|B|C|D|E] Sample` → opens demo scene with right-click `[ContextMenu]` triggers per element.

`KitforgeRoot.prefab` ships pre-wired with: 3 managers (`PopupManager` / `UIManager` / `ToastManager`) + `UIServices` container + `EventSystem` + global `PopupBackdrop` + `KitforgeThemeBinder` bound to `Theme_Default`. The 3 managers' `_themeConfig` fields are deliberately null — the binder is the single source of truth.

---

## 2. The 3 spawn patterns

```csharp
// Popup (modal / gameplay / meta) — managed by PopupManager
var popup = _popupManager.Show<ConfirmPopup>(new ConfirmPopupData {
    Title = "Quit?", Message = "Progress will be lost.",
    ConfirmLabel = "Yes", CancelLabel = "Stay"
});
popup.OnConfirmed += () => QuitToMenu();

// Screen (full-canvas, stack-based) — managed by UIManager
_uiManager.Push<MainMenuScreen>(new MainMenuScreenData { Title = "MyGame" });
// _uiManager.Pop() returns to previous screen; _uiManager.Replace<T>(data) swaps top.

// Toast (non-blocking, auto-dismiss) — managed by ToastManager
_toastManager.Show<NotificationToast>(new NotificationToastData {
    Message = "Saved!", Severity = ToastSeverity.Success
});
```

All popups expose `OnDismissed`. All popups read services via `Services.X.Y()` — never call services directly from UI.

---

## 3. Catalog at a glance (17 elements)

### Popups — `_popupManager.Show<T>(data)`

| Element | When to use | Key DTO fields | Events you wire |
|---|---|---|---|
| **ConfirmPopup** | Yes/No question (single-button when `ShowCancel=false`) | `Title` · `Message` · `ConfirmLabel` · `CancelLabel` · `Tone` · `ShowCancel` · `CloseOnBackdrop` | `OnConfirmed` · `OnCancelled` · `OnDismissed` |
| **PausePopup** | In-game pause overlay (owns `Time.timeScale` while visible) | `Title` · `Subtitle` · `Show*` toggles (Resume/Restart/Settings/Home/Shop/Help/Quit) · `Show*Toggle` (Sound/Music/Vibration) · `*On` · `CloseOnBackdrop` | `OnResume` · `OnRestart` · `OnSettings` · `OnHome` · `OnShop` · `OnHelp` · `OnQuit` · `OnSoundChanged(bool)` · `OnMusicChanged(bool)` · `OnVibrationChanged(bool)` · `OnPaused` · `OnResumed` |
| **TutorialPopup** | Multi-step onboarding with Next / Previous / Skip | `Steps[]` (Title/Body/Image) · `StartIndex` · `ShowPrevious` · `ShowSkip` · `LoopBackToFirst` · `TapToAdvance` · `*Label` | `OnNext(int)` · `OnPrevious(int)` · `OnStepChanged(int)` · `OnSkip` · `OnCompleted` · `OnDismissed` |
| **RewardPopup** | Player gets coins / gems / item / bundle | `Kind` · `Amount` · `ItemId` · `IconOverride` · `BundleLines[]` · `AutoClaimSeconds` · `ClaimLabel` | `OnClaimed(CurrencyType, int)` · `OnDismissed` |
| **ShopPopup** | Grid of items + buy buttons | `Title` · `Category` · `CloseOnBackdrop` | `OnPurchaseCompleted(ShopItemData, PurchaseResult)` · `OnPurchaseInsufficient(ShopItemData)` · `OnClosed` · `OnDismissed` |
| **NotEnoughCurrencyPopup** | Player can't afford → offer Buy / Watch ad | `Currency` · `Required` · `Missing` · `ShowBuyMore` · `ShowWatchAd` · `ShowDecline` · `*Label` | `OnBuyMoreRequested(CurrencyType, int)` · `OnWatchAdRequested(CurrencyType, int)` · `OnDeclined` · `OnDismissed` |
| **DailyLoginPopup** | 7-day calendar reward (auto-trigger via `DailyLoginFlow.ShowIfDue`) | `RewardEntries[]` · `CurrentDay` · `LastClaimUtc` · `AlreadyClaimedToday` · `DoubledToday` · `MaxStreakGapDays` | `OnDayClaimed(int day, RewardPopupData[])` · `OnWatchAdRequested(int, RewardPopupData[])` · `OnDismissed` |
| **LevelCompletePopup** | End-of-level victory (stars + score + rewards) | `Stars` · `Score` · `BestScore` · `IsNewBest` · `Rewards[]` (RewardPopupData) · `Show*` toggles | `OnNextRequested(data)` · `OnRetryRequested(data)` · `OnMainMenuRequested(data)` · `OnDismissed` |
| **GameOverPopup** | Player died → Continue (ad / currency) / Restart / Quit | `Score` · `ContinueMode` (Ad/Currency) · `ContinueCurrency` · `ContinueAmount` · `MaxContinuesPerSession` · `BackPressBehavior` | `OnContinueWithAdRequested` · `OnContinueWithCurrencyRequested(CurrencyType, int)` · `OnContinueAffordCheckFailed(CurrencyType, int)` · `OnRestartRequested` · `OnMainMenuRequested` · `OnDismissed` |
| **SettingsPopup** | Audio / language / notifications / haptics | `Show*Slider/Toggle/Picker` · `LanguageOptions[]` (Code+DisplayName) | `OnMusicVolumeChanged(float)` · `OnSfxVolumeChanged(float)` · `OnLanguageChanged(string)` · `OnNotificationsChanged(bool)` · `OnHapticsChanged(bool)` · `OnDismissed` |

### Toasts — `_toastManager.Show<T>(data)`

| Element | When | Key fields | Events |
|---|---|---|---|
| **NotificationToast** | Non-blocking auto-dismiss banner | `Message` · `Severity` (Info/Success/Warning/Error) · `DurationOverride` · `TapToDismiss` | `OnTapped` · `OnDismissed` |

### Screens — `_uiManager.Push<T>(data)`

| Element | When | Key fields | Events |
|---|---|---|---|
| **LoadingScreen** | Initial boot / async load | `Title` · `Subtitle` · `InitialProgress` · `ShowProgressBar` · `ShowSpinner` · `MinDisplaySeconds` | `OnProgressComplete` · `OnMinDisplayTimeElapsed` |
| **MainMenuScreen** | Hub after Loading (Play / Settings / Shop / Daily) | `Title` · `ShowPlayButton` · `ShowSettingsButton` · `ShowShopButton` · `ShowDailyButton` | `OnPlayRequested` · `OnSettingsRequested` · `OnShopRequested` · `OnDailyRequested` · `OnBackRequested` · `OnShown` |

### HUDs — drag prefab into `KitforgeRoot/UICanvas/ScreenRoot`

| Element | What it shows | Service required | Notes |
|---|---|---|---|
| **HUDCoins** | Coin counter | `IEconomyService` | Auto-subscribes to `OnChanged`; punch tween on update; `--` shown if service null |
| **HUDGems** | Gem counter | `IEconomyService` | Same as Coins, currency = Gems |
| **HUDEnergy** | Energy bar + regen countdown + cap label | `IEconomyService` + `IProgressionService` + `ITimeService` | 1Hz poll for regen state; silent degrade (regen UI hidden) if Progression null |
| **HUDTimer** | Live timer label (3 modes) | `ITimeService` (UTC modes only) | Modes: `CountdownToTarget` / `CountupSinceTarget` / `LocalStopwatch` · `SetTarget(DateTime)` runtime API · `OnExpired` / `OnWarningEntered` events |

---

## 4. Theme — swap + customize (themes are unlimited)

**`UIThemeConfig` is a ScriptableObject — buyers create N themes, never capped at 3.** `Theme_Default` ships with the package; `Theme_Casual` + `Theme_Premium` ship as M4.1 sample assets (move to `Runtime/Theme/Presets/` deferred to M5.4).

**Swap theme at design time**:
- `KitforgeRoot → KitforgeThemeBinder._theme` slot → drag any `UIThemeConfig.asset`.

**Swap theme at runtime**:
```csharp
_themeBinder.SetTheme(myTheme); // distributes to 3 managers + re-Initializes cached instances
```

**Create a custom theme**:
1. `Assets → Create → Kitforge → Theme` (or duplicate `Theme_Default.asset`).
2. Tune slots in Inspector (16 colors · sprites · 1 font · audio cues · `_defaultAnimPreset` · safe-area config).
3. Drag your asset to `KitforgeThemeBinder._theme`.

Theme slots cover: 16 colors (Primary/Secondary/Accent/Tertiary/Muted/SuccessColor/WarningColor/FailureColor + 4 background slots + 4 text slots) · 9 sprite slots (icons + backdrop) · audio cues (ButtonTap / PopupOpen / PopupClose / Success / Error) · `_defaultAnimPreset` (popup show/hide motion). Future M5.1 will add `_titleFont` / `_bodyFont` / `_labelFont` separation (currently single `_fontPrimary`).

---

## 5. Settings persistence — what's automatic

If you wire `SettingsPopup` + `IPlayerDataService` (default = `PlayerPrefsPlayerDataService` ships in Runtime), these persist **out of the box**:

| Setting | Persists OOTB? | PlayerPrefs key |
|---|---|---|
| Music volume | ✅ | `kf_settings.music_volume` |
| SFX volume | ✅ | `kf_settings.sfx_volume` |
| Language | ✅ | `kf_settings.language` |
| Notifications enabled | ✅ | `kf_settings.notifications` |
| Haptics enabled | ✅ | `kf_settings.haptics` |
| DailyLogin streak | ✅ | `kf_dailylogin.last_claim_utc` (+2 more) |

**You wire**: `OnMusicVolumeChanged(float)` → your `AudioMixer.SetFloat("MusicVol", volumeDb)`. The kit doesn't know your mixer.

**Game-state save (saved games / level progress / inventory) is YOUR responsibility**. Either implement `IPlayerDataService` yourself or use the 12-method primitive surface (`GetInt/SetInt/GetFloat/SetFloat/GetString/SetString/GetBool/SetBool/Has/Delete/Save/Reload`) for arbitrary keys.

---

## 6. Top friction points (read me first)

| Symptom | Root cause | Fix |
|---|---|---|
| HUDCoins shows `--` | `IEconomyService` not wired | Drag your service into `KitforgeRoot/UIServices.Economy` slot |
| Music slider does nothing audible | `OnMusicVolumeChanged` event unsubscribed | Wire it to your `AudioMixer.SetFloat("MusicVol", ...)` in your game code |
| `Show<MyPopup>` errors `No prefab registered` | Popup not present in `PopupManager._popupPrefabs[]` Inspector array | Drag your popup prefab into the `_popupPrefabs` array on `KitforgeRoot/PopupManager` (Inspector). For catalog popups: open a Group sample scene (built via `Tools → Kitforge → UI Kit → Build Group X Sample`) — those ship with prefabs pre-wired. |
| Theme change doesn't reskin | Modified asset directly OR forgot `SetTheme` | Use `KitforgeThemeBinder.SetTheme(asset)` (re-distributes); or restart Play |
| Unity warns "Multiple EventSystems" | Buyer scene already had one; `KitforgeRoot.prefab` ships its own | Delete one of the two |
| `HUDEnergy` regen countdown hidden | `IProgressionService` not wired | Wire it; HUD silently degrades when missing (per spec § 4.5) |
| `HUDTimer` shows `--:--` in UTC mode | `ITimeService` not wired | Wire it; UTC modes need server-time source. `LocalStopwatch` mode works without service. |
| Popup spawns but no animation | `_defaultAnimPreset` null on Theme | Wire a `UIAnimPreset.asset` (defaults under `Assets/Settings/UIAnimPresets/` after `Bootstrap Defaults` runs once) |
| `DailyLoginPopup` doesn't trigger on first launch | `LastClaimUtc = DateTime.MinValue` not stored | `DailyLoginFlow.ShowIfDue` checks `IPlayerDataService.GetString("kf_dailylogin.last_claim_utc")`; on first launch missing key = treated as "due" |

---

## 7. Going deeper

- **Architectural contracts (MUST / MUSTN'T)**: [Specs/CATALOG.md](Specs/CATALOG.md)
- **Per-element specs (DTO complete · service interface · edge cases)**: [Specs/Catalog/](Specs/Catalog/)
- **Service interfaces** (`IEconomyService`, `IPlayerDataService`, `IProgressionService`, `ITimeService`, `IUILocalizationService`, `IAdsService`, `IShopDataProvider`, `IUIAudioService`): [Specs/Services/](Specs/Services/)
- **Sample scenes**: `Tools → Kitforge → UI Kit → Build Group [A-E] Sample` → live demos with `[ContextMenu]` triggers (right-click in Hierarchy on the demo host).
- **Audit gate** (pre-tag verification): `Tools → Kitforge → UI Kit → Audit → Run Everything` — structural integrity scanner across 23 targets (17 prefabs + 6 demo scenes).
- **Bootstrap Defaults** (one-shot): `Tools → Kitforge → UI Kit → Bootstrap Defaults` generates 10 `UIAnimPreset` SOs in `Assets/Settings/UIAnimPresets/` for the 10 animation styles (Subtle / Bouncy / Snappy / Smooth / Dramatic / Playful / Sharp / Soft / Energetic / Calm).
