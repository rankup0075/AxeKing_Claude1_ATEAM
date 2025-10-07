using UnityEngine;
using System.Collections;

public class PreviewSpawner : MonoBehaviour
{
    public static IEnumerator SpawnAfter(GameObject preview, GameObject actual, Vector3 pos, Quaternion rot, float delay, System.Action<GameObject> onActual = null)
    {
        GameObject p = null;
        if (preview) p = Instantiate(preview, pos, rot);
        yield return new WaitForSeconds(delay);
        if (p) Destroy(p);
        var a = Instantiate(actual, pos, rot);
        onActual?.Invoke(a);
    }
}
