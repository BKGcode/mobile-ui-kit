# KitforgeLabs · UI Kit — Cheatsheet

> One-page reference. **Goal**: spawn any of the 17 catalog elements in under 30 seconds. Per-element spec lives under [Specs/Catalog/](Specs/Catalog/). Cross-cutting contracts live in [Specs/CATALOG.md](Specs/CATALOG.md).

---

## 1. Quick start (drop-and-play)

1. Drag `Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab` into your scene.
2. Press **Play** → kit boots, `Theme_Default` applies, zero LogError.
3. Open **`KitforgeLabs → UI Kit → Hub`** → Setup wizard confirms the scene, Catalog tab generates `Show<T>(dto)` / `Push<T>(dto)` snippets, Theme Studio previews the 3 shipped themes.

`KitforgeRoot.prefab` ships pre-wired with: 3 managers (`PopupManager` / `UIManager` / `ToastManager`) + `UIServices` container + `EventSystem` + global `PopupBackdrop` + `KitforgeThemeBinder` bound to `Theme_Default`.

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

| Element | When to use | Events you wire |
|---|---|---|
| **ConfirmPopup** | Yes/No question (single-button when `ShowCancel=false`) | `OnConfirmed` · `OnCancelled` · `OnDismissed` |
| **PausePopup** | In-game pause overlay (owns `Time.timeScale` while visible) | `OnResume` · `OnRestart` · `OnSettings` · `OnHome` · `OnShop` · `OnHelp` · `OnQuit` · audio toggles · `OnPaused` / `OnResumed` |
| **TutorialPopup** | Multi-step onboarding with Next / Previous / Skip | `OnNext(int)` · `OnPrevious(int)` · `OnStepChanged(int)` · `OnSkip` · `OnCompleted` · `OnDismissed` |
| **RewardPopup** | Player gets coins / gems / item / bundle | `OnClaimed(CurrencyType, int)` · `OnDismissed` |
| **ShopPopup** | Grid of items + buy buttons | `OnPurchaseCompleted` · `OnPurchaseInsufficient` · `OnClosed` · `OnDismissed` |
| **NotEnoughCurrencyPopup** | Player can't afford → offer Buy / Watch ad | `OnBuyMoreRequested` · `OnWatchAdRequested` · `OnDeclined` · `OnDismissed` |
| **DailyLoginPopup** | 7-day calendar reward (auto-trigger via `DailyLoginFlow.ShowIfDue`) | `OnDayClaimed` · `OnWatchAdRequested` · `OnDismissed` |
| **LevelCompletePopup** | End-of-level victory (stars + score + rewards) | `OnNextRequested` · `OnRetryRequested` · `OnMainMenuRequested` · `OnDismissed` |
| **GameOverPopup** | Player died → Continue (ad / currency) / Restart / Quit | `OnContinueWithAdRequested` · `OnContinueWithCurrencyRequested` · `OnRestartRequested` · `OnMainMenuRequested` · `OnDismissed` |
| **SettingsPopup** | Audio / language / notifications / haptics | `OnMusicVolumeChanged` · `OnSfxVolumeChanged` · `OnLanguageChanged` · `OnNotificationsChanged` · `OnHapticsChanged` · `OnDismissed` |

### Toasts — `_toastManager.Show<T>(data)`

| Element | When | Events |
|---|---|---|
| **NotificationToast** | Non-blocking auto-dismiss banner with severity tint | `OnTapped` · `OnDismissed` |

### Screens — `_uiManager.Push<T>(data)`

| Element | When | Events |
|---|---|---|
| **LoadingScreen** | Initial boot / async load. Optional progress bar + spinner. | `OnProgressComplete` · `OnMinDisplayTimeElapsed` |
| **MainMenuScreen** | Hub after Loading (Play / Settings / Shop / Daily) | `OnPlayRequested` · `OnSettingsRequested` · `OnShopRequested` · `OnDailyRequested` · `OnBackRequested` · `OnShown` |

### HUDs — drag prefab into `KitforgeRoot/UICanvas/ScreenRoot`

| Element | What it shows | Service required | Notes |
|---|---|---|---|
| **HUDCoins** | Coin counter | `IEconomyService` | Auto-subscribes to `OnChanged`; punch tween on update; `--` shown if service null |
| **HUDGems** | Gem counter | `IEconomyService` | Same as Coins, currency = Gems |
| **HUDEnergy** | Energy bar + regen countdown + cap label | `IEconomyService` + `IProgressionService` + `ITimeService` | 1Hz poll for regen state; silent degrade (regen UI hidden) if Progression null |
| **HUDTimer** | Live timer label (3 modes) | `ITimeService` (UTC modes only) | Modes: `CountdownToTarget` / `CountupSinceTarget` / `LocalStopwatch` |

---

## 4. Theme — swap + customize

`UIThemeConfig` is a `ScriptableObject` — create as many as you want. Three presets ship in `Runtime/Theme/Presets/`:

- `Theme_Default` — neutral baseline
- `Theme_Casual` — bright, saturated
- `Theme_Premium` — dark, desaturated

**Swap at design time**: assign any `UIThemeConfig.asset` to `KitforgeRoot/KitforgeThemeBinder._theme`.

**Swap at runtime**:
```csharp
_themeBinder.SetTheme(myTheme); // distributes to 3 managers + re-Initializes cached instances
```

**Create a custom theme**: **Assets → Create → KitforgeLabs → UI Kit → Theme** (or duplicate a preset). Tune slots in Inspector (16 colors · sprites · 1 font · audio cues · `_defaultAnimPreset` · safe-area config).

---

## 5. Settings persistence

If you wire `SettingsPopup` + `IPlayerDataService` (default `PlayerPrefsPlayerDataService` ships in Runtime), these persist out of the box:

| Setting | PlayerPrefs key |
|---|---|
| Music volume | `kfmui.settings.music_volume` |
| SFX volume | `kfmui.settings.sfx_volume` |
| Language | `kfmui.settings.language` |
| Notifications enabled | `kfmui.settings.notifications` |
| Haptics enabled | `kfmui.settings.haptics` |
| DailyLogin streak | `kfmui.dailylogin.last_claim_utc` (+ 2 more) |

You wire `OnMusicVolumeChanged(float)` → your `AudioMixer.SetFloat("MusicVol", volumeDb)`. The kit doesn't touch your mixer.

Game-state save (level progress / inventory) is your responsibility — implement `IPlayerDataService` yourself or use the 12-method primitive surface for arbitrary keys.

---

## 6. Top friction points

| Symptom | Fix |
|---|---|
| HUDCoins shows `--` | Drag your `IEconomyService` impl into `KitforgeRoot/UIServices.Economy` |
| Music slider does nothing audible | Wire `OnMusicVolumeChanged` to your `AudioMixer.SetFloat("MusicVol", ...)` |
| `Show<MyPopup>` errors `No prefab registered` | Drag your popup prefab into `PopupManager._popupPrefabs[]` |
| Theme change doesn't reskin | Use `KitforgeThemeBinder.SetTheme(asset)` — never edit a Theme asset and expect live re-skin |
| Unity warns "Multiple EventSystems" | Delete the duplicate (`KitforgeRoot` ships its own) |
| HUDEnergy regen countdown hidden | Wire `IProgressionService` (otherwise silent degrade) |
| HUDTimer shows `--:--` | Wire `ITimeService` (UTC modes only); `LocalStopwatch` mode works without |
| Popup spawns but no animation | Run `KitforgeLabs → UI Kit → Bootstrap Defaults` once, then assign a preset to `Theme._defaultAnimPreset` |
| DailyLoginPopup doesn't trigger on first launch | `DailyLoginFlow.ShowIfDue` treats missing key as "due" — wire `IPlayerDataService` |
| Input System exception on Play | Resolved in v1.0.1+: the kit swaps the legacy input module automatically |

---

## 7. Going deeper

- **Architectural contracts**: [Specs/CATALOG.md](Specs/CATALOG.md)
- **Per-element specs**: [Specs/Catalog/](Specs/Catalog/)
- **Service interfaces**: [Specs/Services/](Specs/Services/)
- **Hub** (`KitforgeLabs → UI Kit → Hub`): Setup / Catalog browser / Theme Studio / Test launcher / inline Cheatsheet
- **Bootstrap Defaults** (`KitforgeLabs → UI Kit → Bootstrap Defaults`): generates 10 `UIAnimPreset` assets at `Assets/KitforgeLabs/UI Kit/Settings/UIAnimPresets/`
