using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Data")]
    [Tooltip("Exactly eight QuizData assets, one for each curriculum element.")]
    public QuizData[] quizBanks;

    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceButtonLabels;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;

    [Header("Enter Quiz Button")]
    public GameObject enterQuizButtonObject;
    public Button enterQuizButton;

    [Header("Question Actions")]
    public GameObject continueButtonObject;
    public Button continueButton;
    public GameObject quitButtonObject;
    public Button quitButton;

    [Header("Results")]
    public GameObject closeButtonObject;
    public Button closeButton;
    public GameObject mainboardObject;
    public GameObject startExperimentingButtonObject;
    public float revealScaleDuration = 0.4f;

    public Transform playerOrigin;
    public Vector3 quizStartPosition = new Vector3(-2f, 0f, 3f);
    public Vector3 quizStartEulerAngles = Vector3.zero;

    private readonly List<QuizQuestion> questionSequence = new();
    private QuizQuestion currentQuestion;
    private int currentQuestionIndex;
    private int score;
    private int questionsAnswered;
    private bool answerLocked;
    private bool showingResults;

    private static readonly Color PanelButtonColor = Color.white;
    private static readonly Color CorrectColor = new(0.133f, 0.773f, 0.369f, 1f);
    private static readonly Color IncorrectColor = new(0.937f, 0.267f, 0.267f, 1f);
    private static readonly Color AccentColor = new(0.133f, 0.827f, 0.933f, 1f);

    private void Start()
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (questionText != null)
        {
            questionText.enableAutoSizing = true;
            questionText.fontSizeMin = 24f;
            questionText.fontSizeMax = 40f;
            questionText.maxVisibleLines = 3;
            questionText.overflowMode = TextOverflowModes.Ellipsis;
        }

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choiceIndex = i;
            if (choiceButtons[i] != null)
                choiceButtons[i].onClick.AddListener(() => OnAnswerSelected(choiceIndex));
        }

        if (enterQuizButton != null)
            enterQuizButton.onClick.AddListener(OpenQuiz);
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseQuizFinal);

        SetActive(continueButtonObject, false);
        SetActive(quitButtonObject, false);
        SetActive(closeButtonObject, false);
        SetButtonColor(continueButton, AccentColor);
        SetButtonColor(quitButton, AccentColor);
        SetButtonColor(closeButton, AccentColor);
    }

    public void PrepareQuizForElement(ElementData element)
    {
        if (enterQuizButtonObject != null)
            enterQuizButtonObject.SetActive(true);
    }

    /// <summary>
    /// Resets the player to the configured spawn point before starting the quiz.
    /// </summary>
    public void OpenQuiz()
    {
        questionSequence.Clear();

        RespawnPlayerAtStart();

        score = 0;
        questionsAnswered = 0;
        currentQuestionIndex = 0;
        showingResults = false;

        if (quizBanks == null || quizBanks.Length != 8)
        {
            Debug.LogError($"[QuizManager] Expected exactly 8 quiz banks, found {quizBanks?.Length ?? 0}.");
            return;
        }

        List<QuizData> shuffledBanks = new(quizBanks);
        Shuffle(shuffledBanks);
        foreach (QuizData bank in shuffledBanks)
        {
            if (bank == null || bank.questions == null || bank.questions.Length != 3)
            {
                Debug.LogError($"[QuizManager] Quiz bank '{bank?.name ?? "null"}' must contain exactly 3 questions.");
                return;
            }

            List<QuizQuestion> bankQuestions = new(bank.questions);
            foreach (QuizQuestion question in bankQuestions)
            {
                if (question == null || question.choices == null || question.choices.Length != 4 ||
                    question.correctAnswerIndex < 0 || question.correctAnswerIndex >= question.choices.Length)
                {
                    Debug.LogError($"[QuizManager] Quiz bank '{bank.name}' contains a question with exactly four choices and a valid correct-answer index required.");
                    return;
                }
            }

            Shuffle(bankQuestions);
            questionSequence.AddRange(bankQuestions);
        }

        Shuffle(questionSequence);
        if (mainboardObject != null)
            mainboardObject.SetActive(false);
        ShowQuestionAt(0);
    }

    /// <summary>
    /// Moves the XR origin to the quiz spawn point without changing the button position.
    /// </summary>
    public void RespawnPlayerAtStart()
    {
        if (playerOrigin == null)
        {
            Debug.LogWarning("[QuizManager] Player origin is not assigned; quiz opened without respawning.");
            return;
        }

        CharacterController characterController = playerOrigin.GetComponent<CharacterController>();
        if (characterController != null)
            characterController.enabled = false;

        playerOrigin.SetPositionAndRotation(quizStartPosition, Quaternion.Euler(quizStartEulerAngles));
        Debug.Log($"[QuizManager] Player respawned at {quizStartPosition} before opening the quiz.");

        if (characterController != null)
            characterController.enabled = true;
    }

    private void ShowQuestionAt(int index)
    {
        if (index >= questionSequence.Count)
        {
            ShowResults(true);
            return;
        }

        currentQuestionIndex = index;
        currentQuestion = questionSequence[index];
        answerLocked = false;
        showingResults = false;

        questionText.text = currentQuestion.questionText;
        feedbackText.text = string.Empty;
        feedbackText.color = Color.white;
        scoreText.text = $"Score: {score}/{questionsAnswered}    Question {index + 1}/{questionSequence.Count}";
        SetActive(continueButtonObject, false);
        SetActive(quitButtonObject, true);
        SetActive(closeButtonObject, false);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool hasChoice = i < currentQuestion.choices.Length;
            SetActive(choiceButtons[i]?.gameObject, hasChoice);
            if (!hasChoice)
                continue;

            choiceButtonLabels[i].text = currentQuestion.choices[i];
            choiceButtons[i].interactable = true;
            SetButtonColor(choiceButtons[i], PanelButtonColor);
        }

        quizPanel.SetActive(true);
    }

    private void OnAnswerSelected(int choiceIndex)
    {
        if (answerLocked || currentQuestion.choices == null || choiceIndex < 0 || choiceIndex >= currentQuestion.choices.Length)
            return;

        answerLocked = true;
        questionsAnswered++;
        bool isCorrect = choiceIndex == currentQuestion.correctAnswerIndex;
        if (isCorrect)
        {
            score++;
            feedbackText.text = "Correct!";
            feedbackText.color = CorrectColor;
            SetButtonColor(choiceButtons[choiceIndex], CorrectColor);
        }
        else
        {
            feedbackText.text = "Not quite - the correct answer is highlighted.";
            feedbackText.color = IncorrectColor;
            SetButtonColor(choiceButtons[choiceIndex], IncorrectColor);
            if (currentQuestion.correctAnswerIndex >= 0 && currentQuestion.correctAnswerIndex < choiceButtons.Length)
                SetButtonColor(choiceButtons[currentQuestion.correctAnswerIndex], CorrectColor);
        }

        foreach (Button button in choiceButtons)
            if (button != null)
                button.interactable = false;

        scoreText.text = $"Score: {score}/{questionsAnswered}    Question {currentQuestionIndex + 1}/{questionSequence.Count}";
        SetActive(continueButtonObject, true);
        SetActive(quitButtonObject, true);
    }

    private void OnContinueClicked()
    {
        Debug.Log("Continue clicked");

        if (!answerLocked || showingResults)
            return;

        int nextIndex = currentQuestionIndex + 1;
        if (nextIndex < questionSequence.Count)
            ShowQuestionAt(nextIndex);
        else
            ShowResults(true);
    }

    private void OnQuitClicked()
    {
        if (!showingResults)
            ShowResults(false);
    }

    private void ShowResults(bool completed)
    {
        showingResults = true;
        answerLocked = true;
        questionText.text = completed ? "Quiz Complete!" : "Quiz Ended";
        feedbackText.text = $"Final score: {score}/{questionsAnswered}";
        feedbackText.color = Color.white;
        scoreText.text = "All answered questions are included in your score.";

        foreach (Button button in choiceButtons)
        {
            if (button == null)
                continue;
            button.gameObject.SetActive(false);
            button.interactable = false;
        }

        SetActive(continueButtonObject, false);
        SetActive(quitButtonObject, false);
        SetActive(closeButtonObject, true);
        quizPanel.SetActive(true);
    }

    private void CloseQuizFinal()
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);
        SetActive(closeButtonObject, false);
        SetActive(continueButtonObject, false);
        SetActive(quitButtonObject, false);

        foreach (Button button in choiceButtons)
        {
            if (button == null)
                continue;
            button.gameObject.SetActive(true);
            button.interactable = true;
        }

        if (mainboardObject != null)
            StartCoroutine(ScaleUpReveal(mainboardObject));
        else if (startExperimentingButtonObject != null)
            StartCoroutine(ScaleUpReveal(startExperimentingButtonObject));
    }

    private IEnumerator ScaleUpReveal(GameObject target)
    {
        target.SetActive(true);
        Vector3 finalScale = target.transform.localScale;
        target.transform.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < revealScaleDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / revealScaleDuration);
            float scale = Mathf.SmoothStep(0f, 1f, normalizedTime);
            target.transform.localScale = finalScale * scale;
            yield return null;
        }
        target.transform.localScale = finalScale;
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private static void SetButtonColor(Button button, Color color)
    {
        if (button == null)
            return;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        colors.disabledColor = color;
        button.colors = colors;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
