using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class MapManager : MonoBehaviour
{

    public GameObject designGrid;
    public Tilemap paths;
    public Tilemap tilemapPrefab;
    private TilemapRenderer tilemapRenderer;
    public Grid grid;

    public int gridWidth = 40;
    public int gridHeight = 35;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {

                Debug.Log($"{-x} + {-y}");

                Tilemap newTile = Tilemap.Instantiate(tilemapPrefab);
                TileBase currTile = getTileInMap(paths, -x, -y);


                newTile.SetTile(new Vector3Int(-x, -y), currTile);
                newTile.name = x + "," + y;
                newTile.transform.SetParent(grid.transform, false);

                tilemapRenderer = newTile.GetComponent<TilemapRenderer>();
                tilemapRenderer.sortingOrder = x + y;
            }
        }

        designGrid.SetActive(false);
        
    }

    TileBase getTileInMap(Tilemap map, int x, int y)
    {

        return map.GetTile(new Vector3Int(x, y));   

    }
}
