﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Should there be a type? 
public enum TileType { Empty, Floor, Space, Scaffolding, Metal, Wood };

public class Tile { 

    TileType _type = TileType.Empty;

    Action<Tile> cbTileChanged;

    UninstalledObject uninstalledObject;

    public Furniture furniture
    {
        get; protected set;
    }

    public Job pendingFurnitureJob;

    public World world { get; protected set; }
    int x;
    int y;

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
                return 1; // what about other tile types?
            }
            return 1 * furniture.movementCost;
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
    public int X { get => x; }
    public int Y { get => y; }

    public Tile( World world, int x, int y )
    {
        this.world = world;
        this.x = x;
        this.y = y;
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
}