using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject[] uiPanels; // Przypisz panele UI w inspektorze

    private void Awake()
    {
        // Upewnij siê, ¿e jest tylko jedna instancja UIManager
        if (Instance == null)
        {
            Instance = this;
    
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Sprawdza, czy którykolwiek z paneli UI jest aktywny
    public bool IsAnyUIPanelActive()
    {
        foreach (GameObject panel in uiPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                return true; // Zwróæ true, jeœli którykolwiek panel jest aktywny
            }
        }
        return false; // Zwróæ false, jeœli ¿aden panel nie jest aktywny
    }
}
