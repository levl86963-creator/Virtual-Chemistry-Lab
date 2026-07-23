using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class ElementButton : MonoBehaviour
{
    [Tooltip("Drag the ElementData ScriptableObject here (e.g., Sodium.asset)")]
    public ElementData elementData;

    private Button button;
    private Image buttonImage;
    private PeriodicTableManager manager;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    void Start()
    {
        // Auto-find the manager in the scene
        manager = FindObjectOfType<PeriodicTableManager>();
        if (manager == null)
        {
            Debug.LogError($"[{name}] PeriodicTableManager not found in scene!");
            enabled = false;
            return;
        }

        // Wire up the click
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        if (elementData == null)
        {
            Debug.LogError($"[{name}] No ElementData assigned!");
            return;
        }

        // Tell the manager this button was selected
        manager.SelectElement(elementData, this);
    }

    public Image GetImage() => buttonImage;

    void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnClicked);
    }
}