using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Progress_bar : MonoBehaviour
{
    Slider Bar;

    public float act;
    public TMP_Text percentageText;
    private void Awake()
    {
        Bar = GetComponent<Slider>();
    }
    void Update()
    {
        UpdateProgressBar(100, act);
    }
    private void UpdateProgressBar(float valMax, float valAct)
    {
        float percentage;
        percentage = valAct / valMax;
        Bar.value = percentage;
        percentageText.text = percentage * 100 + "%";

    }

    public void SetValue(float value)
    {
        act = value; // Ajusta según tu lógica interna
    }

}
