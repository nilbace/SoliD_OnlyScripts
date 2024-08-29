using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beaker : MonoBehaviour
{
    public ScienceClassroom ScienceClassroom;
    public E_CardColor ThisBeakerColor;
    private Color _color;
    private void Start()
    {
        if (ThisBeakerColor == E_CardColor.Magenta) _color = Color.magenta;
        else if (ThisBeakerColor == E_CardColor.Cyan) _color = Color.cyan;
        else _color = Color.yellow;
    }
    private void OnMouseUpAsButton()
    {
        ScienceClassroom.AddColor(_color);
    }
}
