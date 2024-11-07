using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Info_Creat_Server : MonoBehaviour
{
    [SerializeField] private TMP_Text Cs_Name;
    [SerializeField] private TMP_Text Cs_Server;
    [SerializeField] private TMP_Text Cs_Ip;

    private void Start()
    {
        // Verifica y asigna el texto
        if (PlayerPrefs.HasKey("Name_Player"))
        {
            Cs_Name.text = PlayerPrefs.GetString("Name_Player");
        }
        else
        {
            Cs_Name.text = "No hay texto guardado";
        }

        if (PlayerPrefs.HasKey("Server_Name"))
        {
            Cs_Server.text = PlayerPrefs.GetString("Server_Name");
        }
        else
        {
            Cs_Server.text = "No hay texto guardado";
        }
        if (PlayerPrefs.HasKey("LocalIPAddress_create_server"))
        {
            Cs_Ip.text = PlayerPrefs.GetString("LocalIPAddress_create_server");
        }
        else
        {
            Cs_Ip.text = "No hay texto guardado";
        }
    }
}
