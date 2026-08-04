using UnityEngine;
using UnityEngine.UI;

/// <summary>
<<<<<<< HEAD
/// Initializes the chemistry UI, creates its runtime elements, assigns the XR camera,
/// and wires the element and submit buttons to ChemistrySlotManager.
=======
/// Automatically wires element buttons to the ChemistrySlotManager at runtime.
/// Finds all Btn_* children in each panel, reads their label text,
/// and adds onClick listeners that call the corresponding slot method.
/// Also wires the Submit button to call ChemistrySlotManager.Submit().
/// Sets the Canvas worldCamera at runtime since the UI is in a
/// different scene than the XR Main Camera.
>>>>>>> sombo
/// </summary>
public class ChemistryUIInitializer : MonoBehaviour
{
    private ChemistrySlotManager _manager;
    private Canvas _canvas;
<<<<<<< HEAD
    private bool _buttonsWired;
    private float _cameraRetryTimer;

    private const float CameraRetryInterval = 0.5f;
=======
    private bool _buttonsWired = false;
    private float _cameraRetryTimer = 0f;
    private const float CAMERA_RETRY_INTERVAL = 0.5f;
>>>>>>> sombo

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _manager = GetComponent<ChemistrySlotManager>();

<<<<<<< HEAD
        if (GetComponent<ChemistryUIBuilder>() == null)
        {
            gameObject.AddComponent<ChemistryUIBuilder>();
            Debug.Log("[ChemistryUI] Runtime builder attached to " + gameObject.name);
        }
=======
        // Add the builder component if not already present.
        // The builder's Awake runs synchronously during AddComponent,
        // creating the Submit button and Result canvas before any
        // OnEnable or Start calls on other components.
        if (GetComponent<ChemistryUIBuilder>() == null)
            gameObject.AddComponent<ChemistryUIBuilder>();
>>>>>>> sombo
    }

    private void Start()
    {
<<<<<<< HEAD
=======
        // Wire buttons in Start — all GameObjects and components
        // are guaranteed to be initialized by this point.
>>>>>>> sombo
        WireUpButtons();
        SetupCamera();
    }

    private void Update()
    {
<<<<<<< HEAD
        if (_canvas != null && _canvas.worldCamera == null)
        {
            _cameraRetryTimer += Time.deltaTime;
            if (_cameraRetryTimer >= CameraRetryInterval)
=======
        // Retry setting the worldCamera periodically until it's set.
        // This handles the case where the Camera scene loads after
        // this script starts, so Camera.main is initially null.
        if (_canvas != null && _canvas.worldCamera == null)
        {
            _cameraRetryTimer += Time.deltaTime;
            if (_cameraRetryTimer >= CAMERA_RETRY_INTERVAL)
>>>>>>> sombo
            {
                _cameraRetryTimer = 0f;
                SetupCamera();
            }
        }

<<<<<<< HEAD
        if (!_buttonsWired)
            WireUpButtons();
    }

    private void SetupCamera()
    {
        if (_canvas == null)
            return;

        Camera camera = _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = GameObject.FindWithTag("MainCamera");
            if (cameraObject != null)
                camera = cameraObject.GetComponent<Camera>();
        }

        if (camera == null)
            return;

        if (_canvas.worldCamera == null)
        {
            _canvas.worldCamera = camera;
            Debug.Log("[ChemistryUI] worldCamera set to: " + camera.gameObject.name);
        }

        SetupResultCanvasCamera(camera);
    }

    private void SetupResultCanvasCamera(Camera camera)
    {
        Transform parent = transform.parent;
        if (parent == null)
            return;

        Transform resultTransform = parent.Find("ResultCanvas");
        if (resultTransform == null)
            return;

        Canvas resultCanvas = resultTransform.GetComponent<Canvas>();
        if (resultCanvas != null && resultCanvas.worldCamera == null)
        {
            resultCanvas.worldCamera = camera;
            Debug.Log("[ChemistryUI] ResultCanvas worldCamera set to: " + camera.gameObject.name);
        }
    }

    private void WireUpButtons()
    {
        if (_manager == null)
            _manager = GetComponent<ChemistrySlotManager>();
        if (_manager == null)
            return;

        int leftButtonCount = WirePanel("LeftPanel", _manager.SelectLeftElement);
        int rightButtonCount = WirePanel("RightPanel", _manager.SelectRightElement);
        bool submitButtonWired = WireSubmitButton();

        _buttonsWired = leftButtonCount > 0 && rightButtonCount > 0 && submitButtonWired;
        if (_buttonsWired)
            Debug.Log($"[ChemistryUI] Buttons wired successfully: left={leftButtonCount}, right={rightButtonCount}, submit=true");
    }

    private bool WireSubmitButton()
    {
        Transform submitTransform = transform.Find("CenterArea/SubmitBtn");
        if (submitTransform == null)
        {
            Debug.LogWarning("[ChemistryUI] SubmitBtn is not ready under CenterArea.");
            return false;
        }

        Button submitButton = submitTransform.GetComponent<Button>();
        if (submitButton == null)
        {
            Debug.LogError("[ChemistryUI] SubmitBtn has no Button component.");
            return false;
        }

        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(_manager.Submit);
        return true;
    }

    private int WirePanel(string panelName, System.Action<string> selectMethod)
    {
        Transform panel = transform.Find(panelName);
        if (panel == null)
        {
            Debug.LogError("[ChemistryUI] Panel not found: " + panelName);
            return 0;
        }
=======
        // If buttons weren't wired in Start (e.g. manager wasn't ready),
        // retry wiring until it succeeds.
        if (!_buttonsWired && _manager != null)
        {
            WireUpButtons();
        }
    }

    /// <summary>
    /// Finds the main camera and assigns it as the Canvas worldCamera.
    /// Tries Camera.main first, then falls back to FindWithTag.
    /// </summary>
    private void SetupCamera()
    {
        if (_canvas == null || _canvas.worldCamera != null)
            return;

        // Try Camera.main (uses the "MainCamera" tag internally)
        var cam = Camera.main;
        if (cam != null)
        {
            _canvas.worldCamera = cam;
            Debug.Log("[ChemistryUI] worldCamera set to: " + cam.gameObject.name);
            SetupResultCanvasCamera(cam);
            return;
        }

        // Fallback: search by tag
        var camObj = GameObject.FindWithTag("MainCamera");
        if (camObj != null)
        {
            cam = camObj.GetComponent<Camera>();
            if (cam != null)
            {
                _canvas.worldCamera = cam;
                Debug.Log("[ChemistryUI] worldCamera set via FindWithTag to: " + camObj.name);
                SetupResultCanvasCamera(cam);
            }
        }
    }

    /// <summary>
    /// Sets the worldCamera on the ResultCanvas sibling if it exists.
    /// </summary>
    private void SetupResultCanvasCamera(Camera cam)
    {
        var parent = transform.parent;
        if (parent == null) return;

        var resultCanvasObj = parent.Find("ResultCanvas");
        if (resultCanvasObj == null) return;

        var resultCanvas = resultCanvasObj.GetComponent<Canvas>();
        if (resultCanvas != null && resultCanvas.worldCamera == null)
        {
            resultCanvas.worldCamera = cam;
            Debug.Log("[ChemistryUI] ResultCanvas worldCamera set to: " + cam.gameObject.name);
        }
    }

    /// <summary>
    /// Finds all element buttons in both panels and connects them to the slot manager.
    /// Each button's label text is captured as the element symbol for the callback.
    /// Also wires the Submit button to the manager's Submit method.
    /// </summary>
    private void WireUpButtons()
    {
        if (_manager == null)
        {
            _manager = GetComponent<ChemistrySlotManager>();
            if (_manager == null)
                return;
        }

        WirePanel("LeftPanel", _manager.SelectLeftElement);
        WirePanel("RightPanel", _manager.SelectRightElement);
        WireSubmitButton();
        _buttonsWired = true;
        Debug.Log("[ChemistryUI] Buttons wired successfully");
    }

    /// <summary>
    /// Wires the Submit button to call the manager's Submit method.
    /// </summary>
    private void WireSubmitButton()
    {
        var submit = transform.Find("CenterArea/SubmitBtn");
        if (submit == null)
        {
            Debug.LogWarning("[ChemistryUI] SubmitBtn not found under CenterArea");
            return;
        }

        var button = submit.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("[ChemistryUI] SubmitBtn has no Button component");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(_manager.Submit);
        Debug.Log("[ChemistryUI] Submit button wired");
    }

    /// <summary>
    /// Wires all Btn_* children in the given panel to the specified selection callback.
    /// </summary>
    /// <param name="panelName">Name of the panel (LeftPanel or RightPanel).</param>
    /// <param name="selectMethod">Callback that receives the element symbol string.</param>
    private void WirePanel(string panelName, System.Action<string> selectMethod)
    {
        var panel = transform.Find(panelName);
        if (panel == null)
            return;
>>>>>>> sombo

        int wiredCount = 0;
        foreach (Transform child in panel)
        {
            if (!child.name.StartsWith("Btn_"))
                continue;

<<<<<<< HEAD
            Button button = child.GetComponent<Button>();
            Text label = child.Find("Label")?.GetComponent<Text>();
            if (button == null || label == null)
                continue;

            string element = label.text.Trim();
=======
            var button = child.GetComponent<Button>();
            if (button == null)
                continue;

            var label = child.Find("Label")?.GetComponent<Text>();
            if (label == null)
                continue;

            // Capture the element symbol from the label text.
            string element = label.text;
>>>>>>> sombo
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => selectMethod(element));
            wiredCount++;
        }
<<<<<<< HEAD

        if (wiredCount == 0)
            Debug.LogWarning("[ChemistryUI] No element buttons found in " + panelName + ".");
        return wiredCount;
=======
        Debug.Log($"[ChemistryUI] {panelName}: wired {wiredCount} buttons");
>>>>>>> sombo
    }
}
