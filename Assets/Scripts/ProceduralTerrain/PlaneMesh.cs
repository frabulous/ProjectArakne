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

		//VERTICES
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
        //TRIANGLES
        int numOfVerticesToScan = (verticesDensity-1)*verticesDensity; //skip right edge
		int t = 0;
		for (v=0; v < numOfVerticesToScan; v++)
		{
			if (v%verticesDensity != verticesDensity-1) //skip top edge
			{
				triangles[t++] = v;
				triangles[t++] = (v+verticesDensity+1);
				triangles[t++] = (v+verticesDensity);
				
				triangles[t++] = v;
				triangles[t++] = (v+1);
				triangles[t++] = (v+verticesDensity+1);
			}
		}
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
