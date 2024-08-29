using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitEffects : MonoBehaviour
{
    public UnitBase thisUnit;
    private GameObject _iconGO;
    void Start()
    {
        if (thisUnit == null) thisUnit = transform.parent.parent.GetComponent<UnitBase>();
        _iconGO = Resources.Load("Prefabs/IMG_EffectIcon") as GameObject;
        thisUnit.EffectUpdateAction += UpdateUI;
    }

    public void UpdateUI()
    {
        // 현재 자식 오브젝트를 모두 삭제
        foreach (Transform child in transform)
        {
            if (child == null) return;
            Destroy(child.gameObject);
        }

        // ActiveEffects 리스트의 각 효과에 대해 아이콘 생성 및 설정
        for (int i = 0; i < thisUnit.BuffList.Count; i++)
        {
            GameObject newIcon = Instantiate(_iconGO, transform);
            newIcon.GetComponent<EffectIcon>().SetIcon(thisUnit.BuffList[i]);
        }
    }

    //0.2초간 반짝임
    public void TwinkleIcion(int index)
    {
        var img_RelicIcon = transform.GetChild(index).GetComponent<Image>();
        img_RelicIcon.TwinkleIcon();
    }

}
