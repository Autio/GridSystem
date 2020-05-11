using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager 
{
    // A list of all live inventories
    public List<Inventory> inventories;

    public InventoryManager()
    {
        inventories = new List<Inventory>();
    }

    public bool PlaceInventory(Tile tile, Inventory inv)
    {
        if(tile.PlaceInventory(inv) == false){
            // The tile did not accept the inventory, so stop
            return false;
        }

        // Inv might be an empty stack if it was merged to another stack
        if(inv.stackSize == 0)
        {
            inventories.Remove(inv);
        }

        // A new stack may be created on the tile


        inventories.Add(inv);
    }
}
