using UnityEngine;

/// <summary>
/// Attach to spawned atom GameObjects to store their ElementData reference.
/// Used by ChemistrySlotManager and ReactionZone to identify what element
/// a GameObject represents.
/// </summary>
public class AtomModel : MonoBehaviour
{
    public ElementData data;
}
