using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Character 
{
    public float X
    {
        get
        {
            return Mathf.Lerp(currTile.X, nextTile.X, movementPercentage);
        }
    }
    public float Y
    {
        get
        {
            return Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage);
        }
    }

    public Tile currTile {
        get; protected set;

    }
    Tile destTile; // If not moving, destTile = currTile
    Tile nextTile; // Next tile in the pathfinding sequence
    Path_AStar pathAStar;
    float movementPercentage; // Between 0 and 1 as we go from currTile to destTile

    float speed = 3f; // Tiles per second

    Job myJob;

    Action<Character> cbCharacterChanged;

    public Character(Tile tile)
    {
        currTile = destTile = nextTile = tile;
    }

    void Update_DoJob(float deltaTime)
    {
        // Do I have a job? 
        if (myJob == null)
        {
            // Get a new job
            myJob = currTile.world.jobQueue.Dequeue();

            if (myJob != null)
            {

                // Is the job reachable? 



                destTile = myJob.tile;
                myJob.RegisterJobCompleteCallback(OnJobEnded);
                myJob.RegisterJobCancelCallback(OnJobEnded);
            }

        }


        // If already at destination
        if (myJob != null && currTile == destTile)
        {
            //if(pathAStar != null && pathAStar.Length() == 1) // We are now adjacent to the job site 
            
                
            if (myJob != null)
            {
                myJob.DoWork(deltaTime);
            }
        }
    }

    public void AbandonJob()
    {
        nextTile = destTile = currTile;
        pathAStar = null;
        currTile.world.jobQueue.Enqueue(myJob);
        myJob = null;
    }

    void Update_DoMovement(float deltaTime)
    {
        // Don't try to move if you're already at the destination
        if(currTile == destTile)
        {
            pathAStar = null;
            return;
        }

        if(nextTile == null || nextTile == currTile)
        {
            // Get the next tile from the pathfinder system
            if (pathAStar == null || pathAStar.Length() == 0)
            {
                // Generate a path to our destination
                pathAStar = new Path_AStar(currTile.world, currTile, destTile);
                if(pathAStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar returned no path to the destination");
                    // FIXME: Job should mabe be re-enqueued
                    AbandonJob();
                    pathAStar = null;
                    return;
                }
            }

            // Set the next tile
            nextTile = pathAStar.Dequeue();

            if(nextTile == currTile)
            {
                Debug.Log("Update_DoMovement. NextTile is currTile?");
            }

        }


        // Total distance from A to B
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - nextTile.X, 2) + Mathf.Pow(currTile.Y - nextTile.Y, 2));

        // How much can be traveled this Update
        float distThisFrame = speed * deltaTime;

        // Percentage to our desination of this travel
        float percentageThisFrame = distThisFrame / distToTravel;
        movementPercentage += percentageThisFrame;
        if (movementPercentage >= 1)
        {
            // TODO: Get the next tile from the pathfinding system
            // If no more tiles in queue, then destination reached


            // Destination reached
            currTile = nextTile;
            movementPercentage = 0;
        }



    
    }

    public void Update(float deltaTime)

    {
        Update_DoJob(deltaTime);

        Update_DoMovement(deltaTime);

        if (cbCharacterChanged != null)
        {
            cbCharacterChanged(this);
        }
    }

 

    public void SetDestination(Tile tile)
    {
        if(currTile.IsNeighbour(tile) == false)
        {
            Debug.LogError("Character::SetDestination. Destination tile isn't our neighbour");

        }

        destTile = tile;
    }

    public void RegisterOnChangedCallback(Action<Character> cb)
    {
        cbCharacterChanged += cb;
    }
    public void UnregisterOnChangedCallback(Action<Character> cb)
    {
        cbCharacterChanged -= cb;
    }

    void OnJobEnded(Job j)
    {
        // Job completed or cancelled
        if(j != myJob) {
            
            Debug.Log("Character is being told about a job that doesn't belong to them. You forgot to unregister something."); // Was Error

            return;
        }
        
        myJob = null;
    }


}
