using System.Collections;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator animator; // Animator ������Ʈ�� �����մϴ�.

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator != null)
        {
            // Animator�� �⺻ ������ �ִϸ��̼� Ŭ�� �̸��� �����ɴϴ�.
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                string animationClipName = clipInfo[0].clip.name;

                // ������ �ִϸ��̼� Ŭ���� ����մϴ�.
                animator.Play(animationClipName);

                // �ִϸ��̼� Ŭ���� ���̸� �����ɴϴ�.
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

                // �ִϸ��̼� Ŭ�� ���� �Ŀ� ������Ʈ �ı�
                StartCoroutine(DestroyAfterTime(clipLength));
            }
            else
            {
                Debug.LogError("Animator�� �⺻ �ִϸ��̼� Ŭ���� �����Ǿ� ���� �ʽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogError("Animator�� �����ϴ�. Animator�� �߰��ϰų� ������ �����ϼ���.");
        }
    }

    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
