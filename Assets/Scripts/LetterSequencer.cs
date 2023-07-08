using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LetterSequencer : MonoBehaviour
{
    readonly Dictionary<char, int> frequencies = new Dictionary<char, int>()
    {
        { 'E', 12 }, { 'A', 9 }, {'I', 9}, { 'O', 8 }, { 'N', 6 }, {'R', 6 }, {'T', 6 }, {'L', 4 }, {'S', 4}, {'U', 4},
        { 'D', 4 }, { 'G', 3 }, { 'B', 2 }, { 'C', 2 }, {'M', 2 }, {'P', 2},
        {'F', 2 }, {'H', 2}, {'V', 2}, {'W', 2}, {'Y', 2},
        {'K', 1 },
        {'J', 1 }, {'X', 1},
        {'Q', 1 }, {'Z', 1 },
        // {' ', 2},
    };

    List<string> DrawStash = new List<string>();

    void CreateStash()
    {
        DrawStash = string
            .Join("", frequencies.Keys.Select(key => new string(key, frequencies[key])))
            .ToList()
            .Select(ch => ch.ToString())
            .ToList();

        DrawStash.Shuffle();
    }

    public List<string> Draw(int count)
    {
        var take = DrawStash.Take(count).ToList();

        DrawStash = DrawStash.Skip(take.Count).ToList();

        if (take.Count() < count)
        {
            CreateStash();
            var remainder = DrawStash.Take(count - take.Count()).ToList();
            DrawStash = DrawStash.Skip(remainder.Count).ToList();

            take.AddRange(remainder);
        }

        return take;
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
        if (newPhase == GamePhase.FactoryReset)
        {
            DrawStash.Clear();
        }
    }
}
