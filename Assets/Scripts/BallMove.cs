using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BallMove : MonoBehaviour 
{
    [SerializeField]
    private Sprite[] sprites;

	public enum Direction{Up,Down,Left,Right,Error};
	delegate Vector3 Path(float val);	// path takes in 0..1 & returns a path in range -1..0..+1
	public static float BallSpeed=1.0f;
	
	int gridX,gridY;	
	int colour;
	Direction inDir,outDir;
	Vector3 gridCentre;
	float pathPos;
	Path currentPath;
	
	MarblezControl map;
    private SpriteRenderer spriteRenderer;
	
	public int GridX{get{return gridX;}}
	public int GridY{get{return gridY;}}
    public int Colour { get { return colour; } }
	
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

	public void Init(int x,int y,Direction dir,int col)
	{
		gridX=x;	gridY=y;
		SetColour(col);
		inDir=outDir=dir;
		gridCentre=MarblezControl.GridToVector3(gridX,gridY);
		gridCentre.z=-50;	// a long way back
		pathPos=0.5f;	// mid path
		GetPath();	// get it
		transform.position=gridCentre;
	}
	
	/// <summary>
	/// Changes the colour of the ball
	/// </summary>
	public void ChangeColour()
	{
		if (colour<3)
			SetColour(colour+1);
		else if (colour==3)
			SetColour(0);	// wrap
		else
		{
			// its white/grey: pick anything
			SetColour(Random.Range(0,4));
		}
	}
	
	public void SetColour(int col)
	{
		colour=col;
        spriteRenderer.sprite = sprites[col];
	}
	
	// Use this for initialization
	void Start () 
	{
		map=GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MarblezControl>();	
	}
	
	// Update is called once per frame
	void Update () 
	{
        if (outDir == Direction.Error)    // special expection: trapped ball
        {
            int gridCell = MapLoader.Tiles[gridX, gridY]; // current grid
            outDir = tileExits[gridCell, (int)inDir];   // try to look for the exit
            if (outDir != Direction.Error)
            {
                GetPath();  // get the path again
                pathPos = 0;	// reset pos
            }
            return; // dont move this cycle
        }
        float oldPos = pathPos;
		pathPos+=Time.deltaTime*BallSpeed;
        if (pathPos >= 0.5f && oldPos < 0.5f)   // just crossed the 1/2 way point
            map.BallOnTile(this, gridX, gridY);   // tell the game controller
		if (pathPos>1)
			NextGrid();	// move to next grid
        Vector3 pos = gridCentre + 64 * currentPath(pathPos);
        pos.z = -50; // in front of tiles, but behind the tile tops
        transform.position = pos;
	}
	
	void GetPath()
	{
        // a simple table lookup
		int idx=4*(int)inDir+(int)outDir;
		currentPath=paths[idx];
	}

    /// <summary>
    /// forces the ball to change direction
    /// </summary>
    /// <param name="dir"></param>
    public void SetDirection(Direction dir)
    {
       ///* Debug.Log("SetDirection " + dir + " oldpath " + currentPath.ToString());
        inDir = outDir = dir;
        GetPath(); // and a new path
    }
	
	// have finished the path, so next grid:
	void NextGrid()
	{
		// figure out where we will be
		int newGx=gridX+nextGrid[(int)outDir,0];
		int newGy=gridY+nextGrid[(int)outDir,1];
		// get grid cell
		int newGridCell=0;
		if (newGx>=0 && newGx<10 && newGy>=0 && newGy<7)
			newGridCell=MapLoader.Tiles[newGx,newGy];
        Direction newInDir = Direction.Error, newOutDir = Direction.Error;
        newInDir = entranceDirection[(int)outDir];  // lookup the new entrance direction
        newOutDir = tileExits[newGridCell, (int)newInDir];
        //Debug.Log(string.Format("Moving to cell {0},{1} dir {2}, type: {3}, dir: {4}-{5}",
        //                        newGx, newGy, outDir, newGridCell, newInDir, newOutDir));
        if (newOutDir != Direction.Error) // valid tile
        {
            // update grid & directions
            gridX = newGx;
            gridY = newGy;
            inDir = newInDir;
            outDir = newOutDir;
        }
        else
        {
            // rebound:
            // gridx&y are the same
            inDir = outDir; // exit right becomes enter right
            newGridCell = MapLoader.Tiles[gridX, gridY]; // current grid
            outDir = tileExits[newGridCell, (int)inDir];
            map.PlayBounce();
            // special case: if the cell is a turn table & might have moved
            // there might not be an exit
            if (outDir == Direction.Error)
            {
                Debug.Log("Ball stuck!!!!");
                return;
            }
        }
		// update the grid centre
		gridCentre=MarblezControl.GridToVector3(gridX,gridY);
		gridCentre.z=900;	// a long way back
		GetPath(); // and a new path
		pathPos=0;	// reset pos
	}

	#region Lookup Tables
    // the functions for motion:
    private static Vector3 PathLR(float val) { return new Vector3(val - 0.5f, 0, 0); }
    private static Vector3 PathRL(float val) { return PathLR(1 - val); }
    private static Vector3 PathUD(float val) { return new Vector3(0, 0.5f - val, 0); }
    private static Vector3 PathDU(float val) { return PathUD(1 - val); }
    private static Vector3 PathUU(float val) { if (val > 0.5f) val = 1 - val; return PathUD(val); }
    private static Vector3 PathDD(float val) { return -PathUU(val); }
    private static Vector3 PathLL(float val) { if (val > 0.5f) val = 1 - val; return PathLR(val); }
    private static Vector3 PathRR(float val) { return -PathLL(val); }
    private static Vector3 PathUL(float val) { val *= (Mathf.PI / 2); return new Vector3(Mathf.Cos(val) - 1, 1 - Mathf.Sin(val)) * 0.5f; }
    private static Vector3 PathUR(float val) { val *= (Mathf.PI / 2); return new Vector3(1 - Mathf.Cos(val), 1 - Mathf.Sin(val)) * 0.5f; }
    private static Vector3 PathDL(float val) { val *= (Mathf.PI / 2); return new Vector3(Mathf.Cos(val) - 1, Mathf.Sin(val) - 1) * 0.5f; }
    private static Vector3 PathDR(float val) { val *= (Mathf.PI / 2); return new Vector3(1 - Mathf.Cos(val), Mathf.Sin(val) - 1) * 0.5f; }
    private static Vector3 PathLU(float val) { return PathUL(1 - val); }
    private static Vector3 PathLD(float val) { return PathDL(1 - val); }
    private static Vector3 PathRU(float val) { return PathUR(1 - val); }
    private static Vector3 PathRD(float val) { return PathDR(1 - val); }
    // lookup table for paths X is outDir, Y in inDir
	readonly Path[] paths={	PathUU,PathUD,PathUL,PathUR,
							PathDU,PathDD,PathDL,PathDR,
							PathLU,PathLD,PathLL,PathLR,
							PathRU,PathRD,PathRL,PathRR};
	// lookup table to update the gridXY based upon Direction
    readonly int[,] nextGrid = { { 0, -1 }, { 0, +1 }, { -1, 0 }, { +1, 0 } };
    // lookup table for converting exit direction to entrance direction
    readonly Direction[] entranceDirection = { Direction.Down, Direction.Up, Direction.Right, Direction.Left };
    // lookup table for the exits for each tile type
    readonly Direction[,] tileExits={
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // blank
                            {Direction.Error,Direction.Error,Direction.Right,Direction.Left},  // horiz
                            {Direction.Down,Direction.Up,Direction.Error,Direction.Error},  // vert
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},  // cross
                            {Direction.Right,Direction.Left,Direction.Down,Direction.Up},  // UR LD
                            {Direction.Left,Direction.Right,Direction.Up,Direction.Down},  // UL DR
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},  // hole
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},  // cross
                            {Direction.Error,Direction.Right,Direction.Error,Direction.Down},  // DR curve
                            {Direction.Error,Direction.Left,Direction.Down,Direction.Error},  // LD curve
                            {Direction.Error,Direction.Right,Direction.Right,Direction.Left},  // horz+DR
                            {Direction.Error,Direction.Left,Direction.Right,Direction.Left},  // horz+DL
                            {Direction.Down,Direction.Up,Direction.Error,Direction.Down},  // vert+DR
                            {Direction.Down,Direction.Up,Direction.Down,Direction.Error},  // vert+DL
                            {Direction.Down,Direction.Up,Direction.Up,Direction.Up},  // arrow up
                            {Direction.Right,Direction.Right,Direction.Right,Direction.Left},  // right arrow
                            {Direction.Right,Direction.Error,Direction.Error,Direction.Up},  // UR curve
                            {Direction.Left,Direction.Error,Direction.Up,Direction.Error},  // UL curve
                            {Direction.Right,Direction.Error,Direction.Right,Direction.Left},  // horz+UR
                            {Direction.Left,Direction.Error,Direction.Right,Direction.Left},  // horz+UL
                            {Direction.Down,Direction.Up,Direction.Error,Direction.Up},  // vert+UR
                            {Direction.Down,Direction.Up,Direction.Up,Direction.Error},  // vert+UL
                            {Direction.Down,Direction.Up,Direction.Down,Direction.Down},  // arrow D
                            {Direction.Left,Direction.Left,Direction.Right,Direction.Left},  // arrow L
                            // URDL rebound
                            {Direction.Up,Direction.Error,Direction.Error,Direction.Error},
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Right},
                            {Direction.Error,Direction.Down,Direction.Error,Direction.Error},
                            {Direction.Error,Direction.Error,Direction.Left,Direction.Error},
                            // URDL catcher
                            {Direction.Up,Direction.Error,Direction.Error,Direction.Error},
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Right},
                            {Direction.Error,Direction.Down,Direction.Error,Direction.Error},
                            {Direction.Error,Direction.Error,Direction.Left,Direction.Error},
                            // URDL spawn
                            {Direction.Up,Direction.Error,Direction.Error,Direction.Error},
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Right},
                            {Direction.Error,Direction.Down,Direction.Error,Direction.Error},
                            {Direction.Error,Direction.Error,Direction.Left,Direction.Error},
                            // URDL home
                            {Direction.Up,Direction.Error,Direction.Error,Direction.Error},
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Right},
                            {Direction.Error,Direction.Down,Direction.Error,Direction.Error},
                            {Direction.Error,Direction.Error,Direction.Left,Direction.Error},
                            // turntables
                            {Direction.Left,Direction.Error,Direction.Up,Direction.Error},  // UL curve
                            {Direction.Right,Direction.Error,Direction.Error,Direction.Up},  // UR curve
                            {Direction.Error,Direction.Right,Direction.Error,Direction.Down},  // DR curve
                            {Direction.Error,Direction.Left,Direction.Down,Direction.Error},  // LD curve
                            {Direction.Left,Direction.Right,Direction.Up,Direction.Down},  // UL DR
                            {Direction.Right,Direction.Left,Direction.Down,Direction.Up},  // UR LD
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // blank
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // blank
                            // turntables (partial turned)
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // bounce all
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // bounce all
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // bounce all
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // bounce all
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // bounce all
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // bounce all
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // blank 
                            {Direction.Error,Direction.Error,Direction.Error,Direction.Error},  // blank
                            // redirectors
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},
                            // Colour changers
                            {Direction.Error,Direction.Error,Direction.Right,Direction.Left},  // horiz
                            {Direction.Down,Direction.Up,Direction.Error,Direction.Error},  // vert
                            {Direction.Down,Direction.Up,Direction.Right,Direction.Left},  // cross
                            };
	#endregion
}
