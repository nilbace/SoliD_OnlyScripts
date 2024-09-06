using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMouseDetection : MonoBehaviour
{
    SpriteRenderer _glowingBorder;                  //카드의 빛나는 테두리 스프라이트 렌더러
    private float _duration = 0.58f;                //애니메이션 지속 시간
    private float _yOffset = -2.04f;                //드래그 시, 사용 판정 기준 Y 오프셋
    public bool IsDraggingOverUsingZone;            //카드가 사용 영역 위로 드래그 중인지 여부
    public static bool IsUsing;                     //현재 카드가 사용중인지
    private bool _needTarget;                       //대상 지정이 필요한 카드인지
    private CardData _thisCardData;                 //현재 카드 데이터
    private bool IsCanceled;                        //우클릭으로 사용이 취소됐는지 여부
    private Vector3 _beforeMouseEnterPoz;           //마우스가 카드 위에 올라오기 전 카드의 위치
    private Quaternion _beforeMouseEnterRotation;   //마우스가 카드 위에 올라오기 전 카드의 회전 상태

    private void Start()
    {
        _thisCardData = GetComponent<CardGO>().thisCardData;                // 카드 데이터 초기화
        _needTarget = _thisCardData.NeedTarget;                             // 카드 사용에 타겟이 필요한지 설정
        Transform glowTransform = transform.Find("glow");                   // 카드의 빛나는 테두리 찾기
        _glowingBorder = glowTransform.GetComponent<SpriteRenderer>();      // 스프라이트 렌더러 초기화
    }

    private void Update()
    {
        // 우클릭으로 사용이 취소된 경우
        if (!IsCanceled && IsDraggingOverUsingZone && Input.GetMouseButtonUp(1))
        {
            CancelUse(); // 카드 사용 취소
        }
    }

    //마우스가 진입하면 카드가 위로 살짝 들어올려집니다.
    private void OnMouseEnter()
    {
        transform.DOKill();                                             // 현재 진행 중인 애니메이션 중지
        if (IsDraggingOverUsingZone) return;                            // 카드가 사용 영역 위에서 드래그 중이면 무시

        //여기서부터 마우스가 진입했을때 로직 처리

        //들어 오기 전 위치 저장
        if (_beforeMouseEnterPoz == Vector3.zero)
        {
            _beforeMouseEnterPoz = transform.position; 
            _beforeMouseEnterRotation = transform.rotation; 
        }
        transform.localScale = Vector3.one * 0.7f;
        transform.rotation = Quaternion.identity; 
        GlowBorder(); 
        transform.localPosition = _beforeMouseEnterPoz + Vector3.up * 3; // 카드 위치 조정
        MoveCardFront(); // 카드가 항상 앞에 오도록 정렬
    }


    /// <summary>
    /// 카드를 클릭하면 
    /// </summary>
    private void OnMouseDown()
    {
        IsCanceled = false;                         // 사용 취소 플래그 초기화
        transform.rotation = Quaternion.identity;   // 카드 회전 초기화
        if (DOTween.IsTweening(transform))          // Dotween 애니메이션 진행 중이면 취소
        {
            DOTween.Kill(transform); 
        }
        MoveCardFront();                            
    }

    private void OnMouseDrag()
    {
        // 사용이 취소된 경우 작동하지 않음
        if (IsCanceled) return;

        //사용 가능 범위로 들어올리지 않았다면(일정 높이 아래에 있다면)
        if (!IsDraggingOverUsingZone)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); // 마우스 위치
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition); // 월드 좌표로 변환
            transform.position = objPosition; // 카드의 위치를 마우스 위치에 맞춤
        }

        // 일정 높이 이상 올라갔을 때, 사용 가능 여부 판정
        if (!IsDraggingOverUsingZone && transform.position.y > _yOffset)
        {
            // 카드의 코스트가 현재 에너지보다 많으면 취소
            if (_thisCardData.CardCost > GameManager.Battle.NowEnergy)
            {
                CancelUse(); // 카드 사용 취소
            }
            // 카드의 사용자가 죽어 있으면 취소
            else if (!BattleManager.Inst.GetPlayer(_thisCardData.CardOwner).IsAlive())
            {
                CancelUse(); // 카드 사용 취소
            }
            else
            {
                IsDraggingOverUsingZone = true; // 사용 영역 위에서 드래그 중으로 설정
                if (_needTarget) transform.DOMove(new Vector3(-0, -2.3f, 0), 0.15f); // 타겟 필요 시 위치 이동
            }
        }

        //일정 높이 이상에서 드래그 중일 때
        if (IsDraggingOverUsingZone)
        {
            //대상 지정이 필요하다면 베지어 곡선을 통해 대상을 조준
            if (_needTarget)
            {
                BezierCurveDrawer.Inst.DrawCurveFromScreenBottom(); // 타겟을 조준하기 위한 곡선 그리기
            }
            else
            {
                Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10); // 마우스 위치
                Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition); // 월드 좌표로 변환
                transform.position = objPosition; // 카드의 위치를 마우스 위치에 맞춤
            }
        }
    }

    private void OnMouseUp()
    {
        // 사용이 취소된 경우 작업하지 않음
        if (IsCanceled) return;

        // 타겟이 필요한 카드일 경우
        if (_needTarget)
        {
            // 몬스터를 조준한 경우
            if (IsTargetMonster())
            {
                UseCard(); // 카드 사용
            }
            else
            {
                CancelUse(); // 카드 사용 취소
            }
        }
        // 타겟이 필요 없는 카드일 경우
        else
        {
            UseCard(); // 카드 사용
        }
    }

    private void OnMouseExit()
    {
        // 사용 영역 위에서 드래그 중인 경우 무시
        if (IsDraggingOverUsingZone) { return; }

        gameObject.transform.position = _beforeMouseEnterPoz; // 마우스가 카드 위에서 나가면 원래 위치로 복구
        gameObject.transform.rotation = _beforeMouseEnterRotation; // 원래 회전 상태로 복구
        _beforeMouseEnterPoz = Vector3.zero; // 초기화
        HandManager.Inst.ArrangeCardsOnce(); // 카드 재정렬
        HideBorder();
    }

    /// <summary>
    /// 마우스가 몬스터를 조준하고 있는지 여부
    /// </summary>
    /// <returns></returns>
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
                GameManager.Battle.TargetMonster = hitCollider.GetComponent<UnitBase>(); // 타겟 설정
                return true;
            }
        }
        return false; // 타겟이 없는 경우
    }

    /// <summary>
    /// 카드 테두리가 빛나는 애니메이션
    /// </summary>
    private void GlowBorder()
    {
        if (_thisCardData.CardCost > GameManager.Battle.NowEnergy) return; // 에너지가 부족하면 아무 작업도 하지 않음
        transform.DOScale(new Vector3(0.7f, 0.7f), _duration); // 카드 크기 애니메이션
        Color originalColor = _glowingBorder.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1.0f); // 테두리 색상을 밝게 설정
        _glowingBorder.DOColor(targetColor, _duration); // 테두리 색상 애니메이션
    }

    /// <summary>
    /// 카드 테두리 빛나던걸 끔
    /// </summary>
    private void HideBorder()
    {
        if (IsDraggingOverUsingZone)
        {
            _glowingBorder.DOKill(); // 애니메이션 중지
            _glowingBorder.color = Color.clear; // 테두리 색상 투명하게 설정
            return;
        }
        transform.DOScale(new Vector3(0.5f, 0.5f), _duration); // 카드 크기 애니메이션
        Color originalColor = _glowingBorder.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // 테두리 색상을 투명하게 설정
        _glowingBorder.DOColor(targetColor, _duration); // 테두리 색상 애니메이션
    }

    /// <summary>
    /// 이 카드를 가장 앞으로 옮김
    /// </summary>
    private void MoveCardFront()
    {
        // 스프라이트 렌더러와 캔버스의 정렬 순서 조정
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        for (int j = 0; j < spriteRenderers.Length; j++)
        {
            spriteRenderers[j].sortingOrder = 2000 - j; // 스프라이트 정렬 순서 설정
        }

        Canvas[] canvases = GetComponentsInChildren<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            canvas.sortingOrder = 2000 + 1; // 캔버스 정렬 순서 설정
        }
    }

    /// <summary>
    /// 카드 사용 취소 처리
    /// </summary>
    private void CancelUse()
    {
        transform.DOKill(); // 현재 진행 중인 애니메이션 중지
        IsCanceled = true; // 사용 취소 플래그 설정
        IsDraggingOverUsingZone = false; // 드래그 영역 플래그 초기화
        transform.localScale = Vector3.one * 0.5f; // 카드 크기 축소

        HideBorder(); // 테두리 숨기기
        BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; // 곡선 선 숨기기
        HandManager.Inst.ArrangeCardsOnce(); // 카드 재정렬
    }

    /// <summary>
    /// 카드 사용시 관련 매니저들을 호출하여 처리
    /// </summary>
    private void UseCard()
    {
        // 다른 카드가 사용 중이면 현재 카드 사용 취소
        if (IsUsing) { CancelUse(); return; }

        IsUsing = true; // 카드 사용 중으로 설정
        transform.DOKill(); // 현재 진행 중인 애니메이션 중지
        HideBorder(); 
        HandManager.Inst.CardsInMyHandList.Remove(gameObject); // 손패에서 카드 제거
        StartCoroutine(HandManager.Inst.ArrangeCardsCoroutine()); // 카드 재정렬
        BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; // 곡선 선 숨기기
        CardEffectManager.CurrentCardData = _thisCardData; // 카드 데이터 설정
        CardEffectManager.CurrentCardGO = gameObject; // 카드 게임 오브젝트 설정
        CardEffectManager.Inst.UseCard(); // 카드 사용
        // 하위 모든 스프라이트 렌더러와 TMP 텍스트의 색상을 투명하게 변경
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0); // 스프라이트 색상 투명하게 설정
        }

        var texts = GetComponentsInChildren<TMPro.TMP_Text>();
        foreach (var text in texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0); // 텍스트 색상 투명하게 설정
        }
    }

    private void OnDisable()
    {
        transform.DOKill(); // 현재 진행 중인 애니메이션 중지
        if (BezierCurveDrawer.Inst.lineRenderer != null)
        {
            BezierCurveDrawer.Inst.lineRenderer.positionCount = 0; // 곡선 선 숨기기
        }
        else
        {
            // Null인 경우의 처리(예: 오류 또는 경고 로그 작성)
        }
    }
}
