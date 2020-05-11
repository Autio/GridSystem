using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureSpriteController : MonoBehaviour
{
       
    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    Dictionary<string, Sprite> furnitureSprites;

    World world
    {
        get { return WorldController.Instance.world; }
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        world.RegisterFurnitureCreated(OnFurnitureCreated);

        // Go through existing furniture from a save which was loaded OnEnable and 
        // Run the callback manually
        foreach(Furniture f in world.furnitures)
        {
            OnFurnitureCreated(f);
        }

    }

    //float randomizeTileTimer = 2f;

    void LoadSprites()
    {
        // Initialize sprite dictionary for installed objects
        furnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Furnitures/");

        // Assign sprites by name to dictionary
        foreach (Sprite s in sprites)
        {
            furnitureSprites[s.name] = s;
        }
    }
    
    public void OnFurnitureCreated( Furniture furn)
    {
        // FIXME: Multi-tile and rotated objects

        // Create a visual GameObject linked to this data
        GameObject furn_go = new GameObject();

  
        // Add tile/GO pair to the dictionary
        furnitureGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.objectType + " " + furn.tile.X + "_ " + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X, furn.tile.Y, 0);
        furn_go.transform.SetParent(this.transform, true);

        // FIXME
        if (furn.objectType == "Door")
        {
            // By default door sprite is for N/S 
            // Check to see if there's a wall E/W, in which case rotate 90 degrees
            Tile eastTile = world.GetTileAt(furn.tile.X + 1, furn.tile.Y);
            Tile westTile = world.GetTileAt(furn.tile.X - 1, furn.tile.Y);

            if (eastTile != null && westTile != null && eastTile.furniture != null && westTile.furniture != null &&
                eastTile.furniture.objectType == "Wall" && westTile.furniture.objectType == "Wall")
            {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
                furn_go.transform.Translate(1f, 0, 0, Space.World); // hack and fix
            }
        }

        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furn);
        sr.sortingLayerName = "Furniture";
        

        // Add callback to check for object infor changes
        furn.RegisterOnChangedCallback(OnFurnitureChanged);

    }

    void OnFurnitureChanged(Furniture f)
    {

        // Ensure furniture graphics are correct

        if (furnitureGameObjectMap.ContainsKey(f) == false)
        {
            Debug.LogError("OnFurnitureChanged - Error in trying to change visuals for furniture map");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[f];
        // Instruct to look at neighbours and update graphics
        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(f);

    }

    public Sprite GetSpriteForFurniture(Furniture obj)
    {
        string spriteName = obj.objectType;
        if (obj.linksToNeighbour == false)
        {
            // If this is a door then let's check openness
            if (obj.objectType == "Door")
            {
                // Cycle through options of openness
                for (int i = 1; i <= 8; i++)
                {
                    if (obj.GetParameter("openness") <= 1f / (float)i)
                    {
                        Debug.Log("Door open stage: " + obj.GetParameter("openness"));
                        spriteName = "Door_" + (i-1).ToString();

                    }
                }

            }
            return furnitureSprites[spriteName];
        }

        // Otherwise sprite name is more complex
        spriteName = obj.objectType + "_";

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

        if(furnitureSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + spriteName);
        }

        Debug.Log("Placing furniture");
     


        return furnitureSprites[spriteName];
    }

    public Sprite GetSpriteForFurniture(string objectType)
    {
        if (furnitureSprites.ContainsKey(objectType))
        {
            return furnitureSprites[objectType]; 
        }
        if (furnitureSprites.ContainsKey(objectType + "_"))
        {
            return furnitureSprites[objectType + "_"];
        }

        Debug.LogError("GetSpriteForFurniture. No sprites with the name: " + objectType);
        return null;

    }


}
