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
            return Mathf.Lerp(currTile.X, destTile.X, movementPercentage);
        }
    }
    public float Y
    {
        get
        {
            return Mathf.Lerp(currTile.Y, destTile.Y, movementPercentage);
        }
    }

    public Tile currTile {
        get; protected set;

    }
    Tile destTile; // If not moving, destTile = currTile
    float movementPercentage; // Between 0 and 1 as we go from currTile to destTile

    float speed = 3f; // Tiles per second

    Job myJob;

    Action<Character> cbCharacterChanged;

    public Character(Tile tile)
    {
        currTile = destTile = tile;
    }


    public void Update(float deltaTime)

    {

        // Do I have a job? 
        if(myJob == null)
        {
            // Get a new job
            myJob = currTile.world.jobQueue.Dequeue();
        }

        if (myJob != null)
        {
            destTile = myJob.tile;
            myJob.RegisterJobCompleteCallback(OnJobEnded);
            myJob.RegisterJobCancelCallback(OnJobEnded);
        }

        // If already at destination
        if (currTile == destTile)
        {
            if(myJob!= null)
            {
                myJob.DoWork(deltaTime);
            }
            return;
        }



        // Total distance from A to B
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - destTile.X, 2) + Mathf.Pow(currTile.Y - destTile.Y, 2));

        // How much can be traveled this Update
        float distThisFrame = speed * deltaTime;

        // Percentage to our desination of this travel
        float percentageThisFrame = distThisFrame / distToTravel;
        movementPercentage += percentageThisFrame;
        if(movementPercentage >= 1)
        {
            // Destination reached
            currTile = destTile;
            movementPercentage = 0;
        }

        if(cbCharacterChanged != null)
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
            Debug.LogError("Character is being told about a job that doesn't belong to them. You forgot to unregister something.");
            return;
        }
        
        myJob = null;
    }
}
