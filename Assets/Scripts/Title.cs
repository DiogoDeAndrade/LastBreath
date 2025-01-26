using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Title : UIGroup
{
    [SerializeField] UIButton playButton;
    [SerializeField] UIButton creditsButton;
    [SerializeField] UIButton quitButton;
    [SerializeField, Scene] string characterSelectScene;
    [SerializeField] CanvasGroup mainMenuCanvas;
    [SerializeField] BigTextScroll creditsScroll;
    [SerializeField] AudioClip titleMusic;

    protected override void Start()
    {
        base.Start();

        Cursor.visible = false;

        playButton.onInteract += StartGame;
        creditsButton.onInteract += ShowCredits;
        quitButton.onInteract += QuitGame;

        if (titleMusic) SoundManager.PlayMusic(titleMusic);
    }

    private void ShowCredits(BaseUIControl control)
    {
        _uiEnable = false;

        mainMenuCanvas.FadeOut(0.5f);

        var canvasGroup = creditsScroll.GetComponent<CanvasGroup>();
        canvasGroup.FadeIn(0.5f);

        creditsScroll.Reset();

        creditsScroll.onEndScroll += BackToMenu;
    }

    private void BackToMenu()
    {
        mainMenuCanvas.FadeIn(0.5f);

        var canvasGroup = creditsScroll.GetComponent<CanvasGroup>();
        canvasGroup.FadeOut(0.5f);

        _uiEnable = true;
        selectedControl = playButton;

        creditsScroll.onEndScroll -= BackToMenu;
    }

    private void StartGame(BaseUIControl control)
    {
        _uiEnable = false;
        GameManager.Instance.numPlayers = 2;
        
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            SceneManager.LoadScene(characterSelectScene);
        });
    }

    private void QuitGame(BaseUIControl control)
    {
        _uiEnable = false;
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
#else
            Application.Quit();
#endif
        });
    }

}
