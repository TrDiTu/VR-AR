using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class AudioToggleController : MonoBehaviour
{
    [Header("Target Icon")]
    [SerializeField] private Image iconImage;

    [Header("Icon Sprites")]
    [SerializeField] private Sprite unmuteIcon;
    [SerializeField] private Sprite muteIcon;

    [Header("Icon Sizing")]
    [Tooltip("Enable to force custom pixel dimensions on the child icon only")]
    [SerializeField] private bool useCustomIconSize = true;

    [SerializeField] private Vector2 iconSize = new Vector2(32f, 32f);

    [Tooltip("Preserve aspect ratio to prevent stretching/distortion")]
    [SerializeField] private bool preserveAspect = true;

    private bool isMuted = false;

    private void Start()
    {
        ValidateIconReference();
        isMuted = Mathf.Approximately(AudioListener.volume, 0f);
        ApplyIconVisuals();
    }

    private void OnValidate()
    {
        ValidateIconReference();
        ApplyIconVisuals();
    }

    private void ValidateIconReference()
    {
        // If not assigned, search specifically in child objects first to avoid grabbing the button itself
        if (iconImage == null)
        {
            Transform child = transform.Find("Icon");
            if (child != null)
            {
                iconImage = child.GetComponent<Image>();
            }
            else
            {
                // Look for any child Image that is not on this root GameObject
                Image[] images = GetComponentsInChildren<Image>(true);
                foreach (Image img in images)
                {
                    if (img.gameObject != this.gameObject)
                    {
                        iconImage = img;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attach this to your UI Button's OnClick() event.
    /// </summary>
    public void ToggleAudio()
    {
        isMuted = !isMuted;
        ApplyGlobalMute();
        ApplyIconVisuals();
    }

    private void ApplyGlobalMute()
    {
        // 1. Mutes all standard scene audio (AudioSources on AR targets, SFX, 3D sound)
        AudioListener.volume = isMuted ? 0f : 1f;

        // 2. Safeguard for VideoPlayers set to Direct audio output
        VideoPlayer[] players = FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None);
        foreach (VideoPlayer vp in players)
        {
            if (vp != null && vp.canSetDirectAudioVolume)
            {
                vp.SetDirectAudioMute(0, isMuted);
            }
        }

        Debug.Log($"[AudioToggle] Global Audio Muted: {isMuted}");
    }

    private void ApplyIconVisuals()
    {
        if (iconImage == null) return;

        // Prevent accidental mutation of the button itself
        if (iconImage.gameObject == this.gameObject)
        {
            Debug.LogWarning("[AudioToggle] 'Icon Image' is set to the Button itself. Please assign a separate child Image object to avoid altering button dimensions.", this);
            return;
        }

        // Swap sprite on the child icon
        iconImage.sprite = isMuted ? muteIcon : unmuteIcon;
        iconImage.preserveAspect = preserveAspect;

        // Resize ONLY the child icon's rect
        if (useCustomIconSize)
        {
            RectTransform iconRect = iconImage.rectTransform;
            if (iconRect != null)
            {
                iconRect.sizeDelta = iconSize;
            }
        }
    }
}