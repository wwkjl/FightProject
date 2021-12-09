using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class QuickInstantiate : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    public GameManager gameManager;
    public GameObject[] SpawnLocationBR = new GameObject[8];
    public GameObject[] SpawnLocationTF_Red = new GameObject[8];
    public GameObject[] SpawnLocationTF_Blue = new GameObject[8];

    private void Awake()
    {
        int index = -1;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerIndex"))
            index = (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerIndex"] - 1;

        MasterManager.NetworkInstantiate(_prefab, OutLocate(index).position, OutLocate(index).rotation);
    }

    Transform OutLocate(int idx)
    {
        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameMode"))
        {
            gameManager.gameMode = (CreateRoomMenu.GameMode)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"];
            switch (gameManager.gameMode)
            {
                case CreateRoomMenu.GameMode.BatteRoyale:
                case CreateRoomMenu.GameMode.Raid:
                    return SpawnLocationBR[idx].transform;
                case CreateRoomMenu.GameMode.TeamFight:
                    if(PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
                    {
                        int team = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
                        if (team == 1) return SpawnLocationTF_Red[idx].transform;
                        else if (team == 2) return SpawnLocationTF_Blue[idx].transform;
                        else
                        {
                            Debug.LogError("플레이어의 팀이 설정되지 않았습니다.");
                            return null;
                        }
                    }
                    else
                    {
                        Debug.LogError("플레이어의 팀이 설정되지 않았습니다.");
                        return null;
                    }
                default:
                    Debug.LogError("게임모드가 정상적으로 설정되지 않았습니다.");
                    return SpawnLocationBR[idx].transform;
            }
        }
        else
        {
            Debug.LogError("Room에서 게임모드 설정이 존재하지 않습니다.");
            return SpawnLocationBR[idx].transform;
        }
        //룸 프로퍼티 게임 모드에 따라  변경

    }
}
