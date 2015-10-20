using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EscapeDoor : MonoBehaviour {

    private LevelManager levelManager;
    private DestructibleWall[] walls;

    private bool isInit = false;

    private Text winningText;

    public AudioClip tunnelSound;

	// Use this for initialization
	void Start () {
        levelManager = FindObjectOfType<LevelManager>();
        winningText = GameObject.Find("Winning Text").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!isInit)
        {
            walls = FindObjectsOfType<DestructibleWall>();
            PlaceTheEscapeRandomly();
            isInit = true;
        }
	}

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() && isInit)
        {
            Debug.Log("Player enter trigger");
            AudioSource.PlayClipAtPoint(tunnelSound, transform.position);
            winningText.text = "You Win!";
            Invoke("Progress", 3f);
        }
    }

    void PlaceTheEscapeRandomly()
    {
        int random = Random.Range(0, walls.Length);
        transform.position = walls[random].transform.position;
    }

    void Progress()
    {
        levelManager.LoadNextLevel();
    }
}
