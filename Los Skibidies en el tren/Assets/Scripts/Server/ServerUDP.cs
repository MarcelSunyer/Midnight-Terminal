using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

public class ServerUDP : MonoBehaviour
{
    Socket socket;
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    public void startServer()
    {
        serverText = "Starting UDP Server...";

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
        UItext.text = serverText;
    }

    void Receive()
    {
        int recv;
        byte[] data = new byte[1024];

        serverText += "\nWaiting for new Client...";

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)sender;

        while (true)
        {
            // Recibir mensaje
            recv = socket.ReceiveFrom(data, ref Remote);
            serverText += $"\nMessage received from {Remote.ToString()}: " + Encoding.ASCII.GetString(data, 0, recv);

            // Enviar ping de respuesta
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
        serverText += $"\nSent to {Remote.ToString()}: {welcome}";
    }
}
