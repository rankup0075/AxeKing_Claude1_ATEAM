using UnityEngine;

public class BossAOEPreview : MonoBehaviour
{
    public Transform cylinderVisual;
    public Vector3 LastCenter { get; private set; }

    public void Setup(float radius, float height)
    {
        if (cylinderVisual != null)
            cylinderVisual.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
    }

    public void SetPreviewCenter(Vector3 worldPos)
    {
        LastCenter = worldPos;
        transform.position = worldPos;
    }

    public void Hide()
    {
        if (cylinderVisual) cylinderVisual.gameObject.SetActive(false);
    }
}
