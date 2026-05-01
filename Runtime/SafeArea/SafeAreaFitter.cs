using UnityEngine;

namespace KitforgeLabs.MobileUIKit.SafeArea
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private bool _applyTop = true;
        [SerializeField] private bool _applyBottom = true;
        [SerializeField] private bool _applyLeft = true;
        [SerializeField] private bool _applyRight = true;

        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastResolution;

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            var area = Screen.safeArea;
            var resolution = new Vector2Int(Screen.width, Screen.height);
            if (area == _lastSafeArea && resolution == _lastResolution) return;
            Apply();
        }

        private void Apply()
        {
            if (_rect == null) _rect = (RectTransform)transform;
            var area = Screen.safeArea;
            var resolution = new Vector2(Screen.width, Screen.height);
            if (resolution.x <= 0f || resolution.y <= 0f) return;

            var anchorMin = area.position;
            var anchorMax = area.position + area.size;
            anchorMin.x /= resolution.x;
            anchorMin.y /= resolution.y;
            anchorMax.x /= resolution.x;
            anchorMax.y /= resolution.y;

            if (!_applyLeft) anchorMin.x = 0f;
            if (!_applyRight) anchorMax.x = 1f;
            if (!_applyBottom) anchorMin.y = 0f;
            if (!_applyTop) anchorMax.y = 1f;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;

            _lastSafeArea = area;
            _lastResolution = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
