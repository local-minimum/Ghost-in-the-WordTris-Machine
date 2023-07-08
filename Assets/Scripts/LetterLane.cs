using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterLane : MonoBehaviour
{
    [SerializeField]
    LetterPosition prefab;

    [SerializeField]
    int depth = 9;

    LetterPosition[] letters;

    void Start()
    {
        letters = new LetterPosition[depth];

        for (int i = 0; i < depth; i++)
        {
            var letter = Instantiate(prefab, transform);
            letter.Letter = "";
            letters[i] = letter;
        }
    }

    public bool Full
    {
        get => letters[0].Occupied;
    }

    public int RowIndexToRow(int index) => letters.Length - index - 1;
    public int RowToRowIndex(int row) => letters.Length - row - 1;

    public bool DropLetter(string letter, out int row)
    {
        row = FirstOpenRow();

        if (row >= 0)
        {
            letters[RowToRowIndex(row)].Letter = letter;
            return true;
        }

        return false;
    }

    public int FirstOpenRow()
    {
        for (int i=letters.Length - 1; i>=0; i--)
        {
            if (!letters[i].Occupied)
            {
                return RowIndexToRow(i);
            }
        }

        return -1;
    }

    public string RowLetter(int row)
    {
        var letter = letters[RowToRowIndex(row)];
        if (letter.Occupied)
        {
            return letter.Letter;
        }
        return ".";
    }

    public void ClearLetter(int row)
    {
        letters[RowToRowIndex(row)].MarkClearing();
    }

    public void ClearWord(string word)
    {
        var pattern = string.Join("", Pattern(true));
        for (int i=pattern.IndexOf(word), l = i + word.Length; i<l; i++)
        {
            ClearLetter(RowIndexToRow(i));
        }
    }

    public bool InsertLetter(string letter, out int landingRow)
    {
        if (!Full)
        {
            letters[0].Letter = letter;
            for (int i = 1; i<letters.Length; i++)
            {
                if (letters[i].Occupied)
                {
                    landingRow = RowToRowIndex(i - 1);
                    return true;
                }
            }
            landingRow = 0;
            return true;
        }
        landingRow = -1;
        return false;
    }

    public bool ShiftDownOne()
    {
        bool shifted = false;
        var prevLetter = letters[letters.Length - 1];
        for (int i = letters.Length - 2; i >= 0; i--)
        {
            var letter = letters[i];
            if (letter.Occupied && !prevLetter.Occupied)
            {
                prevLetter.Letter = letter.Letter;
                letter.Letter = "";
                shifted = true;
            }
            prevLetter = letter;
        }
        return shifted;
    }

    public void RemoveCleared()
    {
        for (int i = 0; i < letters.Length; i++)
        {
            letters[i].InvokeClears();
        }
    }

    // Deprecated
    public void InstaDrop()
    {
        int dropTo = -1;
        for (int i = letters.Length - 1; i>=0; i--)
        {
            var letter = letters[i];
            if (letter.Clearing)
            {
                // Debug.Log($"Clear Row {i}: {letter.Letter}");
                letter.InvokeClears();
                if (dropTo == -1) dropTo = i;
            } else if (dropTo != -1 && letter.Occupied)
            {
                // Debug.Log($"Move {i}->{dropTo}: {letter.Letter}->{letters[dropTo].Letter}");
                letters[dropTo].Letter = letter.Letter;
                letter.Letter = "";
                dropTo--;
            } else 
            {
                // Debug.Log($"Ignore {i}: {letter.Letter}");
            }
        }
    }

    public IEnumerable<string> Pattern(bool withWildCards = false)
    {
        for (int i=0; i<letters.Length;i++)
        {
            if (letters[i].Occupied)
            {
                yield return letters[i].Letter;
            } else if (withWildCards)
            {
                yield return ".";
            }
        }
    }
}
