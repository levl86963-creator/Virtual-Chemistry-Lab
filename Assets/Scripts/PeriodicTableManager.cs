using UnityEngine;
using TMPro;

public class PeriodicTableManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI elementNameText;
    public TextMeshProUGUI symbolText;
    public TextMeshProUGUI atomicNumberText;
    public TextMeshProUGUI funFactText;

    [Header("Atom Viewer")]
    public Transform atomSpawnPoint;      // Empty GameObject where atom appears
    public GameObject atomPrefab;       // Your AtomViewer prefab
    private GameObject currentAtom;
    private AtomViewer currentAtomViewer;

    public void SelectElement(ElementData element)
    {
        // Update UI text
        elementNameText.text = element.elementName;
        symbolText.text = element.symbol;
        atomicNumberText.text = "Atomic Number: " + element.atomicNumber;
        funFactText.text = element.funFact;

        // Spawn or update atom
        SpawnAtom(element);
    }

    void SpawnAtom(ElementData element)
    {
        // Remove old atom
        if (currentAtom != null)
            Destroy(currentAtom);

        // Spawn new atom
        if (atomPrefab != null && atomSpawnPoint != null)
        {
            currentAtom = Instantiate(atomPrefab, atomSpawnPoint.position, Quaternion.identity);
            currentAtom.transform.SetParent(atomSpawnPoint);

            // Build the atom structure
            currentAtomViewer = currentAtom.GetComponent<AtomViewer>();
            if (currentAtomViewer != null)
            {
                currentAtomViewer.BuildAtom(element.atomicNumber);
            }
        }
    }
}