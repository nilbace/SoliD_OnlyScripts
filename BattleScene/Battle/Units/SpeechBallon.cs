using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechBallon : MonoBehaviour
{
    public TMPro.TMP_Text TMP_text;
    public SpriteRenderer spriteRenderer;

    public IEnumerator SetUPSpeechBalloon(float time, string message)
    {
        TMP_text.text = message;

        float fadeInTime = time * 0.1f;
        float waitTime = time * 0.8f;
        float fadeOutTime = time * 0.1f;

        Vector3 originalScale = transform.localScale;
        Color textOriginalColor = TMP_text.color;
        Color spriteOriginalColor = spriteRenderer.color;

        // Fade in
        for (float t = 0; t < fadeInTime; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeInTime;
            transform.localScale = Vector3.Lerp(originalScale * 0.3f, originalScale, normalizedTime);
            TMP_text.color = new Color(textOriginalColor.r, textOriginalColor.g, textOriginalColor.b, normalizedTime);
            spriteRenderer.color = new Color(spriteOriginalColor.r, spriteOriginalColor.g, spriteOriginalColor.b, normalizedTime);
            yield return null;
        }
        transform.localScale = originalScale;
        TMP_text.color = textOriginalColor;
        spriteRenderer.color = spriteOriginalColor;

        // Wait
        yield return new WaitForSeconds(waitTime);

        // Fade out
        for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeOutTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.2f, normalizedTime);
            TMP_text.color = new Color(textOriginalColor.r, textOriginalColor.g, textOriginalColor.b, 1 - normalizedTime);
            spriteRenderer.color = new Color(spriteOriginalColor.r, spriteOriginalColor.g, spriteOriginalColor.b, 1 - normalizedTime);
            yield return null;
        }

        // After fade out, destroy the object
        Destroy(gameObject);
    }
}
