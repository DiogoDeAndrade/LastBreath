using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UC;

public class ArenaSelect : UIGroup
{
    [SerializeField] private AudioClip      musicClip;
    [SerializeField] private ArenaDisplay   arenaDisplayPrefab;
    [SerializeField] private ArenaData[]        arenas;

    List<ArenaDisplay> arenaDisplays;

    protected override void Start()
    {
        base.Start();

        GameManager.Instance.selectedLevel = null;

        FullscreenFader.FadeIn(0.5f);

        if (musicClip) SoundManager.PlayMusic(musicClip);

        int nPlayers = GameManager.Instance.numPlayers;

        arenaDisplays = new();
        foreach (var arena in arenas)
        {
            if (nPlayers < arena.playerLimit.x) continue;
            if (nPlayers > arena.playerLimit.y) continue;

            var arenaDisplay = Instantiate(arenaDisplayPrefab, transform);
            arenaDisplay.arenaDef = arena;

            arenaDisplays.Add(arenaDisplay);
        }

        for (int i = 0; i < arenaDisplays.Count; i++)
        {
            var left = (i > 0) ? (arenaDisplays[i - 1]) : (arenaDisplays[arenaDisplays.Count - 1]);
            var right = (i < arenaDisplays.Count - 1) ? (arenaDisplays[i + 1]) : (arenaDisplays[0]);

            arenaDisplays[i].GetComponent<BaseUIControl>().SetNav(null, null, left.GetComponent<BaseUIControl>(), right.GetComponent<BaseUIControl>());
        }

        SetControl(arenaDisplays[0].GetComponent<BaseUIControl>());
    }

    protected override void OnSelect()
    {
        if (_selectedControl == null) return;
        var arenaDisplay = _selectedControl.GetComponent<ArenaDisplay>();
        if (arenaDisplay == null) return;

        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {           
            SceneManager.LoadScene(arenaDisplay.arenaDef.sceneName);
        });
        enabled = false;
    }
}
