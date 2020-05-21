using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour
{
    // Ensure jobs are appropriately visualized
    FurnitureSpriteController fsc;
    Dictionary<Job, GameObject> jobGameObjectMap;

    // Start is called before the first frame update
    void Start()
    {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();

        // FIXME: Create a job queue
        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);

    }

    void OnJobCreated(Job job)
    {
        //FIXME: Currently only accepts furniture

        if (jobGameObjectMap.ContainsKey(job))
        {
            // FIXME: Job requeueing
            Debug.LogError("OnJobCreated for a jobGO that already exists. Job probably being re-queued instead of created");
            return;
        }

        GameObject job_go = new GameObject();

        // Add tile/GO pair to the dictionary
        jobGameObjectMap.Add(job, job_go);
        job_go.name = "JOB_" + job.jobObjectType + " " + job.tile.X + "_ " + job.tile.Y;

        job_go.transform.position = new Vector3(job.tile.X, job.tile.Y, 0);
        job_go.transform.SetParent(this.transform, true);

        // Add a sprite renderer
        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        sr.sprite = fsc.GetSpriteForFurniture(job.jobObjectType);
        sr.color = new Color(.6f, 1f, .6f, 0.35f);
        // Put it on the right layer
        job_go.GetComponent<SpriteRenderer>().sortingLayerName = "Jobs";

        // FIXME
        if (job.jobObjectType == "Door")
        {
            // By default door sprite is for N/S 
            // Check to see if there's a wall E/W, in which case rotate 90 degrees
            Tile eastTile = job.tile.world.GetTileAt(job.tile.X + 1, job.tile.Y);
            Tile westTile = job.tile.world.GetTileAt(job.tile.X - 1, job.tile.Y);

            if (eastTile != null && westTile != null && eastTile.furniture != null && westTile.furniture != null &&
                eastTile.furniture.objectType == "Wall" && westTile.furniture.objectType == "Wall")
            {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }


        job.RegisterJobCompleteCallback(OnJobEnded);
        job.RegisterJobCancelCallback(OnJobEnded);

    }



    void OnJobEnded(Job job)
    {
        // Whenever a job is completed or cancelled

        // TODO: Delete sprites
        GameObject job_go = jobGameObjectMap[job];
        
        job.UnregisterJobCompleteCallback(OnJobEnded);
        job.UnregisterJobCancelCallback(OnJobEnded);
        Destroy(job_go);

    }


}
