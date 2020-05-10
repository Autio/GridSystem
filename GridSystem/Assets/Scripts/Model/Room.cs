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
            // This tile already exists in this room
            return;
        }

        if(t.room != null)
        {
            // Belongs to some other room
            t.room.tiles.Remove(t);
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

        // Try building new rooms for each NESW direction
        foreach(Tile t in sourceFurniture.tile.GetNeighbours())
        {
            FloodFill(t, oldRoom);
        }

        sourceFurniture.tile.room = null; // Walls don't belong to any room
        oldRoom.tiles.Remove(sourceFurniture.tile);

        // All tiles point to another room now
        // Force old room tiles list to be blank
        if(oldRoom != world.GetOutsideRoom())
        {
            // oldRoom should not have any more tiles in it 
            // DeleteRoom should need mostly to only need to 
            // Remove th eroom from the world list
            if (oldRoom.tiles.Count > 0)
            {
                Debug.LogError("'oldRoom' still has tiles assigned to it.");
            }
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

        if (tile.Type == TileType.Empty)
        {
            // This tile is empty space
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
                Tile[] neighbours = t.GetNeighbours();
                foreach (Tile t2 in neighbours) {
                    if(t2 == null || t2.Type == TileType.Empty)
                    {
                        // Open space or edge of map
                        // End the flood fill
                        // Delete this newRoom and re-assign all to the Outside
                        newRoom.UnassignAllTiles();
                        return;
                    }
                    if (t2 != null && t2.room == oldRoom && (t2.furniture == null || t2.furniture.roomEnclosure == false))
                    {
                        tilesToCheck.Enqueue(t2);
                    }
                   
                }
                
            }
        }

        // Tell the world that a new room has been formed
        tile.world.AddRoom(newRoom);
    }

}
 