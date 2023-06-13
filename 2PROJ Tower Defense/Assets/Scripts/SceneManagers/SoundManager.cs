using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SoundManager : MonoBehaviour
{
    public AudioSource src;

    // Audio clips
    public AudioClip background_music;
    public AudioClip click;

    string folder = "Sounds"; // Specify the folder path where the sound file is located
    public List<AudioClip> audioClips; // List to store the loaded AudioClips

    public bool isMusicEnabled = true;
    public bool isSoundEnabled = true;


    

    // Start is called before the first frame update
    void Start()
    {
        // Load the music and sound settings from PlayerPrefs or wherever you store them
        isMusicEnabled = bool.Parse(PlayerPrefs.GetString("SETTING_MUSIC", "false"));
        isSoundEnabled = bool.Parse(PlayerPrefs.GetString("SETTING_SOUND", "false"));



        if (isMusicEnabled)
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
        if (!isSoundEnabled)
        {
            return; // Don't play sound if sound is disabled
        }

        switch (sound)
        {
            case SoundList.background_music:
                src.clip = background_music;
                src.volume = 0.1f;
                src.Play();
                break;

            case SoundList.click:
            case SoundList.click_wrong:
                src.clip = click;
                src.time = 0.5f;
                src.volume = 1f;
                src.Play();
                break;
        }
    }

    public void StopSoundAll()
    {
        src.Stop();
    }
    public void toggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;
        PlayerPrefs.SetString("SETTING_MUSIC", isMusicEnabled.ToString());

        Debug.Log(isMusicEnabled);
        if (isMusicEnabled)
        {
            PlaySound(SoundList.background_music);
        }
        else
        {
            StopSoundAll();
        }

    }

    public void toggleSound()
    {
        isSoundEnabled = !isSoundEnabled;
        PlayerPrefs.SetString("SETTING_SOUND", isSoundEnabled.ToString());


        if (!isSoundEnabled)
        {
            StopSoundAll();
        }
        else
        {
            PlaySound("click");
        }
        

    }

    public enum SoundList
    {
        background_music,
        click,
        click_wrong
    }



}
