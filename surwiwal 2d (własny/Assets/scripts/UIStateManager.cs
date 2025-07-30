using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject[] uiPanels; // Przypisz panele UI w inspektorze

    private void Awake()
    {
        // Upewnij si�, �e jest tylko jedna instancja UIManager
        if (Instance == null)
        {
            Instance = this;
    
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Sprawdza, czy kt�rykolwiek z paneli UI jest aktywny
    public bool IsAnyUIPanelActive()
    {
        foreach (GameObject panel in uiPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                return true; // Zwr�� true, je�li kt�rykolwiek panel jest aktywny
            }
        }
        return false; // Zwr�� false, je�li �aden panel nie jest aktywny
    }
}
