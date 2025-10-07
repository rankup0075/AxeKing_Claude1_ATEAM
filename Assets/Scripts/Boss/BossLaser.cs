using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BossLaser : MonoBehaviour
{
    public bool isPreview = true;
    public float worldWidth = 120f;
    public float duration = 1f;
    public GameObject vfxRoot;

    BoxCollider col;
    int damage;
    LayerMask playerLayer;
    float thickness;
    bool fired;

    public float LastY { get; private set; }

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    public void Setup(bool isPreview, float thickness, int damage, LayerMask playerLayer)
    {
        this.isPreview = isPreview;
        this.thickness = thickness;
        this.damage = damage;
        this.playerLayer = playerLayer;
        ApplyCollider();
    }

    void ApplyCollider()
    {
        col.size = new Vector3(worldWidth, thickness, 2f);
        col.center = Vector3.zero;
        col.enabled = !isPreview;
        if (vfxRoot) vfxRoot.transform.localScale = new Vector3(worldWidth, thickness, 1f);
    }

    public void AlignHorizontalAtY(float y)
    {
        LastY = y;
        var p = transform.position;
        p.y = y;
        transform.position = p;
        if (vfxRoot) vfxRoot.transform.position = transform.position;
    }

    public void Hide()
    {
        if (vfxRoot) vfxRoot.SetActive(false);
    }

    public void FireOnceThenDie(float aliveSeconds)
    {
        if (fired) return;
        fired = true;
        isPreview = false;
        col.enabled = true;
        if (vfxRoot) vfxRoot.SetActive(true);
        StartCoroutine(DieSoon(aliveSeconds));
    }

    IEnumerator DieSoon(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isPreview) return;
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            col.enabled = false;
        }
    }
}
