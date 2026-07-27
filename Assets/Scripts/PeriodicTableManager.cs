using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PeriodicTableManager : MonoBehaviour
{
    [Header("UI Text Fields (Old InfoPanel — optional)")]
    public TextMeshProUGUI elementNameText;
    public TextMeshProUGUI symbolText;
    public TextMeshProUGUI atomicNumberText;
    public TextMeshProUGUI funFactText;

    [Header("Selection Highlight")]
    public Image highlightBorder;
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(1f, 0.85f, 0.2f); // Golden Yellow

    [Header("Atom Viewer")]
    public Transform atomSpawnPoint;
    public GameObject atomPrefab;

    [Header("Floating Info Slab")]
    public InfoSlabFollower infoSlab;

    private GameObject currentAtom;
    private AtomViewer currentAtomViewer;
    private GameObject currentTile;
    private Image currentButtonImage;
    private ElementData currentSelectedData;
    private Color currentOriginalTileColor;

    void Start()
    {
        EnsureInfoSlabReference();
        ResetStateOnly();
    }

private void EnsureInfoSlabReference()
    {
        if (infoSlab == null)
        {
            infoSlab = FindAnyObjectByType<InfoSlabFollower>(FindObjectsInactive.Include);
        }

        // If the InfoSlab GameObject is inactive, activate it so Start() runs
        // and the 2.5s coroutine can begin. The script hides the canvas itself.
        if (infoSlab != null && !infoSlab.gameObject.activeSelf)
        {
            infoSlab.gameObject.SetActive(true);
        }
    }

    public void SelectElement(ElementData data, GameObject clickedTile = null)
    {
        if (data == null) return;

        EnsureInfoSlabReference();

        // TOGGLE DESELECT: If clicking the currently selected element again -> DESELECT!
        if (currentSelectedData != null && currentSelectedData.atomicNumber == data.atomicNumber)
        {
            ClearSelection();
            return;
        }

        // If another tile was previously selected, restore its original color
        if (currentButtonImage != null && currentTile != null && currentTile != clickedTile)
        {
            currentButtonImage.color = currentOriginalTileColor;
        }

        currentSelectedData = data;
        Debug.Log($"[PeriodicTableManager] Selected: {data.elementName}");

        // Update old info panel
        UpdateInfoPanel(data);

        // Update floating slab
        if (infoSlab != null)
        {
            infoSlab.ShowElement(data);
        }

        // Highlight selected tile
        if (clickedTile != null)
        {
            SetHighlight(clickedTile, data);
        }

        // Spawn 3D atom
        SpawnAtom(data);
    }

    private void UpdateInfoPanel(ElementData data)
    {
        if (elementNameText != null) elementNameText.text = data.elementName;
        if (symbolText != null) symbolText.text = data.symbol;
        if (atomicNumberText != null) atomicNumberText.text = $"Atomic Number: {data.atomicNumber}";
        if (funFactText != null) funFactText.text = data.funFact;
    }

    private void SetHighlight(GameObject newTile, ElementData data)
    {
        currentTile = newTile;
        currentButtonImage = newTile.GetComponent<Image>();

        if (currentButtonImage != null)
        {
            // Store original tile color before applying highlight
            PeriodicTableBuilder builder = FindAnyObjectByType<PeriodicTableBuilder>(FindObjectsInactive.Include);
            if (builder != null)
            {
                Color groupColor = builder.GetGroupColor(data.atomicNumber);
                currentOriginalTileColor = new Color(groupColor.r, groupColor.g, groupColor.b, builder.activeGlowAlpha);
            }
            else
            {
                currentOriginalTileColor = currentButtonImage.color;
            }

            currentButtonImage.color = selectedColor;
        }

        if (highlightBorder != null)
        {
            highlightBorder.gameObject.SetActive(true);
            highlightBorder.transform.SetParent(newTile.transform, false);
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
        {
            SafeDestroy(currentAtom);
            currentAtom = null;
        }

        if (atomSpawnPoint != null)
        {
            for (int i = atomSpawnPoint.childCount - 1; i >= 0; i--)
            {
                SafeDestroy(atomSpawnPoint.GetChild(i).gameObject);
            }
        }

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

    // Resets tile/atom/UI state WITHOUT hiding the InfoSlab — used on startup
    // so InfoSlabFollower's own 2.5s coroutine can run unimpeded.
    private void ResetStateOnly()
    {
        if (currentButtonImage != null)
            currentButtonImage.color = currentOriginalTileColor;

        currentTile = null;
        currentButtonImage = null;
        currentSelectedData = null;

        if (highlightBorder != null)
            highlightBorder.gameObject.SetActive(false);

        if (currentAtom != null)
        {
            SafeDestroy(currentAtom);
            currentAtom = null;
        }

        if (atomSpawnPoint != null)
        {
            for (int i = atomSpawnPoint.childCount - 1; i >= 0; i--)
                SafeDestroy(atomSpawnPoint.GetChild(i).gameObject);
        }

        if (elementNameText != null) elementNameText.text = "";
        if (symbolText != null) symbolText.text = "";
        if (atomicNumberText != null) atomicNumberText.text = "";
        if (funFactText != null) funFactText.text = "";
    }

    public void ClearSelection()
    {
        // Restore tile image color to original group color (NOT white, NOT gray)
        if (currentButtonImage != null)
        {
            currentButtonImage.color = currentOriginalTileColor;
        }

        currentTile = null;
        currentButtonImage = null;
        currentSelectedData = null;

        if (highlightBorder != null)
        {
            highlightBorder.gameObject.SetActive(false);
        }

        if (currentAtom != null)
        {
            SafeDestroy(currentAtom);
            currentAtom = null;
        }

        if (atomSpawnPoint != null)
        {
            for (int i = atomSpawnPoint.childCount - 1; i >= 0; i--)
            {
                SafeDestroy(atomSpawnPoint.GetChild(i).gameObject);
            }
        }

        // Clear old panel text
        if (elementNameText != null) elementNameText.text = "";
        if (symbolText != null) symbolText.text = "";
        if (atomicNumberText != null) atomicNumberText.text = "";
        if (funFactText != null) funFactText.text = "";

        // Hide slab
        EnsureInfoSlabReference();
        if (infoSlab != null)
        {
            infoSlab.Hide();
        }
    }

    public void HideTableAndSlab()
    {
        ClearSelection();

        // Hide this Mainboard directly (do NOT walk up to root Canvas)
        gameObject.SetActive(false);

        // Hide InfoSlab
        EnsureInfoSlabReference();
        if (infoSlab != null)
        {
            infoSlab.Hide();
        }
    }

    private void SafeDestroy(GameObject obj)
    {
        if (obj == null) return;
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }
}
