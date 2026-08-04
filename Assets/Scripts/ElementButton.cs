using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to element button GameObjects. Stores the ElementData reference and self-wires
/// a click handler so Periodic Table tiles (built by PeriodicTableBuilder) respond to clicks
/// by default. ChemistryUIInitializer detects this component on Chemistry mixing UI buttons
/// and overrides the click handler with its own slot-selection logic, so both UIs share this
/// single component without conflicting.
/// </summary>
[RequireComponent(typeof(Button))]
public class ElementButton : MonoBehaviour
{
    public ElementData elementData;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (elementData == null) return;

        PeriodicTableManager periodicTableManager = FindAnyObjectByType<PeriodicTableManager>();
        if (periodicTableManager != null)
            periodicTableManager.SelectElement(elementData, gameObject);
        else
            GameFlowManager.Instance?.OnElementSelected(elementData);
    }
}
