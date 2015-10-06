using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {    //Camera keeps a definite distance from player

    private Camera movingCamera;
    private Vector3 offset;
    private PlayerController player;

	// Use this for initialization
	void Start () {
        movingCamera = Camera.main;
        player = FindObjectOfType<PlayerController>();

        offset = player.transform.position - movingCamera.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = player.transform.position - offset;
	}
}
