using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_UnitBuffBar : MonoBehaviour
{
    public UnitBase thisUnit;
    private GameObject _buffIconGO;
    void Start()
    {
        if (thisUnit == null) thisUnit = transform.parent.parent.GetComponent<UnitBase>();
        _buffIconGO = Resources.Load("Prefabs/IMG_EffectIcon") as GameObject;
        thisUnit.OnBuffUpdate += UpdateUI;
    }

    //화면에 보이는 버프 아이콘 갱신
    public void UpdateUI()
    {
        // 현재 자식 오브젝트를 모두 삭제
        foreach (Transform child in transform)
        {
            if (child == null) return;
            Destroy(child.gameObject);
        }

        // ActiveEffects 리스트의 각 효과에 대해 아이콘 생성 및 설정
        for (int i = 0; i < thisUnit.ActiveBuffList.Count; i++)
        {
            GameObject newIcon = Instantiate(_buffIconGO, transform);
            newIcon.GetComponent<EffectIcon>().SetIcon(thisUnit.ActiveBuffList[i]);
        }
    }

    //0.2초간 반짝임
    public void TwinkleIcion(int index)
    {
        var img_RelicIcon = transform.GetChild(index).GetComponent<Image>();
        img_RelicIcon.TwinkleIcon();
    }

}
