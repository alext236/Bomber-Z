using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIPlayerHealth : MonoBehaviour {

    private Text text;

    // Use this for initialization
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateText();
    }

    void UpdateText()
    {
        text.text = "Health: " + FindObjectOfType<PlayerController>().PlayerHealth;

    }
}
