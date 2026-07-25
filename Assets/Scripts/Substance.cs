using UnityEngine;

// Attach this to any grabbable element/compound prefab (Na block, water beaker, etc.)
public class Substance : MonoBehaviour
{
    [Tooltip("Chemical symbol, e.g. Na, K, Ca, H2O")]
    public string symbol;
}
