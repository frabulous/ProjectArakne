using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTuner
{
    Noise noise = new Noise();
    //NoiseV2 noise = new NoiseV2();
    NoiseSettings settings;

    public NoiseTuner(NoiseSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noisedOutput = 0f;// noise.Evaluate(settings.center + point*settings.roughness)*0.5f + 0.5f; // translate from [-1,1] to [0,1]
        float frequency = settings.baseRoughness;
        float amplitude = 1f;

        for (var i = 0; i < settings.numHarmonics; i++)
        {
            float n = noise.Evaluate(settings.center + point*frequency);
            noisedOutput = noisedOutput + (n+1)*0.5f * amplitude;
            frequency = frequency * settings.roughness;
            amplitude = amplitude * settings.decayRate;
        }
        noisedOutput = Mathf.Max(0, noisedOutput - settings.minValue); // clamp the output to allow flat low regions
        return noisedOutput * settings.strength;
    }
}
