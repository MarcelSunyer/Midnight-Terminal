using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class Join_server_info : MonoBehaviour
{
    [SerializeField] private InputField Js_name;

    [SerializeField] private InputField Js_IP;

    public void SaveInputTexts()
    {

        PlayerPrefs.SetString("Join_Server_Name", Js_name.text);

        PlayerPrefs.SetString("Join_Server_IP", Js_IP.text);

        PlayerPrefs.Save();

        UnityEngine.Debug.Log(Js_name.text);
    }

}
