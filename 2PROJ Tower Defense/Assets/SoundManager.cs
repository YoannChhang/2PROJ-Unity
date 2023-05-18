using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SoundManager : MonoBehaviour
{
    public AudioClip backgroundMusic;
    public AudioSource src;



    string folder = "Sounds"; // Specify the folder path where the sound file is located
    public List<AudioClip> audioClips; // List to store the loaded AudioClips


    // Start is called before the first frame update
    void Start()
    {
        LoadAudioClips();


        if (bool.Parse(PlayerPrefs.GetString("SETTING_MUSIC", "false")))
        {
            src.clip = backgroundMusic;
            src.volume = 0.1f;
            src.Play();
        }
    }


    public void PlaySoundByFileName(string fileName)
    {

        foreach (AudioClip audioClip in audioClips)
        {
            if (audioClip.name == fileName)
            {
                src.clip = audioClip;
                src.Play();
            }
        }

        Debug.LogError("AudioClip not found: " + fileName);



    }
    void LoadAudioClips()
    {
        string folderPath = Path.Combine(Application.dataPath, folder); // Construct the full file path
        string[] files = Directory.GetFiles(folderPath, "*.mp3"); // Change the file extension to match your audio file types

        foreach (string file in files)
        {
            StartCoroutine(LoadAudioClip(file));
        }
        Debug.Log(audioClips.Count);
    }

    
    IEnumerator LoadAudioClip(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                yield break;
            }
            else
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                Debug.Log("Adding : " + myClip.name);
                audioClips.Add(myClip);
            }
        }
    }

}
