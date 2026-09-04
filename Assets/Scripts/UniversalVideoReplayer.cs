using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Vuforia;

public class VuforiaVideoReplayer : MonoBehaviour
{
    // The VideoPlayer currently tracked and visible on camera
    private VideoPlayer currentTrackedPlayer;

    private void OnEnable()
    {
        // Find all Vuforia target observers in the scene (Image Targets, Model Targets, etc.)
        ObserverBehaviour[] observers = FindObjectsByType<ObserverBehaviour>(FindObjectsSortMode.None);

        foreach (ObserverBehaviour observer in observers)
        {
            observer.OnTargetStatusChanged += HandleTargetStatusChanged;
        }
    }

    private void OnDisable()
    {
        ObserverBehaviour[] observers = FindObjectsByType<ObserverBehaviour>(FindObjectsSortMode.None);

        foreach (ObserverBehaviour observer in observers)
        {
            observer.OnTargetStatusChanged -= HandleTargetStatusChanged;
        }
    }

    private void HandleTargetStatusChanged(ObserverBehaviour observer, TargetStatus status)
    {
        // Find the VideoPlayer attached to this target or in its child Quads
        VideoPlayer targetPlayer = observer.GetComponentInChildren<VideoPlayer>(true);
        if (targetPlayer == null) return;

        // Check if Vuforia is actively tracking this target
        bool isTracked = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;

        if (isTracked)
        {
            currentTrackedPlayer = targetPlayer;
            Debug.Log($"[VuforiaVideo] Target Found: {observer.TargetName}. Active Video: {targetPlayer.gameObject.name}");

            // Start or resume playing when tracked
            if (!targetPlayer.isPlaying)
            {
                targetPlayer.Play();
            }
        }
        else
        {
            // Pause playback when camera loses view of the target
            if (targetPlayer.isPlaying)
            {
                targetPlayer.Pause();
            }

            // Clear the active reference if this was the current target
            if (currentTrackedPlayer == targetPlayer)
            {
                currentTrackedPlayer = null;
                Debug.Log($"[VuforiaVideo] Target Lost: {observer.TargetName}");
            }
        }
    }

    /// <summary>
    /// Attach this method to your UI Button's OnClick() event.
    /// </summary>
    public void ReplayCurrentVideo()
    {
        if (currentTrackedPlayer != null)
        {
            Debug.Log($"[VuforiaVideo] Replaying: {currentTrackedPlayer.gameObject.name}");
            currentTrackedPlayer.frame = 0;
            currentTrackedPlayer.Play();
        }
        else
        {
            Debug.LogWarning("[VuforiaVideo] No AR target is currently being tracked to replay.");
        }
    }
}