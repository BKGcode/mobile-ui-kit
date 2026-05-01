using System.Collections.Generic;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Core
{
    [DisallowMultipleComponent]
    public sealed class PopupManager : MonoBehaviour
    {
        private const int MAX_DEPTH = 3;

        [SerializeField] private Transform _popupRoot;
        [SerializeField] private Canvas _popupCanvas;
        [SerializeField] private GameObject _backdrop;
        [SerializeField] private UIModuleBase[] _popupPrefabs;

        private readonly Dictionary<System.Type, UIModuleBase> _popupCache = new();
        private readonly List<PopupEntry> _activeStack = new();
        private readonly List<PopupRequest> _pendingQueue = new();

        public int ActiveCount => _activeStack.Count;
        public UIModuleBase TopPopup => _activeStack.Count > 0 ? _activeStack[_activeStack.Count - 1].Module : null;

        public T Show<T>(object data = null, PopupPriority priority = PopupPriority.Gameplay) where T : UIModuleBase
        {
            var request = new PopupRequest { Type = typeof(T), Data = data, Priority = priority };
            if (_activeStack.Count >= MAX_DEPTH)
            {
                _pendingQueue.Add(request);
                return null;
            }
            return (T)ShowInternal(request);
        }

        public void Dismiss(UIModuleBase popup)
        {
            for (var i = _activeStack.Count - 1; i >= 0; i--)
            {
                if (_activeStack[i].Module != popup) continue;
                HidePopup(_activeStack[i].Module);
                _activeStack.RemoveAt(i);
                break;
            }
            DrainQueue();
            UpdateBackdrop();
        }

        public void DismissAll()
        {
            for (var i = _activeStack.Count - 1; i >= 0; i--) HidePopup(_activeStack[i].Module);
            _activeStack.Clear();
            _pendingQueue.Clear();
            UpdateBackdrop();
        }

        public bool IsShowing<T>() where T : UIModuleBase
        {
            var type = typeof(T);
            for (var i = 0; i < _activeStack.Count; i++)
            {
                if (_activeStack[i].Module.GetType() == type) return true;
            }
            return false;
        }

        public bool DispatchBackPressed()
        {
            if (_activeStack.Count == 0) return false;
            var top = _activeStack[_activeStack.Count - 1].Module;
            top.OnBackPressed();
            return true;
        }

        private UIModuleBase ShowInternal(PopupRequest request)
        {
            var popup = ResolvePopup(request.Type);
            if (popup == null) return null;
            _activeStack.Add(new PopupEntry { Module = popup, Priority = request.Priority });
            popup.gameObject.SetActive(true);
            if (request.Data != null) popup.BindUntyped(request.Data);
            popup.OnShow();
            UpdateBackdrop();
            return popup;
        }

        private void DrainQueue()
        {
            while (_pendingQueue.Count > 0 && _activeStack.Count < MAX_DEPTH)
            {
                var next = _pendingQueue[0];
                _pendingQueue.RemoveAt(0);
                ShowInternal(next);
            }
        }

        private UIModuleBase ResolvePopup(System.Type type)
        {
            if (_popupCache.TryGetValue(type, out var cached)) return cached;
            var prefab = FindPrefab(type);
            if (prefab == null)
            {
                Debug.LogError($"[PopupManager] No prefab registered for {type.Name}.", this);
                return null;
            }
            var instance = Instantiate(prefab, _popupRoot);
            instance.gameObject.SetActive(false);
            _popupCache[type] = instance;
            return instance;
        }

        private UIModuleBase FindPrefab(System.Type type)
        {
            if (_popupPrefabs == null) return null;
            for (var i = 0; i < _popupPrefabs.Length; i++)
            {
                if (_popupPrefabs[i] != null && _popupPrefabs[i].GetType() == type) return _popupPrefabs[i];
            }
            return null;
        }

        private void HidePopup(UIModuleBase popup)
        {
            popup.OnHide();
            popup.gameObject.SetActive(false);
        }

        private void UpdateBackdrop()
        {
            if (_backdrop != null) _backdrop.SetActive(_activeStack.Count > 0);
        }

        private struct PopupEntry
        {
            public UIModuleBase Module;
            public PopupPriority Priority;
        }

        private struct PopupRequest
        {
            public System.Type Type;
            public object Data;
            public PopupPriority Priority;
        }
    }
}
