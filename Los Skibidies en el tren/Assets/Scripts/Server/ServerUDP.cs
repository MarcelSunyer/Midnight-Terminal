using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;

public class ServerUDP : MonoBehaviour
{
    Socket socket;
    string serverText;

    private Message_chat message_Chat;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>(); // Cola para mensajes

    void Start()
    {
        // Obtener referencia al componente Message_chat
        message_Chat = FindObjectOfType<Message_chat>();

        if (message_Chat != null)
        {
            message_Chat.SendMessageToChat("Starting server...", Message.MessageType.info);
        }
        else
        {
            Debug.LogError("No se encontró el componente Message_chat.");
        }

        // Crear y vincular el socket
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        // Iniciar el hilo para recibir mensajes
        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }

    void Update()
    {
        // Procesar los mensajes de la cola en el hilo principal
        while (messageQueue.TryDequeue(out string message))
        {
            if (message_Chat != null)
            {
                message_Chat.SendMessageToChat(message, Message.MessageType.info);
            }
        }
    }

    void Receive()
    {
        int recv;
        byte[] data = new byte[1024];

        if (message_Chat != null)
        {
            messageQueue.Enqueue("Waiting for new Client...");
        }

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)sender;

        while (true)
        {
            // Recibir mensaje
            recv = socket.ReceiveFrom(data, ref Remote);
            string receivedMessage = $"Message received from {Remote.ToString()}: " + Encoding.ASCII.GetString(data, 0, recv);

            // Agregar el mensaje a la cola
            messageQueue.Enqueue(receivedMessage);

            // Enviar ping de respuesta en un nuevo hilo
            Thread sendThread = new Thread(() => Send(Remote));
            sendThread.Start();
        }
    }

    void Send(EndPoint Remote)
    {
        string welcome = "Ping";
        byte[] data = Encoding.ASCII.GetBytes(welcome);

        // Enviar el mensaje de ping al cliente
        socket.SendTo(data, data.Length, SocketFlags.None, Remote);

        // Agregar mensaje de envío a la cola
        messageQueue.Enqueue($"Sent to {Remote.ToString()}: {welcome}");
    }
}