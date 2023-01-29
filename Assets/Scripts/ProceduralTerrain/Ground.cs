using System;
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

    
    private void OnEnable()
    {
        //print("enabled!");
        float backupStrength = shape_sets.NoiseSettings.strength;
        shape_sets.NoiseSettings.strength = 0f;
        OnShapeUpdated();
        StartCoroutine(RaiseStrength(backupStrength, 1.9f));
    }
    public void OnDisable() 
    {
        //print("disabled!");
        float backupStrength = shape_sets.NoiseSettings.strength;
        StopAllCoroutines();
        //shape_sets.NoiseSettings.strength = 0f;
        //OnShapeUpdated();
        //StartCoroutine(RaiseStrength(0f, 1.99f));
        //StartCoroutine(WaitAndSetStrength(backupStrength, 2.0f));
    }

    IEnumerator RaiseStrength(float targetStrength, float animSeconds)
    {   
        float startStrength = shape_sets.NoiseSettings.strength; // = 0f;
        float t = 0f;
        while (t <= animSeconds)
        {
            t += Time.deltaTime;
            float percent = Mathf.Clamp01(t / animSeconds);
            
            shape_sets.NoiseSettings.strength = 
                Mathf.Lerp(startStrength, targetStrength, percent);

            OnShapeUpdated();

            yield return null;
        }
        shape_sets.NoiseSettings.strength = targetStrength;
    }
    IEnumerator WaitAndSetStrength(float targetStrength, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        shape_sets.NoiseSettings.strength = targetStrength;
        this.gameObject.SetActive(false);
    }

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
