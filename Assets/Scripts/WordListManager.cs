using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class WordListManager : MonoBehaviour
{
    [SerializeField]
    string wordsResource;

    [SerializeField]
    int minLength = 3;

    [SerializeField]
    int maxLength = 9;

    Dictionary<int, HashSet<string>> words = new Dictionary<int, HashSet<string>>();
    Dictionary<int, int> wordCounts = new Dictionary<int, int>();


    void Start()
    {
        var resource = Resources.Load<TextAsset>(wordsResource);
        int length = 0;
        foreach(string word in resource.text.Split("\n").Select(line => line.Trim()).Where(w => w.Length >= minLength && w.Length <= maxLength).OrderBy(w => w.Length))
        {
            if (word.Length > length)
            {
                if (length > 0)
                {
                    Debug.Log($"Loaded words of length {length}: {words[length].Count}");
                }
                length = word.Length;
                words[length] = new HashSet<string>();
            }
            words[length].Add(word);
        }
        Debug.Log($"Loaded words of length {length}: {words[length].Count}");

        foreach(int wordLength in words.Keys)
        {
            wordCounts[wordLength] = words[wordLength].Count;
        }

        // DebugFuctionality("OXE");
    }

    
    void DebugFuctionality(string line)
    {

        if (HasWord("OXE", out var result, out var position))
        {
            Debug.Log($"Found word {result} at {position}");
        } else
        {
            Debug.Log("Not a word");
        }

        Debug.Log($"Possible words: {CountPossible(line)}");
    }

    IEnumerable<string> GenerateOptions(string line)
    {
        int length = line.Length;
        while (length >= minLength)
        {
            for (int i = 0, max = line.Length - length; i<=max; i++)
            {
                yield return line.Substring(i, length);
            }
            length--;
        }
    }

    public bool HasWord(string line, out string word, out int position)
    {
        if (!words.ContainsKey(line.Length))
        {
            word = "";
            position = -1;
            return false;
        }

        Debug.Log($"Evaluating {line} with threshold {Game.MinWordLength}");
        foreach (var option in GenerateOptions(line))
        {
            if (option.Length < Game.MinWordLength) continue;

            if (words[option.Length].Contains(option)) {
                position = line.IndexOf(option);
                word = option;
                return true;
            }
        }
        word = "";
        position = -1;
        return false;
    }


    public int CountPossible(string line)
    {
        int hits = 0;

        foreach(var option in GenerateOptions(line))
        {
            bool free = option == new string('.', option.Length);
            var pattern = $"^{option}$";
            var rx = new Regex(pattern, RegexOptions.Compiled);
            for (int i = option.Length; i<=line.Length; i++)
            {
                if (free) {
                    hits += wordCounts[i];
                } else
                {
                    hits += words[i].Where(w => rx.IsMatch(w)).Count();
                }
            }
        }

        return hits;
    }
}
