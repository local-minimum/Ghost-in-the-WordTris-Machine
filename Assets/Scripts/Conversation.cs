using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conversation : MonoBehaviour
{
    [SerializeField]
    ConversationItem prefab;

    private void OnEnable()
    {
        Game.OnPhaseChange += Game_OnPhaseChange;
        PlayField.OnWord += PlayField_OnWord;
        Game.OnChangeMinWordLength += Game_OnChangeMinWordLength;
    }


    private void OnDisable()
    {
        Game.OnChangeMinWordLength -= Game_OnChangeMinWordLength;
        Game.OnPhaseChange -= Game_OnPhaseChange;
        PlayField.OnWord -= PlayField_OnWord;
    }


    private void Game_OnChangeMinWordLength(int minLength)
    {
        if (minLength <= 3) return;
        if (minLength == 4)
        {
            GetItem().Message = "Oh so it's four letter words now";
            return;
        } else if (minLength == 5)
        {
            GetItem().Message = "Five letter words ain't fair!";
        } else
        {
            GetItem().Message = $"{minLength} is just not possible, this is not a game!";
        }

    }

    [SerializeField]
    int maxItems = 5;

    Queue<ConversationItem> Items = new Queue<ConversationItem>();

    ConversationItem GetItem()
    {
        if (Items.Count >= maxItems)
        {
            ConversationItem item = Items.Dequeue();
            item.transform.SetAsFirstSibling();
            Items.Enqueue(item);
            return item;
        } else
        {
            var item = Instantiate(prefab, transform);
            item.transform.SetAsFirstSibling();
            Items.Enqueue(item);
            return item;
        }
    }

    [SerializeField]
    float infoDumpEmbargo = 5f;
    float nextInfoDump;
    int infoDumpStep = 0;

    [SerializeField]
    string[] infoDumps;

    bool InfoDump()
    {
        if (Time.timeSinceLevelLoad < nextInfoDump) return false;

        if (infoDumpStep >= infoDumps.Length) return false;

        GetItem().Message = infoDumps[infoDumpStep];
        infoDumpStep++;

        nextInfoDump = Time.timeSinceLevelLoad + infoDumpEmbargo;
        return true;
    }

    [SerializeField]
    string[] retryGame;

    [SerializeField]
    int frustrationThreshold = 7;

    [SerializeField]
    string[] frustrations;

    [SerializeField]
    float complainWaitDuration = 20;

    int waitComplainIterations = 1;
    float playerTurnComplainTime;
    private void Game_OnPhaseChange(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.Player)
        {
            waitComplainIterations = 1;
            noWordRounds++;
            playerTurnComplainTime = Time.timeSinceLevelLoad + complainWaitDuration * waitComplainIterations;
        }


        if (newPhase == GamePhase.Player && (oldPhase == GamePhase.None))
        {
            InfoDump();
        } else if (newPhase == GamePhase.FactoryReset)
        {
            GetItem().Message = retryGame[Random.Range(0, retryGame.Length)];
        } else if (newPhase == GamePhase.Player && noWordRounds > frustrationThreshold)
        {
            GetItem().Message = frustrations[Random.Range(0, frustrations.Length)];
        }
    }


    float nextCelebration;

    [SerializeField]
    float celebrationEmbargoDuration = 4f;

    [SerializeField]
    string[] iterationCelebrations;

    [SerializeField]
    string[] multiWordCelebrations;

    [SerializeField]
    string[] wordCelebrations;

    private string FormatCelebration(string celerbration, List<string> words, int iteration)
    {
        int nWords = words.Count;
        var word = nWords > 0 ? words[Random.Range(0, nWords)] : "";
        return celerbration
            .Replace("%iter", iteration.ToString())
            .Replace("%word", word)
            .Replace("%count", nWords.ToString());
    }

    private void PublishRandomPick(string[] options, List<string> words, int iteration)
    {
        GetItem().Message = FormatCelebration(options[Random.Range(0, options.Length)], words, iteration);
        nextCelebration = Time.timeSinceLevelLoad + celebrationEmbargoDuration;
    }

    int noWordRounds;

    private void PlayField_OnWord(List<string> words, int iteration)
    {
        noWordRounds = 0;

        if (Time.timeSinceLevelLoad < nextCelebration) return;

        if (iteration > 1)
        {
            PublishRandomPick(iterationCelebrations, words, iteration);
        } else if (words.Count > 1)
        {
            PublishRandomPick(multiWordCelebrations, words, iteration);
        } else if (words.Count == 1)
        {
            PublishRandomPick(wordCelebrations, words, iteration);
        }
    }

    [SerializeField]
    string[] waitComplaints;

    private void Update()
    {
        if (Game.Phase == GamePhase.None) return;

        if (InfoDump()) return;

        if (Game.Phase == GamePhase.Player)
        {
            if (Time.timeSinceLevelLoad > playerTurnComplainTime)
            {
                waitComplainIterations++;
                playerTurnComplainTime = Time.timeSinceLevelLoad + complainWaitDuration * waitComplainIterations;
                GetItem().Message = waitComplaints[Random.Range(0, waitComplaints.Length)];
            }
        }
    }

}
