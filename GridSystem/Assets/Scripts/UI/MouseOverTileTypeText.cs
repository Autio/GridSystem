using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MouseOverTileTypeText : MonoBehaviour
{
    TMP_Text TMP_text;
    MouseController mouseController;
    // Check to see which tile is being hovered over by the mouse 
    // Start is called before the first frame update
    void Start()
    {
        TMP_text = GetComponent<TMP_Text>();

        if(TMP_text == null)
        {
            Debug.LogError("MouseOverTileTypeText: No TextMeshPro UI component on this object ");
            this.enabled = false;
            return;
        }
        mouseController = GameObject.FindObjectOfType<MouseController>();
        if(mouseController == null)
        {
            Debug.LogError("There's no instance of mousecontroller!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Tile t = mouseController.GetMouseOverTile();
        TMP_text.text = "Tile Type : " + t.Type.ToString();
    }
}
