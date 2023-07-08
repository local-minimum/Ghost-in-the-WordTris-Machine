using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scorer : MonoBehaviour
{
    [SerializeField]
    RectTransform scoreHistory;

    [SerializeField]
    ScoreWord prefab;

    [SerializeField]
    int maxHistory = 10;

    [SerializeField]
    TMPro.TextMeshProUGUI ScoreUI;

    List<ScoreWord> history = new List<ScoreWord>();

    private void OnEnable()
    {
        PlayField.OnWord += PlayField_OnWord;
        SyncScoreText();
    }

    private void OnDisable()
    {
        PlayField.OnWord -= PlayField_OnWord;
    }

    private void PlayField_OnWord(List<string> words, int iteration)
    {
        int combo = words.Count;
        for (int i = 0; i<combo; i++)
        {
            AddWord(words[i], combo, iteration);
        }
    }

    [SerializeField]
    int comboMultiplier = 3;

    [SerializeField]
    int iterationMultiplier = 2;

    int score = 0;

    [SerializeField]
    int totalScorePadding = 6;

    int Score(string word, int combo, int iteration)
    {
        int l = word.Length;
        int score = l * combo * comboMultiplier + l * iteration * iterationMultiplier;

        this.score += score;

        SyncScoreText();
        return score;
    }

    void SyncScoreText()
    {
        var scoreText = this.score.ToString().PadLeft(totalScorePadding, '0');
        ScoreUI.text = $"Score: {scoreText}";
    }

    void ShiftHistoryDown()
    {

        for (int i = history.Count - 2; i>=0; i--)
        {
            var target = history[i + 1];
            var source = history[i];
            target.Word = source.Word;
            target.Score = source.Score;
        }
    }

    void AddWord(string word, int combo, int iteration)
    {
        int wordScore = Score(word, combo, iteration);

        if (history.Count < maxHistory)
        {
            history.Add(Instantiate(prefab, scoreHistory));
        }

        ShiftHistoryDown();

        history[0].Word = word;
        history[0].Score = wordScore;
    }
}
