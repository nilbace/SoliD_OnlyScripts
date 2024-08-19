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
    private bool IsCanceled; //우클릭으로 취소했는지
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
        if (DOTween.IsTweening(transform)) // 만약 현재 이 게임 오브젝트에 대해 Dotween이 실행 중이라면
        {
            DOTween.Kill(transform); // 해당 Dotween을 취소
        }
        MoveCardFront();
    }

    void OnMouseDrag()
    {
        //취소됐다면 작동하지않음
        if (IsCanceled) return;

        if(!IsDraggingOverUsingZone)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); // 마우스 위치
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition); // 월드 좌표로 변환
            transform.position = objPosition; // 카드의 transform을 마우스를 따라가게 함
        }

        //어느정도 들어 올려야 사용중 판정
        if (!IsDraggingOverUsingZone && transform.position.y > _yOffset)
        {
            //코스트 여유가 되지 않으면
            if(_thisCardData.CardCost > GameManager.Battle.NowEnergy)
            {
                CancelUse();
            }
            //이 카드의 사용자가 죽어있다면
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
                Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); // 마우스 위치
                Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition); // 월드 좌표로 변환
                transform.position = objPosition; // 카드의 transform을 마우스를 따라가게 함
            }
        }
    }

    private void OnMouseUp()
    {
        //취소됐다면(마우스 우클릭) 탈출
        if (IsCanceled) return;

        //대상이 필요한 카드일때
        if(_needTarget)
        {
            //몬스터를 조준했다면
            if(IsTargetMonster())
            {
                UseCard();
            }
            else
            {
                CancelUse();
            }
        }
        //대상이 필요 없는 카드일 경우
        else
        {
            UseCard();
        }
    }

    private void OnMouseExit()
    {
        //사용을 위해 들어올려져서 마우스 바깥으로 나갔다면 아래 명령들 무시
        if (IsDraggingOverUsingZone) {  return; }

        gameObject.transform.position = _beforeMouseEnterPoz;
        gameObject.transform.rotation = _beforeMouseEnterRotation;
        _beforeMouseEnterPoz = Vector3.zero;
        HandManager.Inst.ArrangeCardsOnce();
        HideBorder();
    }
 

    private bool IsTargetMonster()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // 마우스 위치와 겹치는 모든 Collider2D를 찾음
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
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1.0f); // 테두리 잔상을 빛나게
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
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // 테두리 잔상을 투명하게 안보이게
        _glowingBorder.DOColor(targetColor, _duration);
    }

    void MoveCardFront()
    {
        // 스프라이트 렌더러와 캔버스 정렬 순서 조정
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
        BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; //선을 숨깁니다.
        HandManager.Inst.ArrangeCardsOnce();
    }

    void UseCard()
    {
        //이미 다른 카드가 사용중이라면 사용 취소
        if (IsUsing) { CancelUse(); return; }

        IsUsing = true;
        transform.DOKill();
        HideBorder();
        HandManager.Inst.CardsInMyHandList.Remove(gameObject);
        StartCoroutine(HandManager.Inst.ArrangeCardsCoroutine());
        BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; //선을 숨깁니다.
        CardEffectManager.NowCardData = _thisCardData;
        CardEffectManager.NowCardGO = gameObject;
        CardEffectManager.Inst.UseCard();
        // 하위 모든 스프라이트 랜더러와 TMPtext의 색을 투명하게 변경합니다.
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
            BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; // 선을 숨깁니다.
        }
        else
        {
            // Handle the null case if needed, such as logging an error or warning
        }
    }
}
