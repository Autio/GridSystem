﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class InventorySpriteController : MonoBehaviour
{
    public GameObject inventoryUIPrefab;

    Dictionary<Inventory, GameObject> inventoryGameObjectMap;
    Dictionary<string, Sprite> inventorySprites;
    World world
    {
        get { return WorldController.Instance.world; }
    }


    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        world.RegisterInventoryCreated(OnInventoryCreated);

       // Check for pre-existing characters
       foreach(string objectType in world.inventoryManager.inventories.Keys)
        {
            foreach(Inventory inv in world.inventoryManager.inventories[objectType])
            {
                OnInventoryCreated(inv);

            }
        }
    }

    void LoadSprites()
    {
        // Initialize sprite dictionary for installed objects
        inventorySprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Inventory/");

        // Assign sprites by name to dictionary
        foreach (Sprite s in sprites)
        {
            inventorySprites[s.name] = s;
        }

    }

    public void OnInventoryCreated(Inventory inventory)
    {
        // FIXME: Multi-tile and rotated objects
        Debug.Log("OnInventoryCreated");
        // Create a visual GameObject linked to this data
        GameObject inventory_go = new GameObject();

        // Add tile/GO pair to the dictionary
        inventoryGameObjectMap.Add(inventory, inventory_go);

        inventory_go.name = inventory.objectType;
        inventory_go.transform.position = new Vector3(inventory.tile.X, inventory.tile.Y, 0);
        inventory_go.transform.SetParent(this.transform, true);


        // Add a sprite renderer
        SpriteRenderer sr = inventory_go.AddComponent<SpriteRenderer>();
        sr.sprite = inventorySprites[inventory.objectType];
        // Put it on the right layer
        sr.sortingLayerName = "Inventory";

        if(inventory.maxStackSize > 1)
        {
            // Stackable object, show UI element to indicate number of items
            GameObject ui_go = Instantiate(inventoryUIPrefab);
            ui_go.transform.SetParent(inventory_go.transform);
            ui_go.transform.localPosition = Vector3.zero;
            GameObject textObject = ui_go.transform.Find("InventoryTextMesh").gameObject;
            textObject.GetComponent<TMP_Text>().text = inventory.stackSize.ToString();
        }

        // FIXME: Add on changed callback
        //inventory.RegisterOnChangedCallback(OnCharacterChanged);

    }

    void OnInventoryChanged(Inventory inventory)
    {
        if (inventoryGameObjectMap.ContainsKey(inventory) == false)
        {
            Debug.LogError("OnInventoryChanged. Trying to change visuals for a character not in our dictionary");
        }

        GameObject inventory_go = inventoryGameObjectMap[inventory];
        inventory_go.transform.position = new Vector3(inventory.tile.X, inventory.tile.Y, 0);
             
        
    }

}
