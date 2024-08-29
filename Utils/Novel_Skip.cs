using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Novel_Skip : MonoBehaviour
{
    public async void Skip()
    {
        await GameManager.Novel.CallSwitchToAdventureMode();
    }
}
