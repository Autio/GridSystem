using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{


    public static WorldController Instance { get; protected set; }

    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;

    public World World { get; protected set; }

    public Sprite wallSprite; // fixme
    public Sprite floorSprite;

    // Start is called before the first frame update
    void Start()
    {
        if(Instance != null)
        {
            Debug.Log("There should only ever be one world controller");
        }
        Instance = this;
        // Create blank world
        World = new World();

        World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);

        // Dictionary to track which GameObject is rendering which Tile data
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();


        // Create gameobjects for each tile
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                GameObject tile_go = new GameObject();
                Tile tile_data = World.GetTileAt(x, y);

                // Add tile/GO pair to the dictionary
                tileGameObjectMap.Add(tile_data, tile_go);

                tile_go.name = "Tile_" + x + "_ " + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_go.transform.SetParent(this.transform, true);


                // Add a sprite renderer
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();

                // Add callback to check for tile type changes
                tile_data.RegisterTileTypeChangedCallback(OnTileTypeChanged);

            }
        }
        World.RandomizeTiles();
    }

    //float randomizeTileTimer = 2f;

    // Update is called once per frame
    void Update()
    {
        //randomizeTileTimer -= Time.deltaTime;
        //if(randomizeTileTimer <0 )
        //{
        //    randomizeTileTimer = 2f;
        //    World.RandomizeTiles();
        //}
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
            tile_data.UnregisterTileTypeChangedCallback(OnTileTypeChanged);

            // Destroy the visual GameObject
            Destroy(tile_go); //Use SimplePool?
        }
    }

    void OnTileTypeChanged(Tile tile_data)
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
            Debug.LogError("OnTileTypeChanged - Unrecognized tile type");
        }
    }

    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x);
        int y = Mathf.FloorToInt(coord.y);

        return World.GetTileAt(x, y);
    }

    public void OnInstalledObjectCreated( InstalledObject obj)
    {
        // FIXME: Multi-tile and rotated objects

        // Create a visual GameObject linked to this data
        GameObject obj_go = new GameObject();

        // Add tile/GO pair to the dictionary
        installedObjectGameObjectMap.Add(obj, obj_go);

        obj_go.name = obj.objectType + " " + obj.tile.X + "_ " + obj.tile.Y;
        obj_go.transform.position = new Vector3(obj.tile.X, obj.tile.Y, 0);
        obj_go.transform.SetParent(this.transform, true);


        // Add a sprite renderer
        obj_go.AddComponent<SpriteRenderer>().sprite = wallSprite;
        // Put it on the right layer
        obj_go.GetComponent<SpriteRenderer>().sortingLayerName = "InstalledObjects";

        // Add callback to check for object infor changes
        obj.RegisterOnChangedCallback(OnInstalledObjectChanged);

    }

    void OnInstalledObjectChanged( InstalledObject obj )
    {
        Debug.LogError(" OnInstalledObjectChnaged not yet implemented");
    }

}
