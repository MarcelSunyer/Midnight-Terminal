using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBarToZero : MonoBehaviour
{
    private Progress_bar progress_Bar;
    // Start is called before the first frame update
    void Start()
    {
        progress_Bar.act = 0;
    }
}
