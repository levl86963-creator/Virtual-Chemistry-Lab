using UnityEngine;

/// <summary>
/// Attach to spawned atom GameObjects (e.g. ElementData.atomModelPrefab). Stores the
/// ElementData reference read by ChemistrySlotManager and DropZone to identify which
/// element the atom represents, and sets up XR grab interaction so the atom can be
/// picked up and dropped into reaction slots.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class AtomModel : MonoBehaviour
{
    public ElementData data;

    void Awake()
    {
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        if (grab.attachTransform == null)
        {
            var attachPoint = new GameObject("AttachPoint").transform;
            attachPoint.SetParent(transform);
            attachPoint.localPosition = Vector3.zero;
            grab.attachTransform = attachPoint;
        }
    }

    void Update() => transform.Rotate(Vector3.up, 20f * Time.deltaTime);
}
