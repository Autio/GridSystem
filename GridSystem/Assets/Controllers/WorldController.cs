using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }
    public World World { get; protected set; }

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

        // Create gameobjects for each tile
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                GameObject tile_go = new GameObject();
                Tile tile_data = tile_data = World.GetTileAt(x, y);

                tile_go.name = "Tile_" + x + "_ " + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_go.transform.SetParent(this.transform, true);


                // Add a sprite renderer
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();

                tile_data.RegisterTileTypeChangedCallback( (tile) => { OnTileTypeChanged(tile, tile_go); } );

            }
        }
        World.RandomizeTiles();
    }

    float randomizeTileTimer = 2f;

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

    void OnTileTypeChanged(Tile tile_data, GameObject tile_go)
    {
        if(tile_data.Type == Tile.TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        } 
        else if (tile_data.Type == Tile.TileType.Empty)

        {
            tile_go.GetComponent<SpriteRenderer>().sprite = null;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged - Unrecognized tile type");
        }
    }


}
