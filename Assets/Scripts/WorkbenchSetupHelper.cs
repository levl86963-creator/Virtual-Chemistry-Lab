using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor helper — validates and corrects the Y-heights of the InfoSlab and
/// Glass Panel so they match the spec:
///
///   • InfoSlab (floating HUD):  ~1.4 m world-space Y
///   • Glass Panel (table top):  ~0.9 m world-space Y
///
/// Usage
/// -----
///   1. Attach this component to any convenient scene object (e.g. "WorkbenchManager").
///   2. Drag in the InfoSlab GameObject and GlassPanel GameObject.
///   3. In the Inspector, click "Apply Heights (Editor)".
///   4. This script does nothing at runtime; it is stripped from builds.
///
/// The script also validates heights in OnValidate() and warns in the Console
/// when objects drift outside the allowed tolerance.
/// </summary>
[ExecuteAlways]   // runs OnValidate in Edit mode too
public class WorkbenchSetupHelper : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("The InfoSlab GameObject (world-space Canvas that follows the player head).")]
    public GameObject infoSlabObject;

    [Tooltip("The GlassPanel GameObject sitting on top of the lab table.")]
    public GameObject glassPanelObject;

    [Header("Target Heights (world-space Y)")]
    [Tooltip("InfoSlab should float at approximately this height.")]
    public float infoSlabTargetY  = 1.4f;

    [Tooltip("Glass Panel should sit at approximately this height (table surface ~0.9 m).")]
    public float glassPanelTargetY = 0.9f;

    [Header("Tolerance")]
    [Tooltip("How far (metres) from the target before a warning is shown.")]
    public float warnTolerance = 0.05f;

    // ── Runtime: this component does nothing in a build ───────────────────────
    void Awake()
    {
#if !UNITY_EDITOR
        // Destroy the component in builds — pure editor utility
        Destroy(this);
#endif
    }

    // ── Editor validation ─────────────────────────────────────────────────────
#if UNITY_EDITOR
    void OnValidate()
    {
        CheckHeights(applyFix: false);
    }

    // ── Public methods called by the custom Inspector ─────────────────────────

    /// <summary>Apply the target Y positions to both objects immediately.</summary>
    public void ApplyHeights()
    {
        bool changed = false;

        if (infoSlabObject != null)
        {
            Undo.RecordObject(infoSlabObject.transform, "Set InfoSlab Height");
            Vector3 p = infoSlabObject.transform.position;
            p.y = infoSlabTargetY;
            infoSlabObject.transform.position = p;
            changed = true;
            Debug.Log($"[WorkbenchSetup] InfoSlab Y set to {infoSlabTargetY:F3} m");
        }
        else Debug.LogWarning("[WorkbenchSetup] infoSlabObject is not assigned.");

        if (glassPanelObject != null)
        {
            Undo.RecordObject(glassPanelObject.transform, "Set GlassPanel Height");
            Vector3 p = glassPanelObject.transform.position;
            p.y = glassPanelTargetY;
            glassPanelObject.transform.position = p;
            changed = true;
            Debug.Log($"[WorkbenchSetup] GlassPanel Y set to {glassPanelTargetY:F3} m");
        }
        else Debug.LogWarning("[WorkbenchSetup] glassPanelObject is not assigned.");

        if (changed) SceneManagement();
    }

    /// <summary>Check heights and optionally report mismatches in the Console.</summary>
    public void CheckHeights(bool applyFix = false)
    {
        if (infoSlabObject != null)
        {
            float y   = infoSlabObject.transform.position.y;
            float err = Mathf.Abs(y - infoSlabTargetY);
            if (err > warnTolerance)
            {
                Debug.LogWarning(
                    $"[WorkbenchSetup] InfoSlab Y = {y:F3} m  (target {infoSlabTargetY:F3} m, " +
                    $"delta {err:F3} m). Click 'Apply Heights' to fix.",
                    infoSlabObject);
            }
        }

        if (glassPanelObject != null)
        {
            float y   = glassPanelObject.transform.position.y;
            float err = Mathf.Abs(y - glassPanelTargetY);
            if (err > warnTolerance)
            {
                Debug.LogWarning(
                    $"[WorkbenchSetup] GlassPanel Y = {y:F3} m  (target {glassPanelTargetY:F3} m, " +
                    $"delta {err:F3} m). Click 'Apply Heights' to fix.",
                    glassPanelObject);
            }
        }
    }

    // Scene is dirtied so the change shows up in version control
    private static void SceneManagement()
    {
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    // ── Custom Inspector ──────────────────────────────────────────────────────
    [CustomEditor(typeof(WorkbenchSetupHelper))]
    public class WorkbenchSetupHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var helper = (WorkbenchSetupHelper)target;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── Actions ──────────────────────────",
                EditorStyles.boldLabel);

            // Height display
            if (helper.infoSlabObject != null)
            {
                float y = helper.infoSlabObject.transform.position.y;
                EditorGUILayout.LabelField(
                    "InfoSlab current Y",
                    $"{y:F3} m  (target {helper.infoSlabTargetY:F3} m)",
                    EditorStyles.helpBox);
            }

            if (helper.glassPanelObject != null)
            {
                float y = helper.glassPanelObject.transform.position.y;
                EditorGUILayout.LabelField(
                    "GlassPanel current Y",
                    $"{y:F3} m  (target {helper.glassPanelTargetY:F3} m)",
                    EditorStyles.helpBox);
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button("✅  Apply Heights to Scene Objects"))
                helper.ApplyHeights();

            if (GUILayout.Button("🔍  Check Heights (log warnings)"))
                helper.CheckHeights(applyFix: false);

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "InfoSlab: floats at ~1.4 m (follows player head — height is an initial snap point).\n" +
                "GlassPanel: sits flush on lab table at ~0.9 m (table surface height).\n\n" +
                "This component is editor-only and is automatically stripped from builds.",
                MessageType.Info);
        }
    }
#endif
}
