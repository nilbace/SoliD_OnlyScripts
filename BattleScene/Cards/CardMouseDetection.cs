using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMouseDetection : MonoBehaviour
{
    SpriteRenderer _glowingBorder;                  //ī���� ������ �׵θ� ��������Ʈ ������
    private float _duration = 0.58f;                //�ִϸ��̼� ���� �ð�
    private float _yOffset = -2.04f;                //�巡�� ��, ��� ���� ���� Y ������
    public bool IsDraggingOverUsingZone;            //ī�尡 ��� ���� ���� �巡�� ������ ����
    public static bool IsUsing;                     //���� ī�尡 ���������
    private bool _needTarget;                       //��� ������ �ʿ��� ī������
    private CardData _thisCardData;                 //���� ī�� ������
    private bool IsCanceled;                        //��Ŭ������ ����� ��ҵƴ��� ����
    private Vector3 _beforeMouseEnterPoz;           //���콺�� ī�� ���� �ö���� �� ī���� ��ġ
    private Quaternion _beforeMouseEnterRotation;   //���콺�� ī�� ���� �ö���� �� ī���� ȸ�� ����

    private void Start()
    {
        _thisCardData = GetComponent<CardGO>().thisCardData;                // ī�� ������ �ʱ�ȭ
        _needTarget = _thisCardData.NeedTarget;                             // ī�� ��뿡 Ÿ���� �ʿ����� ����
        Transform glowTransform = transform.Find("glow");                   // ī���� ������ �׵θ� ã��
        _glowingBorder = glowTransform.GetComponent<SpriteRenderer>();      // ��������Ʈ ������ �ʱ�ȭ
    }

    private void Update()
    {
        // ��Ŭ������ ����� ��ҵ� ���
        if (!IsCanceled && IsDraggingOverUsingZone && Input.GetMouseButtonUp(1))
        {
            CancelUse(); // ī�� ��� ���
        }
    }

    //���콺�� �����ϸ� ī�尡 ���� ��¦ ���÷����ϴ�.
    private void OnMouseEnter()
    {
        transform.DOKill();                                             // ���� ���� ���� �ִϸ��̼� ����
        if (IsDraggingOverUsingZone) return;                            // ī�尡 ��� ���� ������ �巡�� ���̸� ����

        //���⼭���� ���콺�� ���������� ���� ó��

        //��� ���� �� ��ġ ����
        if (_beforeMouseEnterPoz == Vector3.zero)
        {
            _beforeMouseEnterPoz = transform.position; 
            _beforeMouseEnterRotation = transform.rotation; 
        }
        transform.localScale = Vector3.one * 0.7f;
        transform.rotation = Quaternion.identity; 
        GlowBorder(); 
        transform.localPosition = _beforeMouseEnterPoz + Vector3.up * 3; // ī�� ��ġ ����
        MoveCardFront(); // ī�尡 �׻� �տ� ������ ����
    }


    /// <summary>
    /// ī�带 Ŭ���ϸ� 
    /// </summary>
    private void OnMouseDown()
    {
        IsCanceled = false;                         // ��� ��� �÷��� �ʱ�ȭ
        transform.rotation = Quaternion.identity;   // ī�� ȸ�� �ʱ�ȭ
        if (DOTween.IsTweening(transform))          // Dotween �ִϸ��̼� ���� ���̸� ���
        {
            DOTween.Kill(transform); 
        }
        MoveCardFront();                            
    }

    private void OnMouseDrag()
    {
        // ����� ��ҵ� ��� �۵����� ����
        if (IsCanceled) return;

        //��� ���� ������ ���ø��� �ʾҴٸ�(���� ���� �Ʒ��� �ִٸ�)
        if (!IsDraggingOverUsingZone)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); // ���콺 ��ġ
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition); // ���� ��ǥ�� ��ȯ
            transform.position = objPosition; // ī���� ��ġ�� ���콺 ��ġ�� ����
        }

        // ���� ���� �̻� �ö��� ��, ��� ���� ���� ����
        if (!IsDraggingOverUsingZone && transform.position.y > _yOffset)
        {
            // ī���� �ڽ�Ʈ�� ���� ���������� ������ ���
            if (_thisCardData.CardCost > GameManager.Battle.NowEnergy)
            {
                CancelUse(); // ī�� ��� ���
            }
            // ī���� ����ڰ� �׾� ������ ���
            else if (!BattleManager.Inst.GetPlayer(_thisCardData.CardOwner).IsAlive())
            {
                CancelUse(); // ī�� ��� ���
            }
            else
            {
                IsDraggingOverUsingZone = true; // ��� ���� ������ �巡�� ������ ����
                if (_needTarget) transform.DOMove(new Vector3(-0, -2.3f, 0), 0.15f); // Ÿ�� �ʿ� �� ��ġ �̵�
            }
        }

        //���� ���� �̻󿡼� �巡�� ���� ��
        if (IsDraggingOverUsingZone)
        {
            //��� ������ �ʿ��ϴٸ� ������ ��� ���� ����� ����
            if (_needTarget)
            {
                BezierCurveDrawer.Inst.DrawCurveFromScreenBottom(); // Ÿ���� �����ϱ� ���� � �׸���
            }
            else
            {
                Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); // ���콺 ��ġ
                Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition); // ���� ��ǥ�� ��ȯ
                transform.position = objPosition; // ī���� ��ġ�� ���콺 ��ġ�� ����
            }
        }
    }

    private void OnMouseUp()
    {
        // ����� ��ҵ� ��� �۾����� ����
        if (IsCanceled) return;

        // Ÿ���� �ʿ��� ī���� ���
        if (_needTarget)
        {
            // ���͸� ������ ���
            if (IsTargetMonster())
            {
                UseCard(); // ī�� ���
            }
            else
            {
                CancelUse(); // ī�� ��� ���
            }
        }
        // Ÿ���� �ʿ� ���� ī���� ���
        else
        {
            UseCard(); // ī�� ���
        }
    }

    private void OnMouseExit()
    {
        // ��� ���� ������ �巡�� ���� ��� ����
        if (IsDraggingOverUsingZone) { return; }

        gameObject.transform.position = _beforeMouseEnterPoz; // ���콺�� ī�� ������ ������ ���� ��ġ�� ����
        gameObject.transform.rotation = _beforeMouseEnterRotation; // ���� ȸ�� ���·� ����
        _beforeMouseEnterPoz = Vector3.zero; // �ʱ�ȭ
        HandManager.Inst.ArrangeCardsOnce(); // ī�� ������
        HideBorder();
    }

    /// <summary>
    /// ���콺�� ���͸� �����ϰ� �ִ��� ����
    /// </summary>
    /// <returns></returns>
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
                GameManager.Battle.TargetMonster = hitCollider.GetComponent<UnitBase>(); // Ÿ�� ����
                return true;
            }
        }
        return false; // Ÿ���� ���� ���
    }

    /// <summary>
    /// ī�� �׵θ��� ������ �ִϸ��̼�
    /// </summary>
    private void GlowBorder()
    {
        if (_thisCardData.CardCost > GameManager.Battle.NowEnergy) return; // �������� �����ϸ� �ƹ� �۾��� ���� ����
        transform.DOScale(new Vector3(0.7f, 0.7f), _duration); // ī�� ũ�� �ִϸ��̼�
        Color originalColor = _glowingBorder.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1.0f); // �׵θ� ������ ��� ����
        _glowingBorder.DOColor(targetColor, _duration); // �׵θ� ���� �ִϸ��̼�
    }

    /// <summary>
    /// ī�� �׵θ� �������� ��
    /// </summary>
    private void HideBorder()
    {
        if (IsDraggingOverUsingZone)
        {
            _glowingBorder.DOKill(); // �ִϸ��̼� ����
            _glowingBorder.color = Color.clear; // �׵θ� ���� �����ϰ� ����
            return;
        }
        transform.DOScale(new Vector3(0.5f, 0.5f), _duration); // ī�� ũ�� �ִϸ��̼�
        Color originalColor = _glowingBorder.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // �׵θ� ������ �����ϰ� ����
        _glowingBorder.DOColor(targetColor, _duration); // �׵θ� ���� �ִϸ��̼�
    }

    /// <summary>
    /// �� ī�带 ���� ������ �ű�
    /// </summary>
    private void MoveCardFront()
    {
        // ��������Ʈ �������� ĵ������ ���� ���� ����
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        for (int j = 0; j < spriteRenderers.Length; j++)
        {
            spriteRenderers[j].sortingOrder = 2000 - j; // ��������Ʈ ���� ���� ����
        }

        Canvas[] canvases = GetComponentsInChildren<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            canvas.sortingOrder = 2000 + 1; // ĵ���� ���� ���� ����
        }
    }

    /// <summary>
    /// ī�� ��� ��� ó��
    /// </summary>
    private void CancelUse()
    {
        transform.DOKill(); // ���� ���� ���� �ִϸ��̼� ����
        IsCanceled = true; // ��� ��� �÷��� ����
        IsDraggingOverUsingZone = false; // �巡�� ���� �÷��� �ʱ�ȭ
        transform.localScale = Vector3.one * 0.5f; // ī�� ũ�� ���

        HideBorder(); // �׵θ� �����
        BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; // � �� �����
        HandManager.Inst.ArrangeCardsOnce(); // ī�� ������
    }

    /// <summary>
    /// ī�� ���� ���� �Ŵ������� ȣ���Ͽ� ó��
    /// </summary>
    private void UseCard()
    {
        // �ٸ� ī�尡 ��� ���̸� ���� ī�� ��� ���
        if (IsUsing) { CancelUse(); return; }

        IsUsing = true; // ī�� ��� ������ ����
        transform.DOKill(); // ���� ���� ���� �ִϸ��̼� ����
        HideBorder(); 
        HandManager.Inst.CardsInMyHandList.Remove(gameObject); // ���п��� ī�� ����
        StartCoroutine(HandManager.Inst.ArrangeCardsCoroutine()); // ī�� ������
        BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; // � �� �����
        CardEffectManager.CurrentCardData = _thisCardData; // ī�� ������ ����
        CardEffectManager.CurrentCardGO = gameObject; // ī�� ���� ������Ʈ ����
        CardEffectManager.Inst.UseCard(); // ī�� ���
        // ���� ��� ��������Ʈ �������� TMP �ؽ�Ʈ�� ������ �����ϰ� ����
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0); // ��������Ʈ ���� �����ϰ� ����
        }

        var texts = GetComponentsInChildren<TMPro.TMP_Text>();
        foreach (var text in texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0); // �ؽ�Ʈ ���� �����ϰ� ����
        }
    }

    private void OnDisable()
    {
        transform.DOKill(); // ���� ���� ���� �ִϸ��̼� ����
        if (BezierCurveDrawer.Inst.lineRenderer != null)
        {
            BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; // � �� �����
        }
        else
        {
            // Null�� ����� ó��(��: ���� �Ǵ� ��� �α� �ۼ�)
        }
    }
}
