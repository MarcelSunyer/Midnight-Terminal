using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Message_chat : MonoBehaviour
{
    public int maxMessage = 25;

    public GameObject general_chat;
    public GameObject chatPanel;
    public GameObject textObject;
    public InputField chatbox;

    [SerializeField]
    List<Message> messageList = new List<Message>();
    void Start()
    {
        general_chat.SetActive(false);
    }

   void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Cambia entre activado y desactivado
            general_chat.SetActive(!general_chat.activeSelf);
        }

        if (general_chat.activeSelf)
        {
            if (chatbox.text != "")
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    SendMessageToChat(chatbox.text);
                    chatbox.text = "";
                }
            }
            if (chatbox.isFocused)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SendMessageToChat("Sborrada Premium");
                }
            }
        }
    }

    public void SendMessageToChat(string text)
    {
        if(messageList.Count >= maxMessage)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
        }

        Message newMessage = new Message();

        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);

        newMessage.textObject = newText.GetComponent<Text>();

        newMessage.textObject.text = newMessage.text;

        messageList.Add(newMessage);
    }
}

[System.Serializable]
public class Message
{
    public string text;
    public Text textObject;
}
