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

    public bool BombTriggered {
        get {
            return bombTriggered;
        }

        set {
            bombTriggered = value;
        }
    }


    // Use this for initialization
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
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
        CreateFireParticles();

        DrawRaycast(Vector3.left);
        DrawRaycast(Vector3.right);
        DrawRaycast(Vector3.forward);
        DrawRaycast(Vector3.back);

    }

    private void CreateFireParticles() {        
        GameObject fire = Instantiate(bombFirePrefab, transform.position, Quaternion.identity) as GameObject;
        //fire.transform.SetParent(transform);

        foreach (Transform child in fire.transform) {
            Debug.Log("particle element changes");
            child.GetComponent<ParticleSystem>().startLifetime = fireTime;
            //startSpeed 2.5 equals one tile fire length, or 1 + 0.5 in tile length
            float fireLength = (length + 0.5f) * 2.5f / 1.5f;
            child.GetComponent<ParticleSystem>().startSpeed = fireLength;
            //fireLength should also be equal to the distance from the bomb to the nearest wall it hits
        }

        Destroy(fire, fireTime+1f);
    }

    void DrawRaycast(Vector3 direction) {
        RaycastHit hit;

        Ray impactRay = new Ray(transform.position, direction);
        if (Physics.Raycast(impactRay, out hit, length)) {
            if (hit.collider.tag == "Wall") {       //TODO: Change this tag to appropriate tag later
                Destroy(hit.collider.gameObject);
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
