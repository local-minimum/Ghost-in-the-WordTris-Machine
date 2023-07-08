using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSpeaker : MonoBehaviour
{
    [SerializeField]
    AudioClip simpleWord;

    [SerializeField]
    AudioClip betterWord;

    [SerializeField]
    AudioClip bestWord;

    AudioSource speaker;

    private void OnEnable()
    {
        speaker = GetComponent<AudioSource>();
        PlayField.OnWord += PlayField_OnWord;
    }

    private void OnDisable()
    {
        PlayField.OnWord -= PlayField_OnWord;
    }

    private void PlayField_OnWord(List<string> words, int iteration)
    {
        if (iteration > 2)
        {
            speaker.PlayOneShot(bestWord);
        } else if (iteration > 1 || words.Count > 1)
        {
            speaker.PlayOneShot(betterWord);
        } else
        {
            speaker.PlayOneShot(simpleWord);
        }
    }
}
