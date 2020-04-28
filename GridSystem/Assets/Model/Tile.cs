using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Should there be a type? 
public enum TileType { Empty, Floor, Space, Scaffolding, Metal, Wood };

public class Tile { 

    TileType _type = TileType.Empty;

    Action<Tile> cbTileTypeChanged;

    UninstalledObject uninstalledObject;

    public InstalledObject installedObject
    {
        get; protected set;
    }

    World world;
    int x;
    int y;

    public TileType Type {

        get => _type;
        set
        {
            TileType oldType = _type;
            _type = value;

            // Call the callback to make it clear things have changed

            if (cbTileTypeChanged != null && oldType != _type)
                cbTileTypeChanged(this);
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
        cbTileTypeChanged += callback;
    }

    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged -= callback;
    }

    public bool PlaceObject(InstalledObject objInstance)
    {
        if (objInstance == null)
        {
            // Uninstalling whatever was here before
            installedObject = null;
            return true;
        }

        if(installedObject != null)
        {
            Debug.LogError("Trying to assign an installed object to a tile that already has one");
            return false;
        }

        // Installing object
        installedObject = objInstance;
        return true;
    }
}
