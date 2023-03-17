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

    private Vector3Int previousCellIndex = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue); // Initialize the previous cell index to a value that is different from any valid index

    void Update()
    {

        //Vector3 cellPos = grid.GetCellCenterWorld(new Vector3Int(0, 0));
        //Debug.Log("cellPos : " + cellPos.x + " " + cellPos.y);

        Vector3 mousePos = Input.mousePosition;
        Debug.Log("mousepos : " + mousePos.x + " " + mousePos.y);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        Debug.Log("worldpos : " + worldPos.x + " " + worldPos.y);

        Vector3Int cellIndex = grid.WorldToCell(worldPos);
        Debug.Log("cell : " + cellIndex.x + " " + cellIndex.y);


        TileBase currTile = getTileInMap(paths, 0, 0);

        foreach (Transform child in grid.transform)
        {
            if (child.gameObject.name == $"{-cellIndex.x},{-cellIndex.y}")
            {
                Tilemap cellTileMap = child.gameObject.GetComponent<Tilemap>();

                cellTileMap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y), currTile);
            }
        }
    
    }









}
