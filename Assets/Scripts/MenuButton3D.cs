using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attaches to a 3D button in the menu. When poked/selected via XR,
/// it notifies the MenuController which action to perform.
/// </summary>
public class MenuButton3D : MonoBehaviour
{
    public enum ButtonAction { StartExperiment, StartPeriodicTable, StartBond, StartQuiz, Exit }

    [Header("What this button does")]
    public ButtonAction action;

    [Header("Visual feedback")]
    public Color normalColor = new Color(0.2f, 0.6f, 0.3f);
    public Color pressedColor = Color.white;

    [Header("Label settings")]
    [Tooltip("Local scale applied to the child Label TextMesh at start")]
    public float labelScale = 0.005f;

    private XRSimpleInteractable _interactable;
    private Renderer _renderer;
    private bool _cooling;
    private bool _isSelected;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _renderer = GetComponent<Renderer>();

        // Fix interaction layer so buttons respond to any ray interactor
        if (_interactable != null)
            _interactable.interactionLayers = ~0; // Everything

        if (_renderer != null)
            _renderer.material.color = normalColor;

        // Preserve the authored label scale and font size so world-space text
        // keeps the same dimensions in the Scene and Game views.
    }

    void OnEnable()
    {
        if (_interactable != null)
        {
            _interactable.selectEntered.AddListener(OnSelected);
            _interactable.activated.AddListener(OnActivated);
            _interactable.hoverEntered.AddListener(OnHoverEnter);
            _interactable.hoverExited.AddListener(OnHoverExit);
        }
    }

    void OnDisable()
    {
        if (_interactable != null)
        {
            _interactable.selectEntered.RemoveListener(OnSelected);
            _interactable.activated.RemoveListener(OnActivated);
            _interactable.hoverEntered.RemoveListener(OnHoverEnter);
            _interactable.hoverExited.RemoveListener(OnHoverExit);
        }
    }

    private void OnSelected(SelectEnterEventArgs args) => TriggerButton();
    private void OnActivated(ActivateEventArgs args) => TriggerButton();

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (_renderer == null || _cooling || _isSelected) return;
        Color bright = new Color(
            Mathf.Clamp01(normalColor.r * 1.4f),
            Mathf.Clamp01(normalColor.g * 1.4f),
            Mathf.Clamp01(normalColor.b * 1.4f),
            normalColor.a);
        _renderer.material.color = bright;
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (_renderer == null || _cooling || _isSelected) return;
        _renderer.material.color = normalColor;
    }

public void TriggerButton()
    {
        if (_cooling) return;
        _cooling = true;

        // Flash pressed color + punch scale animation, THEN fire action
        StartCoroutine(PressAnimation());
    }

/// <summary>
    /// Scale-punch + color flash, fires action AFTER animation so the button
    /// isn't destroyed before the coroutine can finish.
    /// </summary>
    private System.Collections.IEnumerator PressAnimation()
    {
        Vector3 originalScale = transform.localScale;

        // --- flash white ---
        if (_renderer != null)
            _renderer.material.color = pressedColor;

        // --- punch: scale UP (pop forward) ---
        float elapsed = 0f;
        float punchDuration = 0.08f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.15f, t);
            yield return null;
        }

        // --- punch: scale back DOWN ---
        elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;
            transform.localScale = Vector3.Lerp(originalScale * 1.15f, originalScale, t);
            yield return null;
        }
        transform.localScale = originalScale;

        // Keep the selected button illuminated until the user presses Exit.
        if (action != ButtonAction.Exit)
        {
            var menuController = FindFirstObjectByType<MenuController>();
            if (menuController != null)
                menuController.SetSelectedButton(this);
        }

        var mc = FindFirstObjectByType<MenuController>();

        if (mc != null)
        {
            switch (action)
            {
                case ButtonAction.StartExperiment:    mc.OnStartExperiment();    break;
                case ButtonAction.StartPeriodicTable: mc.OnStartPeriodicTable(); break;
                case ButtonAction.StartBond:          mc.OnStartBond();          break;
                case ButtonAction.StartQuiz:          mc.OnStartQuiz();          break;
                case ButtonAction.Exit:               mc.OnExit();               break;
            }
        }
        else
        {
            Debug.LogWarning("[MenuButton3D] MenuController not found!");
        }

        yield return new WaitForSeconds(0.3f);
        _cooling = false;
    }

    public void SetSelectedVisual(bool selected)
    {
        _isSelected = selected;
        if (_renderer != null)
            _renderer.material.color = selected ? pressedColor : normalColor;
    }



private System.Collections.IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(0.3f);
        if (_renderer != null)
            _renderer.material.color = normalColor;
        yield return new WaitForSeconds(0.5f);
        _cooling = false;
    }
}
