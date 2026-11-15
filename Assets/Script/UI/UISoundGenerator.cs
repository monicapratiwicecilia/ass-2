using UnityEngine;

public class UISoundGenerator : MonoBehaviour
{
    public static AudioClip CreateClickClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.05f); // 50ms click
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(1200f, 400f, (float)i / sampleCount);
            float envelope = 1f - ((float)i / sampleCount);
            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.35f;
        }

        AudioClip clip = AudioClip.Create("UIClick", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public static AudioClip CreateAmbientHumClip()
    {
        int sampleRate = 44100;
        int durationSec = 4;
        int sampleCount = sampleRate * durationSec;
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            // Deep forensic sci-fi drone (55Hz base + 110Hz harmonic)
            float wave1 = Mathf.Sin(2 * Mathf.PI * 55f * t);
            float wave2 = Mathf.Sin(2 * Mathf.PI * 110f * t) * 0.4f;
            float wave3 = Mathf.Sin(2 * Mathf.PI * 165f * t) * 0.15f;
            samples[i] = (wave1 + wave2 + wave3) * 0.12f;
        }

        AudioClip clip = AudioClip.Create("AmbientHum", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
