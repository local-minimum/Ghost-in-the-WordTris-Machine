using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ChangePhaseEvent(GamePhase oldPhase, GamePhase newPhase);

public static class Game
{
    public static event ChangePhaseEvent OnPhaseChange;

    private static GamePhase phase = GamePhase.None;

    public static GamePhase Phase
    {
        get => phase;
        set
        {
            var oldPhase = phase;
            phase = value;
            OnPhaseChange?.Invoke(oldPhase, value);
        }
    }

    public static int MinWordLength { get; set; }
}
