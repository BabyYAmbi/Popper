using PopperBurst;
using System.Collections;
using UnityEngine;

public class PopperAnimator : MonoBehaviour, IAnimator
{
    [Header("References")]
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private Transform popperBody;
    [SerializeField] private SpriteRenderer popperRenderer;

    [Header("Animation Settings")]
    [SerializeField] private float eyeScaleSpeed = 2f;
    [SerializeField] private float eyeScaleAmount = 0.3f;
    [SerializeField] private float eyeAnimationInterval = 1f;
    [SerializeField] private float pumpSpeed = 1.5f;
    [SerializeField] private float pumpScaleAmount = 0.1f;
    [SerializeField] private float pumpInterval = 2f;
    [SerializeField] private float burstScale = 1.5f;
    [SerializeField] private float burstDuration = 0.3f;

    [Header("Sprite Transition")]
    [SerializeField] private float spriteTransitionDuration = 0.2f;
    [SerializeField] private AnimationCurve spriteTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 leftEyeOriginalScale;
    private Vector3 rightEyeOriginalScale;
    private Vector3 popperOriginalScale;
    private Coroutine idleAnimationCoroutine;
    private bool isAnimating = true;

    void Awake()
    {
        // Store original scales
        if (leftEye) leftEyeOriginalScale = leftEye.localScale;
        if (rightEye) rightEyeOriginalScale = rightEye.localScale;
        if (popperBody) popperOriginalScale = popperBody.localScale;

        // Ensure we have a sprite renderer
        if (!popperRenderer && popperBody)
            popperRenderer = popperBody.GetComponent<SpriteRenderer>();
    }

    public void PlayIdleAnimation()
    {
        StopAllAnimations();
        isAnimating = true;
        idleAnimationCoroutine = StartCoroutine(IdleAnimationRoutine());
    }

    public void PlayTapAnimation()
    {
        StartCoroutine(TapBounceRoutine());
    }

    public void PlayBurstAnimation()
    {
        StopAllAnimations();
        StartCoroutine(BurstAnimationRoutine());
    }

    public void StopAllAnimations()
    {
        isAnimating = false;
        if (idleAnimationCoroutine != null)
        {
            StopCoroutine(idleAnimationCoroutine);
            idleAnimationCoroutine = null;
        }
    }

    // NEW METHOD: Update sprite instead of color
    public void UpdateSprite(Sprite newSprite)
    {
        if (popperRenderer && newSprite)
        {
            StartCoroutine(SpriteTransitionRoutine(newSprite));
        }
    }

    // DEPRECATED: Keep for backwards compatibility
    public void UpdateColor(Color newColor)
    {
        // This method is now deprecated in favor of UpdateSprite
        // Keep it for backwards compatibility but log a warning
        Debug.LogWarning("UpdateColor is deprecated. Use UpdateSprite instead.");
    }

    // Smooth sprite transition animation
    private IEnumerator SpriteTransitionRoutine(Sprite newSprite)
    {
        if (!popperRenderer) yield break;

        Sprite originalSprite = popperRenderer.sprite;
        Vector3 originalScale = popperBody ? popperBody.localScale : Vector3.one;
        Vector3 shrinkScale = originalScale * 0.8f;

        // Shrink down
        float elapsed = 0f;
        while (elapsed < spriteTransitionDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = spriteTransitionCurve.Evaluate(elapsed / (spriteTransitionDuration / 2f));

            if (popperBody)
                popperBody.localScale = Vector3.Lerp(originalScale, shrinkScale, t);

            yield return null;
        }

        // Change sprite at the smallest scale
        popperRenderer.sprite = newSprite;

        // Grow back to original size
        elapsed = 0f;
        while (elapsed < spriteTransitionDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = spriteTransitionCurve.Evaluate(elapsed / (spriteTransitionDuration / 2f));

            if (popperBody)
                popperBody.localScale = Vector3.Lerp(shrinkScale, originalScale, t);

            yield return null;
        }

        // Ensure we end at exactly the right scale
        if (popperBody)
            popperBody.localScale = originalScale;
    }

    // [Rest of the animation methods remain the same...]
    private IEnumerator IdleAnimationRoutine()
    {
        StartCoroutine(EyeBlinkRoutine());
        StartCoroutine(PumpRoutine());

        while (isAnimating)
        {
            yield return null;
        }
    }

    private IEnumerator EyeBlinkRoutine()
    {
        while (isAnimating)
        {
            yield return new WaitForSeconds(eyeAnimationInterval);

            if (!isAnimating) break;

            int pattern = UnityEngine.Random.Range(0, 3);

            switch (pattern)
            {
                case 0:
                    yield return StartCoroutine(AnimateEyesRoutine(true));
                    break;
                case 1:
                    yield return StartCoroutine(AnimateEyesRoutine(false));
                    break;
                case 2:
                    yield return StartCoroutine(BothEyesBlinkRoutine());
                    break;
            }
        }
    }

    private IEnumerator AnimateEyesRoutine(bool leftDown)
    {
        if (!leftEye || !rightEye || !isAnimating) yield break;

        float elapsed = 0f;
        float duration = 1f / eyeScaleSpeed;

        Vector3 leftTargetScale = leftDown ?
            leftEyeOriginalScale * (1f - eyeScaleAmount) :
            leftEyeOriginalScale * (1f + eyeScaleAmount);

        Vector3 rightTargetScale = leftDown ?
            rightEyeOriginalScale * (1f + eyeScaleAmount) :
            rightEyeOriginalScale * (1f - eyeScaleAmount);

        // Scale to target
        while (elapsed < duration && isAnimating)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            leftEye.localScale = Vector3.Lerp(leftEyeOriginalScale, leftTargetScale, t);
            rightEye.localScale = Vector3.Lerp(rightEyeOriginalScale, rightTargetScale, t);

            yield return null;
        }

        // Scale back
        elapsed = 0f;
        while (elapsed < duration && isAnimating)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            leftEye.localScale = Vector3.Lerp(leftTargetScale, leftEyeOriginalScale, t);
            rightEye.localScale = Vector3.Lerp(rightTargetScale, rightEyeOriginalScale, t);

            yield return null;
        }
    }

    private IEnumerator BothEyesBlinkRoutine()
    {
        if (!leftEye || !rightEye || !isAnimating) yield break;

        float duration = 0.15f;
        Vector3 closedScale = new Vector3(1f, 0.1f, 1f);

        // Close eyes
        float elapsed = 0f;
        while (elapsed < duration && isAnimating)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            leftEye.localScale = Vector3.Lerp(leftEyeOriginalScale,
                Vector3.Scale(leftEyeOriginalScale, closedScale), t);
            rightEye.localScale = Vector3.Lerp(rightEyeOriginalScale,
                Vector3.Scale(rightEyeOriginalScale, closedScale), t);

            yield return null;
        }

        // Open eyes
        elapsed = 0f;
        while (elapsed < duration && isAnimating)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            leftEye.localScale = Vector3.Lerp(Vector3.Scale(leftEyeOriginalScale, closedScale),
                leftEyeOriginalScale, t);
            rightEye.localScale = Vector3.Lerp(Vector3.Scale(rightEyeOriginalScale, closedScale),
                rightEyeOriginalScale, t);

            yield return null;
        }
    }

    private IEnumerator PumpRoutine()
    {
        while (isAnimating)
        {
            yield return new WaitForSeconds(pumpInterval);

            if (!popperBody || !isAnimating) continue;

            float elapsed = 0f;
            float duration = 1f / pumpSpeed;
            Vector3 pumpScale = popperOriginalScale * (1f + pumpScaleAmount);

            // Pump up
            while (elapsed < duration / 2f && isAnimating)
            {
                elapsed += Time.deltaTime;
                float t = (elapsed / (duration / 2f));
                popperBody.localScale = Vector3.Lerp(popperOriginalScale, pumpScale, t);
                yield return null;
            }

            // Pump down
            elapsed = 0f;
            while (elapsed < duration / 2f && isAnimating)
            {
                elapsed += Time.deltaTime;
                float t = (elapsed / (duration / 2f));
                popperBody.localScale = Vector3.Lerp(pumpScale, popperOriginalScale, t);
                yield return null;
            }
        }
    }

    private IEnumerator TapBounceRoutine()
    {
        if (!popperBody) yield break;

        float duration = 0.2f;
        Vector3 bounceScale = popperOriginalScale * 1.2f;
        Vector3 startScale = popperBody.localScale;

        // Bounce up
        float elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            popperBody.localScale = Vector3.Lerp(startScale, bounceScale, t);
            yield return null;
        }

        // Bounce back
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            popperBody.localScale = Vector3.Lerp(bounceScale, popperOriginalScale, t);
            yield return null;
        }
    }

    private IEnumerator BurstAnimationRoutine()
    {
        if (!popperBody) yield break;

        float elapsed = 0f;
        Vector3 burstScaleVector = popperOriginalScale * burstScale;

        // Scale up quickly
        while (elapsed < burstDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (burstDuration / 2f);
            popperBody.localScale = Vector3.Lerp(popperOriginalScale, burstScaleVector, t);
            yield return null;
        }

        // Scale down and fade
        elapsed = 0f;
        Color startColor = popperRenderer ? popperRenderer.color : Color.white;
        while (elapsed < burstDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (burstDuration / 2f);
            popperBody.localScale = Vector3.Lerp(burstScaleVector, Vector3.zero, t);

            if (popperRenderer)
            {
                Color fadeColor = startColor;
                fadeColor.a = Mathf.Lerp(1f, 0f, t);
                popperRenderer.color = fadeColor;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }
}