﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable
{
    Tile[,] tiles;
    public List<Character> characters;
    public List<Furniture> furnitures;
    public List<Room> rooms;
    public InventoryManager inventoryManager;

    // Pathfinding graph used to navigate world map
    public Path_TileGraph tileGraph { get; set; }

    Dictionary<string, Furniture> furniturePrototypes;
    public Dictionary<string, Job> furnitureJobPrototypes;

    public int Width { get; protected set; }
    public int Height { get; protected set; }

    Action<Furniture> cbFurnitureCreated;
    Action<Tile> cbTileChanged;
    Action<Character> cbCharacterCreated;
    Action<Inventory> cbInventoryCreated;


    // Queues are like arrays but you only put stuff at the end and take it from front
    // TODO: Replace with a dedicated class for managing job queues
    public JobQueue jobQueue;

public World(int width, int height) {
        // Creates an empty world
        SetupWorld(width, height);

        // Make one character
        Character c = CreateCharacter(GetTileAt(Width / 2, Height / 2));

    }

    /// <summary>
    /// Default constructor used when loading a world from a file
    /// </summary>

    public World()
    {

    }

    public Room GetOutsideRoom()
    {
        return rooms[0];
    }

    public void AddRoom(Room r)
    {
        rooms.Add(r);
    }

    public void DeleteRoom(Room r, bool skipUnassignment)
    {
        if(r == GetOutsideRoom())
        {
            Debug.LogError("Trying to delete the outside room");
            return;
        }
        rooms.Remove(r);
    }

    void SetupWorld(int width, int height) {
		jobQueue = new JobQueue();

		Width = width;
		Height = height;

        rooms = new List<Room>();
        rooms.Add(new Room()); // Create the outside 

        tiles = new Tile[Width,Height];

		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				tiles[x,y] = new Tile(this, x, y);
				tiles[x,y].RegisterTileTypeChangedCallback( OnTileChanged );
                tiles[x, y].room = rooms[0]; // Rooms[0] is always going to be outside
			}
		}

		Debug.Log ("World created with " + (Width*Height) + " tiles.");

		CreateFurniturePrototypes();

		characters = new List<Character>();
        furnitures = new List<Furniture>();
        inventoryManager = new InventoryManager();

	}

    public void Update(float deltaTime)
    {
        foreach(Character c in characters)
        {
            // Can adjust system time here for game speed-up etc 
            c.Update(deltaTime);
        }

        foreach (Furniture f in furnitures)
        {
            // Can adjust system time here for game speed-up etc 
            f.Update(deltaTime);
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
        furnitureJobPrototypes = new Dictionary<string, Job>();

        furniturePrototypes.Add("Wall",
            new Furniture(
                "Wall",
                0,      // Impassable
                1,      // Width
                1,      // Height
                true,   // links to neighbour
                true    // Walls enclose rooms
            )
        );
        furnitureJobPrototypes.Add("Wall", new Job(null, "Wall", FurnitureActions.JobComplete_FurnitureBuilding, 1f, new Inventory[] { new Inventory("Steel Plate", 5, 0) }));

        furniturePrototypes.Add("Door",
            new Furniture(
                "Door",
                1,      // Open
                1,
                1,
                false,  // links to neighbour
                true    // Walls enclose rooms
            )
        );

        // Scriptable object behaviours
        furniturePrototypes["Door"].SetParameter("openness",0);
        furniturePrototypes["Door"].SetParameter("is_opening", 0);

        furniturePrototypes["Door"].RegisterUpdateAction(FurnitureActions.Door_UpdateAction);

        furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;

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
        if ( x >= Width || x < 0 )
        {
          // Debug.LogError("Tile at x " + x + " is out of range");
            return null;
        }
        if (y >= Height || y < 0)
        {
          //   Debug.LogError("Tile at y " + y + " is out of range");
            return null;
        }

        return tiles[x, y];
    }

    public Furniture PlaceFurniture(string objectType, Tile t)
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
            // TODO: Handling deletion of existing furniture
            return null;
        }

        furnitures.Add(obj);
        if(obj.roomEnclosure)
        {
            Room.DoRoomFloodFill(obj);
        }

        if(cbFurnitureCreated != null)
        {
            cbFurnitureCreated(obj);
            if (obj.movementCost != 1)
            {
                InvalidateTileGraph();

            }
        }

        return obj;


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


    public void RegisterInventoryCreated(Action<Inventory> callbackfunc)
    {
        cbInventoryCreated += callbackfunc;
    }


    public void UnregisterInventoryCreated(Action<Inventory> callbackfunc)
    {
        cbInventoryCreated -= callbackfunc;
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
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
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

    ///////////////////////////////////////////////////////////////////////////
    ///
    ///                         SAVING AND LOADING
    ///
    ///////////////////////////////////////////////////////////////////////////



    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // Save info
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (tiles[x, y].Type != TileType.Empty)
                {
                    writer.WriteStartElement("Tile");
                    tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Furnitures");
        foreach (Furniture f in furnitures)
        {
            writer.WriteStartElement("Furniture");
            f.WriteXml(writer);
            writer.WriteEndElement();
        }
            
        
        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();
        }


        writer.WriteEndElement();
        ////writer.WriteStartElement("Width");
        ////writer.WriteValue(width);
        ////writer.WriteEndElement();

    }
    public void ReadXml(XmlReader reader)
    {
        // Load info

      
        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));


        SetupWorld(Width, Height);

        while(reader.Read())
        {
            switch(reader.Name)
            { 
                case "Tiles":
                 
                ReadXml_Tiles(reader);
                    
                break;

                case "Furnitures":

                ReadXml_Furnitures(reader);

                break;

                case "Characters":

                ReadXml_Characters(reader);

                break;
            }
        }


        // DEBUGGING ONLY
        // TODO: Remove
        // Create an inventory item
        Inventory inv = new Inventory();
        inv.stackSize = 10;
        Tile t = GetTileAt(Width / 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory();
        inv.stackSize = 18;
        t = GetTileAt(Width / 2 + 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory();
        inv.stackSize = 45;
        t = GetTileAt(Width / 2 + 1, Height / 2 + 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }
    }

    void ReadXml_Tiles(XmlReader reader)
    {
        
        if(reader.ReadToDescendant("Tile"))
        {
            // There's at least one tile to save

            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                tiles[x, y].ReadXml(reader);

            } while (reader.ReadToNextSibling("Tile"));
        }

    }

    void ReadXml_Furnitures(XmlReader reader)
    {
        //reader.ReadToDescendant("Tiles");
        // reader.ReadToDescendant("Tile");
        if (reader.ReadToDescendant("Furniture"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Furniture f = PlaceFurniture(reader.GetAttribute("objectType"), tiles[x, y]);
                f.ReadXml(reader);
            }
            while (reader.ReadToNextSibling("Furniture"));
        }


    }


    void ReadXml_Characters(XmlReader reader)
    {
        //reader.ReadToDescendant("Tiles");
        // reader.ReadToDescendant("Tile");
        if (reader.ReadToDescendant("Character"))        
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));

                    Character c = CreateCharacter(tiles[x, y]);
                    c.ReadXml(reader);

                }

                while (reader.ReadToNextSibling("Character"));
            }
    }




}
