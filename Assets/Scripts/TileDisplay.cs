using UnityEngine;
using System.Collections;

public class TileDisplay : MonoBehaviour 
{
    public Material tileMaterial;

    Rect[] spriteLookup;
    Rect[] srcRects, dstRects;
    Vector3[] verts, normals;
    Vector2[] uvs;
    int[] tris;
    MeshFilter meshFilter;

	// Use this for initialization
	void Start () 
    {
        GenerateLookups();
        srcRects = new Rect[70];
        dstRects = new Rect[70];
        //srcRects = new Rect[35];
        //dstRects = new Rect[35];
        GenerateCoords();
        RegenerateMesh();
	
	}
    void GenerateLookups()
    {
        /*spriteLookup = new Rect[64];
        for (int i = 0; i < 64; i++)
        {
            int x = i % 8, y = i / 8;
            spriteLookup[i] = new Rect(x/8.0f, (7-y)/8.0f, 1/8.0f, 1/8.0f);
        }*/
        spriteLookup = new Rect[4];
        for (int i = 0; i < 4; i++)
        {
            int x = i % 2, y = i / 2;
            //spriteLookup[i] = new Rect((x)/2.0f, (y)/2.0f, 0.5f,0.5f);
            float sz = 128.0f;
            float TOL = 0.0001f;
            spriteLookup[i] = new Rect((x * 64) / sz, (y * 64) / sz, 64 / sz, 64 / sz);
            spriteLookup[i] = new Rect((x * 64 + TOL) / sz, (y * 64 + TOL) / sz, (64 - 2 * TOL) / sz, (64 - 2 * TOL) / sz);
        }
    }
    void GenerateCoords()
    {
        int len = srcRects.Length;
        for (int i = 0; i < len; i++)
        {
            int x=i%10,y=i/10;
            //dstRects[i] = new Rect(x * 100+10, y * 100+10, 64, 64);
            dstRects[i] = new Rect(x * 64, y * 64, 64, 64);
            // src if randomish:
            /*if (y%2==0)
                srcRects[i] = spriteLookup[11];
            else
                srcRects[i] = spriteLookup[0];*/
            srcRects[i] = spriteLookup[Random.Range(0,4)];
        }
    }
    void RegenerateMesh()
    {
        if (meshFilter==null)
            meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.mesh == null)
            meshFilter.mesh = new Mesh();
        MeshRenderer render = GetComponent<MeshRenderer>();
        render.material = tileMaterial;
        // now the real work:
        int len=srcRects.Length;
        if (verts==null)
            verts = new Vector3[4*len];
        if (normals==null)
            normals = new Vector3[4*len];
        if (uvs==null)
            uvs = new Vector2[4*len];
        if (tris==null)
            tris = new int[6*len];

        for (int i = 0; i < len; i++)
        {
            Rect dst = dstRects[i];
            verts[i * 4 + 0] = new Vector3(dst.xMin - 320, 240 - dst.yMax, 0);
            verts[i * 4 + 1] = new Vector3(dst.xMax - 320, 240 - dst.yMax, 0);
            verts[i * 4 + 2] = new Vector3(dst.xMin - 320, 240 - dst.yMin, 0);
            verts[i * 4 + 3] = new Vector3(dst.xMax - 320, 240 - dst.yMin, 0);

            //	Lower left triangle.
            tris[i * 6 + 0] = i * 4 + 0;
            tris[i * 6 + 1] = i * 4 + 2;
            tris[i * 6 + 2] = i * 4 + 1;

            //	Upper right triangle.	
            tris[i * 6 + 3] = i * 4 + 2;
            tris[i * 6 + 4] = i * 4 + 3;
            tris[i * 6 + 5] = i * 4 + 1;


            normals[i * 4 + 0] = -Vector3.forward;
            normals[i * 4 + 1] = -Vector3.forward;
            normals[i * 4 + 2] = -Vector3.forward;
            normals[i * 4 + 3] = -Vector3.forward;

            Rect src = srcRects[i];
            uvs[i * 4 + 0] = new Vector2(src.xMin, src.yMin);
            uvs[i * 4 + 1] = new Vector2(src.xMax, src.yMin);
            uvs[i * 4 + 2] = new Vector2(src.xMin, src.yMax);
            uvs[i * 4 + 3] = new Vector2(src.xMax, src.yMax);
        }

        meshFilter.mesh.vertices = verts;
        meshFilter.mesh.normals = normals;
        meshFilter.mesh.uv = uvs;
        meshFilter.mesh.triangles = tris;

    }

}
