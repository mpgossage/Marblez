using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class MarblezControl : MonoBehaviour
{
    public const int GRID_W = 10, GRID_H = 7;
    [SerializeField]
    private GameObject tileObject;
    [SerializeField]
    private Sprite[] tileMap, tileTops;
    [SerializeField]
    private GameObject ballObject;


    #region sprites
    SpriteRenderer[,] uTileSprites;    // 2d array of the sprites for the tiles
    SpriteRenderer[,] uTileTopSprites;	// 2d array of the sprites for the tile tops (mainly nulls)
    private BallMove[] ballSprites;
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
        public AudioClip ballStick, ballRelease;
        public AudioClip ballChange;
        public AudioClip timeup, levelComplete;
    }
    [SerializeField]
    private MarblezSounds sounds;

    public MarblezSounds Sounds { get { return sounds; } }

    #region Unity Events
    // called before starting
    void Awake()
    {
        // must call this early as others may depend upon it
        string scenename = string.Format("level{0:00}", GameState.levelNumber);
        MapLoader.LoadMap(scenename);
    }
    // Use this for initialization
    void Start()
    {
        directionSwap = false;  // reset the direction

        CreateBalls();
        CreateTiles();
        UpdateTiles();

        //SpawnBall();
        //SpawnBall(2,0);
        //SpawnBall(2,0);
        //SpawnBall(5,0);
        gui = GetComponent<GameGui>();

        // spawn a ball every 5 seconds (first one quickly)
        InvokeRepeating("SpawnBall", 1.5f, 5);
    }
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
            gui.BallHome(); // one less ball
#endif
        // look for a click:
        if (Input.GetMouseButtonUp(0))  // left
        {
            Vector2 gridPos = Vector3ToGrid(Input.mousePosition);
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
                uTileSprites[i, j].sprite = tileMap[MapLoader.Tiles[i, j]];
            }
        }
    }

    void CreateTiles()
    {
        Transform tileHolder = tileObject.transform.parent;

        uTileSprites = new SpriteRenderer[GRID_W, GRID_H];
        for (int i = 0; i < GRID_W; i++)
        {
            for (int j = 0; j < GRID_H; j++)
            {
                GameObject tile = Instantiate(tileObject);
                tile.transform.SetParent(tileHolder);
                tile.transform.position = GridToVector3(i, j);
                uTileSprites[i, j] = tile.GetComponent<SpriteRenderer>();
            }
        }

        // now the tops:
        uTileTopSprites = new SpriteRenderer[GRID_W, GRID_H];
        int colour = 0;	// each top should be a different colour
        for (int i = 0; i < GRID_W; i++)
        {
            for (int j = 0; j < GRID_H; j++)
            {
                int tileType = MapLoader.Tiles[i, j];
                int top = -1;
                if (tileType == 7 || (tileType >= 36 && tileType <= 39))  // a home
                {
                    top = colour;   // the colour
                    colour = (colour + 1) % 4;  // next colour
                }
                else if (tileType >= 32 && tileType <= 35)  // a launcher
                {
                    top = tileType - 32 + 10;
                }
                else if (tileType >= 56 && tileType <= 60)  // redirector
                {
                    top = tileType - 56 + 4;
                }
                else if (tileType >= 61)    // colour changer
                {
                    top = 9;
                }
                if (top >= 0)   // there is a top
                {
                    GameObject spr = Instantiate(tileObject);
                    spr.transform.SetParent(tileHolder);
                    spr.transform.position = GridToVector3(i, j) + new Vector3(0, 0, -100); // more forward
                    uTileTopSprites[i, j] = spr.GetComponent<SpriteRenderer>(); ;
                    uTileTopSprites[i, j].sprite = tileTops[top];
                }
            }
        }
        tileObject.SetActive(false);    // hide original
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
        if (ballSprites != null) return;
        Transform parent = ballObject.transform.parent;
        ballSprites = new BallMove[MapLoader.MaxBalls];
        for (int i = 0; i < ballSprites.Length; i++)
        {
            // create the ball, but disable it
            GameObject obj = Instantiate(ballObject);
            obj.transform.SetParent(parent);
            obj.SetActive(false);    // its not active
            ballSprites[i] = obj.GetComponent<BallMove>();   // store for later
        }
        ballObject.SetActive(false);    // hide original
    }
    /// <summary>
    /// spawns the next ball, at wherever it should be
    /// </summary>
    public void SpawnBall()
    {
        // find an inactive ball
        BallMove bm = null;
        foreach (var b in ballSprites)
        {
            if (b.gameObject.activeSelf == false)
            {
                bm = b;
                break;
            }
        }
        if (bm == null) return;
        Vector2 pos = GetSpawnPoint();
        // get the grid info:
        int tileType = MapLoader.Tiles[(int)pos.x, (int)pos.y];
        // new get the direction of ball motion:
        BallMove.Direction dir = BallMove.Direction.Error;
        if (tileType == 32)
            dir = BallMove.Direction.Up;
        if (tileType == 33)
            dir = BallMove.Direction.Right;
        if (tileType == 34)
            dir = BallMove.Direction.Down;
        if (tileType == 35)
            dir = BallMove.Direction.Left;
        // colour (fron GUI)
        int col = gui.NextBallColour();
        // activate the ball:
        bm.gameObject.SetActive(true);
        bm.enabled = true;
        bm.Init((int)pos.x, (int)pos.y, dir, col);
        // sound effect:
        GetComponent<AudioSource>().PlayOneShot(sounds.ballSpawn);
    }
    private Vector2 GetSpawnPoint()
    {
        List<Vector2> spawnPoints = new List<Vector2>();
        for (int i = 0; i < GRID_W; i++)
        {
            for (int j = 0; j < GRID_H; j++)
            {
                if (MapLoader.Tiles[i, j] >= 32 && MapLoader.Tiles[i, j] <= 35)
                    spawnPoints.Add(new Vector2(i, j));
            }
        }
        if (spawnPoints.Count < 1) return Vector2.zero;
        // pick one:
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
    void DisableBall(BallMove bm)
    {
        bm.gameObject.SetActive(false);
        bm.enabled = false;
    }
    public void DisableAllBalls()
    {
        foreach (var b in ballSprites)
        {
            b.gameObject.SetActive(false);
        }
    }

    #endregion

    /// <summary>
    /// call by the tile click routine when the user clicks on a tile
    /// </summary>
    /// <param name="gridPos"></param>
    public void mouseClick(Vector2 gridPos, bool leftButton)
    {
        //Debug.Log("mouseClick" + gridPos);
        int gx = (int)gridPos.x, gy = (int)gridPos.y;
        int tileType = MapLoader.Tiles[gx, gy];
        if (tileType >= 40 && tileType <= 45)
        {
            bool clockwise = !leftButton;   // left button is anti-clockwise
            if (directionSwap)
                clockwise = !clockwise;

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
            GetComponent<AudioSource>().PlayOneShot(sounds.tileClick);
        }
        else if (tileType >= 28 && tileType <= 31)  // ball catcher
        {
            // check all balls to see if there are any disabled on this cell
            BallMove bm = FindTrappedBall(gx, gy);
            if (bm != null)
            {
                bm.enabled = true;  // turn on
                GetComponent<AudioSource>().PlayOneShot(sounds.ballRelease);
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
            BallMove.Direction dir = (BallMove.Direction)Random.Range(0, 4);
            if (tileType == 56) dir = BallMove.Direction.Up;
            if (tileType == 57) dir = BallMove.Direction.Right;
            if (tileType == 58) dir = BallMove.Direction.Down;
            if (tileType == 59) dir = BallMove.Direction.Left;

            bm.SetDirection(dir);   // force it to change direction
        }
        else if (tileType >= 61)    // colour change
        {
            bm.ChangeColour();
            GetComponent<AudioSource>().PlayOneShot(sounds.ballChange);
        }
        else if (tileType >= 28 && tileType <= 31)  // ball catcher
        {
            // see if any already trapped:
            if (FindTrappedBall(gridX, gridY) == null)
            {
                // none trapped so trap
                bm.enabled = false;
                GetComponent<AudioSource>().PlayOneShot(sounds.ballStick);
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
        int homeColour = GetHomeColour(gridX, gridY);
        // right ball or wrong ball?
        bool correct = false;
        int ballColour = bm.Colour;
        if (ballColour >= 4)    // gray/white
        {
            correct = true;
        }
        else
        {
            if (homeColour == ballColour)
                correct = true;
        }
        if (correct)
        {
            GetComponent<AudioSource>().PlayOneShot(sounds.ballHome);   // correct
            uTileTopSprites[gridX,gridY].sprite = tileTops[Random.Range(0, 4)];    // new top 
            if (ballColour == 4)    // grey
                GrayEffect();   // extras
            else
                gui.BallHome(); // one less ball
        }
        else
        {
            gui.WrongBallHome();    // one less ball
            GetComponent<AudioSource>().PlayOneShot(sounds.ballWrongHome); // wrong
        }
        // disable no matter what
        DisableBall(bm);
    }
    int GetHomeColour(int gridX, int gridY)
    {
        // find the tile top for that cell:
        // cannot just get index, so need to cross ref the tiles
        SpriteRenderer spr = uTileTopSprites[gridX, gridY];
        for(int i=0;i<tileTops.Length;i++)
        {
            if (tileTops[i] == spr.sprite) return i;
        }
        return 0; // guess
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
        GetComponent<AudioSource>().PlayOneShot(sounds.ballDestroy);
    }
	// public so others can call it
	public void PlayBounce()
	{
		GetComponent<AudioSource>().PlayOneShot(sounds.ballBounce);
	}
 
	
	private BallMove FindTrappedBall(int gridX,int gridY)
	{
		foreach(var bm in ballSprites)
		{
			if (bm.enabled==false &&	// disabled
				bm.GridX==gridX && bm.GridY==gridY)	// same cell
			{
				return bm;
			}
		}
		return null;
	}
 
}
