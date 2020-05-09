using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room 
{
    float atmos02;

    List<Tile> tiles;

    public Room()
    {
        tiles = new List<Tile>();
    }

    public void AssignTile(Tile t)
    {
        t.room = this;
        if(tiles.Contains(t))
        {
            return;
        }
        t.room = this;
        tiles.Add(t);
    }

    public void UnassignAllTiles()
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            tiles[i].room = tiles[i].world.GetOutsideRoom();
        }
        tiles = new List<Tile>();
    }

    public static void DoRoomFloodFill(Furniture sourceFurniture)
    {
        // sourceFurniture may split two existing rooms
        // Or enclose a place to form a new room 
        // Check NESW neighbours of the tile and do a flood fill from them

        World world = sourceFurniture.tile.world;

        if(sourceFurniture.tile.room != world.GetOutsideRoom())
        {
            world.DeleteRoom(sourceFurniture.tile.room);)
        }
        sourceFurniture.tile.room.UnassignAllTiles();
    }


}
 