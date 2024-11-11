using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class Create_Server_Info : MonoBehaviour
{
    [SerializeField] public InputField Cs_Name;

    [SerializeField] public InputField Cs_Server;

    public void SaveInputTexts()
    {

        PlayerPrefs.SetString("Name_Player", Cs_Name.text);

        PlayerPrefs.SetString("Server_Name", Cs_Server.text);

        PlayerPrefs.Save();
    }

}
