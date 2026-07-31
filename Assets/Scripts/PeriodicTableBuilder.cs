using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PeriodicTableBuilder : MonoBehaviour
{
    [Header("Grid")]
    public GameObject elementTilePrefab;
    public Transform gridParent;
    public Vector2 cellSize = new Vector2(50f, 50f);
    public Vector2 spacing = new Vector2(8f, 8f);

    [Header("Data")]
    public ElementData[] implementedElementData;

    [Header("Visual")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public float activeGlowAlpha = 1f;


    void Start()
{
    Debug.Log("BUILD TABLE STARTING");
    BuildTable();
}
    public void BuildTable()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        Dictionary<int, ElementData> elementLookup = new Dictionary<int, ElementData>();
        foreach (var el in implementedElementData)
            if (el != null) elementLookup[el.atomicNumber] = el;

        for (int i = 1; i <= 118; i++)
        {
            int row = (i - 1) / 18;
            int col = (i - 1) % 18;
            Vector2 pos = new Vector2(col * (cellSize.x + spacing.x), -row * (cellSize.y + spacing.y));

            var go = Instantiate(elementTilePrefab, gridParent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = cellSize;

            var btn = go.GetComponent<ElementButton>();
            var image = go.GetComponent<Image>();
            var button = go.GetComponent<Button>();

            ElementData data = elementLookup.ContainsKey(i) ? elementLookup[i] : null;
            
            if (data != null && data.isImplemented)
            {
                btn.elementData = data;
                button.interactable = true;
                image.color = data.displayColor;
            }
            else
            {
                button.interactable = false;
                image.color = lockedColor;
            }

            var text = go.GetComponentInChildren<Text>();
            if (text != null) text.text = i.ToString();
        }
    }

    public Color GetGroupColor(int atomicNumber)
    {
        // Find the element and return its display color
        foreach (var el in implementedElementData)
            if (el != null && el.atomicNumber == atomicNumber)
                return el.displayColor;
        return Color.gray;
    }
}