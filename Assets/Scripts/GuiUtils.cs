#region Licence
// Copyright Mark Gossage (mark.gossage@sp.edu.sg) 2010-2012.
// Distributed under the Boost Software License, Version 1.0.
//    (See http://www.boost.org/LICENSE_1_0.txt)
#endregion
using UnityEngine;
using System.Collections;

public class GuiUtils : MonoBehaviour {
    public static Rect CentreRect(float w, float h)
    {
        return new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
    }

    public static void CentreText(string text, GUIStyle style)
    {
        GUI.Label(GuiUtils.CentreRect(400, 100), text, style);
    }
    public static void CentreTextShadow(string text, GUIStyle style, Color color, Color shadow)
    {
        Rect r = GuiUtils.CentreRect(400, 100);
        TextShadow(text, r, style, color, shadow);
    }
    public static void TextShadow(string text, Rect r, GUIStyle style, Color color, Color shadow)
    {
        style.normal.textColor = shadow;
        GUI.Label(new Rect(r.xMin + 1, r.yMin + 1, r.width, r.height), text, style);
        style.normal.textColor = color;
        GUI.Label(r, text, style);
    }
    public static void TextShadow(string text, Rect r, GUIStyle style, Color color, Color shadow, Vector2 offset)
    {
        style.normal.textColor = shadow;
        GUI.Label(new Rect(r.xMin + offset.x, r.yMin + offset.y, r.width, r.height), text, style);
        style.normal.textColor = color;
        GUI.Label(r, text, style);
    }
    public static void CentreTextShadowAt(string text, float x, float y, GUIStyle style, Color color, Color shadow, Vector2 offset)
    {
        Rect r = Rect.MinMaxRect(x - 200, y - 50, x + 200, y + 50); // large rect based upon area
        style.alignment = TextAnchor.MiddleCenter;
        TextShadow(text, r, style, color, shadow, offset);
    }


    public static void BeginCentre()
    {
        BeginCentre(new Rect(0, 0, Screen.width, Screen.height));
    }
    public static void BeginCentre(Rect r)
    {
        GUILayout.BeginArea(r);
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
    }
    public static void EndCentre()
    {
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    public static Vector2 GetTextSize(string text)
    {
        return GetTextSize(text, GUI.skin.label);   // best guess
    }

    public static Vector2 GetTextSize(string text, GUIStyle style)
    {
        //Debug.Log(string.Format("Calc size '{0}'", text));
        Vector2 size = Vector2.zero;
        string[] a = text.Split('\n');  // look for \n's
        foreach (string s in a)
        {
            Vector2 sz = style.CalcSize(new GUIContent(s));
            size.y += sz.y;
            size.x = Mathf.Max(size.x, sz.x);
        }
        return size;
    }
    public static Rect InflateRect(Rect r, float w, float h)
    {
        r.x -= w / 2;
        r.y -= h / 2;
        r.width += w;
        r.height += h;
        return r;
    }

    static Texture2D texture;
    public static Texture2D Texture
    {
        get
        {
            if (texture == null)
            {
                texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
            }
            return texture;
        }
    }
    static Texture2D blackTexture;
    public static Texture2D BlackTexture
    {
        get
        {
            if (blackTexture == null)
            {
                blackTexture = new Texture2D(1, 1);
                blackTexture.SetPixel(0, 0, Color.black);
                blackTexture.Apply();
            }
            return blackTexture;
        }
    }
    public static readonly Color[] HealthColors3 = new Color[] { Color.red, Color.yellow, Color.green };
    public static readonly Color[] HealthColors2 = new Color[] { Color.red, Color.green };

    public static Color ColorLerp(float val, Color[] colors)
    {
        if (colors.Length == 1)
            return colors[0];
        if (colors.Length == 2)
        {
            return Color.Lerp(colors[0], colors[1], val);
        }
        if (colors.Length == 3)
        {
            if (val<0.5f)
                return Color.Lerp(colors[0], colors[1], val * 2);
            else
                return Color.Lerp(colors[1], colors[2], val * 2-1);
        }
        return Color.black;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
