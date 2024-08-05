using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum E_CharName
{
    Null,
    Minju,
    Seolha,
    Yerin
}

public enum E_CharNameKor
{
    »´πŒ¡÷,
    √÷º≥«œ,
    »≤øπ∏∞,
}

public enum E_CardType { Attack, Defence, Skill }
public enum E_CardColor { Magenta, Cyan, Yellow, Black }
public enum E_CardTier { Basic,Normal, Rare, InBattle }
public enum E_TargetType
{
    None,
    TargetEnemy,
    AllEnemies,
    Self,
    AnyEnemy,
    AllAllies,
    MaxCount
}

public enum E_WeaponType { None,Talisman, Staff, Blade, ShotGun, Guitar, Scythe}

[System.Serializable]
public class CardEffectData
{
    public int EffectIndex;
    public E_TargetType TargetType;
    public E_EffectType CardEffectType;
    public float Amount;
    public string InfoString;

    public CardEffectData(CardEffectData cardEffectData)
    {
        EffectIndex = cardEffectData.EffectIndex;
        TargetType = cardEffectData.TargetType;
        CardEffectType = cardEffectData.CardEffectType;
        Amount = cardEffectData.Amount;
        InfoString = cardEffectData.InfoString;
    }

    public CardEffectData() { }
}
[System.Serializable]
public class CardData
{
    public int CardIndex;
    public E_CardType CardType;
    public E_CharName CardOwner;
    public E_CardColor CardColor;
    public E_WeaponType WeaponType;
    public E_CardTier CardTier;
    public int CardCost;
    public string CardName;
    public string CardInfoText;
    public bool NeedTarget;
    public string CardSpriteNameString;
    public List<CardEffectData> CardEffectList;

    public CardData()
    {
        CardEffectList = new List<CardEffectData>();
    }
}

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Inst { get; private set; }
    protected virtual void Awake() => Inst = FindObjectOfType(typeof(T)) as T;
}

public static class ColorExtensions
{
    public static Color magenta { get { return new Color(1f, 0f, 1f, 1f); } }
    public static Color cyan { get { return new Color(0f, 1f, 1f, 1f); } }
}