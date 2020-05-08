using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

public class Path_AStar 
{
    Queue<Tile> path;

    public Path_AStar(World world, Tile tileStart, Tile tileEnd)
    {
        // Check if we have a valid tile graph
        if(world.tileGraph == null)
        {
            world.tileGraph = new Path_TileGraph(world);
        }

        // Dict of all valid, walkable nodes
        Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;

        // Ensure start and end tiles are possible
        if (nodes.ContainsKey(tileStart) == false)
        {
            Debug.LogError("Path_AStar: The starting tile isn't in the list of nodes");
            return;
        }
        if (nodes.ContainsKey(tileEnd) == false)
        {
            Debug.LogError("Path_AStar: The ending tile isn't in the list of nodes");
            return;
        }


        Path_Node<Tile> start = nodes[tileStart];
        Path_Node<Tile> goal = nodes[tileEnd];


        // https://en.wikipedia.org/wiki/A*_search_algorithm
        List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>>();

        //List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();
        //OpenSet.Add(start);

        // Priority queue
        SimplePriorityQueue<Path_Node<Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>>();
        OpenSet.Enqueue(start, 0);

        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();
        foreach(Path_Node<Tile> n in nodes.Values)
        {
            g_score[n] = Mathf.Infinity;
        }

        // Starting tile is free
        g_score[start] = 0;

        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        foreach (Path_Node<Tile> n in nodes.Values)
        {
            f_score[n] = Mathf.Infinity;
        }
        f_score[start] = heuristic_cost_estimate(start, goal);

        while (OpenSet.Count > 0)
        {
            Path_Node<Tile> current = OpenSet.Dequeue();

            if (current == goal)
            {
                // Goal reached so create the path of tiles to travel
                path = reconstruct_path(Came_From, current);
                return;
            }

            ClosedSet.Add(current);
    
            foreach(Path_Edge<Tile> edge_neighbour in current.edges)
            {
                Path_Node<Tile> neighbour = edge_neighbour.node;
                if (ClosedSet.Contains(neighbour) == true)
                {
                    continue; // already completed neighbour
                }

                float movement_cost_to_neighbour = dist_between(current, neighbour) * neighbour.data.movementCost;

                float tentative_g_score = g_score[current] + movement_cost_to_neighbour;

                if (OpenSet.Contains(neighbour) && tentative_g_score >= g_score[neighbour])
                {
                    continue;

                }
                Came_From[neighbour] = current;
                g_score[neighbour] = tentative_g_score;
                f_score[neighbour] = g_score[neighbour] + heuristic_cost_estimate(neighbour, goal);
                
                if(OpenSet.Contains(neighbour) == false)
                {
                    OpenSet.Enqueue(neighbour, f_score[neighbour]);
                } else
                {
                    OpenSet.UpdatePriority(neighbour, f_score[neighbour]);
                }
            }
        }

        // We've gotten through the OpenSet without ever reaching a point where current == goal.
        // This means there's no path from start to goal 

        return;

    }

    float heuristic_cost_estimate(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        // Hypotenuse - direct line to target
        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2)
            );
    }

    float dist_between(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        // We're on a grid
        // Horizontal / vertical neighbours have a distance of 1
        if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1f;
        }

        // Diagonal neighbours have a distance of 1.41421356237
        if (Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1.41421356237f;
        }

        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2)
            );
    }

    Queue<Tile> reconstruct_path(Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From, Path_Node<Tile> current) {
        // Current is the goal, so go back through Came_From map until 
        // We make it back to the starting node
        Queue<Tile> total_path = new Queue<Tile>();
        total_path.Enqueue(current.data); // The last step in the path is the goal itself

        while(Came_From.ContainsKey(current))
        {
            // Came_From maps some_node => node_we_got_here_from
            current = Came_From[current];
            total_path.Enqueue(current.data);
        }

        // total_path is now a queue running backwards from the end tile to the start tile
        Queue<Tile> reconstructed_path = new Queue<Tile>(total_path.Reverse());
        return reconstructed_path;

    }


    public Tile Dequeue()
    {
        return path.Dequeue();
    }
    public int Length()
    {
        if (path == null)
        {
            return 0;
        }
        return path.Count;
    }
}
