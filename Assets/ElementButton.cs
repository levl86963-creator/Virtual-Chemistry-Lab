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
        GameFlowManager.Instance?.OnElementSelected(elementData);
    }
}