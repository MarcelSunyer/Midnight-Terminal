using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pantalla_decarga : MonoBehaviour
{
    public Canvas pantalla;
    public float timer = 3;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            pantalla.enabled = false;
            timer = 0;
        }
    }
}
