using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class DraggingControls : MonoBehaviour
{
    TerminalBehaviour _term;

    Controls _controls;
    InputAction _lmbDrag;

    bool _dragging = false;
    HashSet<int2> _draggedPoints = new HashSet<int2>();

    public System.Action<int2> OnPointDragged;
    public System.Action<int2> OnDragStarted;
    public System.Action<HashSet<int2>> OnDragEnded;

    private void OnEnable()
    {
        _controls = new Controls();
        _controls.Enable();

        _lmbDrag = _controls.Default.LMBDrag;
        _lmbDrag.performed += OnDragBegin;
        _lmbDrag.canceled += OnDragEnd;

        _term = GetComponent<TerminalBehaviour>();
    }

    void OnDragBegin(InputAction.CallbackContext ctx)
    {
        var p = GetMouseConsolePosition();
        if (!_term.IsInBounds(p))
            return;

        OnDragStarted?.Invoke(p);
        //_draggedPoints.Add(p);
        _dragging = true;
    }

    void OnDragEnd(InputAction.CallbackContext ctx)
    {
        _dragging = false;

        OnDragEnded?.Invoke(_draggedPoints);

        _draggedPoints.Clear();
    }

    void OnDrag()
    {
        int2 p = GetMouseConsolePosition();

        if(!_term.IsInBounds(p) || _draggedPoints.Contains(p))
          return;

        _draggedPoints.Add(p);
        OnPointDragged?.Invoke(p);
    }

    int2 GetMouseConsolePosition()
    {
        float2 mouseXY = Mouse.current.position.ReadValue();
        float3 mousePos = new float3(mouseXY, transform.position.z);

        float3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        int2 tileIndex = _term.WorldPosToTileIndex(worldPos);
        return tileIndex;
    }

    private void Update()
    {
        if (_dragging)
            OnDrag();
    }

}
