using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerListingMenu : MonoBehaviourPunCallbacks
{
    private int redNum = 0;
    private int blueNum = 0;
    private List<PlayerListing> _listings = new List<PlayerListing>();
    private RoomsCanvases _roomsCanvases;
    private Stack<int> stackPlayerIndex = new Stack<int>(); //마스터 전용
    private bool _ready = false;
    private CreateRoomMenu.GameMode gameMode;
    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();
    private ExitGames.Client.Photon.Hashtable _yourCustomProperties = new ExitGames.Client.Photon.Hashtable(); //마스터 전용
    private ExitGames.Client.Photon.Hashtable _roomCustomProperties = new ExitGames.Client.Photon.Hashtable(); //마스터 전용

    [SerializeField]
    private Transform _content;
    [SerializeField]
    private Transform _contentRed;
    [SerializeField]
    private Transform _contentBlue;
    [SerializeField]
    private PlayerListing _playerListing;
    [SerializeField]
    private PlayerListing _playerListingTeam;
    [SerializeField]
    private Text _readyUpText;
    [SerializeField]
    private PlayerListing choosedPlayer = null;
    [SerializeField]
    private UIPopupSystem uIPopupSystem;

    public GameObject _startButton;
    public GameObject _readyButton;
    public GameObject _kickButton;

    public GameObject NormScroll;
    public GameObject TeamScroll;

    public override void OnEnable()
    {
        base.OnEnable();
        SetReadyUp(false);
        GetCurrentRoomPlayers();
    }

    public void SetReadyUp(bool state)
    {
        _myCustomProperties["isReady"] = state;
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
        _ready = state;
        _readyUpText.text = _ready ? "Ready!" : "Ready?";
    }

    public void InitReady()
    {
        SetReadyUp(false);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        _ready = false;
        choosedPlayer = null;
        for (int i = 0; i < _listings.Count; i++)
            Destroy(_listings[i].gameObject);

        _listings.Clear();
    }

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
        StackInit();
    }

    public void StackInit()
    {
        stackPlayerIndex.Clear();

        for (int i = 8; i > 1; i--)
        {
            stackPlayerIndex.Push(i);
        }
    }

    public void ButtonToggle()
    {
        _startButton.SetActive(PhotonNetwork.IsMasterClient);
        _kickButton.SetActive(PhotonNetwork.IsMasterClient);
        _readyButton.SetActive(!PhotonNetwork.IsMasterClient);

        bool flag = (CreateRoomMenu.GameMode)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"] == CreateRoomMenu.GameMode.TeamFight;

        TeamScroll.SetActive(flag);
        NormScroll.SetActive(!flag);
    }

    public void PlayerChoose(PlayerListing player)
    {
        if (choosedPlayer != null) choosedPlayer.outline.effectColor = Color.white;

        choosedPlayer = player;
        player.outline.effectColor = Color.red;
    }

    private void GetCurrentRoomPlayers()
    {
        if (!PhotonNetwork.IsConnected) return;
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null) return;

        gameMode = (CreateRoomMenu.GameMode)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"];

        if (PhotonNetwork.IsMasterClient)
        {
            _roomCustomProperties["PlayerIndexStack"] = stackPlayerIndex.Pop();
            PhotonNetwork.CurrentRoom.SetCustomProperties(_roomCustomProperties);
        }

        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            if (playerInfo.Value == PhotonNetwork.LocalPlayer) continue;
            AddPlayerListing(playerInfo.Value);
        }
        AddPlayerListing(PhotonNetwork.LocalPlayer);
    }

    private void AddPlayerListing(Player player)
    {
        int index = _listings.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            _listings[index].SetPlayerInfo(player);
        }
        else
        {
            PlayerListing listing;
            int team;

            if (gameMode == CreateRoomMenu.GameMode.TeamFight)
            {
                if (player.CustomProperties.ContainsKey("Team") && player.CustomProperties["Team"] != null)
                {
                    team = (int)player.CustomProperties["Team"];
                }
                else
                {
                    team = blueNum < redNum ? 2 : 1;

                }

                _yourCustomProperties["Team"] = team;
                player.SetCustomProperties(_yourCustomProperties);
                _yourCustomProperties.Clear();

                if(team == 1)
                {

                    listing = (PlayerListing)Instantiate(_playerListingTeam, _contentRed);
                    redNum++;
                }
                else
                {

                    listing = (PlayerListing)Instantiate(_playerListingTeam, _contentBlue);
                    blueNum++;
                }

            }
            else
            {
                listing = (PlayerListing)Instantiate(_playerListing, _content);
            }

            if (listing != null)
            {
                listing.playerListingMenu = this;
                listing.SetPlayerInfo(player);
                _listings.Add(listing);
            }
        }

    }

    public void TeamMoving(PlayerListing list, int team)
    {
        int index = _listings.FindIndex(x => x.Player == list.Player);
        if (index != -1)
        {
            if(team == 2)
            {
                _listings[index].transform.SetParent(_contentBlue);
            }
            else if (team == 1)
            {
                _listings[index].transform.SetParent(_contentRed);
            }
        }
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        _roomsCanvases.CurrentRoomCanvas.LeaveRoomMenu.OnClick_LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayerListing(newPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            _roomCustomProperties["PlayerIndexStack"] = stackPlayerIndex.Pop();
            PhotonNetwork.CurrentRoom.SetCustomProperties(_roomCustomProperties);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.NetworkClientState == ClientState.Leaving)
        {
            return;
        }

        if (PhotonNetwork.IsMasterClient && !otherPlayer.IsMasterClient)
        {
            /*if ((int)PhotonNetwork.CurrentRoom.CustomProperties["PlayerIndexStack"] > (int)otherPlayer.CustomProperties["PlayerIndex"])
            {
                stackPlayerIndex.Push((int)PhotonNetwork.CurrentRoom.CustomProperties["PlayerIndexStack"]);
                _roomCustomProperties["PlayerIndexStack"] = otherPlayer.CustomProperties["PlayerIndex"];
                PhotonNetwork.CurrentRoom.SetCustomProperties(_roomCustomProperties);
            }
            else
            {
                stackPlayerIndex.Push((int)otherPlayer.CustomProperties["PlayerIndex"]);
            }
            //플레이어 나가도 인덱스를 계속 남기려면 쓰는 방법
             */

            foreach (PlayerListing playerListing in _listings)
            {
                if ((int)playerListing.Player.CustomProperties["PlayerIndex"] > (int)otherPlayer.CustomProperties["PlayerIndex"])
                {
                    _yourCustomProperties["PlayerIndex"] = (int)playerListing.Player.CustomProperties["PlayerIndex"] - 1;
                    playerListing.Player.SetCustomProperties(_yourCustomProperties);
                }
            }

            int idx = (int)PhotonNetwork.CurrentRoom.CustomProperties["PlayerIndexStack"];
            stackPlayerIndex.Push(idx);
            _roomCustomProperties["PlayerIndexStack"] = idx - 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(_roomCustomProperties);

        }

        if (choosedPlayer != null)
            if (choosedPlayer.Player == otherPlayer) choosedPlayer = null;

        int index = _listings.FindIndex(x => x.Player == otherPlayer);
        if (index != -1)
        {
            if (_listings[index].Team == 1) redNum--;
            else if (_listings[index].Team == 2) blueNum--;
            Destroy(_listings[index].gameObject);
            _listings.RemoveAt(index);
        }

        _yourCustomProperties.Clear();
    }


    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        _myCustomProperties["Team"] = null;
        _myCustomProperties["isReady"] = false;
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
        redNum = 0;
        blueNum = 0;
        StackInit();
    }

    public void OnClick_StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (_listings.Count < 2)
            {
                uIPopupSystem.PopupShow("플레이어 수가\n너무 적습니다!");
                Debug.Log("플레이어 수가 너무 적습니다!");
                return;
            }
            for (int i = 0; i < _listings.Count; i++)
            {
                if (_listings[i].Player != PhotonNetwork.LocalPlayer)
                {
                    if (!_listings[i].Ready)
                    {
                        uIPopupSystem.PopupShow("준비되지 않은\n플레이어가 있습니다!");
                        Debug.Log("Players Not Ready!");
                        return;
                    }
                }
            }

            if(gameMode == CreateRoomMenu.GameMode.TeamFight)
            {
                if(redNum != blueNum)
                {
                    uIPopupSystem.PopupShow("두 팀의 인원수가\n동일하지 않습니다!");
                    Debug.Log("Team Unbalanced!");
                    return;
                }
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            photonView.RPC("RPC_DoozyToGameUI", RpcTarget.All);
            PhotonNetwork.LoadLevel(1);
            //Doozy.Engine.GameEventMessage.SendEvent("ToGame");
        }
    }

    public void OnClick_ReadyUp()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            SetReadyUp(!_ready);
            photonView.RPC("RPC_ChangeReadyState", RpcTarget.All, PhotonNetwork.LocalPlayer, _ready);
            int index = _listings.FindIndex(x => x.Player == PhotonNetwork.LocalPlayer);
            if (index != -1) _listings[index].SetPlayerReady(_ready);
            //base.photonView.RpcSecure("RPC_ChangeReadyState", RpcTarget.MasterClient,true, PhotonNetwork.LocalPlayer, _ready);
        }
    }

    public void OnClick_Kick()
    {
        if (!PhotonNetwork.IsMasterClient || choosedPlayer == null)
        {
            return;
        }
        if (choosedPlayer.Player != null && !choosedPlayer.Player.IsMasterClient)
        {
            PhotonNetwork.CloseConnection(choosedPlayer.Player);
        }
    }


    public void OnClick_GoBlue()
    {
        if (PhotonNetwork.IsMasterClient && choosedPlayer != null)
        {
            if (choosedPlayer.Player.CustomProperties.ContainsKey("PlayerIndex"))
            {
                int num = (int)choosedPlayer.Player.CustomProperties["PlayerIndex"];
                photonView.RPC("RPC_GoBlue", RpcTarget.All, num);
            }
            else
            {
                Debug.LogError("플레이어 인덱싱이 잘못되었습니다!");
            }
        }
        else
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerIndex"))
            {
                int num = (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerIndex"];
                photonView.RPC("RPC_GoBlue", RpcTarget.All, num);
            }
            else
            {
                Debug.LogError("플레이어 인덱싱이 잘못되었습니다!");
            }
        }
    }

    public void OnClick_GoRed()
    {
        if (PhotonNetwork.IsMasterClient && choosedPlayer != null)
        {
            if (choosedPlayer.Player.CustomProperties.ContainsKey("PlayerIndex"))
            {
                int num = (int)choosedPlayer.Player.CustomProperties["PlayerIndex"];
                photonView.RPC("RPC_GoRed", RpcTarget.All, num);
            }
            else
            {
                Debug.LogError("플레이어 인덱싱이 잘못되었습니다!");
            }
        }
        else
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerIndex"))
            {
                int num = (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerIndex"];
                photonView.RPC("RPC_GoRed", RpcTarget.All, num);
            }
            else
            {
                Debug.LogError("플레이어 인덱싱이 잘못되었습니다!");
            }
        }


    }

    [PunRPC]
    private void RPC_DoozyToGameUI()
    {
        Doozy.Engine.GameEventMessage.SendEvent("ToGame");
    }


    [PunRPC]
    private void RPC_ChangeReadyState(Player player, bool ready)
    {
        int index = _listings.FindIndex(x => x.Player == player);
        if (index != -1) _listings[index].SetPlayerReady(ready);

    }

    [PunRPC]
    private void RPC_GoBlue(int num)
    {
        int index = _listings.FindIndex(x => (int)x.Player.CustomProperties["PlayerIndex"] == num);
        if (index != -1)
        {
            if (_listings[index].Team == 2) return;

            _yourCustomProperties["Team"] = 2;
            _listings[index].Player.SetCustomProperties(_yourCustomProperties);
            _yourCustomProperties.Clear();
            _listings[index].transform.SetParent(_contentBlue);
            redNum--; blueNum++;
        }
    }


    [PunRPC]
    private void RPC_GoRed(int num)
    {
        int index = _listings.FindIndex(x => (int)x.Player.CustomProperties["PlayerIndex"] == num);
        if (index != -1)
        {
            if (_listings[index].Team == 1) return;

            _yourCustomProperties["Team"] = 1;
            _listings[index].Player.SetCustomProperties(_yourCustomProperties);
            _yourCustomProperties.Clear();
            _listings[index].transform.SetParent(_contentRed);
            redNum++; blueNum--;
        }
    }
}
