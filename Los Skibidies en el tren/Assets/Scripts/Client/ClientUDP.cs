using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class ClientUDP : MonoBehaviour
{
    private Socket socket;
    private Message_chat message_Chat;

    private Queue<System.Action> mainThreadTasks = new Queue<System.Action>(); // Cola para tareas del hilo principal

    void Start()
    {
        message_Chat = FindObjectOfType<Message_chat>();

        if (message_Chat != null)
        {
            message_Chat.SendMessageToChat("Client initialized...", Message.MessageType.info);
        }
        else
        {
            Debug.LogError("No se encontró el componente Message_chat.");
        }
        Thread mainThread = new Thread(SendInitialMessage);
        mainThread.Start();
    }

    void Update()
    {
        // Ejecuta las tareas de la cola en el hilo principal
        while (mainThreadTasks.Count > 0)
        {
            mainThreadTasks.Dequeue().Invoke();
        }
    }

    void SendInitialMessage()
    {
        // Crear el endpoint utilizando la IP guardada en PlayerPrefs
        string serverIP = null;

        // Agregar la tarea de obtener la IP a la cola para que se ejecute en el hilo principal
        mainThreadTasks.Enqueue(() =>
        {
            serverIP = PlayerPrefs.GetString("Join_Server_IP", "127.0.0.1"); // Default to local IP if no value found
        });

        // Esperar a que el valor se obtenga
        while (serverIP == null) { }

        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIP), 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        string initialMessage = "Hello from client";
        byte[] data = Encoding.ASCII.GetBytes(initialMessage);
        socket.SendTo(data, data.Length, SocketFlags.None, ipep);

        message_Chat?.SendMessageToChat("Skibidy Toilet", Message.MessageType.playerMessage);

        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();
    }

    void Receive()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)sender;
        byte[] data = new byte[1024];

        while (true)
        {
            int recv = socket.ReceiveFrom(data, ref Remote);
            string receivedText = $"Message from server: " + Encoding.ASCII.GetString(data, 0, recv);
            message_Chat?.SendMessageToChat(receivedText, Message.MessageType.info);
        }
    }
}