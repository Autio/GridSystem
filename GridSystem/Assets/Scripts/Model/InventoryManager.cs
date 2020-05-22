using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager 
{
    // A dictionary of all live inventories
    public Dictionary<string, List<Inventory>> inventories;

    public InventoryManager()
    {
        inventories = new Dictionary<string, List<Inventory>>();
    }

    public bool PlaceInventory(Tile tile, Inventory inv)
    {
        bool tileWasEmpty = tile.inventory == null;

        if(tile.PlaceInventory(inv) == false){
            // The tile did not accept the inventory, so stop
            return false;
        }

        // Inv might be an empty stack if it was merged to another stack
        if(inv.stackSize == 0)
        {
            if (inventories.ContainsKey(tile.inventory.objectType))
            {
                inventories[inv.objectType].Remove(inv);
            }
        }

        // A new stack may be created on the tile
        if(tileWasEmpty)
        {
            if(inventories.ContainsKey(tile.inventory.objectType) == false)
            {
                inventories[tile.inventory.objectType] = new List<Inventory>();
            }
            inventories[tile.inventory.objectType].Add(tile.inventory);
        }

        return true;
    }

    public bool PlaceInventory(Job job, Inventory inv)
    {

        if (job.inventoryRequirements.ContainsKey(inv.objectType) == false)
        {
            Debug.LogError("Attempting to add inventory to a job that it does not want");
        }
        job.inventoryRequirements[inv.objectType].stackSize += inv.stackSize;

        if(job.inventoryRequirements[inv.objectType].maxStackSize > job.inventoryRequirements[inv.objectType].stackSize)
        {
            inv.stackSize = job.inventoryRequirements[inv.objectType].stackSize = job.inventoryRequirements[inv.objectType].maxStackSize;
            job.inventoryRequirements[inv.objectType].stackSize = job.inventoryRequirements[inv.objectType].maxStackSize;
        }
        else
        {
            inv.stackSize = 0;
        }
        
        // Inv might be an empty stack if it was merged to another stack
        if (inv.stackSize == 0)
        {
            if (inventories.ContainsKey(inv.objectType))
            {
                inventories[inv.objectType].Remove(inv);
            }
        }

        return true;
    }
}
