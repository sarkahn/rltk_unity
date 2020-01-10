
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

using RLTK.MonoBehaviours;

namespace RLTK.EditorNS.Menu
{
    public class Create : MonoBehaviour
    {
        [MenuItem("GameObject/RLTK/Initialize Simple Console", false, 10)]
        public static void SimpleConsole(MenuCommand command)
        {
            GameObject camGO;

            var cam = Find<Camera>();

            if (cam == null)
                cam = CreateCamera();

            camGO = cam.gameObject;

            var pixelCamera = GetOrCreate<PixelPerfectCamera>(camGO);
            var cameraLock = GetOrCreate<LockCameraToConsole>(camGO);

            var console = Find<SimpleConsoleProxy>();
            if (console == null)
                console = CreateConsole();

            console.Resize(40, 15);

            cameraLock.SetTarget(console);

            Selection.activeGameObject = console.gameObject;
        }


        static T GetOrCreate<T>(GameObject go) where T : Behaviour
        {
            var t = go.GetComponent<T>();
            if (t == null)
            {
                t = Undo.AddComponent<T>(go);
            }
            return t;
        }

        static T Find<T>(string tagToSearch = default) where T : Behaviour
        {
            T t = default;

            if (t == default)
            {
                if (!string.IsNullOrEmpty(tagToSearch))
                {
                    var taggedGO = GameObject.FindWithTag(tagToSearch);
                    if (taggedGO != null)
                        t = taggedGO.GetComponent<T>();
                }

                if (t == default)
                {
                    t = FindObjectOfType<T>();
                    if (t == default)
                        return default;
                }
            }

            return t as T;
        }

        static Camera CreateCamera()
        {
            var camGO = new GameObject("Camera", typeof(Camera));
            Undo.RegisterCreatedObjectUndo(camGO, $"Initialize Camera");
            var cam = camGO.GetComponent<Camera>();
            cam.transform.position = new Vector3(0, 0, -10);
            cam.orthographic = true;
            return cam;
        }

        static SimpleConsoleProxy CreateConsole()
        {
            var go = new GameObject("Console", typeof(SimpleConsoleProxy));
            var console = go.GetComponent<SimpleConsoleProxy>();

            return console;
        }
    }

}