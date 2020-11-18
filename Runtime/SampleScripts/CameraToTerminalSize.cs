
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sark.RenderUtils;
using Sark.Terminals;

[ExecuteAlways]
[RequireComponent(typeof(TiledCamera))]
public class CameraToTerminalSize : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    TiledCamera _cam;

    [SerializeField]
    TerminalBehaviour _terminal;

    private void OnEnable()
    {
        if (_cam == null)
            _cam = GetComponent<TiledCamera>();

        if(_terminal == null)
        {
            if (_terminal == null)
                _terminal = FindObjectOfType<TerminalBehaviour>();
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        UpdateCamera();
    }
#endif

    void UpdateCamera()
    {
        if(_cam != null && _terminal != null)
            _cam.TileCount = _terminal.Size;
    }
}
