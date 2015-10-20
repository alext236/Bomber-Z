using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DestructibleWall : MonoBehaviour
{
    //public class iPowerUpComparer : IEqualityComparer<IPowerUp>
    //{
    //    public bool Equals(IPowerUp x, IPowerUp y)
    //    {
    //        if (x == null & y == null)
    //            return false;
    //        else if (x == null | y == null)
    //            return false;
    //        else if (x.GetSpawnChance() == y.GetSpawnChance() && x.GetNumberOfSpawn() == y.GetNumberOfSpawn() && x.GetPersonalValue() == y.GetPersonalValue())
    //            return true;
    //        else
    //            return false;
    //    }

    //    public int GetHashCode(IPowerUp obj)
    //    {
    //        int hCode = obj.GetPersonalValue();
    //        return hCode;
    //    }
    //}

    public List<IPowerUp> powerUps;
    private static Dictionary<IPowerUp, int> maxNumberOfSpawn;

    // Use this for initialization
    void Awake()
    {
        powerUps = new List<IPowerUp>();
        maxNumberOfSpawn = new Dictionary<IPowerUp, int>();
    }

    public void Start()
    {
        foreach (IPowerUp item in powerUps)
        {
            AddOrUpdateDictionaryEntry(item, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void AddOrUpdateDictionaryEntry(IPowerUp key, int value)
    {
        if (maxNumberOfSpawn.ContainsKey(key))
        {
            maxNumberOfSpawn[key] = value;
        }
        else
        {
            maxNumberOfSpawn.Add(key, value);
        }
    }

    void IncreaseCount(IPowerUp key)
    {
        int newValue = maxNumberOfSpawn[key] + 1;        
        AddOrUpdateDictionaryEntry(key, newValue);
    }

    public void SpawnAPowerUp()
    {
        foreach (IPowerUp item in powerUps)
        {
            if (UnityEngine.Random.value <= item.GetSpawnChance() && maxNumberOfSpawn[item] < item.GetNumberOfSpawn())
            {

                GameObject newPowerUp = Instantiate(item.GetGameObject(), transform.position, Quaternion.identity) as GameObject;
                IncreaseCount(item);
                
                //Sort into parent folder
                if (GameObject.Find("PowerUp Parent"))
                {
                    newPowerUp.transform.SetParent(GameObject.Find("PowerUp Parent").transform);
                }
                else
                {
                    GameObject parent = new GameObject("PowerUp Parent");
                    newPowerUp.transform.SetParent(parent.transform);
                }

                return; //Each wall can only have one item spawned
            }
        }
    }
}
