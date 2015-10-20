using UnityEngine;
using System.Collections;

public interface IPowerUp {

    float GetSpawnChance();
    int GetNumberOfSpawn();
    GameObject GetGameObject();
    
}
