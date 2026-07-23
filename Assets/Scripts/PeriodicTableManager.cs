using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PeriodicTableManager : MonoBehaviour
{
    [Header("UI Text Fields")]
    public TextMeshProUGUI elementNameText;
    public TextMeshProUGUI symbolText;
    public TextMeshProUGUI atomicNumberText;
    public TextMeshProUGUI funFactText;

    [Header("Selection Highlight")]
    [Tooltip("Optional: Drag a border/outline image here. Leave empty to use color tint only.")]
    public Image highlightBorder;
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(1f, 0.85f, 0.2f); // golden yellow

    [Header("Atom Viewer")]
    public Transform atomSpawnPoint;
    public GameObject atomPrefab;

    private GameObject currentAtom;
    private AtomViewer currentAtomViewer;
    private ElementButton currentButton;
    private Image currentButtonImage;

    /// <summary>
    /// Called by ElementButton when clicked.
    /// </summary>
    public void SelectElement(ElementData data, ElementButton clickedButton)
    {
        if (data == null || clickedButton == null) return;

        Debug.Log($"[PeriodicTableManager] Selected: {data.elementName}");

        // 1. Update info panel
        UpdateInfoPanel(data);

        // 2. Highlight the selected button
        SetHighlight(clickedButton);

        // 3. Spawn the 3D atom
        SpawnAtom(data);
    }

    private void UpdateInfoPanel(ElementData data)
    {
        if (elementNameText != null) elementNameText.text = data.elementName;
        if (symbolText != null) symbolText.text = data.symbol;
        if (atomicNumberText != null) atomicNumberText.text = $"Atomic Number: {data.atomicNumber}";
        if (funFactText != null) funFactText.text = data.funFact;
    }

    private void SetHighlight(ElementButton newButton)
    {
        // Un-highlight previous
        if (currentButtonImage != null)
            currentButtonImage.color = normalColor;

        // Highlight new
        currentButton = newButton;
        currentButtonImage = newButton.GetImage();
        if (currentButtonImage != null)
            currentButtonImage.color = selectedColor;

        // Move optional border on top
        if (highlightBorder != null)
        {
            highlightBorder.gameObject.SetActive(true);
            highlightBorder.transform.SetParent(newButton.transform, false);
            highlightBorder.transform.SetAsLastSibling();

            RectTransform rt = highlightBorder.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
    }

    private void SpawnAtom(ElementData data)
    {
        if (currentAtom != null)
            Destroy(currentAtom);

        if (atomPrefab == null || atomSpawnPoint == null)
        {
            Debug.LogWarning("[PeriodicTableManager] atomPrefab or atomSpawnPoint not assigned!");
            return;
        }

        currentAtom = Instantiate(atomPrefab, atomSpawnPoint.position, Quaternion.identity, atomSpawnPoint);

        currentAtomViewer = currentAtom.GetComponent<AtomViewer>();
        if (currentAtomViewer != null)
            currentAtomViewer.BuildAtom(data.atomicNumber);
        else
            Debug.LogWarning("[PeriodicTableManager] atomPrefab is missing AtomViewer script!");
    }

    /// <summary>
    /// Clears selection, info text, and destroys the atom.
    /// </summary>
    public void ClearSelection()
    {
        if (currentButtonImage != null)
            currentButtonImage.color = normalColor;

        currentButton = null;
        currentButtonImage = null;

        if (highlightBorder != null)
            highlightBorder.gameObject.SetActive(false);

        if (currentAtom != null)
        {
            Destroy(currentAtom);
            currentAtom = null;
        }

        if (elementNameText != null) elementNameText.text = "";
        if (symbolText != null) symbolText.text = "";
        if (atomicNumberText != null) atomicNumberText.text = "";
        if (funFactText != null) funFactText.text = "";
    }
}