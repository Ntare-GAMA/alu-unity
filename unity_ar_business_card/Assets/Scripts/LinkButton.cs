using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Opens a link (web page, app deep link or mailto:) when its button is pressed,
/// with audible (click) and visual (press bounce) feedback before leaving the app.
/// </summary>
[RequireComponent(typeof(Button))]
public class LinkButton : MonoBehaviour
{
    /// <summary>URL opened when the button is pressed, e.g. https://github.com/user or mailto:me@mail.com.</summary>
    public string url;

    /// <summary>Audio source used to play the click sound.</summary>
    public AudioSource audioSource;

    /// <summary>Click sound played on press.</summary>
    public AudioClip clickClip;

    /// <summary>Scale the button shrinks to while being pressed.</summary>
    public float pressedScale = 0.88f;

    /// <summary>Delay, in seconds, before opening the link so the feedback can be seen and heard.</summary>
    public float openDelay = 0.2f;

    // Button this component listens to
    private Button button;

    // Prevents double taps from opening the link twice
    private bool isOpening;

    // Hook the button click
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPressed);
    }

    // Reset state when the card is shown again
    private void OnEnable()
    {
        isOpening = false;
        transform.localScale = Vector3.one;
    }

    /// <summary>Plays the feedback and opens the link.</summary>
    public void OnPressed()
    {
        if (isOpening || string.IsNullOrEmpty(url))
            return;
        isOpening = true;
        if (audioSource != null && clickClip != null)
            audioSource.PlayOneShot(clickClip);
        StartCoroutine(PressAndOpen());
    }

    // Quick squash then bounce back, then open the link
    private IEnumerator PressAndOpen()
    {
        const float down = 0.07f;
        const float up = 0.13f;
        for (float t = 0f; t < down; t += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(1f, pressedScale, t / down);
            yield return null;
        }
        for (float t = 0f; t < up; t += Time.unscaledDeltaTime)
        {
            float k = t / up;
            float overshoot = Mathf.Sin(k * Mathf.PI) * 0.06f;
            transform.localScale = Vector3.one * (Mathf.Lerp(pressedScale, 1f, k) + overshoot);
            yield return null;
        }
        transform.localScale = Vector3.one;

        float remaining = openDelay - down - up;
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        Application.OpenURL(url);
        isOpening = false;
    }
}
