using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance = null;
    private AudioSource soundTrack;

    public AudioClip startMenu;
    public AudioClip mainGame;
    public AudioClip gameOver;

    void Awake()
    {
        if (instance != null)
        {
            GameObject.Destroy(gameObject);
            Debug.Log("A duplicate music player is destroyed.");
        }
        else
        {
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
            soundTrack = GetComponent<AudioSource>();
        }
    }

    public void Start()
    {
        soundTrack.clip = startMenu;
        soundTrack.loop = true;
        soundTrack.Play();
    }


    public void OnLevelWasLoaded(int level)
    {
        Debug.Log("Set music");
        if (level == 0)
        {            //Start Menu and Tutorial

            //soundTrack.clip = startMenu;
            //soundTrack.Play();
        }
        else if (level == 1)
        {      //Main Game
            if (mainGame)
            {
                soundTrack.clip = mainGame;
                soundTrack.Play();
            } else
            {
                Debug.LogError("No main game music");
            }            
        }
        //else if (level == 2)
        //{      //Game over
        //    if (gameOver)
        //    {
        //        soundTrack.clip = gameOver;
        //        soundTrack.Play();
        //    } else
        //    {
        //        Debug.LogError("No game over music");
        //    }
            
        //}
        //else if (level == 3)
        //{
        //    return;
        //}
    }
}
