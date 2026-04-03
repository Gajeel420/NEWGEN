using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks performance metrics: frame rate, memory usage, garbage collection, and input latency.
/// Provides real-time diagnostics and historical data for optimization.
/// </summary>
public class PerformanceProfiler : MonoBehaviour
{
    public static PerformanceProfiler Instance { get; private set; }

    [System.Serializable]
    public class FrameMetrics
    {
        public float deltaTime;
        public float frameTime; // milliseconds
        public int fps;
        public float memoryUsage; // MB
        public float heapSize; // MB
        public float temporaryMemory; // MB
        public int garbageCollectionCount;
    }

    [SerializeField] private int historySize = 300; // ~5 seconds at 60fps
    [SerializeField] private bool trackMemory = true;
    [SerializeField] private bool trackGC = true;
    [SerializeField] private float profileInterval = 0.1f; // Sample every 100ms

    private Queue<FrameMetrics> frameHistory;
    private float frameSampleTimer = 0f;
    private int previousGCCount = 0;
    private float maxFrameTime = 0f;
    private float minFrameTime = float.MaxValue;

    // Events for external tracking
    public event System.Action<FrameMetrics> OnFrameSampled;
    public event System.Action<float> OnFrameTimeSpike; // Triggers if frame > 16.7ms at 60fps

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        frameSampleTimer += Time.deltaTime;

        if (frameSampleTimer >= profileInterval)
        {
            RecordFrameMetrics();
            frameSampleTimer = 0f;
        }
    }

    private void RecordFrameMetrics()
    {
        if (frameHistory == null)
            frameHistory = new Queue<FrameMetrics>(historySize);

        FrameMetrics metrics = new FrameMetrics
        {
            deltaTime = Time.deltaTime,
            frameTime = Time.deltaTime * 1000f,
            fps = Mathf.RoundToInt(1f / Time.deltaTime),
            memoryUsage = trackMemory ? System.GC.GetTotalMemory(false) / (1024f * 1024f) : 0f,
            heapSize = trackMemory ? UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / (1024f * 1024f) : 0f,
            temporaryMemory = trackMemory ? UnityEngine.Profiling.Profiler.GetMonoReservedSizeLong() / (1024f * 1024f) : 0f,
            garbageCollectionCount = trackGC ? System.GC.CollectionCount(0) : 0
        };

        frameHistory.Enqueue(metrics);
        if (frameHistory.Count > historySize)
            frameHistory.Dequeue();

        // Track frame time spikes (exceeding 16.7ms = 60fps frame budget)
        if (metrics.frameTime > 16.7f)
        {
            OnFrameTimeSpike?.Invoke(metrics.frameTime);
        }

        // Update min/max
        maxFrameTime = Mathf.Max(maxFrameTime, metrics.frameTime);
        minFrameTime = Mathf.Min(minFrameTime, metrics.frameTime);

        OnFrameSampled?.Invoke(metrics);
    }

    /// <summary>
    /// Get average FPS over history window
    /// </summary>
    public float GetAverageFPS()
    {
        if (frameHistory == null || frameHistory.Count == 0) return 0f;

        float totalFPS = 0f;
        foreach (var metric in frameHistory)
            totalFPS += metric.fps;

        return totalFPS / frameHistory.Count;
    }

    /// <summary>
    /// Get average frame time in milliseconds
    /// </summary>
    public float GetAverageFrameTime()
    {
        if (frameHistory == null || frameHistory.Count == 0) return 0f;

        float totalTime = 0f;
        foreach (var metric in frameHistory)
            totalTime += metric.frameTime;

        return totalTime / frameHistory.Count;
    }

    /// <summary>
    /// Get current memory usage in MB
    /// </summary>
    public float GetCurrentMemoryMB()
    {
        return System.GC.GetTotalMemory(false) / (1024f * 1024f);
    }

    /// <summary>
    /// Get frame time statistics
    /// </summary>
    public (float avg, float min, float max) GetFrameTimeStats()
    {
        return (GetAverageFrameTime(), minFrameTime, maxFrameTime);
    }

    /// <summary>
    /// Get percentage of frames exceeding target frame time
    /// </summary>
    public float GetFrameTimeSpikesPercentage(float targetFrameTime = 16.7f)
    {
        if (frameHistory == null || frameHistory.Count == 0) return 0f;

        int spikeCount = 0;
        foreach (var metric in frameHistory)
        {
            if (metric.frameTime > targetFrameTime)
                spikeCount++;
        }

        return (spikeCount / (float)frameHistory.Count) * 100f;
    }

    /// <summary>
    /// Reset statistics
    /// </summary>
    public void Reset()
    {
        frameHistory.Clear();
        maxFrameTime = 0f;
        minFrameTime = float.MaxValue;
        previousGCCount = System.GC.CollectionCount(0);
    }

    /// <summary>
    /// Get full history for analysis
    /// </summary>
    public Queue<FrameMetrics> GetFrameHistory()
    {
        return frameHistory ?? new Queue<FrameMetrics>();
    }
}
