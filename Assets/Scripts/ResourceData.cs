using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Bubble/ResourceData")]
public class ResourceData : ScriptableObject
{
    public string displayName;
    public Color  displayColor = Color.white;
    public Sprite nodeImage;
    public Sprite resourceImage;
}

#if UNITY_6000_0_OR_NEWER

[Serializable]
public class ResourceDataProbList : ProbList<ResourceData> { }

#endif
