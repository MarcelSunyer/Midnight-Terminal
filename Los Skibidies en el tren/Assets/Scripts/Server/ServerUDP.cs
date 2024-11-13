using System.Collections;
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
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>(); // Cola para mensajes
    private List<EndPoint> connectedClients = new List<EndPoint>(); // Lista para clientes conectados

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
    void Start()
    {

        player_name = PlayerPrefs.HasKey("Name_Player") ? PlayerPrefs.GetString("Name_Player") : "No hay texto guardado";
        general_chat = GameObject.Find("GeneralChat");
        chatPanel = GameObject.Find("ChatPanel");
        chatbox = GameObject.FindObjectOfType<InputField>();

        // Inicializar el componente de chat
        general_chat.SetActive(false);

        // Configurar el socket del servidor
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        // Iniciar el hilo para recibir mensajes
        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();

        // Mostrar mensaje inicial en el chat
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
            general_chat.SetActive(!general_chat.activeSelf); // Activar/desactivar chat
        }

        // Enviar mensaje de chat desde el servidor
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(chatbox.text))
        {
            string message = chatbox.text;
            SendMessageToChat(player_name + ": " + message, Message.MessageType.playerMessage);
            BroadcastMessage(player_name + ": " + message, null); // Difundir mensaje a todos los clientes
            chatbox.text = ""; // Limpiar el input
        }
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        EndPoint remoteClient = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            int recv = socket.ReceiveFrom(data, ref remoteClient);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            // Registrar nuevo cliente
            if (!connectedClients.Contains(remoteClient))
            {
                connectedClients.Add(remoteClient);
            }

            // Añadir mensaje recibido a la cola y difundir a otros clientes
            messageQueue.Enqueue($"{receivedMessage}");
            BroadcastMessage(receivedMessage, remoteClient);
        }
    }

    void BroadcastMessage(string message, EndPoint sender)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        foreach (var client in connectedClients)
        {
            if (sender == null || !client.Equals(sender)) // No reenviar al remitente si hay uno
            {
                socket.SendTo(data, client);
            }
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