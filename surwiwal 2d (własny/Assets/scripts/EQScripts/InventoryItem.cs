using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Dodaj te pola jeœli ich nie ma
    public int onGridPositionX = -1;
    public int onGridPositionY = -1;

    public ItemData itemData;

    public int HEIGHT
    {
        get
        {
            if (rotated == false)
            {
                return itemData.height;
            }
            return itemData.width;
        }
    }
    public int WIDTH
    {
        get
        { if (rotated == false)
            {
                return itemData.width;
            }
            return itemData.height;
        }
    }
    public bool rotated = false;

    internal void Set(ItemData itemData)
    {
        this.itemData = itemData;
        GetComponent<Image>().sprite = itemData.itemIcon;

        Vector2 size = new Vector2();
        size.x = itemData.width * ItemGrid.tileSizeWidth;//ok
        size.y = itemData.height * ItemGrid.tileSizeHeight;//ok
        GetComponent<RectTransform>().sizeDelta = size;
    }
    internal void Rotate()
    {
        rotated = !rotated;
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.rotation = Quaternion.Euler(0, 0, rotated == true ? 90f : 0);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (InventoryController.Instance.IsCarryingItem) return;
        InventoryController.Instance.SelectItem(this);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Logika rozpoczêcia przeci¹gania
    }
    public void OnDrag(PointerEventData eventData)
    {
        // Logika przeci¹gania
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        // Logika zakoñczenia przeci¹gania
        InventoryController.Instance.DeselectItem();
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}

