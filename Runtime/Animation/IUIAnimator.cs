using System;

namespace KitforgeLabs.UIKit.Animation
{
    public interface IUIAnimator
    {
        bool IsPlaying { get; }
        void PlayShow(Action onComplete = null);
        void PlayHide(Action onComplete = null);
        void Skip();
        void ApplyPreset(UIAnimPreset preset);
    }
}
