using System;
using UnityEngine;

/// <summary>
/// Stores a pair-specific "no reaction" or "special case" result for two elements.
/// Used by ReactionDatabase to display accurate chemistry feedback.
/// </summary>
[Serializable]
public class NoReactionEntry
{
    [Tooltip("Symbol of the first reactant, e.g. \"Li\"")]
    public string reactantA;

    [Tooltip("Symbol of the second reactant, e.g. \"Na\"")]
    public string reactantB;

    [Tooltip("What to show as the result, e.g. \"No Reaction\" or \"S + O₂ → SO₂\"")]
    [TextArea]
    public string resultDisplay;

    [Tooltip("Why this result occurs (shown after the display text)")]
    [TextArea]
    public string reason;
}
