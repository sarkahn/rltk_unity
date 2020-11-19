using RLTK;
using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CameraToTerminalTest : MonoBehaviour
{
    SimpleTerminal _term;
    // Start is called before the first frame update
    void Start()
    {
        _term = new SimpleTerminal(10, 10, Allocator.Persistent);
        RenderUtility.AdjustCameraToTerminal(_term);
    }

    private void OnDestroy()
    {
        _term.Dispose();
    }

    private void Update()
    {
        RenderUtility.RenderTerminal(_term);
    }
}
