using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph
{
    // Class constructs a path-finding compatible graph of our world. 
    // Each tile is a node. Each walkable neighbour from a tile is 
    // linked by edges

    public Dictionary<Tile, Path_Node<Tile>> nodes { get; protected set; }

    public Path_TileGraph(World world)
    {
        // Loop through all tiles of the world 
        // For each tile build a node
        // Do we create nodes for non-floor tiles? (Eventually, yes)
        // Unwalkable tiles don't get nodes

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < world.Width; x++)
            for (int y = 0; y < world.Height; y++)
            {
                Tile t = world.GetTileAt(x, y);

               // if (t.movementCost > 0) // Tiles with movement cost of 0 are unwalkable

               // {
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    nodes.Add(t, n);
               // }
            }

        Debug.Log("Path_TileGraph: Created " + nodes.Count + " nodes.");

        int edgeCount = 0;

        // Create the edges by looping through all nodes
        
        foreach(Tile t in nodes.Keys)
        {
            Path_Node<Tile> n = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();
            // Get neighbours for the tile
            Tile[] neighbours = t.GetNeighbours(true); // Diagonals okay. Some array entries can be null    
 
            // If the neighbour is walkable, create an edge to the relevant node
            for (int i = 0; i < neighbours.Length; i++)
            {
                if(neighbours[i] != null && neighbours[i].movementCost > 0)
                {
                    // The neighbour exists and is walkable, so create an edge. 

                    // But make sure we aren't clipping a diagonal
                    if(IsClippingCorner(t, neighbours[i]))
                    {
                        continue; // Skip to the next neighbour and do NOT build an edge.
                    }

                    Path_Edge<Tile> e = new Path_Edge<Tile>();
                    e.cost = neighbours[i].movementCost;
                    e.node = nodes[neighbours[i]];
                    edges.Add(e);

                    edgeCount++;
                }
            }

            n.edges = edges.ToArray();

        }
        Debug.Log("Path_TileGraph: Created " + edgeCount.ToString() + " edges.");


    }

    bool IsClippingCorner(Tile curr, Tile neighbour)
    {
        // When moving diagonally, check if we are clipping
        if(Mathf.Abs(curr.X - neighbour.X) + Mathf.Abs(curr.Y - neighbour.Y) == 2)
        {
            // Diagonal
            int dX = curr.X - neighbour.X;
            int dY = curr.Y - neighbour.Y;

            if(curr.world.GetTileAt(curr.X - dX, curr.Y).movementCost == 0)
            {
                // E or W is unwalkable, so movement would be clipped
                return true;
            }
            if (curr.world.GetTileAt(curr.X, curr.Y - dY).movementCost == 0)
            {
                // N or S is unwalkable, so movement would be clipped
                return true;
            }
        }
        return false;

    }

}
