using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Inspectors
{
    internal static class AudioPreviewBridge
    {
        private static MethodInfo _play;
        private static MethodInfo _stopAll;

        public static void Play(AudioClip clip)
        {
            if (clip == null) return;
            EnsureMethods();
            _play?.Invoke(null, new object[] { clip, 0, false });
        }

        public static void StopAll()
        {
            EnsureMethods();
            _stopAll?.Invoke(null, null);
        }

        private static void EnsureMethods()
        {
            if (_play != null && _stopAll != null) return;
            var assembly = typeof(AudioImporter).Assembly;
            var audioUtil = assembly.GetType("UnityEditor.AudioUtil");
            if (audioUtil == null) return;
            _play = audioUtil.GetMethod("PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null);
            _stopAll = audioUtil.GetMethod("StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public);
        }
    }
}
