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

    void Start() => SetStage(Stage.Welcome);

    public void SetStage(Stage newStage)
    {
        currentStage = newStage;
        welcomeCanvas?.SetActive(newStage == Stage.Welcome);
        periodicTableBoard?.SetActive(newStage == Stage.ElementSelect);
        workbenchArea?.SetActive(newStage == Stage.ReactionExperiment || newStage == Stage.Result);
        continueButton?.SetActive(false);
        backButton?.SetActive(newStage == Stage.ReactionExperiment || newStage == Stage.Result);
        
        if (newStage == Stage.ElementSelect)
        {
            if (currentStage == Stage.Welcome) 
                selectedElementA = null;
            infoSlab?.GetComponent<InfoSlabFollower>()?.ResetSelection();
        }
        if (newStage == Stage.ReactionExperiment)
        {
            ReactionWorkbench.Instance?.PrepareElementA(selectedElementA);
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