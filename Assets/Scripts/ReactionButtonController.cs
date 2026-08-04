using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to the "H2O + Na" button on the lab table.
/// Supports XR poke/select interaction AND a plain public method for UI Button.onClick.
/// Loads Na into Slot A and H2O into Slot B, then lets ReactionWorkbench run its reaction.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class ReactionButtonController : MonoBehaviour
{
    [Header("Element Data — drag in ScriptableObject assets")]
    [Tooltip("Sodium (Na) — drag Element_Na.asset here")]
    public ElementData elementA_Na;

    [Tooltip("Water (H2O) — drag the H2O ElementData asset here")]
    public ElementData elementB_H2O;

    [Header("Visual Feedback")]
    [Tooltip("The MeshRenderer of the physical button model")]
    public Renderer buttonRenderer;
    public Color normalColor  = new Color(0.00f, 0.85f, 0.90f, 1f); // cyan
    public Color pressedColor = new Color(0.80f, 1.00f, 1.00f, 1f); // bright flash
    public float flashDuration = 0.15f;

    [Header("Audio")]
    public AudioClip pressSound;

    // ── internals ──────────────────────────────────────────────────────────────
    private XRSimpleInteractable _interactable;
    private MaterialPropertyBlock _mpb;
    private bool _cooling = false;          // prevent double-fire during cooldown

    // ── lifecycle ──────────────────────────────────────────────────────────────
    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _mpb = new MaterialPropertyBlock();
        ApplyColor(normalColor);
    }

    void OnEnable()
    {
        // XRI poke and select both route here
        _interactable.selectEntered.AddListener(OnSelectEntered);
        _interactable.activated.AddListener(OnActivated);
    }

    void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(OnSelectEntered);
        _interactable.activated.RemoveListener(OnActivated);
    }

    // ── XRI event callbacks ────────────────────────────────────────────────────
    private void OnSelectEntered(SelectEnterEventArgs _) => TriggerReaction();
    private void OnActivated(ActivateEventArgs _) => TriggerReaction();

    // ── public entry point (also works with UI Button.onClick in editor) ───────
    /// <summary>
    /// Fires the H2O + Na reaction on the workbench.
    /// Safe to call from a UI Button, an XR event, or test code.
    /// </summary>
    public void TriggerReaction()
    {
        if (_cooling) return;

        ReactionWorkbench wb = ReactionWorkbench.Instance;
        if (wb == null)
        {
            Debug.LogWarning("[ReactionButton] ReactionWorkbench.Instance not found.");
            return;
        }

        _cooling = true;
        StartCoroutine(FlashButton());
        PlayPressSound();

        // ── Step 1: load Na into Slot A ────────────────────────────────────────
        if (elementA_Na != null)
        {
            wb.PrepareElementA(elementA_Na);
        }
        else
        {
            Debug.LogWarning("[ReactionButton] elementA_Na is not assigned — Slot A not set.");
        }

        // ── Step 2: spawn H2O atom at Slot B and drop it ───────────────────────
        if (elementB_H2O != null && wb.slotB != null)
        {
            GameObject atomB = SpawnAtomB(wb.slotB.position);
            wb.OnAtomDroppedInSlot(atomB, "B");
        }
        else
        {
            if (elementB_H2O == null)
                Debug.LogWarning("[ReactionButton] elementB_H2O is not assigned — reaction will not fire.");
            if (wb.slotB == null)
                Debug.LogError("[ReactionButton] ReactionWorkbench.slotB is null — assign it in the Inspector.");
        }

        // cooldown = reaction duration + 1 s buffer
        float cooldown = wb.reactionDuration + 1f;
        StartCoroutine(ResetCooldown(cooldown));
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates the H2O atom model at <paramref name="worldPos"/>.
    /// Falls back to a minimal placeholder if no prefab is set on the ElementData.
    /// </summary>
    private GameObject SpawnAtomB(Vector3 worldPos)
    {
        if (elementB_H2O.atomModelPrefab != null)
        {
            GameObject go = Instantiate(elementB_H2O.atomModelPrefab, worldPos, Quaternion.identity);
            var model = go.GetComponent<AtomModel>();
            if (model != null) model.data = elementB_H2O;
            return go;
        }

        // No prefab — build a minimal kinematic stand-in
        Debug.LogWarning("[ReactionButton] elementB_H2O has no atomModelPrefab; using placeholder.");
        GameObject placeholder = new GameObject($"Atom_{elementB_H2O.symbol}");
        placeholder.transform.position = worldPos;

        var rb  = placeholder.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;

        var col = placeholder.AddComponent<BoxCollider>();
        col.size = Vector3.one * 0.08f;

        var atomModel      = placeholder.AddComponent<AtomModel>();
        atomModel.data     = elementB_H2O;

        return placeholder;
    }

    private void PlayPressSound()
    {
        if (pressSound == null) return;
        AudioSource src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f; // 3-D: this is a physical table button
        src.PlayOneShot(pressSound);
    }

    private IEnumerator FlashButton()
    {
        ApplyColor(pressedColor);
        yield return new WaitForSeconds(flashDuration);
        ApplyColor(normalColor);
    }

    private IEnumerator ResetCooldown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _cooling = false;
    }

    private void ApplyColor(Color c)
    {
        if (buttonRenderer == null) return;
        buttonRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", c); // URP / HDRP Lit
        _mpb.SetColor("_Color",     c); // Built-in fallback
        buttonRenderer.SetPropertyBlock(_mpb);
    }

    // ── Editor test button ─────────────────────────────────────────────────────
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(ReactionButtonController))]
    public class ReactionButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UnityEditor.EditorGUILayout.Space(6);
            UnityEditor.EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            if (GUILayout.Button("▶  Test: Trigger Reaction (Play Mode only)"))
                ((ReactionButtonController)target).TriggerReaction();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }
#endif
}
