using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    BulletMom mom;
    int index;
    float power = 1200f;
    Rigidbody rgd;
    Vector3 dir;
    Coroutine coroutine;

    private void Awake()
    {
        rgd = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        dir = -transform.right;
        rgd.AddForce(dir * power);
        coroutine = StartCoroutine(LifeCoroutine());
    }

    public void Create(BulletMom mom, int num)
    {
        this.mom = mom;
        this.index = num;
        gameObject.SetActive(false);
    }

    IEnumerator LifeCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        rgd.velocity = Vector3.zero;
        mom.BulletEnque(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Player":
                SimpleObjectMover player = other.gameObject.GetComponent<SimpleObjectMover>();
                if (player == null || player == mom.player)
                {
                    break;
                }
                if (player.TeamDistinguish(mom.player))
                {
                    player.Ouch(mom.player.index);
                }
                goto case "Bullet";
            case "Wall":
            case "Bullet":
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
                rgd.velocity = Vector3.zero;
                mom.BulletEnque(this);
                break;
            default:
                break;
        }
    }
}
