using UnityEngine;
using System.Collections;
using System;

public class AddBomb : MonoBehaviour, IPowerUp
{
    [Range(0f, 1f)]
    public float spawnChance;

    [Tooltip("Maximum number of powerups that can be found in this level")]
    public int numberOfSpawn;

    public AudioClip sound;
    //Used for comparer
    //private int personalValue = 2;

    private PlayerController player;

    // Use this for initialization
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            Debug.Log("Player enter trigger");
            AudioSource.PlayClipAtPoint(sound, transform.position);
            IncreaseNumberOfBombs();
            Destroy(gameObject);
        }        
    }

    void IncreaseNumberOfBombs()
    {
        player.maxNumberOfBombs++;
    }

    public float GetSpawnChance()
    {
        return spawnChance;
    }

    public int GetNumberOfSpawn()
    {
        return numberOfSpawn;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
}
