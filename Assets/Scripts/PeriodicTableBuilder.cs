using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PeriodicTableBuilder : MonoBehaviour
{
    [Header("References")]
    public Transform tableContainer;
    public GameObject tilePrefab;
    public PeriodicTableManager tableManager;

    [Header("Element Data Assets")]
    [Tooltip("Drag your 8 active ElementData ScriptableObjects here")]
    public ElementData[] activeElementData;

    [Header("Tile Settings")]
    public Vector2 tileSize = new Vector2(44f, 50f);
    public float tileSpacing = 4f;

    [Header("Colors by Group")]
    public ColorGroup[] colorGroups;

    [Header("Active vs Locked")]
    [Range(0, 1)] public float activeGlowAlpha = 0.3f;
    [Range(0, 1)] public float lockedAlpha = 0.12f;
    [Range(0, 1)] public float activeBorderAlpha = 0.8f;
    [Range(0, 1)] public float lockedBorderAlpha = 0.15f;

    private Dictionary<int, Vector2Int> positions;

    void Awake()
    {
        EnsureColorGroups();
        InitPositions();
    }

    void Start()
    {
        EnsureColorGroups();
        BuildTable();
    }

    public void EnsureColorGroups()
    {
        var defaultGroups = new ColorGroup[]
        {
            new ColorGroup { group = ElementGroup.AlkaliMetal, color = HexToColor("#F43F5E") },
            new ColorGroup { group = ElementGroup.AlkalineEarth, color = HexToColor("#F97316") },
            new ColorGroup { group = ElementGroup.TransitionMetal, color = HexToColor("#3B82F6") },
            new ColorGroup { group = ElementGroup.PostTransition, color = HexToColor("#10B981") },
            new ColorGroup { group = ElementGroup.Metalloid, color = HexToColor("#84CC16") },
            new ColorGroup { group = ElementGroup.Nonmetal, color = HexToColor("#22C55E") },
            new ColorGroup { group = ElementGroup.Halogen, color = HexToColor("#06B6D4") },
            new ColorGroup { group = ElementGroup.NobleGas, color = HexToColor("#A855F7") },
            new ColorGroup { group = ElementGroup.Lanthanide, color = HexToColor("#EC4899") },
            new ColorGroup { group = ElementGroup.Actinide, color = HexToColor("#E11D48") }
        };

        if (colorGroups == null || colorGroups.Length < 10)
        {
            colorGroups = defaultGroups;
        }
        else
        {
            // Verify if array contains duplicate or default values
            bool isInvalid = false;
            for (int i = 0; i < colorGroups.Length; i++)
            {
                if (colorGroups[i] == null || (i > 0 && colorGroups[i].group == colorGroups[0].group))
                {
                    isInvalid = true;
                    break;
                }
            }

            if (isInvalid)
            {
                colorGroups = defaultGroups;
            }
        }
    }

    void InitPositions()
    {
        positions = new Dictionary<int, Vector2Int>
        {
            // Row 0: H, He
            {1, new Vector2Int(0, 0)}, {2, new Vector2Int(17, 0)},
            // Row 1: Li-Ne
            {3, new Vector2Int(0, 1)}, {4, new Vector2Int(1, 1)},
            {5, new Vector2Int(12, 1)}, {6, new Vector2Int(13, 1)}, {7, new Vector2Int(14, 1)}, {8, new Vector2Int(15, 1)}, {9, new Vector2Int(16, 1)}, {10, new Vector2Int(17, 1)},
            // Row 2: Na-Ar
            {11, new Vector2Int(0, 2)}, {12, new Vector2Int(1, 2)},
            {13, new Vector2Int(12, 2)}, {14, new Vector2Int(13, 2)}, {15, new Vector2Int(14, 2)}, {16, new Vector2Int(15, 2)}, {17, new Vector2Int(16, 2)}, {18, new Vector2Int(17, 2)},
            // Row 3: K-Kr
            {19, new Vector2Int(0, 3)}, {20, new Vector2Int(1, 3)},
            {21, new Vector2Int(2, 3)}, {22, new Vector2Int(3, 3)}, {23, new Vector2Int(4, 3)}, {24, new Vector2Int(5, 3)}, {25, new Vector2Int(6, 3)},
            {26, new Vector2Int(7, 3)}, {27, new Vector2Int(8, 3)}, {28, new Vector2Int(9, 3)}, {29, new Vector2Int(10, 3)}, {30, new Vector2Int(11, 3)},
            {31, new Vector2Int(12, 3)}, {32, new Vector2Int(13, 3)}, {33, new Vector2Int(14, 3)}, {34, new Vector2Int(15, 3)}, {35, new Vector2Int(16, 3)}, {36, new Vector2Int(17, 3)},
            // Row 4: Rb-Xe
            {37, new Vector2Int(0, 4)}, {38, new Vector2Int(1, 4)},
            {39, new Vector2Int(2, 4)}, {40, new Vector2Int(3, 4)}, {41, new Vector2Int(4, 4)}, {42, new Vector2Int(5, 4)}, {43, new Vector2Int(6, 4)},
            {44, new Vector2Int(7, 4)}, {45, new Vector2Int(8, 4)}, {46, new Vector2Int(9, 4)}, {47, new Vector2Int(10, 4)}, {48, new Vector2Int(11, 4)},
            {49, new Vector2Int(12, 4)}, {50, new Vector2Int(13, 4)}, {51, new Vector2Int(14, 4)}, {52, new Vector2Int(15, 4)}, {53, new Vector2Int(16, 4)}, {54, new Vector2Int(17, 4)},
            // Row 5: Cs, Ba + placeholder + Hf-Rn
            {55, new Vector2Int(0, 5)}, {56, new Vector2Int(1, 5)},
            {72, new Vector2Int(3, 5)}, {73, new Vector2Int(4, 5)}, {74, new Vector2Int(5, 5)}, {75, new Vector2Int(6, 5)},
            {76, new Vector2Int(7, 5)}, {77, new Vector2Int(8, 5)}, {78, new Vector2Int(9, 5)}, {79, new Vector2Int(10, 5)}, {80, new Vector2Int(11, 5)},
            {81, new Vector2Int(12, 5)}, {82, new Vector2Int(13, 5)}, {83, new Vector2Int(14, 5)}, {84, new Vector2Int(15, 5)}, {85, new Vector2Int(16, 5)}, {86, new Vector2Int(17, 5)},
            // Row 6: Fr, Ra + placeholder only (no >92 elements)
            {87, new Vector2Int(0, 6)}, {88, new Vector2Int(1, 6)},
        };
    }

    void BuildTable()
    {
        // Clear existing generated tiles if rebuilding
        if (tableContainer != null)
        {
            for (int i = tableContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(tableContainer.GetChild(i).gameObject);
            }
        }

        // Main table rows 0-4: elements 1-56
        for (int z = 1; z <= 56; z++)
            if (positions.ContainsKey(z)) CreateTile(z, positions[z]);

        // Row 5: Cs, Ba, Hf-Rn (57-71 are in lanthanide row below)
        for (int z = 72; z <= 86; z++)
            if (positions.ContainsKey(z)) CreateTile(z, positions[z]);
        if (positions.ContainsKey(55)) CreateTile(55, positions[55]);
        if (positions.ContainsKey(56)) CreateTile(56, positions[56]);

        // Row 6: Fr, Ra only (89-92 are in actinide row below; 93+ hidden)
        if (positions.ContainsKey(87)) CreateTile(87, positions[87]);
        if (positions.ContainsKey(88)) CreateTile(88, positions[88]);

        // Placeholders in main table gaps
        CreatePlaceholderTile("57-71", "Lanthanides", new Vector2Int(2, 5), ElementGroup.Lanthanide);
        CreatePlaceholderTile("89-92", "Actinides", new Vector2Int(2, 6), ElementGroup.Actinide);

        // Lanthanides row (row 8, below gap)
        for (int z = 57; z <= 71; z++)
            CreateTile(z, new Vector2Int(z - 55, 8));

        // Natural Actinides row (row 9)
        for (int z = 89; z <= 92; z++)
            CreateTile(z, new Vector2Int(z - 87, 9));
    }

    void CreateTile(int atomicNumber, Vector2Int gridPos)
    {
        GameObject tile = Instantiate(tilePrefab, tableContainer);
        RectTransform rt = tile.GetComponent<RectTransform>();

        // Position using top-left anchor for consistent layout
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        float x = 20f + gridPos.x * (tileSize.x + tileSpacing);
        float y = -32f - gridPos.y * (tileSize.y + tileSpacing);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = tileSize;

        ConfigureTextLayout(tile);
        SetupTile(tile, atomicNumber);
    }

    void ConfigureTextLayout(GameObject tile)
    {
        Transform symbolT = tile.transform.Find("SymbolText");
        if (symbolT)
        {
            RectTransform srt = symbolT.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 0.25f);
            srt.anchorMax = new Vector2(1, 0.85f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            srt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = symbolT.GetComponent<TextMeshProUGUI>();
            if (tmp)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 6f;
                tmp.fontSizeMax = 22f;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        Transform numberT = tile.transform.Find("AtomicNumberText");
        if (numberT)
        {
            RectTransform nrt = numberT.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0, 0.8f);
            nrt.anchorMax = new Vector2(0.5f, 1f);
            nrt.offsetMin = new Vector2(2, 0);
            nrt.offsetMax = new Vector2(0, -1);
            nrt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = numberT.GetComponent<TextMeshProUGUI>();
            if (tmp)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 4f;
                tmp.fontSizeMax = 8f;
                tmp.alignment = TextAlignmentOptions.TopLeft;
            }
        }

        Transform nameT = tile.transform.Find("NameText");
        if (nameT)
        {
            RectTransform mrt = nameT.GetComponent<RectTransform>();
            mrt.anchorMin = new Vector2(0, 0);
            mrt.anchorMax = new Vector2(1, 0.25f);
            mrt.offsetMin = new Vector2(1, 1);
            mrt.offsetMax = new Vector2(-1, 0);
            mrt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = nameT.GetComponent<TextMeshProUGUI>();
            if (tmp)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 3f;
                tmp.fontSizeMax = 7f;
                tmp.alignment = TextAlignmentOptions.Bottom;
                tmp.overflowMode = TextOverflowModes.Truncate;
            }
        }
    }

    void SetupTile(GameObject tile, int atomicNumber)
    {
        bool isActive = IsImplementedElement(atomicNumber);
        Color groupColor = GetGroupColor(atomicNumber);

        Image bgImage = tile.GetComponent<Image>();
        Outline outline = tile.GetComponent<Outline>();
        if (outline == null) outline = tile.gameObject.AddComponent<Outline>();

        Button btn = tile.GetComponent<Button>();
        if (btn)
        {
            btn.transition = Selectable.Transition.None;
        }

        Transform symbolT = tile.transform.Find("SymbolText");
        Transform numberT = tile.transform.Find("AtomicNumberText");
        Transform nameT = tile.transform.Find("NameText");

        TextMeshProUGUI symbolText = symbolT ? symbolT.GetComponent<TextMeshProUGUI>() : null;
        TextMeshProUGUI numberText = numberT ? numberT.GetComponent<TextMeshProUGUI>() : null;
        TextMeshProUGUI nameText = nameT ? nameT.GetComponent<TextMeshProUGUI>() : null;

        string symbol = GetSymbol(atomicNumber);
        string name = GetName(atomicNumber);

        if (symbolText) symbolText.text = symbol;
        if (numberText) numberText.text = atomicNumber.ToString();
        if (nameText) nameText.text = name;

        if (isActive)
        {
            bgImage.color = new Color(groupColor.r, groupColor.g, groupColor.b, activeGlowAlpha);
            outline.effectColor = new Color(groupColor.r, groupColor.g, groupColor.b, activeBorderAlpha);
            outline.effectDistance = new Vector2(2f, 2f);

            if (symbolText) { symbolText.color = Color.white; symbolText.fontStyle = FontStyles.Bold; }
            if (numberText) numberText.color = new Color(1, 1, 1, 0.9f);
            if (nameText) nameText.color = new Color(1, 1, 1, 0.8f);

            if (btn)
            {
                btn.interactable = true;
                ElementData data = FindElementData(atomicNumber);
                if (data != null && tableManager != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => tableManager.SelectElement(data, tile));
                }
            }
        }
        else
        {
            bgImage.color = new Color(groupColor.r, groupColor.g, groupColor.b, lockedAlpha);
            outline.effectColor = new Color(groupColor.r, groupColor.g, groupColor.b, lockedBorderAlpha);
            outline.effectDistance = new Vector2(1f, 1f);

            if (symbolText) { symbolText.color = new Color(groupColor.r, groupColor.g, groupColor.b, 0.4f); symbolText.fontStyle = FontStyles.Normal; }
            if (numberText) numberText.color = new Color(groupColor.r, groupColor.g, groupColor.b, 0.25f);
            if (nameText) nameText.color = new Color(groupColor.r, groupColor.g, groupColor.b, 0.2f);

            if (btn) btn.interactable = false;
        }
    }

    void CreatePlaceholderTile(string label, string subtitle, Vector2Int gridPos, ElementGroup group)
    {
        GameObject tile = Instantiate(tilePrefab, tableContainer);
        RectTransform rt = tile.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        float x = 20f + gridPos.x * (tileSize.x + tileSpacing);
        float y = -32f - gridPos.y * (tileSize.y + tileSpacing);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = tileSize;

        ConfigureTextLayout(tile);

        Color groupColor = GetColorForGroup(group);
        Image bgImage = tile.GetComponent<Image>();
        Outline outline = tile.GetComponent<Outline>();
        if (outline == null) outline = tile.gameObject.AddComponent<Outline>();

        Button btn = tile.GetComponent<Button>();
        if (btn)
        {
            btn.transition = Selectable.Transition.None;
            btn.interactable = false;
        }

        Transform symbolT = tile.transform.Find("SymbolText");
        Transform numberT = tile.transform.Find("AtomicNumberText");
        Transform nameT = tile.transform.Find("NameText");

        TextMeshProUGUI symbolText = symbolT ? symbolT.GetComponent<TextMeshProUGUI>() : null;
        TextMeshProUGUI numberText = numberT ? numberT.GetComponent<TextMeshProUGUI>() : null;
        TextMeshProUGUI nameText = nameT ? nameT.GetComponent<TextMeshProUGUI>() : null;

        if (symbolText)
        {
            symbolText.text = label;
            symbolText.color = new Color(groupColor.r, groupColor.g, groupColor.b, 0.7f);
            symbolText.fontStyle = FontStyles.Bold;
        }
        if (numberText) numberText.text = "";
        if (nameText)
        {
            nameText.text = subtitle;
            nameText.color = new Color(groupColor.r, groupColor.g, groupColor.b, 0.5f);
        }

        bgImage.color = new Color(groupColor.r, groupColor.g, groupColor.b, lockedAlpha);
        outline.effectColor = new Color(groupColor.r, groupColor.g, groupColor.b, lockedBorderAlpha);
        outline.effectDistance = new Vector2(1f, 1f);
    }

    public ElementData FindElementData(int atomicNumber)
    {
        if (activeElementData == null) return null;
        foreach (var ed in activeElementData)
            if (ed != null && ed.atomicNumber == atomicNumber) return ed;
        return null;
    }

    public bool IsImplementedElement(int z)
    {
        return z == 3 || z == 8 || z == 11 || z == 12 || z == 16 || z == 17 || z == 19 || z == 20;
    }

    public Color GetGroupColor(int atomicNumber)
    {
        return GetColorForGroup(GetElementGroup(atomicNumber));
    }

    public Color GetColorForGroup(ElementGroup group)
    {
        if (colorGroups != null)
        {
            foreach (var cg in colorGroups)
                if (cg != null && cg.group == group) return cg.color;
        }

        switch (group)
        {
            case ElementGroup.AlkaliMetal: return HexToColor("#F43F5E");
            case ElementGroup.AlkalineEarth: return HexToColor("#F97316");
            case ElementGroup.TransitionMetal: return HexToColor("#3B82F6");
            case ElementGroup.PostTransition: return HexToColor("#10B981");
            case ElementGroup.Metalloid: return HexToColor("#84CC16");
            case ElementGroup.Nonmetal: return HexToColor("#22C55E");
            case ElementGroup.Halogen: return HexToColor("#06B6D4");
            case ElementGroup.NobleGas: return HexToColor("#A855F7");
            case ElementGroup.Lanthanide: return HexToColor("#EC4899");
            case ElementGroup.Actinide: return HexToColor("#E11D48");
            default: return HexToColor("#3B82F6");
        }
    }

    public ElementGroup GetElementGroup(int z)
    {
        if (z == 3 || z == 11 || z == 19 || z == 37 || z == 55 || z == 87) return ElementGroup.AlkaliMetal;
        if (z == 4 || z == 12 || z == 20 || z == 38 || z == 56 || z == 88) return ElementGroup.AlkalineEarth;
        if ((z >= 21 && z <= 30) || (z >= 39 && z <= 48) || (z >= 72 && z <= 80) || (z >= 104 && z <= 112)) return ElementGroup.TransitionMetal;
        if (z == 13 || z == 31 || z == 49 || z == 50 || z == 81 || z == 82 || z == 83 || z == 113 || z == 114 || z == 115) return ElementGroup.PostTransition;
        if (z == 5 || z == 14 || z == 32 || z == 33 || z == 51 || z == 52 || z == 84) return ElementGroup.Metalloid;
        if (z == 1 || z == 6 || z == 7 || z == 8 || z == 15 || z == 16 || z == 34) return ElementGroup.Nonmetal;
        if (z == 9 || z == 17 || z == 35 || z == 53 || z == 85 || z == 117) return ElementGroup.Halogen;
        if (z == 2 || z == 10 || z == 18 || z == 36 || z == 54 || z == 86 || z == 118) return ElementGroup.NobleGas;
        if (z >= 57 && z <= 71) return ElementGroup.Lanthanide;
        if (z >= 89 && z <= 103) return ElementGroup.Actinide;
        return ElementGroup.TransitionMetal;
    }

    public string GetSymbol(int z)
    {
        string[] symbols = {"", "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne",
            "Na", "Mg", "Al", "Si", "P", "S", "Cl", "Ar", "K", "Ca",
            "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn",
            "Ga", "Ge", "As", "Se", "Br", "Kr", "Rb", "Sr", "Y", "Zr",
            "Nb", "Mo", "Tc", "Ru", "Rh", "Pd", "Ag", "Cd", "In", "Sn",
            "Sb", "Te", "I", "Xe", "Cs", "Ba", "La", "Ce", "Pr", "Nd",
            "Pm", "Sm", "Eu", "Gd", "Tb", "Dy", "Ho", "Er", "Tm", "Yb",
            "Lu", "Hf", "Ta", "W", "Re", "Os", "Ir", "Pt", "Au", "Hg",
            "Tl", "Pb", "Bi", "Po", "At", "Rn", "Fr", "Ra", "Ac", "Th",
            "Pa", "U"};
        return z < symbols.Length ? symbols[z] : "?";
    }

    public string GetName(int z)
    {
        string[] names = {"", "Hydrogen", "Helium", "Lithium", "Beryllium", "Boron", "Carbon", "Nitrogen", "Oxygen", "Fluorine", "Neon",
            "Sodium", "Magnesium", "Aluminium", "Silicon", "Phosphorus", "Sulfur", "Chlorine", "Argon", "Potassium", "Calcium",
            "Scandium", "Titanium", "Vanadium", "Chromium", "Manganese", "Iron", "Cobalt", "Nickel", "Copper", "Zinc",
            "Gallium", "Germanium", "Arsenic", "Selenium", "Bromine", "Krypton", "Rubidium", "Strontium", "Yttrium", "Zirconium",
            "Niobium", "Molybdenum", "Technetium", "Ruthenium", "Rhodium", "Palladium", "Silver", "Cadmium", "Indium", "Tin",
            "Antimony", "Tellurium", "Iodine", "Xenon", "Cesium", "Barium", "Lanthanum", "Cerium", "Praseodymium", "Neodymium",
            "Promethium", "Samarium", "Europium", "Gadolinium", "Terbium", "Dysprosium", "Holmium", "Erbium", "Thulium", "Ytterbium",
            "Lutetium", "Hafnium", "Tantalum", "Tungsten", "Rhenium", "Osmium", "Iridium", "Platinum", "Gold", "Mercury",
            "Thallium", "Lead", "Bismuth", "Polonium", "Astatine", "Radon", "Francium", "Radium", "Actinium", "Thorium",
            "Protactinium", "Uranium"};
        return z < names.Length ? names[z] : "Unknown";
    }

    private Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color c))
        {
            c.a = 1f;
            return c;
        }
        return Color.white;
    }
}

[System.Serializable]
public class ColorGroup
{
    public ElementGroup group;
    public Color color;
}