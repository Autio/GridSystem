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

        Room oldRoom = sourceFurniture.tile.room;

        // Try building a new room starting from the north  
        FloodFill(sourceFurniture.tile.North(), oldRoom);
        FloodFill(sourceFurniture.tile.East(),  oldRoom);
        FloodFill(sourceFurniture.tile.South(), oldRoom);
        FloodFill(sourceFurniture.tile.West(),  oldRoom);



        // All tiles point to another room now
        // Force old room tiles list to be blank
        oldRoom.tiles = new List<Tile>(); 
        if(oldRoom != world.GetOutsideRoom())
        {
            world.DeleteRoom(oldRoom, false);
        }
    }

    protected static void FloodFill(Tile tile, Room oldRoom)
    {
        if(tile == null)
        {
            // Flood fill attempt off the map, so just return
            return;
        }

        if(tile.room != oldRoom)
        {
            // Already assigned to another new room 
            // so the direction is not isolated
        }

        if(tile.furniture != null && tile.furniture.roomEnclosure)
        {
            // Tile has a wall/door etc so can't be a room
            return;
        }

        // We need to create a new room

        Room newRoom = new Room();
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(tile);

        while(tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();
            if(t.room == oldRoom)
            {
                newRoom.AssignTile(t);
            }
        }
    }

}
 