using UnityEngine;

// One question in a quiz bank.
[System.Serializable]
public class QuizQuestion
{
    [TextArea(2, 4)]
    public string questionText;

    // 3-4 answer choices. Keep order consistent with correctAnswerIndex.
    public string[] choices = new string[4];

    // Index into 'choices' that is correct (0-based).
    public int correctAnswerIndex;
}

// Create one of these per element (e.g. Sodium_Quiz, Potassium_Quiz...).
// Right-click in Project window -> Create -> Chemistry/QuizData
[CreateAssetMenu(fileName = "NewQuizData", menuName = "Chemistry/QuizData")]
public class QuizData : ScriptableObject
{
    [Tooltip("The element this quiz bank belongs to.")]
    public ElementData element;

    [Tooltip("Pool of questions for this element. QuizManager picks one at random.")]
    public QuizQuestion[] questions;
}
