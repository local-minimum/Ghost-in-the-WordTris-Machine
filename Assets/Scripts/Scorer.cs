using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Game.OnPhaseChange += Game_OnPhaseChange;
        FactoryReset();
    }

    private void OnDisable()
    {
        Game.OnPhaseChange -= Game_OnPhaseChange;
        PlayField.OnWord -= PlayField_OnWord;
    }

    void FactoryReset()
    {
        score = 0;
        words = 0;

        for (int i = 0, l = history.Count; i<l; i++)
        {
            history[i].FactoryReset();
        }

        Game.MinWordLength = baseLength;
        SyncScoreText();
        SyncMinLength();
    }

    private void Game_OnPhaseChange(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.FactoryReset)
        {
            FactoryReset();
        }
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
    int words = 0;

    public int TotalScore => score;

    [SerializeField]
    int totalScorePadding = 6;

    [SerializeField]
    int[] wordLevels;

    [SerializeField]
    int baseLength = 3;

    [SerializeField]
    TMPro.TextMeshProUGUI minLengthUI;

    int Score(string word, int combo, int iteration)
    {
        int l = word.Length;
        int score = l * combo * comboMultiplier + l * iteration * iterationMultiplier;

        this.score += score;
        words++;

        if (wordLevels.Any(lvl => words == lvl))
        {
            Game.MinWordLength ++;
            SyncMinLength();
        }

        SyncScoreText();
        return score;
    }

    void SyncMinLength()
    {
        minLengthUI.text = $"Min word length: {Game.MinWordLength}";
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
