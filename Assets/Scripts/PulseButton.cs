using UnityEngine;

public class PulseButton : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float scaleAmount = 0.05f;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float s = 1 + Mathf.Sin(Time.time * pulseSpeed) * scaleAmount;
        transform.localScale = baseScale * s;
    }
}