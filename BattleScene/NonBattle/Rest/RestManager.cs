using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RestManager : MonoBehaviour
{
    public GameObject RestPage;
    public GameObject EnterPage;
    public GameObject InkPage;

    private void OnEnable()
    {
        RestPage.SetActive(false);
        EnterPage.SetActive(true);
        InkPage.SetActive(false);
    }

    public void ExitRest()
    {
        Map.MapPlayerTracker.Instance.Locked = false;
        gameObject.SetActive(false);
        BaseUI.Inst.MapBTN();
    }

    public void HealMagenta()
    {
        StartCoroutine(BattleManager.Inst.GetPlayer(E_CharName.Minju).HealCoroutine(35));
        gameObject.SetActive(false);

        var seq = DOTween.Sequence();
        seq.AppendInterval(1).AppendCallback(() => { ExitRest(); });
    }

    public void HealCyan()
    {
        StartCoroutine(BattleManager.Inst.GetPlayer(E_CharName.Seolha).HealCoroutine(35));
        gameObject.SetActive(false);

        var seq = DOTween.Sequence();
        seq.AppendInterval(1).AppendCallback(() => { ExitRest(); });
    }

    public void HealYellow()
    {
        StartCoroutine(BattleManager.Inst.GetPlayer(E_CharName.Yerin).HealCoroutine(35));
        gameObject.SetActive(false);

        var seq = DOTween.Sequence();
        seq.AppendInterval(1).AppendCallback(() => { ExitRest(); });
    }
}
