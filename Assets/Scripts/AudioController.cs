using UnityEngine;
using Vuforia;

public class AudioController1 : MonoBehaviour
{
    private AudioSource currentTrackedAudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
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
        bool isTracked = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;
        AudioSource targetAudio = observer.GetComponentInChildren<AudioSource>(true);

        if (isTracked)
        {
            currentTrackedAudio = targetAudio;
            Debug.Log($"[AudioController] Active Target: {observer.TargetName} | Clip: {targetAudio.clip?.name}");
        }
        else
        {
            if (currentTrackedAudio == targetAudio)
            {
                if (currentTrackedAudio.isPlaying)
                {
                    currentTrackedAudio.Stop();
                }
                currentTrackedAudio = null;
            }
        }
    }
    public void TogglePlayActiveAudio()
    {
        if (currentTrackedAudio == null)
        {
            Debug.LogWarning("[ARAudioPlayer] No AR target is currently tracked on camera.");
            return;
        }

        if (currentTrackedAudio.isPlaying)
        {
            currentTrackedAudio.Stop();
            Debug.Log($"[ARAudioPlayer] Paused: {currentTrackedAudio.clip?.name}");
        }
        else
        {
            currentTrackedAudio.Play();
            Debug.Log($"[ARAudioPlayer] Playing: {currentTrackedAudio.clip?.name}");
        }
    }
}

