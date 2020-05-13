using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Uninstalled objects are things on floors such as items or non-installed furniture

public class Inventory
{
    public string objectType = "Steel Plate";
    public int maxStackSize = 50;
    public int stackSize = 1;

    public Inventory()
    {

    }

    protected Inventory(Inventory other)
    {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        stackSize = other.stackSize;
    }

    public virtual Inventory Clone()
    {
        return new Inventory(this);
    }
}
