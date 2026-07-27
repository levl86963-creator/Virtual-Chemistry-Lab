using System.Collections;
using UnityEngine;
using TMPro;

public class InfoSlabFollower : MonoBehaviour
{
    [Header("Target (Player Camera)")]
    public Transform playerCamera;

    [Header("Positioning")]
    public float followDistance = 1.1f;
    public float heightOffset = -0.18f;
    public float horizontalOffset = 0.0f;
    public float smoothSpeed = 5f;

    [Header("Scale")]
    public Vector3 defaultScale = new Vector3(0.0012f, 0.0012f, 0.0012f);

    [Header("Slant (Tilt toward player face)")]
    [Range(-45f, 45f)] public float downwardTilt = -15f;
    [Range(-30f, 30f)] public float sideTilt = 0f;

    [Header("UI Text Fields")]
    public TextMeshProUGUI elementNameText;
    public TextMeshProUGUI symbolText;
    public TextMeshProUGUI atomicNumberText;
    public TextMeshProUGUI funFactText;

    [Header("Initial Delay & Prompt")]
    public float initialAppearanceDelay = 2.5f;

    public string defaultElementName = "Periodic Table";
    public string defaultSymbol = "?";
    public string defaultAtomicNumber = "Select Element";
    public string defaultFunFact = "Select an element from the periodic table to view details and 3D atomic structure.";

    private Coroutine initialDelayCoroutine;
    private bool hasSelectedElement = false;

    void Start()
    {
        Debug.Log("[INFO SLAB] Start() on: " + gameObject.name);

        // Make sure we have a canvas - search up and down
        Canvas c = GetCanvas();
        if (c == null)
        {
            Debug.LogError("[INFO SLAB] NO CANVAS FOUND on " + gameObject.name + " or parents/children!");
            return;
        }

        Debug.Log("[INFO SLAB] Canvas found: " + c.name);
        c.enabled = false;

        if (!hasSelectedElement)
        {
            initialDelayCoroutine = StartCoroutine(ShowInitialPromptRoutine());
        }
    }

    private Canvas GetCanvas()
    {
        // Try self first
        Canvas c = GetComponent<Canvas>();
        if (c != null) return c;

        // Try parent
        c = GetComponentInParent<Canvas>();
        if (c != null) return c;

        // Try children
        c = GetComponentInChildren<Canvas>();
        if (c != null) return c;

        // Last resort: any canvas in scene
        return FindAnyObjectByType<Canvas>();
    }

    private IEnumerator ShowInitialPromptRoutine()
    {
        Debug.Log("[INFO SLAB] Coroutine waiting " + initialAppearanceDelay + "s...");
        yield return new WaitForSeconds(initialAppearanceDelay);
        Debug.Log("[INFO SLAB] Coroutine done waiting.");

        if (!hasSelectedElement)
        {
            ShowInitialPrompt();
        }
    }

    public void ShowInitialPrompt()
    {
        Debug.Log("[INFO SLAB] ShowInitialPrompt()");

        if (elementNameText != null) elementNameText.text = defaultElementName;
        else Debug.LogWarning("[INFO SLAB] elementNameText is null");

        if (symbolText != null) symbolText.text = defaultSymbol;
        if (atomicNumberText != null) atomicNumberText.text = defaultAtomicNumber;
        if (funFactText != null) funFactText.text = defaultFunFact;

        ShowCanvas();
        SnapToTargetPosition();
    }

    public void ShowElement(ElementData data)
    {
        if (data == null) return;

        hasSelectedElement = true;

        if (initialDelayCoroutine != null)
        {
            StopCoroutine(initialDelayCoroutine);
            initialDelayCoroutine = null;
        }

        if (elementNameText != null) elementNameText.text = data.elementName;
        if (symbolText != null) symbolText.text = data.symbol;
        if (atomicNumberText != null) atomicNumberText.text = $"Atomic Number: {data.atomicNumber}";
        if (funFactText != null) funFactText.text = data.funFact;

        ShowCanvas();
        SnapToTargetPosition();
    }

    private void ShowCanvas()
    {
        Canvas c = GetCanvas();
        if (c != null)
        {
            c.enabled = true;
            Debug.Log("[INFO SLAB] Canvas ENABLED on: " + c.name);
        }
        else Debug.LogError("[INFO SLAB] FAILED - no canvas found!");
    }

    public void Hide()
    {
        Canvas c = GetCanvas();
        if (c != null) c.enabled = false;
    }

    public void ResetSelection()
    {
        hasSelectedElement = false;
        Hide();
    }

    void LateUpdate()
    {
        Canvas c = GetCanvas();
        if (c == null || !c.enabled) return;

        EnsurePlayerCamera();
        if (playerCamera == null) return;

        UpdatePositionAndRotation(instantSnap: false);
    }

    private void EnsurePlayerCamera()
    {
        if (playerCamera != null) return;

        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
            return;
        }

        var xrOrigin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            var cam = xrOrigin.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                playerCamera = cam.transform;
                return;
            }
        }

        Camera fallback = FindAnyObjectByType<Camera>();
        if (fallback != null) playerCamera = fallback.transform;
    }

    public void SnapToTargetPosition()
    {
        EnsurePlayerCamera();
        if (playerCamera == null)
        {
            Debug.LogError("[INFO SLAB] No camera to snap to!");
            return;
        }

        // Force scale
        if (transform.localScale.sqrMagnitude < 0.0000001f)
            transform.localScale = defaultScale;

        UpdatePositionAndRotation(instantSnap: true);
        Debug.Log("[INFO SLAB] Snapped to: " + transform.position);
    }

    private void UpdatePositionAndRotation(bool instantSnap)
    {
        Vector3 flatForward = playerCamera.forward;
        flatForward.y = 0;
        flatForward = flatForward.sqrMagnitude > 0.001f ? flatForward.normalized : Vector3.forward;

        Vector3 flatRight = playerCamera.right;
        flatRight.y = 0;
        flatRight = flatRight.sqrMagnitude > 0.001f ? flatRight.normalized : Vector3.right;

        Vector3 targetPos = playerCamera.position
                          + (flatForward * followDistance)
                          + (flatRight * horizontalOffset)
                          + (Vector3.up * heightOffset);

        Vector3 lookDir = targetPos - playerCamera.position;
        lookDir.y = 0;

        Quaternion facePlayer;
        if (lookDir.sqrMagnitude > 0.001f)
            facePlayer = Quaternion.LookRotation(lookDir);
        else
            facePlayer = Quaternion.Euler(0, playerCamera.eulerAngles.y, 0);

        Quaternion slant = Quaternion.Euler(downwardTilt, 0f, sideTilt);
        Quaternion targetRot = facePlayer * slant;

        if (instantSnap)
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothSpeed * Time.deltaTime);
        }
    }
}