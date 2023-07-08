using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ChangePhaseEvent(GamePhase oldPhase, GamePhase newPhase);
public delegate void ChangeMinWordLength(int minLength);

public static class Game
{
    public static event ChangePhaseEvent OnPhaseChange;
    public static event ChangeMinWordLength OnChangeMinWordLength;

    private static GamePhase phase = GamePhase.None;

    public static GamePhase Phase
    {
        get => phase;
        set
        {
            var oldPhase = phase;
            phase = value;
            Debug.Log($"Phase {oldPhase} -> {value}");
            OnPhaseChange?.Invoke(oldPhase, value);

            if (value == GamePhase.FactoryReset)
            {
                oldPhase = value;
                phase = GamePhase.Player;
                Debug.Log($"Second Phase {oldPhase} -> {phase}");
                OnPhaseChange?.Invoke(oldPhase, value);
            }
        }
    }

    private static int minWordLength = 3;
    public static int MinWordLength {
        get => minWordLength;

        set
        {
            minWordLength = value;
            OnChangeMinWordLength?.Invoke(value);
        }
    }
}
