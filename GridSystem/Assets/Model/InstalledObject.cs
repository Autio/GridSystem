using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObject
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

    Action<InstalledObject> cbOnChanged;

    protected InstalledObject()
    {

    }

    // Used by our object factory to create the prototypical object
    static public InstalledObject CreatePrototype(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false )
    {
        InstalledObject obj = new InstalledObject();
        obj.objectType = objectType;
        obj.movementCost = movementCost;
        obj.width = width;
        obj.height = height;
        obj.linksToNeighbour = linksToNeighbour;

        return obj;
    }

    static public InstalledObject PlaceInstance (InstalledObject proto, Tile tile)
    {
        InstalledObject obj = new InstalledObject();

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
        else
        {
            return obj;
        }
    }

    // Inform the system when any part of the object changes
    public void RegisterOnChangedCallback(Action<InstalledObject> callbackFunc)
    {
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback(Action<InstalledObject> callbackFunc)
    {
        cbOnChanged -= callbackFunc;
    }
}
