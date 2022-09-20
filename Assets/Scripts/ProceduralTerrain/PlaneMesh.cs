using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneMesh
{
	protected ShapeGenerator shapeGenerator;
    protected int verticesDensity;
    protected Mesh mesh;
	//protected float size;
    //protected Vector3 axisN, axisT, axisB;
    

    public PlaneMesh(ShapeGenerator shapeGenerator, int verticesDensity, Mesh mesh)
    {
		this.shapeGenerator = shapeGenerator;
		//this.size = size;
        this.verticesDensity = Mathf.Clamp(verticesDensity, 2, verticesDensity);
        this.mesh = mesh;

        /*axisN = Vector3.up;
        axisT = Vector3.right;
        axisB = Vector3.forward;*/
    }

    public void GenerateMesh()
    {
        Vector3[] vertices = new Vector3[verticesDensity*verticesDensity];
        int[] triangles = new int[6*(verticesDensity-1)*(verticesDensity-1)];

        /*for (var i = 0; i < verticesDensity; i++)
        {
            for (var j = 0; j < verticesDensity; j++)
            {
                Vector2 p = new Vector2(i,j) / (verticesDensity-1);
                vertices[i] = new Vector3();
            }
        }*/
        //float step = size/(float)(verticesDensity-1);
		float step = 1f/(float)(verticesDensity-1);
		int v = 0;
		for (int i = 0; i < verticesDensity; i++)
		{
			float tmp_x = -0.5f + step*i;
			float tmp_z = -0.5f;
			for (int j = 0; j < verticesDensity; j++)
			{
				vertices[v] = shapeGenerator.FromUnitQuadToShape(
					new Vector3(tmp_x,0,tmp_z)
					);
				tmp_z += step;
				v++;
			}
		}
        //triangles
        int numOfVerticesToScan = (2*verticesDensity-1)*verticesDensity; //TODO: check how many iterations are done (probably too many)
		int f = 0;
		for (v=0; v < numOfVerticesToScan; v++)
		{
			int i = v/verticesDensity;
			int j = v%verticesDensity;

			if (j != verticesDensity-1)
			{
				if (i < verticesDensity-1) //front side
				{
					triangles[f++] = v;
					triangles[f++] = (v+verticesDensity+1);
					triangles[f++] = (v+verticesDensity);
					
					triangles[f++] = v;
					triangles[f++] = (v+1);
					triangles[f++] = (v+verticesDensity+1);
				}
			}
		}
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
