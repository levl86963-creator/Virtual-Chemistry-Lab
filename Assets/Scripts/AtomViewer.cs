using UnityEngine;

public class AtomViewer : MonoBehaviour
{
    [Header("References")]
    public Transform nucleus;
    public Transform shell1;
    public Transform shell2;
    public Transform shell3;
    public Transform shell4;
    public GameObject electronPrefab;

    [Header("Settings")]
    public float shell1Radius = 0.3f;
    public float shell2Radius = 0.5f;
    public float shell3Radius = 0.7f;
    public float shell4Radius = 0.9f;
    public float orbitSpeed1 = 90f;
    public float orbitSpeed2 = 60f;
    public float orbitSpeed3 = 40f;
    public float orbitSpeed4 = 25f;

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
        int[] shells = CalculateShells(atomicNumber);
        SpawnElectronsOnShell(shell1, shells[0], shell1Radius);
        SpawnElectronsOnShell(shell2, shells[1], shell2Radius);
        SpawnElectronsOnShell(shell3, shells[2], shell3Radius);
        SpawnElectronsOnShell(shell4, shells[3], shell4Radius);
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
        ClearShell(shell1);
        ClearShell(shell2);
        ClearShell(shell3);
        ClearShell(shell4);
    }

    void ClearShell(Transform shell)
    {
        if (shell == null) return;
        for (int i = shell.childCount - 1; i >= 0; i--)
            Destroy(shell.GetChild(i).gameObject);
    }
}