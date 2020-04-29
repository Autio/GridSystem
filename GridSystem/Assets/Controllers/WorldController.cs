using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{

    public static WorldController Instance { get; protected set; }

    public World world { get; protected set; }

    // Start is called before the first frame update
    void OnEnable()
    {
        if(Instance != null)
        {
            Debug.Log("There should only ever be one world controller");
        }
        Instance = this;
        // Create blank world
        world = new World();

        // Center the camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

    }

    void Update()
    {
        // Add pause/unpause, speed controls 
        world.Update(Time.deltaTime);

    }
    
    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x);
        int y = Mathf.FloorToInt(coord.y);

        return world.GetTileAt(x, y);
    }
    
}
