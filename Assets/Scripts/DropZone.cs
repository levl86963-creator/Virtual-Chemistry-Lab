using UnityEngine;

public class DropZone : MonoBehaviour
{
    public string slotName; // "A" or "B"
    
    void OnTriggerEnter(Collider other)
    {
        var atomModel = other.GetComponent<AtomModel>();
        if (atomModel != null && atomModel.data != null)
        {
            Debug.Log($"[DropZone] {atomModel.data.symbol} entered Slot{slotName}");
            ReactionWorkbench.Instance?.OnAtomDroppedInSlot(other.gameObject, slotName);
        }
    }
}
