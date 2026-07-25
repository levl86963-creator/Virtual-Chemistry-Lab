using UnityEngine;

[CreateAssetMenu(fileName = "NewReaction", menuName = "Chemistry/Reaction Recipe")]
public class ReactionRecipe : ScriptableObject
{
    [Header("Reactants (order doesn't matter)")]
    public string reactantA;   // e.g. "Na"
    public string reactantB;   // e.g. "H2O"

    [Header("Result (shown in UI/quiz feedback)")]
    [TextArea]
    public string productDescription; // e.g. "NaOH + H2 (hydrogen gas released)"
    public bool isHazardousInRealLife = true;

    [Header("Visual/Audio Effects")]
    public GameObject gasBubbleEffectPrefab;
    public GameObject sparkEffectPrefab;      // optional - for exothermic reactions like Na/K + water
    public AudioClip reactionSound;           // fizzing/hissing sound
    public Color resultingColorTint = Color.white;
    public float reactionDuration = 3f;
    public bool consumesReactant = true;      // e.g. sodium dissolves away after reacting
}
