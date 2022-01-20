using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int subsampling = 10;

    void Start()
    {
        TerrainData terrainData = GetComponent<Terrain>().terrainData;

        int size = terrainData.heightmapResolution;
        float [,] heights = new float[size,size];

        for (int i=0; i < size; i+=subsampling)
        {
            for (int j=0; j < size; j+=subsampling)
            {
                heights[j,i] = Random.Range(0f,1f);
            }
        }

        // cutting out border effects
        int cut = subsampling*((int)Mathf.Floor(size/subsampling));

        // along X with i
        for (int i=0; i < cut; i+=subsampling)
        {
            for (int j=0; j <= cut; j+=subsampling)
            {
                for (int k = 0; k < subsampling; k++)
                {
                    heights[j,i+k] = Mathf.Lerp(heights[j,i], heights[j,i+subsampling], k/(float)subsampling);
                }
            }
        }
        // along Y with j
        for (int i=0; i <= cut; i++)
        {
            for (int j=0; j < cut; j+=subsampling)
            {
                for (int k = 0; k < subsampling; k++)
                {
                    heights[j+k,i] = Mathf.Lerp(heights[j,i], heights[j+subsampling,i], k/(float)subsampling);
                }
            }
        }

        terrainData.SetHeights(0,0, heights);
    }

    void Update()
    {
        
    }
}
