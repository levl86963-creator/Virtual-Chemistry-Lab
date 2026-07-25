using UnityEngine;

[CreateAssetMenu(fileName = "NewElement", menuName = "VR Chem Lab/Element Data")]
public class ElementData : ScriptableObject
{
    [Header("Basic Info")]
    public string elementName;
    public string symbol;
    public int atomicNumber;

    [Header("Atomic Structure")]
    public int protons;
    public int neutrons;
    public int electrons;

    [Header("UI & Visuals")]
    public ElementGroup elementGroup;
    public Color displayColor;
    public bool isImplemented = false; // Your 8 active elements = true
    public string funFact;

    [Header("3D Atom")]
    public GameObject atomModelPrefab;
}

public enum ElementGroup
{
    AlkaliMetal,      // Red
    AlkalineEarth,    // Orange
    TransitionMetal,  // Blue
    PostTransition,   // Teal
    Metalloid,        // Lime
    Nonmetal,         // Green
    Halogen,          // Cyan
    NobleGas,         // Purple
    Lanthanide,       // Pink
    Actinide          // Rose
}