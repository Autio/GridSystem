using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World
{
    Tile[,] tiles;

    Dictionary<string, InstalledObject> installedObjectPrototypes;

    int width, height;
    public int Width { get => width; }
    public int Height { get => height; }

    Action<InstalledObject> cbInstalledObjectCreated;

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
            }
        }

        Debug.Log("World created with " + (width * height) + " tiles");


        CreateInstalledObjectPrototypes();
        // TODO: Implement larger objects
        // TODO: Implement object rotation
    }

    void CreateInstalledObjectPrototypes()
    {
        installedObjectPrototypes = new Dictionary<string, InstalledObject>();

        installedObjectPrototypes.Add("Wall",
            InstalledObject.CreatePrototype(
            "Wall",
            0,      // Impassable
            1,
            1,
            true    // links to neighbour
            )
        );


    }

    InstalledObject CreateOneInstalledObjectPrototype()
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

    public void PlaceInstalledObject(string objectType, Tile t)
    {
        // FIXME: Assumes 1x1 tiles, change later
        if (installedObjectPrototypes.ContainsKey(objectType) == false)
            {
            Debug.LogError("installedObjectPrototypes doesn't contain a proto for the key: " + objectType);

        }

        InstalledObject obj = InstalledObject.PlaceInstance(installedObjectPrototypes[objectType], t);
        if(obj == null)
        {
            // Failed to place object. Probably something there already
            return;
        }

        if(cbInstalledObjectCreated != null)
        {
            cbInstalledObjectCreated(obj);
        }


    }

    public void RegisterInstalledObjectCreated(Action<InstalledObject> callbackfunc)
    {
        cbInstalledObjectCreated += callbackfunc;
    }


    public void UnregisterInstalledObjectCreated(Action<InstalledObject> callbackfunc)
    {
        cbInstalledObjectCreated -= callbackfunc;
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
