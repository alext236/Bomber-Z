using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    [Range(1f, 10f)]
    public float speed;

    [Range (0f, 2f)]
    public float edgePadding;   //For putting walls or something on the side to limit player movement

    public GameObject bomb;

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

        //RestrictPosition();
    }
    //TODO: change this to suit 3D space
    void RestrictPosition() {
        Vector3 limitPos = transform.position;
        Camera camera = Camera.main;

        float xMin = camera.ViewportToWorldPoint(new Vector3(0, 0)).x + edgePadding;
        float xMax = camera.ViewportToWorldPoint(new Vector3(1, 1)).x - edgePadding;
        float yMin = camera.ViewportToWorldPoint(new Vector3(0, 0)).y + edgePadding;
        float yMax = camera.ViewportToWorldPoint(new Vector3(1, 1)).y - edgePadding;

        limitPos.x = Mathf.Clamp(limitPos.x, xMin, xMax);
        limitPos.y = Mathf.Clamp(limitPos.y, yMin, yMax);
        //this is a better way than directly giving values for xMin, xMax etc for when we change sprite size
        //we only need to adjust the edge padding variable

        transform.position = limitPos;

    }

    void SpawnBomb() {
        GameObject newBomb = Instantiate(bomb, transform.position, Quaternion.identity) as GameObject;
    }
}
