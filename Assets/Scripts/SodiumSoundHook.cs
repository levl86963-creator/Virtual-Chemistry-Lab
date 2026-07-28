using System.Reflection;
using UnityEngine;

/// Added automatically by LabSoundManager to each Sodium object.
/// Plays pickup sound on grab, splash sound when it hits the beaker.
[RequireComponent(typeof(Rigidbody))]
public class SodiumSoundHook : MonoBehaviour
{
    [HideInInspector] public AudioClip pickupSound;
    [HideInInspector] public AudioClip splashSound;
    [HideInInspector] public float volume = 0.75f;

    private bool _splashPlayed = false;

    void OnEnable()
    {
        // Hook XRGrabInteractable selectEntered/selectExited via reflection
        // so we avoid a hard compile dependency on the XRI assembly version.
        MonoBehaviour grab = GetComponent("XRGrabInteractable") as MonoBehaviour;
        if (grab == null) return;

        System.Type grabType = grab.GetType();
        HookUnityEvent(grab, grabType, "selectEntered", "PlayPickupSound");
    }

    private void HookUnityEvent(MonoBehaviour target, System.Type type,
                                 string fieldName, string callbackName)
    {
        FieldInfo field = type.GetField(fieldName,
            BindingFlags.Public | BindingFlags.Instance);
        if (field == null) return;

        object evt = field.GetValue(target);
        if (evt == null) return;

        MethodInfo addListener = evt.GetType().GetMethod("AddListener");
        if (addListener == null) return;

        System.Type delegateType = addListener.GetParameters()[0].ParameterType;
        MethodInfo handler = GetType().GetMethod(callbackName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (handler == null) return;

        try
        {
            System.Delegate del = System.Delegate.CreateDelegate(delegateType, this, handler);
            addListener.Invoke(evt, new object[] { del });
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[SodiumSoundHook] Could not bind grab event: " + e.Message +
                             " — collision-only detection active.");
        }
    }

    // Called by XRGrabInteractable.selectEntered (signature matches UnityAction<SelectEnterEventArgs>)
    // We use object parameter to accept any event args type without a hard reference.
    public void PlayPickupSound(object args)
    {
        _splashPlayed = false;
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
    }

    void OnCollisionEnter(Collision col)
    {
        if (_splashPlayed) return;

        bool hitWater = col.gameObject.name.ToLower().Contains("beaker") ||
                        col.gameObject.name.ToLower().Contains("water") ||
                        col.gameObject.GetComponent<ReactionZone>() != null;

        if (hitWater && splashSound != null)
        {
            _splashPlayed = true;
            AudioSource.PlayClipAtPoint(splashSound, transform.position, volume * 0.8f);
        }
    }
}
