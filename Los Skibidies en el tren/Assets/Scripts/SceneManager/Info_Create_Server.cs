using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Info_Creat_Server : MonoBehaviour
{
    private TMP_Text Cs_Name;
    private TMP_Text Cs_Server;
    private TMP_Text Cs_Ip;

    private void Start()
    {
        // Buscar y asignar los componentes TMP_Text en la jerarquía
        Cs_Name = GameObject.Find("Cs_Name").GetComponent<TMP_Text>();
        Cs_Server = GameObject.Find("Cs_Server").GetComponent<TMP_Text>();
        Cs_Ip = GameObject.Find("Cs_Ip").GetComponent<TMP_Text>();

        Cs_Name.text = PlayerPrefs.HasKey("Name_Player") ? PlayerPrefs.GetString("Name_Player") : "No hay texto guardado";
        Cs_Server.text = PlayerPrefs.HasKey("Server_Name") ? PlayerPrefs.GetString("Server_Name") : "No hay texto guardado";
        Cs_Ip.text = PlayerPrefs.HasKey("LocalIPAddress_create_server") ? PlayerPrefs.GetString("LocalIPAddress_create_server") : "No hay texto guardado";
    }
}
