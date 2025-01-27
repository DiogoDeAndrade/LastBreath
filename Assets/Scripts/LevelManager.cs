using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class LevelManager : MonoBehaviour
{
    enum GameState { Ongoing, GameOver };
    // Concept:
    // Phase 1: Cities have different requests, and conflict is kept to a minimum, no torpedos
    // Phase 2: Cities have the same request, no torpedos
    // Phase 3: Weapons free
    [SerializeField] 
    private CanvasGroup     gameOverCanvas;
    [SerializeField] 
    private TextMeshProUGUI gameOverText;
    [SerializeField, Scene] 
    private string titleScene;

    private City[]      cities;
    private GameState   state = GameState.Ongoing;
    private int         winnerId;
    private int         _phase;
    private float       matchDuration;

    private static LevelManager Instance;

    void Start()
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

        cities = FindObjectsByType<City>(FindObjectsSortMode.None);
        _phase = 0;
        matchDuration = 0;
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
