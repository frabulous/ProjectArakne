using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Ground : MonoBehaviour
{
    public bool autoUpdate = true;
    [HideInInspector] public bool shapeSettingsFoldout, colorSettingsFoldout;
    public SettingsShape shape_sets;
    public SettingsColor color_sets;

    ShapeGenerator shapeGenerator;

    [SerializeField][Range(2,256)]
    private int resolution = 16;

    //[SerializeField][Range(1,500)]
    //private float planeSize = 10;

    private PlaneMesh plane;
    [SerializeField] private MeshFilter meshFilter;

    private MeshCollider meshCollider;

    void Start()
    {
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;
    }
    void Update()
    {
        
    }

/*  private void OnValidate() // make edits from inspector
    {
        GenerateGround();
    }
*/

    public void GenerateGround()
    {
        Setup();
        UpdateShape();
        UpdateColors();
    }

    private void Setup()
    {
        shapeGenerator = new ShapeGenerator(shape_sets);
        if(!meshFilter){
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
        }
        plane = new PlaneMesh(shapeGenerator, resolution, meshFilter.sharedMesh);
        plane.GenerateMesh();
    }

    private void UpdateShape()
    {
        plane.GenerateMesh();
        if(meshCollider) meshCollider.sharedMesh = meshFilter.mesh;
    }
    private void UpdateColors()
    {
        meshFilter.GetComponent<MeshRenderer>().sharedMaterial.color = color_sets.mainColor;
    }

    public void OnShapeUpdated()
    {
        if (autoUpdate)
        {
            Setup();
            UpdateShape();
        }
    }
    public void OnColorUpdated()
    {
        if (autoUpdate)
        {
            Setup();
            UpdateColors();
        }
    }
}
