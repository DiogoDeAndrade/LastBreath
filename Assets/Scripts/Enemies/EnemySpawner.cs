using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private EnemyProbList   enemyPrefabs;
    [SerializeField] 
    private BoxCollider2D   spawnArea;
    [SerializeField, ShowIf(nameof(needSpawnRadius))] 
    private float           spawnRadius = 100.0f;
    [SerializeField]
    private Vector2Int      initialSpawnCount = Vector2Int.zero;
    [SerializeField]
    private Vector2Int      minActiveSpawns = Vector2Int.zero;
    [SerializeField]
    private Vector2         spawnDelay;
    [SerializeField]
    private bool            checkClearance;
    [SerializeField, ShowIf(nameof(checkClearance))]
    private float           clearanceRadius;
    [SerializeField, ShowIf(nameof(checkClearance))]
    private LayerMask       clearanceMask;

    private bool needSpawnRadius => spawnArea == null;

    List<Enemy>     spawnedEnemies = new();
    int             currentSpawnCount;
    int             definedMinActiveSpawns;

    void Start()
    {
        int nSpawns = initialSpawnCount.Random();
        Spawn(nSpawns);

        definedMinActiveSpawns = minActiveSpawns.Random();
    }

    void Update()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy == null) currentSpawnCount--;
        }
        spawnedEnemies.RemoveAll((enemy) => enemy == null);

        if (currentSpawnCount < definedMinActiveSpawns)
        {
            int nSpawns = definedMinActiveSpawns - currentSpawnCount;

            StartCoroutine(SpawnCR(nSpawns));
        }
    }

    IEnumerator SpawnCR(int nSpawns)
    {
        currentSpawnCount += nSpawns;

        yield return new WaitForSeconds(spawnDelay.Random());

        Spawn(nSpawns, true);
    }

    int Spawn(int nSpawns, bool alreadyAddedToCount = false)
    {
        int totalSpawns = 0;

        for (int i = 0; i < nSpawns; i++)
        {
            if (GetRandomPoint(out var pos, out var rot))
            {
                var newEnemy = Instantiate(enemyPrefabs.Get(), pos, rot);
                var spriteRenderer = newEnemy.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    var color = spriteRenderer.color;
                    spriteRenderer.color = color.ChangeAlpha(0);
                    spriteRenderer.FadeTo(color, 0.25f);
                }
                spawnedEnemies.Add(newEnemy);
                totalSpawns++;
            }
            else
            {
                if (alreadyAddedToCount) currentSpawnCount--;
            }
        }

        if (!alreadyAddedToCount)
        {
            currentSpawnCount += totalSpawns;
        }

        return totalSpawns;
    }

    private bool GetRandomPoint(out Vector3 pos, out Quaternion rotation)
    {
        rotation = transform.rotation;

        int nTries = 0;
        while (nTries < 20)
        {
            Vector3 candidatePos = Vector3.zero;

            if (spawnArea != null)
            {
                Bounds bounds = spawnArea.bounds;
                candidatePos = bounds.Random();
            }
            else
            {
                candidatePos = transform.position.xy() + UnityEngine.Random.insideUnitCircle * spawnRadius;
            }

            if (checkClearance)
            {
                var collider = Physics2D.OverlapCircle(candidatePos, clearanceRadius, clearanceMask);
                if (collider == null)
                {
                    pos = candidatePos;
                    return true;
                }
            }
            else
            {
                pos = candidatePos;
                return true;
            }

            nTries++;
        }

        pos = Vector3.zero;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnArea == null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
