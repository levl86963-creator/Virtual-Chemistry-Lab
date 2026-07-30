using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton — stores all ReactionRecipe assets and looks them up by element symbol.
/// ReactionRecipe already uses string symbols (reactantA / reactantB), so we match on those.
/// </summary>
public class ReactionDatabase : MonoBehaviour
{
    public static ReactionDatabase Instance;

    [Header("Drag ReactionRecipe ScriptableObject assets here")]
    public List<ReactionRecipe> recipes = new List<ReactionRecipe>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Returns the matching ReactionRecipe, or null if no reaction exists.</summary>
    public ReactionRecipe GetReaction(ElementData a, ElementData b)
    {
        if (a == null || b == null) return null;

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;
            bool match = (recipe.reactantA == a.symbol && recipe.reactantB == b.symbol)
                      || (recipe.reactantA == b.symbol && recipe.reactantB == a.symbol);
            if (match) return recipe;
        }
        return null;
    }

    /// <summary>Returns a human-readable "no reaction" string.</summary>
    public string GetNoReactionText(ElementData a, ElementData b)
    {
        if (a == null || b == null) return "No reaction.";
        return $"No reaction between {a.symbol} and {b.symbol}.";
    }
}
