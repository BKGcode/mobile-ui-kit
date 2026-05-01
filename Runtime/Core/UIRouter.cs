using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Routing;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Core
{
    [DisallowMultipleComponent]
    public sealed class UIRouter : MonoBehaviour
    {
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private PopupManager _popupManager;
        [SerializeField] private AppState _initialState = AppState.Loading;

        private AppState _currentState;
        private bool _isTransitioning;
        private bool _isInitialized;
        private readonly HashSet<Type> _popupAllowList = new();
        private bool _popupAllowListActive;

        public AppState CurrentState => _currentState;
        public UIManager Screens => _uiManager;
        public PopupManager Popups => _popupManager;
        public bool HasPopupRestrictions => _popupAllowListActive;
        public bool IsInitialized => _isInitialized;

        public event Action<AppState, AppState> OnStateChanged;

        private void Start() => Initialize();

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            _currentState = _initialState;
            OnStateChanged?.Invoke(_initialState, _initialState);
        }

        public bool TransitionTo(AppState next)
        {
            if (_isTransitioning) return false;
            if (next == _currentState) return false;
            _isTransitioning = true;
            var previous = _currentState;
            _currentState = next;
            try { OnStateChanged?.Invoke(previous, next); }
            finally { _isTransitioning = false; }
            return true;
        }

        public void RestrictPopupsTo(IEnumerable<Type> allowedTypes)
        {
            _popupAllowList.Clear();
            if (allowedTypes != null)
            {
                foreach (var type in allowedTypes) _popupAllowList.Add(type);
            }
            _popupAllowListActive = true;
        }

        public void ClearPopupRestrictions()
        {
            _popupAllowList.Clear();
            _popupAllowListActive = false;
        }

        public bool IsValidPopup(Type popupType)
        {
            if (popupType == null) return false;
            if (!_popupAllowListActive) return true;
            return _popupAllowList.Contains(popupType);
        }

        public void HandleBackPressed()
        {
            if (_popupManager != null && _popupManager.DispatchBackPressed()) return;
            if (_uiManager == null) return;
            if (_uiManager.Current != null) _uiManager.Current.OnBackPressed();
        }
    }
}
