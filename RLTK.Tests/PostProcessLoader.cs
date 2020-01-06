using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experiemntal.Rendering.Universal;

public class PostProcessLoader : MonoBehaviour
{
    [SerializeField]
    Blit _blitPass;

    private void OnEnable()
    {
        if (_blitPass != null)
            _blitPass.settings.enabled = true;
    }

    private void OnDisable()
    {
        if (_blitPass != null)
            _blitPass.settings.enabled = false;
    }
}
