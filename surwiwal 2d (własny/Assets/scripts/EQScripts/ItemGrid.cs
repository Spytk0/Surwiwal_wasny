using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGrid : MonoBehaviour
{
    public const float tileSizeWidth = 50;
    public const float tileSizeHeight = 50;

    InventoryItem[,] inventoryItemSlot;
    RectTransform rectTransform;

    [SerializeField] int gridSizeWidth;
    [SerializeField] int gridSizeHeight;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        Init(gridSizeWidth, gridSizeHeight);
    }

    private void Init(int width, int height)
    {
        inventoryItemSlot = new InventoryItem[width, height];
        Vector2 size = new Vector2(width * tileSizeWidth, height * tileSizeHeight);
        rectTransform.sizeDelta = size;
    }

    public bool PositionCheck(int posX, int posY)
    {
        return posX >= 0 && posX < gridSizeWidth && posY >= 0 && posY < gridSizeHeight;
    }

    public bool BoundaryCheck(int posX, int posY)
    {
        return PositionCheck(posX, posY);
    }

    public Vector2Int? FindSpaceForObject(InventoryItem item)
    {
        for (int y = 0; y <= gridSizeHeight - item.HEIGHT; y++)
        {
            for (int x = 0; x <= gridSizeWidth - item.WIDTH; x++)
            {
                // Dodajemy null jako ignoreItem i przekazujemy _
                if (CheckAvailableSpace(x, y, item.WIDTH, item.HEIGHT, null, out _))
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return null;
    }


    public InventoryItem GetItem(int x, int y)
    {
        if (BoundaryCheck(x, y))
        {
            return inventoryItemSlot[x, y];
        }
        return null; // Poza granicami
    }

    public void RemoveItem(InventoryItem item)
    {
        for (int x = 0; x < item.WIDTH; x++)
        {
            for (int y = 0; y < item.HEIGHT; y++)
            {
                inventoryItemSlot[item.onGridPositionX + x, item.onGridPositionY + y] = null;
            }
        }
    }

    public List<InventoryItem> CheckOverlaps(int posX, int posY, int width, int height)
    {
        List<InventoryItem> overlappedItems = new List<InventoryItem>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int checkX = posX + x;
                int checkY = posY + y;
                if (!BoundaryCheck(checkX, checkY)) // U¿ycie BoundaryCheck
                {
                    return null;
                }
                InventoryItem item = inventoryItemSlot[checkX, checkY];
                if (item != null && !overlappedItems.Contains(item))
                {
                    overlappedItems.Add(item);
                }
            }
        }
        return overlappedItems;
    }

    public bool PlaceItem(InventoryItem item, int posX, int posY, ref InventoryItem overlapItem)
    {
        overlapItem = null;

        // SprawdŸ czy mo¿na umieœciæ przedmiot
        List<InventoryItem> overlaps = CheckOverlaps(posX, posY, item.WIDTH, item.HEIGHT);

        if (overlaps == null) return false; // Poza granicami
        if (overlaps.Count > 1) return false; // Zbyt wiele kolizji

        if (overlaps.Count == 1)
        {
            // Jeœli jest kolizja z jednym przedmiotem (do zamiany)
            overlapItem = overlaps[0];
            CleanGridReference(overlapItem);
        }

        // Umieœæ przedmiot
        Place(item, posX, posY);
        return true;
    }



    public void Place(InventoryItem inventoryItem, int posX, int posY)
    {
        RectTransform itemRect = inventoryItem.GetComponent<RectTransform>();
        itemRect.SetParent(rectTransform);

        for (int x = 0; x < inventoryItem.WIDTH; x++)
        {
            for (int y = 0; y < inventoryItem.HEIGHT; y++)
            {
                inventoryItemSlot[posX + x, posY + y] = inventoryItem;
            }
        }

        inventoryItem.onGridPositionX = posX;
        inventoryItem.onGridPositionY = posY;

        Vector2 position = CalculatePositionOnGrid(inventoryItem, posX, posY);
        itemRect.localPosition = position;
    }

    private void CleanGridReference(InventoryItem item)
    {
        for (int x = 0; x < item.WIDTH; x++)
        {
            for (int y = 0; y < item.HEIGHT; y++)
            {
                inventoryItemSlot[item.onGridPositionX + x, item.onGridPositionY + y] = null;
            }
        }
    }

    public Vector2Int GetTileGridPosition(Vector2 mousePosition)
    {
        Vector2 positionOnGrid = new Vector2();
        positionOnGrid.x = mousePosition.x - rectTransform.position.x;
        positionOnGrid.y = rectTransform.position.y - mousePosition.y;

        Vector2Int tileGridPosition = new Vector2Int();
        tileGridPosition.x = (int)(positionOnGrid.x / tileSizeWidth);
        tileGridPosition.y = (int)(positionOnGrid.y / tileSizeHeight);

        return tileGridPosition;
    }

    public InventoryItem PickUpItem(int x, int y)
    {
        InventoryItem toReturn = inventoryItemSlot[x, y];
        if (toReturn == null) return null;
        CleanGridReference(toReturn);
        return toReturn;
    }

    public Vector2 CalculatePositionOnGrid(InventoryItem inventoryItem, int posX, int posY)
    {
        Vector2 position = new Vector2();
        position.x = posX * tileSizeWidth + tileSizeWidth * inventoryItem.WIDTH / 2;
        position.y = -(posY * tileSizeHeight + tileSizeHeight * inventoryItem.HEIGHT / 2);
        return position;
    }

    public bool CheckAvailableSpace(int posX, int posY, int width, int height,
                               InventoryItem ignoreItem, out InventoryItem overlappingItem)
    {
        overlappingItem = null;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int checkX = posX + x;
                int checkY = posY + y;

                if (!PositionCheck(checkX, checkY))
                {
                    return false;
                }

                InventoryItem item = inventoryItemSlot[checkX, checkY];
                if (item != null && item != ignoreItem)
                {
                    if (overlappingItem == null)
                    {
                        overlappingItem = item;
                    }
                    else if (overlappingItem != item)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    public int CalculateOccupiedArea(int posX, int posY, int width, int height)
    {
        int occupiedTiles = 0;

        for (int x = posX; x < posX + width; x++)
        {
            for (int y = posY; y < posY + height; y++)
            {
                if (PositionCheck(x, y) && inventoryItemSlot[x, y] != null)
                {
                    occupiedTiles++;
                }
            }
        }
        return occupiedTiles;
    }

    public bool CanSwapItems(InventoryItem item1, Vector2Int pos1,
                         InventoryItem item2, Vector2Int pos2)
    {
        // Stwórz tymczasow¹ kopiê siatki
        bool[,] tempGrid = new bool[gridSizeWidth, gridSizeHeight];

        // Zaznacz zajête pola przed zamian¹
        for (int x = 0; x < gridSizeWidth; x++)
        {
            for (int y = 0; y < gridSizeHeight; y++)
            {
                tempGrid[x, y] = inventoryItemSlot[x, y] != null;
            }
        }

        // Usuñ oba przedmioty z siatki
        RemoveFromTempGrid(tempGrid, pos1.x, pos1.y, item1.WIDTH, item1.HEIGHT);
        RemoveFromTempGrid(tempGrid, pos2.x, pos2.y, item2.WIDTH, item2.HEIGHT);

        // Próbujemy dodaæ je w nowych pozycjach
        if (!CanPlaceInTempGrid(tempGrid, pos2.x, pos2.y, item1.WIDTH, item1.HEIGHT) ||
            !CanPlaceInTempGrid(tempGrid, pos1.x, pos1.y, item2.WIDTH, item2.HEIGHT))
        {
            return false;
        }

        return true;
    }

    private void RemoveFromTempGrid(bool[,] grid, int posX, int posY, int width, int height)
    {
        for (int x = posX; x < posX + width; x++)
        {
            for (int y = posY; y < posY + height; y++)
            {
                if (PositionCheck(x, y))
                {
                    grid[x, y] = false;
                }
            }
        }
    }

    private bool CanPlaceInTempGrid(bool[,] grid, int posX, int posY, int width, int height)
    {
        for (int x = posX; x < posX + width; x++)
        {
            for (int y = posY; y < posY + height; y++)
            {
                if (!PositionCheck(x, y) || grid[x, y])
                {
                    return false;
                }
            }
        }
        return true;
    }



}



/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGrid : MonoBehaviour
{
   public const float tileSizeWidth = 50;
   public const float tileSizeHeight = 50;

    InventoryItem[,] inventoryItemSlot;
    RectTransform rectTransform;

    [SerializeField] int gridSizeWidth;
    [SerializeField] int gridSizeHeight;

    

    Vector2 positionOnTheGrid = new Vector2();
    Vector2Int tileGridPosition = new Vector2Int();

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        Init(gridSizeWidth,gridSizeHeight );

    }

    private void Init(int width, int height)
    {
       inventoryItemSlot = new InventoryItem[width, height];
        Vector2 size = new Vector2(width * tileSizeWidth, height * tileSizeHeight);
        rectTransform.sizeDelta = size;
    }

    public Vector2Int GetTileGridPosition(Vector2 mousePosition)
    {
        positionOnTheGrid.x = mousePosition.x -rectTransform.position.x;
        positionOnTheGrid.y = rectTransform.position.y - mousePosition.y;

        tileGridPosition.x = (int)(positionOnTheGrid.x / tileSizeWidth);
        tileGridPosition.y = (int)(positionOnTheGrid.y / tileSizeHeight);

        return tileGridPosition;
    }
    public InventoryItem PickUpItem(int x, int y)
    {
        InventoryItem toReturn = inventoryItemSlot[x, y];

        if (toReturn == null) { return null; }
        CleanGridReferece(toReturn);
        return toReturn;
    }

    private void CleanGridReferece(InventoryItem item)
    {
        for (int ix = 0; ix < item.WIDTH; ix++)//ok
        {
            for (int iy = 0; iy < item.HEIGHT; iy++)//ok
            {
                inventoryItemSlot[item.onGridPositionX + ix, item.onGridPositionY + iy] = null;
            }
        }
    }

    public bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, ref InventoryItem overlapItem)
    {
        if (BoundryCheck(posX, posY, inventoryItem.WIDTH, inventoryItem.HEIGHT) == false)//ok
        {
            return false;
        }

        if (OverlapCheck(posX, posY, inventoryItem.WIDTH, inventoryItem.HEIGHT, ref overlapItem) == false)//ok
        {
            overlapItem = null;
            return false;
        }


        if (overlapItem != null)
        {
            CleanGridReferece(overlapItem);
        }

        Place(inventoryItem, posX, posY);
        return true;
    }

    public void Place(InventoryItem inventoryItem, int posX, int posY)
    {
        RectTransform rectTransform = inventoryItem.GetComponent<RectTransform>();
        rectTransform.SetParent(this.rectTransform);

        for (int x = 0; x < inventoryItem.WIDTH; x++)//ok
        {
            for (int y = 0; y < inventoryItem.HEIGHT; y++)//ok
            {
                inventoryItemSlot[posX + x, posY + y] = inventoryItem;
            }
        }

        inventoryItem.onGridPositionX = posX;
        inventoryItem.onGridPositionY = posY;

        Vector2 position = CalculatePositionOnGrid(inventoryItem, posX, posY);

        rectTransform.localPosition = position;
    }

    public Vector2 CalculatePositionOnGrid(InventoryItem inventoryItem, int posX, int posY)
    {
        Vector2 position = new Vector2();
        position.x = posX * tileSizeWidth + tileSizeWidth * inventoryItem.WIDTH / 2;//ok
        position.y = -(posY * tileSizeHeight + tileSizeHeight * inventoryItem.HEIGHT / 2);//ok
        return position;
    }

    private bool OverlapCheck(int posX, int posY, int width, int height, ref InventoryItem overlapItem)
    {
        for(int x = 0; x< width; x++)
        {
            for(int y = 0; y< height; y++)
            {
                if (inventoryItemSlot[posX + x, posY + y] != null)
                {
                    if (overlapItem == null)
                    overlapItem = inventoryItemSlot[posX + x, posY + y];
                }
                else
                {
                    if(overlapItem != inventoryItemSlot[posX + x, posY + y])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    private bool CheckAvaibleSpace(int posX, int posY, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (inventoryItemSlot[posX + x, posY + y] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool PositonCheck(int posX, int posY)
    {
        if (posX < 0 || posY < 0)
        {
            return false;
        }
        if (posX >= gridSizeWidth || posY >= gridSizeHeight)
        {
            return false;
        }
        return true;
    }

    public bool BoundryCheck(int posX, int posY, int width, int height) 
    {
        if(PositonCheck(posX, posY) == false) { return false; }
        posX += width-1;
        posY += height-1;
        if(PositonCheck(posX, posY) == false) { return false; } 
        return true;
    }

    internal InventoryItem GetItem(int x, int y)
    {
        return inventoryItemSlot[x, y];
    }

    internal Vector2Int? FindSpaceForObject(InventoryItem itemToInsert)
    {
        int height = gridSizeHeight - itemToInsert.HEIGHT +1;//ok
        int width = gridSizeWidth - itemToInsert.WIDTH+1;//ok
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

               if( CheckAvaibleSpace(x, y, itemToInsert.WIDTH, itemToInsert.HEIGHT) == true)//ok
                {
                    return new Vector2Int(x, y);

                }
            }
        }
        return null;
    }
    public List<InventoryItem> CheckOverlaps(int posX, int posY, int width, int height)
    {
        List<InventoryItem> overlappedItems = new List<InventoryItem>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int checkX = posX + x;
                int checkY = posY + y;
                if (checkX < 0 || checkX >= gridSizeWidth || checkY < 0 || checkY >= gridSizeHeight)
                {
                    return null;
                }
                InventoryItem item = inventoryItemSlot[checkX, checkY];
                if (item != null && !overlappedItems.Contains(item))
                {
                    overlappedItems.Add(item);
                }
            }
        }
        return overlappedItems;
    }


}
*/