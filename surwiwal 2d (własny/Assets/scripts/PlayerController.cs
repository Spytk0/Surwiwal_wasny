using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public Tilemap tilemap; // Przypisz Tilemap w inspektorze
    public float moveSpeed = 1f; // Pr�dko�� poruszania
    private Vector3 targetPosition;

    void Start()
    {
        // Ustaw pocz�tkow� pozycj� gracza
        targetPosition = transform.position;
    }
    void Update()
    {
        // Sprawd�, czy kt�rykolwiek z paneli UI jest aktywny
        if (UIManager.Instance.IsAnyUIPanelActive())
        {
            return; // Zatrzymaj dzia�anie skryptu, je�li kt�rykolwiek UI jest aktywny
        }
    

    // Poruszaj gracza w kierunku docelowej pozycji
    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Sprawd�, czy gracz osi�gn�� docelow� pozycj�
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // Ustaw now� docelow� pozycj� po klikni�ciu myszk�
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPosition);
                targetPosition = tilemap.GetCellCenterWorld(cellPosition);
            }
        }
    }
}
