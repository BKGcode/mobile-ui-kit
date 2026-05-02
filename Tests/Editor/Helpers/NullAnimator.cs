using System;
using KitforgeLabs.MobileUIKit.Animation;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers
{
    internal sealed class NullAnimator : IUIAnimator
    {
        public bool IsPlaying => false;
        public void ApplyPreset(UIAnimPreset preset) { }
        public void PlayShow(Action onComplete = null) { onComplete?.Invoke(); }
        public void PlayHide(Action onComplete = null) { onComplete?.Invoke(); }
        public void Skip() { }
    }
}
