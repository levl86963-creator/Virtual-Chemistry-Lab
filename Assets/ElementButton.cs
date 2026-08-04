using UnityEngine;
using UnityEngine.UI;

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

void OnClicked()
    {
        if (elementData == null) return;
        var mgr = FindAnyObjectByType<PeriodicTableManager>();
        if (mgr != null)
            mgr.SelectElement(elementData, gameObject);
        else
            GameFlowManager.Instance?.OnElementSelected(elementData);
    }
}