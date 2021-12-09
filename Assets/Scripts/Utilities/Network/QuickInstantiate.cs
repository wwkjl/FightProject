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
                            Debug.LogError("�÷��̾��� ���� �������� �ʾҽ��ϴ�.");
                            return null;
                        }
                    }
                    else
                    {
                        Debug.LogError("�÷��̾��� ���� �������� �ʾҽ��ϴ�.");
                        return null;
                    }
                default:
                    Debug.LogError("���Ӹ�尡 ���������� �������� �ʾҽ��ϴ�.");
                    return SpawnLocationBR[idx].transform;
            }
        }
        else
        {
            Debug.LogError("Room���� ���Ӹ�� ������ �������� �ʽ��ϴ�.");
            return SpawnLocationBR[idx].transform;
        }
        //�� ������Ƽ ���� ��忡 ����  ����

    }
}
