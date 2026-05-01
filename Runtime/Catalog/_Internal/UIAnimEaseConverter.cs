using DG.Tweening;
using KitforgeLabs.MobileUIKit.Animation;

namespace KitforgeLabs.MobileUIKit.Catalog.Internal
{
    internal static class UIAnimEaseConverter
    {
        public static Ease ToDoTween(this UIAnimEase ease)
        {
            switch (ease)
            {
                case UIAnimEase.Linear:     return Ease.Linear;
                case UIAnimEase.InQuad:     return Ease.InQuad;
                case UIAnimEase.OutQuad:    return Ease.OutQuad;
                case UIAnimEase.InOutQuad:  return Ease.InOutQuad;
                case UIAnimEase.InCubic:    return Ease.InCubic;
                case UIAnimEase.OutCubic:   return Ease.OutCubic;
                case UIAnimEase.InOutCubic: return Ease.InOutCubic;
                case UIAnimEase.InSine:     return Ease.InSine;
                case UIAnimEase.OutSine:    return Ease.OutSine;
                case UIAnimEase.InOutSine:  return Ease.InOutSine;
                case UIAnimEase.InBack:     return Ease.InBack;
                case UIAnimEase.OutBack:    return Ease.OutBack;
                case UIAnimEase.InOutBack:  return Ease.InOutBack;
                case UIAnimEase.InElastic:  return Ease.InElastic;
                case UIAnimEase.OutElastic: return Ease.OutElastic;
                case UIAnimEase.InBounce:   return Ease.InBounce;
                case UIAnimEase.OutBounce:  return Ease.OutBounce;
                default:                    return Ease.OutQuad;
            }
        }
    }
}
