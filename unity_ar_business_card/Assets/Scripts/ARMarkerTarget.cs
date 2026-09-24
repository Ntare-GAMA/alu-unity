using UnityEngine;
using Vuforia;

/// <summary>
/// Creates the Vuforia Image Target for the business card marker once Vuforia has started,
/// anchors the card layout to it and shows / hides the card depending on the tracking status.
/// </summary>
public class ARMarkerTarget : MonoBehaviour
{
    /// <summary>Marker image to track. Must be imported with Read/Write enabled and no compression.</summary>
    public Texture2D markerTexture;

    /// <summary>Real-world width of the printed marker, in meters.</summary>
    public float printedWidth = 0.15f;

    /// <summary>Name given to the image target created at runtime.</summary>
    public string targetName = "ARBusinessCardMarker";

    /// <summary>Root transform of the business card layout that gets anchored to the marker.</summary>
    public Transform cardAnchor;

    /// <summary>Animator that plays the card intro when the marker is found.</summary>
    public BusinessCardAnimator cardAnimator;

    // Image target created from markerTexture
    private ImageTargetBehaviour target;

    // Whether the card is currently shown
    private bool isVisible;

    // Hide the card and wait for Vuforia to be ready
    private void Awake()
    {
        SetCardVisible(false);
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
    }

    // Unsubscribe from Vuforia and target events
    private void OnDestroy()
    {
        if (VuforiaApplication.Instance != null)
            VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        if (target != null)
            target.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    // Build the image target from the marker texture and parent the card under it
    private void OnVuforiaStarted()
    {
        if (target != null)
            return;

        target = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(markerTexture, printedWidth, targetName);
        if (target == null)
        {
            Debug.LogError("ARMarkerTarget: could not create the image target from " + markerTexture.name);
            return;
        }

        cardAnchor.SetParent(target.transform, false);
        cardAnchor.localPosition = Vector3.zero;
        cardAnchor.localRotation = Quaternion.identity;
        cardAnchor.localScale = Vector3.one;
        target.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    // Show the card only while the marker itself is in view
    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool tracked = status.Status == Status.TRACKED;
        if (tracked == isVisible)
            return;

        SetCardVisible(tracked);
        if (tracked)
            cardAnimator.PlayIntro();
    }

    // Toggle every element of the card at once
    private void SetCardVisible(bool visible)
    {
        isVisible = visible;
        if (cardAnimator != null)
            cardAnimator.SetVisible(visible);
    }
}
