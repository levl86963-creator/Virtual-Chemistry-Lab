using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Animates all 5 finger bones of a 3D hand model into a grip pose
/// when the linked XR interactor selects (grabs) an object,
/// and returns to the open-hand rest pose on release.
/// Place this on the hand model root (the parent of the wrist bone).
/// </summary>
public class HandGrabAnimator : MonoBehaviour
{
    [Header("Interactor")]
    [Tooltip("The XR interactor that drives the grab state (e.g. Near-Far Interactor).")]
    [SerializeField] private XRBaseInteractor interactor;

    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed = 12f;

    // Per-joint curl angles in degrees (applied on local X axis)
    private const float ProximalCurl = 75f;
    private const float IntermediateCurl = 60f;
    private const float DistalCurl = 50f;
    private const float ThumbProximalCurl = 35f;
    private const float ThumbDistalCurl = 45f;

    private class FingerBone
    {
        public Transform Transform;
        public Quaternion BaseRotation;
        public Quaternion GripRotation;
    }

    private readonly List<FingerBone> _bones = new List<FingerBone>();
    private float _currentGrip;
    private float _targetGrip;

    private void Awake()
    {
        // Locate the wrist bone (first child whose name contains "Wrist")
        Transform wrist = null;
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Wrist"))
            {
                wrist = child;
                break;
            }
        }

        if (wrist == null)
        {
            Debug.LogError($"{nameof(HandGrabAnimator)}: No wrist bone found under {gameObject.name}", this);
            return;
        }

        // Determine prefix (L_ or R_) from the wrist name
        string prefix = wrist.name.StartsWith("L") ? "L" : "R";

        // Index, Middle, Ring – full 3-joint curl
        SetupFinger(wrist, prefix, "Index", ProximalCurl, IntermediateCurl, DistalCurl);
        SetupFinger(wrist, prefix, "Middle", ProximalCurl, IntermediateCurl, DistalCurl);
        SetupFinger(wrist, prefix, "Ring", ProximalCurl, IntermediateCurl, DistalCurl);

        // Little finger – slightly less curl
        SetupFinger(wrist, prefix, "Little",
            ProximalCurl * 0.85f, IntermediateCurl * 0.85f, DistalCurl * 0.85f);

        // Thumb – 2 joints, different curl amounts
        SetupThumb(wrist, prefix);
    }

    /// <summary>
    /// Registers the three curl joints (proximal, intermediate, distal) of a finger.
    /// </summary>
    private void SetupFinger(Transform wrist, string prefix, string fingerName,
        float proxCurl, float interCurl, float distCurl)
    {
        AddBone(wrist, $"{prefix}_{fingerName}Proximal", proxCurl);
        AddBone(wrist, $"{prefix}_{fingerName}Intermediate", interCurl);
        AddBone(wrist, $"{prefix}_{fingerName}Distal", distCurl);
    }

    /// <summary>
    /// Registers the two curl joints (proximal, distal) of the thumb.
    /// </summary>
    private void SetupThumb(Transform wrist, string prefix)
    {
        AddBone(wrist, $"{prefix}_ThumbProximal", ThumbProximalCurl);
        AddBone(wrist, $"{prefix}_ThumbDistal", ThumbDistalCurl);
    }

    /// <summary>
    /// Stores the rest rotation and computes the gripped rotation for a single bone.
    /// </summary>
    private void AddBone(Transform wrist, string boneName, float curlDegrees)
    {
        Transform bone = FindDeepChild(wrist, boneName);
        if (bone == null)
        {
            Debug.LogWarning($"{nameof(HandGrabAnimator)}: Bone '{boneName}' not found under {wrist.name}");
            return;
        }

        _bones.Add(new FingerBone
        {
            Transform = bone,
            BaseRotation = bone.localRotation,
            GripRotation = bone.localRotation * Quaternion.Euler(curlDegrees, 0f, 0f)
        });
    }

    /// <summary>
    /// Depth-first search for a child Transform by exact name.
    /// </summary>
    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private void Update()
    {
        _targetGrip = interactor != null && interactor.hasSelection ? 1f : 0f;
        _currentGrip = Mathf.Lerp(_currentGrip, _targetGrip, Time.deltaTime * animationSpeed);

        foreach (var bone in _bones)
        {
            bone.Transform.localRotation = Quaternion.Slerp(
                bone.BaseRotation, bone.GripRotation, _currentGrip);
        }
    }
}
