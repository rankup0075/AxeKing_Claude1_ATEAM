using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    int damage;
    float life;
    LayerMask targetMask;

    public void Init(int dmg, float lifeSec, LayerMask mask)
    {
        damage = dmg; life = lifeSec; targetMask = mask;
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetMask) == 0) return;
        other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }
}
