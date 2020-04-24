using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public Sprite floorSprite;
    World world;

    // Start is called before the first frame update
    void Start()
    {
        // Create blank world
        world = new World();

        // Create gameobjects for each tile
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                GameObject tile_go = new GameObject();
                Tile tile_data = tile_data = world.GetTileAt(x, y);

                tile_go.name = "Tile_" + x + "_ " + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);

                // Add a sprite renderer
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();


            }
        }
        world.RandomizeTiles();




    }

    // Update is called once per frame
    void Update()
    {
        
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
