using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Initializes the chemistry UI, creates its runtime elements, assigns the XR camera,
/// and wires the element and submit buttons to ChemistrySlotManager.
/// </summary>
public class ChemistryUIInitializer : MonoBehaviour
{
    private ChemistrySlotManager _manager;
    private Canvas _canvas;
    private bool _buttonsWired;
    private float _cameraRetryTimer;

    private const float CameraRetryInterval = 0.5f;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _manager = GetComponent<ChemistrySlotManager>();

        if (GetComponent<ChemistryUIBuilder>() == null)
        {
            gameObject.AddComponent<ChemistryUIBuilder>();
            Debug.Log("[ChemistryUI] Runtime builder attached to " + gameObject.name);
        }
    }

    private void Start()
    {
        WireUpButtons();
        SetupCamera();
    }

    private void Update()
    {
        if (_canvas != null && _canvas.worldCamera == null)
        {
            _cameraRetryTimer += Time.deltaTime;
            if (_cameraRetryTimer >= CameraRetryInterval)
            {
                _cameraRetryTimer = 0f;
                SetupCamera();
            }
        }

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

        int wiredCount = 0;
        foreach (Transform child in panel)
        {
            if (!child.name.StartsWith("Btn_"))
                continue;

            Button button = child.GetComponent<Button>();
            Text label = child.Find("Label")?.GetComponent<Text>();
            if (button == null || label == null)
                continue;

            string element = label.text.Trim();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => selectMethod(element));
            wiredCount++;
        }

        if (wiredCount == 0)
            Debug.LogWarning("[ChemistryUI] No element buttons found in " + panelName + ".");
        return wiredCount;
    }
}
