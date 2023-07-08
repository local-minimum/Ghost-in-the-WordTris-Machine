using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetterPosition : MonoBehaviour
{
    [SerializeField]
    Color baseColor;

    [SerializeField]
    Color clearingColor;

    [SerializeField]
    Image image;

    [SerializeField]
    TMPro.TextMeshProUGUI TextUI;

    public bool Occupied
    {
        get => TextUI.enabled;
        private set {
            TextUI.enabled = value;
            image.enabled = value;
        }
    }

    public string Letter
    {
        get => TextUI.text;

        set
        {
            Occupied = !string.IsNullOrEmpty(value);
            TextUI.text = value; 
        }
    }

    bool awaitingClearing = false;

    public bool Clearing => awaitingClearing;

    public void MarkClearing()
    {
        awaitingClearing = true;
        image.enabled = true;
        image.color = clearingColor;
    }

    public void InvokeClears()
    {
        if (!awaitingClearing) return;

        image.color = baseColor;
        Letter = "";
        awaitingClearing = false;
    }
}
