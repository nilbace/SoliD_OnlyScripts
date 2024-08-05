using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardGO : MonoBehaviour
{
    public CardData thisCardData;
    public Sprite[] Sprites_Cost;
    public Sprite[] Sprites_Line;
    [Tooltip("모든 카드 일러스트 담아두는 곳")]
    public Sprite[] Sprites_illust;
    public SpriteRenderer SR_Cost;
    public SpriteRenderer SR_Line;
    public SpriteRenderer SR_illust;
    public TMPro.TMP_Text TMP_Name;
    public TMPro.TMP_Text TMP_Info;
    public TMPro.TMP_Text TMP_Cost;


    [ContextMenu("초기 설정")]
    public void SetUpCardSprite()
    {
        int index = (int)thisCardData.CardColor;
        SR_Cost.sprite = Sprites_Cost[index];
        SR_Line.sprite = Sprites_Line[index];
        SetUp_illust();
        TMP_Name.text = thisCardData.CardName;
        TMP_Info.text = thisCardData.CardInfoText;
        TMP_Cost.text = thisCardData.CardCost.ToString();
    }

    void SetUp_illust()
    {
        if (thisCardData.CardSpriteNameString == "")
        {
             return;
        }
        SR_illust.sprite = Sprites_illust.FirstOrDefault(sprite => sprite.name == thisCardData.CardSpriteNameString);
    }
}
