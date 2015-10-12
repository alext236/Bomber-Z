using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour {

    [Range (1, 10)]
    public int length = 1;
    public GameObject bombFirePrefab;
    [Range(0.1f, 2f)]
    public float fireTime;

    private bool hasCollider = false;
    private bool bombTriggered = false;

    private float distanceToNearestWall_left;
    private float distanceToNearestWall_right;
    private float distanceToNearestWall_forward;
    private float distanceToNearestWall_back;

    public bool BombTriggered {
        get 
        {
            return bombTriggered;
        }

        set 
        {
            bombTriggered = value;
        }
    }


    // Use this for initialization
    void Start() 
    {
        distanceToNearestWall_left = length;
        distanceToNearestWall_right = length;
        distanceToNearestWall_forward = length;
        distanceToNearestWall_back = length;
    }

    // Update is called once per frame
    void Update() 
    {
        CreateCollider();
    }

    //Used as animation event for now
    //TODO: trigger this with a timer
    void BombExplode() {
        BombImpact();
        Destroy(gameObject);

    }

    void BombImpact() {
        BombTriggered = true;        

        DrawRaycast(Vector3.left);
        DrawRaycast(Vector3.right);
        DrawRaycast(Vector3.forward);
        DrawRaycast(Vector3.back);

        CreateFireParticles();
    }
    //Cut the length of fire to distance to nearest wall
    void DefineFireLength(Transform transform, float nearestDistance, float iFireTime) ///////////////////////////////////////////////////////////////changed by Kourosh 
    {        
        float fireLength = 0;
        if (nearestDistance < length * iFireTime) ////////////////////////////////////////////////////////////////////////////////////////////////////Added by Kourosh 
        {
            fireLength = Mathf.RoundToInt(nearestDistance);
        }
        else 
        {
            fireLength = length;
        }
        //startSpeed 2.5 equals one tile fire length, or 1 + 0.5 in tile length
        fireLength = (fireLength + 0.5f) * 2.5f / 1.5f;
        transform.GetComponent<ParticleSystem>().startSpeed = fireLength;
        transform.GetComponent<ParticleSystem>().startLifetime = iFireTime;
    }

    private void CreateFireParticles() {
        GameObject fire = Instantiate(bombFirePrefab, transform.position, Quaternion.identity) as GameObject;
        //fire.transform.SetParent(transform);

        foreach (Transform child in fire.transform) 
        {
            if (child.name == "Left") {
                DefineFireLength(child, distanceToNearestWall_left, fireTime);
            }

            if (child.name == "Right") {
                DefineFireLength(child, distanceToNearestWall_right, fireTime);
            }

            if (child.name == "Forward") {
                DefineFireLength(child, distanceToNearestWall_forward, fireTime);
            }

            if (child.name == "Back") {
                DefineFireLength(child, distanceToNearestWall_back, fireTime);
            }
        }

        Destroy(fire, fireTime + 1f);   //1s padding so the particles have time to die out before destroyed
    }

    float GetDistanceToNearestWall(GameObject wall, Vector3 direction) {
        if (direction == Vector3.left) {            
            distanceToNearestWall_left = Mathf.Abs(transform.position.x - wall.transform.position.x) - 1f;   //So the fire stops right before a wall         
            return distanceToNearestWall_left;
        }

        if (direction == Vector3.right) {
            distanceToNearestWall_right = Mathf.Abs(transform.position.x - wall.transform.position.x) - 1f;
            return distanceToNearestWall_right;
        }

        if (direction == Vector3.forward) {
            distanceToNearestWall_forward = Mathf.Abs(transform.position.z - wall.transform.position.z) - 1f;
            return distanceToNearestWall_forward;
        }

        if (direction == Vector3.back) {
            distanceToNearestWall_back = Mathf.Abs(transform.position.z - wall.transform.position.z) - 1f;
            return distanceToNearestWall_back;
        }
        return 0f;
    }

    void DrawRaycast(Vector3 direction) {
        RaycastHit hit;

        Ray impactRay = new Ray(transform.position, direction);
        if (Physics.Raycast(impactRay, out hit, length)) {
            if (hit.collider.tag == "IndestructibleWall")
            {
                GetDistanceToNearestWall(hit.collider.gameObject, direction);
//                float ratio_real_planned = animation_length / (length * fireTime);
//                Destroy(hit.collider.gameObject, ratio_real_planned * fireTime);
            }
            else if (hit.collider.tag == "Wall")/////////////////////////////////////////////////////////////////////////////////////////////////////////Added by Kourosh 
            {// if the actual fire length is smaller than planned fire length then the time that fire reach to the object is the portion of the planned time
                float animation_length = GetDistanceToNearestWall(hit.collider.gameObject, direction);
                float ratio_real_planned = animation_length / (length * fireTime);
                Destroy(hit.collider.gameObject, ratio_real_planned * fireTime);
            }

            //Bomb can explode other bombs
            if (hit.collider.GetComponent<Bomb>()) {
                Bomb bomb = hit.collider.GetComponent<Bomb>();
                if (!bomb.BombTriggered) {          //Trigger only the bomb that has not been triggered
                    bomb.BombExplode();

                }

            }
        }
    }

    void CreateCollider() {
        //only create collider after player is >(a tile) away from where the bomb is spawned
        //so that player can move away from the bomb when first placing it but cannot step back on the bomb
        //now assume a tile grid size is (1, y, 1)
        if (!hasCollider) {
            PlayerController player = FindObjectOfType<PlayerController>();

            float distance_x = player.transform.position.x - transform.position.x;
            float distance_z = player.transform.position.z - transform.position.z;
            CreateMap myMap = FindObjectOfType<CreateMap>();

            if (Mathf.Abs(distance_x) > myMap.getGridSize().x || Mathf.Abs(distance_z) > myMap.getGridSize().z) {
                BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
                newCollider.center = Vector3.zero;
                newCollider.size = Vector3.one;
                hasCollider = true;
            }
        }
    }
}
