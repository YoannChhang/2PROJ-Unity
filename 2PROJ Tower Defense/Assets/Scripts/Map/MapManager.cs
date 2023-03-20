using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity;

public class MapManager : MonoBehaviour
{

    [SerializeField] public GameObject designGrid;
    [SerializeField] public Tilemap paths;
    [SerializeField] public Tilemap tilemapPrefab;
    [SerializeField] public Grid grid;

    [SerializeField] public int gridWidth = 40;
    [SerializeField] public int gridHeight = 35;

    private TilemapRenderer tilemapRenderer;

    // Start is called before the first frame update
    void Start()
    {
        RenderMap();
        designGrid.SetActive(false);

    }

    public void RenderMap()
    {
        
        HelperFunctions.remove_all_childs_from_gameobject(grid.gameObject);


        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {

                Tilemap newTile = Tilemap.Instantiate(tilemapPrefab);
                TileBase currTile = getTileInMap(paths, -x, -y);


                newTile.SetTile(new Vector3Int(-x, -y), currTile);
                newTile.name = x + "," + y;
                newTile.transform.SetParent(grid.transform, false);

                newTile.GetComponent<TilemapRenderer>().sortingOrder = x + y;

            }
        }

        
    }

    public static TileBase getTileInMap(Tilemap map, int x, int y)
    {

        return map.GetTile(new Vector3Int(x, y));   

    }





}
