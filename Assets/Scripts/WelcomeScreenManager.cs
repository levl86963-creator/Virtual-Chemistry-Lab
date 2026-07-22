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

        // Wait then reveal the board
        yield return new WaitForSeconds(boardRevealDelay);

        if (periodicTableBoard != null)
        {
            periodicTableBoard.SetActive(true);

            // Optional: animate board scale up
            periodicTableBoard.transform.localScale = Vector3.zero;
            float scaleT = 0;
            while (scaleT < 1)
            {
                scaleT += Time.deltaTime * 3f;
                periodicTableBoard.transform.localScale = Vector3.Lerp(
                    Vector3.zero,
                    new Vector3(0.005f, 0.005f, 0.005f), // your canvas scale
                    Mathf.SmoothStep(0, 1, scaleT)
                );
                yield return null;
            }
        }

        if (atomSpawnPoint != null)
            atomSpawnPoint.SetActive(true);
    }
}