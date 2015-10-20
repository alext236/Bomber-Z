using UnityEngine;
using System.Collections;

public class Playground : MonoBehaviour {
    [Tooltip("Should be an even number")]
    public int width;
    [Tooltip("Should be an even number")]
    public int height;

    public GameObject[] hiddenPowerUpsToSpawn;   //Temporary. Move this to DestructibleWall class after having a prefab
    private IPowerUp[] hiddenPowerUps;

    private Material material;

    public IPowerUp[] HiddenPowerUps
    {
        get
        {
            return hiddenPowerUps;
        }

        set
        {
            hiddenPowerUps = value;
        }
    }

    // Use this for initialization
    void Awake () {
        Bomb.length = 1;
        ModifyMaterialTiling();
        SetGroundSize();
        SetPlaygroundAxis();

        hiddenPowerUps = new IPowerUp[hiddenPowerUpsToSpawn.Length];

        for (int i = 0; i < hiddenPowerUpsToSpawn.Length; i++)
        {
            hiddenPowerUps[i] = hiddenPowerUpsToSpawn[i].GetComponent(typeof(IPowerUp)) as IPowerUp;
        }
                
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void SetGroundSize() {
        Vector3 newSize = new Vector3(width, transform.localScale.y, height);
        transform.localScale = newSize;
    }

    void ModifyMaterialTiling() {
        material = GetComponent<MeshRenderer>().material;
        material.mainTextureScale = new Vector2(width / 2, height / 2);
    }

    void SetPlaygroundAxis() {  //So bottom left tile has coordinate (1, y, 1)
        Vector3 newPosition = new Vector3();
        newPosition.x = width / 2 + 0.5f;
        newPosition.y = transform.position.y;
        newPosition.z = height / 2 + 0.5f;
        
        transform.position = newPosition;
    }
}
