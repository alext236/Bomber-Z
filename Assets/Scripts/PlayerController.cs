using UnityEngine;
using System.Collections;
using System;
public class PlayerController : MonoBehaviour {
    public GameObject planeInfo;
    [Range(1f, 10f)]
    public float speed;

    [Range (1f, 2f)]
    public float edgePadding;   //For putting walls or something on the side to limit player movement

    public GameObject bomb;

    bool flag_got_map_info = false;
    CreateMap myMapInfo;
    Vector3 myGridSize;
    Vector3 myLocationOfFirstCube;
    ArrayList myMap;
    // Use this for initialization
    void Start() {

    }

    Vector3 mLocatePlayerRandomly(ArrayList iMap, Vector3 iLocationOfFirstCube, Vector3 iGridSize)
    {
        bool flag_continue = true;
        int index_i = 0;
        int index_j = 0;
        for (int i = 0; i < iMap.Count && flag_continue; i++)
        {
            ArrayList iMap_col = (ArrayList)iMap[i];
            for (int j = 0; j < iMap_col.Count && flag_continue; j++)
            {
                int val_ij = (int)iMap_col[j];
                if (val_ij == (int)CreateMap.GridType.Free)
                {
                    flag_continue = false;
                    index_i = i;
                    index_j = j;
                }
            }
        }
        return new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]) + new Vector3(index_i * iGridSize[0], 0f, index_j * iGridSize[2]);
    }

    // Update is called once per frame
    void Update() 
    {
        if (!flag_got_map_info)
        {
            myMapInfo = planeInfo.GetComponent<CreateMap>();
            myGridSize = myMapInfo.getGridSize();
            myLocationOfFirstCube = myMapInfo.getFirstLocationOfCube();
            myMap = myMapInfo.GetMyMap();

            Vector3 player_size = this.transform.localScale;
            if (player_size[0] > myGridSize[0])
                player_size[0] = myGridSize[0] - myGridSize[0] / 10;
            if (player_size[2] > myGridSize[2])
                player_size[2] = myGridSize[2] - myGridSize[2] / 10;
            this.transform.localScale = player_size;

            this.transform.position = mLocatePlayerRandomly(myMap, myLocationOfFirstCube, myGridSize) + new Vector3(0f, transform.position[1], 0f);
            // +new Vector3(this.transform.localScale[0] / 2, 0f, this.transform.localScale[2] / 2);

            flag_got_map_info = true;
        }
        KeyboardMovement();
        if (Input.GetKeyDown(KeyCode.Space)) {
            SpawnBomb();
        }
    }

    bool isInsideFreeSpace(Vector3 iPos, Vector3 iPlayerSize)
    {
        RaycastHit hit;

        Vector3 p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        Vector3 p2 = new Vector3(p1[0], -p1[1], p1[2]);
        if (Physics.Raycast(p1, (p2 - p1) / (p2 - p1).magnitude, out hit, (p2 - p1).magnitude))
        {
            Transform objectHit = hit.transform;
            if (objectHit.name != "Grass Playground")
            {
                return false;
            }
        }

        p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        p2 = new Vector3(p1[0], -p1[1], p1[2]);
        if (Physics.Raycast(p1, (p2 - p1) / (p2 - p1).magnitude, out hit, (p2 - p1).magnitude))
        {
            Transform objectHit = hit.transform;
            if (objectHit.name != "Grass Playground")
            {
                return false;
            }
        }

        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        p2 = new Vector3(p1[0], -p1[1], p1[2]);
        if (Physics.Raycast(p1, (p2 - p1) / (p2 - p1).magnitude, out hit, (p2 - p1).magnitude))
        {
            Transform objectHit = hit.transform;
            if (objectHit.name != "Grass Playground")
            {
                return false;
            }
        }

        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        p2 = new Vector3(p1[0], -p1[1], p1[2]);
        if (Physics.Raycast(p1, (p2 - p1) / (p2 - p1).magnitude, out hit, (p2 - p1).magnitude))
        {
            Transform objectHit = hit.transform;
            if (objectHit.name != "Grass Playground")
            {
                return false;
            }
        }

        return true;
    }

    void KeyboardMovement() {
        //movement change depends on speed given
        Vector3 moveLeftRight = Vector3.right * speed * Time.deltaTime;
        Vector3 MoveForwardBackward = Vector3.forward * speed * Time.deltaTime;

        Vector3 nPos = transform.position;
        if (Input.GetKey(KeyCode.A)) {
            nPos -= moveLeftRight;
        }

        if (Input.GetKey(KeyCode.D)) {
            nPos += moveLeftRight;
        }
        
        if (Input.GetKey(KeyCode.S)) {
            nPos -= MoveForwardBackward;
        }

        if (Input.GetKey(KeyCode.W)) {
            nPos += MoveForwardBackward;
        }

        if (isInsideFreeSpace(nPos, new Vector3(this.transform.localScale[0]/2, 0f, this.transform.localScale[2]/10)))
        {
            transform.position = nPos;
        }
    }
    
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
        GameObject newBomb = Instantiate(bomb, bombPosition, Quaternion.identity) as GameObject;
    }
}
