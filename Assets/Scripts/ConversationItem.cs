using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationItem : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI MessageUI;

    public string Message
    {
        set => MessageUI.text = value;
    }
}

