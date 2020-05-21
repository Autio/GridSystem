using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

// Should there be a type? 
public enum TileType { Empty, Floor, Space, Scaffolding, Metal, Wood };

public enum ENTERABILITY { Yes, Never, Soon }; 

public class Tile : IXmlSerializable { 

    TileType _type = TileType.Empty;

    Action<Tile> cbTileChanged;

    // Inventory
    public Inventory inventory { get; protected set; }
    
    public Room room;

    public Furniture furniture
    {
        get; protected set; 
    }

    public Job pendingFurnitureJob;
    public World world { get; protected set; }
    int x;
    int y;

    const float baseTileMovementCost = 1f;

    public float movementCost
    {
        get
        {
            if (Type == TileType.Empty)
            {
                return 0; // unwalkable
            }
            if (furniture == null)
            {
                return baseTileMovementCost; // what about other tile types?
            }
            return baseTileMovementCost * furniture.movementCost;
        }

        set
        {

        }
    }



    public TileType Type {

        get => _type;
        set
        {
            TileType oldType = _type;
            _type = value;

            // Call the callback to make it clear things have changed

            if (cbTileChanged != null && oldType != _type)
                cbTileChanged(this);
        }
    }
    public int X { get; protected set; }
    public int Y { get; protected set; }

    public Tile( World world, int x, int y )
    {
        this.world = world;
        this.X = x;
        this.Y = y;
    }

    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }

    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }

    public bool PlaceObject(Furniture objInstance)
    {
        if (objInstance == null)
        {
            // Uninstalling whatever was here before
            furniture = null;
            return true;
        }

        if(furniture != null)
        {
            Debug.LogError("Trying to assign an installed object to a tile that already has one");
            return false;
        }

        // Installing object
        furniture = objInstance;
        return true;
    }

    public bool PlaceInventory(Inventory inv)
    {
        if (inv == null)
        {
            inventory = null;
            return true;
        }

        if (inv != null)
        {
            if (inventory != null)
            {
                if (inventory.objectType != inv.objectType)
                {
                    Debug.LogError("Trying to assign inventory to a tile that already has some of a different type");
                    return false;
                }

                int numToMove = inv.stackSize;
                if (inventory.stackSize + numToMove > inventory.maxStackSize)
                {
                    numToMove = inventory.maxStackSize - inventory.stackSize;
                }

                inventory.stackSize += numToMove;
                inv.stackSize -= numToMove;

                return true;
            }
        }

        // Current inventory is null
        // The inventory manager needs to know that the old stack
        // Is now empty and has to be removed from previous lsits
        inventory = inv.Clone();
        inventory.tile = this;
        inv.stackSize = 0;

        return true;
    }

    // Are two tiles adjacent? 
    public bool IsNeighbour(Tile tile, bool diagOkay = false)
    {
        if(this.X == tile.X && (this.Y == tile.Y + 1 || this.Y == tile.Y-1))
            {
            return true;
        }
        if (this.Y == tile.Y && (this.X == tile.X + 1 || this.X == tile.X - 1))
            {
            return true;
        }

        if (diagOkay)
        {
            if (this.X == tile.X +1 && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1))
            {
                return true;
            }
            if (this.X == tile.X - 1 && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1))
            {
                return true;
            }

        }
        return false;
    }

    // return array of all neighbouring tiles
    // Is diagonal movement ok?
    // Is clipping corners ok?

    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] ns;
        if(diagOkay == false)
        {
            ns = new Tile[4]; // N E S W
        }
        else
        { 
            ns = new Tile[8]; // N E S W NE SE SW NW 
        }

        Tile n;

        // Cache for optimizing performance?

        n = world.GetTileAt(X, Y + 1);
        ns[0] = n; // Could be null
        n = world.GetTileAt(X + 1, Y);
        ns[1] = n; // Could be null
        n = world.GetTileAt(X, Y - 1);
        ns[2] = n; // Could be null
        n = world.GetTileAt(X - 1, Y);
        ns[3] = n; // Could be null

        if(diagOkay == true)
        {
            
            n = world.GetTileAt(X + 1, Y + 1);
            ns[4] = n; // Could be null
            n = world.GetTileAt(X + 1, Y - 1);
            ns[5] = n; // Could be null
            n = world.GetTileAt(X - 1, Y - 1);
            ns[6] = n; // Could be null
            n = world.GetTileAt(X - 1, Y + 1);
            ns[7] = n; // Could be null

        }

        return ns;
    }


    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader)
    {
    
        Type = (TileType)int.Parse(reader.GetAttribute("Type"));

    }

    public ENTERABILITY IsEnterable()
    {
        // True if you can enter this tile at this moment
        if(movementCost == 0)
        {
            return ENTERABILITY.Never;
        }

        // Check our furniture if there's a block on enterability

        if(furniture != null && furniture.IsEnterable != null)
        {
            return furniture.IsEnterable(furniture);
        }

        return ENTERABILITY.Yes;
    }

    public Tile North()
    {
        return world.GetTileAt(X, Y + 1);
    }
    public Tile East()
    {
        return world.GetTileAt(X + 1, Y);
    }
    public Tile South()
    {
        return world.GetTileAt(X, Y - 1);
    }
    public Tile West()
    {
        return world.GetTileAt(X - 1, Y);
    }
}
