﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture
{
    // This represents the base tile of the boject but large objects may occupy multiple ties
    public Tile tile
    {
        get; protected set;
    }

    // Queried by the visual system to know what sprite to render for this object
    public string objectType
    {
        get; protected set;
    }
    // 2 means you walk at half speed. Movement modifiers add up. So Fire + table + rough tile = 3 + 3 + 2 = 8.
    // If movementCost = 0 then this tile is impassable. 
    float movementCost;

    int width;
    int height;

    public bool linksToNeighbour
    {
        get; protected set;
    }

    Action<Furniture> cbOnChanged;

    Func<Tile, bool> funcPositionValidation;

    protected Furniture()
    {

    }

    // Used by our object factory to create the prototypical object
    static public Furniture CreatePrototype(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false )
    {
        Furniture obj = new Furniture();
        obj.objectType = objectType;
        obj.movementCost = movementCost;
        obj.width = width;
        obj.height = height;
        obj.linksToNeighbour = linksToNeighbour;

        obj.funcPositionValidation = obj.IsValidPosition; // override for special cases

        return obj;
    }

    static public Furniture PlaceInstance (Furniture proto, Tile tile)
    {
        if(proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance position validity function returned FALSE");
            return null;
        }
        Furniture obj = new Furniture();

        obj.objectType = proto.objectType;
        obj.movementCost = proto.movementCost;
        obj.width = proto.width;
        obj.height = proto.height;
        obj.linksToNeighbour = proto.linksToNeighbour;

        obj.tile = tile;

        // FIXME: Assumes we are 1x1

        if (tile.PlaceObject(obj) == false)
        {
            // Unable to place tile
            return null;
        }

        if(obj.linksToNeighbour)
        {
            // This type of furniture links itself to its neighbours
            // So we inform our neighbours that they have this neighbour
            // Trigger their OnChangedCallback

            Tile t;
            int x = tile.X;
            int y = tile.Y;

            t = tile.world.GetTileAt(x, y + 1);
            // Is there a neighbour matching our object type?
            if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
            {
                // Trigger callback with self
                // Have northern neighbour with same object type as us so 
                // Tell it that it has changed
                t.furniture.cbOnChanged(t.furniture);
                
            }
            t = tile.world.GetTileAt(x + 1, y);
            if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t. furniture);
            }
            t = tile.world.GetTileAt(x, y - 1);
            if (t != null && t. furniture != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t. furniture);
            }
            t = tile.world.GetTileAt(x - 1, y);
            if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
        }


        return obj;
 
 
    }

    // Inform the system when any part of the object changes
    public void RegisterOnChangedCallback(Action<Furniture> callbackFunc)
    {
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc)
    {
        cbOnChanged -= callbackFunc;
    }

    public bool IsValidPosition(Tile t)
    {
        // ensure the tile is Floor
        // Ensure there's no furniture
        if(t.Type != TileType.Floor)
        {
            return false;
        }

        if(t.furniture != null)
        {
            // Already have furniture
            return false;
        }

        return true;
    }

    public bool IsValidPosition_Door(Tile t)
    {
        // Ensure there's a pair of N/S or E/W walls
        if (IsValidPosition(t) == false)
        {
            return false;
        }
        return true;
    }
}
