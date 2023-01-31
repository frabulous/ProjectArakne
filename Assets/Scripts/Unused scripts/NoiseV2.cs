using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseV2
{
    // The noise values will be generated within this range
    public Vector2 noiseRange = new Vector2(0, 1);

    // The frequency of the noise (smaller values result in more detailed noise)
    public float frequency = 1;

    // The number of octaves to use (more octaves result in more detailed noise)
    public int octaves = 1;

    // The persistence value (affects the amplitude of each octave)
    public float persistence = 0.5f;

    // The lacunarity value (affects the frequency of each octave)
    public float lacunarity = 2;

    // The seed value for the noise generator
    public int seed = 0;

    // The offset of the noise (useful for creating different variations of the noise)
    public Vector2 offset = Vector2.zero;

    public NoiseV2()
    {

    }

    public float Evaluate(Vector3 position)
    {
        // Generate a noise value at the given position
        float noiseValue = Mathf.PerlinNoise(
            (position.x + offset.x) * frequency,
            (position.z + offset.y) * frequency
        );

        // Scale the noise value to the desired range
        noiseValue = Mathf.Lerp(noiseRange.x, noiseRange.y, noiseValue);

        // Apply octaves and persistence to the noise value
        float amplitude = 1;
        float tmp_freq = frequency;
        for (int i = 1; i < octaves; i++)
        {
            amplitude *= persistence;
            tmp_freq *= lacunarity;
            noiseValue += amplitude * Mathf.PerlinNoise(
                (position.x + offset.x) * tmp_freq,
                (position.z + offset.y) * tmp_freq
            );
        }

        return noiseValue;
    }
}
