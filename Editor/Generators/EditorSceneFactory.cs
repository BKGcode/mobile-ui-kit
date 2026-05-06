using UnityEngine;
using UnityEngine.SceneManagement;

namespace KitforgeLabs.MobileUIKit.Editor.Generators
{
    internal static class EditorSceneFactory
    {
        internal static GameObject CreateMainCamera(Scene scene)
        {
            var go = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.19f, 0.30f, 0.47f);
            camera.orthographic = false;
            go.transform.position = new Vector3(0f, 1f, -10f);
            go.AddComponent<AudioListener>();
            return go;
        }
    }
}
