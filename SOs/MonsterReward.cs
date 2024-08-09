using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterReward", menuName = "Scriptable Object/MonsterReward", order = int.MaxValue)]
public class MonsterReward : ScriptableObject
{
    [Range(0, 150)] public int MinMoonStoneReward;
    [Range(0, 150)] public int MaxMoonStoneReward;
    [Range(0, 10)] public int MinMemoryFramentDiv100;
    [Range(0, 10)] public int MaxMemoryFragmentDiv100;
    [Range(0, 10)] public int MinCoreFragment;
    [Range(0, 10)] public int MaxCoreFragment;
    public E_CardTier[] RewardCardType;
    public E_RelicType[] RewardRelicType;
    public bool IsBoss;
    public E_RelicType[] ExtraRelicReward;
}
