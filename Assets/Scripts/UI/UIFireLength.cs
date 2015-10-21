using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIFireLength : MonoBehaviour
{

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
        text.text = "Fire Length: " + Bomb.length;

    }
}
