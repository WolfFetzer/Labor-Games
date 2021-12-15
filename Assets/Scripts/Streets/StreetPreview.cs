using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetPreview : MonoBehaviour
{
    [SerializeField] private Material bluePreviewMaterial;
    [SerializeField] private Material redPreviewMaterial;

    private MeshRenderer _renderer;
    
    private bool _canBePlaced;
    public bool CanBePlaced
    {
        get => _canBePlaced;
        set
        {
            if (value != _canBePlaced)
            {
                _canBePlaced = value;

                if (_canBePlaced) _renderer.sharedMaterial = bluePreviewMaterial;
                else _renderer.sharedMaterial = redPreviewMaterial;
            }
        }
    }

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
    }
}
