using System.Collections;
using UnityEngine;

/// Attach this to the same GameObject as ReactionZone (the water beaker).
/// Handles the H2 ignition flash, boom sound, and camera shake for Na+H2O.
public class ReactionEffects : MonoBehaviour
{
    [Header("Flash Light")]
    [Tooltip("A Point light child of this beaker used for the ignition flash")]
    public Light flashLight;
    public float flashPeakIntensity = 8f;
    public Color flashColor = new Color(1f, 0.85f, 0.4f); // warm yellow-white
    public float flashDuration = 0.25f;

    [Header("Camera Shake")]
    public float shakeDuration = 0.45f;
    public float shakeMagnitude = 0.012f;  // subtle — realistic small pop
    public float shakeFrequency = 28f;     // fast vibration

    [Header("Boom Sound")]
    public AudioClip boomClip;
    [Range(0f, 1f)] public float boomVolume = 0.7f;

    private Transform _cameraTransform;
    private Vector3 _cameraOriginalLocalPos;

    void Awake()
    {
        // Auto-create flash light if not assigned
        if (flashLight == null)
        {
            GameObject lightGO = new GameObject("ReactionFlashLight");
            lightGO.transform.SetParent(transform);
            lightGO.transform.localPosition = Vector3.up * 0.1f;
            flashLight = lightGO.AddComponent<Light>();
            flashLight.type = LightType.Point;
            flashLight.range = 3f;
            flashLight.intensity = 0f;
            flashLight.color = flashColor;
            flashLight.enabled = false;
        }
    }

    /// Call this from ReactionZone at the moment of H2 ignition (spark phase).
    public void TriggerIgnitionEffect()
    {
        StartCoroutine(FlashRoutine());
        StartCoroutine(ShakeRoutine());
        if (boomClip != null)
            AudioSource.PlayClipAtPoint(boomClip, transform.position, boomVolume);
        else
            Debug.LogWarning("[ReactionEffects] boomClip not assigned — assign a boom/pop AudioClip.");
    }

    private IEnumerator FlashRoutine()
    {
        flashLight.enabled = true;
        flashLight.color = flashColor;

        // Attack: ramp up fast
        float t = 0f;
        float attackTime = flashDuration * 0.2f;
        while (t < attackTime)
        {
            t += Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(0f, flashPeakIntensity, t / attackTime);
            yield return null;
        }

        // Decay: ramp down slower
        t = 0f;
        float decayTime = flashDuration * 0.8f;
        while (t < decayTime)
        {
            t += Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(flashPeakIntensity, 0f, t / decayTime);
            yield return null;
        }

        flashLight.intensity = 0f;
        flashLight.enabled = false;
    }

    private IEnumerator ShakeRoutine()
    {
        // Find the XR camera to shake
        if (_cameraTransform == null)
            _cameraTransform = FindCameraTransform();

        if (_cameraTransform == null)
        {
            Debug.LogWarning("[ReactionEffects] Could not find camera for shake.");
            yield break;
        }

        _cameraOriginalLocalPos = _cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;

            // Envelope: full shake at start, fades out
            float envelope = 1f - progress;

            float offsetX = Mathf.Sin(elapsed * shakeFrequency * 1.1f) * shakeMagnitude * envelope;
            float offsetY = Mathf.Sin(elapsed * shakeFrequency * 0.9f) * shakeMagnitude * envelope;

            _cameraTransform.localPosition = _cameraOriginalLocalPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        // Restore original position
        _cameraTransform.localPosition = _cameraOriginalLocalPos;
    }

    private Transform FindCameraTransform()
    {
        // Prefer XR camera offset (shaking localPos gives head-relative feel)
        var xrOrigin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null && xrOrigin.CameraFloorOffsetObject != null)
            return xrOrigin.CameraFloorOffsetObject.transform;

        if (Camera.main != null)
            return Camera.main.transform;

        return null;
    }
}
