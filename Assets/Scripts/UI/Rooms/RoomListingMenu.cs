using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomListingMenu : MonoBehaviourPunCallbacks
{
    private List<RoomListing> _listings = new List<RoomListing>();
    private RoomsCanvases _roomsCanvases;
    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    [SerializeField]
    private Transform _content;
    [SerializeField]
    private RoomListing _roomListing;

    public GameObject loadingText;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room.");
        loadingText.SetActive(false);
        _roomsCanvases.CurrentRoomCanvas.Show();
        _content.DestoryChildren();
        _listings.Clear();

        _myCustomProperties["PlayerIndex"] = (int)PhotonNetwork.CurrentRoom.CustomProperties["PlayerIndexStack"];
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
        PhotonNetwork.LocalPlayer.CustomProperties = _myCustomProperties;
        //게임 모드 갱신 필요
        
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        loadingText.SetActive(false);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index != -1)
                {
                    Destroy(_listings[index].gameObject);
                    _listings.RemoveAt(index);
                }
            }
            //Added to room list.
            else
            {
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index == -1)
                {
                    RoomListing listing = (RoomListing)Instantiate(_roomListing, _content);
                    if (listing != null)
                    {
                        listing.roomListingMenu = this;
                        listing.SetRoomInfo(info);
                        _listings.Add(listing);
                    }
                }
                else
                {
                    _listings[index].SetRoomInfo(info);
                    //Modify Listing Here.
                    //_listing[index].dowhatever;
                }
            }
        }
    }

}
