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
    }


    private void OnDisable()
    {
        Game.OnPhaseChange -= Game_OnPhaseChange;
        PlayField.OnWord -= PlayField_OnWord;
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

    private void Game_OnPhaseChange(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.Player && (oldPhase == GamePhase.None))
        {
            InfoDump();
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

    private void PlayField_OnWord(List<string> words, int iteration)
    {
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

    private void Update()
    {
        if (Game.Phase == GamePhase.None) return;

        if (InfoDump()) return;
    }

}
