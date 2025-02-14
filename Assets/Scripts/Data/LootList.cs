using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LootList", menuName = "Bubble/Loot List")]
public class LootList : ScriptableObject
{
    public ResourceDataProbList lootList;

    internal ResourceData Get()
    {
        if (lootList == null) return null;

        return lootList.Get();
    }
}
