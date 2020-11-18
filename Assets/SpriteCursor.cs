using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpriteCursor : MonoBehaviour
{
    [SerializeField]
    TerminalBehaviour _term;

    [SerializeField]
    Transform _cursor = null;

    // Update is called once per frame
    void Update()
    {
        var mouse = Mouse.current.position.ReadValue();
        float3 mouseWorld = Camera.main.ScreenToWorldPoint(new float3(mouse, 0));
        mouseWorld.z = -1;

        mouseWorld.xy = math.floor(mouseWorld.xy);

        _cursor.position = mouseWorld;
    }
}
