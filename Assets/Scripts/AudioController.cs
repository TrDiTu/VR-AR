using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class AudioToggleController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite unmuteIcon;
    [SerializeField] private Sprite muteIcon;

    [Header("Manual Sizing")]
    [Tooltip("Enable to manually force custom width and height on the icon")]
    [SerializeField] private bool useCustomSize = true;

    [Tooltip("Exact width (X) and height (Y) in pixels")]
    [SerializeField] private Vector2 iconSize = new Vector2(40f, 40f);

    [Tooltip("Keep aspect ratio intact within the custom size boundaries")]
    [SerializeField] private bool preserveAspect = true;

    [Header("AR References")]
    [SerializeField] private VuforiaVideoReplayer vuforiaReplayer;

    private bool isMuted = false;

    private void Start()
    {
        InitializeComponents();
        UpdateIcon();
    }

    // Runs in the Unity Editor when you change values in the Inspector
    private void OnValidate()
    {
        InitializeComponents();
        UpdateIcon();
    }

    private void InitializeComponents()
    {
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }
    }

    public void ToggleAudio()
    {
        isMuted = !isMuted;
        ApplyMute();
        UpdateIcon();
    }

    private void ApplyMute()
    {
        VideoPlayer[] players = FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None);
        AudioListener.volume = isMuted ? 0f : 1f;
        Debug.Log($"[AudioToggle] Mute state: {isMuted}");
    }

    private void UpdateIcon()
    {
        if (buttonImage == null) return;

        // Swap Sprite
        buttonImage.sprite = isMuted ? muteIcon : unmuteIcon;
        buttonImage.preserveAspect = preserveAspect;

        // Apply manual pixel size to the RectTransform
        if (useCustomSize)
        {
            RectTransform rect = buttonImage.rectTransform;
            if (rect != null)
            {
                rect.sizeDelta = iconSize;
            }
        }
    }
}