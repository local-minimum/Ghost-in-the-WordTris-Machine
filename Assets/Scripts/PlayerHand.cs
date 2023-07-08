using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerHand : MonoBehaviour
{
    [SerializeField]
    int HandSize = 6;

    [SerializeField]
    PlayerTile prefab;

    [SerializeField]
    LetterSequencer letters;

    List<PlayerTile> hand = new List<PlayerTile>();

    [SerializeField]
    Computer computer;

    public int VisibleHand { get; private set; }

    private bool Interactable
    {
        set
        {
            for (int i = 0; i<hand.Count; i++)
            {
                hand[i].Interactable = value;
            }
        }
    }

    private void Start()
    {
        for (int i = 0; i<HandSize; i++)
        {
            var tile = Instantiate(prefab, transform);
            tile.Letter = "";
            hand.Add(tile);
        }

        Game.Phase = GamePhase.Player;
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
        Interactable = newPhase == GamePhase.Player;
    }

    private void Update()
    {
        if (VisibleHand > 0) return;

        var handLetters = letters.Draw(HandSize);

        for (int i = 0; i<HandSize; i++)
        {
            var tile = hand[i];
            tile.Letter = handLetters[i];
            tile.Interactable = true;
        }
        VisibleHand = HandSize;
    }

    public void PlayTile(PlayerTile tile)
    {
        VisibleHand--;
        var letter = tile.Letter;
        tile.Letter = "";
        tile.gameObject.SetActive(false);
        computer.ReceiveLetter(letter);
    }
}
