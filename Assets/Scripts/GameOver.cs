using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField]
    GameObject GameOverRoot;

    [SerializeField]
    TMPro.TextMeshProUGUI MessageUI;

    [SerializeField]
    Scorer scorer;

    private void OnEnable()
    {
        Game.OnPhaseChange += Game_OnPhaseChange;
    }

    private void OnDisable()
    {
        Game.OnPhaseChange -= Game_OnPhaseChange;
    }

    bool isWorstScore(int score)
    {
        var prefKey = "Scores.Worst";
        if (PlayerPrefs.HasKey(prefKey))
        {
            if (score < PlayerPrefs.GetInt(prefKey))
            {
                PlayerPrefs.SetInt(prefKey, score);
                return true;
            }
            return false;
        }
        PlayerPrefs.SetInt(prefKey, score);
        return true;
    }

    bool isBestScore(int score)
    {
        var prefKey = "Scores.Best";
        if (PlayerPrefs.HasKey(prefKey))
        {
            if (score > PlayerPrefs.GetInt(prefKey))
            {
                PlayerPrefs.SetInt(prefKey, score);
                return true;
            }
            return false;
        }
        PlayerPrefs.SetInt(prefKey, score);
        return true;
    }

    private void Game_OnPhaseChange(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.GameOver)
        {
            int score = scorer.TotalScore;
            bool isWorst = isWorstScore(score);
            bool isBest = isBestScore(score);

            if (isBest && isWorst)
            {
                MessageUI.text = $"COMPUTER PLAYED THEIR FIRST GAME: {score}\nCAN YOU HELP THEM BEAT IT OR ENSURE THEY FAIL FASTER?";
            } else if (isBest)
            {
                MessageUI.text = $"COMPUTER PLAYED THEIR BEST GAME: {score}\nCAN YOU HELP THEM EVEN MORE?";
            } else if (isWorst)
            {
                MessageUI.text = $"COMPUTER PLAYED THIER WORST GAME: {score}\nCAN YOU MAKE THEM SUFFER EVEN MORE?";
            }

            GameOverRoot.SetActive(true);
        } else
        {
            GameOverRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (Game.Phase == GamePhase.GameOver)
        {
            if (Input.anyKeyDown)
            {
                Game.Phase = GamePhase.FactoryReset;
            }
        }
    }
}
