using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Recreates an echo with delayed AudioSources because AudioMixer DSP effects
/// are not reliable in WebGL builds.
/// </summary>
[DisallowMultipleComponent]
public sealed class WebGLEchoAudio : MonoBehaviour
{
    [SerializeField, Min(0f)] private float delaySeconds = 0.3f;
    [SerializeField, Range(0f, 1f)] private float wetMix = 0.5f;
    [SerializeField, Range(0f, 1f)] private float decayRatio = 0.1f;
    [SerializeField, Range(1, 3)] private int echoCount = 2;
    [SerializeField] private bool forceInEditor;

    private AudioSource primarySource;
    private AudioSource[] echoSources;
    private AudioMixerGroup originalOutputGroup;

    public float TailDuration => delaySeconds * echoCount;

    public bool IsFallbackActive =>
        forceInEditor || Application.platform == RuntimePlatform.WebGLPlayer;

    public void Initialize(AudioSource source)
    {
        primarySource = source;
        if (primarySource == null)
            return;

        originalOutputGroup = primarySource.outputAudioMixerGroup;

        // Avoid processing both the Mixer Echo and the fallback if a browser
        // happens to support the Mixer effect.
        if (IsFallbackActive)
            primarySource.outputAudioMixerGroup = null;

        if (echoSources == null || echoSources.Length != echoCount)
            BuildEchoSources();

        SyncEchoSources();
    }

    public void Play(AudioClip clip)
    {
        if (primarySource == null)
            return;

        Stop();
        primarySource.clip = clip;

        if (clip == null)
            return;

        primarySource.Play();

        if (!IsFallbackActive)
            return;

        for (int i = 0; i < echoSources.Length; i++)
        {
            AudioSource echoSource = echoSources[i];
            echoSource.clip = clip;
            echoSource.volume = GetEchoVolume(i);
            echoSource.PlayDelayed(delaySeconds * (i + 1));
        }
    }

    public void Stop()
    {
        if (primarySource != null)
            primarySource.Stop();

        if (echoSources == null)
            return;

        foreach (AudioSource echoSource in echoSources)
        {
            if (echoSource == null)
                continue;

            echoSource.Stop();
            echoSource.clip = null;
        }
    }

    private void OnValidate()
    {
        delaySeconds = Mathf.Max(0f, delaySeconds);
        wetMix = Mathf.Clamp01(wetMix);
        decayRatio = Mathf.Clamp01(decayRatio);
        echoCount = Mathf.Clamp(echoCount, 1, 3);
    }

    private void BuildEchoSources()
    {
        echoSources = new AudioSource[echoCount];

        for (int i = 0; i < echoSources.Length; i++)
        {
            GameObject echoObject = new GameObject($"WebGL Echo {i + 1}");
            echoObject.transform.SetParent(primarySource.transform, false);
            echoSources[i] = echoObject.AddComponent<AudioSource>();
        }
    }

    private void SyncEchoSources()
    {
        if (primarySource == null || echoSources == null)
            return;

        AudioMixerGroup outputGroup = IsFallbackActive ? null : originalOutputGroup;

        for (int i = 0; i < echoSources.Length; i++)
        {
            AudioSource echoSource = echoSources[i];
            echoSource.playOnAwake = false;
            echoSource.loop = false;
            echoSource.mute = primarySource.mute;
            echoSource.priority = primarySource.priority;
            echoSource.spatialBlend = primarySource.spatialBlend;
            echoSource.panStereo = primarySource.panStereo;
            echoSource.pitch = primarySource.pitch;
            echoSource.spatialize = primarySource.spatialize;
            echoSource.spatializePostEffects = primarySource.spatializePostEffects;
            echoSource.dopplerLevel = primarySource.dopplerLevel;
            echoSource.spread = primarySource.spread;
            echoSource.rolloffMode = primarySource.rolloffMode;
            echoSource.minDistance = primarySource.minDistance;
            echoSource.maxDistance = primarySource.maxDistance;
            echoSource.reverbZoneMix = primarySource.reverbZoneMix;
            echoSource.outputAudioMixerGroup = outputGroup;
            echoSource.bypassEffects = primarySource.bypassEffects;
            echoSource.bypassListenerEffects = primarySource.bypassListenerEffects;
            echoSource.bypassReverbZones = primarySource.bypassReverbZones;
            echoSource.volume = GetEchoVolume(i);
        }
    }

    private float GetEchoVolume(int echoIndex)
    {
        return primarySource.volume * wetMix * Mathf.Pow(decayRatio, echoIndex);
    }
}
