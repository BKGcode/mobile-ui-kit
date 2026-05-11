using System;
using KitforgeLabs.UIKit.Animation;

namespace KitforgeLabs.UIKit.Catalog.Tests.Helpers
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
