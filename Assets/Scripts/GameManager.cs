using System;
using PlayerInput;
using UnityEngine;
using Util;

public class GameManager : Singleton<GameManager>
{
    public Transform streetTransform;
    public Transform intersectionTransform;
    
    public GameObject prefab;
    public GameObject streetCursorPrefab;
    public GameObject intersectionPrefab;
    public GameObject carPrefab;

    public GameObject buildingAreaPrefab;
    public GameObject residentialPrefab;
    public GameObject commercialPrefab;
    public GameObject industrialPrefab;

    public Material streetMaterial;

    private GameMode _mode;
    public GameMode GameMode
    {
        get => _mode;
        set
        {
            if (_mode == value) return;
            _mode = value;
            MeshRenderer r;
            switch (_mode)
            {
                case GameMode.AreaMode:
                    r = buildingAreaPrefab.GetComponent<MeshRenderer>();
                    r.sharedMaterial.color = new Color(1f, 1f, 1f, 1f);
                    break;

                default:
                    r = buildingAreaPrefab.GetComponent<MeshRenderer>();
                    r.sharedMaterial.color = new Color(0f, 0f, 0f, 0f);
                    break;
            }
        }
    }
}

public enum GameMode
{
    DefaultMode,
    ConstructionMode,
    AreaMode,
    UI
}