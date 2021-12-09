using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    private bool isDead = false;
    private bool isFinished = false;
    private bool isEveryReady = false;
    private int? watchingTeam = 0;
    private int watchingIndex = -1;
    private int remainPlayers = 0;
    private int remainReds = 0;
    private int remainBlues = 0;

    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private KeyCode[] keyCodes =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
    };

    [SerializeField]
    private GameObject _readyText;
    [SerializeField]
    private GameObject _waitingText;
    [SerializeField]
    private GameObject _deadText;
    [SerializeField]
    private Text _winText;
    [SerializeField]
    private Text _additionalText;
    [SerializeField]
    private Text _allivedText;
    [SerializeField]
    private GameObject _quitButton;

    public CreateRoomMenu.GameMode gameMode = CreateRoomMenu.GameMode.BatteRoyale;
    public CameraWork cameraWork;
    public SimpleObjectMover[] localPlayers = new SimpleObjectMover[8];
    public bool[] alliveChecker = new bool[8];

    private void Start()
    {
        for(int i = 0; i < 8; i++)
        {
            alliveChecker[i] = false;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerIndex"))
        {
            watchingIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerIndex"] - 1;
            alliveChecker[watchingIndex] = true;
        }
    }

    private void Update()
    {
        for(int i = 0; i<9; i++)
        {
            if(Input.GetKeyDown(keyCodes[i]))
            {
                if (isDead && !isFinished && localPlayers[i] != null)
                {
                    watchingIndex = i;
                    _additionalText.text = "Player " + (watchingIndex + 1);

                    watchingTeam = (int?)localPlayers[watchingIndex].photonView.Owner.CustomProperties["Team"];
                    if (watchingTeam == 1) _additionalText.color = Color.red;
                    else if (watchingTeam == 2) _additionalText.color = Color.blue;

                    cameraWork.target = localPlayers[watchingIndex].gameObject;
                }
            }
        }
    }

    public void OnClick_QuitGame()
    {
        if (PhotonNetwork.NetworkClientState != ClientState.Leaving)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int idx = (int)otherPlayer.CustomProperties["PlayerIndex"];
        idx--;
        if (alliveChecker[idx])
        {
            alliveChecker[idx] = false;
            remainPlayers--;

            SimpleObjectMover player = localPlayers[idx];
            int? team = (int?)player.photonView.Owner.CustomProperties["Team"];
            if (team == 1) remainReds--;
            else if (team == 2) remainBlues--;

            _allivedText.text = AllivedText();

            if (FinishCheck_BattleRoyale()) return;
            if (FinishCheck_TeamFight()) return;
        }
    }

    public override void OnLeftRoom()
    {
        _myCustomProperties["Team"] = null;
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
        SceneManager.LoadSceneAsync("0. Front");
    }

    public void ImReady(int num)
    {
        if (num == -1) return;
        if (localPlayers[num] == null) return;

        SimpleObjectMover player = localPlayers[num];
        if (!alliveChecker[num])
        {
            int? team = (int?)player.photonView.Owner.CustomProperties["Team"];
            if (team == 1) remainReds++;
            else if (team == 2) remainBlues++;
        }

        alliveChecker[num] = true;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (!alliveChecker[i])
            {
                return;
            }

        }
        if(!isEveryReady)
            EveryReady();
    }

    public void PlayerDead(int num, int index)
    {
        if (localPlayers[num] == null) return;
        alliveChecker[num] = false;
        remainPlayers--;

        SimpleObjectMover player = localPlayers[num];
        int? team = (int?)player.photonView.Owner.CustomProperties["Team"];
        if (team == 1) remainReds--;
        else if (team == 2) remainBlues--;

        _allivedText.text = AllivedText();

        if (FinishCheck_BattleRoyale()) return;
        if (FinishCheck_TeamFight()) return;

        if (player.isMyPlayer())
        {
            _deadText.SetActive(true);
            _additionalText.text = localPlayers[index].nameText.text + " Killed You";
            Invoke("DeadTextOff", 2.0f);
            //cameraWork.target = localPlayers[0].gameObject;
        }
    }
    
    private bool FinishCheck_BattleRoyale()
    {
        if (remainPlayers == 1 && gameMode == CreateRoomMenu.GameMode.BatteRoyale && !isFinished)
        {
            for (int i = 0; i < 8; i++)
            {
                if (alliveChecker[i])
                {
                    cameraWork.target = localPlayers[i].gameObject;
                    _deadText.SetActive(false);
                    _allivedText.text = "";
                    isFinished = true;
                    _winText.text = localPlayers[i].nameText.text + " Win!";
                    _additionalText.text = "";
                    Invoke("OnClick_QuitGame", 4.0f);
                    return true;
                }
            }
        }
        return false;
    }

    private bool FinishCheck_TeamFight()
    {
        if(gameMode == CreateRoomMenu.GameMode.TeamFight && !isFinished)
        {
            int num = -1;

            if (alliveChecker[watchingIndex]) num = watchingIndex;
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    if (alliveChecker[i])
                    {
                        num = i;
                        break;
                    }
                }
            }

            if (num == -1) return false;

            if (remainBlues == 0)
            {
                cameraWork.target = localPlayers[num].gameObject;
                _deadText.SetActive(false);
                _allivedText.text = "";
                isFinished = true;
                _winText.text = "<color=red>Red Win!</color>";
                _additionalText.text = "";
                Invoke("OnClick_QuitGame", 4.0f);
                return true;

            }
            else if (remainReds == 0)
            {
                cameraWork.target = localPlayers[num].gameObject;
                _deadText.SetActive(false);
                _allivedText.text = "";
                isFinished = true;
                _winText.text = "<color=blue>Blue Win!</color>";
                _additionalText.text = "";
                Invoke("OnClick_QuitGame", 4.0f);
                return true;
            }
        }

        return false;
    }

    private void EveryReady()
    {
        isEveryReady = true;
        cameraWork.target = localPlayers[watchingIndex].gameObject;

        remainPlayers = PhotonNetwork.PlayerList.Length;
        _waitingText.SetActive(false);
        _readyText.SetActive(true);
        Invoke("ReadyTextOff", 2.0f);
    }

    private void DeadTextOff()
    {
        if(!isFinished)
        {
            _deadText.SetActive(false);
            _additionalText.text = "Player " + (watchingIndex + 1);
            watchingTeam = (int?)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            if (watchingTeam == 1) _additionalText.color = Color.red;
            else if (watchingTeam == 2) _additionalText.color = Color.blue;
            isDead = true;
        }
    }

    private void ReadyTextOff()
    {
        _readyText.SetActive(false);
        _quitButton.SetActive(true);
        _allivedText.text = AllivedText();
        foreach (SimpleObjectMover player in localPlayers)
        {
            if (player == null) continue;
            player.StartPlay();
        }
    }

    private string AllivedText()
    {
        string k = "";

        switch (gameMode)
        {
            case CreateRoomMenu.GameMode.BatteRoyale:
                k = "<color=yellow>" + remainPlayers + "</color> Players Left";
                break;
            case CreateRoomMenu.GameMode.TeamFight:
                k += "<color=red>" + remainReds + "</color> : ";
                k += "<color=blue>" + remainBlues + "</color>";
                break;
            case CreateRoomMenu.GameMode.Raid:
                k = "<color=yellow>" + remainPlayers + "</color> Players Left";
                break;
        }

        return k;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogError("Disconnected : " + cause);
    }
}
