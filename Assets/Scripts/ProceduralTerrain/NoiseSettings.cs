using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public float strength = 1;
    public float baseRoughness = 1, roughness = 2;
    public Vector3 center;

    [Range(1,8)] public int numHarmonics = 1;
    public float decayRate = 0.5f;
    public float minValue;
}
