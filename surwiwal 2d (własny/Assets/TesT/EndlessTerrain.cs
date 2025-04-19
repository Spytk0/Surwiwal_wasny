using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDst = 300;
    public Transform viewer;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunkVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.MapChunkSize;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst) / chunkSize;
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.y);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].isVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
                }
            }
        }
    }

    public class TerrainChunk
    {
        Vector2 position;
        Bounds bounds;
        MapGenerator mapGenerator;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            mapGenerator = FindObjectOfType<MapGenerator>();
            GenerateChunk();
        }

        void GenerateChunk()
        {
            mapGenerator.RequestMapData(OnMapDataReceived, position);
        }

        void OnMapDataReceived(MapData mapData)
        {
            for (int y = 0; y < mapData.heightMap.GetLength(0); y++)
            {
                for (int x = 0; x < mapData.heightMap.GetLength(1); x++)
                {
                    float currentHeight = mapData.heightMap[x, y];
                    TileBase tileToSet = null;

                    // Wybierz odpowiedni kafelek na podstawie wysokoœci
                    for (int i = 0; i < mapGenerator.regions.Length; i++)
                    {
                        if (currentHeight <= mapGenerator.regions[i].height)
                        {
                            tileToSet = mapGenerator.regions[i].tile;
                            break;
                        }
                    }

                    if (tileToSet != null)
                    {
                        mapGenerator.SetTile(new Vector3Int((int)position.x + x, (int)position.y + y, 0), tileToSet);
                    }
                }
            }
        }

        public void UpdateTerrainChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            // W tej wersji nie u¿ywamy GameObject, poniewa¿ u¿ywamy Tilemap
        }

        public bool isVisible()
        {
            return true; // Zawsze widoczne, poniewa¿ u¿ywamy Tilemap
        }
    }
}

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDst = 300;
    public Transform viewer;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunkVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst) / chunkSize;
    }
    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.y);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();
        int curentChunkCordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int curentChunkCordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(curentChunkCordX + xOffset, curentChunkCordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].isVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
                }
            }
        }
    }
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positonV3 = new Vector3(position.x, position.y, 0);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshObject.transform.position = positonV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
            mapGenerator.RequestMapData(OnMapDataReceived);
        }
        void OnMapDataReceived(MapData mapData) 
        {
            print("Map data received");

        }
        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }
        public void UpdateTerrainChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
        }
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
    }
}


*/
/*using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
public class EndlessTerrain : MonoBehaviour
{
    public TileTypeManager tileTypeManager; // Przypisz w inspektorze
    public const float maxViewDst = 300;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunkVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    public Tilemap tilemap; // Dodaj pole do przypisania Tilemap

    private void Start()
    {
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst) / chunkSize;
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.y);
        UpdateVisibleChunks();
    }
void UpdateVisibleChunks()
    {
        // Ukryj wszystkie widoczne chunk
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        // Obliczanie aktualnych wspó³rzêdnych chunk
        int currentChunkCordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        // Generowanie nowych chunków w zasiêgu widoku
        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCordX + xOffset, currentChunkCordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].isVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    // Tworzenie nowego chunku
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, tilemap, tileTypeManager.tilesRegions, new Vector2(viewedChunkCoord.x * chunkSize, viewedChunkCoord.y * chunkSize)));
                }
            }
        }
    }

    public class TerrainChunk
    {
        Tilemap tilemap;
        Vector2 position;
        Bounds bounds;
        TileType[] tilesRegions; // Dodaj to pole

        public TerrainChunk(Vector2 coord, int size, Transform parent, Tilemap tilemap, TileType[] tilesRegions, Vector2 offset)
        {
            this.tilesRegions = tilesRegions; // Przypisanie do lokalnej zmiennej
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, position.y, 0);

            // Tworzenie nowego GameObject dla Tilemap
            GameObject tilemapObject = new GameObject("Chunk_" + coord);
            tilemapObject.transform.position = positionV3;
            tilemapObject.transform.parent = parent;

            // Dodawanie komponentu Tilemap
            this.tilemap = tilemapObject.AddComponent<Tilemap>();
            tilemapObject.AddComponent<TilemapRenderer>();

            // Generowanie kafelków w Tilemap
            GenerateTiles(tilemap, coord, size, offset);
            SetVisible(false);
        }

        void GenerateTiles(Tilemap tilemap, Vector2 coord, int size, Vector2 offset)
        {
            if (tilemap == null)
            {
                Debug.LogError("Tilemap is null!");
                return;
            }

            if (tilesRegions == null || tilesRegions.Length == 0)
            {
                Debug.LogError("TilesRegions is null or empty!");
                return;
            }

            // Generowanie mapy ha³asu
            float[,] noiseMap = Noise.GenerateNoiseMap(size, size, 0, 1f, 1, 0.5f, 2f, Vector2.zero);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < tilesRegions.Length; i++)
                    {
                        if (currentHeight <= tilesRegions[i].height)
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), tilesRegions[i].tile);
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateTerrainChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            tilemap.gameObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return tilemap.gameObject.activeSelf;
        }
    }
}









*/
