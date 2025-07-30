using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public TMP_Text healthText;
    public static int healthPercent;

    private void Start()
    {
        healthPercent = 100;
    }
    private void Update()
    {
        if (healthPercent > -1)
        {
            healthText.text = healthPercent + "%";
        }
    }
}
