using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour
{

    Dictionary<Character, GameObject> characterGameObjectMap;
    Dictionary<string, Sprite> characterSprites;
    World world
    {
        get { return WorldController.Instance.world; }
    }


    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        characterGameObjectMap = new Dictionary<Character, GameObject>();

        world.RegisterCharacterCreated(OnCharacterCreated);

        // DEBUG
        world.CreateCharacter(world.GetTileAt(world.Width / 2, world.Height / 2));
    }

    void LoadSprites()
    {
        // Initialize sprite dictionary for installed objects
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Characters/");

        // Assign sprites by name to dictionary
        foreach (Sprite s in sprites)
        {
            characterSprites[s.name] = s;
        }

    }

    public void OnCharacterCreated(Character character)
    {
        // FIXME: Multi-tile and rotated objects
        Debug.Log("OnCharacterCreated");
        // Create a visual GameObject linked to this data
        GameObject character_go = new GameObject();

        // Add tile/GO pair to the dictionary
        characterGameObjectMap.Add(character, character_go);

        character_go.name = "Character";
        character_go.transform.position = new Vector3(character.currTile.X, character.currTile.Y, 0);
        character_go.transform.SetParent(this.transform, true);


        // Add a sprite renderer
        character_go.AddComponent<SpriteRenderer>().sprite = characterSprites["AIdude1"];
        // Put it on the right layer
        character_go.GetComponent<SpriteRenderer>().sortingLayerName = "Character";

    }

}
