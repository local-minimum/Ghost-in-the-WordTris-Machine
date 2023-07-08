using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreWord : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI WordUI;

    [SerializeField]
    TMPro.TextMeshProUGUI ScoreUI;

    public string Word
    {
        get => WordUI.text;
        set => WordUI.text = value;
    }

    public int Score
    {
        get
        {
            if (int.TryParse(ScoreUI.text, out int value))
            {
                return value;
            }
            return 0;
        }

        set
        {
            ScoreUI.text = value.ToString();
        }
    }
}
