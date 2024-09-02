using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InkPage : MonoBehaviour
{
    public TMP_Text TMP_MinjuInfo;
    public TMP_Text TMP_SeolhaInfo;
    public TMP_Text TMP_YerinInfo;
    public Button ExitBTN;
    public Button BackBTN;


    private void OnEnable()
    {
        TMP_MinjuInfo.text = InkSkillManager.Inst.Info_NowMinjuInkSkill;
        TMP_SeolhaInfo.text = InkSkillManager.Inst.Info_NowSeolhaInkSkill;
        TMP_YerinInfo.text = InkSkillManager.Inst.Info_NowYerinInkSkill;
        ExitBTN.gameObject.SetActive(false);
    }

    public void ChangeInkSkill()
    {
        BackBTN.gameObject.SetActive(false);
        InkSkillManager.Inst.ChangeInkSkills();
        TMP_MinjuInfo.text = InkSkillManager.Inst.Info_NowMinjuInkSkill;
        TMP_SeolhaInfo.text = InkSkillManager.Inst.Info_NowSeolhaInkSkill;
        TMP_YerinInfo.text = InkSkillManager.Inst.Info_NowYerinInkSkill;
        var seq = DOTween.Sequence();
        seq.Join(TMP_MinjuInfo.DOFade(0, 0)).Join(TMP_SeolhaInfo.DOFade(0, 0)).Join(TMP_YerinInfo.DOFade(0, 0)).
            Append(TMP_MinjuInfo.DOFade(1, 1)).Join(TMP_SeolhaInfo.DOFade(1, 1)).Join(TMP_YerinInfo.DOFade(1, 1))
            .AppendCallback(()=> { ExitBTN.gameObject.SetActive(true); });
    }
}
