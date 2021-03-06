﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Furniture
{
    protected Dictionary<string, float> furnitureParameters;
    /// <summary>
    ///  Actions called every update. 
    ///  Pass the furniture they belong to and the deltaTime
    /// </summary>
    protected Action<Furniture, float> updateActions;

    public Func<Furniture, ENTERABILITY> IsEnterable;

    public void Update(float deltaTime)
    {
        if (updateActions != null)
        {
            updateActions(this, deltaTime);
        }
    }



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
    public float movementCost { get; protected set; }
    public bool roomEnclosure; // Is the room closed off from the rest of the map?

    int width;
    int height;

    public bool linksToNeighbour
    {
        get; protected set;
    }

    public Action<Furniture> cbOnChanged;

    public Func<Tile, bool> funcPositionValidation;

    public Furniture()
    {
        furnitureParameters = new Dictionary<string, float>();
    }

    // Copy Constructor
    protected Furniture(Furniture other)
    {
        this.objectType = other.objectType;
        this.movementCost = other.movementCost;
        this.roomEnclosure = other.roomEnclosure;
        this.width = other.width;
        this.height = other.height;
        this.linksToNeighbour = other.linksToNeighbour;

        furnitureParameters = new Dictionary<string, float>(other.furnitureParameters);
        if (other.updateActions != null)
        {
            this.updateActions = (Action<Furniture, float>)other.updateActions.Clone();
        }

        this.IsEnterable = other.IsEnterable;
    }

    virtual public Furniture Clone( )
    {
        return new Furniture(this);
    }

    // Used by our object factory to create the prototypical object
    public Furniture(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false )
    {
      
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.roomEnclosure = roomEnclosure;
        this.width = width;
        this.height = height;
        this.linksToNeighbour = linksToNeighbour;

        this.funcPositionValidation = this.DEFAULT__IsValidPosition; // override for special cases

        this.furnitureParameters = new Dictionary<string, float>();

    }

    static public Furniture PlaceInstance (Furniture proto, Tile tile)
    {
        if(proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance position validity function returned FALSE");
            return null;
        }
        Furniture obj = proto.Clone();
    
        obj.tile = tile;

        // FIXME: Assumes we are 1x1

        if (tile.PlaceObject(obj) == false)
        {
            // Unable to place furniture on this tile
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
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                // Trigger callback with self
                // Have northern neighbour with same object type as us so 
                // Tell it that it has changed
                t.furniture.cbOnChanged(t.furniture);
                
            }
            t = tile.world.GetTileAt(x + 1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t. furniture);
            }
            t = tile.world.GetTileAt(x, y - 1);
            if (t != null && t. furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t. furniture);
            }
            t = tile.world.GetTileAt(x - 1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
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
        return funcPositionValidation(t);
    }

    // TODO: Validation checks coming from furniture scripts
    protected bool DEFAULT__IsValidPosition(Tile t)
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

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("objectType", objectType);
        //writer.WriteAttributeString("movementCost", movementCost.ToString());

        foreach (string k in furnitureParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", furnitureParameters[k].ToString());
            writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader reader)
    {
        //objectType = reader.GetAttribute("objectType");
        // X, Y and objecType have already been set
        //movementCost = int.Parse(reader.GetAttribute("movementCost"));

        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
                furnitureParameters[k] = v;
            } while 
            (reader.ReadToNextSibling("Param"));
            
        }
    }

    public float GetParameter(string key, float default_value = 0)
    {
        if (furnitureParameters.ContainsKey(key) == false)
        {
            return default_value;
        }
        return furnitureParameters[key];
    }

    public void SetParameter(string key, float value)
    {

        furnitureParameters[key] = value;
    }

    public void ChangeParameter(string key, float value)
    {
        if (furnitureParameters.ContainsKey(key) == false)
        {
            furnitureParameters[key] = value;
        } else
        {
            furnitureParameters[key] += value;

        }
    }
    /// <summary>
    /// Register a function that will be called every Update
    /// </summary>
    /// <param name="a"></param>
    public void RegisterUpdateAction(Action<Furniture, float> a)
    {
        updateActions += a;
    }
    public void UnregisterUpdateAction(Action<Furniture, float> a)
    {
        updateActions -= a;
    }
}
