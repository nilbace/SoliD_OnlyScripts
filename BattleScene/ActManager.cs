using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActManager : MonoBehaviour
{
    public static ActManager Inst;
    [HideInInspector] public int nowAct;

    private void Awake()
    {
        Inst = this;
        nowAct = 1;
    }
 }
