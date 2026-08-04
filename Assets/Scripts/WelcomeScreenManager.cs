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
    public GameObject mainMenuHolder;       // The main menu with 4 buttons
    public GameObject periodicTableBoard;   // Canvas/MainBoard (controlled by menu)
    public GameObject atomSpawnPoint;       // Optional: hide until start

    [Header("Timing")]
    public float fadeOutDuration = 1.5f;
    public float boardRevealDelay = 0.3f;

    [Header("Audio")]
    public AudioClip confirmSound;
    public AudioClip startSound;

    private AudioSource audioSource;

    void Start()
    {
        fadePanel.alpha = 1f;
        welcomeCanvas.SetActive(true);

        if (mainMenuHolder == null)
            mainMenuHolder = GameObject.Find("MainMenuHolder");

        if (periodicTableBoard != null)
            periodicTableBoard.SetActive(false);

        if (atomSpawnPoint != null)
            atomSpawnPoint.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnStartButtonClicked()
    {
        if (confirmSound != null)
            audioSource.PlayOneShot(confirmSound, 0.7f);
        StartCoroutine(StartLabSequence());
    }

    IEnumerator StartLabSequence()
    {
        if (startSound != null)
            audioSource.PlayOneShot(startSound);

        // Fade out black panel
        float t = fadePanel.alpha;
        while (t > 0)
        {
            t -= Time.deltaTime / fadeOutDuration;
            fadePanel.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        welcomeCanvas.SetActive(false);
        yield return new WaitForSeconds(boardRevealDelay);

        // Show main menu with scale-up animation
        if (mainMenuHolder != null)
        {
            mainMenuHolder.SetActive(true);
            mainMenuHolder.transform.localScale = Vector3.zero;
            float scaleT = 0f;
            while (scaleT < 1f)
            {
                scaleT += Time.deltaTime * 3f;
                mainMenuHolder.transform.localScale = Vector3.Lerp(
                    Vector3.zero, Vector3.one, Mathf.SmoothStep(0f, 1f, scaleT));
                yield return null;
            }
            mainMenuHolder.transform.localScale = Vector3.one;
        }

        if (atomSpawnPoint != null)
            atomSpawnPoint.SetActive(true);
    }
}
