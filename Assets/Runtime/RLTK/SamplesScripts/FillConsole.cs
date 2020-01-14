using RLTK.Consoles;
using RLTK.MonoBehaviours;
using Unity.Collections;
using UnityEngine;

namespace RLTK.Samples
{
    public class FillConsole : MonoBehaviour
    {
        SimpleConsoleProxy _console;

        public bool _randomFGColors = false;
        public Color _fgColor = Color.white;

        public bool _randomBGColors = false;
        public Color _bgColor = Color.black;

        private bool _update;

        private void Awake()
        {
            _console = GetComponent<SimpleConsoleProxy>();
            if (_console == null)
                Debug.LogError("Couldn't find console", gameObject);
        }

        private void Start()
        {
            _update = true;
        }

        private void Update()
        {
            if (_update)
            {
                Fill();
                _update = false;
            }

            _console.Update();
        }

        void Fill()
        {
            var tiles = _console.ReadAllTiles(Allocator.Temp);
            for (int i = 0; i < tiles.Length; ++i)
            {
                var t = tiles[i];
                t.fgColor = _randomFGColors ? Random.ColorHSV(0, 1) : _fgColor;
                t.bgColor = _randomBGColors ? Random.ColorHSV(0, 1) : _bgColor;
                t.glyph = (byte)Random.Range(0, 255);
                tiles[i] = t;
            }
            _console.WriteAllTiles(tiles);
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
                _update = true;
        }
    }
}