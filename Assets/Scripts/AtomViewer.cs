using UnityEngine;
using TMPro;

public class AtomViewer : MonoBehaviour
{
    [Header("References")]
    public Transform nucleus;
    public Transform shell1;
    public Transform shell2;
    public Transform shell3;
    public Transform shell4;
    public GameObject electronPrefab;

    [Header("Nucleus")]
    [Range(0.01f, 1.0f)] public float nucleusScale = 0.06f;
    public Color nucleusColor = new Color(1f, 0.55f, 0.15f);

    [Header("Shell Radii")]
    public float shell1Radius = 0.35f;
    public float shell2Radius = 0.65f;
    public float shell3Radius = 1.0f;
    public float shell4Radius = 1.4f;

    [Header("Orbit Speeds")]
    [Range(0f, 90f)] public float orbitSpeed1 = 22f;
    [Range(0f, 90f)] public float orbitSpeed2 = 16f;
    [Range(0f, 90f)] public float orbitSpeed3 = 11f;
    [Range(0f, 90f)] public float orbitSpeed4 = 7f;

    [Header("Electrons")]
    [Range(0.001f, 0.2f)] public float electronScale = 0.05f;
    public Color electronColor = new Color(1f, 0.75f, 0.3f);
    public float electronEmission = 1.5f;

    [Header("Orbit Rings")]
    public Color ringColor = new Color(1f, 0.55f, 0.15f);
    [Range(0.001f, 0.05f)] public float ringWidth = 0.015f;

    void Awake()
    {
        // Disable root mesh renderer and collider to prevent outer sphere mesh/collision jitter
        MeshRenderer rootRend = GetComponent<MeshRenderer>();
        if (rootRend != null) rootRend.enabled = false;

        Collider rootCol = GetComponent<Collider>();
        if (rootCol != null) rootCol.enabled = false;

        if (nucleus == null)
            nucleus = transform.Find("Nucleus");

        if (nucleus != null)
        {
            nucleus.localScale = Vector3.one * nucleusScale;

            Renderer rend = nucleus.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = nucleusColor;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", nucleusColor * 2.0f);
                rend.material = mat;
            }

            TextMeshPro tmp = nucleus.GetComponentInChildren<TextMeshPro>(true);
            if (tmp == null)
            {
                GameObject t = new GameObject("NucleusText");
                t.transform.SetParent(nucleus, false);
                tmp = t.AddComponent<TextMeshPro>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 3f;
                tmp.fontStyle = FontStyles.Bold;
            }
        }

        DisableShellMeshes(shell1);
        DisableShellMeshes(shell2);
        DisableShellMeshes(shell3);
        DisableShellMeshes(shell4);
    }

    void Update()
    {
        if (shell1 != null) shell1.Rotate(Vector3.up, orbitSpeed1 * Time.deltaTime);
        if (shell2 != null) shell2.Rotate(Vector3.up, orbitSpeed2 * Time.deltaTime);
        if (shell3 != null) shell3.Rotate(Vector3.up, orbitSpeed3 * Time.deltaTime);
        if (shell4 != null) shell4.Rotate(Vector3.up, orbitSpeed4 * Time.deltaTime);
    }

    public void BuildAtom(int atomicNumber)
    {
        ClearElectrons();
        RemoveAllRings();

        int[] shells = CalculateShells(atomicNumber);
        Debug.Log($"[AtomViewer] Shells: {shells[0]}, {shells[1]}, {shells[2]}, {shells[3]}");

        if (shells[0] > 0) { SpawnElectronsOnShell(shell1, shells[0], shell1Radius); DrawOrbitRing(shell1, shell1Radius); }
        if (shells[1] > 0) { SpawnElectronsOnShell(shell2, shells[1], shell2Radius); DrawOrbitRing(shell2, shell2Radius); }
        if (shells[2] > 0) { SpawnElectronsOnShell(shell3, shells[2], shell3Radius); DrawOrbitRing(shell3, shell3Radius); }
        if (shells[3] > 0) { SpawnElectronsOnShell(shell4, shells[3], shell4Radius); DrawOrbitRing(shell4, shell4Radius); }

        if (nucleus != null)
        {
            TextMeshPro tmp = nucleus.GetComponentInChildren<TextMeshPro>(true);
            if (tmp != null)
            {
                tmp.text = atomicNumber.ToString();
                tmp.color = Color.white;
            }
        }
    }

    int[] CalculateShells(int atomicNumber)
    {
        int[] shells = new int[4];
        int remaining = atomicNumber;
        shells[0] = Mathf.Min(remaining, 2); remaining -= shells[0];
        shells[1] = Mathf.Min(remaining, 8); remaining -= shells[1];
        shells[2] = Mathf.Min(remaining, 8); remaining -= shells[2];
        shells[3] = remaining;
        return shells;
    }

    void SpawnElectronsOnShell(Transform shell, int count, float radius)
    {
        if (shell == null || count <= 0 || electronPrefab == null) return;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            GameObject electron = Instantiate(electronPrefab, shell);
            electron.transform.localPosition = pos;
            electron.transform.localRotation = Quaternion.identity;
            electron.transform.localScale = Vector3.one * electronScale;

            Renderer rend = electron.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = electronColor;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", electronColor * electronEmission);
                rend.material = mat;
            }
        }
    }

    void DrawOrbitRing(Transform shell, float radius)
    {
        if (shell == null) return;

        LineRenderer lr = shell.gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.positionCount = 64;

        Material lineMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        lineMat.color = ringColor;
        lr.material = lineMat;

        for (int i = 0; i < 64; i++)
        {
            float angle = (float)i / 63f * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
        }
    }

    void RemoveAllRings()
    {
        RemoveRing(shell1);
        RemoveRing(shell2);
        RemoveRing(shell3);
        RemoveRing(shell4);
    }

    void RemoveRing(Transform shell)
    {
        if (shell == null) return;
        LineRenderer lr = shell.GetComponent<LineRenderer>();
        if (lr != null) Destroy(lr);
    }

    void DisableShellMeshes(Transform shell)
    {
        if (shell == null) return;
        Renderer[] renderers = shell.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
        {
            if (rend is LineRenderer) continue;
            rend.enabled = false;
        }
    }

    void ClearElectrons()
    {
        ClearShell(shell1);
        ClearShell(shell2);
        ClearShell(shell3);
        ClearShell(shell4);
    }

    void ClearShell(Transform shell)
    {
        if (shell == null) return;
        for (int i = shell.childCount - 1; i >= 0; i--)
        {
            Transform child = shell.GetChild(i);
            if (child.name == "NucleusText") continue;
            Destroy(child.gameObject);
        }
    }
}