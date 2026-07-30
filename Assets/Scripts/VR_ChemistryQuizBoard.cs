using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VR_ChemistryQuizBoard : MonoBehaviour
{
    private const int TotalQuestions = 10;
    private const float MenuButtonY = -400f;
    private static readonly Color Navy = new Color(0.117647f, 0.160784f, 0.231373f, 1f);
    private static readonly Color Cyan = new Color(0.133333f, 0.827451f, 0.933333f, 1f);
    private static readonly Color Green = new Color(0.20f, 0.78f, 0.45f, 1f);
    private static readonly Color Red = new Color(0.90f, 0.28f, 0.30f, 1f);
    private static readonly Color White = new Color(0.95f, 0.98f, 1f, 1f);

    private sealed class QuizQuestion
    {
        public readonly string Prompt;
        public readonly string[] Answers;
        public readonly int CorrectIndex;

        public QuizQuestion(string prompt, string[] answers, int correctIndex)
        {
            Prompt = prompt;
            Answers = answers;
            CorrectIndex = correctIndex;
        }
    }

    private readonly List<QuizQuestion> questions = new List<QuizQuestion>
    {
        new QuizQuestion("Which element does the symbol Cl represent?", new[] { "Calcium", "Chlorine", "Sulfur", "Chloride" }, 1),
        new QuizQuestion("What is the atomic number of oxygen?", new[] { "6", "7", "8", "16" }, 2),
        new QuizQuestion("Which element is a gas at room temperature?", new[] { "Calcium", "Magnesium", "Oxygen", "Potassium" }, 2),
        new QuizQuestion("What happens when sodium reacts with water?", new[] { "It floats and reacts quickly", "It freezes the water", "It turns into oxygen", "Nothing happens" }, 0),
        new QuizQuestion("Which symbol represents potassium?", new[] { "P", "Po", "K", "Pt" }, 2),
        new QuizQuestion("Which element has atomic number 20?", new[] { "Calcium", "Chlorine", "Sulfur", "Lithium" }, 0),
        new QuizQuestion("Which of these is a metal?", new[] { "Sulfur", "Chlorine", "Oxygen", "Magnesium" }, 3),
        new QuizQuestion("Which element is a yellow non-metal?", new[] { "Sulfur", "Sodium", "Calcium", "Lithium" }, 0),
        new QuizQuestion("Which element combines with sodium to make table salt?", new[] { "Chlorine", "Oxygen", "Magnesium", "Sulfur" }, 0),
        new QuizQuestion("Which element is the lightest of the eight in this quiz?", new[] { "Lithium", "Oxygen", "Sodium", "Magnesium" }, 0)
    };

    private readonly List<Button> answerButtons = new List<Button>();
    private readonly List<TMP_Text> answerLabels = new List<TMP_Text>();
    private RectTransform board;
    private TMP_Text progressText;
    private TMP_Text scoreText;
    private TMP_Text questionText;
    private TMP_Text feedbackText;
    private Button nextButton;
    private Button backToMenuButton;
    private Button exitButton;
    private int questionIndex;
    private int score;
    private bool answerLocked;

    private void Start()
    {
        CreateQuizEntryButton();
        CreateQuizBoard();
    }

    /// <summary>
    /// Opens the quiz board and starts a fresh ten-question session.
    /// </summary>
    public void OpenQuiz()
    {
        ResetQuizState();
        Transform mainboard = transform.Find("Mainboard");
        if (mainboard != null)
            mainboard.gameObject.SetActive(false);
        board.gameObject.SetActive(true);
        ShowQuestion();
    }

    /// <summary>
    /// Closes the quiz and returns the player to the main menu with a clean session.
    /// </summary>
    public void ExitQuiz()
    {
        ResetQuizState();
        board.gameObject.SetActive(false);
        Transform mainboard = transform.Find("Mainboard");
        if (mainboard != null)
            mainboard.gameObject.SetActive(true);
    }

    private void CreateQuizEntryButton()
    {
        Transform mainboard = transform.Find("Mainboard");
        if (mainboard == null || mainboard.Find("VR Chemistry Quiz Button") != null)
            return;

        GameObject buttonObject = CreateUiObject("VR Chemistry Quiz Button", mainboard as RectTransform);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(500f, MenuButtonY);
        buttonRect.sizeDelta = new Vector2(240f, 48f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = Cyan;
        Button button = buttonObject.AddComponent<Button>();
        ConfigureButton(button, Cyan);
        button.onClick.AddListener(OpenQuiz);

        TMP_Text label = CreateText("Enter Quiz", buttonRect, 24, White);
        Stretch(label.rectTransform, 0f, 0f, 0f, 0f);
        label.alignment = TextAlignmentOptions.Center;
    }

    private void CreateQuizBoard()
    {
        GameObject boardObject = CreateUiObject("VR Chemistry Lab — Quiz Board", transform as RectTransform);
        board = boardObject.GetComponent<RectTransform>();
        board.anchorMin = new Vector2(0.5f, 0.5f);
        board.anchorMax = new Vector2(0.5f, 0.5f);
        board.pivot = new Vector2(0.5f, 0.5f);
        board.anchoredPosition = Vector2.zero;
        board.sizeDelta = new Vector2(820f, 560f);

        Image background = boardObject.AddComponent<Image>();
        background.color = Navy;

        TMP_Text title = CreateText("VR Chemistry Lab — Quiz Board", board, 30, Cyan);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(600f, 52f));
        title.alignment = TextAlignmentOptions.Center;

        exitButton = CreateButton("Exit", board, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(120f, 44f), Red, ExitQuiz);

        progressText = CreateText(string.Empty, board, 20, White);
        SetRect(progressText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -95f), new Vector2(300f, 36f));

        scoreText = CreateText(string.Empty, board, 20, White);
        SetRect(scoreText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-42f, -95f), new Vector2(250f, 36f));
        scoreText.alignment = TextAlignmentOptions.Right;

        questionText = CreateText(string.Empty, board, 26, White);
        SetRect(questionText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -158f), new Vector2(700f, 82f));
        questionText.alignment = TextAlignmentOptions.Center;

        for (int i = 0; i < 4; i++)
        {
            int answerIndex = i;
            GameObject answerObject = CreateUiObject("Answer " + (i + 1), board);
            RectTransform answerRect = answerObject.GetComponent<RectTransform>();
            SetRect(answerRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -252f - (i * 58f)), new Vector2(680f, 46f));
            Image answerImage = answerObject.AddComponent<Image>();
            answerImage.color = Cyan;
            Button answerButton = answerObject.AddComponent<Button>();
            ConfigureButton(answerButton, Cyan);
            answerButton.onClick.AddListener(() => SelectAnswer(answerIndex));
            TMP_Text label = CreateText(string.Empty, answerRect, 20, Navy);
            Stretch(label.rectTransform, 0f, 0f, 0f, 0f);
            label.alignment = TextAlignmentOptions.Center;
            answerButtons.Add(answerButton);
            answerLabels.Add(label);
        }

        feedbackText = CreateText(string.Empty, board, 18, White);
        SetRect(feedbackText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(-80f, 34f));
        feedbackText.alignment = TextAlignmentOptions.Center;

        nextButton = CreateButton("Next", board, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(180f, 44f), Cyan, NextQuestion);
        nextButton.gameObject.SetActive(false);

        backToMenuButton = CreateButton("Back to Main Menu", board, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(42f, 22f), new Vector2(220f, 44f), Cyan, ExitQuiz);
        backToMenuButton.gameObject.SetActive(false);

        board.gameObject.SetActive(false);
    }

    private void ShowQuestion()
    {
        QuizQuestion question = questions[questionIndex];
        answerLocked = false;
        progressText.text = "Question " + (questionIndex + 1) + " of " + TotalQuestions;
        scoreText.text = "Score: " + score + "/" + TotalQuestions;
        questionText.text = question.Prompt;
        feedbackText.text = string.Empty;
        feedbackText.color = White;
        nextButton.gameObject.SetActive(false);
        backToMenuButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(true);

        for (int i = 0; i < answerButtons.Count; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].interactable = true;
            answerLabels[i].text = question.Answers[i];
            answerLabels[i].color = Navy;
            SetButtonColor(answerButtons[i], Cyan);
        }
    }

    private void SelectAnswer(int answerIndex)
    {
        if (answerLocked)
            return;

        answerLocked = true;
        QuizQuestion question = questions[questionIndex];
        bool correct = answerIndex == question.CorrectIndex;
        if (correct)
        {
            score++;
            feedbackText.text = "Correct! Nice work.";
            feedbackText.color = Green;
            SetButtonColor(answerButtons[answerIndex], Green);
        }
        else
        {
            feedbackText.text = "Not quite. The correct answer is: " + question.Answers[question.CorrectIndex];
            feedbackText.color = Red;
            SetButtonColor(answerButtons[answerIndex], Red);
            SetButtonColor(answerButtons[question.CorrectIndex], Green);
        }

        scoreText.text = "Score: " + score + "/" + TotalQuestions;
        for (int i = 0; i < answerButtons.Count; i++)
            answerButtons[i].interactable = false;
        nextButton.gameObject.SetActive(true);
    }

    private void NextQuestion()
    {
        questionIndex++;
        if (questionIndex < TotalQuestions)
        {
            ShowQuestion();
            return;
        }

        ShowResults();
    }

    private void ShowResults()
    {
        int wrongAnswers = TotalQuestions - score;
        progressText.text = "Quiz Complete";
        scoreText.text = "Final Score: " + score + "/" + TotalQuestions;
        questionText.text = "Correct Answers: " + score + "    Wrong Answers: " + wrongAnswers;
        feedbackText.text = GetResultMessage();
        feedbackText.color = score >= 7 ? Green : White;
        nextButton.GetComponentInChildren<TMP_Text>().text = "Retry Quiz";
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OpenQuiz);
        nextButton.gameObject.SetActive(true);
        backToMenuButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);

        for (int i = 0; i < answerButtons.Count; i++)
            answerButtons[i].gameObject.SetActive(false);
    }

    private void ResetQuizState()
    {
        questionIndex = 0;
        score = 0;
        answerLocked = false;
        feedbackText?.SetText(string.Empty);

        for (int i = 0; i < answerButtons.Count; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].interactable = true;
            answerLabels[i].text = string.Empty;
            answerLabels[i].color = Navy;
            SetButtonColor(answerButtons[i], Cyan);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextQuestion);
            nextButton.GetComponentInChildren<TMP_Text>().text = "Next";
            nextButton.gameObject.SetActive(false);
        }

        backToMenuButton?.gameObject.SetActive(false);
        exitButton?.gameObject.SetActive(true);
    }

    private string GetResultMessage()
    {
        if (score == TotalQuestions)
            return "Perfect result — you are a chemistry star!";
        if (score >= 7)
            return "Great job — your element knowledge is strong!";
        if (score >= 4)
            return "Good effort — keep exploring the lab and try again!";
        return "Keep experimenting — every attempt helps you learn!";
    }

    private GameObject CreateUiObject(string objectName, RectTransform parent)
    {
        GameObject objectToCreate = new GameObject(objectName, typeof(RectTransform));
        objectToCreate.transform.SetParent(parent, false);
        return objectToCreate;
    }

    private TMP_Text CreateText(string text, RectTransform parent, float fontSize, Color color)
    {
        GameObject textObject = CreateUiObject("Text", parent);
        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        return textComponent;
    }

    private Button CreateButton(string label, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, Color color, UnityAction action)
    {
        GameObject buttonObject = CreateUiObject(label + " Button", parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetRect(buttonRect, anchorMin, anchorMax, pivot, position, size);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();
        ConfigureButton(button, color);
        button.onClick.AddListener(action);
        TMP_Text text = CreateText(label, buttonRect, 21, label == "Exit" ? White : Navy);
        Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
        text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private void ConfigureButton(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = White;
        colors.pressedColor = Green;
        colors.selectedColor = White;
        colors.disabledColor = color;
        colors.colorMultiplier = 1f;
        button.colors = colors;
    }

    private void SetButtonColor(Button button, Color color)
    {
        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = color;
        ConfigureButton(button, color);
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
