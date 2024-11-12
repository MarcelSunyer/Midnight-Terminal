using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Info_Join_Server : MonoBehaviour
{
    private TMP_Text Js_name;
    private TMP_Text Js_Ip;

    private void Start()
    {
        // Buscar y asignar los componentes TMP_Text en la jerarquía
        Js_name = GameObject.Find("Js_name").GetComponent<TMP_Text>();
        Js_Ip = GameObject.Find("Js_Ip").GetComponent<TMP_Text>();

        Js_name.text = PlayerPrefs.HasKey("Name_Player") ? PlayerPrefs.GetString("Name_Player") : "No hay texto guardado";
        Js_Ip.text = PlayerPrefs.HasKey("LocalIPAddress_create_server") ? PlayerPrefs.GetString("LocalIPAddress_create_server") : "No hay texto guardado";

    }
}
