using Sark.RenderUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RuntimeResizeTest : MonoBehaviour
{
    TiledCamera _cam;

    Controls _controls;
    InputAction _resize;

    private void Awake()
    {
        _cam = GetComponent<TiledCamera>();
        _controls = new Controls();
        _controls.Enable();

        _resize = _controls.Default.ResizeMap;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 resize = _resize.ReadValue<Vector2>();
        int hor = (int)resize.x;
        int ver = (int)resize.y;

        var count = _cam.TileCount;
        count.x += hor;
        count.y += ver;
        _cam.TileCount = count;
    }
}
