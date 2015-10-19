using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour {

    public class mySortClass : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            RaycastHit mX = (RaycastHit)x;
            RaycastHit mY = (RaycastHit)y;
            if (mY.distance > mX.distance)
                return -1;
            else
                return 1;
        }

    }

    [Range (1, 10)]     //Consider setting static
    public static int length = 1;
    
//    [Range(0.1f, 2f)]
//    [Tooltip("Duration the fire stays on screen")]
    float fireTime = 1f;

    [Range(1f, 5f)]
    public float timeToExplode;

    public GameObject bombFirePrefab;
    public AudioClip explodeSound;

    private bool hasCollider = false;
    private bool bombTriggered = false;
    private Animator anim;

    private float distanceToNearestWall_left;
    private float distanceToNearestWall_right;
    private float distanceToNearestWall_forward;
    private float distanceToNearestWall_back;

    bool flagCheckPlayer = false;
    bool flagCheckEnemy = false;
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

    private ArrayList m_enemies;
    ArrayList Firelength_bomb_i;
    Vector3 myGridSize;
    Vector3 myLocationOfFirstCube;
    // Use this for initialization
    void Start() 
    {
        GameObject planeInfo = FindObjectOfType<Playground>().gameObject;
        m_enemies = planeInfo.GetComponent<CreateMap>().getEnemies();
        SetDefaultDistance();
        anim = GetComponent<Animator>();
        Debug.Log(anim);
        Firelength_bomb_i = new ArrayList();

        CreateMap myMapInfo = FindObjectOfType<Playground>().GetComponent<CreateMap>();
        myGridSize = myMapInfo.getGridSize();
        myLocationOfFirstCube = myMapInfo.getFirstLocationOfCube();
        Invoke("StartExplosionAnimation", timeToExplode);
    }

    // Update is called once per frame
    void Update() 
    {
        CreateCollider();
    }

    private void SetDefaultDistance()
    {
        distanceToNearestWall_left = length;
        distanceToNearestWall_right = length;
        distanceToNearestWall_forward = length;
        distanceToNearestWall_back = length;
    }

    Vector3 mPosToMapIndex(Vector3 iPos, Vector3 iLocationOfFirstCube, Vector3 iGridSize)
    {
        iPos = iPos - new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]);
        return new Vector3(iPos[0] / iGridSize[0], 0f, iPos[2] / iGridSize[2]);
    }

    //Used as animation event for now
    void BombExplode() 
    {
        AudioSource.PlayClipAtPoint(explodeSound, transform.position);
        BombImpact();
        if (flagCheckPlayer || flagCheckEnemy)
        {
            Vector3 mgrid_size = FindObjectOfType<CreateMap>().getGridSize();
            float max_size = Mathf.Max(mgrid_size[0],mgrid_size[2]);
            if (flagCheckPlayer)
            {
                FindObjectOfType<PlayerController>().setBombInfo(this.transform.position, fireTime, Firelength_bomb_i);
            }
            if (flagCheckEnemy)
            {
                for (int i = 0; i < m_enemies.Count; i++)
                {
                    GameObject m_enemy_i = (GameObject)m_enemies[i];
                    if (m_enemy_i != null)
                    {
                        m_enemy_i.GetComponent<EnemyScript>().setBombInfo(this.transform.position, fireTime, Firelength_bomb_i);
                        Vector3 bombPosition = mPosToMapIndex(transform.position, myLocationOfFirstCube, myGridSize);
                        bombPosition.x = Mathf.Round(bombPosition.x);
                        bombPosition.z = Mathf.Round(bombPosition.z);
                        m_enemy_i.GetComponent<EnemyScript>().updateMapBombPos(bombPosition, false);
                    }
                }
            }
        }
        
        Destroy(gameObject);

    }

    void StartExplosionAnimation() 
    {
        anim.SetBool("BombExplode", true);
    }

    void BombImpact() 
    {
        BombTriggered = true;

        DrawRaycast(Vector3.forward);
        DrawRaycast(Vector3.back);
        DrawRaycast(Vector3.right);
        DrawRaycast(Vector3.left);
        DrawRaycast(Vector3.down);

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
        
        fireLength = (10.0f/9.0f)*((fireLength)  * grid_size + grid_size/2);
        transform.GetComponent<ParticleSystem>().startSpeed = fireLength;
        transform.GetComponent<ParticleSystem>().startLifetime = iFireTime;
    }

    private void CreateFireParticles() 
    {
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
            pos -= 3*Vector3.down;
        float dis = (length * max_size + max_size/2) * fireTime;
        Ray impactRay = new Ray(pos, direction);

        RaycastHit[] hit = Physics.RaycastAll(impactRay, dis);
        ArrayList hit_arr = new ArrayList();
        for (int i = 0; i < hit.Length; i++)
            hit_arr.Add(hit[i]);
        if (hit_arr.Count > 0)
        {
            IComparer myComparer = new mySortClass();
            hit_arr.Sort(myComparer);
        }

        Debug.DrawLine(pos + new Vector3(0,1,0), new Vector3(0,1,0) + pos + direction * dis, Color.red);
        bool flag_continue = true;
        float animation_length = length;
        for (int i = 0; i < hit_arr.Count && flag_continue; i++)
        {
            RaycastHit m_hit = (RaycastHit)hit_arr[i];
            if (m_hit.collider.tag == "IndestructibleWall")
            {
                animation_length = GetDistanceToNearestWall(m_hit.collider.gameObject, direction);
                flag_continue = false;
            }
            else if (m_hit.collider.GetComponent<DestructibleWall>())//Change this to recognize 'DestructibleWall' class instead 
            {
                // if the actual fire length is smaller than planned fire length then the time that fire reach to the object is the portion of the planned time
                animation_length = GetDistanceToNearestWall(m_hit.collider.gameObject, direction);
                animation_length = ((animation_length) * max_size + max_size / 20);
                Firelength_bomb_i.Add(animation_length);
                float planned_length = ((length) * max_size + max_size / 2);
                float ratio_real_planned = animation_length / (planned_length * fireTime);

                for (int j = 0; j < m_enemies.Count; j++)
                {
                    GameObject m_enemy_j = (GameObject)m_enemies[j];
                    if (m_enemy_j != null)
                        m_enemy_j.GetComponent<EnemyScript>().mUpdateMyMap(m_hit.collider.gameObject.transform.position);
                }
                //Spawn a powerup based on its drop chance
                m_hit.collider.GetComponent<DestructibleWall>().SpawnAPowerUp();

                Destroy(m_hit.collider.gameObject, ratio_real_planned * fireTime);
                flag_continue = false;

            }
            flagCheckPlayer = true;
            flagCheckEnemy = true;
            //Bomb can explode other bombs
            if (m_hit.collider.GetComponent<Bomb>())
            {
                Bomb bomb = m_hit.collider.GetComponent<Bomb>();
                //Trigger only the bomb that has not been triggered
                if (!bomb.BombTriggered) 
                {
                    bomb.BombExplode();
                }

            }
        }
        Firelength_bomb_i.Add(animation_length * (max_size) + max_size / 2);
    }

    void CreateCollider() 
    {
        //now assume a tile grid size is (1, y, 1)
        if (!hasCollider) 
        {
            CreateMap mMap = FindObjectOfType<CreateMap>();
            Vector3 myGridSize = mMap.getGridSize();
            PlayerController myPlayer = FindObjectOfType<PlayerController>();
            Vector3 dis_to_bomb = (myPlayer.transform.position - transform.position);
            if (Mathf.Abs(dis_to_bomb[0]) < myGridSize[0] && Mathf.Abs(dis_to_bomb[2]) < myGridSize[2])
            {
                myPlayer.addEscapingTilePos(this.transform.position);
            }
            BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
            newCollider.center = Vector3.zero;
            newCollider.size = Vector3.one;
            hasCollider = true;
        }
    }
}
