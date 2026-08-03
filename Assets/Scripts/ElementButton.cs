using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to element button GameObjects. Stores the ElementData reference
/// and is detected by ChemistryUIInitializer to auto-wire button clicks.
/// </summary>
public class ElementButton : MonoBehaviour
{
    public ElementData elementData;
}
