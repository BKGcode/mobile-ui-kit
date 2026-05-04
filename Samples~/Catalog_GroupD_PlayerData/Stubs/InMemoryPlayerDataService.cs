using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupD
{
    /// <summary>
    /// In-memory <see cref="IPlayerDataService"/> sample stub. Dictionary-backed; <c>Save()</c>
    /// is a no-op, <c>Reload()</c> clears the cache (simulates "fresh app launch"). Useful for
    /// repeated-Play demo scenarios where buyer wants stateless behavior.
    /// <para>
    /// For real cross-session persistence in a demo, wire <c>PlayerPrefsPlayerDataService</c>
    /// (Runtime, ships with the kit) instead — the default Group D builder does this.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class InMemoryPlayerDataService : MonoBehaviour, IPlayerDataService
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public int GetInt(string key, int defaultValue = 0) => Read(key, defaultValue);
        public void SetInt(string key, int value) => Write(key, value);
        public float GetFloat(string key, float defaultValue = 0f) => Read(key, defaultValue);
        public void SetFloat(string key, float value) => Write(key, value);
        public string GetString(string key, string defaultValue = "") => Read(key, defaultValue);
        public void SetString(string key, string value) => Write(key, value);
        public bool GetBool(string key, bool defaultValue = false) => Read(key, defaultValue);
        public void SetBool(string key, bool value) => Write(key, value);

        public bool Has(string key)
        {
            ValidateKey(key);
            return _values.ContainsKey(key);
        }

        public void Delete(string key)
        {
            ValidateKey(key);
            _values.Remove(key);
        }

        public void Save() { }

        public void Reload() => _values.Clear();

        [ContextMenu("Debug — Dump all keys")]
        private void DumpKeys()
        {
            foreach (var kvp in _values) Debug.Log($"[InMemoryPlayerData] {kvp.Key} = {kvp.Value}", this);
        }

        [ContextMenu("Debug — Clear all keys")]
        private void ClearAll() => Reload();

        private T Read<T>(string key, T defaultValue)
        {
            ValidateKey(key);
            if (!_values.TryGetValue(key, out var raw)) return defaultValue;
            if (raw is T typed) return typed;
            throw new InvalidCastException($"Key '{key}' stored as {raw?.GetType().Name ?? "null"} but read as {typeof(T).Name}.");
        }

        private void Write<T>(string key, T value)
        {
            ValidateKey(key);
            _values[key] = value;
        }

        private static void ValidateKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
        }
    }
}
