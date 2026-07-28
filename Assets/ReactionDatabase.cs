using UnityEngine;
using System.Collections.Generic;

public class ReactionDatabase : MonoBehaviour
{
    public static ReactionDatabase Instance;

    [System.Serializable]
    public class ReactionEntry
    {
        public ElementData elementA;
        public ElementData elementB;
        public CompoundData result;
    }

    public List<ReactionEntry> reactions = new List<ReactionEntry>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public CompoundData GetReaction(ElementData a, ElementData b)
    {
        if (a == null || b == null) return null;
        foreach (var entry in reactions)
            if ((entry.elementA == a && entry.elementB == b) || (entry.elementA == b && entry.elementB == a))
                return entry.result;
        return null;
    }

    public string GetNoReactionText(ElementData a, ElementData b)
    {
        if (!string.IsNullOrEmpty(a.noReactionReason)) return $"No reaction — {a.noReactionReason}";
        if (!string.IsNullOrEmpty(b.noReactionReason)) return $"No reaction — {b.noReactionReason}";
        return $"No reaction between {a.name} and {b.name}.";
    }
}