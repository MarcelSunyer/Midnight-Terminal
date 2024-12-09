﻿using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System;
using TMPro;

public class ClientUDP : MonoBehaviour
{
    private Socket socket;
    private IPEndPoint serverEndPoint;
    private Queue<System.Action> mainThreadTasks = new Queue<System.Action>();

    public int maxMessage = 25;
    private GameObject general_chat;
    private GameObject chatPanel;
    public GameObject textObject;
    private InputField chatbox;

    public Color playerMessage;
    public Color infoMessage;

    [SerializeField]
    List<Message> messageList = new List<Message>();
    string playerName;

    public GameObject serverRepresentationPrefab;
    private GameObject serverInstance;

    void Start()
    {
        playerName = PlayerPrefs.HasKey("Join_Server_Name") ? PlayerPrefs.GetString("Join_Server_Name") : "No hay texto guardado";
        general_chat = GameObject.Find("GeneralChat");
        chatPanel = GameObject.Find("ChatPanel");
        chatbox = GameObject.FindObjectOfType<InputField>();
        general_chat.SetActive(false);

        string serverIP = PlayerPrefs.GetString("Join_Server_IP", "0.0.0.0");
        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), 9050);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        SendMessageToServer(playerName + " has joined the server");

        if (serverRepresentationPrefab != null && serverInstance == null)
        {
            serverInstance = Instantiate(serverRepresentationPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }

        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();
    }

    void Update()
    {
        while (mainThreadTasks.Count > 0)
        {
            mainThreadTasks.Dequeue().Invoke();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            general_chat.SetActive(!general_chat.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(chatbox.text))
        {
            SendMessageToChat(playerName + ": " + chatbox.text, Message.MessageType.playerMessage);
            SendMessageToServer(playerName + ": " + chatbox.text);
            chatbox.text = "";
        }
        else if (!chatbox.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            chatbox.ActivateInputField();
        }

        SendPlayerPosition();
    }

    void SendPlayerPosition()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        Position data = new Position(position.x, position.y, position.z, rotation);

        string serializedData = "POS:" + Position.Serialize(data);
        SendMessageToServer(serializedData);
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            int recv = socket.ReceiveFrom(data, ref remoteEndPoint);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            if (receivedMessage.StartsWith("POS:"))
            {
                string positionDataStr = receivedMessage.Substring(4);
                Position positionData = Position.Deserialize(positionDataStr);

                mainThreadTasks.Enqueue(() =>
                {
                    UpdateServerPosition(positionData);
                });
            }
            else
            {
                mainThreadTasks.Enqueue(() =>
                {
                    SendMessageToChat(receivedMessage, Message.MessageType.info);
                });
            }
            if (receivedMessage.StartsWith("NAME"))
            {
                string nameServer = receivedMessage.Substring(4);
                

                mainThreadTasks.Enqueue(() =>
                {
                    UpdateServerName(nameServer);
                });
            }
        }
    }
    void UpdateServerName(String name)
    {
        if (serverInstance != null)
        {
            // Busca el componente TextMeshPro en los hijos
            TextMeshPro textMeshPro = serverInstance.GetComponentInChildren<TextMeshPro>();

            if (textMeshPro != null)
            {
                textMeshPro.text = name;
            }
        }
    }

    void UpdateServerPosition(Position data)
    {
        if (serverInstance != null)
        {
            serverInstance.transform.position = new Vector3(data.x, data.y, data.z);
            serverInstance.transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
        }
        else
        {
            Debug.LogWarning("Server instance is null; cannot update position and rotation.");
        }
    }


    void SendMessageToServer(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, serverEndPoint);
    }

    public void SendMessageToChat(string text, Message.MessageType messageType)
    {
        if (messageList.Count >= maxMessage)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }

        Message newMessage = new Message { text = text };
        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text;
        newMessage.textObject.color = MessageTypeColor(messageType);

        messageList.Add(newMessage);
    }

    Color MessageTypeColor(Message.MessageType messageType)
    {
        return messageType == Message.MessageType.playerMessage ? playerMessage : infoMessage;
    }

    [System.Serializable]
    public class Message
    {
        public string text;
        public Text textObject;
        public MessageType messageType;

        public enum MessageType
        {
            playerMessage,
            info
        }
    }
}
