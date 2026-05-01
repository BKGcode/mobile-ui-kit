using System;
using System.Collections;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Toast
{
    [DisallowMultipleComponent]
    public sealed class ToastManager : MonoBehaviour
    {
        [SerializeField] private UIThemeConfig _themeConfig;
        [SerializeField] private Transform _toastRoot;
        [SerializeField] private UIToastBase[] _toastPrefabs;
        [SerializeField] private int _maxConcurrent = 3;

        private readonly List<ActiveToast> _active = new();
        private readonly Queue<PendingToast> _pending = new();

        public UIThemeConfig Theme => _themeConfig;
        public int ActiveCount => _active.Count;

        public T Show<T>(object data = null, float? duration = null) where T : UIToastBase
        {
            var request = new PendingToast { Type = typeof(T), Data = data, Duration = duration };
            if (_active.Count >= _maxConcurrent)
            {
                _pending.Enqueue(request);
                return null;
            }
            return (T)ShowInternal(request);
        }

        public void DismissAll()
        {
            for (var i = _active.Count - 1; i >= 0; i--) DismissAt(i);
            _pending.Clear();
        }

        private UIToastBase ShowInternal(PendingToast request)
        {
            var prefab = FindPrefab(request.Type);
            if (prefab == null)
            {
                Debug.LogError($"[ToastManager] No prefab registered for {request.Type.Name}.", this);
                return null;
            }
            var instance = Instantiate(prefab, _toastRoot);
            if (request.Data != null) instance.BindUntyped(request.Data);
            instance.OnShow();
            var lifetime = request.Duration ?? instance.DefaultDuration;
            var routine = StartCoroutine(AutoDismiss(instance, lifetime));
            _active.Add(new ActiveToast { Instance = instance, Routine = routine });
            return instance;
        }

        private IEnumerator AutoDismiss(UIToastBase toast, float lifetime)
        {
            yield return new WaitForSecondsRealtime(lifetime);
            DismissInstance(toast);
        }

        private void DismissInstance(UIToastBase toast)
        {
            for (var i = 0; i < _active.Count; i++)
            {
                if (_active[i].Instance != toast) continue;
                DismissAt(i);
                break;
            }
            DrainPending();
        }

        private void DismissAt(int index)
        {
            var record = _active[index];
            if (record.Routine != null) StopCoroutine(record.Routine);
            if (record.Instance != null)
            {
                record.Instance.OnHide();
                Destroy(record.Instance.gameObject);
            }
            _active.RemoveAt(index);
        }

        private void DrainPending()
        {
            while (_pending.Count > 0 && _active.Count < _maxConcurrent)
            {
                ShowInternal(_pending.Dequeue());
            }
        }

        private UIToastBase FindPrefab(Type type)
        {
            if (_toastPrefabs == null) return null;
            for (var i = 0; i < _toastPrefabs.Length; i++)
            {
                if (_toastPrefabs[i] != null && _toastPrefabs[i].GetType() == type) return _toastPrefabs[i];
            }
            return null;
        }

        private void OnDestroy()
        {
            for (var i = 0; i < _active.Count; i++)
            {
                if (_active[i].Routine != null) StopCoroutine(_active[i].Routine);
                if (_active[i].Instance != null) _active[i].Instance.OnHide();
            }
            _active.Clear();
            _pending.Clear();
        }

        private struct ActiveToast
        {
            public UIToastBase Instance;
            public Coroutine Routine;
        }

        private struct PendingToast
        {
            public Type Type;
            public object Data;
            public float? Duration;
        }
    }
}
