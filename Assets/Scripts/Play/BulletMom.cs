using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMom : MonoBehaviour
{
    public SimpleObjectMover player;

    public Queue<Bullet> bulletPool = new Queue<Bullet>();

    [SerializeField]
    private Bullet bulletPrefab;

    public void BulletAllocate(int num)
    {
        for (int i = 0; i < num; i++)
        {
            Bullet alloc = Instantiate(bulletPrefab, transform);
            alloc.Create(this, i);
            bulletPool.Enqueue(alloc);
        }
    }

    public void BulletDeque()
    {
        if (bulletPool.Count != 0)
        {
            Bullet bul = bulletPool.Dequeue();
            bul.transform.position = player.BulletPoint.transform.position;
            bul.transform.rotation = player.BulletPoint.transform.rotation;
            bul.gameObject.SetActive(true);
        }
    }

    public void BulletEnque(Bullet bul)
    {
        bul.gameObject.SetActive(false);
        if (player != null)
        {
            bul.transform.position = player.BulletPoint.transform.position;
        }
        bulletPool.Enqueue(bul);

    }
}
