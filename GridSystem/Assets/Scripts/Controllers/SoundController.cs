using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{

    float soundCooldown = 0;
    // Start is called before the first frame update
    void Start()
    {

        WorldController.Instance.world.RegisterFurnitureCreated( OnFurnitureCreated );
        WorldController.Instance.world.RegisterTileChanged(OnTileTypeChanged);
    }

    // Update is called once per frame
    void Update()
    {
        soundCooldown -= Time.deltaTime;
    }

    void OnTileTypeChanged( Tile tile_data )
    {
        if (soundCooldown > 0)
        {
            return;
        }
        // FIXME 
        AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;

    }

    public void OnFurnitureCreated(Furniture furn)
    {
        if (soundCooldown > 0)
        {
            return;
        }
        // FIXME 
        AudioClip ac = Resources.Load<AudioClip>("Sounds/" + furn.objectType + "_OnCreated");

        if (ac == null)
        {
            // No specific sound for furniture, just use default sound 
            ac = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
        }
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;

    }
}
