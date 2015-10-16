using UnityEngine;
using System.Collections;
using System;
public class PlayerController : MonoBehaviour {

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

    [Range(1f, 10f)]
    public float speed;
    [Range(1, 10)]      /////////////////Add limit on bombs
    public int maxNumberOfBombs;

    public GameObject bomb;
    public AudioClip footStepSound;
    public AudioClip placeBombSound;
    public AudioClip playerHurtSound;

    private GameObject planeInfo;
    private ArrayList m_enemy_i_script;
    
    bool flag_got_map_info = false;
    CreateMap myMapInfo;
    Vector3 myGridSize;
    Vector3 myLocationOfFirstCube;
    ArrayList myMap;

    //bomb variables
    ArrayList delta_time;//float
    ArrayList check_hit_bomb;//bool
    ArrayList check_directions_forEachBomb;//arrayList
    ArrayList last_bomb_pos;//Vector3
    ArrayList bombFireLength;//float
    ArrayList bombFireTime;//float
    //bomb escaping
    Vector3 EscapeTileBombPos;
    // Use this for initialization
    void Start() 
    {
        delta_time = new ArrayList();
        check_hit_bomb = new ArrayList();
        check_directions_forEachBomb = new ArrayList();
        last_bomb_pos = new ArrayList();
        bombFireLength = new ArrayList();
        bombFireTime = new ArrayList();

        planeInfo = FindObjectOfType<Playground>().gameObject;
        m_enemy_i_script = new ArrayList();
        m_enemy_i_script.Add(GameObject.Find("Enemy").GetComponent<EnemyScript>());
        m_enemy_i_script.Add(GameObject.Find("Enemy 1").GetComponent<EnemyScript>());
    }

    // Update is called once per frame
    void Update() 
    {
        PlacePlayerOnMapAndGetInfo();
        KeyboardMovement();

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            SpawnBomb();
        }

        checkBombHitPlayer();

        if (EscapeTileBombPos[1] >= 0)
        {
            Vector3 dis_player_bomb = transform.position - EscapeTileBombPos;
            if (!(Mathf.Abs(dis_player_bomb[0]) < myGridSize[0] && Mathf.Abs(dis_player_bomb[2]) < myGridSize[2]))
            {
                EscapeTileBombPos[1] = -1;
            }
        }
        for (int i = 0; i < m_enemy_i_script.Count; i++)
        {
            ((EnemyScript)m_enemy_i_script[i]).setTargetPos(this.transform.position);
        }
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

    public void setBombInfo(Vector3 iBombPos, float iFireTime, float iMaxLength)
    {
        //check_hit_bomb
        if (check_hit_bomb == null)
        {
            check_hit_bomb = new ArrayList();
        }
        check_hit_bomb.Add(true);

        //check_directions_forEachBomb
        if (check_directions_forEachBomb == null)
        {
            check_directions_forEachBomb = new ArrayList();
        }
        check_directions_forEachBomb.Add(new ArrayList());
        for (int i = 0; i < 5; i++)
        {
            ((ArrayList)check_directions_forEachBomb[check_directions_forEachBomb.Count - 1]).Add(true);
        }

        //delta_time
        if (delta_time == null)
        {
            delta_time = new ArrayList();
        }
        delta_time.Add(0.15f);// because it takes 0.11 sec to traverse half of the grid_size

        //last_bomb_pos
        if (last_bomb_pos == null)
        {
            last_bomb_pos = new ArrayList();
        }
        last_bomb_pos.Add(iBombPos);

        //bombFireLength
        if (bombFireLength == null)
        {
            bombFireLength = new ArrayList();
        }
        bombFireLength.Add(iMaxLength);

        //bombFireTime
        if (bombFireTime == null)
        {
            bombFireTime = new ArrayList();
        }
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
            case 4:
                return Vector3.down;
        }
        return Vector3.up;
    }

    private void checkBombHitPlayer()
    {
        if (check_hit_bomb == null)
            return;
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

                if (dis <= (float)bombFireLength[i] && (bool)check_hit_bomb[i])
                {
                    for (int j = 0; j < 5 && !mHit_flag; j++)
                    {
                        if ((bool)((ArrayList)check_directions_forEachBomb[i])[j])
                        {
                            //cast rays on different directions
                            Vector3 mDir = getDirection(j);
                            Vector3 pos = (Vector3)last_bomb_pos[i];
                            if (mDir == Vector3.down)
                                pos -= 3 * mDir;
                            float ray_dis = ((float)bombFireLength[i]) * (float)delta_time[i];
                            Ray impactRay = new Ray(pos, mDir);
                            RaycastHit[] hit = Physics.RaycastAll(impactRay, ray_dis);

                            //sort rays based on the distance of objects
                            ArrayList hit_arr = new ArrayList();
                            for (int k = 0; k < hit.Length; k++)
                                hit_arr.Add(hit[k]);
                            if (hit_arr.Count > 0)
                            {
                                IComparer myComparer = new mySortClass();
                                hit_arr.Sort(myComparer);
                            }

                            bool flag_continue_on_ray = true;
                            for (int k = 0; k < hit_arr.Count && flag_continue_on_ray; k++)
                            {
                                RaycastHit m_hit = (RaycastHit)hit_arr[k];
                                Debug.DrawLine(pos, pos + mDir * ray_dis * (float)delta_time[i], Color.red);
                                if (m_hit.transform.name == "Player")
                                {
                                    //Do something to the player
                                    Debug.Log("The player is hit by the bomb ");
                                    AudioSource.PlayClipAtPoint(playerHurtSound, transform.position);

                                    mHit_flag = true;
                                    check_hit_bomb[i] = false;
                                    flag_continue_on_ray = false;
                                }
                                if (m_hit.transform.name == "Cube")
                                {
                                    flag_continue_on_ray = false;
                                    ((ArrayList)check_directions_forEachBomb[i])[j] = false;
                                }
                            }
                        }//if direction j of bomb i
                    }//for directions
                }//if inside dis of bomb i
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
                    check_directions_forEachBomb.RemoveAt(i);
                    i--;
                }
            }
            
            if (mHit_flag)
                this.transform.position = LocateFirstAvailableSpace(myMap, myLocationOfFirstCube, myGridSize) + new Vector3(0f, transform.position[1], 0f);
        }
    }

    private void PlacePlayerOnMapAndGetInfo() 
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

    bool DoesRaycastHitObject(Vector3 originPos, Vector3 iPos, Vector3 iLastPos) 
    {
        Vector3 originPosMirror = new Vector3(originPos.x, -originPos.y, originPos.z);
        Ray newRay = new Ray(originPos, Vector3.down);      //Seems like Vector3.down works well

        RaycastHit[] hit = Physics.RaycastAll(newRay, (originPosMirror - originPos).magnitude);
        for (int i=0; i < hit.Length; i++) 
        {
            Debug.DrawRay(originPos, Vector3.down, Color.red);
            if (hit[i].transform.name == "Cube") //wall or indestructible wall
            {
                return true;
            }
            if (hit[i].transform.name == "Bomb(Clone)" || hit[i].transform.name == "Bomb")
            {
                if (EscapeTileBombPos[1] >= 0)//if there is a tile that the player can get away from it
                {
                    Vector3 dis_player_bomb = transform.position - EscapeTileBombPos;
                    if (Mathf.Abs(dis_player_bomb[0]) < myGridSize[0] && Mathf.Abs(dis_player_bomb[2]) < myGridSize[2])
                    {
                        return false;//player can get away from bomb
                    }
                    else
                        return true;//player cannot get closer to the bomb
                }
                else
                    return true;//player cannot get closer to the bomb
            }
        }

        return false;
    }

    bool isInsideFreeSpace(Vector3 iPos, Vector3 iPlayerSize, Vector3 iDisplacement) 
    {
        Vector3 last_pos = iPos - iDisplacement;
        //Draw a RayCast on lower left of player sprite
        Vector3 p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        if (DoesRaycastHitObject(p1, iPos, last_pos)) 
        {
            return false;
        }

        //Draw a RayCast on upper left of player sprite
        p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        if (DoesRaycastHitObject(p1, iPos, last_pos)) 
        {
            return false;
        }

        //Draw a RayCast on lower right of player sprite
        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        if (DoesRaycastHitObject(p1, iPos, last_pos)) 
        {
            return false;
        }

        //Draw a RayCast on upper right of player sprite
        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        if (DoesRaycastHitObject(p1, iPos, last_pos)) 
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

        if (isInsideFreeSpace(targetPos, new Vector3(this.transform.localScale[0] / 2, 0f, this.transform.localScale[2] / 10), targetPos - transform.position)) 
        {
            
            transform.position = targetPos;
        }
    }

    //used as an animation event once we have movement animation
    void PlayFootstepSound()
    {
        AudioSource.PlayClipAtPoint(footStepSound, transform.position);
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
        AudioSource.PlayClipAtPoint(placeBombSound, newBomb.transform.position);
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

    public void addEscapingTilePos(Vector3 iTilePos)
    {
        EscapeTileBombPos = iTilePos;
    }
}
