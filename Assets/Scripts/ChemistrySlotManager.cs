using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manages element selection for the chemistry experiment UI.
/// Receives button clicks from the element panels and displays
/// the selected element symbol in the corresponding slot.
/// Left panel buttons populate Slot A; right panel buttons populate Slot B.
/// Supports Enter key / mouse left-click to confirm the button currently
/// under the mouse cursor, and Backspace/Delete to clear the slots.
/// When both slots are filled, the Submit button becomes interactable.
/// Clicking Submit looks up the reaction and displays the result on
/// the ResultCanvas panel.
/// </summary>
public class ChemistrySlotManager : MonoBehaviour
{
    private const string SLOT_A_PATH = "CenterArea/SlotA/ElementText";
    private const string SLOT_B_PATH = "CenterArea/SlotB/ElementText";
    private const string SUBMIT_BUTTON_PATH = "CenterArea/SubmitBtn";

    private Text _slotAText;
    private Text _slotBText;
    private Button _submitButton;

    private string _selectedA = "";
    private string _selectedB = "";

    // Tracks the button currently hovered by the mouse for Enter key support.
    private Button _hoveredButton;

    // Result panel references (ResultCanvas is a sibling of ChemistryUI under ChemistryUIHolder).
    private GameObject _resultCanvas;
    private Text _resultFormulaText;
    private Text _resultNameText;
    private Text _resultReasonText;

    // 3D model display.
    private Transform _modelDisplayPoint;
    private GameObject _currentModel;

    /// <summary>Constants for no-reaction / special-case display text.</summary>
    private const string NoReaction = "No Reaction";
    private const string NoDirectReaction = "No Direct Reaction";

    /// <summary>
    /// Reaction database mapping (metal, nonMetal) to (balanced equation, compound name, model type).
    /// Unicode subscript characters are used for chemical formulas.
    /// </summary>
    private static readonly Dictionary<(string, string), (string Equation, string Name, CompoundModelType ModelType)> Reactions = new()
    {
        { ("Li", "S"),  ("2Li + S \u2192 Li\u2082S",          "Lithium Sulfide",       CompoundModelType.AlkaliSulfide) },
        { ("Li", "Cl"), ("2Li + Cl\u2082 \u2192 2LiCl",       "Lithium Chloride",      CompoundModelType.AlkaliChloride) },
        { ("Li", "O"),  ("4Li + O\u2082 \u2192 2Li\u2082O",   "Lithium Oxide",         CompoundModelType.AlkaliOxide) },
        { ("Na", "S"),  ("2Na + S \u2192 Na\u2082S",          "Sodium Sulfide",        CompoundModelType.AlkaliSulfide) },
        { ("Na", "Cl"), ("2Na + Cl\u2082 \u2192 2NaCl",       "Sodium Chloride",       CompoundModelType.AlkaliChloride) },
        { ("Na", "O"),  ("4Na + O\u2082 \u2192 2Na\u2082O",   "Sodium Oxide",          CompoundModelType.AlkaliOxide) },
        { ("K", "S"),   ("2K + S \u2192 K\u2082S",            "Potassium Sulfide",     CompoundModelType.AlkaliSulfide) },
        { ("K", "Cl"),  ("2K + Cl\u2082 \u2192 2KCl",         "Potassium Chloride",    CompoundModelType.AlkaliChloride) },
        { ("K", "O"),   ("4K + O\u2082 \u2192 2K\u2082O",     "Potassium Oxide",       CompoundModelType.AlkaliOxide) },
        { ("Mg", "S"),  ("Mg + S \u2192 MgS",                 "Magnesium Sulfide",     CompoundModelType.AlkalineSulfide) },
        { ("Mg", "Cl"), ("Mg + Cl\u2082 \u2192 MgCl\u2082",   "Magnesium Chloride",    CompoundModelType.AlkalineChloride) },
        { ("Mg", "O"),  ("2Mg + O\u2082 \u2192 2MgO",         "Magnesium Oxide",       CompoundModelType.AlkalineOxide) },
        { ("Ca", "S"),  ("Ca + S \u2192 CaS",                 "Calcium Sulfide",       CompoundModelType.AlkalineSulfide) },
        { ("Ca", "Cl"), ("Ca + Cl\u2082 \u2192 CaCl\u2082",   "Calcium Chloride",      CompoundModelType.AlkalineChloride) },
        { ("Ca", "O"),  ("2Ca + O\u2082 \u2192 2CaO",         "Calcium Oxide",         CompoundModelType.AlkalineOxide) },
    };

    /// <summary>
    /// No-reaction / special-case database mapping element pairs to
    /// (display text, reason). Checked when no successful reaction is found.
    /// Unicode subscript characters are used for chemical formulas.
    /// </summary>
    private static readonly Dictionary<(string, string), (string Display, string Reason)> NoReactions = new()
    {
        // --- Metal + Metal pairs (no compound formation) ---
        { ("Li", "Na"), (NoReaction,
            "Both are metals and cannot form an ionic or covalent bond; they only form a metallic alloy.") },
        { ("Li", "K"),  (NoReaction,
            "Both are alkali metals with identical bonding tendencies; no chemical reaction occurs.") },
        { ("Li", "Mg"), (NoReaction,
            "Both are metals and do not react with each other under normal laboratory conditions.") },
        { ("Li", "Ca"), (NoReaction,
            "Both are metals; they will only mix physically as an alloy without forming a compound.") },
        { ("Na", "K"),  (NoReaction,
            "Both are alkali metals; mixing them produces a liquid metal alloy (NaK) but no chemical compound.") },
        { ("Na", "Mg"), (NoReaction,
            "Both are metals; they lack complementary electronegativities to react.") },
        { ("Na", "Ca"), (NoReaction,
            "Both are electropositive metals and do not transfer electrons to one another.") },
        { ("K", "Mg"),  (NoReaction,
            "Both are electropositive metals; no ionic or covalent bond can form.") },
        { ("K", "Ca"),  (NoReaction,
            "Both are electropositive metals and will not undergo a chemical reaction.") },
        { ("Mg", "Ca"), (NoReaction,
            "Both are alkaline earth metals with similar properties; they only mix to form an alloy.") },

        // --- Non-metal + Non-metal pairs ---
        { ("O", "S"),   ("S + O\u2082 \u2192 SO\u2082",
            "While these two non-metals can react (forming Sulfur Dioxide), it typically requires an ignition/combustion source unlike spontaneous metal-nonmetal reactions.") },
        { ("O", "Cl"),  (NoDirectReaction,
            "Both are highly electronegative non-metals; oxygen and chlorine do not combine directly under standard conditions without specialized multi-step synthesis.") },
        { ("S", "Cl"),  (NoDirectReaction,
            "Non-metals require elevated heat or specific pressure to form sulfur chlorides (S\u2082Cl\u2082), so mixing them at room temperature yields no immediate reaction.") },
    };

    private void OnEnable()
    {
        FindSlotTexts();
    }

    private void Start()
    {
        // Retry finding the Submit button and result panel in Start,
        // since they may be created at runtime by ChemistryUIBuilder in Awake.
        FindSubmitButton();
        FindResultPanel();
    }

    private void Update()
    {
        TrackHoveredButton();
        HandleKeyboardInput();
    }

    /// <summary>
    /// Resolves the slot Text components by navigating the hierarchy.
    /// </summary>
    private void FindSlotTexts()
    {
        _slotAText = transform.Find(SLOT_A_PATH)?.GetComponent<Text>();
        _slotBText = transform.Find(SLOT_B_PATH)?.GetComponent<Text>();
    }

    /// <summary>
    /// Resolves the Submit button reference from the hierarchy.
    /// </summary>
    private void FindSubmitButton()
    {
        var btn = transform.Find(SUBMIT_BUTTON_PATH);
        if (btn != null)
            _submitButton = btn.GetComponent<Button>();
    }

    /// <summary>
    /// Resolves the ResultCanvas and its text components from the sibling hierarchy.
    /// Hides the result panel initially.
    /// </summary>
    private void FindResultPanel()
    {
        var parent = transform.parent;
        if (parent == null) return;

        var result = parent.Find("ResultCanvas");
        if (result == null) return;

        _resultCanvas = result.gameObject;
        _resultFormulaText = result.Find("FormulaText")?.GetComponent<Text>();
        _resultNameText = result.Find("CompoundNameText")?.GetComponent<Text>();
        _resultReasonText = result.Find("ReasonText")?.GetComponent<Text>();
        _resultCanvas.SetActive(false);

        // Find the 3D model display point (sibling of ResultCanvas).
        _modelDisplayPoint = parent.Find("ModelDisplayPoint");
    }

    /// <summary>
    /// Tracks which button the mouse is currently hovering over by
    /// performing a GraphicRaycast from the mouse position.
    /// This is used so Enter can trigger the hovered button.
    /// </summary>
    private void TrackHoveredButton()
    {
        _hoveredButton = null;

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current?.position.ReadValue() ?? Vector2.zero
        };

        var raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            return;

        var results = new System.Collections.Generic.List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        foreach (var result in results)
        {
            var button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                _hoveredButton = button;
                break;
            }
        }
    }

    /// <summary>
    /// Handles Enter key to confirm the currently hovered button,
    /// and Backspace/Delete to clear the slots.
    /// </summary>
    private void HandleKeyboardInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // Enter -> trigger the button currently under the mouse cursor
        if (keyboard[Key.Enter].wasPressedThisFrame)
        {
            if (_hoveredButton != null)
                _hoveredButton.onClick.Invoke();
        }

        // Backspace -> clear Slot A
        if (keyboard[Key.Backspace].wasPressedThisFrame)
        {
            ClearSlotA();
        }

        // Delete -> clear Slot B
        if (keyboard[Key.Delete].wasPressedThisFrame)
        {
            ClearSlotB();
        }
    }

    /// <summary>
    /// Called when an element button on the left panel is clicked.
    /// Displays the element symbol in Slot A.
    /// </summary>
    public void SelectLeftElement(string element)
    {
        if (_slotAText == null) FindSlotTexts();
        _selectedA = element;
        if (_slotAText != null)
            _slotAText.text = element;
        UpdateSubmitButtonState();
    }

    /// <summary>
    /// Called when an element button on the right panel is clicked.
    /// Displays the element symbol in Slot B.
    /// </summary>
    public void SelectRightElement(string element)
    {
        if (_slotBText == null) FindSlotTexts();
        _selectedB = element;
        if (_slotBText != null)
            _slotBText.text = element;
        UpdateSubmitButtonState();
    }

    /// <summary>
    /// Clears the selected element from Slot A.
    /// </summary>
    public void ClearSlotA()
    {
        if (_slotAText == null) FindSlotTexts();
        _selectedA = "";
        if (_slotAText != null)
            _slotAText.text = "";
        UpdateSubmitButtonState();
    }

    /// <summary>
    /// Clears the selected element from Slot B.
    /// </summary>
    public void ClearSlotB()
    {
        if (_slotBText == null) FindSlotTexts();
        _selectedB = "";
        if (_slotBText != null)
            _slotBText.text = "";
        UpdateSubmitButtonState();
    }

    /// <summary>
    /// Enables or disables the Submit button based on whether both slots are filled.
    /// </summary>
    private void UpdateSubmitButtonState()
    {
        if (_submitButton == null) FindSubmitButton();
        if (_submitButton != null)
            _submitButton.interactable = !string.IsNullOrEmpty(_selectedA) && !string.IsNullOrEmpty(_selectedB);
    }

    /// <summary>
    /// Called when the Submit button is clicked.
    /// Looks up the reaction for the selected element pair and displays
    /// the balanced equation and compound name on the ResultCanvas.
    /// Also spawns a 3D ball-and-stick model next to the result display.
    /// </summary>
    public void Submit()
    {
        if (string.IsNullOrEmpty(_selectedA) || string.IsNullOrEmpty(_selectedB))
        {
            Debug.Log("[ChemistryUI] Submit clicked but not both slots are filled");
            return;
        }

        // Lazily find the result panel if it wasn't available earlier.
        if (_resultCanvas == null) FindResultPanel();

        // Try both orderings since elements can be placed in either slot.
        if (Reactions.TryGetValue((_selectedA, _selectedB), out var reaction))
        {
            ShowResult(reaction.Equation, reaction.Name, "", reaction.ModelType);
        }
        else if (Reactions.TryGetValue((_selectedB, _selectedA), out var reactionReversed))
        {
            ShowResult(reactionReversed.Equation, reactionReversed.Name, "", reactionReversed.ModelType);
        }
        else if (NoReactions.TryGetValue((_selectedA, _selectedB), out var noReaction))
        {
            ShowResult(noReaction.Display, "", noReaction.Reason, CompoundModelType.None);
        }
        else if (NoReactions.TryGetValue((_selectedB, _selectedA), out var noReactionReversed))
        {
            ShowResult(noReactionReversed.Display, "", noReactionReversed.Reason, CompoundModelType.None);
        }
        else
        {
            ShowResult("No reaction found", "Unknown Combination", "", CompoundModelType.None);
        }
    }

    /// <summary>
    /// Shows the reaction result on the ResultCanvas panel and spawns or clears
    /// the 3D ball-and-stick model next to it.
    /// </summary>
    /// <param name="equation">The balanced chemical equation or no-reaction display text.</param>
    /// <param name="compoundName">The name of the resulting compound (empty for no-reaction cases).</param>
    /// <param name="reason">Optional explanation for no-reaction / special-case results.</param>
    /// <param name="modelType">The type of 3D ball-and-stick model to spawn (None to clear).</param>
    private void ShowResult(string equation, string compoundName, string reason = "",
        CompoundModelType modelType = CompoundModelType.None)
    {
        if (_resultCanvas != null)
            _resultCanvas.SetActive(true);
        if (_resultFormulaText != null)
            _resultFormulaText.text = equation;
        if (_resultNameText != null)
            _resultNameText.text = compoundName;
        if (_resultReasonText != null)
            _resultReasonText.text = reason;

        UpdateModelDisplay(modelType);

        Debug.Log($"[ChemistryUI] Result: {equation} ({compoundName}) Reason: {reason} Model: {modelType}");
    }

    /// <summary>
    /// Destroys the previous 3D model (if any) and spawns a new ball-and-stick
    /// model at the ModelDisplayPoint. Pass None to clear the display.
    /// </summary>
    private void UpdateModelDisplay(CompoundModelType modelType)
    {
        // Remove the previous model.
        if (_currentModel != null)
        {
            Destroy(_currentModel);
            _currentModel = null;
        }

        if (modelType == CompoundModelType.None)
            return;

        if (_modelDisplayPoint == null)
        {
            if (_resultCanvas != null && _resultCanvas.transform.parent != null)
                _modelDisplayPoint = _resultCanvas.transform.parent.Find("ModelDisplayPoint");
        }

        if (_modelDisplayPoint == null)
        {
            Debug.LogWarning("[ChemistryUI] ModelDisplayPoint not found — skipping model spawn.");
            return;
        }

        _currentModel = BallAndStickModelBuilder.BuildModel(modelType);
        if (_currentModel != null)
        {
            _currentModel.transform.SetParent(_modelDisplayPoint, false);
            _currentModel.transform.localPosition = Vector3.zero;
            _currentModel.transform.localRotation = Quaternion.identity;
            _currentModel.transform.localScale = Vector3.one;
        }
    }
}
