using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public static class SnapshotValidation
    {
        public static bool LooksRendered(byte[] pngBytes, out string diagnostic)
        {
            diagnostic = string.Empty;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!tex.LoadImage(pngBytes)) { diagnostic = "PNG bytes did not decode into a Texture2D"; return false; }
                var pixels = tex.GetPixels32();
                return EvaluatePixels(pixels, out diagnostic);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        private static bool EvaluatePixels(Color32[] pixels, out string diagnostic)
        {
            diagnostic = string.Empty;
            if (pixels == null || pixels.Length == 0) { diagnostic = "Pixel buffer empty"; return false; }
            if (FindFirstNonTransparent(pixels) < 0) { diagnostic = "Snapshot is fully transparent — camera or canvas not configured correctly"; return false; }
            if (IsUniformColor(pixels)) { diagnostic = "Snapshot is uniform color — prefab may not have rendered any visible elements"; return false; }
            return true;
        }

        private static int FindFirstNonTransparent(Color32[] pixels)
        {
            for (var i = 0; i < pixels.Length; i++) if (pixels[i].a != 0) return i;
            return -1;
        }

        private static bool IsUniformColor(Color32[] pixels)
        {
            var first = pixels[0];
            for (var i = 1; i < pixels.Length; i++) if (!ColorEqual(pixels[i], first)) return false;
            return true;
        }

        private static bool ColorEqual(Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }
    }
}
