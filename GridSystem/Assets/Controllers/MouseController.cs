﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameObject circleCursorPrefab; 

    Vector3 lastFramePosition;
    Vector3 dragStartPosition;
    Vector3 currFramePosition;

    List<GameObject> dragPreviewGameObjects;
    // Start is called before the first frame update
    void Start()
    {
        dragPreviewGameObjects = new List<GameObject>();
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
            Destroy(go);
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
                        GameObject go = (GameObject)Instantiate(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        dragPreviewGameObjects.Add(go);
                    }

                }
            }


            // End drag
            if (Input.GetMouseButtonUp(0))
            {


                for (int x = start_x; x <= end_x; x++)
                {
                    for (int y = start_y; y <= end_y; y++)
                    {
                        Tile t = WorldController.Instance.World.GetTileAt(x, y);
                        if (t != null)
                        {
                            t.Type = Tile.TileType.Floor;
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
            Debug.Log(diff);
            Camera.main.transform.Translate(diff);
        }

        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;
    }

    
}
