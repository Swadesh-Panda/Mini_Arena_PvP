using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField JoinRoomInputField;
    // [SerializeField] TMP_Text errorText;
    // [SerializeField] Transform PlayerListContent;
    // [SerializeField] GameObject PlayerListItemPrefab;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        Debug.Log("connecting to master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("connected to master");
        PhotonNetwork.JoinLobby();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("title");
        Debug.Log("Joined Lobby");
        PhotonNetwork.NickName = "Player " + Random.Range(0,1000).ToString("0000");
    }

//-----Create And Join Rooms----------//
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("loading");
    }
    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(JoinRoomInputField.text))
        {
            return;
        }
        PhotonNetwork.JoinRoom(JoinRoomInputField.text);
        MenuManager.Instance.OpenMenu("loading");
    }
//-----Create And Join Rooms-----------//

    public override void OnJoinedRoom()
    {
        Debug.Log("joined Room");
        
        PhotonNetwork.LoadLevel(1);

        // Player[] players = PhotonNetwork.PlayerList;

        // for (int i = 0; i < players.Length; i++)
        // {
        //     Instantiate(PlayerListItemPrefab,PlayerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);

        // }
    }

    // public override void OnCreateRoomFailed(short returnCode, string message)
    // {
    //     errorText.text = "Room Creation Failed: "+ message;
    //     MenuManager.Instance.OpenMenu("error");
    // }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }
    
    // public override void OnPlayerEnteredRoom(Player newPlayer)
    // {
    //     Instantiate(PlayerListItemPrefab,PlayerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    // }

    public void QuitGame()
    {
        Application.Quit();
    }
}
