using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMouseDetection : MonoBehaviour
{
    SpriteRenderer _glowingBorder;
    private float _duration = 0.58f;
    private float _yOffset = -2.04f;
    public bool IsDraggingOverUsingZone;
    public static bool IsUsing;
    private bool _needTarget;
    private CardData _thisCardData;
    private bool IsCanceled; //��Ŭ������ ����ߴ���
    private Vector3 _beforeMouseEnterPoz;
    private Quaternion _beforeMouseEnterRotation;

    private void Start()
    {
        _thisCardData = GetComponent<CardGO>().thisCardData;
        _needTarget = _thisCardData.NeedTarget;
        Transform glowTransform = transform.Find("glow");
        _glowingBorder = glowTransform.GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (!IsCanceled &&IsDraggingOverUsingZone && Input.GetMouseButtonUp(1))
        {
            CancelUse();
        }
    }
    void OnMouseEnter()
    {
        transform.DOKill();
        if (IsDraggingOverUsingZone) return;
        if(_beforeMouseEnterPoz == Vector3.zero)
        {
            _beforeMouseEnterPoz = transform.position;
            _beforeMouseEnterRotation = transform.rotation;
        }
        transform.localScale = Vector3.one * 0.7f;
        transform.rotation = Quaternion.identity;
        GlowBorder();
        transform.localPosition = _beforeMouseEnterPoz + Vector3.up * 3;
        MoveCardFront();
    }

    void OnMouseDown()
    {
        IsCanceled = false;
        transform.rotation = Quaternion.identity;
        if (DOTween.IsTweening(transform)) // ���� ���� �� ���� ������Ʈ�� ���� Dotween�� ���� ���̶��
        {
            DOTween.Kill(transform); // �ش� Dotween�� ���
        }
        MoveCardFront();
    }

    void OnMouseDrag()
    {
        //��ҵƴٸ� �۵���������
        if (IsCanceled) return;

        if(!IsDraggingOverUsingZone)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); // ���콺 ��ġ
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition); // ���� ��ǥ�� ��ȯ
            transform.position = objPosition; // ī���� transform�� ���콺�� ���󰡰� ��
        }

        //������� ��� �÷��� ����� ����
        if (!IsDraggingOverUsingZone && transform.position.y > _yOffset)
        {
            //�ڽ�Ʈ ������ ���� ������
            if(_thisCardData.CardCost > GameManager.Battle.NowEnergy)
            {
                CancelUse();
            }
            //�� ī���� ����ڰ� �׾��ִٸ�
            else if(!BattleManager.Inst.GetPlayer(_thisCardData.CardOwner).isAlive())
            {
                CancelUse();
            }
            else
            {
                IsDraggingOverUsingZone = true;
                if (_needTarget) transform.DOMove(new Vector3(-0, -2.3f, 0), 0.15f);
            }
            
        }

        if (IsDraggingOverUsingZone)
        {
            if(_needTarget)
            {
                BezierCurveDrawer.Inst.DrawCurveFromScreenBottom();
            }
            else
            {
                Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); // ���콺 ��ġ
                Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition); // ���� ��ǥ�� ��ȯ
                transform.position = objPosition; // ī���� transform�� ���콺�� ���󰡰� ��
            }
        }
    }

    private void OnMouseUp()
    {
        //��ҵƴٸ�(���콺 ��Ŭ��) Ż��
        if (IsCanceled) return;

        //����� �ʿ��� ī���϶�
        if(_needTarget)
        {
            //���͸� �����ߴٸ�
            if(IsTargetMonster())
            {
                UseCard();
            }
            else
            {
                CancelUse();
            }
        }
        //����� �ʿ� ���� ī���� ���
        else
        {
            UseCard();
        }
    }

    private void OnMouseExit()
    {
        //����� ���� ���÷����� ���콺 �ٱ����� �����ٸ� �Ʒ� ��ɵ� ����
        if (IsDraggingOverUsingZone) {  return; }

        gameObject.transform.position = _beforeMouseEnterPoz;
        gameObject.transform.rotation = _beforeMouseEnterRotation;
        _beforeMouseEnterPoz = Vector3.zero;
        HandManager.Inst.ArrangeCardsOnce();
        HideBorder();
    }
 

    private bool IsTargetMonster()
    {
        // ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // ���콺 ��ġ�� ��ġ�� ��� Collider2D�� ã��
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(mouseWorldPos2D);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Monster"))
            {
                GameManager.Battle.TargetMonster = hitCollider.GetComponent<UnitBase>();
                return true;
            }
        }
        return false;
    }

    
    void GlowBorder()
    {
        if (_thisCardData.CardCost > GameManager.Battle.NowEnergy) return;
        transform.DOScale(new Vector3(0.7f, 0.7f), _duration);
        Color originalColor = _glowingBorder.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1.0f); // �׵θ� �ܻ��� ������
        _glowingBorder.DOColor(targetColor, _duration);
    }

    void HideBorder()
    {
        if (IsDraggingOverUsingZone)
        {
            _glowingBorder.DOKill();
            _glowingBorder.color = Color.clear;
            return;
        }
        transform.DOScale(new Vector3(0.5f, 0.5f), _duration);
        Color originalColor = _glowingBorder.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // �׵θ� �ܻ��� �����ϰ� �Ⱥ��̰�
        _glowingBorder.DOColor(targetColor, _duration);
    }

    void MoveCardFront()
    {
        // ��������Ʈ �������� ĵ���� ���� ���� ����
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        for (int j = 0; j < spriteRenderers.Length; j++)
        {
            spriteRenderers[j].sortingOrder = 2000 - j;
        }

        Canvas[] canvases = GetComponentsInChildren<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            canvas.sortingOrder = 2000 + 1;
        }
    }
    

    void CancelUse()
    {
        transform.DOKill();
        IsCanceled = true;
        IsDraggingOverUsingZone = false;
        transform.localScale = Vector3.one * 0.5f;

        HideBorder();
        BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; //���� ����ϴ�.
        HandManager.Inst.ArrangeCardsOnce();
    }

    void UseCard()
    {
        //�̹� �ٸ� ī�尡 ������̶�� ��� ���
        if (IsUsing) { CancelUse(); return; }

        IsUsing = true;
        transform.DOKill();
        HideBorder();
        HandManager.Inst.CardsInMyHandList.Remove(gameObject);
        StartCoroutine(HandManager.Inst.ArrangeCardsCoroutine());
        BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; //���� ����ϴ�.
        CardEffectManager.NowCardData = _thisCardData;
        CardEffectManager.NowCardGO = gameObject;
        CardEffectManager.Inst.UseCard();
        // ���� ��� ��������Ʈ �������� TMPtext�� ���� �����ϰ� �����մϴ�.
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }

        var texts = GetComponentsInChildren<TMPro.TMP_Text>();
        foreach (var text in texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }
    }

    private void OnDisable()
    {
        transform.DOKill();
        if (BezierCurveDrawer.Inst.lineRenderer != null)
        {
            BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; // ���� ����ϴ�.
        }
        else
        {
            // Handle the null case if needed, such as logging an error or warning
        }
    }
}
