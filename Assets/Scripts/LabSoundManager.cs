using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Attach to any persistent GameObject (e.g. GameManager).
/// Automatically hooks sound effects to:
///   - Periodic table element tiles (click sound)
///   - Start / Start Experimenting buttons (confirm chord)
///   - Sodium grab/release (pickup whoosh + water splash on drop)
public class LabSoundManager : MonoBehaviour
{
    [Header("UI Sounds")]
    public AudioClip elementClickSound;       // periodic table tile click
    public AudioClip confirmButtonSound;      // Start / Start Experimenting buttons

    [Header("Object Interaction Sounds")]
    public AudioClip grabPickupSound;         // picking up sodium
    public AudioClip waterSplashSound;        // sodium dropped into beaker area

    [Header("Volume")]
    [Range(0f, 1f)] public float uiVolume = 0.6f;
    [Range(0f, 1f)] public float interactionVolume = 0.75f;

    // Internal AudioSource for 2D UI sounds
    private AudioSource _uiSource;

    void Start()
    {
        // Create a dedicated 2D AudioSource for UI clicks (no 3D falloff needed)
        _uiSource = gameObject.AddComponent<AudioSource>();
        _uiSource.spatialBlend = 0f; // fully 2D
        _uiSource.playOnAwake = false;

        StartCoroutine(HookSoundsAfterBuild());
    }

    // Wait one frame so PeriodicTableBuilder has finished creating tiles
    private IEnumerator HookSoundsAfterBuild()
    {
        yield return null; // let Start() on all other scripts finish
        yield return null; // extra frame for tile creation

        HookElementTiles();
        HookConfirmButtons();
        HookSodiumGrab();

        Debug.Log("[LabSoundManager] All sound hooks registered.");
    }

    // ─── Element Tiles ──────────────────────────────────────────────────────

    private void HookElementTiles()
    {
        if (elementClickSound == null) return;

        var tableManager = FindAnyObjectByType<PeriodicTableManager>(FindObjectsInactive.Include);
        if (tableManager == null) return;

        // Find all buttons that are children of the periodic table board
        var buttons = tableManager.GetComponentsInChildren<Button>(true);
        int hooked = 0;
        foreach (var btn in buttons)
        {
            // Only hook buttons that have a SelectElement listener (active tiles)
            if (btn.onClick.GetPersistentEventCount() > 0)
            {
                btn.onClick.AddListener(() => PlayUI(elementClickSound));
                hooked++;
            }
        }
        Debug.Log($"[LabSoundManager] Hooked {hooked} element tile buttons.");
    }

    // ─── Confirm Buttons ────────────────────────────────────────────────────

    private void HookConfirmButtons()
    {
        if (confirmButtonSound == null) return;

        string[] confirmNames = { "StartButton", "StartExperimentingButton" };
        foreach (var name in confirmNames)
        {
            var go = GameObject.Find(name);
            if (go == null) go = FindInactive(name);
            if (go == null) continue;

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => PlayUI(confirmButtonSound));
                Debug.Log($"[LabSoundManager] Hooked confirm sound to: {name}");
            }
        }
    }

    // ─── Sodium Grab / Drop ─────────────────────────────────────────────────

    private void HookSodiumGrab()
    {
        // Find all Substance objects tagged as Sodium
        var substances = FindObjectsByType<Substance>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var s in substances)
        {
            if (s.symbol != "Na") continue;

            // Try to get XRGrabInteractable via reflection to avoid hard dependency
            var grabComp = s.GetComponent("XRGrabInteractable");
            if (grabComp == null) continue;

            // Add our SodiumSoundHook helper
            var hook = s.gameObject.GetComponent<SodiumSoundHook>();
            if (hook == null)
                hook = s.gameObject.AddComponent<SodiumSoundHook>();

            hook.pickupSound = grabPickupSound;
            hook.splashSound = waterSplashSound;
            hook.volume = interactionVolume;
            Debug.Log($"[LabSoundManager] Hooked grab sounds to: {s.gameObject.name}");
        }
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    public void PlayUI(AudioClip clip)
    {
        if (clip == null || _uiSource == null) return;
        _uiSource.PlayOneShot(clip, uiVolume);
    }

    public void PlayAt(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    private GameObject FindInactive(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var g in all)
            if (g.name == name) return g;
        return null;
    }
}
