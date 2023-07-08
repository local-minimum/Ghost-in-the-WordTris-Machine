using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    int DrawWhen = 0;

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

    public string HandAsString => string.Join("", hand.Where(t => t.Visible).Select(t => t.Letter));

    private void Update()
    {
        if (VisibleHand > DrawWhen) return;

        var handLetters = letters.Draw(HandSize - VisibleHand);
        int letterIdx = 0;
        for (int handIdx = 0,l=hand.Count; handIdx<l; handIdx++)
        {
            var tile = hand[handIdx];
            if (!tile.Visible)
            {
                tile.Letter = handLetters[letterIdx];
                tile.Interactable = true;
                letterIdx++;
            }
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
