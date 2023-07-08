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
        Game.Phase = GamePhase.SelectLane;
        Debug.Log($"Computer got: {letter}");
        activeLetter = letter;
    }

    int CheckPattern(string pattern)
    {
        if (wordList.HasWord(pattern, out string word, out int position))
        {
            return word.Length;
        }

        return 0;
    }

    bool FindBestScorer(PlayField.VirtualBoard[] boards, out int option) {
        var options = new List<(int, int)>();
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
        option = options[Random.Range(0, 3) / 2].Item1;
        return true;
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

    void ChooseLane()
    {
        if (Random.value < 0.2f)
        {
            var options = playField.SimulateDroppedLetter(activeLetter).ToArray();
            int option;
            if (FindBestScorer(options, out option))
            {
                selectedLane = option;
            } else if (FindOpenLane(options, out option))
            {
                selectedLane = option;
            } else
            {
                Game.Phase = GamePhase.GameOver;
                return;
            }

            Game.Phase = GamePhase.PreDropping;
        }
    }

    void Drop()
    {
        playField.Drop(activeLetter, selectedLane);
        Game.Phase = GamePhase.Dropping;
    }

    private void Update()
    {
        switch (Game.Phase)
        {
            case GamePhase.SelectLane:
                ChooseLane();
                break;
            case GamePhase.PreDropping:
                Drop();
                break;  
        }
    }
}
