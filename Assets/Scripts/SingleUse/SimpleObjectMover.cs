using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SimpleObjectMover : MonoBehaviourPun, IPunObservable
{
    public enum Team
    {
        BattleRoyale = 0,
        Red,
        Blue,
        Raid
    }

    private Animator _animator;
    private Rigidbody _rigid;
    private GameObject bulletFactory;
    private Renderer[] _renderers;
    private GameManager gameManager;

    private float x = 0f;
    private float z = 0f;

    private bool canMove = false;
    private bool isHit = true;

    private Ray cameraRay;
    private Plane GroupPlane;
    private float rayLength;

    private int health = 10;

    [SerializeField]
    private float _moveSpeed = 10f;
    [SerializeField]
    private float _rotateSpeed = 10f;
    [SerializeField]
    private GameObject BarrelPoint;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private ParticleSystem explosion;


    public GameObject BulletPoint;
    public BulletMom bulletMom;
    public Team myTeam = Team.BattleRoyale;
    public int index = -1;
    public Text nameText;

    //���� �� ������Ʈ�� ���� �����ε�
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(BarrelPoint.transform.rotation);
        }
        else if (stream.IsReading)
        {
            BarrelPoint.transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }       //����ȭ ��� ��ġ���� ���� �̵� ����ȭ (���� Ʈ������ �� ����)

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody>();
        _renderers = GetComponentsInChildren<Renderer>();
        GroupPlane = new Plane(Vector3.up, Vector3.zero);
        bulletFactory = GameObject.Find("BulletFactory");
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        SetBullet();
        SetTeam();
        GameInitWait();
    }

    void Update()
    {
        if (base.photonView.IsMine && canMove)
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");

            //transform.position += (new Vector3(x, 0f, z)).normalized * _moveSpeed;

            if (x != 0f || z != 0f)
            {
                Vector3 look = x * Vector3.forward + z * Vector3.left;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * _rotateSpeed);
                UpdateMovingBoolean(true);
            }
            else
            {
                UpdateMovingBoolean(false);
            }

            UpdateAiming();

            if (Input.GetMouseButtonDown(0))
            {
                photonView.RPC("RPCBulletDeque", RpcTarget.All);
            }
        }

    }

    private void FixedUpdate()
    {
        if ((x != 0f || z != 0f))
        {
            _rigid.velocity = new Vector3(x, 0f, z).normalized * _moveSpeed;
        }
    }


    private void UpdateMovingBoolean(bool moving)
    {
        _animator.SetBool("Moving", moving);    //�����̴� ���� �ִϸ��̼� ����
    }

    private void UpdateAiming()
    {
        cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (GroupPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointTolook = cameraRay.GetPoint(rayLength);
            BarrelPoint.transform.LookAt(new Vector3(pointTolook.x, transform.position.y, pointTolook.z));
        }
    }

    public void Ouch(int index) 
    {
        if (!isHit && photonView.IsMine)
        {
            photonView.RPC("RPCOuch", RpcTarget.All, health - 1, index);
        }
    }

    public void StartPlay()
    {
        _rigid.drag = 4f;
        canMove = true;
        isHit = false;
    }

    public bool TeamDistinguish(SimpleObjectMover other)
    {
        if (this.myTeam == Team.BattleRoyale && other.myTeam == Team.BattleRoyale)
        {
            return true;
        }
        if (this.myTeam == Team.Red && other.myTeam == Team.Blue)
        {
            return true;
        }
        if (this.myTeam == Team.Blue && other.myTeam == Team.Red)
        {
            return true;
        }
        return false;
    }

    public bool isMyPlayer() => photonView.IsMine;

    private void SetBullet()
    {

        BulletMom mom = Instantiate(bulletMom, bulletFactory.transform);
        mom.player = this;
        mom.BulletAllocate(15);
        bulletMom = mom;

    }

    private void SetTeam()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameMode"))
        {
            switch ((CreateRoomMenu.GameMode)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"])
            {
                case CreateRoomMenu.GameMode.BatteRoyale:
                    myTeam = Team.BattleRoyale;
                    break;
                case CreateRoomMenu.GameMode.Raid:
                    myTeam = Team.Raid;
                    break;
                case CreateRoomMenu.GameMode.TeamFight:
                    if (photonView.Owner.CustomProperties.ContainsKey("Team"))
                    {
                        int team = (int)photonView.Owner.CustomProperties["Team"];
                        if (team == 1) myTeam = Team.Red;
                        else if (team == 2) myTeam = Team.Blue;
                        else
                        {
                            Debug.LogError("�÷��̾��� ���� �������� �ʾҽ��ϴ�.");
                            break;
                        }
                    }
                    else
                    {
                        Debug.LogError("�÷��̾��� ���� �������� �ʾҽ��ϴ�.");
                    }
                    break;
                default:
                    Debug.LogError("���Ӹ�尡 ���������� �������� �ʾҽ��ϴ�.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Room���� ���Ӹ�� ������ �������� �ʽ��ϴ�.");
        }
    }

    private void GameInitWait()
    {
        if (photonView.Owner.CustomProperties.ContainsKey("PlayerIndex"))
        {
            index = (int)photonView.Owner.CustomProperties["PlayerIndex"];
            nameText.text = photonView.Owner.NickName;
            if (myTeam == Team.Red) nameText.color = Color.red;
            else if (myTeam == Team.Blue) nameText.color = Color.blue;
            index--;
            gameManager.localPlayers[index] = this;
            if(photonView.IsMine)
            {
                photonView.RPC("RPCImReady", RpcTarget.All, index);
            }
        }
        else
        {
            Debug.LogError("�÷��̾� �ε����� ���� �ʾҽ��ϴ�!");
        }
    }

    [PunRPC]
    void RPCBulletDeque()
    {
        bulletMom.BulletDeque();
    }

    [PunRPC]
    void RPCOuch(int health, int index)
    {
        if (!isHit)
        {
            this.health = health;
            healthBar.value = health;
            isHit = true;
            foreach (Renderer ren in _renderers)
            {
                ren.material.color = Color.red;
            }
            Invoke("HitTime", 0.5f);
        }
        if (health <= 0)
        {
            ParticleSystem boom = Instantiate(explosion);
            boom.transform.position = this.transform.position;
            canMove = false;
            gameManager.PlayerDead(this.index, index);
            gameObject.SetActive(false);
            //PhotonNetwork.LeaveRoom();
        }
    }

    [PunRPC]
    void RPCImReady(int index)
    {
        gameManager.ImReady(index);
    }

    void HitTime()
    {
        foreach (Renderer ren in _renderers)
        {
            ren.material.color = Color.white;
        }
        isHit = false;
    }
}
