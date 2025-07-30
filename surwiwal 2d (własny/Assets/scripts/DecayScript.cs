using System.Collections;
using UnityEngine;
using TMPro;

public class DecayScript : MonoBehaviour
{
    public int decayPercent; // Procent g³odu/nawodnienia
    public int decayAmount; // Iloœæ spadku
    public float decayTime; // Jak czêsto statystyki spadaj¹
    public TMP_Text decayText; // Tekst do wyœwietlania procentu
    private Coroutine decayCoroutine; // Referencja do coroutine

    private void Start()
    {
        decayCoroutine = StartCoroutine(Decay()); // Rozpocznij proces dekay
    }

    private void Update()
    {
        decayText.text = decayPercent + "%"; // Aktualizuj tekst

        // SprawdŸ, czy którykolwiek z paneli UI jest aktywny
        if (UIManager.Instance.IsAnyUIPanelActive())
        {
            if (decayCoroutine != null)
            {
                StopCoroutine(decayCoroutine); // Zatrzymaj proces dekay
                decayCoroutine = null; // Ustaw referencjê na null
            }
        }
        else
        {
            if (decayCoroutine == null)
            {
                decayCoroutine = StartCoroutine(Decay()); // Uruchom ponownie proces dekay
            }
        }
    }

    private IEnumerator Decay()
    {
        while (true) // Pêtla nieskoñczona, kontrola odbywa siê w Update
        {
            yield return new WaitForSeconds(decayTime);
            if (decayPercent > 0)
            {
                decayPercent -= decayAmount; // Zmniejsz procent
            }

            if (decayPercent <= 0)
            {
                HealthScript.healthPercent -= 4; // Zmniejsz zdrowie
            }
        }
    }
}
