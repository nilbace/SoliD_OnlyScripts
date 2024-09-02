using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHpBar : MonoBehaviour
{
    public E_CharName CharName;
    private UnitBase thisUnit;
    private Slider HpSlider;
    public Sprite[] HPSliderBackSprite;
    void Start()
    {
        HpSlider = GetComponent<Slider>();

        if (CharName == E_CharName.Null) thisUnit = transform.parent.parent.GetComponent<UnitBase>();
        else
        {
            thisUnit = GameObject.FindWithTag(CharName.ToString()).GetComponent<UnitBase>();
            Image backgroundImage = transform.Find("Background").GetComponent<Image>();
            backgroundImage.sprite = HPSliderBackSprite[(int)CharName - 1];
        }
    }

    void Update()
    {
        HpSlider.UpdateHP(thisUnit);
    }
}
