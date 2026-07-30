using System.Collections;
using UnityEngine;
using TMPro;

public class WelcomeScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup fadePanel;           // The black Image with CanvasGroup
    public GameObject welcomeCanvas;        // The whole WelcomeCanvas
    public TextMeshProUGUI instructionText; // Optional: to pulse/hide

    [Header("Lab References")]
    public GameObject periodicTableBoard;   // Your Canvas or MainBoard
    public GameObject atomSpawnPoint;       // Optional: hide until start

    [Header("Timing")]
    public float fadeOutDuration = 1.5f;
    public float boardRevealDelay = 0.3f;

    [Header("Audio")]
    
    public AudioClip confirmSound;  // For Start Experimenting button
    
public AudioClip startSound;
    private AudioSource audioSource;

    void Start()
    {
        // Ensure welcome screen is visible
        fadePanel.alpha = 1f;
        welcomeCanvas.SetActive(true);

        // Hide the periodic table at start
        if (periodicTableBoard != null)
            periodicTableBoard.SetActive(false);

        // Optional: hide atom spawn point
        if (atomSpawnPoint != null)
            atomSpawnPoint.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Call this from the Start Button's OnClick
public void OnStartButtonClicked()
    {
        // Play confirm chord immediately on button press
        if (confirmSound != null)
            audioSource.PlayOneShot(confirmSound, 0.7f);
        StartCoroutine(StartLabSequence());
    }

    IEnumerator StartLabSequence()
    {
        // Play sound
        if (startSound != null)
            audioSource.PlayOneShot(startSound);

        // Fade out the black panel
        float t = fadePanel.alpha;
        while (t > 0)
        {
            t -= Time.deltaTime / fadeOutDuration;
            fadePanel.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        // Brief pause
        yield return new WaitForSeconds(0.2f);

        // Disable welcome canvas
        welcomeCanvas.SetActive(false);

        // Wait then reveal the board via GameFlowManager (which owns periodicTableBoard)
        yield return new WaitForSeconds(boardRevealDelay);

        // Hand off to GameFlowManager to show the periodic table
        GameFlowManager.Instance?.SetStage(GameFlowManager.Stage.ElementSelect);

        if (atomSpawnPoint != null)
            atomSpawnPoint.SetActive(true);
    }
}