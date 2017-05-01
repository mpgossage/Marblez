using UnityEngine;
using System.Collections;

public class TileClickDetector : MonoBehaviour
{
    private int gridX, gridY;
    private MarblezControl callback;
    public void SetGridPos(int x, int y, MarblezControl mc)
    {
        gridX = x;
        gridY = y;
        callback = mc;
    }
    // OnMouseDown only works for LMB (why?)
    // so using mouseover with click detection
    void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))  // left
        {
            //Debug.LogFormat("LMB {0} {1}", gridX, gridY);
            callback.mouseClick(gridX, gridY, true);
        }
        if (Input.GetMouseButtonUp(1))  // right
        {
            //Debug.LogFormat("RMB {0} {1}", gridX, gridY);
            callback.mouseClick(gridX, gridY, false);
        }
    }
}
