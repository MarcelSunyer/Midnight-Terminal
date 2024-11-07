using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClientTCP : MonoBehaviour
{
    public GameObject UItextObj;
    private TextMeshProUGUI UItext;
    private string clientText;

    private Socket server;

    public InputField chatBox; // Campo de entrada para el mensaje usando TMP_InputField
    public GameObject chatPanel;
    public GameObject textObject;
    public ScrollRect scrollRect; // Referencia al ScrollRect para desplazar automáticamente
    public int maxMessages = 60;

    private List<Message_Client> messageList = new List<Message_Client>();

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // Actualizar el texto del UI con los mensajes recibidos
        UItext.text = clientText;

        // Enviar mensaje al servidor cuando el InputField tiene texto y se presiona Enter
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // Enviar el mensaje al servidor
                SendMessageToServer(chatBox.text);
                chatBox.text = ""; // Limpiar el InputField después de enviar el mensaje
            }
        }
        else
        {
            // Activar el InputField si está vacío y Enter es presionado
            if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.ActivateInputField();
            }
        }
    }

    public void StartClient()
    {
        Thread connect = new Thread(Connect);
        connect.Start();
    }

    void Connect()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(PlayerPrefs.GetString("Join_Server_IP")), 9050);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Connect(ipep);

        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();
    }

    // Método para enviar un mensaje al servidor
    public void SendMessageToServer(string text)
    {
        if (server != null && server.Connected)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(PlayerPrefs.GetString("Join_Server_Name") + ": " + text);
                server.Send(data);
                clientText += "\n" + text;

                // Mostrar el mensaje enviado en el chat local
                SendMessageToChat(PlayerPrefs.GetString("Join_Server_Name") + ": " + text);
            }
            catch (SocketException ex)
            {
                Debug.Log($"Error al enviar mensaje al servidor: {ex.Message}");
            }
        }
    }

    // Método para mostrar el mensaje en el chat local
    public void SendMessageToChat(string text)
    {
        if (messageList.Count > maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
        }

        Message_Client newMessage = new Message_Client();
        newMessage.text = text;
       
        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text;
        
        messageList.Add(newMessage);
    }

    void Receive()
    {
        byte[] data = new byte[1024];

        while (server != null && server.Connected)
        {
            try
            {
                int recv = server.Receive(data);
                if (recv > 0)
                {
                    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);
                    clientText += "\n" + receivedMessage;

                    // Mostrar mensaje recibido en el chat local
                    SendMessageToChat(receivedMessage);
                }
            }
            catch (SocketException ex)
            {
                Debug.Log($"Error receiving data: {ex.Message}");
                break; // Salir del bucle si hay un error grave
            }
        }

        server.Close(); // Cerrar la conexión cuando el bucle termine
    }

    // Método llamado al hacer clic en el botón de enviar
    public void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(chatBox.text))
        {
            Debug.Log(chatBox.text);
            SendMessageToServer(chatBox.text);
            chatBox.text = ""; // Limpiar el InputField después de enviar el mensaje
        }
    }
}

[System.Serializable]
public class Message_Client
{
    public string text;
    public Text textObject;
}