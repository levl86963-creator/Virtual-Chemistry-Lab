using UnityEngine;
using System.Collections;
using TMPro;
public class ReactionWorkbench : MonoBehaviour
{
    public static ReactionWorkbench Instance;
    
    [Header("Slots")]
    public Transform slotA;
    public Transform slotB;
    public GameObject currentAtomA;
    public GameObject currentAtomB;

    [Header("Reaction")]
    public Transform reactionCenter;
    public Transform resultSpawnPoint;
    public float reactionDuration = 1.5f;

    [Header("Effects")]
    public ParticleSystem reactionParticles;
    public AudioClip reactionSound;
    public AudioClip failSound;

    [Header("UI")]
    public GameObject floatingTextPrefab;
    public GameObject infoCardPrefab;

    void Awake() => Instance = this;

    public void PrepareElementA(ElementData data)
    {
        if (data == null || data.atomModelPrefab == null) return;
        ClearSlots();
        currentAtomA = Instantiate(data.atomModelPrefab, slotA.position, Quaternion.identity);
        currentAtomA.GetComponent<AtomModel>().data = data;
    }

    public void OnAtomDroppedInSlot(GameObject atom, string slotName)
    {
        if (slotName == "A") currentAtomA = atom;
        if (slotName == "B") currentAtomB = atom;
        
        if (currentAtomA != null && currentAtomB != null)
            StartCoroutine(TryReaction());
    }

    IEnumerator TryReaction()
    {
        ElementData a = currentAtomA.GetComponent<AtomModel>().data;
        ElementData b = currentAtomB.GetComponent<AtomModel>().data;
        
        CompoundData result = ReactionDatabase.Instance?.GetReaction(a, b);
        
        if (result != null)
        {
            yield return StartCoroutine(AnimateAtomsToCenter());
            reactionParticles?.Play();
            Destroy(currentAtomA); Destroy(currentAtomB);
            currentAtomA = null; currentAtomB = null;
            
            GameObject compound = Instantiate(result.compoundPrefab, resultSpawnPoint.position, Quaternion.identity);
            ShowInfoCard(result);
        }
        else
        {
            ShakeAtom(currentAtomA); ShakeAtom(currentAtomB);
            ShowFloatingText(ReactionDatabase.Instance?.GetNoReactionText(a, b));
            yield return new WaitForSeconds(3f);
            ClearSlotBOnly();
        }
    }

    IEnumerator AnimateAtomsToCenter()
    {
        float elapsed = 0f;
        Vector3 startA = currentAtomA.transform.position;
        Vector3 startB = currentAtomB.transform.position;
        while (elapsed < reactionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / reactionDuration;
            currentAtomA.transform.position = Vector3.Lerp(startA, reactionCenter.position, t);
            currentAtomB.transform.position = Vector3.Lerp(startB, reactionCenter.position, t);
            yield return null;
        }
    }

    void ShakeAtom(GameObject atom)
    {
        if (atom == null) return;
        StartCoroutine(ShakeCoroutine(atom));
    }

    IEnumerator ShakeCoroutine(GameObject atom)
    {
        Vector3 originalPos = atom.transform.position;
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            if (atom == null) yield break;
            elapsed += Time.deltaTime;
            atom.transform.position = originalPos + Random.insideUnitSphere * 0.02f;
            yield return null;
        }
        if (atom != null) atom.transform.position = originalPos;
    }

    void ShowFloatingText(string text)
    {
        if (floatingTextPrefab == null) return;
        var go = Instantiate(floatingTextPrefab, resultSpawnPoint.position + Vector3.up * 0.3f, Quaternion.identity);
        var tmp = go.GetComponentInChildren<TextMeshPro>();
        if (tmp != null) tmp.text = text;
        Destroy(go, 4f);
    }

    void ShowInfoCard(CompoundData data)
    {
        if (infoCardPrefab == null) return;
        Instantiate(infoCardPrefab, resultSpawnPoint.position + Vector3.up * 0.5f, Quaternion.identity);
    }

    void ClearSlotBOnly()
    {
        if (currentAtomB != null) { Destroy(currentAtomB); currentAtomB = null; }
    }

    public void ClearSlots()
    {
        if (currentAtomA != null) { Destroy(currentAtomA); currentAtomA = null; }
        if (currentAtomB != null) { Destroy(currentAtomB); currentAtomB = null; }
    }
}