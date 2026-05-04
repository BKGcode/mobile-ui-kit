using System;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupC
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class InMemoryTimeService : MonoBehaviour, ITimeService
    {
        [Tooltip("Optional offset (seconds) added to DateTime.UtcNow. Default 0 = real UTC clock. Editor-only ContextMenu helpers nudge this for QA.")]
        [SerializeField] private double _offsetSeconds;

        public DateTime GetServerTimeUtc() => DateTime.UtcNow.AddSeconds(_offsetSeconds);

        public TimeSpan GetTimeUntil(DateTime utcTarget) => utcTarget - GetServerTimeUtc();

        public bool HasElapsed(DateTime utcSince, TimeSpan duration) => GetServerTimeUtc() - utcSince >= duration;

        [ContextMenu("Debug — Skip 1 hour forward")]
        private void DebugSkipHour() => _offsetSeconds += 3600;

        [ContextMenu("Debug — Skip 1 day forward")]
        private void DebugSkipDay() => _offsetSeconds += 86400;

        [ContextMenu("Debug — Reset offset to 0")]
        private void DebugReset() => _offsetSeconds = 0;
    }
}
