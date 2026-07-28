using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach this to the experiment station's water beaker / mixing container.
// It has its own base substance (e.g. "H2O") and checks anything dropped in
// against a list of known reaction recipes.
public class ReactionZone : MonoBehaviour
{
    [Tooltip("The substance this station already contains, e.g. H2O")]
    public string baseSubstance = "H2O";

    [Tooltip("All reactions this station knows how to trigger")]
    public List<ReactionRecipe> knownReactions;

    private void OnTriggerEnter(Collider other)
    {
        Substance incoming = other.GetComponent<Substance>();
        if (incoming == null) return;

        ReactionRecipe match = FindReaction(incoming.symbol, baseSubstance);
        if (match != null)
        {
            StartCoroutine(PlayReaction(match, incoming.gameObject));
        }
    }

    private ReactionRecipe FindReaction(string a, string b)
    {
        foreach (var recipe in knownReactions)
        {
            bool matches = (recipe.reactantA == a && recipe.reactantB == b) ||
                           (recipe.reactantA == b && recipe.reactantB == a);
            if (matches) return recipe;
        }
        return null;
    }

private IEnumerator PlayReaction(ReactionRecipe recipe, GameObject reactant)
    {
        Vector3 reactionPos = reactant.transform.position;

        // --- Phase 1: Sodium hits water — immediate violent bubbling ---
        if (recipe.gasBubbleEffectPrefab != null)
        {
            Instantiate(recipe.gasBubbleEffectPrefab, reactionPos, Quaternion.identity);
            Instantiate(recipe.gasBubbleEffectPrefab, reactionPos + Vector3.up * 0.05f, Quaternion.identity);
        }

        // --- Phase 2: Hissing sound starts immediately ---
        if (recipe.reactionSound != null)
            AudioSource.PlayClipAtPoint(recipe.reactionSound, reactionPos);

        // --- Phase 3: Wait briefly — H2 gas accumulates ---
        yield return new WaitForSeconds(0.3f);

        // --- Phase 4: H2 ignition — spark + flash + camera shake + boom ---
        if (recipe.sparkEffectPrefab != null)
            Instantiate(recipe.sparkEffectPrefab, reactionPos + Vector3.up * 0.08f, Quaternion.identity);

        // Trigger the flash + shake + boom via ReactionEffects component
        ReactionEffects effects = GetComponent<ReactionEffects>();
        if (effects != null)
            effects.TriggerIgnitionEffect();
        else
            Debug.LogWarning("[ReactionZone] ReactionEffects component not found — add it to " + gameObject.name);

        // --- Phase 5: Tint the water to show NaOH solution forming ---
        if (recipe.resultingColorTint != Color.white)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.gameObject != reactant)
                    r.material.color = Color.Lerp(r.material.color, recipe.resultingColorTint, 0.6f);
            }
        }

        // --- Phase 6: Log product ---
        if (!string.IsNullOrEmpty(recipe.productDescription))
            Debug.Log("[ReactionZone] REACTION RESULT: " + recipe.productDescription);

        // Wait for reaction to finish
        yield return new WaitForSeconds(recipe.reactionDuration);

        // --- Phase 7: Sodium shrinks and dissolves ---
        if (recipe.consumesReactant)
        {
            float t = 0f;
            Vector3 startScale = reactant.transform.localScale;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                if (reactant != null)
                    reactant.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
            if (reactant != null)
                reactant.SetActive(false);
        }
    }
}
