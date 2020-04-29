using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureSpriteController : MonoBehaviour
{
       
    Dictionary<Furniture, GameObject> FurnitureGameObjectMap;
    Dictionary<string, Sprite> FurnitureSprites;

    World world
    {
        get { return WorldController.Instance.world; }
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        FurnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        world.RegisterFurnitureCreated(OnFurnitureCreated);

    }

    //float randomizeTileTimer = 2f;

    void LoadSprites()
    {
        // Initialize sprite dictionary for installed objects
        FurnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Furnitures/");

        // Assign sprites by name to dictionary
        foreach (Sprite s in sprites)
        {
            FurnitureSprites[s.name] = s;
        }

    }
    
    public void OnFurnitureCreated( Furniture furn)
    {
        // FIXME: Multi-tile and rotated objects

        // Create a visual GameObject linked to this data
        GameObject furn_go = new GameObject();

        // Add tile/GO pair to the dictionary
        FurnitureGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.objectType + " " + furn.tile.X + "_ " + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X, furn.tile.Y, 0);
        furn_go.transform.SetParent(this.transform, true);


        // Add a sprite renderer
        furn_go.AddComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        // Put it on the right layer
        furn_go.GetComponent<SpriteRenderer>().sortingLayerName = "Furniture";

        // Add callback to check for object infor changes
        furn.RegisterOnChangedCallback(OnFurnitureChanged);

    }

    public Sprite GetSpriteForFurniture(Furniture obj)
    {
        if(obj.linksToNeighbour == false)
        {
            return FurnitureSprites[obj.objectType];
        }

        // Otherwise sprite name is more complex
        string spriteName = obj.objectType + "_";

        int x = obj.tile.X;
        int y = obj.tile.Y;

        // Check for neighbours N, E, S, W
        Tile t;
        t = world.GetTileAt(x, y + 1);
        // Is there a neighbour matching our object type?
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
        {
            spriteName += "N";
            // Tell the neighbour to update
        }
        t = world.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
        {
            spriteName += "E";
        }
        t = world.GetTileAt(x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
        {
            spriteName += "S";
        }
        t = world.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
        {
            spriteName += "W";
        }

        if(FurnitureSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + spriteName);
        }

        return FurnitureSprites[spriteName];
    }

    void OnFurnitureChanged( Furniture furn )
    {
       
        // Ensure furniture graphics are correct

        if (FurnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged - Error in trying to change visuals for furniture map");
            return;
        }

        GameObject furn_go = FurnitureGameObjectMap[furn];
        // Instruct to look at neighbours and update graphics
        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);   

    }

    public Sprite GetSpriteForFurniture(string objectType)
    {
        if (FurnitureSprites.ContainsKey(objectType))
        {
            return FurnitureSprites[objectType]; 
        }
        if (FurnitureSprites.ContainsKey(objectType + "_"))
        {
            return FurnitureSprites[objectType + "_"];
        }

        Debug.LogError("GetSpriteForFurniture. No sprites with the name: " + objectType);
        return null;

    }


}
