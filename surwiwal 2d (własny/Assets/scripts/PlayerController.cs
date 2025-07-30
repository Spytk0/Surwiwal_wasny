using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public Tilemap tilemap; // Przypisz Tilemap w inspektorze
    public float moveSpeed = 1f; // Prêdkoœæ poruszania
    private Vector3 targetPosition;

    void Start()
    {
        // Ustaw pocz¹tkow¹ pozycjê gracza
        targetPosition = transform.position;
    }
    void Update()
    {
        // SprawdŸ, czy którykolwiek z paneli UI jest aktywny
        if (UIManager.Instance.IsAnyUIPanelActive())
        {
            return; // Zatrzymaj dzia³anie skryptu, jeœli którykolwiek UI jest aktywny
        }
    

    // Poruszaj gracza w kierunku docelowej pozycji
    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // SprawdŸ, czy gracz osi¹gn¹³ docelow¹ pozycjê
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // Ustaw now¹ docelow¹ pozycjê po klikniêciu myszk¹
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPosition);
                targetPosition = tilemap.GetCellCenterWorld(cellPosition);
            }
        }
    }
}
