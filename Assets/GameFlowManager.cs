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
    public GameObject backButton;

    [Header("Data")]
    public ElementData selectedElementA;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Only hide non-welcome objects — WelcomeScreenManager controls the welcome canvas itself
        if (periodicTableBoard != null) periodicTableBoard.SetActive(false);
        if (workbenchArea != null) workbenchArea.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
        currentStage = Stage.Welcome;
    }

public void SetStage(Stage newStage)
    {
        currentStage = newStage;

        if (periodicTableBoard != null) periodicTableBoard.SetActive(newStage == Stage.ElementSelect);
        if (workbenchArea != null) workbenchArea.SetActive(newStage == Stage.ReactionExperiment || newStage == Stage.Result);
        if (continueButton != null) continueButton.SetActive(false);
        if (backButton != null) backButton.SetActive(newStage == Stage.ReactionExperiment || newStage == Stage.Result);

        if (newStage == Stage.ElementSelect)
        {
            if (currentStage == Stage.Welcome)
                selectedElementA = null;
            if (infoSlab != null && !infoSlab.activeSelf)
                infoSlab.SetActive(true);
            if (infoSlab != null) infoSlab.GetComponent<InfoSlabFollower>()?.ResetSelection();
        }

        if (newStage == Stage.ReactionExperiment)
        {
            // Hide info slab when going to workbench
            if (infoSlab != null)
            {
                var slabFollower = infoSlab.GetComponent<InfoSlabFollower>();
                if (slabFollower != null) slabFollower.Hide();
                else infoSlab.SetActive(false);
            }

            ReactionWorkbench.Instance?.PrepareElementA(selectedElementA);
        }

        if (newStage == Stage.Result)
        {
            // Keep workbench visible, hide slab
            if (infoSlab != null)
            {
                var slabFollower = infoSlab.GetComponent<InfoSlabFollower>();
                if (slabFollower != null) slabFollower.Hide();
            }
        }
    }

    public void OnStartExperimenting() => SetStage(Stage.ElementSelect);
    
    public void OnElementSelected(ElementData data)
    {
        if (currentStage != Stage.ElementSelect) return;
        selectedElementA = data;
        if (continueButton != null) continueButton.SetActive(true);
        if (infoSlab != null) infoSlab.GetComponent<InfoSlabFollower>()?.ShowElement(data);
    }

    public void OnContinueToExperiment() => SetStage(Stage.ReactionExperiment);
    public void OnBackToTable() => SetStage(Stage.ElementSelect);
}