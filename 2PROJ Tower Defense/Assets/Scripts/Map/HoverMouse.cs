using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HoverMouse : MonoBehaviour
{

    [SerializeField] private Grid grid;
    [SerializeField] private List<TileBase> availableTiles = new List<TileBase>();

    private Vector3Int? hovering;
    private TileBase? hoveredTile;
    private Tilemap? hoveredMap;


    // Start is called before the first frame update
    void Start()
    {
        hovering = null;
        hoveredTile = null;
        hoveredMap = null;
    }


    // Check if mouse is over a grass tile and return true if it is
    public bool checkTileAvailability(Vector3Int cellIndex)
    {

        Tilemap tilemap = getTilemapInGame(cellIndex.x, cellIndex.y);
        if (tilemap != null)
        {
            TileBase currTile = getTileInMap(tilemap, cellIndex.x, cellIndex.y);

            bool isTileAvailable = availableTiles.Contains(currTile);

            UpdateHover(cellIndex, tilemap, currTile, isTileAvailable);

            return isTileAvailable;

        }

        return false;
    }

    private void UpdateHover(Vector3Int cellIndex, Tilemap tilemap, TileBase currTile, bool available)
    {

        if (hovering != cellIndex)
        {

            if (hovering != null)
            {
                hoveredMap.SetTile(hovering ?? Vector3Int.zero, hoveredTile);
            }

            if (available)
            {
                hovering = cellIndex;
                hoveredTile = currTile;
                hoveredMap = tilemap;

                tilemap.SetTile(cellIndex, currTile);
                tilemap.SetColor(cellIndex, Color.red);


            }
            else
            {
                hovering = null;
                hoveredTile = null;
                hoveredMap = null;
            }

        }


    }

    //Get index relative to grid using mouse position
    public Vector3Int getCellIndexFromMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

        Vector3Int cellIndex = grid.WorldToCell(worldPos);

        cellIndex.x += 1;
        cellIndex.y += 1;

        return cellIndex;

    }

    public static TileBase getTileInMap(Tilemap map, int x, int y)
    {

        return map.GetTile(new Vector3Int(x, y));

    }

    private Tilemap getTilemapInGame(int x, int y)
    {
        // Search for game object with name "x,y"
        string gameObjectName = (-x) + "," + (-y);
        GameObject buffer = GameObject.Find("VisibleMap").transform.Find(gameObjectName)?.gameObject;

        if (buffer == null)
        {
            //Debug.LogError("Could not find GameObject with name: " + gameObjectName);
            return null;
        }

        Tilemap tilemap = buffer.GetComponent<Tilemap>();

        if (tilemap == null)
        {
            //Debug.LogError("Could not find Tilemap component on GameObject with name: " + gameObjectName);
            return null;
        }

        return tilemap;

    }
}
