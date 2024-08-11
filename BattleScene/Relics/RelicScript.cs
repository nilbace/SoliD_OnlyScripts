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
        { E_RelicType.RubyPendant, "��� �Ҵ�Ʈ" },
        { E_RelicType.BlackRose, "���� ���" },
        { E_RelicType.DarkHeartbeat, "����� �ƹ�" },
        { E_RelicType.BlessingofFourGods, "�ż��� ��ȣ" },
        { E_RelicType.FourGodsBox, "��ż� ���� ����" },
        { E_RelicType.InfiniteRevolver, "���� ������" },
        { E_RelicType.ReinforcedBullet, "��ȭź" },
        { E_RelicType.BlueScabbard, "�����̾� ����" },
        { E_RelicType.WrathElixir, "�г��� ����" },
        { E_RelicType.BladeofExecutor, "�������� Į��" },
        { E_RelicType.TraitorsTongue, "������� ��" },
        { E_RelicType.BlessingofTopaz, "Ȳ������ ��ȣ" },
        { E_RelicType.RippedDoll, "������ �������" },
        { E_RelicType.BrokenDoll, "������ ��������" },
        { E_RelicType.ScytheofGod, "����� ��" },
        { E_RelicType.HeartofEternity, "������ ����" },
        { E_RelicType.BlackFeather, "����� ����" },
        { E_RelicType.BeautysTear, "������ ����" },
        { E_RelicType.EnergyDrink, "������ �帵ũ" },
        { E_RelicType.HolyGrailofBlood, "���� ����" },
        { E_RelicType.ViperVenom, "������ �͵�" },
        { E_RelicType.UnexpectedPresent, "����� ����" },
        { E_RelicType.Printer, "�����" },
        { E_RelicType.NicePerfume, "����� ���" },
        { E_RelicType.FountainofLife, "������ �м�" },
        { E_RelicType.NegotiationSkills, "������ ���" },
        { E_RelicType.ShopCoupon, "���� ���� ����" },
        { E_RelicType.OpenedLock, "���� ���� �ݰ�" },
        { E_RelicType.Trashcan, "������" },
        { E_RelicType.ChocoProtein, "���ڸ� ����ƾ" },
        { E_RelicType.SnailHouse, "������ ����" },
        { E_RelicType.ScarfofLizard, "�񵵸� �������� �񵵸�" },
        { E_RelicType.TurtleHouse, "�ź��� ���" },
        { E_RelicType.MiniMirror, "�̴� �հſ�" },
        { E_RelicType.MiniComb, "���� �߷����� ������" },
        { E_RelicType.TreasureChest, "��������" },
        { E_RelicType.PinkHairRoll, "��ũ�� ����" },
        { E_RelicType.QuestionCollector, "���� ������" },
        { E_RelicType.TeachersKey, "������ ����" },
        { E_RelicType.FrozenCat, "�ǲ� ������ �����" },
        { E_RelicType.Macaron, "��ī��" },
        { E_RelicType.TeaPartySet, "Ƽ��Ƽ ��Ʈ" }
    };

    private static readonly Dictionary<E_RelicType, string> RelicDescriptions = new Dictionary<E_RelicType, string>
    {
        { E_RelicType.RubyPendant, "���� ���� �� ������ ü���� 2 ȸ���մϴ�." },
        { E_RelicType.BlackRose, "������ ü���� 50% ������ �� �渶���� 3 ȹ���մϴ�." },
        { E_RelicType.DarkHeartbeat, "���� ī�带 ����� ������ ������ ü���� 2 ȸ���մϴ�." },
        { E_RelicType.BlessingofFourGods, "��ż� ������ 30%�� �߰� ���ظ� �ݴϴ�." },
        { E_RelicType.FourGodsBox, "�� �� ���۽� ������ ��ż� ������ �п� �߰��մϴ�." },
        { E_RelicType.InfiniteRevolver, "�� �� ���۸��� ������ źȯ�� 1�� �����մϴ�." },
        { E_RelicType.ReinforcedBullet, "������ źȯ�� 2 ��� 4�� ������� �ο��մϴ�." },
        { E_RelicType.BlueScabbard, "ù �Ͽ� 2���� �ܵ� ī�带 ȹ���մϴ�." },
        { E_RelicType.WrathElixir, "�� �Ͽ� ���� ī�带 3�� ����� ������ �����̸� 1 ȹ���մϴ�." },
        { E_RelicType.BladeofExecutor, "���� ī�带 10�� ����� ������ 1�� �����̸� ȹ���մϴ�." },
        { E_RelicType.TraitorsTongue, "������ �ο��ϴ� ��ȭ�� ����� ���ӽð��� 50% �þ�ϴ�." },
        { E_RelicType.BlessingofTopaz, "�� ���� �� ���� �������� ������� �ʽ��ϴ�." },
        { E_RelicType.RippedDoll, "��� ������ ���� 50% ��� 75%�� ���ظ� �߰��� �޽��ϴ�." },
        { E_RelicType.BrokenDoll, "��ȭ�� ���� ���� ���ط��� 25% ��� 40%��ŭ �����մϴ�." },
        { E_RelicType.ScytheofGod, "���� ���� �� �� ��ü���� ����� ǥ���� �ο��մϴ�." },
        { E_RelicType.HeartofEternity, "�� �� ���� �� �������� 1 �߰��� ȹ���մϴ�. ������ �ó��� ȿ���� ������ �� �����ϴ�." },
        { E_RelicType.BlackFeather, "�� �� ���� �� �������� 1 �߰��� ȹ���մϴ�. ���� ���� �� ��� ���� ���� 1 �߰��� ȹ���մϴ�." },
        { E_RelicType.BeautysTear, "�� �� ���� �� �������� 1 �߰��� ȹ���մϴ�. ������ �������� ȹ���� �� �����ϴ�." },
        { E_RelicType.EnergyDrink, "������ ����Ʈ ���� ���� �� �� ���� �� �������� ȹ���մϴ�." },
        { E_RelicType.HolyGrailofBlood, "���� ������ ���� ��ġ�� �� �̻� �������� �ʽ��ϴ�." },
        { E_RelicType.ViperVenom, "����Ʈ ���� ������ 25% ���� HP�� ���մϴ�." },
        { E_RelicType.UnexpectedPresent, "����Ʈ óġ �� ������ �߰��� �ϳ� �� ���� �� �ֽ��ϴ�." },
        { E_RelicType.Printer, "ȹ�� ��, ������ ī�带 ������ �����մϴ�." },
        { E_RelicType.NicePerfume, "���� ���� �� ù �Ͽ� 1�� �������� ȹ���մϴ�." },
        { E_RelicType.FountainofLife, "���� ���� ���� �� ��� �Ʊ��� ü���� 10 ȸ���մϴ�." },
        { E_RelicType.NegotiationSkills, "������ ��� ǰ���� 50% ���ε˴ϴ�." },
        { E_RelicType.ShopCoupon, "������ ī�� ���� ����� n ���� �����˴ϴ�." },
        { E_RelicType.OpenedLock, "�� óġ �� ��� �������� ���� 25% �����մϴ�." },
        { E_RelicType.Trashcan, "������ ī�� 2���� �����մϴ�." },
        { E_RelicType.ChocoProtein, "�� ������ ���� �� ��� �Ʊ��� �ִ� ü���� 1�� ����մϴ�." },
        { E_RelicType.SnailHouse, "�� �̻� ��ȭ�� ���� �ʽ��ϴ�." },
        { E_RelicType.ScarfofLizard, "�� �̻� ����� ���� �ʽ��ϴ�." },
        { E_RelicType.TurtleHouse, "���� ���� �� ��� �Ʊ��� ���� 10 ȹ���մϴ�." },
        { E_RelicType.MiniMirror, "�ູ�� 5 ���� ä�� ������ �����մϴ�." },
        { E_RelicType.MiniComb, "����ȭ�� 3 ���� ä�� ������ �����մϴ�." },
        { E_RelicType.TreasureChest, "ȹ�� �� 500 �������� ȹ���մϴ�." },
        { E_RelicType.PinkHairRoll, "ȹ�� �� �ھ� ������ 10�� ȹ���մϴ�." },
        { E_RelicType.QuestionCollector, "? �濡 �� ������, 50 �������� ȹ���մϴ�." },
        { E_RelicType.TeachersKey, "? �濡�� �� �̻� �̺�Ʈ�� �ƴ� ������ �߻����� �ʽ��ϴ�." },
        { E_RelicType.FrozenCat, "������ 10 ���� ä�� ������ �����մϴ�." },
        { E_RelicType.Macaron, "ȹ�� ��, ü���� ���� ȸ���մϴ�." },
        { E_RelicType.TeaPartySet, "�޽��� ���� ������ 15�� ü���� �߰��� ȸ���մϴ�." }
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