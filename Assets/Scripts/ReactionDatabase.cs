using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton — stores all ReactionRecipe assets and looks them up by element symbol.
/// Also maintains a lookup of pair-specific "no reaction" / special-case entries
/// so the result display can show accurate chemistry feedback.
/// </summary>
public class ReactionDatabase : MonoBehaviour
{
    public static ReactionDatabase Instance;

    [Header("Drag ReactionRecipe ScriptableObject assets here")]
    public List<ReactionRecipe> recipes = new List<ReactionRecipe>();

    [Header("Optional: override or add no-reaction entries from the Inspector")]
    [Tooltip("Built-in defaults are always loaded. Inspector entries override defaults with the same pair.")]
    public List<NoReactionEntry> customNoReactionEntries = new List<NoReactionEntry>();

    private Dictionary<string, NoReactionEntry> noReactionLookup;

    /// <summary>Constants for default no-reaction / special-case display text.</summary>
    private const string NoReaction = "No Reaction";
    private const string NoDirectReaction = "No Direct Reaction";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildNoReactionLookup();
    }

    /// <summary>Builds the order-independent lookup dictionary with built-in defaults plus any Inspector overrides.</summary>
    private void BuildNoReactionLookup()
    {
        noReactionLookup = new Dictionary<string, NoReactionEntry>();

        // --- Metal + Metal pairs (no compound formation) ---
        AddNoReactionDefault("Li", "Na", NoReaction,
            "Both are metals and cannot form an ionic or covalent bond; they only form a metallic alloy.");
        AddNoReactionDefault("Li", "K", NoReaction,
            "Both are alkali metals with identical bonding tendencies; no chemical reaction occurs.");
        AddNoReactionDefault("Li", "Mg", NoReaction,
            "Both are metals and do not react with each other under normal laboratory conditions.");
        AddNoReactionDefault("Li", "Ca", NoReaction,
            "Both are metals; they will only mix physically as an alloy without forming a compound.");
        AddNoReactionDefault("Na", "K", NoReaction,
            "Both are alkali metals; mixing them produces a liquid metal alloy (NaK) but no chemical compound.");
        AddNoReactionDefault("Na", "Mg", NoReaction,
            "Both are metals; they lack complementary electronegativities to react.");
        AddNoReactionDefault("Na", "Ca", NoReaction,
            "Both are electropositive metals and do not transfer electrons to one another.");
        AddNoReactionDefault("K", "Mg", NoReaction,
            "Both are electropositive metals; no ionic or covalent bond can form.");
        AddNoReactionDefault("K", "Ca", NoReaction,
            "Both are electropositive metals and will not undergo a chemical reaction.");
        AddNoReactionDefault("Mg", "Ca", NoReaction,
            "Both are alkaline earth metals with similar properties; they only mix to form an alloy.");

        // --- Non-metal + Non-metal pairs ---
        AddNoReactionDefault("O", "S", "S + O\u2082 \u2192 SO\u2082",
            "While these two non-metals can react (forming Sulfur Dioxide), it typically requires an ignition/combustion source unlike spontaneous metal-nonmetal reactions.");
        AddNoReactionDefault("O", "Cl", NoDirectReaction,
            "Both are highly electronegative non-metals; oxygen and chlorine do not combine directly under standard conditions without specialized multi-step synthesis.");
        AddNoReactionDefault("S", "Cl", NoDirectReaction,
            "Non-metals require elevated heat or specific pressure to form sulfur chlorides (S\u2082Cl\u2082), so mixing them at room temperature yields no immediate reaction.");

        // --- Apply Inspector overrides ---
        foreach (var entry in customNoReactionEntries)
        {
            if (entry == null) continue;
            if (string.IsNullOrEmpty(entry.reactantA) || string.IsNullOrEmpty(entry.reactantB)) continue;
            noReactionLookup[GetKey(entry.reactantA, entry.reactantB)] = entry;
        }
    }

    /// <summary>Adds a built-in default entry to the lookup.</summary>
    private void AddNoReactionDefault(string a, string b, string display, string reason)
    {
        noReactionLookup[GetKey(a, b)] = new NoReactionEntry
        {
            reactantA = a,
            reactantB = b,
            resultDisplay = display,
            reason = reason
        };
    }

    /// <summary>Returns an order-independent dictionary key for a pair of symbols.</summary>
    private static string GetKey(string a, string b)
    {
        return string.Compare(a, b, System.StringComparison.Ordinal) <= 0 ? $"{a}+{b}" : $"{b}+{a}";
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

    /// <summary>Returns the matching ReactionRecipe by raw symbol strings, or null.</summary>
    public ReactionRecipe GetReaction(string symbolA, string symbolB)
    {
        if (string.IsNullOrEmpty(symbolA) || string.IsNullOrEmpty(symbolB)) return null;

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;
            bool match = (recipe.reactantA == symbolA && recipe.reactantB == symbolB)
                       || (recipe.reactantA == symbolB && recipe.reactantB == symbolA);
            if (match) return recipe;
        }
        return null;
    }

    /// <summary>
    /// Returns a human-readable result string for a no-reaction / special-case pair.
    /// Looks up pair-specific display text and reason; falls back to a generic message.
    /// </summary>
    public string GetNoReactionText(ElementData a, ElementData b)
    {
        if (a == null || b == null) return "No reaction.";
        return GetNoReactionText(a.symbol, b.symbol);
    }

    /// <summary>
    /// Overload that accepts raw symbol strings for use by ReactionZone.
    /// Returns "DisplayText — Reason: ..." for known pairs, or a generic fallback.
    /// </summary>
    public string GetNoReactionText(string symbolA, string symbolB)
    {
        if (string.IsNullOrEmpty(symbolA) || string.IsNullOrEmpty(symbolB))
            return "No reaction.";

        NoReactionEntry entry;
        if (noReactionLookup != null && noReactionLookup.TryGetValue(GetKey(symbolA, symbolB), out entry))
        {
            return $"{entry.resultDisplay} \u2014 Reason: {entry.reason}";
        }

        return $"No reaction between {symbolA} and {symbolB}.";
    }
}
