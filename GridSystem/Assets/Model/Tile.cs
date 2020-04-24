using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile { 

    // Should there be a type? 
    public enum TileType { Empty, Floor, Space, Scaffolding, Metal, Wood };

    TileType type = TileType.Empty;

    UninstalledObject uninstalledObject;
    InstalledObject installedObject;

    World world;
    int x;
    int y;

    public TileType Type { get => type; set => type = value; }
    public int X { get => x; }
    public int Y { get => y; }

    public Tile( World world, int x, int y )
    {
        this.world = world;
        this.x = x;
        this.y = y;
    }


}
