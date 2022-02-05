using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SettingsShape : ScriptableObject
{
    [Range(1,500)]
    public float size = 10;
    public NoiseSettings NoiseSettings;
}
