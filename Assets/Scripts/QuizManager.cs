using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to a QuizPanel GameObject (world-space Canvas, same pattern as your InfoPanel).
// Clicking "Enter Quiz" now runs through ALL quiz banks in sequence (one
// question per element), one after another, with a "Continue" button to
// advance - instead of showing just one random question and stopping.
public class QuizManager : MonoBehaviour
{
    [Header("Quiz Data")]
    [Tooltip("One QuizData asset per element. Drag all 8 in here.")]
    public QuizData[] quizBanks;

    [Header("UI References")]
    public GameObject quizPanel;              // parent panel, toggled on/off
    public TextMeshProUGUI questionText;
    public Button[] choiceButtons;            // size 4 in the inspector
    public TextMeshProUGUI[] choiceButtonLabels; // matching labels for choiceButtons
    public TextMeshProUGUI feedbackText;      // "Correct!" / "Try again"
    public TextMeshProUGUI scoreText;         // optional running score display

    [Header("Enter Quiz Button")]
    [Tooltip("Small button shown next to the InfoPanel. Student clicks it to start the full 8-question quiz.")]
    public GameObject enterQuizButtonObject;  // the button's GameObject, toggled on/off
    public Button enterQuizButton;

    [Header("Continue Button")]
    [Tooltip("Shown after each answer. Clicking it moves to the next element's question, or finishes the quiz on the last one.")]
    public GameObject continueButtonObject;
    public Button continueButton;

    [Header("After Quiz Finishes")]
    [Tooltip("Shown/activated once the student completes all 8 questions - e.g. the Start Experimenting button. It scales up from 0, same style as the Welcome Screen reveal.")]
    public GameObject startExperimentingButtonObject;
    public float revealScaleDuration = 0.4f;

    private ElementData pendingElement;       // last element selected on the periodic table (not required anymore, kept for reference)
    private List<QuizData> quizSequence;      // the full run of 8 quiz banks for this session
    private int currentQuizIndex;             // which bank in quizSequence we're on
    private QuizQuestion currentQuestion;
    private int score = 0;
    private int questionsAnswered = 0;
    private bool answerLocked = false;

    void Start()
    {
        if (quizPanel != null) quizPanel.SetActive(false);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i; // capture for closure
            choiceButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }

        if (enterQuizButton != null)
        {
            enterQuizButton.onClick.AddListener(OnEnterQuizClicked);
        }

        if (continueButtonObject != null) continueButtonObject.SetActive(false);
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    // Call this from PeriodicTableManager.SelectElement() if you still want
    // to track the last-picked element. No longer required for Enter Quiz to
    // work, since Enter Quiz now always runs the full 8-element sequence.
    public void PrepareQuizForElement(ElementData element)
    {
        pendingElement = element;
    }

    private void OnEnterQuizClicked()
    {
        if (quizBanks == null || quizBanks.Length == 0) return;

        // Build a fresh sequence of all 8 banks, shuffled, and reset score.
        quizSequence = new List<QuizData>(quizBanks);
        Shuffle(quizSequence);

        currentQuizIndex = 0;
        score = 0;
        questionsAnswered = 0;
        if (scoreText != null) scoreText.text = $"Score: 0/0";

        ShowQuestionAt(currentQuizIndex);
    }

    private void ShowQuestionAt(int index)
    {
        if (quizSequence == null || index < 0 || index >= quizSequence.Count)
        {
            FinishQuiz();
            return;
        }

        QuizData bank = quizSequence[index];
        if (bank == null || bank.questions == null || bank.questions.Length == 0)
        {
            Debug.LogWarning($"No quiz questions found for {bank?.element?.elementName}. Skipping.");
            currentQuizIndex++;
            ShowQuestionAt(currentQuizIndex);
            return;
        }

        currentQuestion = bank.questions[Random.Range(0, bank.questions.Length)];
        answerLocked = false;

        questionText.text = currentQuestion.questionText;
        feedbackText.text = "";
        if (continueButtonObject != null) continueButtonObject.SetActive(false);

        for (int i = 0; i < choiceButtonLabels.Length; i++)
        {
            bool hasChoice = i < currentQuestion.choices.Length;
            choiceButtons[i].gameObject.SetActive(hasChoice);
            if (hasChoice)
            {
                choiceButtonLabels[i].text = currentQuestion.choices[i];
                SetButtonColor(choiceButtons[i], Color.white);
            }
        }

        quizPanel.SetActive(true);
    }

    private void OnAnswerSelected(int choiceIndex)
    {
        if (answerLocked || currentQuestion == null) return;
        answerLocked = true;
        questionsAnswered++;

        bool isCorrect = choiceIndex == currentQuestion.correctAnswerIndex;

        // Highlight selected + correct answer
        SetButtonColor(choiceButtons[choiceIndex], isCorrect ? Color.green : Color.red);
        if (!isCorrect)
        {
            SetButtonColor(choiceButtons[currentQuestion.correctAnswerIndex], Color.green);
        }

        if (isCorrect)
        {
            score++;
            feedbackText.text = "Correct!";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = "Not quite - check the highlighted answer.";
            feedbackText.color = Color.red;
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}/{questionsAnswered}";
        }

        // Show Continue so the student moves to the next element's question
        // (or finishes, if this was the last one) whenever they're ready.
        if (continueButtonObject != null) continueButtonObject.SetActive(true);
    }

    private void OnContinueClicked()
    {
        currentQuizIndex++;
        if (quizSequence != null && currentQuizIndex < quizSequence.Count)
        {
            ShowQuestionAt(currentQuizIndex);
        }
        else
        {
            FinishQuiz();
        }
    }

    private void FinishQuiz()
    {
        questionText.text = "Quiz complete!";
        feedbackText.text = $"Final score: {score}/{questionsAnswered}";
        feedbackText.color = Color.white;

        foreach (var btn in choiceButtons)
        {
            btn.gameObject.SetActive(false);
        }

        if (continueButtonObject != null) continueButtonObject.SetActive(true);
        // Re-wire Continue's last click to close the panel instead of advancing.
        continueButton.onClick.RemoveListener(OnContinueClicked);
        continueButton.onClick.AddListener(CloseQuizFinal);
    }

    private void CloseQuizFinal()
    {
        CloseQuiz();
        // Restore normal Continue behavior for the next quiz run.
        continueButton.onClick.RemoveListener(CloseQuizFinal);
        continueButton.onClick.AddListener(OnContinueClicked);

        // Reveal Start Experimenting now that the quiz is done, with a
        // scale-up pop-in (same style as the Welcome Screen reveal).
        if (startExperimentingButtonObject != null)
        {
            StartCoroutine(ScaleUpReveal(startExperimentingButtonObject));
        }
    }

    private System.Collections.IEnumerator ScaleUpReveal(GameObject target)
    {
        target.SetActive(true);
        target.transform.localScale = Vector3.zero;

        float t = 0f;
        while (t < revealScaleDuration)
        {
            t += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, 1f, t / revealScaleDuration);
            target.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        target.transform.localScale = Vector3.one;
    }

    private void CloseQuiz()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (continueButtonObject != null) continueButtonObject.SetActive(false);
        foreach (var btn in choiceButtons)
        {
            btn.gameObject.SetActive(true);
        }
    }

    private void SetButtonColor(Button button, Color color)
    {
        var colors = button.colors;
        colors.normalColor = color;
        colors.selectedColor = color;
        button.colors = colors;
    }

    private void Shuffle(List<QuizData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
