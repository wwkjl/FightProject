using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentRoomCanvas : MonoBehaviour
{
    [SerializeField]
    private PlayerListingMenu _playerListingMenu;
    [SerializeField]
    private LeaveRoomMenu _leaveRoomMenu;

    public LeaveRoomMenu LeaveRoomMenu { get { return _leaveRoomMenu; } }

    public Text RoomName;

    private RoomsCanvases _roomCanvases;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
        _playerListingMenu.FirstInitialize(canvases);
        _leaveRoomMenu.FirstInitialize(canvases);
    }

    public void Show()
    {
        Doozy.Engine.GameEventMessage.SendEvent("ToRoom");
        if (PhotonNetwork.InRoom) RoomName.text = PhotonNetwork.CurrentRoom.Name;
        _playerListingMenu.ButtonToggle();
    }

    public void Hide()
    {
        Doozy.Engine.GameEventMessage.SendEvent("ToRobby");
        RoomName.text = "";
    }
}
