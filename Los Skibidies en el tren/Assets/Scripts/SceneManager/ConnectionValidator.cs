using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectionValidator : MonoBehaviour
{
    public Text serverIP; // IP configurada desde la UI
    private int serverPort = 9050; // Puerto fijo para la conexión

    public bool can_join;
    // Configura la IP del servidor antes de conectarte

    public void SetServerIP(Text ip)
    {
        serverIP = ip;
    }
    // Llamado al pulsar el botón de conectar
    public void ValidateConnection()
    {
        if (string.IsNullOrEmpty(serverIP.text))
        {
            Debug.LogWarning("IP no configurada. Introduce una IP válida.");
            return;
        }

        using (Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP.text), serverPort);

            try
            {
                // Enviar solicitud de conexión
                byte[] data = Encoding.ASCII.GetBytes("CAN_JOIN");
                clientSocket.SendTo(data, serverEndPoint);

                // Recibir respuesta del servidor
                byte[] responseBuffer = new byte[1024];
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                clientSocket.ReceiveTimeout = 3000; // Tiempo de espera de 3 segundos
                int received = clientSocket.ReceiveFrom(responseBuffer, ref remoteEndPoint);

                string response = Encoding.ASCII.GetString(responseBuffer, 0, received).TrimEnd('\0');
                if (response == "OK")
                {
                    can_join = true;
                }
                else
                {
                    can_join = false;
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError($"Error al conectar al servidor: {ex.Message}");
                Debug.Log("Cannot join");
            }
        }
    }

}
