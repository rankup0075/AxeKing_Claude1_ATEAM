// CylindricalExplosion.cs
using UnityEngine;
using System.Collections;

public class CylindricalExplosion : MonoBehaviour
{
    [Header("Visuals")]
    public Transform preview;      // ���� ǥ�ÿ� ����Ʈ
    public GameObject explosion;   // ���� ���� (CapsuleCollider + DamageOnTrigger)
    public float autoDestroyAfter = 1f;

    void Awake()
    {
        // explosion�� �⺻ ���� ���·� ����
        if (explosion) explosion.SetActive(false);
        if (preview) preview.gameObject.SetActive(false);
    }

    public void ConfigureShape()
    {
        if (!preview || !explosion) return;

        // preview�� ���� �������� �״�� ���
        Vector3 scale = preview.localScale;

        // Collider�� preview ũ�⿡ �°� �ڵ� ����
        var cap = explosion.GetComponent<CapsuleCollider>();
        if (cap)
        {
            cap.isTrigger = true;
            cap.direction = 1; // Y��
            cap.radius = scale.x * 0.5f; // X������ ���� �ݰ�
            cap.height = scale.y;        // Y������ ���� ����
        }
    }

    public IEnumerator ShowThenExplode(float previewDuration)
    {
        if (preview) preview.gameObject.SetActive(true);
        yield return new WaitForSeconds(previewDuration);
        if (preview) preview.gameObject.SetActive(false);

        if (explosion) explosion.SetActive(true);
        Destroy(gameObject, autoDestroyAfter);
    }
}
