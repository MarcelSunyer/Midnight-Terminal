using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Linq;
using UnityEngine.UIElements;

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

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Dictionary<string, GameObject> clientInstances = new Dictionary<string, GameObject>();
    public GameObject clientPrefab; 

    public GameObject serverRepresentationPrefab;
    private GameObject serverInstance;

    private bool shouldTeleport = false;

    private Clean_Debris clean_Debris;
    bool isSceneLoaded;
    bool isTrainLoaded = false;
    bool isDebrisFound = false;

    bool destoryDebris;

    private float heartbeatInterval = 2f;
    private float nextHeartbeatTime;

    public Progress_bar progressBar;
    private GameObject minijuegos;
    private float lastSentProgress; // Inicializa con un valor que no sea válido

    private GameObject endgame;
    void Start()
    {
        
        if (serverRepresentationPrefab != null && serverInstance == null)
        {
            serverInstance = Instantiate(serverRepresentationPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
        DontDestroyOnLoad(serverInstance);
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
        DontDestroyOnLoad(serverInstance);
        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();
        lastSentProgress = progressBar.act;

    }

    void Update()
    {
        SendProgressToTheServer(progressBar.act);

        minijuegos = GameObject.Find("-----Minigames-----");

        if (isSceneLoaded = SceneManager.GetSceneByName("TrainStation_Level").isLoaded && clean_Debris == null && isTrainLoaded == false)
        {
            clean_Debris = FindObjectOfType<Clean_Debris>();
            isDebrisFound = true;
            isTrainLoaded = true;
            if (minijuegos != null)
            {
                endgame = GameObject.Find("Go_Home");
                // Asegúrate de que tiene al menos 3 hijos
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
        }
        if (clean_Debris == null && isDebrisFound)
        {
            DebrisDestroyed();

        }

        if (progressBar.act <= 100)
        {
            endgame.SetActive(true);
        }

        if (shouldTeleport)
        {
            shouldTeleport = false;

            foreach (var client in clientInstances.Values)
            {
                if (client != null)
                {
                    Debug.Log($"Making client indestructible: {client.name}");
                    DontDestroyOnLoad(client);
                }
                else
                {
                    Debug.LogWarning("A client instance was null and couldn't be made indestructible.");
                }
            }

            // Haz indestructible el servidor
            if (serverInstance != null)
            {
                Debug.Log("Making server instance indestructible.");
                DontDestroyOnLoad(serverInstance);
            }
            SceneManager.LoadScene("TrainStation_Level");
        }
    
        while (mainThreadTasks.Count > 0)
        {
            var action = mainThreadTasks.Dequeue();
            if (action != null)
            {
                action.Invoke();
            }
            else
            {
                Debug.LogWarning("Se encontró una acción nula en mainThreadTasks.");
            }
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

         if(destoryDebris)
            {
            GameObject obj = GameObject.Find("Escombros_parent");
            if (obj != null)
            {
                Destroy(obj);
                Debug.Log($"fue destruido.");
            }
            else
            {
                Debug.LogWarning($"No se encontró el objeto con nombre .");
            }
            destoryDebris = false;
        }
        SendPlayerPosition();
        if (Time.time >= nextHeartbeatTime)
        {
            SendHeartbeat();
            nextHeartbeatTime = Time.time + heartbeatInterval;
        }
    }

    void SendPlayerPosition()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        // Enviar solo si hay cambios significativos
        if (Vector3.Distance(lastPosition, currentPosition) > 0.01f || Quaternion.Angle(lastRotation, currentRotation) > 0.1f)
        {
            Position data = new Position(currentPosition.x, currentPosition.y, currentPosition.z, currentRotation);
            string serializedData = "POS:" + Position.Serialize(data);
            SendMessageToServer(serializedData);

            lastPosition = currentPosition;
            lastRotation = currentRotation;
        }
    }
    void SendHeartbeat()
    {
        SendMessageToServer("HEARTBEAT:");
    }
    void Receive()
    {
        byte[] data = new byte[1024];
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            int recv = socket.ReceiveFrom(data, ref remoteEndPoint);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            if (receivedMessage.StartsWith("NEWCLIENT:"))
            {
                HandleNewClient(receivedMessage.Substring(10));
            }
            else if (receivedMessage.StartsWith("UPDATE_PROGRESS:"))
            {
                string progressValue = receivedMessage.Substring(16);
                 Debug.Log(progressValue.ToString());
                mainThreadTasks.Enqueue(() => UpdateProgressBar(progressValue));
            }
            else if (receivedMessage.StartsWith("POSCIENTS:"))
            {
                HandlePositionUpdate(receivedMessage.Substring(10));
            }
            else if (receivedMessage.StartsWith("POS:"))
            {
                string positionDataStr = receivedMessage.Substring(4);
                Position positionData = Position.Deserialize(positionDataStr);

                mainThreadTasks.Enqueue(() =>
                {
                    UpdateServerPosition(positionData);
                });
            }
            else if (receivedMessage.StartsWith("GAMESTART:"))
            {
                TeleportPrefabs();
            }
            else if (receivedMessage.StartsWith("DEBRISDESTROYED:"))
            {
                if (clean_Debris != null)
                {
                    DestroyDebris();
                }
            }
            else if (receivedMessage.StartsWith("NAME"))
            {
                string nameServer = receivedMessage.Substring(4);

                mainThreadTasks.Enqueue(() =>
                {
                    UpdateServerName(nameServer);
                });
            }
            else
            {
                mainThreadTasks.Enqueue(() =>
                {
                    SendMessageToChat(receivedMessage, Message.MessageType.info);
                });
            }
             
        }
    }
    void DestroyDebris()
    {
        destoryDebris = true;
    }

    void UpdateProgressBar(string value)
    {

        if (int.TryParse(value, out int progressInt))
        {
            if (progressBar != null) // Asegúrate de tener una referencia al script de la barra
            {
                progressBar.act = progressInt;
            }
        }
    }
    void HandleNewClient(string clientInfo)
    {
        string[] parts = clientInfo.Split(':');
        if (parts.Length == 3)
        {
            int clientID = int.Parse(parts[0]);
            string clientEndpoint = $"{parts[1]}:{parts[2]}";

            if (clientID >= 1)
            {
                mainThreadTasks.Enqueue(() =>
                {
                    for (int id = 1; id <= clientID; id++)
                    {
                        string instanceKey = $"{clientEndpoint}_ID{id}";

                        if (!clientInstances.ContainsKey(instanceKey))
                        {
                            GameObject newClientInstance = Instantiate(clientPrefab, Vector3.zero, Quaternion.identity);
                            Debug.Log("Skibidis en el toilet");
                            clientInstances[instanceKey] = newClientInstance;
                            DontDestroyOnLoad(clientInstances[instanceKey]);
                            Debug.Log($"Cliente añadido: ID={id}, Endpoint={clientEndpoint}");
                        }
                    }
                });
            }
        }
    }

  void HandlePositionUpdate(string positionDataStr)
{
    try
    {
        int secondColonIndex = positionDataStr.IndexOf(':');
        string positionJson = positionDataStr.Substring(secondColonIndex + 1);
        Position positionData = Position.Deserialize(positionJson);

        mainThreadTasks.Enqueue(() =>
        {
            foreach (var key in clientInstances.Keys)
            {
                if (key.Contains($"_ID{positionData.id}")) // Busca una coincidencia parcial con el ID
                {
                    if (clientInstances.TryGetValue(key, out GameObject clientObject))
                    {
                        clientObject.transform.position = new Vector3(positionData.x, positionData.y, positionData.z);
                        clientObject.transform.rotation = new Quaternion(positionData.rotX, positionData.rotY, positionData.rotZ, positionData.rotW);
                        return;
                    }
                }
            }

            Debug.LogWarning($"No se encontró el cliente con ID: {positionData.id}");
        });
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error al procesar mensaje POSCIENTS: {ex.Message}");
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
        // Instancia el servidor si aún no se ha creado
        if (serverInstance == null && serverRepresentationPrefab != null)
        {
            serverInstance = Instantiate(serverRepresentationPrefab, new Vector3(data.x, data.y, data.z), Quaternion.identity);
            Debug.Log("Instancia del servidor creada en el cliente.");
        }

        // Actualiza la posición y rotación del servidor
        if (serverInstance != null)
        {
            serverInstance.transform.position = new Vector3(data.x, data.y, data.z);
            serverInstance.transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
        }
        else
        {
            Debug.LogWarning("No se pudo actualizar la posición porque el servidor no tiene una instancia.");
        }
    }


    void SendMessageToServer(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, serverEndPoint);
    }

    void DebrisDestroyed()
    {
        byte[] data = Encoding.ASCII.GetBytes("DEBRISDESTROYED:");
        socket.SendTo(data, serverEndPoint);        
        isDebrisFound = true;

    }
    void SendProgressToTheServer(float newValue)
    {
        if (lastSentProgress != newValue)
        {
            lastSentProgress = newValue; // Actualiza el valor almacenado
            SendProgressBarValue();
        }
    }
    void SendProgressBarValue()
    {
        // Comprueba si el progreso cambió significativamente (tolerancia del 1%)

        string message = "PROGRESS: " + progressBar.act;
        SendMessageToServer(message);

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
    private void TeleportPrefabs()
    {
        shouldTeleport = true; // Marca la operación como pendiente
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
