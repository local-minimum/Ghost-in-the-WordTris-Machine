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
            Debug.Log($"Worst? {score} vs {PlayerPrefs.GetInt(prefKey)}");
            if (score < PlayerPrefs.GetInt(prefKey))
            {
                PlayerPrefs.SetInt(prefKey, score);
                return true;
            }
            return false;
        }

        Debug.Log("First / worst game");
        PlayerPrefs.SetInt(prefKey, score);
        return true;
    }

    bool isBestScore(int score)
    {
        var prefKey = "Scores.Best";
        if (PlayerPrefs.HasKey(prefKey))
        {
            Debug.Log($"Best? {score} vs {PlayerPrefs.GetInt(prefKey)}");
            if (score > PlayerPrefs.GetInt(prefKey))
            {
                PlayerPrefs.SetInt(prefKey, score);
                return true;
            }

            return false;
        }

        Debug.Log("First / best game");
        PlayerPrefs.SetInt(prefKey, score);
        return true;
    }

    float mayStartNewGameTime = 0;
    bool mayStartNewGame = false;

    [SerializeField]
    float newGameEmbargo = 1f;

    private void Game_OnPhaseChange(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.GameOver)
        {
            mayStartNewGameTime = Time.timeSinceLevelLoad + newGameEmbargo;
            mayStartNewGame = true;
            int score = scorer.TotalScore;
            bool isWorst = isWorstScore(score);
            bool isBest = isBestScore(score);

            if (score == 0)
            {
                MessageUI.text = $"COMPUTER GOT {score} POINTS. YOU MUST HAVE BEEN CHEATING";
            } else if (isBest && isWorst)
            {
                MessageUI.text = $"COMPUTER PLAYED THEIR FIRST GAME: {score}\nCAN YOU HELP THEM BEAT IT OR ENSURE THEY FAIL FASTER?";
            } else if (isBest)
            {
                MessageUI.text = $"COMPUTER PLAYED THEIR BEST GAME: {score}\nCAN YOU HELP THEM EVEN MORE?";
            } else if (isWorst)
            {
                MessageUI.text = $"COMPUTER PLAYED THIER WORST GAME: {score}\nCAN YOU MAKE THEM SUFFER EVEN MORE?";
            } else
            {
                MessageUI.text = $"COMPUTER PLAYED A MEDIOCRE GAME:\n{score} POINTS";
            }

            GameOverRoot.SetActive(true);
        } else
        {
            mayStartNewGame = false;
            GameOverRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (Game.Phase == GamePhase.GameOver && mayStartNewGame && Time.timeSinceLevelLoad > mayStartNewGameTime)
        {
            if (Input.anyKeyDown)
            {
                Game.Phase = GamePhase.FactoryReset;
            }
        }
    }
}
