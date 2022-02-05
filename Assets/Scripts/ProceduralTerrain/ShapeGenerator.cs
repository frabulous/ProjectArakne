using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator
{
    SettingsShape settings;
    NoiseTuner noiseTuner;

    public ShapeGenerator(SettingsShape settings)
    {
        this.settings = settings;
        noiseTuner = new NoiseTuner(settings.NoiseSettings);
    }

    public Vector3 FromUnitQuadToShape(Vector3 pointOnUnitQuad)
    {
        float height = noiseTuner.Evaluate(pointOnUnitQuad);
        return (pointOnUnitQuad)*settings.size + Vector3.up*height;
    }
}
