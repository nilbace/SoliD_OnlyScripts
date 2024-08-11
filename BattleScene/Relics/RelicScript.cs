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
public enum E_RelicTier { Mystery, Normal, Elite, Boss }



public class RelicBase
{
    public E_RelicType RelicType;
    public E_RelicTier RelicTier;
    public string RelicInfoString;

    private static readonly Dictionary<E_RelicType, string> RelicTypeToKorNameMap = new Dictionary<E_RelicType, string>
    {
        { E_RelicType.RubyPendant, "루비 팬던트" },
        { E_RelicType.BlackRose, "검은 장미" },
        { E_RelicType.DarkHeartbeat, "어둠의 맥박" },
        { E_RelicType.BlessingofFourGods, "신수의 가호" },
        { E_RelicType.FourGodsBox, "사신수 부적 상자" },
        { E_RelicType.InfiniteRevolver, "무한 리볼버" },
        { E_RelicType.ReinforcedBullet, "강화탄" },
        { E_RelicType.BlueScabbard, "사파이어 검집" },
        { E_RelicType.WrathElixir, "분노의 영약" },
        { E_RelicType.BladeofExecutor, "집행자의 칼날" },
        { E_RelicType.TraitorsTongue, "배신자의 혀" },
        { E_RelicType.BlessingofTopaz, "황수정의 가호" },
        { E_RelicType.RippedDoll, "찢어진 헝겊인형" },
        { E_RelicType.BrokenDoll, "갈라진 나무인형" },
        { E_RelicType.ScytheofGod, "사신의 낫" },
        { E_RelicType.HeartofEternity, "영원의 심장" },
        { E_RelicType.BlackFeather, "어둠의 깃털" },
        { E_RelicType.BeautysTear, "가인의 눈물" },
        { E_RelicType.EnergyDrink, "에너지 드링크" },
        { E_RelicType.HolyGrailofBlood, "피의 성배" },
        { E_RelicType.ViperVenom, "독사의 맹독" },
        { E_RelicType.UnexpectedPresent, "뜻밖의 선물" },
        { E_RelicType.Printer, "복사기" },
        { E_RelicType.NicePerfume, "산뜻한 향수" },
        { E_RelicType.FountainofLife, "생명의 분수" },
        { E_RelicType.NegotiationSkills, "협상의 기술" },
        { E_RelicType.ShopCoupon, "매점 할인 쿠폰" },
        { E_RelicType.OpenedLock, "반쯤 열린 금고" },
        { E_RelicType.Trashcan, "휴지통" },
        { E_RelicType.ChocoProtein, "초코맛 프로틴" },
        { E_RelicType.SnailHouse, "달팽이 껍질" },
        { E_RelicType.ScarfofLizard, "목도리 도마뱀의 목도리" },
        { E_RelicType.TurtleHouse, "거북이 등껍질" },
        { E_RelicType.MiniMirror, "미니 손거울" },
        { E_RelicType.MiniComb, "끝이 잘려나간 꼬리빗" },
        { E_RelicType.TreasureChest, "보물상자" },
        { E_RelicType.PinkHairRoll, "핑크색 헤어롤" },
        { E_RelicType.QuestionCollector, "갈고리 수집가" },
        { E_RelicType.TeachersKey, "교무실 열쇠" },
        { E_RelicType.FrozenCat, "꽁꽁 얼어붙은 고양이" },
        { E_RelicType.Macaron, "마카롱" },
        { E_RelicType.TeaPartySet, "티파티 세트" }
    };

    private static readonly Dictionary<E_RelicType, string> RelicDescriptions = new Dictionary<E_RelicType, string>
    {
        { E_RelicType.RubyPendant, "전투 시작 시 민주의 체력을 2 회복합니다." },
        { E_RelicType.BlackRose, "민주의 체력이 50% 이하일 때 흑마력을 3 획득합니다." },
        { E_RelicType.DarkHeartbeat, "검은 카드를 사용할 때마다 민주의 체력을 2 회복합니다." },
        { E_RelicType.BlessingofFourGods, "사신수 부적이 30%의 추가 피해를 줍니다." },
        { E_RelicType.FourGodsBox, "매 턴 시작시 무작위 사신수 부적을 패에 추가합니다." },
        { E_RelicType.InfiniteRevolver, "내 턴 시작마다 무작위 탄환을 1발 장전합니다." },
        { E_RelicType.ReinforcedBullet, "설하의 탄환이 2 대신 4의 디버프를 부여합니다." },
        { E_RelicType.BlueScabbard, "첫 턴에 2장의 단도 카드를 획득합니다." },
        { E_RelicType.WrathElixir, "한 턴에 공격 카드를 3장 사용할 때마다 날붙이를 1 획득합니다." },
        { E_RelicType.BladeofExecutor, "공격 카드를 10장 사용할 때마다 1의 날붙이를 획득합니다." },
        { E_RelicType.TraitorsTongue, "예린이 부여하는 약화와 취약의 지속시간이 50% 늘어납니다." },
        { E_RelicType.BlessingofTopaz, "턴 종료 시 남은 에너지가 사라지지 않습니다." },
        { E_RelicType.RippedDoll, "취약 상태의 적이 50% 대신 75%의 피해를 추가로 받습니다." },
        { E_RelicType.BrokenDoll, "약화된 적의 공격 피해량이 25% 대신 40%만큼 감소합니다." },
        { E_RelicType.ScytheofGod, "전투 시작 시 적 전체에게 사신의 표식을 부여합니다." },
        { E_RelicType.HeartofEternity, "매 턴 시작 시 에너지를 1 추가로 획득합니다. 앞으로 시너지 효과를 변경할 수 없습니다." },
        { E_RelicType.BlackFeather, "매 턴 시작 시 에너지를 1 추가로 획득합니다. 전투 시작 시 모든 적이 힘을 1 추가로 획득합니다." },
        { E_RelicType.BeautysTear, "매 턴 시작 시 에너지를 1 추가로 획득합니다. 앞으로 문스톤을 획득할 수 없습니다." },
        { E_RelicType.EnergyDrink, "보스와 엘리트 전투 동안 매 턴 시작 시 에너지를 획득합니다." },
        { E_RelicType.HolyGrailofBlood, "턴이 지나도 출혈 수치가 더 이상 감소하지 않습니다." },
        { E_RelicType.ViperVenom, "엘리트 방의 적들이 25% 적은 HP를 지닙니다." },
        { E_RelicType.UnexpectedPresent, "엘리트 처치 시 유물을 추가로 하나 더 얻을 수 있습니다." },
        { E_RelicType.Printer, "획득 시, 덱에서 카드를 선택해 복사합니다." },
        { E_RelicType.NicePerfume, "전투 시작 후 첫 턴에 1의 에너지를 획득합니다." },
        { E_RelicType.FountainofLife, "보스 전투 시작 시 모든 아군의 체력을 10 회복합니다." },
        { E_RelicType.NegotiationSkills, "상점의 모든 품목이 50% 할인됩니다." },
        { E_RelicType.ShopCoupon, "상점의 카드 제거 비용이 n 골드로 고정됩니다." },
        { E_RelicType.OpenedLock, "적 처치 시 얻는 문스톤의 양이 25% 증가합니다." },
        { E_RelicType.Trashcan, "덱에서 카드 2장을 제거합니다." },
        { E_RelicType.ChocoProtein, "매 전투가 끝난 뒤 모든 아군의 최대 체력이 1씩 상승합니다." },
        { E_RelicType.SnailHouse, "더 이상 약화를 받지 않습니다." },
        { E_RelicType.ScarfofLizard, "더 이상 취약을 받지 않습니다." },
        { E_RelicType.TurtleHouse, "전투 시작 시 모든 아군이 방어도를 10 획득합니다." },
        { E_RelicType.MiniMirror, "축복을 5 얻은 채로 전투를 시작합니다." },
        { E_RelicType.MiniComb, "결정화를 3 얻은 채로 전투를 시작합니다." },
        { E_RelicType.TreasureChest, "획득 시 500 문스톤을 획득합니다." },
        { E_RelicType.PinkHairRoll, "획득 시 코어 파편을 10개 획득합니다." },
        { E_RelicType.QuestionCollector, "? 방에 들어갈 때마다, 50 문스톤을 획득합니다." },
        { E_RelicType.TeachersKey, "? 방에서 더 이상 이벤트가 아닌 전투가 발생하지 않습니다." },
        { E_RelicType.FrozenCat, "빙결을 10 얻은 채로 전투를 시작합니다." },
        { E_RelicType.Macaron, "획득 시, 체력을 전부 회복합니다." },
        { E_RelicType.TeaPartySet, "휴식을 취할 때마다 15의 체력을 추가로 회복합니다." }
    };

    public string GetKorName()
    {
        if (RelicTypeToKorNameMap.TryGetValue(RelicType, out string korName))
        {
            return korName;
        }
        return "Unknown Relic";
    }

    public string GetRelicDescription()
    {
        if (RelicDescriptions.TryGetValue(RelicType, out string description))
        {
            return description;
        }
        return "No Description Available.";
    }
  }

public class RelicFactory
{

}