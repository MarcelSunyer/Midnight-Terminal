using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Info_Join_Server : MonoBehaviour
{
    [SerializeField] private TMP_Text Js_name;
    [SerializeField] private TMP_Text Js_Ip;

    private void Start()
    {
        // Verifica y asigna el texto
        if (PlayerPrefs.HasKey("Join_Server_Name"))
        {
            Js_name.text = PlayerPrefs.GetString("Join_Server_Name");
        }
        else
        {
            Js_name.text = "No hay texto guardado";
        }

        if (PlayerPrefs.HasKey("Join_Server_IP"))
        {
            Js_Ip.text = PlayerPrefs.GetString("Join_Server_IP");
        }
        else
        {
            Js_Ip.text = "No hay texto guardado";
        }

    }
}
