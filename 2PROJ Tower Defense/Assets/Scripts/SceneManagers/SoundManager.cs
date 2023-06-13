using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SoundManager : MonoBehaviour
{
    public AudioSource src;


    //Audio clips
    public AudioClip background_music;
    public AudioClip click;


    string folder = "Sounds"; // Specify the folder path where the sound file is located
    public List<AudioClip> audioClips; // List to store the loaded AudioClips


    // Start is called before the first frame update
    void Start()
    {

        if (bool.Parse(PlayerPrefs.GetString("SETTING_MUSIC", "false")))
        {
            PlaySound(SoundList.background_music);
        }
    }

    public void PlaySound(string soundName)
    {
        PlaySound((SoundList)Enum.Parse(typeof(SoundList), soundName));
    }

    public void PlaySound(SoundList sound)
    {
        switch (sound)
        {
            case SoundList.background_music:
                src.clip = background_music;
                src.volume = 0.1f;
                src.Play();
                break;

            case SoundList.click or SoundList.click_wrong:
                src.clip = click;
                src.time = 0.5f;
                src.volume = 1f;

                src.Play();
                break;
        }
        
    }

    public enum SoundList
    {
        background_music,
        click,
        click_wrong
    }

    
   

}
