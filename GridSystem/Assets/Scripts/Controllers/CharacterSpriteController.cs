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
        Character c = world.CreateCharacter(world.GetTileAt(world.Width / 2, world.Height / 2));
        //c.SetDestination(world.GetTileAt(world.Width / 2 + 5, world.Height / 2));
       
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
        character_go.transform.position = new Vector3(character.X, character.Y, 0);
        character_go.transform.SetParent(this.transform, true);


        // Add a sprite renderer
        character_go.AddComponent<SpriteRenderer>().sprite = characterSprites["AIdude1"];
        // Put it on the right layer
        character_go.GetComponent<SpriteRenderer>().sortingLayerName = "Character";

        character.RegisterOnChangedCallback(OnCharacterChanged);

    }

    void OnCharacterChanged(Character character)
    {
        if (characterGameObjectMap.ContainsKey(character) == false)
        {
            Debug.LogError("OnCharacterChanged. Trying to change visuals for a character not in our dictionary");
        }
            GameObject char_go = characterGameObjectMap[character];
            char_go.transform.position = new Vector3(character.X, character.Y, 0);
             
        
    }

}
