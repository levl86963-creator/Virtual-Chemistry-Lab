using UnityEngine;

[CreateAssetMenu(fileName = "NewElement", menuName = "Chemistry/Element Data")]
public class ElementData : ScriptableObject
{
    public string elementName;
    public string symbol;
    public int atomicNumber;
    public int protons;
    public int neutrons;
    public int electrons;
    public Color displayColor;
    [TextArea] public string funFact;
    public GameObject atomModelPrefab;
}