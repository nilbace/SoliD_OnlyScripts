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
        // ���� �ڽ� ������Ʈ�� ��� ����
        foreach (Transform child in transform)
        {
            if (child == null) return;
            Destroy(child.gameObject);
        }

        // ActiveEffects ����Ʈ�� �� ȿ���� ���� ������ ���� �� ����
        for (int i = 0; i < thisUnit.BuffList.Count; i++)
        {
            GameObject newIcon = Instantiate(_iconGO, transform);
            newIcon.GetComponent<EffectIcon>().SetIcon(thisUnit.BuffList[i]);
        }
    }

    //0.2�ʰ� ��¦��
    public void TwinkleIcion(int index)
    {
        var img_RelicIcon = transform.GetChild(index).GetComponent<Image>();
        img_RelicIcon.TwinkleIcon();
    }

}
