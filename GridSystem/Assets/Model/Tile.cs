using System;
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
}
