using System.Collections;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator animator; // Animator 컴포넌트를 참조합니다.

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator != null)
        {
            // Animator의 기본 상태의 애니메이션 클립 이름을 가져옵니다.
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                string animationClipName = clipInfo[0].clip.name;

                // 지정된 애니메이션 클립을 재생합니다.
                animator.Play(animationClipName);

                // 애니메이션 클립의 길이를 가져옵니다.
                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                float clipLength = 0f;

                foreach (AnimationClip clip in clips)
                {
                    if (clip.name == animationClipName)
                    {
                        clipLength = clip.length;
                        break;
                    }
                }

                // 애니메이션 클립 길이 후에 오브젝트 파괴
                StartCoroutine(DestroyAfterTime(clipLength));
            }
            else
            {
                Debug.LogError("Animator에 기본 애니메이션 클립이 설정되어 있지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("Animator가 없습니다. Animator를 추가하거나 참조를 설정하세요.");
        }
    }

    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
