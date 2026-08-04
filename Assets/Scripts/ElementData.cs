using UnityEngine;

[CreateAssetMenu(fileName = "NewElement", menuName = "VR Chem Lab/Element Data")]
public class ElementData : ScriptableObject
{
    public string elementName;
    public string symbol;
    public int atomicNumber;
    public int protons, neutrons, electrons;
    public ElementGroup elementGroup;
    public Color displayColor;
    public bool isImplemented = false;
    public string funFact;
    public string noReactionReason;
    public GameObject atomModelPrefab;
}
