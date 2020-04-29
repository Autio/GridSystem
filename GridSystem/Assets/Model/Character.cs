using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    float speed = 2f; // Tiles per second

    public void Update(float deltaTime)

    {
        // If already at destination
        if (currTile == destTile)
            return;

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
    }
    

    public Character(Tile tile)
    {
        currTile = destTile = tile; 
    }

    public void SetDestination(Tile tile)
    {
        if(currTile.IsNeighbour(tile) == false)
        {
            Debug.LogError("Character::SetDestination. Destination tile isn't our neighbour");

        }

        destTile = tile;
    }
}
