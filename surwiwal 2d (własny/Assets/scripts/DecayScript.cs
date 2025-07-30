using System.Collections;
using UnityEngine;
using TMPro;

public class DecayScript : MonoBehaviour
{
    public int decayPercent; // Procent g�odu/nawodnienia
    public int decayAmount; // Ilo�� spadku
    public float decayTime; // Jak cz�sto statystyki spadaj�
    public TMP_Text decayText; // Tekst do wy�wietlania procentu
    private Coroutine decayCoroutine; // Referencja do coroutine

    private void Start()
    {
        decayCoroutine = StartCoroutine(Decay()); // Rozpocznij proces dekay
    }

    private void Update()
    {
        decayText.text = decayPercent + "%"; // Aktualizuj tekst

        // Sprawd�, czy kt�rykolwiek z paneli UI jest aktywny
        if (UIManager.Instance.IsAnyUIPanelActive())
        {
            if (decayCoroutine != null)
            {
                StopCoroutine(decayCoroutine); // Zatrzymaj proces dekay
                decayCoroutine = null; // Ustaw referencj� na null
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
        while (true) // P�tla niesko�czona, kontrola odbywa si� w Update
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
