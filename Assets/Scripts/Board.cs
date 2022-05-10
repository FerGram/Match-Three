using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class Board : MonoBehaviour
{
    //Board size variables
    public int width;
    public int height;
    public float marginSize;
    //Moving dots variables
    public int falseYOffset = 10;
    public float moveTime = 0.5f;
    public StartingObject[] startingTiles;
    public StartingObject[] startingDots;


    [Header("Prefabs")]
    [SerializeField] GameObject _tileNormalPrefab;
    [SerializeField] GameObject _tileObstaclePrefab;
    [SerializeField] GameObject[] _dotPrefabs;

    [SerializeField] GameObject _adjacentBombPrefab;
    [SerializeField] GameObject _columnBombPrefab;
    [SerializeField] GameObject _rowBombPrefab;
    [SerializeField] GameObject _colorBombPrefab;

    [Header("Collectibles")]
    [SerializeField] float _swapTime = 0.5f;
    [SerializeField] int _maxCollectibles = 3;
    [SerializeField] int _collectibleCount = 0;
    [Range(0,1)][SerializeField] float _chanceForCollectible = 0.1f;
    [SerializeField] GameObject[] _collectiblePrefabs;

    private Tile[,] _tileBoard;
    private GameDot[,] _dotBoard;
    private Tile _clickedTile;
    private Tile _targetTile;
    private ParticleManager _particleManager;

    private GameObject _clickedTileBomb;
    private GameObject _targetTileBomb;

    private int _scoreMultiplier = 0;

    private bool _playerInputEnabled = true;

    //Class for desired initial dots
    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;
        public int x;
        public int y;
        public int z;
    }

    void Start()
    {
        _particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();
        _tileBoard = new Tile[width, height];
        _dotBoard = new GameDot[width, height];
    }

    public void SetUpBoard()
    {
        SetUpTiles();
        SetUpGameDots();

        List<GameDot> startingCollectibles = FindAllCollectibles();
        _collectibleCount = startingCollectibles.Count;

        SetUpCamera();
        FillRandom(falseYOffset, moveTime);
    }
    //Create Tiles, Dots, Bombs
    private void CreateTile(GameObject tilePrefab, int x, int y, int z = 0)
    {
        //Creates Tile, initialises its coordinates and adds it to the _tileBoard array
        GameObject newTile = Instantiate(tilePrefab, new Vector3(x, y, z), Quaternion.identity);
        newTile.name = "Tile (" + x + ", " + y + ")";
        _tileBoard[x, y] = newTile.GetComponent<Tile>();
        _tileBoard[x, y].Init(x, y, this);

        newTile.transform.parent = transform;
    }
    private void CreateGameDot(GameObject dot, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        //Set the given dot the proper coordinates and variable values, the proper parent and moves the dot
        if (dot != null && IsWithinBounds(x, y)) 
        {
            PlaceDot(dot.GetComponent<GameDot>(), x, y);

            dot.GetComponent<GameDot>().Init(this);
            dot.transform.parent = transform;
            if (falseYOffset != 0)
            {
                dot.transform.position = new Vector3(x, y + falseYOffset, 0);
                dot.GetComponent<GameDot>().MoveDot(x, y, moveTime);
            }
        }
    }
    private GameObject CreateBomb(GameObject prefab, int x, int y)
    {
        //Creates Bomb and initialises its coordinates
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);

            bomb.GetComponent<GameDot>().Init(this);
            bomb.GetComponent<GameDot>().SetCoordinates(x, y);
            bomb.transform.parent = transform;
            return bomb;
        }
        return null;
    }

    //Setup
    private void SetUpTiles()
    {
        //Sets initial desired tiles
        foreach (StartingObject sTile in startingTiles)
        {
            if (sTile != null)
            {
                CreateTile(sTile.prefab, sTile.x, sTile.y);
            }
        }
        //Sets rest of the tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_tileBoard[x, y] == null)
                {
                    CreateTile(_tileNormalPrefab, x, y);
                }
            }
        }
    }
    private void SetUpGameDots()
    {
        //Sets the initial desired dots
        foreach (StartingObject sDot in startingDots)
        {
            if (sDot != null)
            {
                GameObject dot = Instantiate(sDot.prefab, new Vector3(sDot.x, sDot.y, 0), Quaternion.identity);
                CreateGameDot(dot, sDot.x, sDot.y, falseYOffset, moveTime);
            }
        }
    }
    private void SetUpCamera()
    {
        Camera.main.transform.position = new Vector3(((float)width -1) / 2 , ((float)height -1) / 2, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        //Calculates the orthographic size (height/2) + margin for both horizontal and vertical axis
        //and take the largest
        float verticalSize = (float)height / 2 + (float)marginSize;
        float horizontalSize = ((float)width / 2 + (float)marginSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    //Fill with random dots
    private void FillRandom(int falseYOffset = 0, float moveTime = 0.1f)
    {
        //Fills the Board with dots that have no matches on fill
        //The top row can create Collectibles

        int maxIterations = 100;
        int iterations = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_dotBoard[x, y] == null && _tileBoard[x, y].tileType != TileType.Obstacle)
                {
                    GameDot dot = null;
                    if (y == height -1 && CanAddCollectible())
                    {
                        dot = FillRandomCollectibleAt(x, y, falseYOffset, moveTime);
                        _collectibleCount++;
                    }
                    else
                    {
                        dot = FillRandomDotAt(x, y, falseYOffset, moveTime);
                        iterations = 0;

                        //Repeat spawning the dot if matches on fill are found
                        while (HasMatchOnFill(x, y, 3))
                        {
                            ClearDotAt(x, y);
                            dot = FillRandomDotAt(x, y, falseYOffset, moveTime);
                            iterations++;

                            if (iterations >= maxIterations) { break; }
                        }
                    }
                }
            }
        }
    }
    private GameDot FillRandomDotAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        //Creates random dot and CreateGameDot initialises its variables, etc.
        if (IsWithinBounds(x, y))
        {
            GameObject dot = Instantiate(GetRandomDot(), Vector3.zero, Quaternion.identity);
            CreateGameDot(dot, x, y, falseYOffset, moveTime);
            return dot.GetComponent<GameDot>();
        }
        return null;
    }
    private GameDot FillRandomCollectibleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        //Creates random collectible and CreateGameDot initialises its variables, etc.
        if (IsWithinBounds(x, y))
        {
            GameObject dot = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity);
            CreateGameDot(dot, x, y, falseYOffset, moveTime);
            return dot.GetComponent<GameDot>();
        }
        return null;
    }
    private bool HasMatchOnFill(int x, int y, int minDotsCountLenght = 3)
    {
        //Given the x and y coordinates, find the matches on the left and right
        //If the dot has a match of 3 from either of the sides, return true

        List<GameDot> leftMatches = FindMatches(x, y, new Vector2(-1,0), minDotsCountLenght);
        List<GameDot> downwardMatches = FindMatches(x, y, new Vector2(0,-1), minDotsCountLenght);

        if (leftMatches == null) { leftMatches = new List<GameDot>(); }
        if (downwardMatches == null) { downwardMatches = new List<GameDot>(); }

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }

    //Get Random
    private GameObject GetRandomObject(GameObject[] objectArray)
    {
        GameObject obj = objectArray[Random.Range(0, objectArray.Length)];
        if (obj == null)
        {
            Debug.LogWarning("Found a dot prefab null");
            return null;
        }
        return obj;

    }
    private GameObject GetRandomDot()
    {
        return GetRandomObject(_dotPrefabs);
    }
    private GameObject GetRandomCollectible()
    {
        return GetRandomObject(_collectiblePrefabs);
    }

    public void PlaceDot(GameDot dot, int x, int y)
    {
        //Set the dot in the proper coordinates and assign it to the _dotBoard

        if (dot == null) 
        { 
            Debug.LogWarning("Found a dot prefab null");
            return;
        }
        dot.transform.position = new Vector3(x, y, 0);
        dot.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        {
            _dotBoard[x, y] = dot;
        }
        dot.SetCoordinates(x, y);
    }

    //Switch Tiles
    public void ClickTile(Tile tile)
    {
        if (_clickedTile == null)
        {
            _clickedTile = tile;
        }
    }
    public void DragToTile(Tile tile)
    {
        if (_clickedTile != null)
        {
            _targetTile = tile;
        }
    }
    public void ReleaseTile()
    {
        if (_clickedTile != null && _targetTile != null && IsNextTo())
        {
            SwitchTiles(_clickedTile, _targetTile);
        }
        _clickedTile = null;
        _targetTile = null;
    }
    private bool IsNextTo()
    {
        //Determine whether the clickedTile and the targetTile are adjacent
        if (Mathf.Abs(_targetTile.xCoord - _clickedTile.xCoord) == 1 && _targetTile.yCoord == _clickedTile.yCoord)
        {
            return true;
        }
        if (Mathf.Abs(_targetTile.yCoord - _clickedTile.yCoord) == 1 && _targetTile.xCoord == _clickedTile.xCoord)
        {
            return true;
        }
        return false;
        
    }
    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }
    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (GameManager.Instance.movesLeft <= 0)
        {
            _playerInputEnabled = false;
        }

        if (_playerInputEnabled)
        {
            GameDot clickedDot = _dotBoard[clickedTile.xCoord, clickedTile.yCoord];
            GameDot targetDot = _dotBoard[targetTile.xCoord, targetTile.yCoord];

            if (clickedDot != null && targetDot != null)
            {
                clickedDot.MoveDot(targetTile.xCoord, targetTile.yCoord, _swapTime);
                targetDot.MoveDot(clickedTile.xCoord, clickedTile.yCoord, _swapTime);

                yield return new WaitForSeconds(_swapTime);

                //Find Matches when dots are switched
                List<GameDot> clickedTileMatches = FindMatchesAt(clickedTile.xCoord, clickedTile.yCoord);
                List<GameDot> targetTileMatches = FindMatchesAt(targetTile.xCoord, targetTile.yCoord);
                List<GameDot> colorMatches = new List<GameDot>();

                //ColorBomb logic if one of the two (or the two) dots are color bombs
                if (IsColorBomb(clickedDot) && !IsColorBomb(targetDot))
                {
                    colorMatches = FindAllMatchValue(targetDot.matchValue);
                    if (targetDot is Bomb) { colorMatches = MakeDotBomb(colorMatches, GetBombPrefab(targetDot)); }
                    if (!colorMatches.Contains(targetDot)) colorMatches.Add(targetDot);
                    if (!colorMatches.Contains(clickedDot)) colorMatches.Add(clickedDot);
                }
                else if (!IsColorBomb(clickedDot) && IsColorBomb(targetDot))
                {
                    colorMatches = FindAllMatchValue(clickedDot.matchValue);
                    if (clickedDot is Bomb) { colorMatches = MakeDotBomb(colorMatches, GetBombPrefab(clickedDot)); }
                    if (!colorMatches.Contains(targetDot)) colorMatches.Add(targetDot);
                    if (!colorMatches.Contains(clickedDot)) colorMatches.Add(clickedDot);
                }
                else if (IsColorBomb(clickedDot) && IsColorBomb(targetDot))
                {
                    foreach(GameDot dot in _dotBoard)
                    {
                        if (!colorMatches.Contains(dot))
                        {
                            colorMatches.Add(dot);
                        }
                    }
                }

                //If not matches found move them back
                if (clickedTileMatches.Count == 0 && targetTileMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedDot.MoveDot(clickedTile.xCoord, clickedTile.yCoord, _swapTime);
                    targetDot.MoveDot(targetTile.xCoord, targetTile.yCoord, _swapTime);
                }
                else
                {
                    if (GameManager.Instance != null) 
                    { 
                        GameManager.Instance.movesLeft--;
                        GameManager.Instance.UpdateMovesLeft();
                    }

                    //If the matches for any (or both) of the sides creates a bomb, change the color to match the dot color
                    Vector2 swapDir = new Vector2(targetTile.xCoord - clickedTile.xCoord, targetTile.yCoord - clickedTile.yCoord);

                    _clickedTileBomb = DropBomb(clickedTile.xCoord, clickedTile.yCoord, swapDir, clickedTileMatches);
                    _targetTileBomb = DropBomb(targetTile.xCoord, targetTile.yCoord, swapDir, targetTileMatches);

                    if (_clickedTileBomb != null && targetDot != null)
                    {
                        GameDot clickedBombDot = _clickedTileBomb.GetComponent<GameDot>();
                        if (!IsColorBomb(clickedBombDot))
                        {
                            clickedBombDot.ChangeColor(targetDot);
                        }
                    }
                    if (_targetTileBomb != null && clickedDot != null)
                    {
                        GameDot targetBombDot = _targetTileBomb.GetComponent<GameDot>();
                        if (!IsColorBomb(targetBombDot))
                        {
                            targetBombDot.ChangeColor(clickedDot);
                        }
                    }

                    ClearAndRefillBoard(clickedTileMatches.Union(targetTileMatches).ToList().Union(colorMatches).ToList());
                }
            }
        }
    }

    //Make Bomb
    private GameDot MakeDotBomb(GameDot dot, GameObject bombPrefab)
    {
        GameObject bomb = null;
        int xCoord = dot.xCoord;
        int yCoord = dot.yCoord;
        MatchValue matchValue = dot.matchValue;

        if (bombPrefab != null)
        {
            ClearDotAt(xCoord, yCoord);
            bomb = CreateBomb(bombPrefab, xCoord, yCoord);
            bomb.GetComponent<Bomb>().matchValue = matchValue;
            ActivateBomb(bomb);
        }
        return bomb.GetComponent<GameDot>();
    }
    private List<GameDot> MakeDotBomb(List<GameDot> dots, GameObject bombPrefab)
    {
        List<GameDot> bombDots = new List<GameDot>();
        foreach (GameDot dot in dots)
        {
            bombDots.Add(MakeDotBomb(dot, bombPrefab));
        }
        return bombDots;
    }
    private GameObject GetBombPrefab(GameDot inputDot)
    {
        BombType bombType = inputDot.GetComponent<Bomb>().bombType;
        GameObject prefab = null;

        switch (bombType)
        {
            case BombType.Adjacent:
                prefab = _adjacentBombPrefab;
                break;
            case BombType.Column:
                prefab = _columnBombPrefab;
                break;
            case BombType.Row:
                prefab = _rowBombPrefab;
                break;
        }
        return prefab;
    }

    private bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //Find matches
    private List<GameDot> FindMatchesAt(int x, int y, int minDotsCountLenght = 3)
    {
        //Find all the matches in all directions of the given dot coordinates
        List<GameDot> horizontalMatches = FindHorizontalMatches(x, y, minDotsCountLenght);
        List<GameDot> verticalMatches = FindVerticalMatches(x, y, minDotsCountLenght);

        if (horizontalMatches == null) { horizontalMatches = new List<GameDot>(); }
        if (verticalMatches == null) { verticalMatches = new List<GameDot>(); }

        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }
    private List<GameDot> FindMatchesAt(List<GameDot> dots, int minDotsCountLenght = 3)
    {
        //Overload method of FindMatchesAt to pass a list as an argument
        List<GameDot> combinedMatches = new List<GameDot>();
        foreach (GameDot dot in dots)
        {
            combinedMatches = combinedMatches.Union(FindMatchesAt(dot.xCoord, dot.yCoord, minDotsCountLenght)).ToList();
        }
        return combinedMatches;
    }
    private List<GameDot> FindVerticalMatches(int startX, int startY, int minDotsCountLenght = 3)
    {
        List<GameDot> upwardMatches = FindMatches(startX, startY, new Vector2(0,1), 2);
        List<GameDot> downwardMatches = FindMatches(startX, startY, new Vector2(0,-1), 2);

        if (upwardMatches == null) { upwardMatches = new List<GameDot>(); }
        if (downwardMatches == null) { downwardMatches = new List<GameDot>(); }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return (combinedMatches.Count >= minDotsCountLenght) ? combinedMatches : null;
    }
    private List<GameDot> FindHorizontalMatches(int startX, int startY, int minDotsCountLenght = 3)
    {
        List<GameDot> rightMatches = FindMatches(startX, startY, new Vector2(1,0), 2);
        List<GameDot> leftMatches = FindMatches(startX, startY, new Vector2(-1,0), 2);

        if (rightMatches == null) { rightMatches = new List<GameDot>(); }
        if (leftMatches == null) { leftMatches = new List<GameDot>(); }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        return (combinedMatches.Count >= minDotsCountLenght) ? combinedMatches : null;
    }
    private List<GameDot> FindMatches(int startX, int startY, Vector2 searchDirection, int minDotsCountLenght = 3)
    {
        //Given a position on the board, return the matches of the searchDirection specified
        //if the matches are more or greater than the minDotsCountLenght

        List<GameDot> matches = new List<GameDot>();
        GameDot startDot = null;

        if (IsWithinBounds(startX, startY))
        {
            startDot = _dotBoard[startX, startY];
        }
        if (startDot != null) { matches.Add(startDot); }
        else { return null; }

        int nextX;
        int nextY;
        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int) Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int) Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if(!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GameDot nextDot = _dotBoard[nextX, nextY];
            if (nextDot == null) { break; }
            else
            {
                if (nextDot.matchValue == startDot.matchValue && !matches.Contains(nextDot) && nextDot.matchValue != MatchValue.None)
                {
                    matches.Add(nextDot);
                }
                else { break; }
            }
        }
        if(matches.Count >= minDotsCountLenght) { return matches; }
        return null;
    }
    private List<GameDot> FindAllMatches()
    {
        //Find all the matches for when the dots collapsed after a move
        List<GameDot> combinedMatches = new List<GameDot>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                combinedMatches = combinedMatches.Union(FindMatchesAt(x, y)).ToList();
            }
        }
        return combinedMatches;
    }
    private List<GameDot> FindAllMatchValue(MatchValue mValue)
    {
        List<GameDot> foundDots = new List<GameDot>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (_dotBoard[i, j] != null && _dotBoard[i, j].matchValue == mValue)
                {
                    foundDots.Add(_dotBoard[i, j]);
                }
            }
        }
        return foundDots;
    }

    //Clear dot
    private void ClearDotAt(int x, int y)
    {
        GameDot dotToClear = _dotBoard[x, y];
        if (dotToClear != null)
        {
            _dotBoard[x, y] = null;
            Destroy(dotToClear.gameObject);
        }
    }
    private void ClearDotAt(List<GameDot> dotList, List<GameDot> bombedDots)
    {
        foreach (GameDot dot in dotList)
        {
            if (dot != null)
            {
                ClearDotAt(dot.xCoord, dot.yCoord);

                int bonus = 0;
                if (dotList.Count >= 4) { bonus = 20; }

                dot.ScorePoints(_scoreMultiplier, bonus);

                if (_particleManager != null)
                {
                    if (bombedDots.Contains(dot))
                    {
                        _particleManager.BombFXAt(dot.xCoord, dot.yCoord);
                    }
                    else 
                    {
                        _particleManager.ClearDotFXAt(dot.xCoord, dot.yCoord);
                    }
                } 
            }
        }
    }

    //Break Tile
    private void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = _tileBoard[x, y];

        if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            if (_particleManager != null)
            {
                _particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y);
            }
            tileToBreak.BreakTile();
        }
    }
    private void BreakTileAt(List<GameDot> dots)
    {
        foreach (GameDot dot in dots)
        {
            if (dot != null)
            {
                BreakTileAt(dot.xCoord, dot.yCoord);
            }
        }
    }

    //Collapse columns
    private List<GameDot> CollapseColumn(int column, float collapseTime = 0.2f)
    {
        List<GameDot> movingDots = new List<GameDot>();
        for (int i = 0; i < height - 1; i++)
        {
            if (_dotBoard[column, i] == null && _tileBoard[column, i].tileType != TileType.Obstacle)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (_dotBoard[column, j] != null)
                    {
                        _dotBoard[column, j].MoveDot(column, i, collapseTime * (j - i));
                        _dotBoard[column, i] = _dotBoard[column, j];
                        _dotBoard[column, i].SetCoordinates(column, i);

                        if (!movingDots.Contains(_dotBoard[column, i]))
                        {
                            movingDots.Add(_dotBoard[column, i]);
                        }
                        _dotBoard[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingDots;
    }
    private List<GameDot> CollapseColumn(List<GameDot> dots)
    {
        List<GameDot> movingDots = new List<GameDot>();
        List<int> columnsToCollapse = GetColumns(dots);

        foreach (int column in columnsToCollapse)
        {
            movingDots = movingDots.Union(CollapseColumn(column)).ToList();
        }
        return movingDots;
    }
    private List<int> GetColumns (List<GameDot> dots)
    {
        List<int> columns = new List<int>();
        foreach (GameDot dot in dots)
        {
            if (!columns.Contains(dot.xCoord))
            {
                columns.Add(dot.xCoord);
            }
        }
        return columns;
    }

    //Clearing logic
    private void ClearAndRefillBoard(List<GameDot> dots)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(dots));
    }
    IEnumerator ClearAndRefillBoardRoutine(List<GameDot> dots)
    {
        _playerInputEnabled = false;
        List<GameDot> matches = dots;

        _scoreMultiplier = 0;

        do
        {
            _scoreMultiplier++;
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;

            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();

            yield return new WaitForSeconds(0.15f);
        }
        while (matches.Count != 0);

        _playerInputEnabled = true;
    }
    IEnumerator ClearAndCollapseRoutine(List<GameDot> dots)
    {
        GameManager.Instance.dotsCollapsed = false;

        List<GameDot> movingDots = new List<GameDot>();
        List<GameDot> matches = new List<GameDot>();

        yield return new WaitForSeconds(0.1f);

        bool isFinished = false;
        while (!isFinished)
        {
            List<GameDot> bombedDots = GetBombedDots(dots);
            dots = dots.Union(bombedDots).ToList();

            bombedDots = GetBombedDots(dots);
            dots = dots.Union(bombedDots).ToList();

            List<GameDot> collectedDots = FindCollectibleAt(0);
            _collectibleCount -= collectedDots.Count;
            dots = dots.Union(collectedDots).ToList();

            ClearDotAt(dots, bombedDots);
            BreakTileAt(dots);

            if (_clickedTileBomb != null)
            {
                ActivateBomb(_clickedTileBomb);
                _clickedTileBomb = null;
            }
            if (_targetTileBomb != null)
            {
                ActivateBomb(_targetTileBomb);
                _targetTileBomb = null;
            }

            yield return new WaitForSeconds(0.15f);

            movingDots = CollapseColumn(dots);

            while (!IsCollapsed(movingDots))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.15f);

            matches = FindMatchesAt(movingDots);
            //DropNewBombs(matches);

            collectedDots = FindCollectibleAt(0);
            matches = matches.Union(collectedDots).ToList();

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                _scoreMultiplier++;
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }

        GameManager.Instance.dotsCollapsed = true;
        yield return null;
    }
    IEnumerator RefillRoutine()
    {
        FillRandom(falseYOffset, moveTime);
        yield return null;
    }
    private bool IsCollapsed(List<GameDot> dots)
    {
        foreach (GameDot dot in dots)
        {
            if (dot != null)
            {
                if (dot.transform.position.y - (float)dot.yCoord > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    //Get bombed dots
    private List<GameDot> GetRowDots(int row)
    {
        List<GameDot> dots = new List<GameDot>();
        for (int i = 0; i < width; i++)
        {
            if (_dotBoard[i, row] != null)
            {
                dots.Add(_dotBoard[i, row]);
            }
        }
        return dots;
    }
    private List<GameDot> GetColumnDots(int column)
    {
        List<GameDot> dots = new List<GameDot>();
        for (int i = 0; i < height; i++)
        {
            if (_dotBoard[column, i] != null)
            {
                dots.Add(_dotBoard[column, i]);
            }
        }
        return dots;
    }
    private List<GameDot> GetAdjacentDots(int x, int y, int offset = 1)
    {
        List<GameDot> dots = new List<GameDot>();

        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if (IsWithinBounds(i, j))
                {
                    dots.Add(_dotBoard[i, j]);
                }
            }
        }
        return dots;
    }

    //Bombs
    private List<GameDot> GetBombedDots(List<GameDot> dots)
    {
        List<GameDot> allDotsToClear = new List<GameDot>();

        foreach (GameDot dot in dots)
        {
            if (dot != null)
            {
                List<GameDot> dotsToClear = new List<GameDot>();
                Bomb bomb = dot.GetComponent<Bomb>();
                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Row:
                            dotsToClear = GetRowDots(bomb.yCoord); 
                            break;

                        case BombType.Column:
                            dotsToClear = GetColumnDots(bomb.xCoord);
                            break;

                        case BombType.Adjacent:
                            dotsToClear = GetAdjacentDots(bomb.xCoord, bomb.yCoord);
                            break;

                        case BombType.Color:
                            break;
                    }
                }
                allDotsToClear = allDotsToClear.Union(dotsToClear).ToList();
                allDotsToClear = RemoveCollectibles(allDotsToClear);
            }
        }
        return allDotsToClear;
    }
    private bool IsCornerMatch(List<GameDot> dots)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (GameDot dot in dots)
        {
            if (dot != null)
            {
                if (xStart == -1 || yStart == -1)
                {
                    xStart = dot.xCoord;
                    yStart = dot.yCoord;
                    continue;
                }
                if (dot.xCoord != xStart && dot.yCoord == yStart)
                {
                    horizontal = true;
                }
                if (dot.xCoord == xStart && dot.yCoord != yStart)
                {
                    vertical = true;
                }
            }
        }
        return (horizontal && vertical);
    }
    private GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GameDot> dots)
    {
        //Create different bombs depending on the type of match

        GameObject bomb = null;
        if (dots.Count >= 4)
        {
            if (IsCornerMatch(dots))
            {
                if (_adjacentBombPrefab != null)
                {
                    bomb = CreateBomb(_adjacentBombPrefab, x, y);
                }
            }
            else
            {
                if (dots.Count >= 5)
                {
                    if (_colorBombPrefab != null)
                    {
                        bomb = CreateBomb(_colorBombPrefab, x, y);
                    }
                }
                else
                {
                    //If the swap was horizontal it means the match of 4 was vertical
                    if (swapDirection.x != 0)
                    {
                        if (_adjacentBombPrefab != null)
                        {
                            bomb = CreateBomb(_rowBombPrefab, x, y);
                        }
                    }
                    else
                    {
                        if (_adjacentBombPrefab != null)
                        {
                            bomb = CreateBomb(_columnBombPrefab, x, y);
                        }
                    }
                }
            }
        }
        return bomb;
    }
    private void ActivateBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if (IsWithinBounds(x, y))
        {
            _dotBoard[x, y] = bomb.GetComponent<GameDot>();
        }
    }
    private bool IsColorBomb(GameDot dot)
    {
        Bomb bomb = dot.GetComponent<Bomb>();

        if (bomb == null) return false;
        return (bomb.bombType == BombType.Color);
    }
            //TODO
    private void DropNewBombs(List<GameDot> matches)
    {
        if (matches != null)
        {
            List<GameDot> newBombs = new List<GameDot>(matches);
            List<GameDot> currentDotMatches = new List<GameDot>();

            foreach (GameDot dot in newBombs)
            {
                currentDotMatches = FindMatchesAt(dot.xCoord, dot.yCoord, 4);
                if (currentDotMatches != null)
                {
                    GameDot bomb = MakeDotBomb(dot, _rowBombPrefab);
                    if (bomb != null)
                    {
                        if (bomb.GetComponent<Bomb>().bombType != BombType.Color)
                        {
                            bomb.GetComponent<Bomb>().ChangeColor(dot);
                        }
                    }
                }
                newBombs = newBombs.Except(currentDotMatches).ToList();
            }
        }
    }

    //Collectibles
    private List<GameDot> FindCollectibleAt(int row)
    {
        List<GameDot> foundCollectibles = new List<GameDot>();
        for (int i = 0; i < width; i++)
        {
            if (_dotBoard[i, row] != null)
            {
                Collectible collectible = _dotBoard[i, row].GetComponent<Collectible>();
                if (collectible != null)
                {
                    foundCollectibles.Add(_dotBoard[i, row]);
                }
            }
        }
        return foundCollectibles;
    }
    private List<GameDot> FindAllCollectibles()
    {
        List<GameDot> foundCollectibles = new List<GameDot>();
        for (int i = 0; i < height; i++)
        {
            List<GameDot> collectibleRow = FindCollectibleAt(i);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }
        return foundCollectibles;
    }
    private bool CanAddCollectible()
    {
        return (Random.Range(0f, 1f) <= _chanceForCollectible) && _collectiblePrefabs.Length > 0 &&
            _collectibleCount < _maxCollectibles;
    }
    private List<GameDot> RemoveCollectibles(List<GameDot> bombedDots)
    {
        List<GameDot> collectibleDots = FindAllCollectibles();
        List<GameDot> dotsToRemove = new List<GameDot>();

        foreach (GameDot dot in collectibleDots)
        {
            Collectible collectibleComponent = dot.GetComponent<Collectible>();
            if (collectibleComponent != null)
            {
                if (!collectibleComponent.clearedByBomb)
                {
                    dotsToRemove.Add(dot);
                }
            }
        }
        return bombedDots.Except(dotsToRemove).ToList();
    }
}
