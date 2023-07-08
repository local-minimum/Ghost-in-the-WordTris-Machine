using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerTile : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI TextUI;

    [SerializeField]
    Button button;

    PlayerHand hand;

    public string Letter
    {
        get => TextUI.text;
        set
        {
            TextUI.text = value;
            gameObject.SetActive(!string.IsNullOrEmpty(value));
        }
    }

    public bool Visible => gameObject.activeSelf;

    public bool Interactable
    {
        get => button.interactable;
        set
        {
            if (gameObject.activeSelf)
            {
                button.interactable = value;
            }
        }
    }

    private void Start()
    {
        hand = GetComponentInParent<PlayerHand>();
    }

    public void OnClick()
    {
        hand.PlayTile(this);
    }
}
