using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "Phase", menuName = "Bubble/Phase Data")]
public class PhaseData : ScriptableObject
{
    public enum PhaseTrigger { Initial, TimeSinceStart, TimeSinceLastPhase };

    public PhaseTrigger phaseTrigger;
    [ShowIf(nameof(needsTime))]
    public float        time;
    public string       title = "Phase 1";
    public Color        titleColor = Color.white;
    public string       subtitle = "Gather";
    public Color        subtitleColor = Color.white;
    [TextArea]
    public string       description = "Gather Resources\nPeaceful Coexistence";
    public Color        descriptionColor = Color.white;
    public bool         cancelOpponentRequestsOnRequestCompletion;
    public bool         forceRequestCompetition;
    public bool         weaponsFree;
    public AudioClip    phaseMusic;

    bool needsTime => (phaseTrigger == PhaseTrigger.TimeSinceStart) || (phaseTrigger == PhaseTrigger.TimeSinceLastPhase);
}
