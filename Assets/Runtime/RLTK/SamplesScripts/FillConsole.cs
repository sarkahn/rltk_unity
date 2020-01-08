using RLTK.Consoles;
using Unity.Collections;
using UnityEngine;

namespace RLTK.Samples
{
    public class FillConsole : MonoBehaviour
    {
        IConsole _console;

        public bool _randomFGColors = false;
        public Color _fgColor = Color.white;

        public bool _randomBGColors = false;
        public Color _bgColor = Color.black;

        private bool _update;

        private void Awake()
        {
            _console = GetComponent<IConsole>();
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
        }

        void Fill()
        {
            var tiles = _console.CopyTiles(Allocator.Temp);
            for (int i = 0; i < tiles.Length; ++i)
            {
                var t = tiles[i];
                t.fgColor = _randomFGColors ? Random.ColorHSV(0, 1) : _fgColor;
                t.bgColor = _randomBGColors ? Random.ColorHSV(0, 1) : _bgColor;
                t.glyph = (byte)Random.Range(0, 255);
                tiles[i] = t;
            }
            _console.WriteTiles(tiles);
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
                _update = true;
        }
    }
}