using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MouseOverFurnitureTypeText : MonoBehaviour
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
            Debug.LogError("FurnitureTypeText: No TextMeshPro UI component on this object ");
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
        string s = "NULL";
        if(t.furniture != null)
        {
            s = t.furniture.objectType;
        }
        TMP_text.text = "Furniture : " + s;
    }
}
