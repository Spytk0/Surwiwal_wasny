using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public const int MapChunkSize = 100;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;

    public Tilemap tilemap;
    public TerrainType[] regions;
    public bool autoUpdate;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMap(Vector2.zero); // Generuj mapê w edytorze

        // Mo¿esz dodaæ kod do wyœwietlania mapy w edytorze, jeœli to potrzebne
        // Na przyk³ad, mo¿esz u¿yæ Tilemap do rysowania kafelków
    }
    public void RequestMapData(Action<MapData> callback, Vector2 position)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback, position);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback, Vector2 position)
    {
        MapData mapData = GenerateMap(position);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.paramiter);
            }
        }
    }

    MapData GenerateMap(Vector2 position)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        // Wype³nianie Tilemap na podstawie wysokoœci
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                TileBase tileToSet = null;

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        tileToSet = regions[i].tile;
                        break;
                    }
                }

                if (tileToSet != null)
                {
                    tilemap.SetTile(new Vector3Int((int)position.x + x, (int)position.y + y, 0), tileToSet);
                }
            }
        }

        return new MapData(noiseMap);
    }

    public void SetTile(Vector3Int position, TileBase tile)
    {
        tilemap.SetTile(position, tile);
    }

    public struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T paramiter;
        public MapThreadInfo(Action<T> callback, T paramiter)
        {
            this.callback = callback;
            this.paramiter = paramiter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public TileBase tile; // U¿yj TileBase do przechowywania kafelka
}

public struct MapData
{
    public readonly float[,] heightMap;

    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}
/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Mesh;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap,};
    public DrawMode drawMode;
    public const int MapChunkSize = 100;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public Tilemap tilemap;
    public Vector3Int tilemapOffset;

    public bool autoUpdate;

    public TerrainType[] regions;
    public TileType[] tilesRegions;
    Queue<MapThreadInfo<MapData>> mapDataTreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMap();

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize));
        }
    }
    public void RequestMapData(Action<MapData> callback)
    {

        ThreadStart threadStart = delegate
            {
                MapDataThread(callback);
            };
        new Thread(threadStart).Start();
    }
    void MapDataThread(Action<MapData> callback)
    {
        MapData mapData = GenerateMap();
        lock (mapDataTreadInfoQueue)
        {
            mapDataTreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }
    public void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {

    }
    private void Update()
    {
        if (mapDataTreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataTreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataTreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.paramiter);
            }
        }
    }
    MapData GenerateMap()
    {
        // Generowanie mapy ha³asu
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        // Wype³nianie colorMap na podstawie wysokoœci
        Color[] colorMap = new Color[MapChunkSize * MapChunkSize];
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float curentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (curentHeight <= regions[i].height)
                    {
                        colorMap[y * MapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < tilesRegions.Length; i++)
                {
                    if (currentHeight <= tilesRegions[i].height)
                    {
                        // U¿yj offsetu do ustawienia kafelka
                        tilemap.SetTile(new Vector3Int(x + tilemapOffset.x, y + tilemapOffset.y, 0), tilesRegions[i].tile);
                        break;
                        //tilemap.SetTile(new Vector3Int(x, y, 0), tilesRegions[i].tile);
                        // break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }
    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
    public struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T paramiter;
        public MapThreadInfo (Action<T> callback, T paramiter)
        {
            this.callback = callback;
            this.paramiter = paramiter;
        }
    }
}

// Typy terenu
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
   // public Tile tile; // Dodaj pole do przechowywania kafelka
}


public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}



*/
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, TileMap};
    public DrawMode drawMode;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    public TerrainType[] regions;
    public TileType[] tiles;
    public void GenerateMap()
    {
        // Generowanie mapy ha³asu
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        // Wype³nianie colorMap na podstawie wysokoœci
        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float curentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (curentHeight <= regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
        else if (drawMode == DrawMode.TileMap)
        {
            TextureGenerator.TileFromTexture();
        }
    
    }
    private void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}
// Typy terenu
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
   // public Tile tile; // Dodaj pole do przechowywania kafelka
}
[System.Serializable]
public struct TileType
{
    public string name;
    public float height;
    public Tile tile; // Dodaj pole do przechowywania kafelka
    public Tilemap tilemap;
}
*/
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap,};
    public DrawMode drawMode;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    const int mapChunkSize = 16;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public Tilemap tilemap;
    public Vector3Int tilemapOffset;

    public bool autoUpdate;

    public TerrainType[] regions;
    public TileType[] tilesRegions;
    public void GenerateMap()
    {
        // Generowanie mapy ha³asu
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        // Wype³nianie colorMap na podstawie wysokoœci
        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float curentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (curentHeight <= regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < tilesRegions.Length; i++)
                {
                    if (currentHeight <= tilesRegions[i].height)
                    {
                        // U¿yj offsetu do ustawienia kafelka
                        tilemap.SetTile(new Vector3Int(x + tilemapOffset.x, y + tilemapOffset.y, 0), tilesRegions[i].tile);
                        break;
                        //tilemap.SetTile(new Vector3Int(x, y, 0), tilesRegions[i].tile);
                        // break;
                    }
                }
            }
        }
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
    }
    private void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}
// Typy terenu
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
   // public Tile tile; // Dodaj pole do przechowywania kafelka
}
[System.Serializable]
public struct TileType
{
    public string name;
    public float height;
    public Tile tile; // Dodaj pole do przechowywania kafelka
} */