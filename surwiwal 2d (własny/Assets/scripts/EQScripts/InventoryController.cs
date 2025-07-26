using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryController : MonoBehaviour
{
    [HideInInspector]
    private ItemGrid selectedItemGrid;
    public ItemGrid SelectedItemGrid
    {
        get => selectedItemGrid;
        set
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
        inventoryHighlight = GetComponent <InventoryHighlight>();
    }
    private void Update()
    {

        ItemIconDrag();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (selectedItem == null)
            {
                CreateRandomItem();
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            InsertRandomItem();
        }
        if (Input.GetKeyDown(KeyCode.R))
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
        if (selectedItem == null) { return; }

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
        if (posOnGrid == null)
        {
            Debug.Log("Brak miejsca w ekwipunku");
            return;
        }
        InventoryItem temp = null;
        selectedItemGrid.PlaceItem(itemToInsert, posOnGrid.Value.x, posOnGrid.Value.y, ref temp);
    }
    private void HandleHighlight()
    {
        Vector2Int positionOnGrid = GetTileGridPosition();
        if (oldPosition == positionOnGrid) { return; }
        oldPosition = positionOnGrid;

        if (selectedItem == null)
        {
            itemToHighlight = selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);
            if (itemToHighlight != null)
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
            // SprawdŸ, czy wszystkie pozycje zajmowane przez przedmiot s¹ w granicach siatki
            bool isInBounds = true;
            for (int x = 0; x < selectedItem.WIDTH; x++)
            {
                for (int y = 0; y < selectedItem.HEIGHT; y++)
                {
                    if (!selectedItemGrid.PositionCheck(positionOnGrid.x + x, positionOnGrid.y + y))
                    {
                        isInBounds = false;
                        break;
                    }
                }
                if (!isInBounds) break;
            }

            inventoryHighlight.Show(isInBounds);
            inventoryHighlight.SetSize(selectedItem);
            inventoryHighlight.SetPosition(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);
        }
    }


    private void CreateRandomItem()
    {
        InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
        selectedItem = inventoryItem;

        rectTransform = inventoryItem.GetComponent <RectTransform>();
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

    private void PlaceItem(Vector2Int tileGridPosition)
    {
        if (selectedItem == null || selectedItemGrid == null) return;

        // SprawdŸ nak³adanie siê z innymi przedmiotami (IGNORUJ¥C w³asn¹ star¹ pozycjê)
        List<InventoryItem> overlappingItems = GetTrueOverlaps(tileGridPosition, selectedItem);

        if (overlappingItems == null)
        {
            Debug.Log("Próba umieszczenia poza granicami siatki.");
            return;
        }

        if (overlappingItems.Count == 1)
        {
            Debug.Log($"Próba zamiany: {selectedItem.itemData.itemName} z {overlappingItems[0].itemData.itemName}");
            if (CheckSwapPossible(tileGridPosition, overlappingItems[0]))
            {
                Debug.Log("CheckSwapPossible: TRUE - wykonujê HandleItemSwap");
                HandleItemSwap(tileGridPosition, overlappingItems[0]);
            }
            else
            {
                Debug.Log("CheckSwapPossible: FALSE - ale nie powinno siê zdarzyæ!");
            }

        }
        else if (overlappingItems.Count == 0)
        {
            // Brak kolizji - normalne umieszczenie
            PlaceSingleItem(tileGridPosition);
        }
        else
        {
            Debug.Log("Zbyt wiele kolizji");
        }
    }

    private List<InventoryItem> GetTrueOverlaps(Vector2Int position, InventoryItem item)
    {
        List<InventoryItem> overlaps = new List<InventoryItem>();
        Vector2Int oldPos = new Vector2Int(item.onGridPositionX, item.onGridPositionY);

        for (int x = 0; x < item.WIDTH; x++)
        {
            for (int y = 0; y < item.HEIGHT; y++)
            {
                int checkX = position.x + x;
                int checkY = position.y + y;

                if (!selectedItemGrid.PositionCheck(checkX, checkY)) return null;

                // Ignoruj komórki zajmowane przez przenoszony przedmiot
                if (checkX >= oldPos.x && checkX < oldPos.x + item.WIDTH &&
                    checkY >= oldPos.y && checkY < oldPos.y + item.HEIGHT) continue;

                InventoryItem cellItem = selectedItemGrid.GetItem(checkX, checkY);
                if (cellItem != null && cellItem != item && !overlaps.Contains(cellItem))
                {
                    overlaps.Add(cellItem);
                }
            }
        }
        return overlaps;
    }


    private bool CheckSwapPossible(Vector2Int newPos, InventoryItem itemToSwap)
    {
        Vector2Int originalPos = new Vector2Int(selectedItem.onGridPositionX, selectedItem.onGridPositionY);

        // SprawdŸ czy przedmioty nie nachodz¹ na siebie po zamianie
        if (!selectedItemGrid.CanSwapItems(selectedItem, newPos, itemToSwap, originalPos))
        {
            Debug.Log("Przedmioty nachodz¹ na siebie po zamianie");
            return false;
        }

        // SprawdŸ czy nowe pozycje mieszcz¹ siê w siatce
        return selectedItemGrid.PositionCheck(newPos.x, newPos.y) &&
               selectedItemGrid.PositionCheck(originalPos.x, originalPos.y);
    }

    private void HandleItemSwap(Vector2Int newPos, InventoryItem itemToSwap)
    {
        Vector2Int originalPos = new Vector2Int(selectedItem.onGridPositionX, selectedItem.onGridPositionY);

        // Tymczasowo usuñ przedmioty
        selectedItemGrid.RemoveItem(selectedItem);
        selectedItemGrid.RemoveItem(itemToSwap);

        // Umieœæ przedmioty w nowych pozycjach
        InventoryItem temp = null;
        selectedItemGrid.PlaceItem(selectedItem, newPos.x, newPos.y, ref temp);
        selectedItemGrid.PlaceItem(itemToSwap, originalPos.x, originalPos.y, ref temp);

        selectedItem = null;
        Debug.Log("Zamiana wykonana pomyœlnie");
    }

    private void PlaceSingleItem(Vector2Int tileGridPosition)
    {
        InventoryItem tempOverlap = null;
        bool placed = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref tempOverlap);
        if (placed)
        {
            selectedItem = null; // Ustaw wybrany przedmiot na null po udanym umieszczeniu
        }
    }


    private void PickUpItem(Vector2Int tileGridPositon)
    {
        selectedItem = selectedItemGrid.PickUpItem(tileGridPositon.x, tileGridPositon.y);
        if (selectedItem != null)
        {
            rectTransform = selectedItem.GetComponent<RectTransform>();
            rectTransform.SetAsLastSibling();
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



/*using System;
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
*/