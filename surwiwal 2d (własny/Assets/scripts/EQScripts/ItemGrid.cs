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
}
