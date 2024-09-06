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

    //ȭ�鿡 ���̴� ���� ������ ����
    public void UpdateUI()
    {
        // ���� �ڽ� ������Ʈ�� ��� ����
        foreach (Transform child in transform)
        {
            if (child == null) return;
            Destroy(child.gameObject);
        }

        // ActiveEffects ����Ʈ�� �� ȿ���� ���� ������ ���� �� ����
        for (int i = 0; i < thisUnit.ActiveBuffList.Count; i++)
        {
            GameObject newIcon = Instantiate(_buffIconGO, transform);
            newIcon.GetComponent<EffectIcon>().SetIcon(thisUnit.ActiveBuffList[i]);
        }
    }

    //0.2�ʰ� ��¦��
    public void TwinkleIcion(int index)
    {
        var img_RelicIcon = transform.GetChild(index).GetComponent<Image>();
        img_RelicIcon.TwinkleIcon();
    }

}
