using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main menu controller for VR Chem Lab.
/// Handles menu navigation, experiment visibility, and persistent button highlighting.
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("Lab Systems")]
    public GameObject chemistryUIHolder;
    public GameObject periodicTableUI;
    public GameObject bondSystemUI;
    public GameObject quizSystemUI;

    [Header("Experiment Props")]
    public GameObject[] experimentObjects;

    [Header("Periodic Table")]
    public GameObject infoSlab;

    [Header("Menu")]
    public GameObject mainMenuHolder;


    private readonly List<GameObject> resolvedExperimentObjects = new();

    private void Awake() { ResolveReferences(); }

    private void Start()
    {
        HideMenu();
        HideAllSystems();
        SetExperimentObjectsActive(false);
    }

    private void ResolveReferences()
    {
        if (chemistryUIHolder == null)
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform candidate in transforms)
            {
                if (!candidate.name.StartsWith("ChemistryUIHolder"))
                    continue;

                ChemistrySlotManager slotManager = candidate.GetComponentInChildren<ChemistrySlotManager>(true);
                if (slotManager == null)
                    continue;

                chemistryUIHolder = candidate.gameObject;
                Debug.Log("[MenuController] Chemistry UI holder resolved to: " + candidate.name);
                break;
            }
        }

        if (periodicTableUI == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.name == "Canvas")
                {
                    periodicTableUI = canvas.gameObject;
                    break;
                }
            }
        }

        if (infoSlab == null)
        {
            InfoSlabFollower[] slabs = FindObjectsByType<InfoSlabFollower>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (InfoSlabFollower slab in slabs)
            {
                if (slab.gameObject.name == "InfoSlab")
                {
                    infoSlab = slab.gameObject;
                    break;
                }
            }
        }

        if (quizSystemUI == null)
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform candidate in transforms)
            {
                if (candidate.name == "QuizPanel" && candidate.parent != null && candidate.parent.name == "Canvas")
                {
                    quizSystemUI = candidate.gameObject;
                    break;
                }
            }
        }

        if (mainMenuHolder == null)
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform candidate in transforms)
            {
                if (candidate.name != "MainMenuHolder") continue;
                if (candidate.Find("Btn_Experiment") != null || candidate.Find("MainMenuCanvas") != null)
                {
                    mainMenuHolder = candidate.gameObject;
                    break;
                }
            }
        }

        ResolveExperimentObjects();
    }

    public void OnStartExperiment()
    {
        Debug.Log("[MenuController] Starting Experiment...");
        SelectSystem(chemistryUIHolder);
        SetExperimentObjectsActive(true);
    }

    public void OnStartPeriodicTable()
    {
        Debug.Log("[MenuController] Starting Periodic Table...");
        if (periodicTableUI == null)
        {
            Debug.LogError("[MenuController] Periodic table Canvas reference is missing.");
            return;
        }
        SelectSystem(periodicTableUI);
        SetInfoSlabActive(true);
    }

public void OnStartBond()
    {
        Debug.Log("[MenuController] Starting Bond System...");

        if (bondSystemUI != null)
        {
            SelectSystem(bondSystemUI);
            return;
        }

        // No bond system built yet – stay on menu and show a floating label
        SpawnComingSoon("Start Bond");
    }

public void OnStartQuiz()
    {
        Debug.Log("[MenuController] Starting Quiz...");

        // If a dedicated quiz UI exists, show it
        if (quizSystemUI != null)
        {
            SelectSystem(quizSystemUI);

            // Try to find a QuizManager – it's optional, don't error if missing
            QuizManager quizManager = FindFirstObjectByType<QuizManager>();
            if (quizManager != null)
                quizManager.OpenQuiz();
            return;
        }

        // No quiz system built yet – stay on menu and show a floating label
        SpawnComingSoon("Start Quiz");
    }

    public void OnExit()
    {
        Debug.Log("[MenuController] Exit pressed - returning to menu");
        HideAllSystems();
        SetExperimentObjectsActive(false);
        ResetMenuButtonHighlights();
        ShowMenu();
    }

    public void SetSelectedButton(MenuButton3D selectedButton)
    {
        MenuButton3D[] buttons = FindObjectsByType<MenuButton3D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MenuButton3D button in buttons) button.SetSelectedVisual(button == selectedButton);
    }

    private void PositionPeriodicTableAtMenu()
    {
        if (mainMenuHolder == null || periodicTableUI == null) return;
        periodicTableUI.transform.SetPositionAndRotation(mainMenuHolder.transform.position, mainMenuHolder.transform.rotation);
    }

    private void SelectSystem(GameObject system)
    {
        HideMenu();
        HideAllSystems();
        if (system != null) system.SetActive(true);
        else Debug.LogWarning("[MenuController] Requested menu system was not found.");
    }

    public void ShowMenu() { if (mainMenuHolder != null) mainMenuHolder.SetActive(true); }
    private void HideMenu() { if (mainMenuHolder != null) mainMenuHolder.SetActive(false); }

    private void HideAllSystems()
    {
        if (chemistryUIHolder != null) chemistryUIHolder.SetActive(false);
        if (periodicTableUI != null) periodicTableUI.SetActive(false);
        if (bondSystemUI != null) bondSystemUI.SetActive(false);
        if (quizSystemUI != null) quizSystemUI.SetActive(false);
        SetInfoSlabActive(false);
    }

    private void SetInfoSlabActive(bool isActive)
    {
        if (infoSlab == null)
            return;

        InfoSlabFollower follower = infoSlab.GetComponent<InfoSlabFollower>();
        if (!isActive)
            follower?.ResetSelection();

        infoSlab.SetActive(isActive);

        if (isActive)
            follower?.ShowPeriodicTablePrompt();
    }

    private void ResolveExperimentObjects()
    {
        resolvedExperimentObjects.Clear();
        if (experimentObjects != null)
        {
            foreach (GameObject experimentObject in experimentObjects)
                if (experimentObject != null && !resolvedExperimentObjects.Contains(experimentObject)) resolvedExperimentObjects.Add(experimentObject);
        }

        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform candidate in transforms)
        {
            string objectName = candidate.name.ToLowerInvariant();
            if ((objectName.StartsWith("sodium") || objectName.Contains("beaker") || objectName.Contains("water")) && !resolvedExperimentObjects.Contains(candidate.gameObject))
                resolvedExperimentObjects.Add(candidate.gameObject);
        }
    }

    private void SetExperimentObjectsActive(bool isActive)
    {
        foreach (GameObject experimentObject in resolvedExperimentObjects)
            if (experimentObject != null) experimentObject.SetActive(isActive);
    }

    private void ResetMenuButtonHighlights()
    {
        MenuButton3D[] buttons = FindObjectsByType<MenuButton3D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MenuButton3D button in buttons) button.SetSelectedVisual(false);
    }

    private void SpawnComingSoon(string featureName = "")
    {
        Camera camera = Camera.main;
        Vector3 spawnPosition = camera != null ? camera.transform.position + camera.transform.forward * 1.5f : Vector3.up * 1.5f;
        GameObject textObject = new GameObject("ComingSoon_Text");
        textObject.transform.position = spawnPosition;
        if (camera != null) textObject.transform.rotation = Quaternion.LookRotation(textObject.transform.position - camera.transform.position);
        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = (featureName.Length > 0 ? featureName + "\n" : "") + "Coming Soon!";
        textMesh.fontSize = 60;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.yellow;
        textObject.transform.localScale = Vector3.one * 0.012f;
        Destroy(textObject, 2.5f);
    }
}
