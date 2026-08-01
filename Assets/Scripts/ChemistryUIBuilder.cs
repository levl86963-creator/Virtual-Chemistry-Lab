using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the Submit button and Result canvas at runtime.
/// The Submit button is added under CenterArea, below the two element slots.
/// The Result canvas is a separate World Space canvas positioned to the left
/// of the main table, displaying the balanced reaction equation and compound name.
/// Both elements are created in Awake so that ChemistrySlotManager and
/// ChemistryUIInitializer can find and wire them in Start.
/// </summary>
public class ChemistryUIBuilder : MonoBehaviour
{
    // ChemistryUI canvas is 700px tall at 0.001 scale = 0.7m, centered at
    // local y=0, so its top edge is at local y=0.35m.
    // Place the result panel directly above the ChemistryUI combination panel.
    // Result canvas: 680px at 0.0015 scale ≈ 1.0m tall — larger for readability.
    // Center at 0.35 + 0.05 + 0.34 = 0.74m (5cm gap above the combination UI).
    // Width 1200px at 0.0015 scale ≈ 1.8m wide.
    // Z: 2.2 — moved further forward (closer to the player) for easier reading.
    private const float RESULT_LOCAL_X = 0f;
    private const float RESULT_LOCAL_Y = 0.74f;
    private const float RESULT_LOCAL_Z = 2.2f;
    private const float RESULT_CANVAS_SCALE = 0.0015f;
    private const float RESULT_WIDTH = 1200f;
    private const float RESULT_HEIGHT = 680f;

    private void Awake()
    {
        BuildSubmitButton();
        BuildResultCanvas();
        BuildModelDisplayPoint();
    }

    /// <summary>
    /// Creates the Submit button under CenterArea, positioned below the two slots.
    /// The button starts disabled and is enabled by ChemistrySlotManager when both
    /// slots are filled.
    /// </summary>
    private void BuildSubmitButton()
    {
        var centerArea = transform.Find("CenterArea");
        if (centerArea == null)
        {
            Debug.LogError("[ChemistryUIBuilder] CenterArea not found");
            return;
        }

        // Skip if already created by a previous run.
        if (centerArea.Find("SubmitBtn") != null)
            return;

        var btnObj = new GameObject("SubmitBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(centerArea, false);

        var rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, -170f);
        rt.sizeDelta = new Vector2(220f, 70f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        var image = btnObj.GetComponent<Image>();
        image.color = new Color(0.08f, 0.28f, 0.14f, 0.9f);
        image.raycastTarget = true;

        var button = btnObj.GetComponent<Button>();
        button.interactable = false;
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = new Color(0.08f, 0.28f, 0.14f, 0.9f);
        colors.highlightedColor = new Color(0.12f, 0.38f, 0.18f, 1f);
        colors.pressedColor = new Color(0.04f, 0.18f, 0.08f, 1f);
        colors.selectedColor = new Color(0.1f, 0.32f, 0.16f, 1f);
        colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        // Create the label text child.
        CreateText(btnObj.transform, "Label", "SUBMIT", 28, Vector2.zero,
            new Vector2(200f, 60f), Color.white, TextAnchor.MiddleCenter, false);

        Debug.Log("[ChemistryUIBuilder] Submit button created");
    }

    /// <summary>
    /// Creates the Result canvas as a sibling of ChemistryUI under ChemistryUIHolder.
    /// Positioned directly above the ChemistryUI panel and slightly in front of it
    /// (closer to the player). The holder has a 180-degree Y rotation, so the
    /// canvas is also rotated 180° on Y to face the player.
    /// The canvas starts hidden and is shown by ChemistrySlotManager.Submit().
    /// </summary>
    private void BuildResultCanvas()
    {
        var parent = transform.parent;
        if (parent == null)
        {
            Debug.LogError("[ChemistryUIBuilder] No parent transform found");
            return;
        }

        // Skip if already created.
        if (parent.Find("ResultCanvas") != null)
            return;

        var canvasObj = new GameObject("ResultCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObj.transform.SetParent(parent, false);
        canvasObj.transform.localPosition = new Vector3(RESULT_LOCAL_X, RESULT_LOCAL_Y, RESULT_LOCAL_Z);
        // Rotate 180° on Y to face the player — the holder has 180° Y rotation,
        // so children face away by default. This cancels out so the text is readable.
        canvasObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        canvasObj.transform.localScale = Vector3.one * RESULT_CANVAS_SCALE;

        var rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(RESULT_WIDTH, RESULT_HEIGHT);

        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.planeDistance = 0.1f;
        canvas.sortingOrder = 20;

        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        // Background panel.
        var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);

        var bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchoredPosition = Vector2.zero;
        bgRt.sizeDelta = Vector2.zero;
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.pivot = new Vector2(0.5f, 0.5f);

        var bgImage = bgObj.GetComponent<Image>();
        bgImage.color = new Color(0.04f, 0.06f, 0.1f, 0.95f);
        bgImage.raycastTarget = false;

        // Title text.
        CreateText(canvasObj.transform, "TitleText", "REACTION RESULT", 46,
            new Vector2(0f, 280f), new Vector2(1150f, 70f),
            new Color(0.3f, 0.8f, 1f, 1f), TextAnchor.MiddleCenter, false);

        // Formula text (the balanced equation or no-reaction display).
        CreateText(canvasObj.transform, "FormulaText", "", 56,
            new Vector2(0f, 140f), new Vector2(1150f, 110f),
            Color.white, TextAnchor.MiddleCenter, false);

        // Compound name text (shown for successful reactions).
        CreateText(canvasObj.transform, "CompoundNameText", "", 44,
            new Vector2(0f, -30f), new Vector2(1150f, 70f),
            new Color(0.6f, 1f, 0.6f, 1f), TextAnchor.MiddleCenter, false);

        // Reason text (shown for no-reaction / special cases).
        var reasonText = CreateText(canvasObj.transform, "ReasonText", "", 34,
            new Vector2(0f, -200f), new Vector2(1150f, 260f),
            new Color(0.9f, 0.75f, 0.4f, 1f), TextAnchor.UpperCenter, false);
        reasonText.horizontalOverflow = HorizontalWrapMode.Wrap;
        reasonText.verticalOverflow = VerticalWrapMode.Overflow;

        // Hidden initially.
        canvasObj.SetActive(false);

        Debug.Log("[ChemistryUIBuilder] Result canvas created");
    }

    /// <summary>
    /// Creates an empty GameObject "ModelDisplayPoint" as a sibling of ResultCanvas,
    /// positioned to the right of the result panel. A small dark pedestal cylinder
    /// is placed beneath to give the laboratory display feel.
    /// Ball-and-stick models are spawned here by ChemistrySlotManager on Submit.
    /// </summary>
    private void BuildModelDisplayPoint()
    {
        var parent = transform.parent;
        if (parent == null) return;

        // Skip if already created.
        if (parent.Find("ModelDisplayPoint") != null) return;

        // Position to the right of the ResultCanvas (canvas half-width ≈ 0.9m).
        // Moved down slightly so the model sits beside the lower half of the panel.
        var displayObj = new GameObject("ModelDisplayPoint");
        displayObj.transform.SetParent(parent, false);
        displayObj.transform.localPosition = new Vector3(1.25f, RESULT_LOCAL_Y - 0.3f, RESULT_LOCAL_Z);
        // Rotate 180° on Y to match the holder orientation so models face the player.
        displayObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        displayObj.transform.localScale = Vector3.one;

        // Pedestal: a short wide cylinder for the craft-style display base.
        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "Pedestal";
        pedestal.transform.SetParent(displayObj.transform, false);
        pedestal.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        pedestal.transform.localScale = new Vector3(0.32f, 0.03f, 0.32f);
        var pedRend = pedestal.GetComponent<Renderer>();
        if (pedRend != null)
        {
            var pedMat = new Material(pedRend.sharedMaterial);
            pedMat.color = new Color(0.12f, 0.1f, 0.08f, 1f);
            pedRend.material = pedMat;
        }

        // Small label above pedestal.
        var labelObj = new GameObject("ModelLabel", typeof(RectTransform), typeof(Text));
        labelObj.transform.SetParent(displayObj.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        labelObj.transform.localRotation = Quaternion.identity;
        labelObj.transform.localScale = Vector3.one * 0.003f;

        var labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.sizeDelta = new Vector2(300f, 60f);
        labelRt.anchorMin = new Vector2(0.5f, 0.5f);
        labelRt.anchorMax = new Vector2(0.5f, 0.5f);
        labelRt.pivot = new Vector2(0.5f, 0.5f);

        var labelText = labelObj.GetComponent<Text>();
        labelText.text = "MOLECULAR MODEL";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 28;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = new Color(0.5f, 0.75f, 1f, 1f);
        labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
        labelText.verticalOverflow = VerticalWrapMode.Overflow;
        labelText.raycastTarget = false;

        Debug.Log("[ChemistryUIBuilder] Model display point created");
    }

    /// <summary>
    /// Creates a Text UI element as a child of the given parent.
    /// </summary>
    private Text CreateText(Transform parent, string name, string text, int fontSize,
        Vector2 position, Vector2 size, Color color, TextAnchor alignment, bool raycastTarget)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);

        var rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        var textComp = obj.GetComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = fontSize;
        textComp.fontStyle = FontStyle.Bold;
        textComp.alignment = alignment;
        textComp.color = color;
        textComp.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComp.verticalOverflow = VerticalWrapMode.Overflow;
        textComp.raycastTarget = raycastTarget;

        return textComp;
    }
}
