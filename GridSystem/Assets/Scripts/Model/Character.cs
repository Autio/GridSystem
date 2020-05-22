using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character : IXmlSerializable
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

    } // If not moving, destTile = currTile
    Tile destTile
    {
        get { return destTile; }
        set
        {
            if(destTile != value)
            {
                destTile = value;
                pathAStar = null; // Reset pathfinding whenever the dest is set to a new one;
            }
        }
    }
    Tile nextTile; // Next tile in the pathfinding sequence
    Path_AStar pathAStar;
    float movementPercentage; // Between 0 and 1 as we go from currTile to destTile

    float speed = 3f; // Tiles per second
    Action<Character> cbCharacterChanged;

    Job myJob;

    // Item the character is carrying (not equipment)
    Inventory inventory;

    public Character()
    {
        // Use only for serialization;
    }


    public Character(Tile tile)
    {
        currTile = destTile = nextTile = tile;
    }

    void GetNewJob()
    {
        // Get a new job
        myJob = currTile.world.jobQueue.Dequeue();

        // Check to see that the final location can be reached
        destTile = myJob.tile;
        myJob.RegisterJobCompleteCallback(OnJobEnded);
        myJob.RegisterJobCancelCallback(OnJobEnded);

        pathAStar = new Path_AStar(currTile.world, currTile, destTile);
        if (pathAStar.Length() == 0)
        {
            Debug.LogError("Path_AStar returned no path to the target job tile");
            AbandonJob();
            destTile = currTile;
        }
    }

    void Update_DoJob(float deltaTime)
    {
        // Do I have a job? 
        if (myJob == null)
        {
            GetNewJob();

            if (myJob == null)
            {
                // No job in the queue, so just return
                destTile = currTile;
                return;
            }
        }

        // Does the job have all the materials it needs?
        if (myJob.HasAllMaterial() == false)
        {
            // We are missing some materials

            // Are we carrying anything that the job location wants? 
            if(inventory != null)
            {
                if (myJob.DesiresInventoryType(inventory))
                {
                    // Deliver the goods, walk to the job tile and drop off the stack into the job
                    
                    if(currTile == myJob.tile)
                    {
                        // We are at the job site, so drop the inventory
                        currTile.world.inventoryManager.PlaceInventory(myJob, inventory);
                        
                        // Are we still carrying something?
                        if(inventory.stackSize == 0)
                        {
                            inventory = null;
                        } else
                        {
                            Debug.LogError("Character is still carrying inventory. Setting to NULL for now, but means we are leaking inventory");
                            inventory = null;
                        }

                    } 
                    else
                    {
                        destTile = myJob.tile;
                    }
                }
                else
                {
                    // We are carrying something but the job doesn't want it
                    // Dump the inventory at our feet
                    // TODO: Walk to the nearest empty tile and dump it there
                    // example state machine:  currentAction = walkToEmptyTile;
                    if(currTile.world.inventoryManager.PlaceInventory(currTile, inventory)==false)
                    {
                        Debug.LogError("Character tried to dump inventory into an invalid tile (maybe there's already something here");
                        // FIXME: Leaking inventory
                        inventory = null;
                    }
                     
                }
            }

            // The job still requires inventory, but we aren't carrying it yet


            // IF we are, deliver the goods
            // Walk to the job tile, then drop off the stack into the job
            destTile = myJob.tile;
            // If not, walk towards a tile containing the required goods
            // If already on such a tile, pick up the goods
           // stTile = someTileWithTheMaterials;
            
            return; // We can't continue until all materials are satisfied
        }

        // If we get to this point, the job has all the materials we need.
        // MAke sure our desination tile is the job site tile
        destTile = myJob.tile;

        // If already at destination
        if (currTile == myJob.tile)
        {   
            // Correct tile for the job
            //if(pathAStar != null && pathAStar.Length() == 1) // We are now adjacent to the job site 
            myJob.DoWork(deltaTime);
        }
        // Where do we decrement the partially used materials? If a job needs to get abandoned etc

    }

    public void AbandonJob()
    {
        nextTile = destTile = currTile;
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
                    return;
                }
                // Let's ignore the first tile because that's where we are at
                nextTile = pathAStar.Dequeue();

            }


            // Set the next tile
            nextTile = pathAStar.Dequeue();

            if(nextTile == currTile)
            {
                Debug.Log("Update_DoMovement. NextTile is currTile?");
            }

        }


        // Total diastance from A to B
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - nextTile.X, 2) + Mathf.Pow(currTile.Y - nextTile.Y, 2));

        if(nextTile.IsEnterable() == ENTERABILITY.Never)
        {
            Debug.LogError("FIXME: A character was trying to enter an unwalkable tile");
            nextTile = null; // Next tile is inaccessible
            pathAStar = null; // Our pathfinding information is stale
            return;
        } else if (nextTile.IsEnterable() == ENTERABILITY.Soon)
        {
            // Can't enter now but should be able to in the future
            // Likely to be a door
            return;
        }
        {
            // The tile we are trying to enter is walkable but 
            // Are we allowed to enter it right now?
        }

        // How much can be traveled this Update
        // Beware div by 0 
        float distThisFrame = speed / nextTile.movementCost * deltaTime;

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

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", currTile.X.ToString());
        writer.WriteAttributeString("Y", currTile.Y.ToString());

    }

    public void ReadXml(XmlReader reader)
    {

    }

}
