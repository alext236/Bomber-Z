using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    [Range(1f, 10f)]
    public float speed;

    [Range (1f, 2f)]
    public float edgePadding;   //For putting walls or something on the side to limit player movement

    public GameObject bomb;
    public GameObject ground;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        KeyboardMovement();
        if (Input.GetKeyDown(KeyCode.Space)) {
            SpawnBomb();
        }
    }

    void KeyboardMovement() {
        //TODO: consider limit movement to one tile per key input
        //movement change depends on speed given
        Vector3 moveLeftRight = Vector3.right * speed * Time.deltaTime;
        Vector3 MoveForwardBackward = Vector3.forward * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.A)) {
            transform.position -= moveLeftRight;
        }

        if (Input.GetKey(KeyCode.D)) {
            transform.position += moveLeftRight;
        }
        
        if (Input.GetKey(KeyCode.S)) {
            transform.position -= MoveForwardBackward;
        }

        if (Input.GetKey(KeyCode.W)) {
            transform.position += MoveForwardBackward;
        }

        RestrictPosition();
    }
    
    void RestrictPosition() {
        Vector3 limitPos = transform.position;

        float xMin = edgePadding;
        float xMax = ground.transform.localScale.x;
        float zMin = edgePadding;
        float zMax = ground.transform.localScale.z;

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
