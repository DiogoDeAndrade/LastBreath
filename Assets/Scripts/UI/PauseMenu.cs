using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UC;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseMenu : UIGroup
{
    [SerializeField] CanvasGroup    mainMenuCanvas;
    [SerializeField] UIButton       resumeButton;
    [SerializeField] UIGroup        optionsGroup;
    [SerializeField] UIButton       returnToMainMenuButton;
    [SerializeField, Scene] string  mainMenuScene;

    public bool isPaused => mainMenuCanvas.alpha > 0;

    public delegate void OnPause(bool pause);
    public event OnPause onPause;

    protected override void Start()
    {
        base.Start();

        Cursor.visible = false;
        mainMenuCanvas.alpha = 0.0f;

        resumeButton.onInteract += ResumeGame;
        returnToMainMenuButton.onInteract += ReturnToMainMenu;
    }

    private void ResumeGame(BaseUIControl control)
    {
        Unpause(null);
    }

    private void ReturnToMainMenu(BaseUIControl control)
    {
        Unpause(null);

        FullscreenFader.FadeOut(0.15f, Color.black, () =>
        {
            SceneManager.LoadScene(mainMenuScene);
        });
    }

    public void Pause(PlayerInput playerThatTriggeredPause)
    {
        _uiEnable = true;

        SetPlayerInput(playerThatTriggeredPause);
        optionsGroup.SetPlayerInput(playerThatTriggeredPause);

        selectedControl = resumeButton;

        SoundManager.PauseAll();

        mainMenuCanvas.FadeIn(0.15f).Done(() =>
        {
            Time.timeScale = 0.0f;
        });

        onPause?.Invoke(true);
    }

    public void Unpause(PlayerInput playerThatTriggeredPause)
    {
        if ((playerThatTriggeredPause == null) || (playerThatTriggeredPause == playerInput))
        {
            _uiEnable = false;

            Time.timeScale = 1.0f;

            mainMenuCanvas.FadeOut(0.15f);

            SoundManager.UnpauseAll();

            onPause?.Invoke(false);
        }
    }
}
