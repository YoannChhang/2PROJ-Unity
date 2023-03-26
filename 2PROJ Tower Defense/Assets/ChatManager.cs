using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : NetworkBehaviour
{

    public int maxHistory = 25;
    public GameObject chatPannel;
    public GameObject chatMessagePrefab;
    public TMP_InputField textInput;

    public List<GameObject> chatMessages;

    void Awake()
    {

    }

    public override void OnNetworkSpawn()
    {
        GameObject.Find("ChatManager").GetComponent<ChatManager>().SendMessageServerRpc($"{PlayerPrefs.GetString("PLAYER_NAME")} joined the lobby", "Game");

    }

    public void Update()
    {

        if (!this.NetworkObject.IsSpawned && NetworkManager.Singleton.IsServer)
        {
            try
            {
                this.NetworkObject.Spawn();
            }
            catch
            {

            }
        }


        if (textInput.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {

                SendMessageServerRpc(textInput.text, PlayerPrefs.GetString("PLAYER_NAME"));
                textInput.text = "";
            }
        }
    }




    [ClientRpc]
    private void ReceiveChatMessageClientRpc(string message, string senderId)
    {
    
        GameObject msgObj = Instantiate(chatMessagePrefab, chatPannel.transform);
        if (senderId == "Game")
        {
            //Eventually use diff color
            msgObj.GetComponent<TMP_Text>().text = $"> {message}";
        }
        else
        {
            msgObj.GetComponent<TMP_Text>().text = $"{senderId} : {message}";
        }

        chatMessages.Add(msgObj);

        if (chatPannel.transform.childCount > maxHistory)
        {
            Destroy(chatMessages[0]);
            chatMessages.RemoveAt(0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRpc(string message, string senderId)
    {
        ReceiveChatMessageClientRpc(message, senderId);

    }

}






