using UnityEngine;
using Vuforia;

/// <summary>
/// Creates a Vuforia Image Target for each business card marker once Vuforia has started,
/// anchors the card layout to whichever marker is tracked and shows / hides the card
/// depending on the tracking status.
/// </summary>
public class ARMarkerTarget : MonoBehaviour
{
    /// <summary>Marker images to track. Each must be imported with Read/Write enabled and no compression.</summary>
    public Texture2D[] markerTextures;

    /// <summary>Width of each marker in scene units, one entry per texture in markerTextures.</summary>
    public float[] printedWidths;

    /// <summary>Root transform of the business card layout that gets anchored to the tracked marker.</summary>
    public Transform cardAnchor;

    /// <summary>Animator that plays the card intro when a marker is found.</summary>
    public BusinessCardAnimator cardAnimator;

    // Image targets created from markerTextures
    private ImageTargetBehaviour[] targets;

    // Target the card is currently shown on, null while hidden
    private ObserverBehaviour currentTarget;

    // Hide the card and wait for Vuforia to be ready
    private void Awake()
    {
        cardAnimator.SetVisible(false);
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
    }

    // Unsubscribe from Vuforia and target events
    private void OnDestroy()
    {
        if (VuforiaApplication.Instance != null)
            VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        if (targets == null)
            return;
        foreach (ImageTargetBehaviour target in targets)
        {
            if (target != null)
                target.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    // Build one image target per marker texture
    private void OnVuforiaStarted()
    {
        if (targets != null)
            return;

        targets = new ImageTargetBehaviour[markerTextures.Length];
        for (int i = 0; i < markerTextures.Length; i++)
        {
            Texture2D texture = markerTextures[i];
            targets[i] = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(texture, printedWidths[i], texture.name);
            if (targets[i] == null)
            {
                Debug.LogError("ARMarkerTarget: could not create the image target from " + texture.name);
                continue;
            }
            targets[i].OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    // Show the card on a marker while it is in view, hide it when that marker is lost
    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool tracked = status.Status == Status.TRACKED;
        if (tracked && currentTarget == null)
        {
            currentTarget = behaviour;
            cardAnchor.SetParent(behaviour.transform, false);
            cardAnchor.localPosition = Vector3.zero;
            cardAnchor.localRotation = Quaternion.identity;
            cardAnchor.localScale = Vector3.one;
            cardAnimator.SetVisible(true);
            cardAnimator.PlayIntro();
        }
        else if (!tracked && behaviour == currentTarget)
        {
            currentTarget = null;
            cardAnimator.SetVisible(false);
        }
    }
}
