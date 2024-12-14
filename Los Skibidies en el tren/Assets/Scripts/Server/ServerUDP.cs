using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Collections.Concurrent;
using System;

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

    // GameObject usado para crear instancias de clientes conectados
    public GameObject serverObject;

    // GameObject dinámico para transmitir la posición del servidor
    public GameObject dynamicServerObject;

    private Dictionary<EndPoint, GameObject> clientPlayerInstances = new Dictionary<EndPoint, GameObject>();
    private Queue<System.Action> mainThreadActions = new Queue<System.Action>(); 


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
        // Procesa todas las acciones encoladas para el hilo principal
        while (mainThreadActions.Count > 0)
        {
            mainThreadActions.Dequeue()?.Invoke();
        }

        // Lógica existente...
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

        if (clientPlayerInstances.Count > 1)
        {
            SendPossitionBetweenClients();
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

            // Validar si el cliente es nuevo
            if (!connectedClients.Contains(remoteClient))
            {
                connectedClients.Add(remoteClient);
                mainThreadActions.Enqueue(() => AddClient(remoteClient));
            }

            // Manejar mensajes de posición
            if (receivedMessage.StartsWith("POS:"))
            {
                string positionDataStr = receivedMessage.Substring(4);

                // Manejo seguro de datos
                if (Position.TryDeserialize(positionDataStr, out Position positionData))
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        if (clientPlayerInstances.ContainsKey(remoteClient))
                        {
                            var clientObject = clientPlayerInstances[remoteClient];
                            clientObject.transform.position = new Vector3(positionData.x, positionData.y, positionData.z);
                            clientObject.transform.rotation = new Quaternion(positionData.rotX, positionData.rotY, positionData.rotZ, positionData.rotW);
                        }
                    });

                    // Transmitir posición a otros clientes
                    mainThreadActions.Enqueue(() => BroadcastPosition(positionData, remoteClient));
                }
                else
                {
                    Debug.LogWarning($"Datos de posición malformados recibidos de {remoteClient}");
                }
            }
            else
            {
                // Otros tipos de mensajes
                messageQueue.Enqueue(receivedMessage);
                mainThreadActions.Enqueue(() => BroadcastMessage(receivedMessage, remoteClient));
            }
        }
    }

    private void SendPossitionBetweenClients()
    {
        foreach (var senderClient in clientPlayerInstances.Keys)
        {
            if (clientPlayerInstances[senderClient] != null)
            {
                var clientObject = clientPlayerInstances[senderClient];
                Vector3 position = clientObject.transform.position;
                Quaternion rotation = clientObject.transform.rotation;

                Position positionData = new Position(position.x, position.y, position.z, rotation);
                string serializedPosition = "POS:" + Position.Serialize(positionData);
                byte[] data = Encoding.ASCII.GetBytes(serializedPosition);

                foreach (var receiverClient in connectedClients)
                {
                    if (!receiverClient.Equals(senderClient))
                    {
                        socket.SendTo(data, receiverClient);
                    }
                }
            }
        }
    }

    void AddClient(EndPoint clientEndpoint)
    {
        mainThreadActions.Enqueue(() =>
        {
            if (serverObject != null && !clientPlayerInstances.ContainsKey(clientEndpoint))
            {
                // Instanciar un nuevo objeto para el cliente
                var newClientInstance = Instantiate(serverObject, new Vector3(0, 1, 0), Quaternion.identity);
                clientPlayerInstances[clientEndpoint] = newClientInstance;

                // Transmitir información de cliente (ej. nombre e ID únicos);
                BroadcastName("NAME" + PlayerPrefs.GetString("Name_Player"));

                string newClientMessage = $"NEWCLIENT:{clientEndpoint.ToString()}";
                BroadcastMessage(newClientMessage, clientEndpoint);

                BroadcastAllClientsData(clientEndpoint);

                Debug.Log($"Nuevo cliente añadido: {clientEndpoint}");
            }
        });     
    }

    void BroadcastServerPosition()
    {
        if (dynamicServerObject == null)
        {
            Debug.LogWarning("dynamicServerObject no está asignado; no se puede transmitir la posición.");
            return;
        }

        Vector3 position = dynamicServerObject.transform.position;
        Quaternion rotation = dynamicServerObject.transform.rotation;
        Position serverData = new Position(position.x, position.y, position.z, rotation);

        string serializedData = "POS:" + Position.Serialize(serverData);
        byte[] buffer = Encoding.ASCII.GetBytes(serializedData);

        foreach (var client in connectedClients)
        {
            socket.SendTo(buffer, client);
        }
    }
    void BroadcastName(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        foreach (var client in connectedClients)
        {
            
            socket.SendTo(data, client);
            
        }
    }
    void BroadcastPosition(Position position, EndPoint sender)
    {
        string serializedPosition = "POS:" + Position.Serialize(position);
        byte[] data = Encoding.ASCII.GetBytes(serializedPosition);

        foreach (var client in connectedClients)
        {
            if (!client.Equals(sender)) // No enviar al remitente
            {
                socket.SendTo(data, client);
            }
        }
    }
    //TODO: Analizar todo esto para enviar los datos a todos los clientes
    void BroadcastAllClientsData(EndPoint requestingClient)
    {
        foreach (var clientEndpoint in clientPlayerInstances.Keys)
        {
            if (clientPlayerInstances[clientEndpoint] != null)
            {
                var clientObject = clientPlayerInstances[clientEndpoint];
                Vector3 position = clientObject.transform.position;
                Quaternion rotation = clientObject.transform.rotation;

                Position positionData = new Position(position.x, position.y, position.z, rotation);
                string serializedPosition = "POS:" + Position.Serialize(positionData);

                byte[] data = Encoding.ASCII.GetBytes(serializedPosition);
                socket.SendTo(data, requestingClient);
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
