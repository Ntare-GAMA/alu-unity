using System.Collections;
using UnityEngine;

/// <summary>
/// Animates the business card: it pops up and stands up from the marker, the header slides in,
/// the link buttons pop in one after another and the card then gently floats while idle.
/// </summary>
public class BusinessCardAnimator : MonoBehaviour
{
    /// <summary>Hinge at the bottom edge of the card, rotated to stand the card up.</summary>
    public Transform cardPivot;

    /// <summary>Accent bar that sweeps open across the top of the card.</summary>
    public RectTransform accentBar;

    /// <summary>Header elements (avatar, name, title, divider...) that slide and fade in, in order.</summary>
    public CanvasGroup[] headerElements;

    /// <summary>Button slots that pop in, in order.</summary>
    public RectTransform[] buttonSlots;

    /// <summary>Avatar that pulses gently while the card is idle.</summary>
    public RectTransform avatar;

    /// <summary>Angle, in degrees, the card tilts up from the marker plane.</summary>
    public float standUpAngle = 55f;

    /// <summary>Duration, in seconds, of the pop-up and stand-up motion.</summary>
    public float popDuration = 0.7f;

    /// <summary>Delay, in seconds, between two consecutive elements appearing.</summary>
    public float stagger = 0.08f;

    /// <summary>Height, in meters, of the idle floating motion.</summary>
    public float floatAmplitude = 0.003f;

    // Distance, in canvas units, the header elements slide in from
    private const float SlideDistance = 80f;

    // Rest position of the pivot, captured once
    private Vector3 pivotRestPosition;

    // Rest positions of the header elements, captured once
    private Vector2[] headerRestPositions;

    // Whether rest values were captured
    private bool initialized;

    // Capture rest values of every animated element
    private void Init()
    {
        if (initialized)
            return;
        pivotRestPosition = cardPivot.localPosition;
        headerRestPositions = new Vector2[headerElements.Length];
        for (int i = 0; i < headerElements.Length; i++)
            headerRestPositions[i] = ((RectTransform)headerElements[i].transform).anchoredPosition;
        initialized = true;
    }

    /// <summary>Shows or hides the whole card. Hiding stops any running animation.</summary>
    /// <param name="visible">True to show the card, false to hide it.</param>
    public void SetVisible(bool visible)
    {
        Init();
        if (!visible)
            StopAllCoroutines();
        cardPivot.gameObject.SetActive(visible);
    }

    /// <summary>Plays the intro animation from the start, then the idle loop.</summary>
    public void PlayIntro()
    {
        Init();
        StopAllCoroutines();
        ResetToStart();
        StartCoroutine(IntroRoutine());
    }

    // Put every element in its pre-intro state
    private void ResetToStart()
    {
        cardPivot.localPosition = pivotRestPosition;
        cardPivot.localRotation = Quaternion.identity;
        cardPivot.localScale = Vector3.one * 0.2f;
        accentBar.localScale = new Vector3(0f, 1f, 1f);
        for (int i = 0; i < headerElements.Length; i++)
        {
            headerElements[i].alpha = 0f;
            ((RectTransform)headerElements[i].transform).anchoredPosition = headerRestPositions[i] + Vector2.left * SlideDistance;
        }
        foreach (RectTransform slot in buttonSlots)
            slot.localScale = Vector3.zero;
        avatar.localScale = Vector3.one;
    }

    // Sequence: card pops and stands up, accent sweeps, header slides in, buttons pop in, then idle
    private IEnumerator IntroRoutine()
    {
        for (float t = 0f; t < popDuration; t += Time.deltaTime)
        {
            float k = t / popDuration;
            cardPivot.localScale = Vector3.one * Mathf.LerpUnclamped(0.2f, 1f, EaseOutBack(k));
            cardPivot.localRotation = Quaternion.Euler(-standUpAngle * EaseOutCubic(k), 0f, 0f);
            accentBar.localScale = new Vector3(EaseOutCubic(Mathf.Clamp01(k * 1.4f - 0.4f)), 1f, 1f);
            yield return null;
        }
        cardPivot.localScale = Vector3.one;
        cardPivot.localRotation = Quaternion.Euler(-standUpAngle, 0f, 0f);
        accentBar.localScale = Vector3.one;

        for (int i = 0; i < headerElements.Length; i++)
            StartCoroutine(SlideIn(headerElements[i], headerRestPositions[i], i * stagger, 0.35f));
        float buttonsStart = headerElements.Length * stagger;
        for (int i = 0; i < buttonSlots.Length; i++)
            StartCoroutine(PopIn(buttonSlots[i], buttonsStart + i * stagger, 0.4f));

        yield return new WaitForSeconds(buttonsStart + buttonSlots.Length * stagger + 0.4f);
        StartCoroutine(IdleRoutine());
    }

    // Fade and slide a header element into its rest position
    private IEnumerator SlideIn(CanvasGroup element, Vector2 restPosition, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        RectTransform rect = (RectTransform)element.transform;
        Vector2 from = restPosition + Vector2.left * SlideDistance;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = EaseOutCubic(t / duration);
            element.alpha = k;
            rect.anchoredPosition = Vector2.LerpUnclamped(from, restPosition, k);
            yield return null;
        }
        element.alpha = 1f;
        rect.anchoredPosition = restPosition;
    }

    // Scale a button slot up from zero with a small overshoot
    private IEnumerator PopIn(RectTransform slot, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            slot.localScale = Vector3.one * EaseOutBack(t / duration);
            yield return null;
        }
        slot.localScale = Vector3.one;
    }

    // Gentle float of the card and pulse of the avatar, subtle enough to keep text readable
    private IEnumerator IdleRoutine()
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime;
            cardPivot.localPosition = pivotRestPosition + Vector3.up * (Mathf.Sin(time * 1.6f) * 0.5f + 0.5f) * floatAmplitude;
            avatar.localScale = Vector3.one * (1f + Mathf.Sin(time * 2.4f) * 0.04f);
            yield return null;
        }
    }

    // Decelerating curve
    private static float EaseOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    // Decelerating curve with a small overshoot past 1
    private static float EaseOutBack(float x)
    {
        x = Mathf.Clamp01(x);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
