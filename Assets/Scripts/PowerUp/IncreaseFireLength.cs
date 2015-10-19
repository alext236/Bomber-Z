using UnityEngine;
using System.Collections;
using System;

public class IncreaseFireLength : MonoBehaviour, IPowerUp
{
    [Range(0f, 1f)]
    public float spawnChance;

    [Tooltip("Maximum number of powerups that can be found in this level")]
    public int numberOfSpawn;

    public AudioClip sound;

    //Temporarily used for comparer
    //private int personalValue = 1;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            Debug.Log("Player enter trigger to increase bomb length");
            AudioSource.PlayClipAtPoint(sound, transform.position);
            IncreaseLength();
            Destroy(gameObject);
        }
    }

    void IncreaseLength()
    {
        Bomb.length++;
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
