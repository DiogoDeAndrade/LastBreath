using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using UnityEngine.Rendering.Universal;
using UC;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    enum GameState { Ongoing, GameOver };

    [SerializeField, Header("Environment")]
    private float           globalLight = 1.0f;
    [SerializeField]
    private float           cityLight = 0.0f;
    [SerializeField]
    private float           playerLight = 0.0f;
    [SerializeField, Header("Resources")]
    private float           _resourceConsumption = 1.0f;
    [SerializeField]
    private float           _resourceBonus  = 1.0f;
    [SerializeField, Header("Weapons")]
    private bool            _overrideWeapons = false;
    [SerializeField, ShowIf(nameof(_overrideWeapons))]
    private Weapon[]        _weapons;
    [SerializeField, Header("Game Flow")]
    private int             debugStartPhase = 0;
    [SerializeField]
    private PhaseData[]     phases;
    [SerializeField]
    private RectTransform   gameUI;
    [SerializeField]
    private PhaseDisplay    phaseDisplayPrefab;
    [SerializeField]
    private CanvasGroup     gameOverCanvas;
    [SerializeField] 
    private TextMeshProUGUI gameOverText;
    [SerializeField, Scene] 
    private string          titleScene;

    private City[]      cities;
    private GameState   state = GameState.Ongoing;
    private int         winnerId;
    private int         _phaseIndex;
    private PhaseData   _phase;
    private float       matchDuration;
    private float       timeSinceLastPhaseChange;

    private static LevelManager Instance;

    public static float cityLightIntensity => Instance.cityLight;
    public static float playerLightIntensity => Instance.playerLight;

    public static float resourceConsumption => Instance._resourceConsumption;
    public static float resourceBonus => Instance._resourceBonus;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        cities = FindObjectsByType<City>(FindObjectsSortMode.None);

#if UNITY_EDITOR
        StartPhase(debugStartPhase);
        if (phases[0].phaseTrigger != PhaseData.PhaseTrigger.Initial)
        {
            Debug.LogWarning("First phase needs to have trigger equal to PhaseData.PhaseTrigger.Initial!");
        }
#else
        StartPhase(0);
#endif
        matchDuration = 0;

        // Find global light
        var lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            if (light.lightType == Light2D.LightType.Global)
            {
                light.intensity = globalLight;
                if (globalLight <= 0.0f) light.enabled = false;
            }
        }
    }

    void Update()
    {
        if (state == GameState.GameOver)
        {
            // Check inputs
            if (winnerId == -1)
            {
                if (Input.anyKeyDown) OnContinue();
            }
            else
            {
                var players = FindObjectsByType<Submarine>(FindObjectsSortMode.None);
                foreach (var player in players)
                {
                    if ((player.GetAttackControl().IsDown()) ||
                        (player.GetGatherControl().IsDown()))
                    {
                        OnContinue();
                    }
                }
            }
        }
        else
        {
            // Phases
            matchDuration += Time.deltaTime;
            timeSinceLastPhaseChange += Time.deltaTime;

            if (_phaseIndex + 1 < phases.Length)
            {
                PhaseData nextPhase = phases[_phaseIndex + 1];

                bool triggerNext = false;
                switch (nextPhase.phaseTrigger)
                {
                    case PhaseData.PhaseTrigger.TimeSinceStart:
                        triggerNext = (matchDuration >= nextPhase.time);
                        break;
                    case PhaseData.PhaseTrigger.TimeSinceLastPhase:
                        triggerNext = (timeSinceLastPhaseChange >= nextPhase.time);
                        break;
                    default:
                        break;
                }

                if (triggerNext)
                {
                    _phaseIndex++;

                    StartPhase(_phaseIndex);
                }
            }

            winnerId = GetWinner();

            if (winnerId >= 0)
            {
                var playerData = GameManager.Instance.GetPlayerData(winnerId);

                gameOverText.text = $"PLAYER {winnerId + 1} WINS!";

                var submarines = FindObjectsByType<Submarine>(FindObjectsSortMode.None);
                foreach (var sub in submarines)
                {
                    if (sub.playerId == winnerId)
                    {
                        SubCustomization sc = sub.GetComponent<SubCustomization>();
                        if (sc)
                        {
                            gameOverText.color = sc.GetTextColor();
                        }
                    }
                }

                gameOverCanvas.FadeIn(0.5f);
                state = GameState.GameOver;
            }
            else if (winnerId == -1)
            {
                gameOverText.text = $"TIE!";
                gameOverCanvas.FadeIn(0.5f);
                state = GameState.GameOver;
            }
        }
    }

    void StartPhase(int phase)
    {
        _phaseIndex = phase;
        _phase = phases[_phaseIndex];
        timeSinceLastPhaseChange = 0;

        if (phaseDisplayPrefab)
        {
            Instantiate(phaseDisplayPrefab, gameUI);
        }
    }

    int GetWinner()
    {
        HashSet<int> alivePlayers = new HashSet<int>();

        foreach (var city in cities)
        {
            if (!city.isDead || !city.isPlayerDead)
            {
                alivePlayers.Add(city.playerId);
            }
        }

        if (alivePlayers.Count == 1)
        {
            // One winner
            foreach (int id in alivePlayers)
                return id;
        }
        else if (alivePlayers.Count == 0)
        {
            // Everyone is dead
            return -1;
        }

        // More than one alive
        return -2;
    }

    private void OnContinue()
    {
        // Pressed interact, next screen
        if (state == GameState.GameOver)
        {
            FullscreenFader.FadeOut(0.5f, Color.black, () =>
            {
                SceneManager.LoadScene(titleScene);
            });
        }
    }

    // When we sort a request, should we clear all requests on all cities?
    public static bool shouldCancelAllRequests => Instance._phase.cancelOpponentRequestsOnRequestCompletion;
    // Should we force competition or not (divide requests by two)
    public static bool shouldCompeteForResources => Instance._phase.forceRequestCompetition;
    // Can we shoot
    public static bool weaponsFree => Instance._phase.weaponsFree;
    // What's the phase?
    public static PhaseData phase => Instance._phase;
    // Override weapons
    public static bool overrideWeapons => Instance._overrideWeapons;
    public static Weapon[]  overrideWeaponPrefabs => Instance._weapons;
}
