using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using UnityEngine.Rendering.Universal;
using System.Runtime.CompilerServices;

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
    [SerializeField, Header("Game Flow")]
    private int             startPhase = 0;
    [SerializeField]
    private CanvasGroup     gameOverCanvas;
    [SerializeField] 
    private TextMeshProUGUI gameOverText;
    [SerializeField, Scene] 
    private string          titleScene;

    private City[]      cities;
    private GameState   state = GameState.Ongoing;
    private int         winnerId;
    private int         _phase;
    private float       matchDuration;

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
        _phase = startPhase;
#else
        _phase = 0;
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

            switch (_phase)
            {
                case 0: if (matchDuration > 30.0f) _phase = 1; break;
                case 1: if (matchDuration > 75.0f) _phase = 2; break;
                case 2: break;
            }

            // Win condition
            int alive = 0;
            foreach (var city in cities)
            {
                if (!city.isDead) continue;
                if (!city.isPlayerDead) continue;

                alive++;
            }

            // Only one alive
            if (alive == 1)
            { 
                winnerId = GetWinner();
                if (winnerId != -1)
                {
                    var playerData = GameManager.Instance.GetPlayerData(winnerId);

                    gameOverText.text = $"PLAYER {winnerId + 1} WINS!";
                    gameOverText.color = playerData.hullColor;
                }
                else
                {
                    gameOverText.text = $"TIE!";
                }
                gameOverCanvas.FadeIn(0.5f);
                state = GameState.GameOver;
            }
        }
    }

    int GetWinner()
    {
        foreach (var city in cities)
        {
            if (!city.isDead) return city.playerId;
        }

        foreach (var city in cities)
        {
            if (!city.isPlayerDead) return city.playerId;
        }

        return -1;
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
    public static bool shouldCancelAllRequests => Instance._phase > 0;
    // Should we force competition or not (divide requests by two)
    public static bool shouldCompeteForResources => Instance._phase > 0;
    // Can we shoot
    public static bool weaponsFree => Instance._phase > 1;
    // What's the phase?
    public static int phase => Instance._phase;
}
