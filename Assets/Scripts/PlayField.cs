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

public delegate void WordEvent(List<string> words, int iteration);


public class PlayField : MonoBehaviour
{
    public static event WordEvent OnWord;

    [SerializeField]
    WordListManager wordList;

    [SerializeField]
    LetterLane prefab;

    [SerializeField]
    int LaneCount = 6;

    LetterLane[] lanes;

    public int MaxRow => lanes[0].MaxRow;

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
    int scoreIterations = 0;

    public bool Drop(string letter, int lane)
    {
        nextEvent = Time.timeSinceLevelLoad + dropSpeed;
        this.lane = lane;
        scoreIterations = 0;
        if (!lanes[lane].InsertLetter(letter, out int landingRow))
        {
            Game.Phase = GamePhase.GameOver;
            return false;
        } else
        {
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

    bool CheckLaneForWord(int lane, out string word)
    {
        var verticalPattern = string.Join("", lanes[lane].Pattern(false));
        if (wordList.HasWord(verticalPattern.Replace(" ", "."), out string verticalWord, out int wordStartVert))
        {
            word = verticalWord;
            lanes[lane].ClearWord(verticalWord);
            return true;
        }

        word = "";
        return false;
    }

    bool CheckRowForWords(int row, out List<string> words)
    {
        words = new List<string>();

        var horizontalCandidates = string.Join("", Apply(lane => lane.RowLetter(row))).Split(".");
        for (int i = 0, left = 0; i < horizontalCandidates.Length; i++)
        {
            var candidate = horizontalCandidates[i];        

            if (wordList.HasWord(candidate.Replace(" ", "."), out string word, out int wordStart))
            {
                words.Add(word);

                for (int lane = left + wordStart, l = lane + word.Length; lane<l; lane++)
                {
                    lanes[lane].ClearLetter(row);
                }
            }

            // One extra for the dot
            left += candidate.Length + 1;
        }

        return words.Count > 0;
    }

    void ScoreDrop()
    {
        madeWord = false;
        List<string> words = new List<string>();
        if (CheckLaneForWord(lane, out string verticalWord))
        {
            words.Add(verticalWord);
        }
        if (CheckRowForWords(row, out List<string> horizontalWords))
        {
            words.AddRange(horizontalWords);
        }

        if (words.Count > 0)
        {
            OnWord?.Invoke(words, scoreIterations);
            madeWord = true;
        }
        // TODO: Something else should say when scored
        Game.Phase = GamePhase.PostScoreShift;
    }

    void ScoreEntireBoard()
    {
        madeWord = false;
        List<string> words = new List<string>();

        for (int lane = 0; lane<lanes.Length; lane++)
        {
            if (CheckLaneForWord(lane, out string verticalWord))
            {
                words.Add(verticalWord);
            }
        }

        for (int row = 0, l=lanes[0].MaxRow; row<=l; row++)
        {
            if (CheckRowForWords(row, out List<string> rowWords))
            {
                words.AddRange(rowWords);
            }
        }

        if (words.Count > 0)
        {
            OnWord?.Invoke(words, scoreIterations);
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
            scoreIterations++;
            if (scoreIterations == 1)
            {
                ScoreDrop();
            } else
            {
                ScoreEntireBoard();
            }
        } else if (newPhase == GamePhase.PostScoreShift)
        {
            if (madeWord)
            {
                nextEvent = Time.timeSinceLevelLoad + scoreTime;
            }
        }
    }

    public class VirtualBoard
    {
        public bool Available;
        public int Lane;
        public string[] Board;

        public VirtualBoard(bool available, string[] board, int lane, string substitute)
        {
            Available = available;
            Lane = lane;
            Board = available ? board.Select((l, i) => i == lane ? substitute : l).ToArray() : board;
        }

        public string RowPattern(int row) => string.Join("", Board.Select(lane => lane[row]));

        public override string ToString()
        {
            List<string> lines = new List<string>();
            for (int i=0, l=Board[0].Length; i<l; i++)
            {
                lines.Add(RowPattern(i));
            }
            return string.Join("\n", lines);
        }
    }

    public IEnumerable<VirtualBoard> SimulateDroppedLetter(string letter)
    {
        var patterns = lanes.Select(lane => string.Join("", lane.Pattern(true))).ToArray();

        for (int i = 0; i<lanes.Length; i++)
        {
            var lanePattern = patterns[i];
            var dropTargetIdx = lanePattern.LastIndexOf(".");
            if (dropTargetIdx == -1)
            {
                yield return new VirtualBoard(false, patterns, i, patterns[i]);
            } else
            {
                var pattern = patterns[i];
                var pos = pattern.LastIndexOf(".");
                var sb = new System.Text.StringBuilder(pattern);
                sb[pos] = letter[0];
                yield return new VirtualBoard(true, patterns, i, sb.ToString());
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
