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

            soundTrack.clip = mainGame;
            soundTrack.Play();
        }
        else if (level == 2)
        {      //Game over

            soundTrack.clip = gameOver;
            soundTrack.Play();
        }
        else if (level == 3)
        {
            return;
        }
    }
}
