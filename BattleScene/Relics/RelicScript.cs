using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_RelicType
{
    RubyPendant,
    BlackRose,
    DarkHeartbeat,
    BlessingofFourGods,
    FourGodsBox,
    InfiniteRevolver,
    ReinforcedBullet,
    BlueScabbard,
    WrathElixir,
    BladeofExecutor,
    TraitorsTongue,
    BlessingofTopaz,
    RippedDoll,
    BrokenDoll,
    ScytheofGod,
    HeartofEternity,
    BlackFeather,
    BeautysTear,
    EnergyDrink,
    HolyGrailofBlood,
    ViperVenom,
    UnexpectedPresent,
    Printer,
    NicePerfume,
    FountainofLife,
    NegotiationSkills,
    ShopCoupon,
    OpenedLock,
    Trashcan,
    ChocoProtein,
    SnailHouse,
    ScarfofLizard,
    TurtleHouse,
    MiniMirror,
    MiniComb,
    TreasureChest,
    PinkHairRoll,
    QuestionCollector,
    TeachersKey,
    FrozenCat,
    Macaron,
    TeaPartySet,
    MaxCount
}
public enum E_RelicTier { Mystery, Normal, Elite, Boss_Synergy, Boss_Energy }
public enum E_RelicEffectTriggerType { OnBattleStart, OnPlayerTurnStart, OnCardUse, None , OnBattleEnd, OnGet }


public class RelicBase
{
    public int RelicID;
    public E_RelicType RelicType;
    public E_RelicTier RelicTier;
    public string RelicInfoString;
    public string RelicNameKor;
    public E_RelicEffectTriggerType TriggerType;
    public bool HasStack;
    public int Stack;

}
