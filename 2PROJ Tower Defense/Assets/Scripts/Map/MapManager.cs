using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{

    [SerializeField] private GameObject designGrid;
    [SerializeField] private Tilemap paths;
    [SerializeField] private Tilemap tilemapPrefab;
    [SerializeField] private Grid grid;

    [SerializeField] private int numberOfTilesX = 40;
    [SerializeField] private int numberOfTilesY = 35;

    // Start is called before the first frame update
    void Start()
    {
        RenderMap();
        designGrid.SetActive(false);
    }

    public void RenderMap()
    {
        
        HelperFunctions.remove_all_childs_from_gameobject(grid.gameObject);


        for (int y = 0; y < numberOfTilesY; y++)
        {
            for (int x = 0; x < numberOfTilesX; x++)
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
