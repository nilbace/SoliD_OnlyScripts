using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestManager : MonoBehaviour
{
    public GameObject RestPage;

    private void OnEnable()
    {
        RestPage.SetActive(false);
    }

    public void ExitRest()
    {
        Map.MapPlayerTracker.Instance.Locked = false;
        gameObject.SetActive(false);
    }

    public void HealMagenta()
    {
        BattleManager.Inst.GetPlayer(E_CharName.Minju).Heal(35);
    }

    public void HealCyan()
    {
        BattleManager.Inst.GetPlayer(E_CharName.Seolha).Heal(35);
    }

    public void HealYellow()
    {
        BattleManager.Inst.GetPlayer(E_CharName.Yerin).Heal(35);
    }
}
