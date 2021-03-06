﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIBombCount : MonoBehaviour
{
    private Text text;
    private int numberOfBombs;

    // Use this for initialization
    void Start()
    {
        numberOfBombs = FindObjectOfType<PlayerController>().maxNumberOfBombs;
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateText();
    }

    void UpdateText()
    {
        numberOfBombs = FindObjectOfType<PlayerController>().maxNumberOfBombs;
        text.text = "Bombs: " + numberOfBombs;

    }
}
