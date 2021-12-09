using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestConnect : MonoBehaviourPunCallbacks
{
    private static bool isFirst = true;
    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    public GameObject loadingText;
    public GameObject gameStart;
    public GameObject nameInput;
    public Text nameInputText;
    public Text myName;
    public MasterManager MyMaster;

    void Start()
    {
        PlayInit();
    }

    public void ServerConnect()
    {
        if (PhotonNetwork.NetworkClientState != ClientState.ConnectingToMasterServer)
        {
            loadingText.SetActive(true);
            Debug.Log("Connecting to Server...", this);
            AuthenticationValues authValues = new AuthenticationValues(MyMaster._gameSettings.NickName);
            //미리 Set해줘야지 써드 파티 각각 유저가 고유 ID를 얻을수 있음
            PhotonNetwork.AuthValues = authValues;
            PhotonNetwork.SendRate = 40;    //20.
            PhotonNetwork.SerializationRate = 20;    //10.
            PhotonNetwork.AutomaticallySyncScene = true; //씬 싱크
            PhotonNetwork.NickName = nameInputText.text == "" ? MyMaster._gameSettings.NickName : nameInputText.text;
            PhotonNetwork.GameVersion = MyMaster._gameSettings.GameVersion;
            _myCustomProperties["RandomName"] = MyMaster._gameSettings.NickName;
            PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
            nameInput.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        isFirst = false;
        Debug.Log("Connected to Photon.", this);
        Debug.Log("My nickname is " + PhotonNetwork.LocalPlayer.NickName, this);

        Doozy.Engine.GameEventMessage.SendEvent("ToRobby");

        myName.text = PhotonNetwork.NickName;

        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Failed to connect to Photon: " + cause.ToString(), this);
    }

    public override void OnJoinedLobby()
    {
        print("Joined Lobby.");
        //PhotonNetwork.FindFriends(new string[] { "1" });
        loadingText.SetActive(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }

    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        base.OnFriendListUpdate(friendList);

        foreach(FriendInfo info in friendList)
        {
            //Debug.Log("Friend info received " + info.UserId + " is online? " + info.IsOnline);
        }
    }

    private void PlayInit()
    {
        loadingText.SetActive(!isFirst);
        gameStart.SetActive(isFirst);
        nameInput.SetActive(isFirst);
    }
}
