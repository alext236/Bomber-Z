using UnityEngine;
using System.Collections;
using System;
public class PlayerController : MonoBehaviour {

    [Range(1f, 10f)]
    public float speed;
    [Range(1, 10)]      /////////////////Add limit on bombs
    public int maxNumberOfBombs;

    public GameObject bomb;

    private GameObject planeInfo;
    bool flag_got_map_info = false;
    CreateMap myMapInfo;
    Vector3 myGridSize;
    Vector3 myLocationOfFirstCube;
    ArrayList myMap;

    ArrayList delta_time;//float
    ArrayList check_hit_bomb;//bool
    ArrayList last_bomb_pos;//Vector3
    ArrayList bombFireLength;//float
    ArrayList bombFireTime;//float
    // Use this for initialization
    void Start() 
    {
        delta_time = new ArrayList();
        check_hit_bomb = new ArrayList();
        last_bomb_pos = new ArrayList();
        bombFireLength = new ArrayList();
        bombFireTime = new ArrayList();

        planeInfo = FindObjectOfType<Playground>().gameObject;
    }

    //Find the first available free tile
    Vector3 LocateFirstAvailableSpace(ArrayList iMap, Vector3 iLocationOfFirstCube, Vector3 iGridSize) 
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
                if (val_ij == (int) CreateMap.GridType.Free) 
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
        PlacePlayerOnMap();
        KeyboardMovement();

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            SpawnBomb();
        }

        checkBombHitPlayer();
    }

    public void setBombInfo(Vector3 iBombPos, float iFireTime, float iMaxLength)
    {
        check_hit_bomb.Add(true);
        delta_time.Add(0.2f);
        last_bomb_pos.Add(iBombPos);
        bombFireLength.Add(iMaxLength);
        bombFireTime.Add(iFireTime);
    }

    Vector3 getDirection(int iDir_i)
    {
        switch (iDir_i)
        {
            case 0:
                return Vector3.forward;
            case 1:
                return Vector3.back;
            case 2:
                return Vector3.right;
            case 3:
                return Vector3.left;
        }
        return Vector3.up;
    }

    private void checkBombHitPlayer()
    {
        if (check_hit_bomb.Count > 0)
        {
            bool mHit_flag = false;
            for (int i = 0; i < check_hit_bomb.Count && !mHit_flag; i++)
            {
                delta_time[i] = (float)(delta_time[i]) + Time.deltaTime;
                if ((float)(delta_time[i]) > (float)(bombFireTime[i]))
                {
                    check_hit_bomb[i] = false;
                }

                Vector3 p_pos = transform.position;
                float dis = (p_pos - (Vector3)last_bomb_pos[i]).magnitude;

                if (dis <= (float)bombFireLength[i])
                {
                    for (int j = 0; j < 4 && !mHit_flag; j++)
                    {
                        Vector3 mDir = getDirection(j);
                        Ray impactRay = new Ray((Vector3)last_bomb_pos[i] - mDir, mDir);
                        RaycastHit hit;
                        if (Physics.Raycast(impactRay, out hit, ((float)bombFireLength[i] + mDir.magnitude) * (float)delta_time[i]))
                        {
                            //Do something to the player
                            Debug.Log("The player is hit by the bomb ");

                            //check_hit_bomb = false;
                            mHit_flag = true;
                            check_hit_bomb.Clear();
                            delta_time.Clear();
                            last_bomb_pos.Clear();
                            bombFireLength.Clear();
                            bombFireTime.Clear();
                        }
                    }
                }
            }

            for (int i = 0; i < check_hit_bomb.Count; i++)
            {
                if (!(bool)(check_hit_bomb[i]))
                {
                    check_hit_bomb.RemoveAt(i);
                    delta_time.RemoveAt(i);
                    last_bomb_pos.RemoveAt(i);
                    bombFireLength.RemoveAt(i);
                    bombFireTime.RemoveAt(i);
                }
            }
            
            if (mHit_flag)
                this.transform.position = LocateFirstAvailableSpace(myMap, myLocationOfFirstCube, myGridSize) + new Vector3(0f, transform.position[1], 0f);
        }
    }

    private void PlacePlayerOnMap() 
    {
        if (!flag_got_map_info) 
        {
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

    bool DoesRaycastHitWall(Vector3 originPos) 
    {
        Vector3 originPosMirror = new Vector3(originPos.x, -originPos.y, originPos.z);
        Ray newRay = new Ray(originPos, Vector3.down);      //Seems like Vector3.down works well

        RaycastHit[] hit = Physics.RaycastAll(newRay, (originPosMirror - originPos).magnitude);
        for (int i=0; i<hit.Length; i++) 
        {
            Debug.DrawRay(originPos, Vector3.down, Color.red);
            if (hit[i].transform.name == "Cube") 
            {
                return true;
            }
        }

        return false;
    }

    bool isInsideFreeSpace(Vector3 iPos, Vector3 iPlayerSize) 
    {
        //Draw a RayCast on lower left of player sprite
        Vector3 p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        if (DoesRaycastHitWall(p1)) 
        {
            return false;
        }

        //Draw a RayCast on upper left of player sprite
        p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        if (DoesRaycastHitWall(p1)) 
        {
            return false;
        }

        //Draw a RayCast on lower right of player sprite
        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        if (DoesRaycastHitWall(p1)) 
        {
            return false;
        }

        //Draw a RayCast on upper right of player sprite
        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        if (DoesRaycastHitWall(p1)) 
        {
            return false;
        }

        return true;
    }

    void KeyboardMovement() 
    {
        //movement change depends on speed given
        Vector3 moveLeftRight = Vector3.right * speed * Time.deltaTime;
        Vector3 MoveForwardBackward = Vector3.forward * speed * Time.deltaTime;

        Vector3 targetPos = transform.position;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) 
        {
            targetPos -= moveLeftRight;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) 
        {
            targetPos += moveLeftRight;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) 
        {
            targetPos -= MoveForwardBackward;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) 
        {
            targetPos += MoveForwardBackward;
        }

        if (isInsideFreeSpace(targetPos, new Vector3(this.transform.localScale[0] / 2, 0f, this.transform.localScale[2] / 10))) 
        {
            transform.position = targetPos;
        }
    }

    void SpawnBomb() 
    {
        //Bombs can only placed on the middle of a tile
        Vector3 bombPosition = transform.position;
        bombPosition.x = Mathf.Round(bombPosition.x);
        bombPosition.z = Mathf.Round(bombPosition.z);
        ////////////////////Add a limit to the number of bombs placed
        int count = 0;
        foreach (Bomb bombPlaced in FindObjectsOfType<Bomb>()) 
        {
            count++;
            if (count >= maxNumberOfBombs) 
            {
                return;
            }
            if (bombPlaced.transform.position == bombPosition) 
            {
                return;
            }
        }

        //Sort the bombs to one parent folder to be neat
        GameObject newBomb = Instantiate(bomb, bombPosition, Quaternion.identity) as GameObject;
        if (GameObject.Find("Bomb Parent")) 
        {
            newBomb.transform.SetParent(GameObject.Find("Bomb Parent").transform);
        }
        else 
        {
            GameObject parent = new GameObject("Bomb Parent");
            newBomb.transform.SetParent(parent.transform);
        }
    }
}
