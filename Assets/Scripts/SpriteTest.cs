using UnityEngine;
using System.Collections;

public class SpriteTest : MonoBehaviour
{
    public Material tileMaterial;
    public Vector2 uvOffset;
    public Vector3 pos;
    Rect[] srcRects, dstRects;
    Vector3[] verts,normals;
    Vector2[] uvs;
    int[] tris;
    MeshFilter meshFilter;

	// Use this for initialization
	void Start () 
    {
        verts = new Vector3[4];
        normals = new Vector3[4];
        uvs = new Vector2[4];
        tris = new int[6];
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        GetComponent<MeshRenderer>().material = tileMaterial;

        verts[0] = new Vector3(0, 0, 0) + pos;
        verts[1] = new Vector3(64, 0, 0) + pos;
        verts[2] = new Vector3(0, 64, 0) + pos;
        verts[3] = new Vector3(64, 64, 0) + pos;
        meshFilter.mesh.vertices = verts;

        //	Lower left triangle.
        tris[0] = 0;
        tris[1] = 2;
        tris[2] = 1;

        //	Upper right triangle.	
        tris[3] = 2;
        tris[4] = 3;
        tris[5] = 1;

        meshFilter.mesh.triangles = tris;

        normals[0] = -Vector3.forward;
        normals[1] = -Vector3.forward;
        normals[2] = -Vector3.forward;
        normals[3] = -Vector3.forward;

        meshFilter.mesh.normals = normals;

        uvs[0] = new Vector2(0.0f, 0.0f) + uvOffset;
        uvs[1] = new Vector2(0.5f, 0.0f) + uvOffset;
        uvs[2] = new Vector2(0.0f, 0.5f) + uvOffset;
        uvs[3] = new Vector2(0.5f, 0.5f) + uvOffset;

        meshFilter.mesh.uv = uvs;
	}
}
