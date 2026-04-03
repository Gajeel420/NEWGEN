using UnityEngine;

/// <summary>
/// Manages particle effects for hits, blocks, and special moves.
/// Handles spawning and cleanup of visual feedback particles.
/// </summary>
public class ParticleEffectManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitImpactPrefab;
    [SerializeField] private ParticleSystem blockPrefab;
    [SerializeField] private ParticleSystem specialMovePrefab;
    [SerializeField] private ParticleSystem knockdownPrefab;

    [SerializeField] private ParticleSystem screenShakeParticles;
    [SerializeField] private float screenShakeIntensity = 0.5f;

    // Pooling for performance
    private ObjectPool<ParticleSystem> hitEffectPool;
    private ObjectPool<ParticleSystem> blockEffectPool;

    private void Awake()
    {
        // Initialize object pools
        if (hitImpactPrefab != null)
            hitEffectPool = new ObjectPool<ParticleSystem>(hitImpactPrefab, 5);

        if (blockPrefab != null)
            blockEffectPool = new ObjectPool<ParticleSystem>(blockPrefab, 3);
    }

    /// <summary>
    /// Spawn hit impact particle effect
    /// </summary>
    public void SpawnHitEffect(Vector3 position, Vector3 normal)
    {
        if (hitImpactPrefab == null)
            return;

        // Get particle from pool or instantiate
        ParticleSystem particle = hitEffectPool?.Get() ?? Instantiate(hitImpactPrefab);
        particle.transform.position = position;

        // Align particles with hit normal
        particle.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        particle.Play();

        // Return to pool after particles finish
        StartCoroutine(ReturnToPoolAfterDelay(particle, hitEffectPool, hitImpactPrefab.main.duration + 1f));
    }

    /// <summary>
    /// Spawn block effect
    /// </summary>
    public void SpawnBlockEffect(Vector3 position)
    {
        if (blockPrefab == null)
            return;

        ParticleSystem particle = blockEffectPool?.Get() ?? Instantiate(blockPrefab);
        particle.transform.position = position;
        particle.Play();

        StartCoroutine(ReturnToPoolAfterDelay(particle, blockEffectPool, blockPrefab.main.duration + 1f));
    }

    /// <summary>
    /// Spawn special move effect
    /// </summary>
    public void SpawnSpecialMoveEffect(Vector3 position, int effectType = 0)
    {
        ParticleSystem particle = null;

        switch (effectType)
        {
            case 0: // Default special effect
                if (specialMovePrefab != null)
                    particle = Instantiate(specialMovePrefab, position, Quaternion.identity);
                break;
            case 1: // Knockdown effect
                if (knockdownPrefab != null)
                    particle = Instantiate(knockdownPrefab, position, Quaternion.identity);
                break;
        }

        if (particle != null)
        {
            particle.Play();
            Destroy(particle.gameObject, particle.main.duration + 1f);
        }
    }

    /// <summary>
    /// Screen shake effect for powerful hits
    /// </summary>
    public void ScreenShake(float duration, float intensity = 1f)
    {
        // TODO: Integrate with main camera shake
        StartCoroutine(PerformScreenShake(duration, intensity * screenShakeIntensity));
    }

    /// <summary>
    /// Perform screen shake
    /// </summary>
    private System.Collections.IEnumerator PerformScreenShake(float duration, float intensity)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            yield break;

        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float randomX = Random.Range(-1f, 1f) * intensity;
            float randomY = Random.Range(-1f, 1f) * intensity;

            mainCamera.transform.position = originalPos + new Vector3(randomX, randomY, 0);
            yield return null;
        }

        mainCamera.transform.position = originalPos;
    }

    /// <summary>
    /// Spawn at world position persistently
    /// </summary>
    public void SpawnWorldParticle(string resourcePath, Vector3 position, float lifetime = 2f)
    {
        GameObject particlePrefab = Resources.Load<GameObject>(resourcePath);
        if (particlePrefab == null)
            return;

        GameObject instance = Instantiate(particlePrefab, position, Quaternion.identity);
        Destroy(instance, lifetime);
    }

    /// <summary>
    /// Return particle to pool after delay
    /// </summary>
    private System.Collections.IEnumerator ReturnToPoolAfterDelay(ParticleSystem particle, ObjectPool<ParticleSystem> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pool != null && particle != null)
        {
            pool.Return(particle);
        }
        else if (particle != null)
        {
            Destroy(particle.gameObject);
        }
    }
}

/// <summary>
/// Simple object pooling system for performance
/// </summary>
public class ObjectPool<T> where T : MonoBehaviour
{
    private T prefab;
    private Queue<T> available;
    private int size;

    public ObjectPool(T prefab, int initialSize = 5)
    {
        this.prefab = prefab;
        this.size = initialSize;
        this.available = new Queue<T>(initialSize);

        for (int i = 0; i < initialSize; i++)
        {
            T instance = Object.Instantiate(prefab);
            instance.gameObject.SetActive(false);
            available.Enqueue(instance);
        }
    }

    public T Get()
    {
        if (available.Count > 0)
        {
            T instance = available.Dequeue();
            instance.gameObject.SetActive(true);
            return instance;
        }
        else
        {
            T instance = Object.Instantiate(prefab);
            instance.gameObject.SetActive(true);
            return instance;
        }
    }

    public void Return(T instance)
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(false);
            available.Enqueue(instance);
        }
    }

    public void Clear()
    {
        while (available.Count > 0)
        {
            T instance = available.Dequeue();
            Object.Destroy(instance.gameObject);
        }
    }
}
