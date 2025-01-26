using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class LevelManager : MonoBehaviour
{
    enum GameState { Ongoing, GameOver };

    [SerializeField] 
    private CanvasGroup        gameOverCanvas;
    [SerializeField] 
    private TextMeshProUGUI    gameOverText;
    [SerializeField, Scene] 
    private string titleScene;

    private City[]      cities;
    private GameState   state = GameState.Ongoing;
    private int         winnerId;

    void Start()
    {
        cities = FindObjectsByType<City>(FindObjectsSortMode.None);
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
}
