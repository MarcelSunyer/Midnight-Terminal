using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class ClientUDP : MonoBehaviour
{
    private Socket socket;
    private IPEndPoint serverEndPoint;
    private Queue<System.Action> mainThreadTasks = new Queue<System.Action>(); // Cola para tareas del hilo principal

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
    void Start()
    {
        //Get UserName
        playerName = PlayerPrefs.HasKey("Join_Server_Name") ? PlayerPrefs.GetString("Join_Server_Name") : "No hay texto guardado";

        general_chat = GameObject.Find("GeneralChat");
        chatPanel = GameObject.Find("ChatPanel");
        chatbox = GameObject.FindObjectOfType<InputField>();
        general_chat.SetActive(false);
        // Configurar el endpoint del servidor
        string serverIP = PlayerPrefs.GetString("Join_Server_IP", "0.0.0.0"); // IP predeterminada
        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), 9050);

        // Crear y configurar el socket del cliente
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Enviar mensaje inicial y empezar a recibir
        SendMessageToServer(playerName + " has joined to the server");
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
            general_chat.SetActive(!general_chat.activeSelf); // Activar/desactivar chat
        }

        // Enviar mensaje de chat al presionar Enter y no estar vacío
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(chatbox.text))
        {
            SendMessageToChat(playerName + ": " + chatbox.text, Message.MessageType.playerMessage); // Mostrar en chat local
            SendMessageToServer(playerName + ": " + chatbox.text); // Enviar al servidor
            chatbox.text = ""; // Limpiar el input
        }
        else if (!chatbox.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            chatbox.ActivateInputField();
        }
    }

    void SendMessageToServer(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, serverEndPoint); // Enviar mensaje al servidor
    }

    void Receive()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remoteEndPoint = (EndPoint)sender;
        byte[] data = new byte[1024];

        while (true)
        {
            int recv = socket.ReceiveFrom(data, ref remoteEndPoint);
            string receivedText = Encoding.ASCII.GetString(data, 0, recv);
            mainThreadTasks.Enqueue(() => SendMessageToChat(receivedText, Message.MessageType.info));
        }
    }

    // Métodos del sistema de chat
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
