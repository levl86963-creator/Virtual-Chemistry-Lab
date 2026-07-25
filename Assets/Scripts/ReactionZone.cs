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
        if (recipe.gasBubbleEffectPrefab != null)
            Instantiate(recipe.gasBubbleEffectPrefab, reactant.transform.position, Quaternion.identity);

        if (recipe.sparkEffectPrefab != null)
            Instantiate(recipe.sparkEffectPrefab, reactant.transform.position, Quaternion.identity);

        if (recipe.reactionSound != null)
            AudioSource.PlayClipAtPoint(recipe.reactionSound, reactant.transform.position);

        // TODO: tint this station's liquid material to recipe.resultingColorTint
        // TODO: display recipe.productDescription on the quiz/feedback UI panel

        yield return new WaitForSeconds(recipe.reactionDuration);

        if (recipe.consumesReactant)
        {
            reactant.SetActive(false); // swap for a shrink/dissolve animation later if you want it smoother
        }
    }
}
