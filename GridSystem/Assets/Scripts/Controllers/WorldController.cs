using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour
{

    public static WorldController Instance { get; protected set; }

    public World world { get; protected set; }

    static bool loadWorld = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (Instance != null)
        {
            Debug.Log("There should only ever be one world controller");
        }
        Instance = this;
        if(loadWorld)
        {
            loadWorld = false;
            CreateWorldFromSaveFile();
        }
        else
        {
            CreateEmptyWorld();
        }
        
    }

    public void SetupPathfindingExample() {
        world.SetupPathfindingExample();
    }

    void Update()
    {
        // Add pause/unpause, speed controls 
        world.Update(Time.deltaTime);

    }
    
    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x + 0.5f);
        int y = Mathf.FloorToInt(coord.y + 0.5f);

        return world.GetTileAt(x, y);
    }

    public void NewWorld()
    {
        Debug.Log("NewWorld button was clicked.");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveWorld()
    {
        Debug.Log("SaveWorld button was clicked.");

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, world);
        writer.Close();

        Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());
    }

    public void LoadWorld()
    {
        Debug.Log("LoadWorld button was clicked.");

        // Reload the scene to reset all data (and purge old references)
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
    void CreateEmptyWorld()
    {
        // Create blank world
        world = new World(100, 100);

        // Center the camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

    }

    void CreateWorldFromSaveFile()
    {
        Debug.Log("CreateWorldFromSaveFile");
        // Create a world from our save file data.

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        Debug.Log(reader.ToString());
        world = (World)serializer.Deserialize(reader);
        reader.Close();



        // Center the Camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

    }

}
