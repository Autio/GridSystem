using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World
{
    Tile[,] tiles;

    Dictionary<string, Furniture> FurniturePrototypes;

    int width, height;
    public int Width { get => width; }
    public int Height { get => height; }

    Action<Furniture> cbFurnitureCreated;
    Action<Tile> cbTileChanged;

    public World(int width = 100, int height = 100)
    {
        this.width = width;
        this.height = height;

        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].RegisterTileTypeChangedCallback( OnTileChanged );
            }
        }

        Debug.Log("World created with " + (width * height) + " tiles");


        CreateFurniturePrototypes();
        // TODO: Implement larger objects
        // TODO: Implement object rotation
    }

    void CreateFurniturePrototypes()
    {
        FurniturePrototypes = new Dictionary<string, Furniture>();

        FurniturePrototypes.Add("Wall",
            Furniture.CreatePrototype(
            "Wall",
            0,      // Impassable
            1,
            1,
            true    // links to neighbour
            )
        );


    }

    Furniture CreateOneFurniturePrototype()
    {
        return null;
    }

    public Tile GetTileAt(int x, int y)
    {
        if ( x > Width || x < 0 )
        {
            Debug.LogError("Tile at x " + x + " is out of range");
            return null;
        }
        if (y > height || y < 0)
        {
            Debug.LogError("Tile at y " + y + " is out of range");
            return null;
        }

        return tiles[x, y];
    }

    public void PlaceFurniture(string objectType, Tile t)
    {
        // FIXME: Assumes 1x1 tiles, change later
        if (FurniturePrototypes.ContainsKey(objectType) == false)
            {
            Debug.LogError("FurniturePrototypes doesn't contain a proto for the key: " + objectType);

        }

        Furniture obj = Furniture.PlaceInstance(FurniturePrototypes[objectType], t);
        if(obj == null)
        {
            // Failed to place object. Probably something there already
            return;
        }

        if(cbFurnitureCreated != null)
        {
            cbFurnitureCreated(obj);
        }


    }

    public void RegisterFurnitureCreated(Action<Furniture> callbackfunc)
    {
        cbFurnitureCreated += callbackfunc;
    }


    public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc)
    {
        cbFurnitureCreated -= callbackfunc;
    }

    public void RegisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged += callbackfunc;
    }


    public void UnregisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged -= callbackfunc;
    }

    void OnTileChanged(Tile t)
    {
        if(cbTileChanged == null)
        {
            return;
        }
        cbTileChanged(t);
    }

    // Initialize when being looked at
    public Tile GetTileAtOnLook(int x, int y)
    {
        if(tiles[x,y] == null)
        {
            tiles[x, y] = new Tile(this, x, y);
        }
        return tiles[x, y];
    }

    public void RandomizeTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    tiles[x, y].Type = TileType.Empty;
                }
                else
                {
                    tiles[x, y].Type = TileType.Floor;
                }
            }
        }

    }

}
