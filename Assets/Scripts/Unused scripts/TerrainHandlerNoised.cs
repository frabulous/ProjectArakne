using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandlerNoised : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triFront;

    public float sizeX, sizeZ;
    public int subdivisionX, subdivisionZ;
    [Range(0,4)] public float maxHeight;
    
    void Start()
    {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

        //mesh = this.GetComponent<MeshFilter>().mesh;

        CreateFlatVertices(subdivisionX+1, subdivisionZ+1);
        CreateTriangles(subdivisionX+1, subdivisionZ+1);
        //UpdateMesh();

        //CreateFlat(verticesWidth, verticesHeight);
		//vertices = new Vector3[flatVertices.Length];

        //RandomizeTerrain();
        UpdateMesh();
    }

    void Update()
    {
        
    }

    private void CreateFlatVertices(int numVerticesX, int numVerticesZ)
	{
		vertices = new Vector3[numVerticesX*numVerticesZ];

		float xStep = sizeX/(float)(numVerticesX-1);
		float zStep = sizeZ/(float)(numVerticesZ-1);

		int v = 0;
		for (int i = 0; i < numVerticesX; i++)
		{
			float tmp_x = xStep*i - sizeX*0.5f;
			float tmp_z = -sizeZ*0.5f;;
			for (int j = 0; j < numVerticesZ; j++)
			{
				vertices[v] = new Vector3(
					tmp_x,
					0, //Mathf.PerlinNoise(tmp_x*.3f, tmp_z*.33f)*maxHeight-(maxHeight*.5f),
					tmp_z
				);
				tmp_z += zStep;
				v++;
			}
		}
	}
    private void CreateTriangles(int numVerticesX, int numVerticesZ)
	{
		int numOfVerticesToScan = (2*numVerticesX-1)*numVerticesZ;
		
		triFront = new int[6*(numVerticesX-1)*(numVerticesZ-1)];

		int f = 0;

		for (int v = 0; v < numOfVerticesToScan; v++)
		{
			int i = v/numVerticesZ;
			int j = v%numVerticesZ;

			if (j != numVerticesZ-1)
			{
				if (i < numVerticesX-1) //front side
				{
					triFront[f++] = v;
					triFront[f++] = (v+numVerticesZ+1);
					triFront[f++] = (v+numVerticesZ);
					
					triFront[f++] = v;
					triFront[f++] = (v+1);
					triFront[f++] = (v+numVerticesZ+1);
				}
			}
		}
	}
	public void UpdateMesh()
    {
		mesh.Clear();
		
		mesh.vertices = vertices;
		
		mesh.subMeshCount = 1;
		mesh.SetTriangles(triFront, 0);
		
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		//makes the collider mesh equal the current mesh
		GetComponent<MeshCollider>().sharedMesh = this.mesh;
    }

    public void RandomizeTerrain()
    {
        int subsampling = 10;
        for (int i=1; i < subdivisionX-2; i+=subsampling)
        {
            for (int j=1; j < subdivisionZ-2; j+=subsampling)
            {
                int v = j + i*subdivisionZ;
                if (v >= vertices.Length)
                {
                    Debug.Log("wrong v: ("+i+","+j+")");
                }
                vertices[v].y = Random.Range(0f,maxHeight);
            }
        }
        /*for (int i=1; i < subdivisionX-1; i+=1)
        {
            for (int j=1; j < subdivisionZ-1; j+=1)
            {
                int v = j + i*subdivisionZ;
                vertices[v].y = Random.Range(0f,maxHeight);;
            }
        }*/

        // cutting out border effects
        //int cutX = subsampling*((int)Mathf.Floor(subdivisionX/subsampling));
        //int cutZ = subsampling*((int)Mathf.Floor(subdivisionZ/subsampling));

        // along X with i
        /*for (int i=2; i < subdivisionX-2; i+=subsampling)
        {
            for (int j=2; j <= subdivisionZ-2; j+=subsampling)
            {
                for (int k = 1; k < subsampling; k++)
                {
                    int v0 = j + i*subdivisionZ;
                    int v1 = j + (i+subsampling)*subdivisionZ;
                    int v = j + (i+k)*subdivisionZ;
                    vertices[v].y = Mathf.Lerp(vertices[v0].y, vertices[v1].y, k/(float)subsampling);
                }
            }
        }
        // along Y with j
        for (int i=2; i <= subdivisionX-2; i+=1)
        {
            for (int j=2; j < subdivisionZ-2; j+=subsampling)
            {
                for (int k = 1; k < subsampling; k++)
                {
                    int v0 = j + i*subdivisionZ;
                    int v1 = j+subsampling + i*subdivisionZ;
                    int v = j+k + i*subdivisionZ;
                    vertices[v].y = Mathf.Lerp(vertices[v0].y, vertices[v1].y, k/(float)subsampling);
                }
            }
        }*/

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position - (.01f)*Vector3.up, new Vector3(sizeX, -0.01f, sizeZ));
    }
}
