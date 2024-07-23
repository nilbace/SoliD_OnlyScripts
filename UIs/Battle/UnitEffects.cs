using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEffects : MonoBehaviour
{
    public UnitBase thisUnit;
    private GameObject _iconGO;
    void Start()
    {
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
        for (int i = 0; i < thisUnit.ActiveEffectList.Count; i++)
        {
            GameObject newIcon = Instantiate(_iconGO, transform);
            newIcon.GetComponent<EffectIcon>().SetIcon(thisUnit.ActiveEffectList[i]);
        }
    }
}
