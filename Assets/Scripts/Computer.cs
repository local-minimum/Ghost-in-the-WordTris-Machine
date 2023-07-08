using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Computer : MonoBehaviour
{
    [SerializeField]
    PlayField playField;

    [SerializeField]
    PlayerHand playerHand;

    [SerializeField]
    WordListManager wordList;

    string activeLetter;
    int selectedLane;

    public void ReceiveLetter(string letter)
    {
        activeLetter = letter;
        Game.Phase = GamePhase.SelectLane;
        Debug.Log($"Computer got: {letter}");
    }

    int CheckPattern(string pattern)
    {
        if (wordList.HasWord(pattern, out string word, out int position))
        {
            return word.Length;
        }

        return 0;
    }

    [SerializeField]
    float bestOptionProbability = 0.66f;

    bool BestOption(List<(int, long)> options, out int option)
    {
        if (options.Count == 0)
        {
            Debug.Log("Nothing scores");
            option = -1;
            return false;
        } else if (options.Count == 1)
        {
            Debug.Log($"Only {options[0]} scores");
            option = options[0].Item1;
            return true;
        }

        Debug.Log($"{options.Count} scores");
        options = options.OrderByDescending(item => item.Item2).Take(2).ToList();
        option = options[Random.value < bestOptionProbability ? 0 : 1].Item1;
        return true;
    }

    Dictionary<string, int> patterWordCount = new Dictionary<string, int>();

    [SerializeField]
    double flexibilityImprovementThreshold = 1.1;

    bool FindMostFlexible(PlayField.VirtualBoard[] boards, out int option)
    {
        var options = new List<(int, long)>();
        int maxRow = playField.MaxRow;

        System.Func<string, int> scorePattern = (pattern) =>
        {
            if (patterWordCount.ContainsKey(pattern))
            {
                return patterWordCount[pattern];
            }
            else
            {
                var patternScore = wordList.CountPossible(pattern);
                Debug.Log($"New {pattern} => {patternScore}");
                patterWordCount.Add(pattern, patternScore);
                return patternScore;
            }
        };

        for (int i = 0; i < boards.Length; i++)
        {
            var board = boards[i];

            // Debug.LogWarning($"Board {i}:\n{board}");
            if (!board.Available) continue;

            long score = 0;

            for (int lane = 0; lane < board.Board.Length; lane++)
            {
                score += scorePattern(board.Board[lane]);
            }

            for (int row = 0; row <= maxRow; row++)
            {
                var rowPattern = board.RowPattern(row);
                var rowScore = scorePattern(rowPattern);
                // Debug.Log($"{i},{row}, {rowPattern} => {rowScore}");
                score += rowScore;
            }

            options.Add((board.Lane, score));
            Debug.Log($"Option {board.Lane} got score {score}");
        }

        long worstScore = (long) (
            options.OrderByDescending(item => item.Item2).Last().Item2 
            * flexibilityImprovementThreshold);

        return BestOption(options.Where(item => item.Item2 > worstScore).ToList(), out option);
    }

    bool FindBestScorer(PlayField.VirtualBoard[] boards, out int option) {
        var options = new List<(int, long)>();
        for (int i = 0; i<boards.Length; i++)
        {
            var board = boards[i];

            if (!board.Available) continue;

            var lane = board.Board[board.Lane];
            var score = lane.Split(".").Sum(pattern => CheckPattern(pattern));

            var rowIdx = lane.LastIndexOf('.');
            var row = board.RowPattern(rowIdx + 1);

            score += row.Split(".").Sum(pattern => CheckPattern(pattern));

            if (score > 0)
            {
                options.Add((board.Lane, score));
            }
        }

        return BestOption(options, out option);
    }

    bool FindOpenLane(PlayField.VirtualBoard[] boards, out int option)
    {

        int possible = boards.Count(b => b.Available);
        if (possible == 0)
        {
            Debug.Log($"All lanes closed {possible}");
            option = -1;
            return false;
        }

        var selected = Random.Range(0, possible);
        Debug.Log($"Random selected lane {selected}");
        for (int i = 0; i<boards.Length; i++)
        {
            if (boards[i].Available)
            {
                if (selected == 0)
                {
                    option = boards[i].Lane;
                    Debug.Log($"And it agrees with {option}");
                    return true;
                }

                selected--;
            }
        }

        Debug.Log("There were no open lanes");
        option = -1;
        return false;
    }

    [SerializeField]
    float decisionDuration;

    float decideTime;

    IEnumerator<WaitForSeconds> LookForBest(PlayField.VirtualBoard[] options)
    {
        int option;
        if (FindBestScorer(options, out option))
        {
            selectedLane = option;
        } else
        {
            yield return new WaitForSeconds(0.01f);
            if (FindMostFlexible(options, out option))
            {
                selectedLane = option;
            }
        }
    }

    void ChooseLane()
    {
        var options = playField.SimulateDroppedLetter(activeLetter).ToArray();
        int option;

        if (FindOpenLane(options, out option))
        {
            selectedLane = option;
        } else
        {
            Game.Phase = GamePhase.GameOver;
            return;
        }
        StartCoroutine(LookForBest(options));
    }

    void Drop()
    {
        playField.Drop(activeLetter, selectedLane);
        Game.Phase = GamePhase.Dropping;
    }

    private void OnEnable()
    {
        Game.OnPhaseChange += Game_OnPhaseChange;
    }

    private void OnDisable()
    {
        Game.OnPhaseChange -= Game_OnPhaseChange;
    }

    private void Game_OnPhaseChange(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.SelectLane)
        {
            decideTime = Time.timeSinceLevelLoad + decisionDuration;
            ChooseLane();
        }
    }

    private void Update()
    {
        switch (Game.Phase)
        {
            case GamePhase.SelectLane:
                if (Time.timeSinceLevelLoad > decideTime)
                {
                    StopAllCoroutines();
                    Game.Phase = GamePhase.PreDropping;
                }
                break;
            case GamePhase.PreDropping:
                Drop();
                break;  
        }
    }
}
