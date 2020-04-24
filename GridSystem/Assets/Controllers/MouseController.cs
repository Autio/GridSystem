using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameObject circleCursor; 

    Vector3 lastFramePosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;
        // Update circle cursor position 
        circleCursor.transform.position = currFramePosition;

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
