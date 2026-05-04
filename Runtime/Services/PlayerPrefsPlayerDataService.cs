using System;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Services
{
    [DisallowMultipleComponent]
    public sealed class PlayerPrefsPlayerDataService : MonoBehaviour, IPlayerDataService
    {
        private static bool _emptyKeyWarned;

        public int GetInt(string key, int defaultValue = 0)
        {
            ValidateKey(key);
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            ValidateKey(key);
            PlayerPrefs.SetInt(key, value);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            ValidateKey(key);
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            ValidateKey(key);
            PlayerPrefs.SetFloat(key, value);
        }

        public string GetString(string key, string defaultValue = "")
        {
            ValidateKey(key);
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            ValidateKey(key);
            PlayerPrefs.SetString(key, value);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            ValidateKey(key);
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        public void SetBool(string key, bool value)
        {
            ValidateKey(key);
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public bool Has(string key)
        {
            ValidateKey(key);
            return PlayerPrefs.HasKey(key);
        }

        public void Delete(string key)
        {
            ValidateKey(key);
            PlayerPrefs.DeleteKey(key);
        }

        public void Save() => PlayerPrefs.Save();

        public void Reload() { }

        private static void ValidateKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length != 0 || _emptyKeyWarned) return;
            Debug.LogWarning("[PlayerPrefsPlayerDataService] Empty key passed. Allowed but discouraged. Warning shows once per session.");
            _emptyKeyWarned = true;
        }
    }
}
