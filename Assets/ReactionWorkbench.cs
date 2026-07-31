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
    public AudioSource audioSource;

    [Header("UI")]
    public GameObject floatingTextPrefab;

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // ── Called by GameFlowManager when entering ReactionExperiment stage ──────
public void PrepareElementA(ElementData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[ReactionWorkbench] PrepareElementA: data is null.");
            return;
        }
        ClearSlots();

        if (data.atomModelPrefab != null)
        {
            currentAtomA = Instantiate(data.atomModelPrefab, slotA.position, Quaternion.identity);
            var model = currentAtomA.GetComponent<AtomModel>();
            if (model != null) model.data = data;
        }
        else
        {
            // Fallback: spawn a colored sphere labeled with the element symbol
            currentAtomA = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
            currentAtomA.transform.position = slotA.position;
            currentAtomA.transform.localScale = Vector3.one * 0.15f;
            var rend = currentAtomA.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(rend.sharedMaterial);
                mat.color = new Color(data.displayColor.r, data.displayColor.g, data.displayColor.b, 1f);
                rend.material = mat;
            }
            // Add AtomModel so TryReaction can read the data
            var atomModel = currentAtomA.GetComponent<AtomModel>();
            if (atomModel == null) atomModel = currentAtomA.AddComponent<AtomModel>();
            atomModel.data = data;

            // Add XRGrabInteractable so player can pick it up
            var rb = currentAtomA.GetComponent<Rigidbody>();
            if (rb == null) rb = currentAtomA.AddComponent<Rigidbody>();
            rb.useGravity = false;
            if (currentAtomA.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
                currentAtomA.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }
    }

    // ── Called when an atom is physically dropped into a slot ────────────────
    public void OnAtomDroppedInSlot(GameObject atom, string slotName)
    {
        if (slotName == "A") { currentAtomA = atom; atom.transform.position = slotA.position; }
        if (slotName == "B") { currentAtomB = atom; atom.transform.position = slotB.position; }

        if (currentAtomA != null && currentAtomB != null)
            StartCoroutine(TryReaction());
    }

    // ── Reaction logic ────────────────────────────────────────────────────────
    IEnumerator TryReaction()
    {
        ElementData a = currentAtomA?.GetComponent<AtomModel>()?.data;
        ElementData b = currentAtomB?.GetComponent<AtomModel>()?.data;

        if (a == null || b == null)
        {
            Debug.LogWarning("[ReactionWorkbench] One or both atoms missing ElementData.");
            yield break;
        }

        ReactionRecipe recipe = ReactionDatabase.Instance?.GetReaction(a, b);

        if (recipe != null)
        {
            // Animate atoms together
            yield return StartCoroutine(AnimateAtomsToCenter());

            // Effects
            if (recipe.reactionSound != null)
                audioSource.PlayOneShot(recipe.reactionSound);

            if (reactionParticles != null)
                reactionParticles.Play();

            // Spawn visual effects from recipe
            if (recipe.gasBubbleEffectPrefab != null)
                Instantiate(recipe.gasBubbleEffectPrefab, reactionCenter.position, Quaternion.identity);

            if (recipe.sparkEffectPrefab != null)
                Instantiate(recipe.sparkEffectPrefab, reactionCenter.position, Quaternion.identity);

            // Destroy reactant atoms
            Destroy(currentAtomA);
            Destroy(currentAtomB);
            currentAtomA = null;
            currentAtomB = null;

            // Show product description as floating text
            if (!string.IsNullOrEmpty(recipe.productDescription))
                ShowFloatingText(recipe.productDescription);

            // Tell GameFlowManager the reaction succeeded
            GameFlowManager.Instance?.SetStage(GameFlowManager.Stage.Result);
        }
        else
        {
            // No reaction — shake and bounce back
            ShakeAtom(currentAtomA);
            ShakeAtom(currentAtomB);
            string msg = ReactionDatabase.Instance?.GetNoReactionText(a, b) ?? "No reaction.";
            ShowFloatingText(msg);
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
            if (currentAtomA != null) currentAtomA.transform.position = Vector3.Lerp(startA, reactionCenter.position, t);
            if (currentAtomB != null) currentAtomB.transform.position = Vector3.Lerp(startB, reactionCenter.position, t);
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
        Vector3 orig = atom.transform.position;
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            if (atom == null) yield break;
            elapsed += Time.deltaTime;
            atom.transform.position = orig + Random.insideUnitSphere * 0.02f;
            yield return null;
        }
        if (atom != null) atom.transform.position = orig;
    }

    void ShowFloatingText(string text)
    {
        if (floatingTextPrefab == null || resultSpawnPoint == null) return;
        var go = Instantiate(floatingTextPrefab, resultSpawnPoint.position + Vector3.up * 0.3f, Quaternion.identity);
        var tmp = go.GetComponentInChildren<TextMeshPro>();
        if (tmp != null) tmp.text = text;
        Destroy(go, 4f);
    }

    public void ClearSlotBOnly()
    {
        if (currentAtomB != null) { Destroy(currentAtomB); currentAtomB = null; }
    }

    public void ClearSlots()
    {
        if (currentAtomA != null) { Destroy(currentAtomA); currentAtomA = null; }
        if (currentAtomB != null) { Destroy(currentAtomB); currentAtomB = null; }
    }
}
