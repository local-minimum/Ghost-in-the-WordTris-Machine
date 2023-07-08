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

    string activeLetter;
    Vector2Int selectedDropTarget;

    public void ReceiveLetter(string letter)
    {
        Game.Phase = GamePhase.SelectLane;
        Debug.Log($"Computer got: {letter}");
        activeLetter = letter;
    }

    void ChooseLane()
    {
        if (Random.value < 0.2f)
        {
            var options = playField.OpenCoordinates().ToArray();
            selectedDropTarget = options[Random.Range(0, options.Length)];
            Game.Phase = GamePhase.PreDropping;
        }
    }

    void Drop()
    {
        playField.Drop(activeLetter, selectedDropTarget.x);
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
