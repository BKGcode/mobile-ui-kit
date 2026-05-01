using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Core
{
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField] private UIThemeConfig _themeConfig;
        [SerializeField] private Transform _screenRoot;
        [SerializeField] private UIModuleBase[] _screenPrefabs;

        private readonly Dictionary<System.Type, UIModuleBase> _screenCache = new();
        private readonly Stack<UIModuleBase> _screenStack = new();

        public UIThemeConfig Theme => _themeConfig;
        public UIModuleBase Current => _screenStack.Count > 0 ? _screenStack.Peek() : null;

        private void Awake()
        {
            if (_themeConfig == null)
            {
                Debug.LogError($"[UIManager] ThemeConfig is null on '{name}'. Assign it in the Inspector.", this);
            }
        }

        public T Push<T>(object data = null) where T : UIModuleBase
        {
            var screen = ResolveScreen<T>();
            if (screen == null) return null;
            if (Current != null) Current.gameObject.SetActive(false);
            _screenStack.Push(screen);
            ActivateScreen(screen, data);
            return screen;
        }

        public void Pop()
        {
            if (_screenStack.Count == 0) return;
            var top = _screenStack.Pop();
            top.OnHide();
            top.gameObject.SetActive(false);
            if (Current != null) Current.gameObject.SetActive(true);
        }

        public T Replace<T>(object data = null) where T : UIModuleBase
        {
            var incoming = ResolveScreen<T>();
            if (incoming == null) return null;
            if (_screenStack.Count > 0)
            {
                var top = _screenStack.Pop();
                top.OnHide();
                top.gameObject.SetActive(false);
            }
            _screenStack.Push(incoming);
            ActivateScreen(incoming, data);
            return incoming;
        }

        public void PopToRoot()
        {
            while (_screenStack.Count > 1) Pop();
        }

        private T ResolveScreen<T>() where T : UIModuleBase
        {
            var type = typeof(T);
            if (_screenCache.TryGetValue(type, out var cached)) return (T)cached;
            var prefab = FindPrefab(type);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] No prefab registered for {type.Name}.", this);
                return null;
            }
            var instance = Instantiate(prefab, _screenRoot);
            instance.gameObject.SetActive(false);
            _screenCache[type] = instance;
            return (T)instance;
        }

        private UIModuleBase FindPrefab(System.Type type)
        {
            if (_screenPrefabs == null) return null;
            for (var i = 0; i < _screenPrefabs.Length; i++)
            {
                if (_screenPrefabs[i] != null && _screenPrefabs[i].GetType() == type) return _screenPrefabs[i];
            }
            return null;
        }

        private void ActivateScreen(UIModuleBase screen, object data)
        {
            screen.gameObject.SetActive(true);
            if (data != null) screen.BindUntyped(data);
            screen.OnShow();
        }

        private void OnDestroy()
        {
            while (_screenStack.Count > 0)
            {
                var top = _screenStack.Pop();
                if (top != null) top.OnHide();
            }
            _screenCache.Clear();
        }

        [ContextMenu("Validate Registry")]
        private void ValidateRegistry()
        {
            if (_screenPrefabs == null || _screenPrefabs.Length == 0)
            {
                Debug.LogWarning($"[UIManager] Registry on '{name}' is empty.", this);
                return;
            }
            var seen = new HashSet<System.Type>();
            for (var i = 0; i < _screenPrefabs.Length; i++)
            {
                if (_screenPrefabs[i] == null) { Debug.LogWarning($"[UIManager] Slot {i} is null.", this); continue; }
                if (!seen.Add(_screenPrefabs[i].GetType()))
                {
                    Debug.LogError($"[UIManager] Duplicate type {_screenPrefabs[i].GetType().Name} at slot {i}.", this);
                }
            }
            Debug.Log($"[UIManager] Registry OK ({seen.Count} unique screens).", this);
        }
    }
}
