using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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

    public void CopyLocalIP()
    {
        string copyIP = GetLocalIPAddress();
        Js_IP.text = copyIP;
    }

    string GetLocalIPAddress()
    {
        string localIP = "";

        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
        }
        catch (System.Exception ex)
        {
           
        }

        return localIP;
    }

}
