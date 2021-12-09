using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Doozy.Engine.Soundy;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{
    public enum GameMode
    {
        BatteRoyale = 0,
        TeamFight,
        Raid,
    }

    private RoomsCanvases _roomCanvases;
    private ExitGames.Client.Photon.Hashtable _roomCustomProperties = new ExitGames.Client.Photon.Hashtable();
    private GameMode gameMode = GameMode.BatteRoyale;

    [SerializeField]
    private Text _roomName;
    [SerializeField]
    private Text _maxPlayerNum;
    [SerializeField]
    private Slider _playerNumSlider;
    [SerializeField]
    private UIPopupSystem uIPopupSystem;


    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
    }

    public void OnSlider_PlayerNumChanged(float value)
    {
        _maxPlayerNum.text = "Max Player : " + ((int)value).ToString();
        SoundyManager.Play("Example Clicks", "Bubble Pop");

    }

    public void OnToggle_BattleRoyale(bool check)
    {
        if(check)
        {
            gameMode = GameMode.BatteRoyale;
        }
    }

    public void OnToggle_TeamFight(bool check)
    {
        if (check)
        {
            gameMode = GameMode.TeamFight;
        }
    }

    public void OnToggle_Raid(bool check)
    {
        if (check)
        {
            gameMode = GameMode.Raid;
        }
    }

    public void OnClick_CreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        if (PhotonNetwork.NetworkClientState == ClientState.Joining)
            return;
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectingToGameServer)
            return;

        _roomCustomProperties["PlayerIndexStack"] = 1;
        _roomCustomProperties["GameMode"] = gameMode;

        RoomOptions options = new RoomOptions();
        options.BroadcastPropsChangeToAll = true;
        options.PublishUserId = true;
        options.MaxPlayers = (byte)_playerNumSlider.value;
        options.CustomRoomProperties = _roomCustomProperties;
        if (_roomName.text == "")
        {
            PhotonNetwork.CreateRoom(PhotonNetwork.LocalPlayer.NickName, options, TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.CreateRoom(_roomName.text, options, TypedLobby.Default);
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room Succesfully", this);
        _roomCanvases.CurrentRoomCanvas.Show();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room Creation Failed: " + message, this);
        if(returnCode == 32766)
        {
            //방 이름 중복 팝업
            uIPopupSystem.PopupShow("중복된 방 이름입니다!");
        }
    }
}
