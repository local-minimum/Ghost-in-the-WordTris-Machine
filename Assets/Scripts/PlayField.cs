using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Patterns
{
    public string Vertical;
    public string Horizontal;

    public Patterns(string vertical, string horizontal)
    {
        Vertical = vertical;
        Horizontal = horizontal;
    }
}

public class PlayField : MonoBehaviour
{
    [SerializeField]
    WordListManager wordList;

    [SerializeField]
    LetterLane prefab;

    [SerializeField]
    int LaneCount = 6;

    LetterLane[] lanes;

    void Start()
    {
        lanes = new LetterLane[LaneCount];

        for (int i = 0; i< LaneCount; i++)
        {
            lanes[i] = Instantiate(prefab, transform);
        }
    }

    public IEnumerable<Vector2Int> OpenCoordinates()
    {
        for (int i=0; i<lanes.Length; i++)
        {
            var lane = lanes[i];
            if (lane.Full) continue;

            yield return new Vector2Int(i, lane.FirstOpenRow());
        }
    }

    IEnumerable<T> Apply<T>(System.Func<LetterLane, T> predicate)
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            yield return predicate(lanes[i]);
        }
    }

    public Patterns PatternsAround(Vector2Int coordinates, bool includeWildCards)
    {
        var verticalPattern = string.Join("", lanes[coordinates.x].Pattern(includeWildCards));
        var horizontalPattern = string.Join("", Apply(lane => lane.RowLetter(coordinates.y)));
        if (!includeWildCards)
        {
            int start = coordinates.x;
            while (start >=0)
            {
                if (horizontalPattern[start] == '.')
                {
                    start++;
                    break;
                }
                start--;
            }
            if (start < 0) start = 0;

            int end = coordinates.x;
            while (end < horizontalPattern.Length)
            {
                if (horizontalPattern[end] == '.')
                {
                    end--;
                    break;
                }
                end++;
            }

            Debug.Log($"{coordinates} {horizontalPattern} {start} - {end}");
            if (end < start)
            {
                horizontalPattern = "";
            }
            else
            {
                horizontalPattern = horizontalPattern.Substring(
                    start,
                    Mathf.Min(end - start + 1, horizontalPattern.Length - start)
                );
            }
        }

        return new Patterns(verticalPattern, horizontalPattern);
    }

    void ClearHorizontal(int row, string word)
    {
        // TODO: Handle blanks
        var horizontalPattern = string.Join("", Apply(lane => lane.RowLetter(row)));
        for (int col = horizontalPattern.IndexOf(word), l=col + word.Length; col<l; col++)
        {
            if (col < 0)
            {
                throw new System.ArgumentException($"{word} not on row {row} {horizontalPattern}");
            }
            lanes[col].ClearLetter(row);
        }
    }

    void RemoveCleared()
    {
        for (int i = 0; i<lanes.Length; i++)
        {
            lanes[i].RemoveCleared();
        }
    }

    [SerializeField]
    float dropSpeed = 0.4f;

    bool madeWord;
    bool clearedTiles;
    int lane;
    int row;
    float nextEvent;

    public bool Drop(string letter, int lane)
    {
        nextEvent = Time.timeSinceLevelLoad + dropSpeed;
        this.lane = lane;
        if (!lanes[lane].InsertLetter(letter, out int landingRow))
        {
            Game.Phase = GamePhase.GameOver;
            return false;
        } else
        {
            Debug.Log(landingRow);
            row = landingRow;
        }

        return true;
    }

    void AnimateDrop()
    {
        if (Time.timeSinceLevelLoad < nextEvent) return;
        if (!lanes[lane].ShiftDownOne())
        {
            Game.Phase = GamePhase.Scoring;
        } else
        {
            nextEvent = Time.timeSinceLevelLoad + dropSpeed;
        }
    }

    void ScoreDrop()
    {
        var patterns = PatternsAround(new Vector2Int(lane, row), false);
        Debug.Log($"H: {patterns.Horizontal.Replace(" ", ".")} V: {patterns.Vertical.Replace(" ", ".")}");
        madeWord = false;
        if (wordList.HasWord(patterns.Vertical.Replace(" ", "."), out string verticalWord, out int wordStartVert))
        {
            lanes[lane].ClearWord(verticalWord);
            Debug.Log($"V{row}: {verticalWord}");
            madeWord = true;
        }
        if (wordList.HasWord(patterns.Horizontal.Replace(" ", "."), out string horizontalWord, out int wordStartHor))
        {
            Debug.Log($"H{lane}: {horizontalWord}");
            ClearHorizontal(row, horizontalWord);
            madeWord = true;
        }

        // TODO: Something else should say when scored
        Game.Phase = GamePhase.PostScoreShift;
    }

    bool ShiftDownOneAllLanes() => lanes.Count(l => l.ShiftDownOne()) > 0;

    void PostScoreShift()
    {
        if (madeWord)
        {
            if (Time.timeSinceLevelLoad < nextEvent) return;
            RemoveCleared();
            madeWord = false;
            clearedTiles = true;
        }
        else if (Time.timeSinceLevelLoad > nextEvent)
        {
            if (!ShiftDownOneAllLanes())
            {
                if (clearedTiles)
                {
                    clearedTiles = false;
                    Game.Phase = GamePhase.Scoring;
                } else
                {
                    Game.Phase = GamePhase.Player;
                }
            } else
            {
                nextEvent = Time.timeSinceLevelLoad + dropSpeed;
            }
        }
    }

    private void OnEnable()
    {
        Game.OnPhaseChange += Game_OnPhaseChange;
    }

    private void OnDisable()
    {
        Game.OnPhaseChange -= Game_OnPhaseChange;
    }

    [SerializeField]
    float scoreTime = 1;

    private void Game_OnPhaseChange(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.Scoring)
        {
            ScoreDrop();
        } else if (newPhase == GamePhase.PostScoreShift)
        {
            if (madeWord)
            {
                nextEvent = Time.timeSinceLevelLoad + scoreTime;
            }
        }
    }

    private void Update()
    {
        switch (Game.Phase)
        {
            case GamePhase.Dropping:
                AnimateDrop();
                break;
            case GamePhase.PostScoreShift:
                PostScoreShift();
                break;
        }
    }
}
