using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class PlayerListing : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text _text;
    [SerializeField]
    private GameObject _readySymbol;
    [SerializeField]
    private GameObject _masterSymbol;

    public Outline outline;
    public Player Player { get; private set; }
    public bool Ready = false;
    public int Team = 1;
    public PlayerListingMenu playerListingMenu;


    public void SetPlayerInfo(Player player)
    {
        Player = player;

        _masterSymbol.SetActive(player.IsMasterClient);

        if (player.CustomProperties.ContainsKey("isReady"))
            SetPlayerReady((bool)player.CustomProperties["isReady"]);

        if (player.CustomProperties.ContainsKey("Team") && player.CustomProperties["Team"] != null)
        {
            Team = (int)player.CustomProperties["Team"];
            playerListingMenu.TeamMoving(this, Team);
        }

        SetPlayerText(player);
    }

    public void SetPlayerReady(bool ready)
    {
        Ready = ready;
        _readySymbol.SetActive(Ready);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if(targetPlayer != null && targetPlayer == Player)
        {
            if(changedProps.ContainsKey("PlayerIndex"))
                SetPlayerText(targetPlayer);

            if (changedProps.ContainsKey("Team") && changedProps["Team"] != null)
            {
                Team = (int)changedProps["Team"];
                playerListingMenu.TeamMoving(this, Team);
            }
        }
    }

    private void SetPlayerText(Player player)
    {
        int result = -1;
        if (player.CustomProperties.ContainsKey("PlayerIndex"))
            result = (int)player.CustomProperties["PlayerIndex"];

        if(_text != null)
        {
            _text.text = result.ToString() + ": " + player.NickName;
        }
    }

    public void OnClick_Button()
    {
        playerListingMenu.PlayerChoose(this);
    }

    public override void OnDisable()
    {
        if(Player == PhotonNetwork.LocalPlayer)
        {
            playerListingMenu.SetReadyUp(false);
        }
    }
}
