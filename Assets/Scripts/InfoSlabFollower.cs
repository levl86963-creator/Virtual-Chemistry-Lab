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

    [Header("Sounds")]
    public AudioClip slabAppearSound;
    [Range(0f,1f)] public float slabSoundVolume = 0.5f;

    
[Header("Initial Delay & Prompt")]
    public float initialAppearanceDelay = 2.5f;

    public string defaultElementName = "Periodic Table";
    public string defaultSymbol = "?";
    public string defaultAtomicNumber = "Select Element";
    private const string DefaultPromptElementName = "Periodic Table";
    private const string DefaultPromptSymbol = "?";
    private const string DefaultPromptAtomicNumber = "Select Element";
    private const string DefaultPromptFunFact = "Select an element from the periodic table to view details and 3D atomic structure.";


    public string defaultFunFact = "Select an element from the periodic table to view details and 3D atomic structure.";

    private Canvas _canvas;
    private Coroutine initialDelayCoroutine;
    private bool hasSelectedElement = false;

    private void OnEnable()
    {
        CacheTextReferences();
        if (!hasSelectedElement)
            ApplyDefaultPromptText();
    }

    private void CacheTextReferences()
    {
        Transform background = transform.Find("infoslabbackgroud");
        if (background == null)
            return;

        elementNameText ??= background.Find("Text_ElementName")?.GetComponent<TextMeshProUGUI>();
        symbolText ??= background.Find("Text_Symbol")?.GetComponent<TextMeshProUGUI>();
        atomicNumberText ??= background.Find("Text_AtomicNumber")?.GetComponent<TextMeshProUGUI>();
        funFactText ??= background.Find("Text_FunFact")?.GetComponent<TextMeshProUGUI>();
    }

// Lazy-init so canvas is found whether the GameObject starts active or inactive.
    private Canvas GetOrFindCanvas()
    {
        if (_canvas != null) return _canvas;
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
            Debug.LogError("[INFO SLAB] No Canvas component found on " + gameObject.name + "! InfoSlabFollower must be on the same GameObject as the Canvas.");
        return _canvas;
    }

    void Awake()
    {
        // Pre-cache while active; lazy fallback handles the inactive-start case.
        GetOrFindCanvas();
    }

void Start()
    {
        Debug.Log("[INFO SLAB] Start() on: " + gameObject.name + " | hasSelectedElement=" + hasSelectedElement);

        Canvas c = GetOrFindCanvas();
        if (c == null) return;

        // If an element was already selected before this object became active,
        // don't hide the canvas or start the idle coroutine.
        if (hasSelectedElement)
        {
            Debug.Log("[INFO SLAB] Element already selected — skipping idle setup.");
            return;
        }

        Debug.Log("[INFO SLAB] Canvas found: " + c.name + " — hiding and starting delay coroutine.");
        c.enabled = false;

        initialDelayCoroutine = StartCoroutine(ShowInitialPromptRoutine());
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

    private void ApplyDefaultPromptText()
    {
        CacheTextReferences();

        string promptElementName = string.IsNullOrWhiteSpace(defaultElementName) ? DefaultPromptElementName : defaultElementName;
        string promptSymbol = string.IsNullOrWhiteSpace(defaultSymbol) ? DefaultPromptSymbol : defaultSymbol;
        string promptAtomicNumber = string.IsNullOrWhiteSpace(defaultAtomicNumber) ? DefaultPromptAtomicNumber : defaultAtomicNumber;
        string promptFunFact = string.IsNullOrWhiteSpace(defaultFunFact) ? DefaultPromptFunFact : defaultFunFact;

        elementNameText?.SetText(promptElementName);
        symbolText?.SetText(promptSymbol);
        atomicNumberText?.SetText(promptAtomicNumber);
        funFactText?.SetText(promptFunFact);

        elementNameText?.ForceMeshUpdate();
        symbolText?.ForceMeshUpdate();
        atomicNumberText?.ForceMeshUpdate();
        funFactText?.ForceMeshUpdate();
    }

    /// <summary>
    /// Shows the default periodic-table prompt immediately when the periodic-table mode opens.
    /// </summary>
    public void ShowPeriodicTablePrompt()
    {
        hasSelectedElement = true;
        gameObject.SetActive(true);
        ApplyDefaultPromptText();
        ShowCanvas();
        SnapToTargetPosition();
        PlaySlabSound();
    }




public void ShowInitialPrompt()
    {
        CacheTextReferences();
        Debug.Log("[INFO SLAB] ShowInitialPrompt()");

        string promptElementName = string.IsNullOrWhiteSpace(defaultElementName) ? DefaultPromptElementName : defaultElementName;
        string promptSymbol = string.IsNullOrWhiteSpace(defaultSymbol) ? DefaultPromptSymbol : defaultSymbol;
        string promptAtomicNumber = string.IsNullOrWhiteSpace(defaultAtomicNumber) ? DefaultPromptAtomicNumber : defaultAtomicNumber;
        string promptFunFact = string.IsNullOrWhiteSpace(defaultFunFact) ? DefaultPromptFunFact : defaultFunFact;

        if (elementNameText != null) elementNameText.SetText(promptElementName);
        else Debug.LogWarning("[INFO SLAB] elementNameText is null");

        if (symbolText != null) symbolText.SetText(promptSymbol);
        if (atomicNumberText != null) atomicNumberText.SetText(promptAtomicNumber);
        if (funFactText != null) funFactText.SetText(promptFunFact);

        elementNameText?.ForceMeshUpdate();
        symbolText?.ForceMeshUpdate();
        atomicNumberText?.ForceMeshUpdate();
        funFactText?.ForceMeshUpdate();

        ShowCanvas();
        SnapToTargetPosition();
        PlaySlabSound();
    }

public void ShowElement(ElementData data)
    {
        if (data == null) return;

        // Set this BEFORE SetActive so that Start() sees it and skips
        // the 2.5s coroutine and canvas-disable logic.
        hasSelectedElement = true;

        // Wake up the GameObject if it was left inactive in the scene.
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (initialDelayCoroutine != null)
        {
            StopCoroutine(initialDelayCoroutine);
            initialDelayCoroutine = null;
        }

        if (elementNameText != null) elementNameText.text = data.elementName;
        else Debug.LogWarning("[INFO SLAB] elementNameText is null!");

        if (symbolText != null) symbolText.text = data.symbol;
        else Debug.LogWarning("[INFO SLAB] symbolText is null!");

        if (atomicNumberText != null) atomicNumberText.text = $"Atomic Number: {data.atomicNumber}";
        else Debug.LogWarning("[INFO SLAB] atomicNumberText is null!");

        if (funFactText != null) funFactText.text = data.funFact;
        else Debug.LogWarning("[INFO SLAB] funFactText is null!");

        Debug.Log($"[INFO SLAB] ShowElement: {data.elementName} | sym={symbolText?.text} | num={atomicNumberText?.text}");

        ShowCanvas();
        SnapToTargetPosition();
        PlaySlabSound();
    }

private void PlaySlabSound()
    {
        if (slabAppearSound == null) return;
        AudioSource src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        src.PlayOneShot(slabAppearSound, slabSoundVolume);
    }

    
private void ShowCanvas()
    {
        Canvas c = GetOrFindCanvas();
        if (c != null)
        {
            c.enabled = true;
            Debug.Log("[INFO SLAB] Canvas ENABLED on: " + c.name);
        }
        else Debug.LogError("[INFO SLAB] FAILED - no canvas found!");
    }

public void Hide()
    {
        Canvas c = GetOrFindCanvas();
        if (c != null) c.enabled = false;
    }

    public void ResetSelection()
    {
        hasSelectedElement = false;
        Hide();
    }

void LateUpdate()
    {
        Canvas c = GetOrFindCanvas();
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
