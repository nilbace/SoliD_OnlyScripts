using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartHelper : MonoBehaviour
{
    public void MaxHP_Plus_3()
    {
        foreach(UnitBase unit in BattleManager.Inst.PlayerUnits)
        {
            unit.MaxHP += 3;
        }
    }
    public void StartBTN()
    {
        Map.MapPlayerTracker.Instance.Locked = false;
        Map.MapView.Inst.ShowMap();
        Destroy(gameObject);
    }
}
