using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World
{
    Tile[,] tiles;
    List<Character> characters;
    
    // Pathfinding graph used to navigate world map
    Path_TileGraph tileGraph;

    Dictionary<string, Furniture> furniturePrototypes;

    int width, height;
    public int Width { get => width; }
    public int Height { get => height; }

    Action<Furniture> cbFurnitureCreated;
    Action<Tile> cbTileChanged;
    Action<Character> cbCharacterCreated;

    // Queues are like arrays but you only put stuff at the end and take it from front
    // TODO: Replace with a dedicated class for managing job queues
    public JobQueue jobQueue;

    public World(int width = 100, int height = 100)
    {
        jobQueue = new JobQueue();
        
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

        characters = new List<Character>();

        // TODO: Implement larger objects
        // TODO: Implement object rotation
    }

    public void Update(float deltaTime)
    {
        foreach(Character c in characters)
        {
            // Can adjust system time here for game speed-up etc 
            c.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);
        characters.Add(c);

        if(cbCharacterCreated != null)
            cbCharacterCreated(c);

        return c;
    }

    void CreateFurniturePrototypes()
    {
        furniturePrototypes = new Dictionary<string, Furniture>();

        furniturePrototypes.Add("Wall",
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

    // Testing script for pathfinding
    public void SetupPathfindingExample()
    {
        Debug.Log("SetupPathfindingExample");

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int y = b-5; y < b + 15; y++)
            {
                tiles[x, y].Type = TileType.Floor;

                if (x == l || x == (l + 9) || y == b || y == (b + 9))
                {
                    if ( x != (l + 9) && y != (b + 4))
                    {
                        PlaceFurniture("Wall", tiles[x, y]);
                    }
                }
            }
        }
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
        if (furniturePrototypes.ContainsKey(objectType) == false)
            {
            Debug.LogError("FurniturePrototypes doesn't contain a proto for the key: " + objectType);

        }

        Furniture obj = Furniture.PlaceInstance(furniturePrototypes[objectType], t);
        if(obj == null)
        {
            // Failed to place object. Probably something there already
            return;
        }

        if(cbFurnitureCreated != null)
        {
            cbFurnitureCreated(obj);
            InvalidateTileGraph();
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

    public void RegisterCharacterCreated(Action<Character> callbackfunc)
    {
        cbCharacterCreated += callbackfunc;
    }


    public void UnregisterCharacterCreated(Action<Character> callbackfunc)
    {
        cbCharacterCreated -= callbackfunc;
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

        InvalidateTileGraph();
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

    // Call whenever a change to the world makes old pathfinding info invalid

    // E.g. build wall, add furniture
    public void InvalidateTileGraph()
    {
        tileGraph = null;   
    }

    public bool IsFurniturePlacementValid(string furnitureType, Tile t)
    {
        return furniturePrototypes[furnitureType].IsValidPosition(t);
        
    }

    public Furniture GetFurniturePrototype(string objectType)
    {
        if(furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("No furniture of type " + objectType);
            return null;
        }
        return furniturePrototypes[objectType];
        
    }

}
