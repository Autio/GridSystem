using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{


    public static WorldController Instance { get; protected set; }

    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<Furniture, GameObject> FurnitureGameObjectMap;
    Dictionary<string, Sprite> FurnitureSprites;

    public World World { get; protected set; }

    public Sprite floorSprite;
    public Sprite emptySprite;

    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        if(Instance != null)
        {
            Debug.Log("There should only ever be one world controller");
        }
        Instance = this;
        // Create blank world
        World = new World();

        World.RegisterFurnitureCreated(OnFurnitureCreated);

        // Dictionary to track which GameObject is rendering which Tile data
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        FurnitureGameObjectMap = new Dictionary<Furniture, GameObject>();


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
                // Add a default sprite for empty tiles
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();
                tile_go.GetComponent<SpriteRenderer>().sprite = emptySprite;

                // Add callback to check for tile type changes
                tile_data.RegisterTileTypeChangedCallback(OnTileTypeChanged);

            }
        }

        // Center the camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);

        // World.RandomizeTiles();
    }

    //float randomizeTileTimer = 2f;

    void LoadSprites()
    {
        // Initialize sprite dictionary for installed objects
        FurnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Furnitures/");

        // Assign sprites by name to dictionary
        foreach (Sprite s in sprites)
        {
            FurnitureSprites[s.name] = s;
        }

    }
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

    public void OnFurnitureCreated( Furniture furn)
    {
        // FIXME: Multi-tile and rotated objects

        // Create a visual GameObject linked to this data
        GameObject furn_go = new GameObject();

        // Add tile/GO pair to the dictionary
        FurnitureGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.objectType + " " + furn.tile.X + "_ " + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X, furn.tile.Y, 0);
        furn_go.transform.SetParent(this.transform, true);


        // Add a sprite renderer
        furn_go.AddComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        // Put it on the right layer
        furn_go.GetComponent<SpriteRenderer>().sortingLayerName = "Furniture";

        // Add callback to check for object infor changes
        furn.RegisterOnChangedCallback(OnFurnitureChanged);

    }

    Sprite GetSpriteForFurniture(Furniture obj)
    {
        if(obj.linksToNeighbour == false)
        {
            return FurnitureSprites[obj.objectType];
        }

        // Otherwise sprite name is more complex
        string spriteName = obj.objectType + "_";

        int x = obj.tile.X;
        int y = obj.tile.Y;

        // Check for neighbours N, E, S, W
        Tile t;
        t = World.GetTileAt(x, y + 1);
        // Is there a neighbour matching our object type?
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
        {
            spriteName += "N";
            // Tell the neighbour to update
        }
        t = World.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
        {
            spriteName += "E";
        }
        t = World.GetTileAt(x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
        {
            spriteName += "S";
        }
        t = World.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
        {
            spriteName += "W";
        }

        if(FurnitureSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + spriteName);
        }

        return FurnitureSprites[spriteName];
    }

    void OnFurnitureChanged( Furniture furn )
    {
       

        // Ensure furniture graphics are correct

        if (FurnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged - Error in trying to change visuals for furniture map");
            return;
        }

        GameObject furn_go = FurnitureGameObjectMap[furn];
        // Instruct to look at neighbours and update graphics
        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

       

    }


}
