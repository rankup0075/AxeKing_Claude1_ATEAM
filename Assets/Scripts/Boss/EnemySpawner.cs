using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public static IEnumerator Summon(BossConfig.Phase3 cfg, Transform origin, LayerMask groundMask)
    {
        int count = Random.Range(cfg.countRange.x, cfg.countRange.y + 1);
        for (int i = 0; i < count; i++)
        {
            var offset = new Vector3(Random.Range(-cfg.spawnAreaHalfExtents.x, cfg.spawnAreaHalfExtents.x), 0,
                                     Random.Range(-cfg.spawnAreaHalfExtents.y, cfg.spawnAreaHalfExtents.y));
            var pos = origin.position + offset;
            var marker = Object.Instantiate(cfg.summonMarkerPrefab, pos, Quaternion.identity);
            Object.Destroy(marker, cfg.markerTime);
        }
        yield return new WaitForSeconds(cfg.markerTime);

        for (int i = 0; i < count; i++)
        {
            var pos = origin.position + new Vector3(Random.Range(-cfg.spawnAreaHalfExtents.x, cfg.spawnAreaHalfExtents.x), 0,
                                                    Random.Range(-cfg.spawnAreaHalfExtents.y, cfg.spawnAreaHalfExtents.y));
            var enemyPrefab = cfg.enemyPrefabs[Random.Range(0, cfg.enemyPrefabs.Length)];
            Object.Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
    }
}
