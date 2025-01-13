using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Collections.Concurrent;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using Unity.VisualScripting;

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

    private Dictionary<EndPoint, float> clientLastHeartbeat = new Dictionary<EndPoint, float>();
    private const float heartbeatTimeout = 5f;


    private int clientIDCounter = 0;
    private Dictionary<EndPoint, int> clientIDs = new Dictionary<EndPoint, int>();


    private StartGame_Button interactionManager;
    private GameObject interactionObject;

    private Clean_Debris clean_Debris;
    bool isSceneLoaded;
    bool isDebrisFound = false;
    bool can_be_destroyed;

    public Progress_bar progressBar;

    private float _lastSentProgress = 0;

    int can_join = 0;

    private GameObject minijuegos;

    bool sceneTrain = false;
    void Start()
    {
        progressBar.act = 0;
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

        interactionObject = GameObject.Find("Boton");

    }

    void Update()
    {
        SendProgressToTheClient(progressBar.act);
        Debug.Log(progressBar.act.ToString());
        if (isSceneLoaded = SceneManager.GetSceneByName("TrainStation_Level").isLoaded && clean_Debris == null)
        {
            if (sceneTrain == false)
            {
                sceneTrain = true;
                minijuegos = GameObject.Find("-----Minigames-----");
                int childCount = minijuegos.transform.childCount;
                if (childCount >= 3)
                {
                    // Elige un índice aleatorio para el hijo que permanecerá activo
                    int activeIndex = UnityEngine.Random.Range(0, childCount);

                    for (int i = 0; i < childCount; i++)
                    {
                        GameObject child = minijuegos.transform.GetChild(i).gameObject;
                        // Desactiva todos los hijos excepto el seleccionado
                        child.SetActive(i == activeIndex);
                    }

                    Debug.Log($"Hijo activo: {minijuegos.transform.GetChild(activeIndex).name}");
                }
            }
            clean_Debris = FindObjectOfType<Clean_Debris>();
            isDebrisFound = true;


        }
        if (clean_Debris == null && isDebrisFound)
        {
            DebrisDestroyed();
        }
        if(can_be_destroyed)
        {
            clean_Debris.DestroyDebris();
            can_be_destroyed = false;
        }
        if (interactionObject != null)
        {
            interactionManager = interactionObject.GetComponent<StartGame_Button>();
            if (interactionManager != null)
            {
                interactionManager.OnSceneLoaded += HandleSceneLoaded;
            }
        }

        CheckForHeartbeatTimeouts();
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
    }

    void SendProgressToTheClient(float newValue)
    {
        if (_lastSentProgress != newValue)
        {
            _lastSentProgress = newValue; // Actualiza el valor almacenado
            BroadcastProgressBarValueToClient();
        }
    }
    void CheckForHeartbeatTimeouts()
    {
        float currentTime = Time.time;

        foreach (var client in clientLastHeartbeat.Keys.ToArray())
        {
            if (currentTime - clientLastHeartbeat[client] > heartbeatTimeout)
            {
                Debug.Log($"Cliente inactivo detectado: {client}. Eliminando...");
                RemoveClient(client);
            }
        }
    }
    void RemoveClient(EndPoint clientEndpoint)
    {
        if (connectedClients.Remove(clientEndpoint))
        {
            if (clientPlayerInstances.TryGetValue(clientEndpoint, out GameObject clientPrefab))
            {
                // Encolar la acción para el hilo principal
                mainThreadActions.Enqueue(() =>
                {
                    Destroy(clientPrefab);
                    clientPlayerInstances.Remove(clientEndpoint);
                });
            }

            if (clientIDs.TryGetValue(clientEndpoint, out int clientID))
            {
                // Informar a los demás clientes sobre el cliente desconectado
                string disconnectMessage = $"DISCONNECTED:{clientID}";
                byte[] data = Encoding.ASCII.GetBytes(disconnectMessage);

                // Iterar sobre los clientes conectados y enviar el mensaje
                foreach (var client in connectedClients)
                {
                    socket.SendTo(data, client);
                }

                clientIDs.Remove(clientEndpoint);
            }

            clientLastHeartbeat.Remove(clientEndpoint);
            Debug.Log($"Cliente desconectado: {clientEndpoint}");
        }
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        EndPoint remoteClient = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                int recv = socket.ReceiveFrom(data, ref remoteClient);
                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                if (receivedMessage == "CAN_JOIN")
                {
                    string response = can_join < 2 ? "OK" : "FULL";
                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                    socket.SendTo(responseData, remoteClient);
                }
                if (receivedMessage.StartsWith("PROGRESS: "))
                {
                    string progressValue = receivedMessage.Substring(9); // Obtén el valor después de "PROGRESS:"
                    BroadcastProgressBarValue(progressValue, remoteClient);
                }
                else if (receivedMessage.StartsWith("HEARTBEAT:"))
                {
                    if (!clientLastHeartbeat.ContainsKey(remoteClient))
                    {
                        Debug.LogWarning($"HEARTBEAT recibido de cliente no registrado: {remoteClient}");
                        continue;
                    }

                    clientLastHeartbeat[remoteClient] = Time.time;
                }
                if (!connectedClients.Contains(remoteClient))
                {
                    if (can_join <= 2)
                    {
                        connectedClients.Add(remoteClient);
                        mainThreadActions.Enqueue(() => AddClient(remoteClient));
                        can_join += 1;
                    }
                    else
                    {
                        Debug.LogWarning("Cliente intentó conectarse, pero el servidor está lleno.");
                    }
                }
                else if (receivedMessage.StartsWith("DEBRISDESTROYED:"))
                {
                    can_be_destroyed = true;
                    DebrisDestroyed();
                }
                else if (receivedMessage.StartsWith("POS:"))
                {
                    string positionDataStr = receivedMessage.Substring(4);
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

                        mainThreadActions.Enqueue(() => BroadcastPosition(positionData, remoteClient));
                    }
                    else
                    {
                        Debug.LogWarning($"Datos de posición malformados recibidos de {remoteClient}");
                    }
                }
                else
                {
                    messageQueue.Enqueue(receivedMessage);
                    mainThreadActions.Enqueue(() => BroadcastMessage(receivedMessage, remoteClient));
                }
            }
            catch (SocketException ex)
            {
                HandleClientDisconnection(remoteClient); // Maneja la desconexión
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error general en Receive: {ex.Message}");
            }
        }
    }
    void HandleClientDisconnection(EndPoint remoteClient)
    {
        if (connectedClients.Contains(remoteClient))
        {
            Debug.Log($"El cliente {remoteClient} se ha desconectado abruptamente.");
            RemoveClient(remoteClient); // Elimina el cliente desconectado
        }
    }
    void BroadcastProgressBarValueToClient()
    {
        // Construye el mensaje para enviar a los demás clientes
        string message = "UPDATE_PROGRESS:" + progressBar.act.ToString();

        byte[] data = Encoding.ASCII.GetBytes(message);

        foreach (var client in connectedClients)
        {
            socket.SendTo(data, client);
        }
    }
    void BroadcastProgressBarValue(string progressValue, EndPoint sender)
    {
        Debug.Log(progressValue);
        // Convierte progressValue a entero antes de usarlo
        if (int.TryParse(progressValue, out int progressInt))
        {
            // Actualiza el valor de la barra de progreso en el servidor
            progressBar.act = progressInt;
            _lastSentProgress = progressBar.act;
            // Construye el mensaje para enviar a los demás clientes
            string message = "UPDATE_PROGRESS:" + progressBar.act.ToString();

            byte[] data = Encoding.ASCII.GetBytes(message);

            foreach (var client in connectedClients)
            {
                // Envía a todos los clientes excepto al remitente
                if (!client.Equals(sender))
                {
                    socket.SendTo(data, client);
                }
            }
        }
        else
        {
            Debug.LogWarning($"No se pudo convertir progressValue a int: {progressValue}");
        }
    }
    void DebrisDestroyed()
    {
        string serializedData = "DEBRISDESTROYED:";
        byte[] buffer = Encoding.ASCII.GetBytes(serializedData);

        foreach (var client in connectedClients)
        {
            socket.SendTo(buffer, client);
        }
        isDebrisFound = true;

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
    void AddClient(EndPoint clientEndpoint)
    {
        mainThreadActions.Enqueue(() =>
        {
            if (can_join <= 2)
            {
                if (serverObject != null && !clientPlayerInstances.ContainsKey(clientEndpoint))
                {
                    // Asignar un ID único al cliente
                    int newClientID = clientIDCounter++;
                    clientIDs[clientEndpoint] = newClientID;

                    // Instanciar un nuevo objeto para el cliente
                    var newClientInstance = Instantiate(serverObject, new Vector3(0, 1, 0), Quaternion.identity);
                    clientPlayerInstances[clientEndpoint] = newClientInstance;

                    // Transmitir el ID del cliente al cliente respectivo

                    string newClientMessage = $"NEWCLIENT:{newClientID}:{clientEndpoint}";

                    byte[] buffer = Encoding.ASCII.GetBytes(newClientMessage);

                    foreach (var client in connectedClients)
                    {
                        socket.SendTo(buffer, client);
                    }
                    // Enviar información de otros clientes a este nuevo cliente
                    //BroadcastAllClientsData(clientEndpoint);

                    Debug.Log($"Nuevo cliente añadido: {clientEndpoint} con ID: {newClientID}");
                }
            }
        });
    }
    void BroadcastPosition(Position position, EndPoint sender)
    {
        if (!clientIDs.TryGetValue(sender, out int senderID))
        {
            Debug.LogWarning($"Cliente no registrado para EndPoint: {sender}. No se puede transmitir la posición.");
            return;
        }

        string serializedPosition = $"POSCIENTS:{senderID}:{Position.Serialize(position)}";
        byte[] data = Encoding.ASCII.GetBytes(serializedPosition);

        foreach (var client in connectedClients)
        {
            // Comparación explícita de EndPoint: IP y puerto
            if (!client.ToString().Equals(sender.ToString()))
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
    private void HandleSceneLoaded()
    {
        Debug.Log("La escena ha sido cargada. Notificando a los clientes...");

        string serializedData = "GAMESTART:La escena ha comenzado.";
        byte[] buffer = Encoding.ASCII.GetBytes(serializedData);

        foreach (var client in connectedClients)
        {
            socket.SendTo(buffer, client);
        }
    }
}
