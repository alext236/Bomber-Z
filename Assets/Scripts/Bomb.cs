using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour {

    [Range (1, 10)]
    public int length = 1;
    
//    [Range(0.1f, 2f)]
//    [Tooltip("Duration the fire stays on screen")]
    float fireTime = 1f;

    [Range(1f, 5f)]
    public float timeToExplode;

    public GameObject bombFirePrefab;

    private bool hasCollider = false;
    private bool bombTriggered = false;
    private Animator anim;

    private float distanceToNearestWall_left;
    private float distanceToNearestWall_right;
    private float distanceToNearestWall_forward;
    private float distanceToNearestWall_back;

    Vector3 playerDir;
    bool flagHitPlayer = false;
    public bool BombTriggered 
    {
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
        SetDefaultDistance();
        anim = GetComponent<Animator>();
        Debug.Log(anim);
        Invoke("StartExplosionAnimation", timeToExplode);
    }

    private void SetDefaultDistance() {
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
    void BombExplode() 
    {        
        BombImpact();
        if (flagHitPlayer)
        {
            Vector3 mgrid_size = FindObjectOfType<CreateMap>().getGridSize();
            float max_size = Mathf.Max(mgrid_size[0],mgrid_size[2]);
            FindObjectOfType<PlayerController>().setBombInfo(this.transform.position, fireTime, length * (max_size) + max_size/2);
        }
        Destroy(gameObject);

    }
    //////////////// Add a new animation state to trigger explosion -- Added by Tuan
    void StartExplosionAnimation() 
    {
        anim.SetBool("BombExplode", true);
    }

    void BombImpact() 
    {
        BombTriggered = true;        

        DrawRaycast(Vector3.left);
        DrawRaycast(Vector3.right);
        DrawRaycast(Vector3.forward);
        DrawRaycast(Vector3.back);
        DrawRaycast(Vector3.down);
        //Vector3.down
        CreateFireParticles();
    }
    //Cut the length of fire to distance to nearest wall
    void DefineFireLength(Transform transform, float nearestDistance, float iFireTime, float grid_size) 
    {        
        float fireLength = 0;
        if (nearestDistance < length * iFireTime) 
        {
            fireLength = Mathf.RoundToInt(nearestDistance);
        }
        else 
        {
            fireLength = length;
        }
        //startSpeed 2.5 equals one tile fire length, or 1 + 0.5 in tile length
        
        fireLength = (10.0f/9.0f)*((fireLength)  * grid_size + grid_size/2);
        transform.GetComponent<ParticleSystem>().startSpeed = fireLength;
        transform.GetComponent<ParticleSystem>().startLifetime = iFireTime;
    }

    private void CreateFireParticles() {
        GameObject fire = Instantiate(bombFirePrefab, transform.position, Quaternion.identity) as GameObject;
        //fire.transform.SetParent(transform);
        Vector3 mgrid_size = FindObjectOfType<CreateMap>().getGridSize();
        foreach (Transform child in fire.transform) 
        {
            if (child.name == "Left") {
                DefineFireLength(child, distanceToNearestWall_left, fireTime, mgrid_size[0]);
            }

            if (child.name == "Right") {
                DefineFireLength(child, distanceToNearestWall_right, fireTime, mgrid_size[0]);
            }

            if (child.name == "Forward") {
                DefineFireLength(child, distanceToNearestWall_forward, fireTime, mgrid_size[2]);
            }

            if (child.name == "Back") {
                DefineFireLength(child, distanceToNearestWall_back, fireTime, mgrid_size[2]);
            }
        }

        Destroy(fire, fireTime + 1f);   //1s padding so the particles have time to die out before destroyed
    }

    float GetDistanceToNearestWall(GameObject wall, Vector3 direction) 
    {
        if (direction == Vector3.left) 
        {            
            distanceToNearestWall_left = Mathf.Abs(transform.position.x - wall.transform.position.x) - 1;   //So the fire stops right before a wall         
            return distanceToNearestWall_left;
        }

        if (direction == Vector3.right) 
        {
            distanceToNearestWall_right = Mathf.Abs(transform.position.x - wall.transform.position.x) - 1;
            return distanceToNearestWall_right;
        }

        if (direction == Vector3.forward) 
        {
            distanceToNearestWall_forward = Mathf.Abs(transform.position.z - wall.transform.position.z) - 1;
            return distanceToNearestWall_forward;
        }

        if (direction == Vector3.back) 
        {
            distanceToNearestWall_back = Mathf.Abs(transform.position.z - wall.transform.position.z) - 1;
            return distanceToNearestWall_back;
        }
        return 0f;
    }

    void DrawRaycast(Vector3 direction) 
    {
        Vector3 mgrid_size = FindObjectOfType<CreateMap>().getGridSize();
        float max_size = Mathf.Max(mgrid_size[0], mgrid_size[2]);
        Vector3 pos = transform.position;
        if (direction == Vector3.down)
            pos -= Vector3.down;
        float dis = (length * max_size + max_size/2) * fireTime;
        Ray impactRay = new Ray(pos, direction);

        RaycastHit[] hit = Physics.RaycastAll(impactRay, dis);
        Debug.DrawLine(pos + new Vector3(0,1,0), new Vector3(0,1,0) + pos + direction * dis, Color.red);
        bool flag_continue = true;
        for (int i = 0; i < hit.Length && flag_continue; i++)
        {
            if (hit[i].collider.tag == "IndestructibleWall")
            {
                GetDistanceToNearestWall(hit[i].collider.gameObject, direction);
                flag_continue = false;
                //                float ratio_real_planned = animation_length / (length * fireTime);
                //                Destroy(hit.collider.gameObject, ratio_real_planned * fireTime);
            }
            else if (hit[i].collider.tag == "Wall")
            {// if the actual fire length is smaller than planned fire length then the time that fire reach to the object is the portion of the planned time
                float animation_length = GetDistanceToNearestWall(hit[i].collider.gameObject, direction);
                float ratio_real_planned = animation_length / (length * fireTime);
                Destroy(hit[i].collider.gameObject, ratio_real_planned * fireTime);
                flag_continue = false;
            }
            else if (hit[i].collider.GetComponent<PlayerController>())
            {
                flagHitPlayer = true;
                //Do something to the player
                playerDir = direction;
            }

            //Bomb can explode other bombs
            if (hit[i].collider.GetComponent<Bomb>())
            {
                Bomb bomb = hit[i].collider.GetComponent<Bomb>();
                if (!bomb.BombTriggered) //Trigger only the bomb that has not been triggered
                {
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
