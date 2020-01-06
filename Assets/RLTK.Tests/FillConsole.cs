using RLTK.Consoles;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class FillConsole : MonoBehaviour
{
    IConsole _console;
    

    private void Awake()
    {
        _console = GetComponent<IConsole>();
        if (_console == null)
            Debug.LogError("Couldn't find console", gameObject);
    }

    private void Start()
    {
        var tiles = _console.CopyTiles(Allocator.Temp);
        for( int i = 0; i < tiles.Length; ++i )
        {
            var t = tiles[i];
            t.fgColor = Random.ColorHSV(0, 1, .45f, 1f);
            t.bgColor = Random.ColorHSV(0, 1, .45f, 1f);
            t.glyph = (byte)Random.Range(0, 255);
            tiles[i] = t;
        }
        _console.WriteTiles(tiles);
    }
}
