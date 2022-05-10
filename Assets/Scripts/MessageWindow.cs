using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour
{
    public Image messageIcon;
    public Text messageText;
    public Text buttonText;

    public void ShowMessage(Sprite sprite = null, string message = "", string buttonMsg = "Start")
    {
        if (sprite != null) { messageIcon.sprite = sprite; }
        if (message != null) { messageText.text = message; }
        if (buttonMsg != null) { buttonText.text = buttonMsg; }
    }
}
