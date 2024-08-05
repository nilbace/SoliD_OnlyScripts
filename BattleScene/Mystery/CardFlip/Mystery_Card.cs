using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Mystery_Card : MonoBehaviour
{
    private bool _isFlipped;
    private SpriteRenderer _sr;
    public TMPro.TMP_Text _text;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    [ContextMenu("뒤집기")]
    public void FlipCard()
    {
        _isFlipped = true;
        transform.DORotate(new Vector3(0, 90, 0), 0.5f).OnComplete(() =>
        {
            _sr.color = Color.gray;
            _text.gameObject.SetActive(false);
            transform.DORotate(new Vector3(0, 180, 0), 0.5f);
        });
    }
    [ContextMenu("원위치")]
    public void UnflipCard()
    {
        _isFlipped = false;
        transform.DORotate(new Vector3(0, 90, 0), 0.5f).OnComplete(() =>
        {
            _sr.color = Color.white;
            _text.gameObject.SetActive(true);
            transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        });
    }
}
