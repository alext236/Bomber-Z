using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    [Range(1f, 10f)]
    public float speed;

    [Range (0f, 2f)]
    public float edgePadding;   //For putting walls or something on the side to limit player movement

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        KeyboardMovement();
    }

    void KeyboardMovement() {
        //movement change depends on speed given
        Vector3 moveLeftRight = Vector3.right * speed * Time.deltaTime;
        Vector3 moveUpDown = Vector3.up * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.A)) {
            transform.position -= moveLeftRight;
        }

        if (Input.GetKey(KeyCode.D)) {
            transform.position += moveLeftRight;
        }
        
        if (Input.GetKey(KeyCode.S)) {
            transform.position -= moveUpDown;
        }

        if (Input.GetKey(KeyCode.W)) {
            transform.position += moveUpDown;
        }

        RestrictPosition();
    }

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
}
