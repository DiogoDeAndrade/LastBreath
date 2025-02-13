using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public class PlayerData
    {
        public Color    hullColor;
        public Color    stripeColor;
        public Color    cockpitColor;
        public int      score;
    }

    [SerializeField] private int                _numPlayers = 2;
    [SerializeField] private List<PlayerData>   _playerData;
    [SerializeField] private Arena              _selectedLevel;

    private float playTime = 0.0f;

    static GameManager _Instance;

    public static GameManager Instance
    {
        get
        {
            if (_Instance == null) _Instance = FindFirstObjectByType<GameManager>();
            return _Instance;
        }
    }

    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public PlayerData GetPlayerData(int playerId)
    {
        if (_playerData == null) _playerData = new();

        for (int i = _playerData.Count; i <= playerId; i++)
        {
            _playerData.Add(new PlayerData());
        }

        return _playerData[playerId];
    }

    public void SetPlayerData(int playerId, PlayerData pd)
    {
        if (_playerData == null) _playerData = new();

        for (int i = _playerData.Count; i <= playerId; i++)
        {
            _playerData.Add(new PlayerData());
        }

        _playerData[playerId] = pd;
    }

    private void Update()
    {
        playTime += Time.deltaTime;
    }

    public int numPlayers
    {
        get { return _numPlayers; }
        set { _numPlayers = value; }
    }

    public Arena selectedLevel
    {
        get { return _selectedLevel; }
        set { _selectedLevel = value; }
    }
}
