using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpriteController : MonoBehaviour
{
       
    Dictionary<Tile, GameObject> tileGameObjectMap;
    public Sprite floorSprite;
    public Sprite emptySprite;

    World world
    {
        get { return WorldController.Instance.world; }
    }
    // Start is called before the first frame update
    void Start()
    {
        // Dictionary to track which GameObject is rendering which Tile data
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Add callback to check for tile type changes
        world.RegisterTileChanged(OnTileChanged);

        // Create gameobjects for each tile
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                GameObject tile_go = new GameObject();
                Tile tile_data = world.GetTileAt(x, y);

                // Add tile/GO pair to the dictionary
                tileGameObjectMap.Add(tile_data, tile_go);

                tile_go.name = "Tile_" + x + "_ " + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_go.transform.SetParent(this.transform, true);


                // Add a sprite renderer 
                // Add a default sprite for empty tiles
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();
                tile_go.GetComponent<SpriteRenderer>().sprite = emptySprite;

            }
        }

    }

    // Example function for when changing floors/levels
    void DestroyAllTileGameObjects()
    {
        while(tileGameObjectMap.Count > 0)
        {
            Tile tile_data = tileGameObjectMap.Keys.First();
            GameObject tile_go = tileGameObjectMap[tile_data];

            // Remove the pair from the map
            tileGameObjectMap.Remove(tile_data);

            // Unregister the callback
            tile_data.UnregisterTileTypeChangedCallback(OnTileChanged);

            // Destroy the visual GameObject
            Destroy(tile_go); //Use SimplePool?
        }
    }

    void OnTileChanged(Tile tile_data)
    {
        if(tileGameObjectMap.ContainsKey(tile_data) == false)
        {
            Debug.LogError("tileGameObjectMap does not contain tile_data. Is the tile missing from the dictionary? Unregistered callback?");
            return;
        }
        GameObject tile_go = tileGameObjectMap[tile_data];

        if(tile_go == null)
        {
            Debug.LogError("tileGameObjectMap does not contain tile_data. Is the tile missing from the dictionary? Unregistered callback?");
            return;
        }

        if(tile_data.Type == TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        } 
        else if (tile_data.Type == TileType.Empty)

        {
            tile_go.GetComponent<SpriteRenderer>().sprite = null;
        }
        else
        {
            Debug.LogError("OnTileChanged - Unrecognized tile type");
        }
    }

}
