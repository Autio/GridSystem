using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class BuildModeController : MonoBehaviour
{

    bool buildModeIsObjects = false;
    bool removingObjects = false;
    TileType buildModeTile = TileType.Floor;
    string buildModeObjectType;

    List<GameObject> dragPreviewGameObjects;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetMode_BuildFloor()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Floor;

    }
    public void SetMode_Bulldoze()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Empty;

    }
    public void SetMode_BuildFurniture( string objectType )
    {

        buildModeIsObjects = true;
        removingObjects = false;
        buildModeObjectType = objectType;


    }
    public void SetMode_RemoveFurniture()
    {

        buildModeIsObjects = true;
        buildModeObjectType = null;
        removingObjects = true;

    }

    public void DoBuild( Tile t )
    {
        if (buildModeIsObjects == true)
        {
            // Create the Furniture and assign it to the tile
            string furnitureType = buildModeObjectType;

            // Check whether can build furniture on the selected tile
            // Run ValidPlacement function
            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t) &&
            t.pendingFurnitureJob == null)
            {
                Job j = new Job(t, furnitureType, (theJob) => {
                    WorldController.Instance.world.PlaceFurniture(furnitureType, t);
                    t.pendingFurnitureJob = null;
                }
                ); 

                // FIXME: Set flags better. Too easy to forget to clear them and set them
                t.pendingFurnitureJob = j;

                j.RegisterJobCancelCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

                // Add the job to the queue
                WorldController.Instance.world.jobQueue.Enqueue(j);
                //Debug.Log("Job queue size: " + WorldController.Instance.world.jobQueue.Count);
            }

        }
        else
        {
            t.Type = buildModeTile;
        }
    }
}
