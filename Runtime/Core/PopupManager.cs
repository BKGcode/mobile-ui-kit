using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Core
{
    [DisallowMultipleComponent]
    public sealed class PopupManager : MonoBehaviour
    {
        private const int MaxDepth = 3;

        [SerializeField] private Transform _popupRoot;
        [SerializeField] private GameObject _backdrop;
        [SerializeField] private UIModuleBase[] _popupPrefabs;

        private readonly Dictionary<Type, UIModuleBase> _popupCache = new();
        private readonly List<PopupRecord> _activeStack = new();
        private readonly List<PopupRecord> _pendingQueue = new();

        public int ActiveCount => _activeStack.Count;
        public UIModuleBase HighestPriorityPopup => _activeStack.Count > 0 ? _activeStack[_activeStack.Count - 1].Module : null;

        public T Show<T>(object data = null, PopupPriority priority = PopupPriority.Gameplay) where T : UIModuleBase
        {
            var request = new PopupRecord { Type = typeof(T), Data = data, Priority = priority };
            if (_activeStack.Count >= MaxDepth && !TryEvictLowerPriority(priority))
            {
                EnqueuePending(request);
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
            var topMost = _activeStack[_activeStack.Count - 1].Module;
            topMost.OnBackPressed();
            return true;
        }

        private UIModuleBase ShowInternal(PopupRecord request)
        {
            var popup = ResolvePopup(request.Type);
            if (popup == null) return null;
            request.Module = popup;
            InsertActiveByPriority(request);
            popup.gameObject.SetActive(true);
            if (request.Data != null) popup.BindUntyped(request.Data);
            popup.OnShow();
            UpdateBackdrop();
            return popup;
        }

        private void InsertActiveByPriority(PopupRecord entry)
        {
            var insertIndex = _activeStack.Count;
            for (var i = 0; i < _activeStack.Count; i++)
            {
                if (entry.Priority < _activeStack[i].Priority) { insertIndex = i; break; }
            }
            _activeStack.Insert(insertIndex, entry);
            ApplySiblingOrder();
        }

        private void ApplySiblingOrder()
        {
            for (var i = 0; i < _activeStack.Count; i++)
            {
                _activeStack[i].Module.transform.SetSiblingIndex(i);
            }
        }

        private bool TryEvictLowerPriority(PopupPriority incoming)
        {
            var lowestIndex = -1;
            var lowest = incoming;
            for (var i = 0; i < _activeStack.Count; i++)
            {
                if (_activeStack[i].Priority < lowest)
                {
                    lowest = _activeStack[i].Priority;
                    lowestIndex = i;
                }
            }
            if (lowestIndex < 0) return false;
            var evicted = _activeStack[lowestIndex];
            _activeStack.RemoveAt(lowestIndex);
            HidePopup(evicted.Module);
            EnqueuePending(new PopupRecord { Type = evicted.Type, Data = evicted.Data, Priority = evicted.Priority });
            return true;
        }

        private void EnqueuePending(PopupRecord request)
        {
            var insertIndex = _pendingQueue.Count;
            for (var i = 0; i < _pendingQueue.Count; i++)
            {
                if (request.Priority > _pendingQueue[i].Priority) { insertIndex = i; break; }
            }
            _pendingQueue.Insert(insertIndex, request);
        }

        private void DrainQueue()
        {
            while (_pendingQueue.Count > 0 && _activeStack.Count < MaxDepth)
            {
                var next = _pendingQueue[0];
                _pendingQueue.RemoveAt(0);
                ShowInternal(next);
            }
        }

        private UIModuleBase ResolvePopup(Type type)
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

        private UIModuleBase FindPrefab(Type type)
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

        private void OnDestroy()
        {
            for (var i = 0; i < _activeStack.Count; i++)
            {
                if (_activeStack[i].Module != null) _activeStack[i].Module.OnHide();
            }
            _activeStack.Clear();
            _pendingQueue.Clear();
            _popupCache.Clear();
        }

        private struct PopupRecord
        {
            public Type Type;
            public UIModuleBase Module;
            public object Data;
            public PopupPriority Priority;
        }
    }
}
