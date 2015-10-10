using UnityEngine;
using System.Collections;
using System;
public class PlayerController : MonoBehaviour {
    
    [Range(1f, 10f)]
    public float speed;

    [Range (1f, 2f)]
    public float edgePadding;   //For putting walls or something on the side to limit player movement

    public GameObject bomb;

    private GameObject planeInfo;
    bool flag_got_map_info = false;
    CreateMap myMapInfo;
    Vector3 myGridSize;
    Vector3 myLocationOfFirstCube;
    ArrayList myMap;

    // Use this for initialization
    void Start() {
        planeInfo = FindObjectOfType<Playground>().gameObject;
        
    }

    //Find the first available free tile
    Vector3 LocateFirstAvailableSpace(ArrayList iMap, Vector3 iLocationOfFirstCube, Vector3 iGridSize) {
        bool flag_continue = true;
        int index_i = 0;
        int index_j = 0;
        for (int i = 0; i < iMap.Count && flag_continue; i++) {
            ArrayList iMap_col = (ArrayList)iMap[i];
            for (int j = 0; j < iMap_col.Count && flag_continue; j++) {
                int val_ij = (int)iMap_col[j];
                if (val_ij == (int) CreateMap.GridType.Free) {
                    flag_continue = false;
                    index_i = i;
                    index_j = j;
                }
            }
        }
        return new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]) + new Vector3(index_i * iGridSize[0], 0f, index_j * iGridSize[2]);
    }

    // Update is called once per frame
    void Update() {
        PlacePlayerOnMap();
        KeyboardMovement();

        if (Input.GetKeyDown(KeyCode.Space)) {
            SpawnBomb();
        }
    }

    private void PlacePlayerOnMap() {
        if (!flag_got_map_info) {
            myMapInfo = planeInfo.GetComponent<CreateMap>();
            myGridSize = myMapInfo.getGridSize();
            myLocationOfFirstCube = myMapInfo.getFirstLocationOfCube();
            myMap = myMapInfo.GetMyMap();

            //Reduce player size if player size is bigger than grid size
            Vector3 player_size = this.transform.localScale;
            if (player_size[0] > myGridSize[0])
                player_size[0] = myGridSize[0] - myGridSize[0] / 10;
            if (player_size[2] > myGridSize[2])
                player_size[2] = myGridSize[2] - myGridSize[2] / 10;
            this.transform.localScale = player_size;

            this.transform.position = LocateFirstAvailableSpace(myMap, myLocationOfFirstCube, myGridSize) + new Vector3(0f, transform.position[1], 0f);
            //+new Vector3(this.transform.localScale[0] / 2, 0f, this.transform.localScale[2] / 2);

            flag_got_map_info = true;
        }
    }

    //Refactored into one function instead of 4 different repeating codes
    bool DoesRaycastHitWall(Vector3 originPos) {
        RaycastHit hit;
        Vector3 originPosMirror = new Vector3(originPos.x, -originPos.y, originPos.z);
        Ray newRay = new Ray(originPos, Vector3.down);      //Seems like Vector3.down works well

        if (Physics.Raycast(newRay, out hit, (originPosMirror - originPos).magnitude)) {
            Debug.DrawRay(originPos, Vector3.down, Color.red);
            if (hit.transform.name != "Grass Playground") {
                return true;
            }
        }

        return false;
    }

    bool isInsideFreeSpace(Vector3 iPos, Vector3 iPlayerSize) {
        //Draw a RayCast on lower left of player sprite
        Vector3 p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        if (DoesRaycastHitWall(p1)) {
            return false;
        }

        //Draw a RayCast on upper left of player sprite
        p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        if (DoesRaycastHitWall(p1)) {
            return false;
        }

        //Draw a RayCast on lower right of player sprite
        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        if (DoesRaycastHitWall(p1)) {
            return false;
        }

        //Draw a RayCast on upper right of player sprite
        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        if (DoesRaycastHitWall(p1)) {
            return false;
        }

        return true;
    }

    void KeyboardMovement() {
        //movement change depends on speed given
        Vector3 moveLeftRight = Vector3.right * speed * Time.deltaTime;
        Vector3 MoveForwardBackward = Vector3.forward * speed * Time.deltaTime;

        Vector3 targetPos = transform.position;
        if (Input.GetKey(KeyCode.A)) {
            targetPos -= moveLeftRight;
        }

        if (Input.GetKey(KeyCode.D)) {
            targetPos += moveLeftRight;
        }

        if (Input.GetKey(KeyCode.S)) {
            targetPos -= MoveForwardBackward;
        }

        if (Input.GetKey(KeyCode.W)) {
            targetPos += MoveForwardBackward;
        }

        if (isInsideFreeSpace(targetPos, new Vector3(this.transform.localScale[0] / 2, 0f, this.transform.localScale[2] / 10))) {
            transform.position = targetPos;
        }
    }

    //Obsolete code
    void RestrictPosition() {
        Vector3 limitPos = transform.position;

        float xMin = edgePadding;
        float xMax = planeInfo.transform.localScale.x;
        float zMin = edgePadding;
        float zMax = planeInfo.transform.localScale.z;

        limitPos.x = Mathf.Clamp(limitPos.x, xMin, xMax);
        limitPos.z = Mathf.Clamp(limitPos.z, zMin, zMax);

        transform.position = limitPos;

    }

    void SpawnBomb() {
        //Bombs can only placed on the middle of a tile
        Vector3 bombPosition = transform.position;
        bombPosition.x = Mathf.Round(bombPosition.x);
        bombPosition.z = Mathf.Round(bombPosition.z);

        foreach (Bomb bombPlaced in FindObjectsOfType<Bomb>()) {
            if (bombPlaced.transform.position == bombPosition) {
                return;
            }
        }
        GameObject newBomb = Instantiate(bomb, bombPosition, Quaternion.identity) as GameObject;
    }
}
