using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHelper : MonoBehaviour


{

    public GameObject soundOnImage;
    public GameObject soundOffImage;
    public GameObject musicOnImage;
    public GameObject musicOffImage;

    private void Update()
    {
        UpdateSoundButtonState();
        UpdateMusicButtonState();
    }

    public void toggleMusic()
    {
        FindObjectOfType<SoundManager>().toggleMusic();
    }

    public void toggleSound()
    {
        PlayerPrefs.SetString("SETTING_SOUND", (!bool.Parse(PlayerPrefs.GetString("SETTING_SOUND", "false"))).ToString());
        FindObjectOfType<SoundManager>().toggleSound();
    }

    private void UpdateSoundButtonState()
    {
        if (FindObjectOfType<SoundManager>().isSoundEnabled)
        {
            soundOnImage.SetActive(true);
            soundOffImage.SetActive(false);
        }
        else
        {
            soundOnImage.SetActive(false);
            soundOffImage.SetActive(true);
        }
    }

    private void UpdateMusicButtonState()
    {
        if (FindObjectOfType<SoundManager>().isMusicEnabled)
        {
            musicOnImage.SetActive(true);
            musicOffImage.SetActive(false);
        }
        else
        {
            musicOnImage.SetActive(false);
            musicOffImage.SetActive(true);
        }
    }
}
