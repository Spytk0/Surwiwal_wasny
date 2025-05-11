using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [HideInInspector]
    private ItemGrid selectedItemGrid;
    public ItemGrid SelectedItemGrid { get => selectedItemGrid; set
        {
            selectedItemGrid = value;
            inventoryHighlight.SetParent(value);
        }
    }

    InventoryItem selectedItem;
    InventoryItem overlapItem;
    RectTransform rectTransform;

    [SerializeField] List<ItemData> items;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] Transform canvasTransform;

    InventoryHighlight inventoryHighlight;
    InventoryItem itemToHighlight;
    Vector2 oldPosition;

    private void Awake()
    {
        inventoryHighlight = GetComponent<InventoryHighlight>();
    }
    private void Update()
    {
        ItemIconDrag();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(selectedItem == null)
            {
                CreateRandomItem();
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            InsertRandomItem();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }

        if (selectedItemGrid == null)
        {
            inventoryHighlight.Show(false);
            return;
        }
        HandleHighlight();

        if (Input.GetMouseButtonDown(0))
        {
            LeftMouseButtonPress();
        }
    }

    private void RotateItem()
    {
        if(selectedItem == null ) { return; }

        selectedItem.Rotate();
    }

    private void InsertRandomItem()
    {
        if (selectedItemGrid == null) { return; }

        CreateRandomItem();
        InventoryItem itemToInsert = selectedItem;
        selectedItem = null;
        InsertItem(itemToInsert);
    }

    private void InsertItem(InventoryItem itemToInsert)
    {
        Vector2Int? posOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);

        if (posOnGrid == null) { return; }

        selectedItemGrid.Place(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y);
    }
    private void HandleHighlight()
    {
        Vector2Int positonOnGrid = GetTileGridPosition();
        if (oldPosition == positonOnGrid)  { return; }
        oldPosition = positonOnGrid;
        if (selectedItem == null)
        {
            itemToHighlight = selectedItemGrid.GetItem(positonOnGrid.x , positonOnGrid.y);
            if(itemToHighlight != null)
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(itemToHighlight);
                inventoryHighlight.SetPosition(selectedItemGrid, itemToHighlight);
            }
            else
            {
                inventoryHighlight.Show(false);
            }
        }
        else
        {
            inventoryHighlight.Show(selectedItemGrid.BoundryCheck(positonOnGrid.x, positonOnGrid.y, selectedItem.WIDTH, selectedItem.HEIGHT));//ok
            inventoryHighlight.SetSize(selectedItem);
            inventoryHighlight.SetPosition(selectedItemGrid, selectedItem, positonOnGrid.x, positonOnGrid.y);
        }
    }

    private void CreateRandomItem()
    {
        InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
        selectedItem = inventoryItem;

        rectTransform = inventoryItem.GetComponent<RectTransform>();
        rectTransform.SetParent(canvasTransform);
        rectTransform.SetAsLastSibling();

        int selectedItemID = UnityEngine.Random.Range(0, items.Count);
        inventoryItem.Set(items[selectedItemID]);
    }

    private void LeftMouseButtonPress()
    {
        Vector2Int tileGridPositon = GetTileGridPosition();

        if (selectedItem == null)
        {
            PickUpItem(tileGridPositon);

        }
        else
        {
            PlaceItem(tileGridPositon);
        }
    }

    private Vector2Int GetTileGridPosition()
    {
        Vector2 position = Input.mousePosition;
        if (selectedItem != null)
        {
            position.x -= (selectedItem.WIDTH - 1) * ItemGrid.tileSizeWidth / 2;//ok
            position.y += (selectedItem.HEIGHT - 1) * ItemGrid.tileSizeHeight / 2;//ok
        }
        return selectedItemGrid.GetTileGridPosition(position);
    }

    private void PlaceItem(Vector2Int tileGridPositon)
    {
        bool complete = selectedItemGrid.PlaceItem(selectedItem, tileGridPositon.x, tileGridPositon.y, ref overlapItem);
        if (complete)
        {
            selectedItem = null;
            if(overlapItem != null)
            {
                selectedItem = overlapItem;
                overlapItem = null;
                rectTransform = selectedItem.GetComponent<RectTransform>();
                rectTransform.SetAsLastSibling();
            }
        }
    }

    private void PickUpItem(Vector2Int tileGridPositon)
    {
        selectedItem = selectedItemGrid.PickUpItem(tileGridPositon.x, tileGridPositon.y);
        if (selectedItem != null)
        {
            rectTransform = selectedItem.GetComponent<RectTransform>();
        }
    }

    private void ItemIconDrag()
    {
        if (selectedItem != null)
        {
            rectTransform.position = Input.mousePosition;
        }
    }
}
