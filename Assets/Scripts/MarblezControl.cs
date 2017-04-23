using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class MarblezControl : MonoBehaviour 
{
	public const int GRID_W=10,GRID_H=7;
	public OTContainer tilesSpriteSheet;
	public OTContainer tileTopsSpriteSheet;
	public OTContainer ballSpriteSheet;
	
	#region sprites
	OTSprite[,] tileSprites;	// 2d array of the sprites for the tiles
	OTSprite[,] tileTopSprites;	// 2d array of the sprites for the tile tops (mainly nulls)
    OTSprite[] ballSprites;
	#endregion
	
	GameGui gui;
	bool directionSwap;
	
	// trick to group sounds together
	// must be marked System.Serializable, 
	// [SerializeField] doesn't work
	[System.Serializable]
	public class MarblezSounds
	{
		public AudioClip tileClick;
		public AudioClip ballSpawn, ballBounce, ballDestroy;
		public AudioClip ballHome, ballWrongHome;
		public AudioClip ballStick,ballRelease;
		public AudioClip ballChange;
        public AudioClip timeup, levelComplete;
	}
	public MarblezSounds sounds=new MarblezSounds();
	
	#region Unity Events
    // called before starting
    void Awake()
    {
        // must call this early as others may depend upon it
		string scenename=string.Format("level{0:00}",GameState.levelNumber);
        MapLoader.LoadMap(scenename);
    }
	// Use this for initialization
	void Start () 
	{
        directionSwap = false;  // reset the direction

        CreateBalls();
		CreateTiles();
		CreateTileTops();
		UpdateTiles();

        //SpawnBall();
		//SpawnBall(2,0);
		//SpawnBall(2,0);
		//SpawnBall(5,0);
		gui=GetComponent<GameGui>();

        // spawn a ball every 5 seconds (first one quickly)
        InvokeRepeating("SpawnBall", 1.5f, 5);
	}
	void Update()
	{
        if (Input.GetKey(KeyCode.F1))
            gui.BallHome();	// one less ball
		// look for a click:
		if (Input.GetMouseButtonUp(0))  // left
		{
			Vector2 gridPos=Vector3ToGrid(Input.mousePosition);
            //Debug.Log("mouse" + Input.mousePosition + " grid:" + gridPos);
            if (gridPos.y <= 7) // check limits
                mouseClick(gridPos, true);
		}
        if (Input.GetMouseButtonUp(1))  // right
        {
            Vector2 gridPos = Vector3ToGrid(Input.mousePosition);
            if (gridPos.y <= 7)
                mouseClick(gridPos, false);
        }
	}
	#endregion

		
	#region Tiles
	void UpdateTiles()
	{
        for (int j = 0; j < GRID_H; j++)
		{
            for (int i = 0; i < GRID_W; i++)
			{
                tileSprites[i, j].frameIndex = MapLoader.Tiles[i, j];
			}
		}
	}

	void CreateTiles()
	{
		tileSprites=new OTSprite[GRID_W, GRID_H];
		Vector2 defaultSize=new Vector2(64,64);

        for (int i = 0; i < GRID_W; i++)
		{
            for (int j = 0; j < GRID_H; j++)
			{
				OTSprite spr=OT.CreateObject(OTObjectType.Sprite).GetComponent<OTSprite>();
				spr.spriteContainer=tilesSpriteSheet;
				spr.frameIndex=0;
				spr.transform.position=GridToVector3(i,j);
				spr.size=defaultSize;
				spr.transparent=false;
				spr.depth=1000;	// at the back
				tileSprites[i,j]=spr;
			}
		}
	}
	void CreateTileTops()
	{
		tileTopSprites=new OTSprite[GRID_W, GRID_H];
		// now the tops:
		int colour=0;	// each top should be a different colour
        for (int i = 0; i < GRID_W; i++)
		{
            for (int j = 0; j < GRID_H; j++)
			{
                int tileType = MapLoader.Tiles[i, j];
				int top=-1;
		        if (tileType == 7 || (tileType >= 36 && tileType <= 39))  // a home
				{
					top=colour;	// the colour
					colour=(colour+1)%4;	// next colour
				}
				else if (tileType>=32 && tileType<=35)	// a launcher
				{
					top=tileType-32+10;
				}
				else if (tileType>=56 && tileType<=60)	// redirector
				{
					top=tileType-56+4;
				}
				else if (tileType>=61)	// colour changer
				{
					top=9;
				}
				if (top>=0)	// there is a top
				{
					OTSprite spr=OT.CreateObject(OTObjectType.Sprite).GetComponent<OTSprite>();
					spr.spriteContainer=tileTopsSpriteSheet;
					spr.frameIndex=top;
					spr.transform.position=GridToVector3(i,j);
					spr.size=new Vector2(28,28);
					spr.transparent=true;
					spr.depth=800;	// in front of the balls
					tileTopSprites[i,j]=spr;
				}
			}
		}
		
	}
	#endregion

    #region Grid/Pos Conversion
    public static Vector3 GridToVector3(int gridX, int gridY)
    {
        // offset of pos (y is +ve because openGL us +Y up)
        Vector3 defaultPos = new Vector3(-5 * 64 + 32, 3 * 64 + 16, 0);
        Vector3 pos = defaultPos;
        pos.x += 64 * gridX;
        // (y is -ve because openGL us +Y up)
        pos.y -= 64 * gridY;
        return pos;
    }
    public static Vector2 Vector3ToGrid(Vector3 screen)
    {
        // offset of pos (y is +ve because openGL us +Y up)
        screen.y = 480 - screen.y;
        return new Vector2(Mathf.FloorToInt(screen.x / 64), Mathf.FloorToInt(screen.y / 64));
    } 
    #endregion

    #region Ball Managment
    private void CreateBalls()
    {
        if (ballSprites!=null)    return;
        ballSprites = new OTSprite[MapLoader.MaxBalls];
        for (int i = 0; i < ballSprites.Length; i++)
        {
            // create the ball, but disable it
            GameObject obj=OT.CreateObject(OTObjectType.Sprite);
		    OTSprite spr=obj.GetComponent<OTSprite>();
		    spr.spriteContainer=ballSpriteSheet;
		    spr.size=new Vector2(19,19);
		    spr.transparent=true;
		    spr.depth=900;	// fairly back
		    BallMove bm=obj.AddComponent<BallMove>();
            bm.enabled = false;
            obj.SetActive(false);    // its not active
            ballSprites[i]=spr;   // store for later
        }
    }
	/// <summary>
    /// spawns the next ball, at wherever it should be
    /// </summary>
    public void SpawnBall()
    {
        // find an inactive ball
        OTSprite spr=null;
        foreach(OTSprite b in ballSprites)
        {
            if (b.gameObject.activeSelf==false)
            {
                spr=b;
                break;
            }
        }
        if (spr==null)  return;
        // being lazy & looking for spawn places now!
        List<Vector2> spawnPoints = new List<Vector2>();
        for (int i = 0; i < GRID_W; i++)
        {
            for (int j = 0; j < GRID_H; j++)
            {
                if (MapLoader.Tiles[i, j] >= 32 && MapLoader.Tiles[i, j] <= 35)
                    spawnPoints.Add(new Vector2(i, j));
            }
        }
        if (spawnPoints.Count < 1) return;
        // pick one:
        Vector2 pos = spawnPoints[Random.Range(0, spawnPoints.Count)];
        // get the grid info:
        int tileType = MapLoader.Tiles[(int)pos.x, (int)pos.y];
        // new get the direction of ball motion:
        BallMove.Direction dir=BallMove.Direction.Error;
        if (tileType==32)
            dir=BallMove.Direction.Up;
        if (tileType==33)
            dir=BallMove.Direction.Right;
        if (tileType==34)
            dir=BallMove.Direction.Down;
        if (tileType==35)
            dir=BallMove.Direction.Left;
        // colour (fron GUI)
        int col=gui.NextBallColour();
        // activate the ball:
        spr.gameObject.SetActive(true);
        BallMove bm=spr.gameObject.GetComponent<BallMove>();
        bm.enabled = true;
		bm.Init((int)pos.x,(int)pos.y,dir,col);
        // sound effect:
        audio.PlayOneShot(sounds.ballSpawn);
	}
    void DisableBall(BallMove bm)
    {
        bm.gameObject.SetActive(false);
        bm.enabled = false;
    }
	public void DisableAllBalls()
	{
		foreach(OTSprite b in ballSprites)
		{
			b.gameObject.SetActive(false);;
			//b.GetComponent<BallMove>().
		}		
	}

    #endregion

    /// <summary>
    /// call by the tile click routine when the user clicks on a tile
    /// </summary>
    /// <param name="gridPos"></param>
    public void mouseClick(Vector2 gridPos,bool leftButton)
    {
        //Debug.Log("mouseClick" + gridPos);
        int gx=(int)gridPos.x,gy=(int)gridPos.y;
        int tileType = MapLoader.Tiles[gx, gy];
        if (tileType >= 40 && tileType <= 45)
        {
            bool clockwise=!leftButton;	// left button is anti-clockwise
			if (directionSwap)
				clockwise=!clockwise;
			
            if (clockwise)
            {
                if (tileType == 43)
                    tileType = 40;  // wrap
                else if (tileType == 45)
                    tileType = 44;  // wrap
                else
                    tileType++; // next tile
            }
            else
            {
                // counter clockwise
                if (tileType == 40)
                    tileType = 43;  // wrap
                else if (tileType == 44)
                    tileType = 45;  // wrap
                else
                    tileType--; // next tile
            }
            MapLoader.Tiles[gx, gy] = tileType;
            //Debug.Log("tiletype" + tileType);
            UpdateTiles();  // overkill to update all the tiles
            audio.PlayOneShot(sounds.tileClick);
        }
		else if (tileType>=28 && tileType<=31)	// ball catcher
		{
			// check all balls to see if there are any disabled on this cell
			BallMove bm=FindTrappedBall(gx,gy);
			if (bm!=null)
			{
				bm.enabled=true;	// turn on
	            audio.PlayOneShot(sounds.ballRelease);
			}
		}
    }
    /// <summary>
    /// called by the ball, when it hits the middle of the tile
    /// </summary>
    public void BallOnTile(BallMove bm, int gridX, int gridY)
    {
        //Debug.Log("Ball reached middle of tile " + gridX + "," + gridY);
        int tileType = MapLoader.Tiles[gridX, gridY];
        if (tileType == 7 || (tileType >= 36 && tileType <= 39))  // at home
        {
            BallHome(bm, gridX, gridY);
        }
        else if (tileType == 6)  // ball destroyer
        {
            BallDestroy(bm); 
        }
        else if (tileType >= 24 && tileType <= 27)  // bouncer
        {
            PlayBounce();
        }
        else if (tileType >= 32 && tileType <= 35)  // spawn
        {
            PlayBounce();
        }
        else if (tileType >= 56 && tileType <= 60)  // redirector
        {
            BallMove.Direction dir = (BallMove.Direction)Random.Range(0,4);
            if (tileType == 56) dir = BallMove.Direction.Up;
            if (tileType == 57) dir = BallMove.Direction.Right;
            if (tileType == 58) dir = BallMove.Direction.Down;
            if (tileType == 59) dir = BallMove.Direction.Left;

            bm.SetDirection(dir);   // force it to change direction
        }
		else if (tileType >= 61)	// colour change
		{
			bm.ChangeColour();
			audio.PlayOneShot(sounds.ballChange);
		}
		else if (tileType>=28 && tileType<=31)	// ball catcher
		{
			// see if any already trapped:
			if (FindTrappedBall(gridX,gridY)==null)
			{
				// none trapped so trap
				bm.enabled=false;
				audio.PlayOneShot(sounds.ballStick);
			}
			else
			{
				// there is a trapped, so bounce:
				PlayBounce();
			}
		}
    }

    private void BallHome(BallMove bm, int gridX, int gridY)
    {
		// find the tile top for that cell:
		OTSprite tileTop=tileTopSprites[gridX,gridY];
		int homeColour=tileTop.frameIndex;
		// right ball or wrong ball?
		bool correct=false;
		int ballColour=bm.GetColour();
		if (ballColour>=4)	// gray/white
		{
			correct=true;
		}
		else
		{
			if (homeColour==ballColour)
				correct=true;
		}
		if (correct)
		{
	        audio.PlayOneShot(sounds.ballHome);   // correct
			tileTop.frameIndex=Random.Range(0,4);	// new top 
			if (ballColour==4)	// grey
				GrayEffect();	// extras
			else
				gui.BallHome();	// one less ball
		}
		else
		{
			gui.WrongBallHome();	// one less ball
			audio.PlayOneShot(sounds.ballWrongHome); // wrong
		}
        // disable no matter what
        DisableBall(bm);
    }
	private void GrayEffect()
	{
        GameState.levelScore += 50; // some score
		int rnd=Random.Range(0,5);
		if (rnd==0)
		{
			//int time=15*Random.Range(2,7);	// 30..90
			int time=+60;
			gui.TimeBonus(time);
			gui.ShowText("Time bonus "+time+" seconds");
		}
		if (rnd==1)
		{
			//int time=15*Random.Range(2,7);	// 30..90
			int time=+60;
			gui.TimeBonus(-time);
			gui.ShowText("Time reduced by "+time+" seconds");
		}
		if (rnd==2)
		{
			//int req=Random.Range(1,4); // 1..3
			int req=2;
			gui.ChangeBallRequirments(-req);
			gui.ShowText("Ball requirement reduced by "+req);
		}
		if (rnd==3)
		{
			//int req=Random.Range(1,4); // 1..3
			int req=2;
			gui.ChangeBallRequirments(req);
			gui.ShowText("Ball requirement increased by "+req);
		}
		if (rnd==4)
		{
			directionSwap=!directionSwap;
			gui.ShowText("Turntable direction flipped");			
		}
	}
    private void BallDestroy(BallMove bm)
    {
        DisableBall(bm);
        audio.PlayOneShot(sounds.ballDestroy);
    }
	// public so others can call it
	public void PlayBounce()
	{
		audio.PlayOneShot(sounds.ballBounce);
	}
 
	
	private BallMove FindTrappedBall(int gridX,int gridY)
	{
		foreach(OTSprite b in ballSprites)
		{
			BallMove bm=b.GetComponent<BallMove>();
			if (bm.enabled==false &&	// disabled
				bm.GridX==gridX && bm.GridY==gridY)	// same cell
			{
				return bm;
			}
		}
		return null;
	}
 
}
