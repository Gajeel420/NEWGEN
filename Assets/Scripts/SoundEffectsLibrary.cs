using UnityEngine;

/// <summary>
/// Centralized library of all sound effects used in the game.
/// Created as a ScriptableObject for easy configuration and reference.
/// </summary>
[CreateAssetMenu(fileName = "SoundEffectsLibrary", menuName = "Fighting Game/Sound Effects Library")]
public class SoundEffectsLibrary : ScriptableObject
{
    [System.Serializable]
    public struct AudioEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volumeScale;
        [Range(0.5f, 1.5f)] public float pitch;
    }

    [SerializeField] public AudioEntry[] attackSounds;
    [SerializeField] public AudioEntry[] hitSounds;
    [SerializeField] public AudioEntry[] blockSounds;
    [SerializeField] public AudioEntry[] impactSounds;
    [SerializeField] public AudioEntry[] uiSounds;
    [SerializeField] public AudioEntry[] specialMoveSounds;
    [SerializeField] public AudioEntry[] knockdownSounds;

    /// <summary>
    /// Get attack sound by type
    /// </summary>
    public AudioClip GetAttackSound(string attackType)
    {
        return GetSoundFromArray(attackSounds, attackType.ToLower());
    }

    /// <summary>
    /// Get hit sound
    /// </summary>
    public AudioClip GetHitSound(int zoneType = 0)
    {
        if (hitSounds.Length == 0)
            return null;

        return hitSounds[zoneType % hitSounds.Length].clip;
    }

    /// <summary>
    /// Get block sound
    /// </summary>
    public AudioClip GetBlockSound()
    {
        if (blockSounds.Length == 0)
            return null;

        return blockSounds[Random.Range(0, blockSounds.Length)].clip;
    }

    /// <summary>
    /// Get impact/clash sound
    /// </summary>
    public AudioClip GetImpactSound()
    {
        if (impactSounds.Length == 0)
            return null;

        return impactSounds[Random.Range(0, impactSounds.Length)].clip;
    }

    /// <summary>
    /// Get UI button sound
    /// </summary>
    public AudioClip GetUISound(string uiAction = "click")
    {
        return GetSoundFromArray(uiSounds, uiAction.ToLower());
    }

    /// <summary>
    /// Get special move sound
    /// </summary>
    public AudioClip GetSpecialMoveSound(string moveName)
    {
        return GetSoundFromArray(specialMoveSounds, moveName.ToLower());
    }

    /// <summary>
    /// Get knockdown sound
    /// </summary>
    public AudioClip GetKnockdownSound()
    {
        if (knockdownSounds.Length == 0)
            return null;

        return knockdownSounds[Random.Range(0, knockdownSounds.Length)].clip;
    }

    /// <summary>
    /// Helper to find sound by name in array
    /// </summary>
    private AudioClip GetSoundFromArray(AudioEntry[] array, string soundName)
    {
        if (array == null || array.Length == 0)
            return null;

        foreach (var entry in array)
        {
            if (entry.name.ToLower() == soundName)
                return entry.clip;
        }

        // Return random if name not found
        return array[Random.Range(0, array.Length)].clip;
    }

    /// <summary>
    /// Get audio entry with volume and pitch
    /// </summary>
    public AudioEntry GetAudioEntry(AudioEntry[] array, string name)
    {
        if (array == null || array.Length == 0)
            return new AudioEntry { volumeScale = 1f, pitch = 1f };

        foreach (var entry in array)
        {
            if (entry.name.ToLower() == name.ToLower())
                return entry;
        }

        return array[Random.Range(0, array.Length)];
    }
}
