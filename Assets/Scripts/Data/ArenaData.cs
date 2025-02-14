using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "Arena", menuName = "Bubble/Arena Data")]
public class ArenaData : ScriptableObject
{
    public string       displayName;
    public Sprite       thumbnail;
    [TextArea]
    public string       description;
    public Vector2Int   playerLimit = new Vector2Int(1, 1);
    [Scene]
    public string       sceneName;
}
