using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class MouseController : MonoBehaviour
{
    public GameObject circleCursorPrefab;

    bool buildModeIsObjects = false;
    TileType buildModeTile = TileType.Floor;
    string buildModeObjectType;

    Vector3 lastFramePosition;
    Vector3 dragStartPosition;
    Vector3 currFramePosition;

    List<GameObject> dragPreviewGameObjects;
    // Start is called before the first frame update
    void Start()
    {
        dragPreviewGameObjects = new List<GameObject>();
        SimplePool.Preload(circleCursorPrefab, 100);
    }

    // Update is called once per frame
    void Update()
    {
        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        //UpdateCursor();
        UpdateDragging();
        UpdateCameraMovement();     
        
    }

    //void UpdateCursor()
    //{
    //    // Update circle cursor position 
    //    Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoord(currFramePosition);

    //    if (tileUnderMouse != null)
    //    {
    //        circleCursor.SetActive(true);
    //        Vector3 cursorPosition = new Vector3(tileUnderMouse.X, tileUnderMouse.Y, 0);
    //        circleCursor.transform.position = cursorPosition;
    //    }
    //    else
    //    {
    //        circleCursor.SetActive(false);
    //    }
    //}

    void UpdateDragging()
    {
 
        // Handle left mouse clicks
        // Start Drag 

        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = currFramePosition;
        }

        int start_x = Mathf.FloorToInt(dragStartPosition.x);
        int end_x = Mathf.FloorToInt(currFramePosition.x);
        if (end_x < start_x)
        {
            int temp = end_x;
            end_x = start_x;
            start_x = temp;
        }

        int start_y = Mathf.FloorToInt(dragStartPosition.y);
        int end_y = Mathf.FloorToInt(currFramePosition.y);
        if (end_y < start_y)
        {
            int temp = end_y;
            end_y = start_y;
            start_y = temp;
        }


        // Clean up old drag previews
        while (dragPreviewGameObjects.Count > 0)
        {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }
        if (Input.GetMouseButton(0))
        {
            // Display a preview of the drag area
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);

                    if (t != null)
                    {
                        GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        go.transform.SetParent(this.transform, true);
                        dragPreviewGameObjects.Add(go);
                    }

                }
            }
        }

        // End drag
        if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Creating floor");


                for (int x = start_x; x <= end_x; x++)
                {
                    for (int y = start_y; y <= end_y; y++)
                    {
                        // Build object or install one
                        Tile t = WorldController.Instance.World.GetTileAt(x, y);
                        if (t != null)
                        {
                            if (buildModeIsObjects == true)
                            {
                            // Create the Furniture and assign it to the tile
                            WorldController.Instance.World.PlaceFurniture(buildModeObjectType, t);

                            } else
                            {
                                t.Type = buildModeTile;
                            }
                        }
                    }

                }
         }
       
    }

    void UpdateCameraMovement()
    {

        // Update to new input management
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector3 diff = lastFramePosition - currFramePosition;
            Camera.main.transform.Translate(diff);


        }

        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 30f);
        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;
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
        buildModeObjectType = objectType;


    }
}
