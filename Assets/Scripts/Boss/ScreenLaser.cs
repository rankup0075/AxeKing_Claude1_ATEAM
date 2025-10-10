// ScreenLaser.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class ScreenLaser : MonoBehaviour
{
    public enum Axis { Horizontal, Vertical }

    public Axis axis = Axis.Horizontal;
    public float thickness = 1.0f;      // ������ ����
    public float previewTime = 1.0f;    // ������ ǥ�� �ð�
    public float fireTime = 0.6f;       // ���� ���� �ð�
    public bool continuous = false;     // true=����ƽ, false=�ѹ���

    public Transform preview;           // ������ ���־�
    public Transform beam;              // ���� �߻� ���־�
    public int damage = 20;
    public float tickInterval = 0.2f;

    BoxCollider box;
    float lastTick = -999f;
    bool firing;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        preview.gameObject.SetActive(false);
        beam.gameObject.SetActive(false);
    }

    public void SetupToCameraBounds(float worldWidth = 25f)
    {
        // worldWidth: ȭ���� ���������� ��ü �� (���� ����)
        // 2.5D ���� 20~30 ������ ��κ� ī�޶� �� �̻�

        if (axis == Axis.Horizontal)
        {
            // �߾� ����
            Vector3 pos = transform.position;
            pos.x = 0f;
            transform.position = pos;

            box.size = new Vector3(worldWidth, thickness, 1f);
            box.center = Vector3.zero;

            if (preview) preview.localScale = new Vector3(worldWidth, thickness , 1f );
            if (beam) beam.localScale = new Vector3(worldWidth, thickness, 1f );
        }
        else
        {
            box.size = new Vector3(thickness, worldWidth, 1f);
            box.center = Vector3.zero;

            if (preview) preview.localScale = new Vector3(thickness, worldWidth ,1f );
            if (beam) beam.localScale = new Vector3(thickness, worldWidth, 1f );
        }
    }


    public IEnumerator FireSequence()
    {
        if (preview) preview.gameObject.SetActive(false);
        if (beam) beam.gameObject.SetActive(true);
        firing = true;
        yield return new WaitForSeconds(fireTime);
        firing = false;
        if (beam) beam.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!firing || continuous) return;
        if (other.TryGetComponent<PlayerHealth>(out var ph))
            ph.TakeDamage(damage);
    }

    void OnTriggerStay(Collider other)
    {
        if (!firing || !continuous) return;
        if (!other.TryGetComponent<PlayerHealth>(out var ph)) return;

        if (Time.time - lastTick >= tickInterval)
        {
            ph.TakeDamage(damage);
            lastTick = Time.time;
        }
    }
}
