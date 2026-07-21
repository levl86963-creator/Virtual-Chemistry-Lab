using UnityEngine;

public class AtomViewer : MonoBehaviour
{
    [Header("References")]
    public Transform nucleus;
    public Transform shell1;  // K shell
    public Transform shell2;  // L shell
    public Transform shell3;  // M shell
    public Transform shell4;  // N shell

    [Header("Settings")]
    public GameObject electronPrefab;  // A small sphere prefab
    public float shell1Radius = 0.15f;  // was 0.3f
    public float shell2Radius = 0.25f;  // was 0.5f
    public float shell3Radius = 0.35f;  // was 0.7f
    public float shell4Radius = 0.45f;  // was 0.9f
    public float orbitSpeed1 = 90f;
    public float orbitSpeed2 = 60f;
    public float orbitSpeed3 = 40f;
    public float orbitSpeed4 = 25f;

    private GameObject[] electrons;

    void Update()
    {
        // Rotate each shell to make electrons orbit
        if (shell1 != null) shell1.Rotate(Vector3.up, orbitSpeed1 * Time.deltaTime);
        if (shell2 != null) shell2.Rotate(Vector3.up, orbitSpeed2 * Time.deltaTime);
        if (shell3 != null) shell3.Rotate(Vector3.up, orbitSpeed3 * Time.deltaTime);
        if (shell4 != null) shell4.Rotate(Vector3.up, orbitSpeed4 * Time.deltaTime);
    }

    public void BuildAtom(int atomicNumber)
    {
        // Clear old electrons
        ClearElectrons();

        // Calculate electrons per shell
        int[] shellElectrons = CalculateShells(atomicNumber);

        // Spawn electrons on each shell
        SpawnElectronsOnShell(shell1, shellElectrons[0], shell1Radius);
        SpawnElectronsOnShell(shell2, shellElectrons[1], shell2Radius);
        SpawnElectronsOnShell(shell3, shellElectrons[2], shell3Radius);
        SpawnElectronsOnShell(shell4, shellElectrons[3], shell4Radius);
    }

    int[] CalculateShells(int atomicNumber)
    {
        int[] shells = new int[4];
        int remaining = atomicNumber;

        // K shell: max 2
        shells[0] = Mathf.Min(remaining, 2);
        remaining -= shells[0];

        // L shell: max 8
        shells[1] = Mathf.Min(remaining, 8);
        remaining -= shells[1];

        // M shell: max 8
        shells[2] = Mathf.Min(remaining, 8);
        remaining -= shells[2];

        // N shell: remainder
        shells[3] = remaining;

        return shells;
    }

    void SpawnElectronsOnShell(Transform shell, int count, float radius)
    {
        if (shell == null || count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            GameObject electron = Instantiate(electronPrefab, shell);
            electron.transform.localPosition = pos;
            electron.transform.localRotation = Quaternion.identity;
        }
    }

    void ClearElectrons()
    {
        // Destroy all children of shell containers
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
            Destroy(shell.GetChild(i).gameObject);
        }
    }
}