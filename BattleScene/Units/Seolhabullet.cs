using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Seolhabullet : MonoBehaviour, IPointerEnterHandler
{
    public E_BulletType BulletType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log(BulletType);
    }

    private void OnMouseEnter()
    {
        Debug.Log(BulletType);
    }
}
