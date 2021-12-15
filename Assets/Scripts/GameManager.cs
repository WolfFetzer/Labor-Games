using GameStats;
using PlayerInput;
using UnityEngine;
using Util;

public class GameManager : Singleton<GameManager>
{
    public GameObject prefab;
    public GameObject streetCursorPrefab;
    public GameObject intersectionPrefab;
    public GameObject carPrefab;

    public Material streetMaterial;

    private GameMode _gameMode;

    public GameMode GameMode
    {
        get => _gameMode;
        set
        {
            if (value == _gameMode) return;
            _gameMode = value;
            Debug.Log(_gameMode.ToString());

            if (_gameMode == GameMode.DefaultMode)
                InputReceiver = new DefaultInputReceiver();
            else
                InputReceiver = new ConstructionInputReceiver();
        }
    }

    public IPlayerInputReceiver InputReceiver { get; private set; } = new DefaultInputReceiver();
    public CityStats CityStats { get; } = new CityStats {citizens = 1000, money = 1000000};
}

public enum GameMode
{
    DefaultMode,
    ConstructionMode
}