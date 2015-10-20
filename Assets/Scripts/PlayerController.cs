using UnityEngine;
using System.Collections;
using System;
public class PlayerController : MonoBehaviour
{
    
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

    public enum myPointType{Enemy = 0, Cube = 1};

    [Range(1f, 10f)]
    public float speed;
    [Range(1, 10)]      /////////////////Add limit on bombs. Consider setting static
    public int maxNumberOfBombs;

    public GameObject bomb;
    public AudioClip[] footStepSound;
    public AudioClip placeBombSound;
    public AudioClip playerHurtSound;

    public int PlayerHealth = 5;
    float PlayerPoints = 0.0f;
    int numberOfKilledEnemy = 0;
    int numberOfDestroyedCubes = 0;

    private Animator anim;

    private GameObject planeInfo;
    private ArrayList m_enemy_i;

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

        //For setting up anim
        anim = GetComponent<Animator>();
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
        for (int i = 0; i < m_enemy_i.Count; i++)
        {
            GameObject mEnemy = (GameObject)m_enemy_i[i];
            if (mEnemy != null)
                ((GameObject)m_enemy_i[i]).GetComponent<EnemyScript>().setTargetPos(this.transform.position, true);
        }
        if (PlayerHealth <= 0)
        {
            FinishScenePlayerDied();
        }

        GoToNextLevel();
    }

    void FinishScenePlayerDied()///////////////////////////////////////////////////////////////////////////////////////////////////////Kourosh Finish scene
    {
        myMapInfo.LoadCurLevel();////////////////////////////////////////////////////////TODO: to sth when you died
    }

    void GoToNextLevel()///////////////////////////////////////////////////////////////////////////////////////////////////////Kourosh Next Level
    {
        if (myMapInfo.getNumberOfEnemies() == numberOfKilledEnemy)
        {
            //////////////////////////////////////////////////////////////TODO: go to next level
        }
    }

    int getMapVal(ArrayList iMap, int i, int j)
    {
        if (i >= iMap.Count || j >= ((ArrayList)iMap[0]).Count)
            return -1;
        ArrayList col_i = (ArrayList)iMap[i]; //each index_i in the map returns a column on the X direction
        return (int)col_i[j];
    }

    //Find the first available free tile
    Vector3 LocateFirstAvailableSpace(ArrayList iMap, Vector3 iLocationOfFirstCube, Vector3 iGridSize)
    {
        bool flag_continue = true;
        int index_i = 1;
        int index_j = 1;
        bool find_first = false;
        for (int i = 1; i < iMap.Count-1 && flag_continue; i++)
        {
            ArrayList iMap_col = (ArrayList)iMap[i];
            for (int j = 1; j < iMap_col.Count-1 && flag_continue; j++)
            {
                int val_ij = (int)iMap_col[j];
                if (val_ij == (int) CreateMap.GridType.Free)
                {
                    if (!find_first)
                    {
                        index_i = i;
                        index_j = j;
                        find_first = true;
                    }
                    if (getMapVal(iMap, i + 1, j) == (int)CreateMap.GridType.Free && getMapVal(iMap, i, j + 1) == (int)CreateMap.GridType.Free)
                    {
                        flag_continue = false;
                        index_i = i;
                        index_j = j;
                    }
                    if (getMapVal(iMap, i - 1, j) == (int)CreateMap.GridType.Free && getMapVal(iMap, i, j + 1) == (int)CreateMap.GridType.Free)
                    {
                        flag_continue = false;
                        index_i = i;
                        index_j = j;
                    }
                    if (getMapVal(iMap, i - 1, j) == (int)CreateMap.GridType.Free && getMapVal(iMap, i, j - 1) == (int)CreateMap.GridType.Free)
                    {
                        flag_continue = false;
                        index_i = i;
                        index_j = j;
                    }
                    if (getMapVal(iMap, i + 1, j) == (int)CreateMap.GridType.Free && getMapVal(iMap, i, j - 1) == (int)CreateMap.GridType.Free)
                    {
                        flag_continue = false;
                        index_i = i;
                        index_j = j;
                    }
                }
            }
        }
        return new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]) + new Vector3(index_i * iGridSize[0], 0f, index_j * iGridSize[2]);
    }

    public void setBombInfo(Vector3 iBombPos, float iFireTime, ArrayList iMaxLength)
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
            ((ArrayList) check_directions_forEachBomb[check_directions_forEachBomb.Count - 1]).Add(true);
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
                delta_time[i] = (float) (delta_time[i]) + Time.deltaTime;
                if ((float) (delta_time[i]) > 1.3f*(float) (bombFireTime[i]))
                {
                    check_hit_bomb[i] = false;
                }

                Vector3 p_pos = transform.position;
                float dis = (p_pos - (Vector3)last_bomb_pos[i]).magnitude;

                
                for (int j = 0; j < 5 && !mHit_flag; j++)
                {
                    if ((bool) ((ArrayList) check_directions_forEachBomb[i])[j] && dis <= (float)((ArrayList)bombFireLength[i])[j] && (bool)check_hit_bomb[i])
                    {
                        //cast rays on different directions
                        Vector3 mDir = getDirection(j);
                        Vector3 pos = (Vector3)last_bomb_pos[i];
                        float ray_dis = ((float)((ArrayList)bombFireLength[i])[j]) * (float)delta_time[i];
                        if ((float)delta_time[i] > 1.0f)
                            ray_dis = ((float)((ArrayList)bombFireLength[i])[j]);
                        if (mDir == Vector3.down)
                        {
                            ray_dis = 5 * ((float)((ArrayList)bombFireLength[i])[j]);
                            pos -= 3 * mDir;
                        }
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
                            Debug.DrawLine(pos, pos + mDir * ray_dis * (float) delta_time[i], Color.red);
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
                                ((ArrayList) check_directions_forEachBomb[i])[j] = false;
                            }
                        }
                    }//if direction j of bomb i
                }//for directions
            }

            for (int i = 0; i < check_hit_bomb.Count; i++)
            {
                if (!(bool) (check_hit_bomb[i]))
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
            {
                HitPlayer();
            }
        }
    }

    private void PlacePlayerOnMapAndGetInfo()
    {
        if (!flag_got_map_info)
        {
            planeInfo = FindObjectOfType<Playground>().gameObject;
            myMapInfo = planeInfo.GetComponent<CreateMap>();
            m_enemy_i = myMapInfo.getEnemies();
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

    bool DoesRaycastHitObject(Vector3 originPos)
    {
        Vector3 originPosMirror = new Vector3(originPos.x, -originPos.y, originPos.z);
        Ray newRay = new Ray(originPos, Vector3.down);      //Seems like Vector3.down works well

        RaycastHit[] hit = Physics.RaycastAll(newRay, (originPosMirror - originPos).magnitude);
        for (int i = 0; i < hit.Length; i++)
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
        if (DoesRaycastHitObject(p1))
        {
            return false;
        }

        //Draw a RayCast on upper left of player sprite
        p1 = iPos + new Vector3(-iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        if (DoesRaycastHitObject(p1))
        {
            return false;
        }

        //Draw a RayCast on lower right of player sprite
        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, -iPlayerSize[2] / 2);
        if (DoesRaycastHitObject(p1))
        {
            return false;
        }

        //Draw a RayCast on upper right of player sprite
        p1 = iPos + new Vector3(iPlayerSize[0] / 2, 1f, iPlayerSize[2] / 2);
        if (DoesRaycastHitObject(p1))
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
            anim.SetBool("LeftMovement", true);
            targetPos -= moveLeftRight;
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            anim.SetBool("LeftMovement", false);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            anim.SetBool("RightMovement", true);
            targetPos += moveLeftRight;
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            anim.SetBool("RightMovement", false);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            anim.SetBool("DownMovement", true);
            targetPos -= MoveForwardBackward;
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            anim.SetBool("DownMovement", false);
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            anim.SetBool("UpMovement", true);
            targetPos += MoveForwardBackward;
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            anim.SetBool("UpMovement", false);
        }

        if (isInsideFreeSpace(targetPos, new Vector3(this.transform.localScale[0] / 2, 0f, this.transform.localScale[2] / 10), targetPos - transform.position))
        {
            transform.position = targetPos;
        }
    }

    //used as an animation events
    void PlayFootstepSound_0()
    {
        AudioSource.PlayClipAtPoint(footStepSound[0], transform.position);
    }

    void PlayFootstepSound_1()
    {
        AudioSource.PlayClipAtPoint(footStepSound[1], transform.position);
    }

    Vector3 mPosToMapIndex(Vector3 iPos, Vector3 iLocationOfFirstCube, Vector3 iGridSize)
    {
        iPos = iPos - new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]);
        return new Vector3(iPos[0] / iGridSize[0], 0f, iPos[2] / iGridSize[2]);
    }

    Vector3 mMapIndexToPos(Vector3 iMapIndex, Vector3 iLocationOfFirstCube, Vector3 iGridSize)
    {
        Vector3 nPos = new Vector3(iMapIndex[0] * iGridSize[0], 0f, iMapIndex[2] * iGridSize[2]);
        return nPos + new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]) + new Vector3(0f, transform.position[1], 0f);
    }

    void SpawnBomb()
    {
        Vector3 bombPosition = mPosToMapIndex(transform.position, myLocationOfFirstCube, myGridSize);
        bombPosition.x = Mathf.Round(bombPosition.x);
        bombPosition.z = Mathf.Round(bombPosition.z);

        for (int i = 0; i < m_enemy_i.Count; i++)
        {
            GameObject mEnemy = (GameObject)m_enemy_i[i];
            if (mEnemy != null)
                ((GameObject)m_enemy_i[i]).GetComponent<EnemyScript>().updateMapBombPos(bombPosition, true);
        }

        bombPosition = mMapIndexToPos(bombPosition, myLocationOfFirstCube, myGridSize);
        //Bombs can only placed on the middle of a tile
        //Vector3 bombPosition = transform.position;
        
        //bombPosition = bombPosition - myLocationOfFirstCube;
        //Add a limit to the number of bombs placed
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

    public void HitPlayer()
    {
        this.transform.position = LocateFirstAvailableSpace(myMap, myLocationOfFirstCube, myGridSize) + new Vector3(0f, transform.position[1], 0f);
        PlayerHealth--;
    }

    public void IncreasePlayerPoint(float iPoints, myPointType iPointType)
    {
        PlayerPoints += iPoints;
        if (iPointType == myPointType.Enemy)
            numberOfKilledEnemy++;
        if (iPointType == myPointType.Cube)
            numberOfDestroyedCubes++;
    }
}
