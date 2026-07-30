using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;
    public enum Stage { Welcome, ElementSelect, ReactionExperiment, Result }
    public Stage currentStage = Stage.Welcome;

    [Header("Stage Objects")]
    public GameObject welcomeCanvas;
    public GameObject periodicTableBoard;
    public GameObject workbenchArea;
    public GameObject continueButton;
    public GameObject infoSlab;

    [Header("Data")]
    public ElementData selectedElementA;

    void Awake() => Instance = this;

    void Start()
    {
        // Only hide non-welcome objects — WelcomeScreenManager controls the welcome canvas itself
        periodicTableBoard?.SetActive(false);
        workbenchArea?.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
        currentStage = Stage.Welcome;
    }

public void SetStage(Stage newStage)
    {
        currentStage = newStage;

        periodicTableBoard?.SetActive(newStage == Stage.ElementSelect);
        workbenchArea?.SetActive(newStage == Stage.ReactionExperiment || newStage == Stage.Result);
        if (continueButton != null) continueButton.SetActive(false);

        if (newStage == Stage.ElementSelect)
        {
            selectedElementA = null;
            if (infoSlab != null && !infoSlab.activeSelf)
                infoSlab.SetActive(true);
            infoSlab?.GetComponent<InfoSlabFollower>()?.ResetSelection();
        }

        if (newStage == Stage.ReactionExperiment)
        {
            // Hide info slab when going to workbench
            var slabFollower = infoSlab?.GetComponent<InfoSlabFollower>();
            if (slabFollower != null) slabFollower.Hide();
            else if (infoSlab != null) infoSlab.SetActive(false);

            ReactionWorkbench.Instance?.PrepareElementA(selectedElementA);
        }

        if (newStage == Stage.Result)
        {
            // Keep workbench visible, hide slab
            var slabFollower = infoSlab?.GetComponent<InfoSlabFollower>();
            if (slabFollower != null) slabFollower.Hide();
        }
    }

    public void OnStartExperimenting() => SetStage(Stage.ElementSelect);
    
    public void OnElementSelected(ElementData data)
    {
        if (currentStage != Stage.ElementSelect) return;
        selectedElementA = data;
        continueButton?.SetActive(true);
        infoSlab?.GetComponent<InfoSlabFollower>()?.ShowElement(data);
    }

    public void OnContinueToExperiment() => SetStage(Stage.ReactionExperiment);
    public void OnBackToTable() => SetStage(Stage.ElementSelect);
}
