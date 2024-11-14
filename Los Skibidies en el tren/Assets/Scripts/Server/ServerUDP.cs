using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Collections.Concurrent;

public class ServerUDP : MonoBehaviour
{
    private Socket socket;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    private List<EndPoint> connectedClients = new List<EndPoint>();
    private Dictionary<EndPoint, Position> clientPositions = new Dictionary<EndPoint, Position>();

    public int maxMessage = 25;
    private GameObject general_chat;
    private GameObject chatPanel;
    public GameObject textObject;
    private InputField chatbox;

    public Color playerMessage;
    public Color infoMessage;

    [SerializeField]
    List<Message> messageList = new List<Message>();

    string player_name;

    public GameObject serverObject;
    private GameObject clientPlayerInstance;

    void Start()
    {
        player_name = PlayerPrefs.HasKey("Name_Player") ? PlayerPrefs.GetString("Name_Player") : "No hay texto guardado";
        general_chat = GameObject.Find("GeneralChat");
        chatPanel = GameObject.Find("ChatPanel");
        chatbox = GameObject.FindObjectOfType<InputField>();

        general_chat.SetActive(false);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();

        SendMessageToChat("Starting server...", Message.MessageType.info);
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            SendMessageToChat(message, Message.MessageType.info);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            general_chat.SetActive(!general_chat.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(chatbox.text))
        {
            string message = chatbox.text;
            SendMessageToChat(player_name + ": " + message, Message.MessageType.playerMessage);
            BroadcastMessage(player_name + ": " + message, null);
            chatbox.text = "";
        }

        BroadcastServerPosition();
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        EndPoint remoteClient = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            int recv = socket.ReceiveFrom(data, ref remoteClient);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            if (!connectedClients.Contains(remoteClient))
            {
                connectedClients.Add(remoteClient);
                AddClient(remoteClient);
            }

            if (receivedMessage.StartsWith("POS:"))
            {
                string positionDataStr = receivedMessage.Substring(4);
                Position positionData = Position.Deserialize(positionDataStr);
                clientPositions[remoteClient] = positionData;
                UpdateClientPlayerPosition(positionData);
                BroadcastPosition(positionData, remoteClient);
            }
            else if (receivedMessage.StartsWith("CLICK:"))
            {
                string clickDataStr = receivedMessage.Substring(6);
                Position clickPosition = Position.Deserialize(clickDataStr);
                HandleClickPosition(remoteClient, clickPosition);
            }
            else
            {
                messageQueue.Enqueue($"{receivedMessage}");
                BroadcastMessage(receivedMessage, remoteClient);
            }
        }
    }

    void HandleClickPosition(EndPoint client, Position clickPosition)
    {
        Debug.Log($"Clic recibido de {client}: Posición {clickPosition.x}, {clickPosition.y}, {clickPosition.z}");
    }

    void AddClient(EndPoint clientEndpoint)
    {
        if (serverObject != null && clientPlayerInstance == null)
        {
            clientPlayerInstance = Instantiate(serverObject, new Vector3(0, 1, 0), Quaternion.identity);
        }
    }

    void UpdateClientPlayerPosition(Position positionData)
    {
        if (clientPlayerInstance != null)
        {
            clientPlayerInstance.transform.position = new Vector3(positionData.x, positionData.y, positionData.z);
        }
    }

    void BroadcastServerPosition()
    {
        if (serverObject == null) return;

        Position serverPosition = new Position(serverObject.transform.position.x, serverObject.transform.position.y, serverObject.transform.position.z);
        BroadcastPosition(serverPosition, null);
    }

    void BroadcastPosition(Position position, EndPoint sender)
    {
        string serializedPosition = Position.Serialize(position);
        byte[] data = Encoding.ASCII.GetBytes(serializedPosition);

        foreach (var client in connectedClients)
        {
            if (sender == null || !client.Equals(sender))
            {
                socket.SendTo(data, client);
            }
        }
    }

    void BroadcastMessage(string message, EndPoint sender)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        foreach (var client in connectedClients)
        {
            if (sender == null || !client.Equals(sender))
            {
                socket.SendTo(data, client);
            }
        }
    }

    public void SendMessageToChat(string text, Message.MessageType messageType)
    {
        if (messageList.Count >= maxMessage)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }

        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text;
        newMessage.textObject.color = MessageTypeColor(messageType);

        messageList.Add(newMessage);
    }

    Color MessageTypeColor(Message.MessageType messageType)
    {
        Color color = infoMessage;

        switch (messageType)
        {
            case Message.MessageType.playerMessage:
                color = playerMessage;
                break;
        }
        return color;
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
