using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PauseSoundOnPause : MonoBehaviour
{
    AudioSource     audioSource;
    PauseMenu       pauseMenu;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        pauseMenu = FindFirstObjectByType<PauseMenu>();
        if (pauseMenu == null)
        {
            Destroy(gameObject);
        }
        else
        {
            pauseMenu.onPause += OnPause;
        }
    }

    void OnPause(bool pause)
    {
        if (pause) audioSource.Pause();
        else audioSource.UnPause();
    }
}
