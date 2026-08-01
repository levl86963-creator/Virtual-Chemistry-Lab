using UnityEngine;

/// <summary>
/// Identifies which ball-and-stick model structure to build for a given compound.
/// </summary>
public enum CompoundModelType
{
    None,
    /// <summary>2 spheres, 1 stick: small silver metal + large blue non-metal (Li/Na/K + Cl).</summary>
    AlkaliChloride,
    /// <summary>3 spheres linear, 2 sticks: central silver metal + 2 outer blue non-metals (Mg/Ca + Cl).</summary>
    AlkalineChloride,
    /// <summary>3 spheres V-shape, 2 sticks: central orange oxygen + 2 smaller silver metals (Li/Na/K + O).</summary>
    AlkaliOxide,
    /// <summary>2 spheres, 2 parallel sticks (double bond): silver metal + orange oxygen (Mg/Ca + O).</summary>
    AlkalineOxide,
    /// <summary>3 spheres V-shape, 2 sticks: central yellow sulfur + 2 smaller silver metals (Li/Na/K + S).</summary>
    AlkaliSulfide,
    /// <summary>2 spheres, 1 stick: silver metal + yellow sulfur (Mg/Ca + S).</summary>
    AlkalineSulfide,
}

/// <summary>
/// Procedurally generates 3D ball-and-stick molecular models using Unity primitives.
/// Spheres represent atoms and cylinders represent bonds, colored to match
/// craft-style laboratory ball-and-stick kit aesthetics.
/// </summary>
public static class BallAndStickModelBuilder
{
    // ── Atom colors ──────────────────────────────────────────────────────────
    private static readonly Color SilverMetal = new Color(0.75f, 0.75f, 0.78f, 1f);
    private static readonly Color BlueNonMetal = new Color(0.2f, 0.4f, 0.9f, 1f);
    private static readonly Color OrangeOxygen = new Color(0.9f, 0.45f, 0.1f, 1f);
    private static readonly Color YellowSulfur = new Color(0.85f, 0.75f, 0.1f, 1f);

    // ── Stick color ───────────────────────────────────────────────────────────
    private static readonly Color WoodStick = new Color(0.55f, 0.35f, 0.2f, 1f);

    // ── Sizes ─────────────────────────────────────────────────────────────────
    private const float MetalRadius = 0.1f;
    private const float NonMetalRadius = 0.13f;
    private const float StickRadius = 0.018f;
    private const float BondGap = 0.05f;

    // ── Bond angle for V-shape molecules (e.g. Li2O, Na2S) in degrees ─────────
    private const float VBondAngleDeg = 104.5f;

    /// <summary>
    /// Builds a complete ball-and-stick model GameObject for the given compound type.
    /// The returned GameObject has all atoms and bonds as children.
    /// Returns null if type is None.
    /// </summary>
    public static GameObject BuildModel(CompoundModelType type)
    {
        if (type == CompoundModelType.None)
            return null;

        var root = new GameObject("BallAndStickModel");

        switch (type)
        {
            case CompoundModelType.AlkaliChloride:
                BuildTwoSphereSingleBond(root.transform, SilverMetal, MetalRadius,
                    BlueNonMetal, NonMetalRadius);
                break;

            case CompoundModelType.AlkalineChloride:
                BuildThreeSphereLinear(root.transform, SilverMetal, MetalRadius,
                    BlueNonMetal, NonMetalRadius);
                break;

            case CompoundModelType.AlkaliOxide:
                BuildThreeSphereVShape(root.transform, OrangeOxygen, NonMetalRadius,
                    SilverMetal, MetalRadius);
                break;

            case CompoundModelType.AlkalineOxide:
                BuildTwoSphereDoubleBond(root.transform, SilverMetal, MetalRadius,
                    OrangeOxygen, NonMetalRadius);
                break;

            case CompoundModelType.AlkaliSulfide:
                BuildThreeSphereVShape(root.transform, YellowSulfur, NonMetalRadius,
                    SilverMetal, MetalRadius);
                break;

            case CompoundModelType.AlkalineSulfide:
                BuildTwoSphereSingleBond(root.transform, SilverMetal, MetalRadius,
                    YellowSulfur, NonMetalRadius);
                break;
        }

        return root;
    }

    // ── Primitive creation helpers ─────────────────────────────────────────────

    /// <summary>Creates a colored sphere atom at the given local position.</summary>
    private static GameObject CreateAtom(Transform parent, string name, Vector3 localPos,
        Color color, float radius)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * radius;

        // Remove the default collider — this is a display model, not a physics object.
        var collider = go.GetComponent<Collider>();
        if (collider != null) Object.Destroy(collider);

        var rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = color;
            rend.material = mat;
        }

        return go;
    }

    /// <summary>Creates a wooden stick (cylinder) between two local positions.</summary>
    private static GameObject CreateStick(Transform parent, string name,
        Vector3 from, Vector3 to, float radius)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);

        Vector3 midpoint = (from + to) * 0.5f;
        float distance = Vector3.Distance(from, to);

        go.transform.localPosition = midpoint;
        // Unity cylinders are 1 unit tall along Y by default; scale Y to half-distance.
        go.transform.localScale = new Vector3(radius, distance * 0.5f, radius);

        // Align the cylinder's Y axis with the bond direction.
        Vector3 direction = (to - from).normalized;
        if (direction != Vector3.zero)
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);

        var collider = go.GetComponent<Collider>();
        if (collider != null) Object.Destroy(collider);

        var rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = WoodStick;
            rend.material = mat;
        }

        return go;
    }

    // ── Model structure builders ───────────────────────────────────────────────

    /// <summary>
    /// Two spheres connected by a single stick.
    /// Used for: LiCl, NaCl, KCl (alkali + chlorine) and MgS, CaS (alkaline + sulfur).
    /// </summary>
    private static void BuildTwoSphereSingleBond(Transform root,
        Color colorA, float radiusA, Color colorB, float radiusB)
    {
        float separation = radiusA + radiusB + BondGap;
        Vector3 posA = new Vector3(-separation * 0.5f, 0f, 0f);
        Vector3 posB = new Vector3(separation * 0.5f, 0f, 0f);

        CreateAtom(root, "AtomA", posA, colorA, radiusA);
        CreateAtom(root, "AtomB", posB, colorB, radiusB);
        CreateStick(root, "Bond", posA, posB, StickRadius);
    }

    /// <summary>
    /// Three spheres in a straight line connected by two sticks.
    /// Used for: MgCl2, CaCl2 (alkaline earth + chlorine).
    /// </summary>
    private static void BuildThreeSphereLinear(Transform root,
        Color centerColor, float centerRadius, Color outerColor, float outerRadius)
    {
        float separation = centerRadius + outerRadius + BondGap;
        Vector3 centerPos = Vector3.zero;
        Vector3 leftPos = new Vector3(-separation, 0f, 0f);
        Vector3 rightPos = new Vector3(separation, 0f, 0f);

        CreateAtom(root, "CenterAtom", centerPos, centerColor, centerRadius);
        CreateAtom(root, "LeftAtom", leftPos, outerColor, outerRadius);
        CreateAtom(root, "RightAtom", rightPos, outerColor, outerRadius);
        CreateStick(root, "BondLeft", centerPos, leftPos, StickRadius);
        CreateStick(root, "BondRight", centerPos, rightPos, StickRadius);
    }

    /// <summary>
    /// Three spheres in a bent V-shape connected by two sticks.
    /// Used for: Li2O, Na2O, K2O (alkali + oxygen) and Li2S, Na2S, K2S (alkali + sulfur).
    /// </summary>
    private static void BuildThreeSphereVShape(Transform root,
        Color centerColor, float centerRadius, Color outerColor, float outerRadius)
    {
        float separation = centerRadius + outerRadius + BondGap;
        float halfAngle = VBondAngleDeg * 0.5f * Mathf.Deg2Rad;

        Vector3 centerPos = Vector3.zero;
        Vector3 leftPos = new Vector3(
            -Mathf.Sin(halfAngle) * separation,
            -Mathf.Cos(halfAngle) * separation,
            0f);
        Vector3 rightPos = new Vector3(
            Mathf.Sin(halfAngle) * separation,
            -Mathf.Cos(halfAngle) * separation,
            0f);

        CreateAtom(root, "CenterAtom", centerPos, centerColor, centerRadius);
        CreateAtom(root, "LeftAtom", leftPos, outerColor, outerRadius);
        CreateAtom(root, "RightAtom", rightPos, outerColor, outerRadius);
        CreateStick(root, "BondLeft", centerPos, leftPos, StickRadius);
        CreateStick(root, "BondRight", centerPos, rightPos, StickRadius);
    }

    /// <summary>
    /// Two spheres connected by two parallel sticks (double bond).
    /// Used for: MgO, CaO (alkaline earth + oxygen).
    /// </summary>
    private static void BuildTwoSphereDoubleBond(Transform root,
        Color colorA, float radiusA, Color colorB, float radiusB)
    {
        float separation = radiusA + radiusB + BondGap;
        Vector3 posA = new Vector3(-separation * 0.5f, 0f, 0f);
        Vector3 posB = new Vector3(separation * 0.5f, 0f, 0f);

        // Offset the two sticks along Z so both are visible side-by-side.
        float stickOffset = 0.02f;
        Vector3 offsetZ = new Vector3(0f, 0f, stickOffset);

        CreateAtom(root, "AtomA", posA, colorA, radiusA);
        CreateAtom(root, "AtomB", posB, colorB, radiusB);
        CreateStick(root, "Bond1", posA + offsetZ, posB + offsetZ, StickRadius);
        CreateStick(root, "Bond2", posA - offsetZ, posB - offsetZ, StickRadius);
    }
}
