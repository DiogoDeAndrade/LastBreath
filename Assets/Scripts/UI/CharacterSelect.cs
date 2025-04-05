using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UC;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField, Scene] private string gameScene;
    [SerializeField] private AudioClip musicClip;

    Customizer[] customizers;

    void Start()
    {
        customizers = FindObjectsByType<Customizer>(FindObjectsSortMode.None);

        FullscreenFader.FadeIn(0.5f);

        if (musicClip) SoundManager.PlayMusic(musicClip);
    }

    void Update()
    {
        bool noActive = true;
        foreach (var customizer in customizers)
        {
            if (customizer == null) continue;
            if (customizer.uiEnable)
            {
                noActive = false;
                break;
            }
        }

        if (noActive)
        {
            FullscreenFader.FadeOut(0.5f, Color.black, () =>
            {
                SceneManager.LoadScene(gameScene);
            });
            enabled = false;
        }
    }
}
