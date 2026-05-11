using System;
using System.Collections.Generic;
using KitforgeLabs.UIKit.Services;

namespace KitforgeLabs.UIKit.Catalog.Tests.Helpers
{
    internal sealed class FakePlayerDataService : IPlayerDataService
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        internal int SaveCallCount { get; private set; }
        internal int ReloadCallCount { get; private set; }

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

        public void Save() => SaveCallCount++;

        public void Reload()
        {
            ReloadCallCount++;
            _values.Clear();
        }

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
