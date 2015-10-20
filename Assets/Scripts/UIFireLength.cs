using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIFireLength : MonoBehaviour
{

    public Text text;

    // Use this for initialization
    void Start()
    {

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
