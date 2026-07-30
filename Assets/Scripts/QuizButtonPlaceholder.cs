using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quiz button placeholder — sits on the RIGHT side of the lab table next to the H2O+Na button.
/// 
/// Visual state (disabled):
///   • Button image desaturated to gray (alpha 0.45)
///   • Dashed-border Image overlay visible
///   • Label text reads "? Quiz" in gray
///   • Poke/click produces a gentle "not yet" wobble and a tooltip message
/// 
/// Teammate integration:
///   1. Call QuizButtonPlaceholder.Enable() at runtime — OR —
///   2. Set isEnabled = true in the Inspector and hit "Apply" — OR —
///   3. Call the static QuizButtonPlaceholder.EnableQuiz() from anywhere.
///   4. Subscribe to OnQuizActivated to receive the poke event when live.
/// </summary>
public class QuizButtonPlaceholder : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("State")]
    [Tooltip("Flip to true (or call Enable()) when the quiz feature is ready.")]
    public bool isEnabled = false;

    [Header("UI References")]
    [Tooltip("The main button Image that will be tinted gray when disabled.")]
    public Image buttonImage;

    [Tooltip("A separate Image used as the dashed-border overlay (set Sprite to a dashed-rect sprite).")]
    public Image dashedBorderImage;

    [Tooltip("TextMeshPro label on the button face.")]
    public TextMeshProUGUI labelText;

    [Tooltip("Small tooltip text that floats up briefly when the disabled button is poked.")]
    public TextMeshProUGUI tooltipText;

    [Header("Colors")]
    public Color enabledColor  = new Color(0.00f, 0.85f, 0.90f, 1.00f); // active cyan
    public Color disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.45f); // gray, semi-transparent
    public Color dashedBorderEnabled  = new Color(0.00f, 0.85f, 0.90f, 0.80f);
    public Color dashedBorderDisabled = new Color(0.55f, 0.55f, 0.55f, 0.50f);

    [Header("Labels")]
    public string enabledLabel  = "? Quiz";
    public string disabledLabel = "? Quiz";
    public string tooltipMessage = "Coming soon — quiz feature in development.";

    [Header("Wobble (disabled poke feedback)")]
    public float wobbleAngle    = 4f;
    public float wobbleDuration = 0.35f;

    [Header("Audio")]
    [Tooltip("Played when the disabled button is poked — a short 'denied' tone.")]
    public AudioClip deniedSound;
    [Range(0f, 1f)] public float deniedVolume = 0.6f;

    // ── Events — teammate hooks in here ───────────────────────────────────────
    /// <summary>Fired when the quiz button is poked while enabled.</summary>
    public static System.Action OnQuizActivated;

    // ── Static convenience ────────────────────────────────────────────────────
    private static QuizButtonPlaceholder _instance;
    /// <summary>Enable the quiz button from anywhere in the project.</summary>
    public static void EnableQuiz()
    {
        if (_instance != null) _instance.Enable();
        else Debug.LogWarning("[QuizButton] No QuizButtonPlaceholder found in scene.");
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    private bool _wobbling = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake() => _instance = this;

    void Start() => ApplyVisualState();

    void OnDestroy() { if (_instance == this) _instance = null; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Enable the quiz button — called by teammate when quiz feature is ready.
    /// </summary>
    public void Enable()
    {
        isEnabled = true;
        ApplyVisualState();
        Debug.Log("[QuizButton] Quiz button ENABLED.");
    }

    /// <summary>
    /// Disable the quiz button again (e.g. after quiz completes).
    /// </summary>
    public void Disable()
    {
        isEnabled = false;
        ApplyVisualState();
    }

    /// <summary>
    /// Call this from the button's UnityEvent (UI Button.onClick or XR Interactable event).
    /// </summary>
    public void OnButtonPoked()
    {
        if (!isEnabled)
        {
            HandleDisabledPoke();
        }
        else
        {
            HandleEnabledPoke();
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void ApplyVisualState()
    {
        // Main button tint
        if (buttonImage != null)
            buttonImage.color = isEnabled ? enabledColor : disabledColor;

        // Dashed border
        if (dashedBorderImage != null)
        {
            dashedBorderImage.gameObject.SetActive(true);
            dashedBorderImage.color = isEnabled ? dashedBorderEnabled : dashedBorderDisabled;
        }

        // Label
        if (labelText != null)
        {
            labelText.text = isEnabled ? enabledLabel : disabledLabel;
            labelText.color = isEnabled ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.8f);
        }

        // Tooltip hidden by default
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);

        // Pulse only when enabled
        var pulse = GetComponent<PulseButton>();
        if (pulse != null) pulse.enabled = isEnabled;
    }

    private void HandleDisabledPoke()
    {
        // Play denied sound
        if (deniedSound != null)
        {
            AudioSource src = GetComponent<AudioSource>();
            if (src == null) src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.PlayOneShot(deniedSound, deniedVolume);
        }

        // Show tooltip
        if (tooltipText != null)
        {
            tooltipText.text = tooltipMessage;
            tooltipText.gameObject.SetActive(true);
        }

        // Wobble animation
        if (!_wobbling)
            StartCoroutine(WobbleRoutine());
    }

    private void HandleEnabledPoke()
    {
        Debug.Log("[QuizButton] Quiz activated!");
        OnQuizActivated?.Invoke();
    }

    private IEnumerator WobbleRoutine()
    {
        _wobbling = true;
        float elapsed = 0f;
        Quaternion originalRot = transform.localRotation;

        while (elapsed < wobbleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / wobbleDuration;
            // Damped sine wobble
            float angle = Mathf.Sin(t * Mathf.PI * 5f) * wobbleAngle * (1f - t);
            transform.localRotation = originalRot * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        transform.localRotation = originalRot;
        _wobbling = false;

        // Hide tooltip after a moment
        yield return new WaitForSeconds(1.8f);
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    // Validate changes made directly in the Inspector
    void OnValidate() => ApplyVisualState();

    [UnityEditor.CustomEditor(typeof(QuizButtonPlaceholder))]
    public class QuizButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var btn = (QuizButtonPlaceholder)target;

            UnityEditor.EditorGUILayout.Space(6);
            UnityEditor.EditorGUILayout.LabelField("— Teammate Integration Test —",
                UnityEditor.EditorStyles.boldLabel);

            UnityEditor.EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            if (GUILayout.Button("▶  Simulate: Poke Disabled Button"))
                btn.OnButtonPoked();

            if (GUILayout.Button("✅  Enable Quiz Button"))
                btn.Enable();

            if (GUILayout.Button("🚫  Disable Quiz Button"))
                btn.Disable();

            UnityEditor.EditorGUI.EndDisabledGroup();

            if (!Application.isPlaying)
                UnityEditor.EditorGUILayout.HelpBox(
                    "Enter Play Mode to test button interactions.", 
                    UnityEditor.MessageType.Info);
        }
    }
#endif
}
