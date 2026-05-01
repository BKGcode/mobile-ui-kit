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
        private readonly HashSet<Type> _allowedPopupsByState = new();

        public AppState CurrentState => _currentState;
        public UIManager Screens => _uiManager;
        public PopupManager Popups => _popupManager;

        public event Action<AppState, AppState> OnStateChanged;

        private void Start()
        {
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

        public bool IsValidPopup(Type popupType)
        {
            if (popupType == null) return false;
            if (_allowedPopupsByState.Count == 0) return true;
            return _allowedPopupsByState.Contains(popupType);
        }

        public void HandleBackPressed()
        {
            if (_popupManager != null && _popupManager.DispatchBackPressed()) return;
            if (_uiManager == null) return;
            if (_uiManager.Current != null) _uiManager.Current.OnBackPressed();
        }
    }
}
