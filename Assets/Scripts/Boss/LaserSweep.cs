using UnityEngine;

public class LaserSweep : MonoBehaviour
{
    public float life = 0.6f;
    public float width = 1f;
    public float damageOnce = 20f;
    public LayerMask targetLayer;

    void Start()
    {
        Destroy(gameObject, life);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer.value) == 0) return;
        var d = other.GetComponentInParent<IDamageable>();
        if (d != null) d.ApplyDamage(damageOnce);
    }

    // 트리거 콜라이더의 스케일을 코드로 맞추고 싶으면 이 스크립트가 붙은 오브젝트의 BoxCollider size.x 또는 size.y 조정
    public void SetSpan(float length, bool horizontal)
    {
        var col = GetComponent<BoxCollider>();
        if (col)
        {
            var s = col.size;
            if (horizontal) { s.x = length; s.y = 1; s.z = width; }
            else { s.x = width; s.y = 1; s.z = length; }
            col.size = s;
        }
    }
}
