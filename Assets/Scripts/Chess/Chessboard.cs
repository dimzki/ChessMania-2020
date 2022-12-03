using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum SpecialMoves
{
    none = 0,
    EnPassant,
    Castling,
    Promotion
}

public class Chessboard : MonoBehaviour
{
    [Header("Data")]
    [ShowOnly] public Team _currentTurn;
    
    [FormerlySerializedAs("manager")]
    [Space(10)][Header("Scripts")]
    [SerializeField] private Manager _manager;                      // main Manager script
    [SerializeField] private WinningPanel _winningPanel;            // game-over panel script
    [SerializeField] private PopupNotification _popupNotification;  // notification popup script;

    [Space(10)][Header("Prefabs & Materials")] 
    [SerializeField] private GameObject[] _prefabs;                 // Prefabs of chess piece: pawn, rook, bishop, knight, queen, king
    [SerializeField] private Material[] _teamMaterials;             // Materials for black and white piece
    [SerializeField] private Material _tileMaterial;                // Material for default tile
    [SerializeField] private Material _hoverMaterial;               // Material for hovered tile
    [SerializeField] private Material _highlightMaterial;           // Material for highlighted tile
    
    [FormerlySerializedAs("tileSize")]
    [Space(10)][Header("Board Setting")] 
    [SerializeField] private float _tileSize = 1f;                  // Size of individual tile 
    [SerializeField] private float _yOffset = 1f;                   // The Y (height) offset for the tile spawned
    [SerializeField] private Vector3 _boardCenter = Vector3.zero;   // The board pivot, in case the board is not pivoted at the 0,0,0
    [SerializeField] private float _deathPieceSize = 0.8f;          // The scale of dead pieces
    [SerializeField] private float _deathSpacing = 0.5f;            // The spacing between dead pieces
    [SerializeField] private float _dragOffset = 1f;                // The Y (Height) offset when dragging a piece around 

    [Space(10)] [Header("External Components")] 
    [SerializeField] private TextMeshProUGUI _tmpCurrentTurn;       // Text Mesh Pro UGUI for current turn
    [SerializeField] private TextMeshProUGUI _tmpSpecialPopup;      // Text Mesh Pro UGUI for special moves popup

    [Space(10)] [Header("GameObjects")] 
    [SerializeField] private GameObject _objSkipButton;
    
    [Space(10)] [Header("Transforms")] 
    [SerializeField] private Transform _tPoolPawn;  
    [SerializeField] private Transform _tPoolKnight;  
    [SerializeField] private Transform _tPoolBishop;  
    [SerializeField] private Transform _tPoolRook;  
    [SerializeField] private Transform _tPoolQueen;  
    [SerializeField] private Transform _tPoolKing;  
    
    [SerializeField] private Transform _activeChessPieces;  
    
    // Private Variables
    private Chesspiece[,] _chessPieces;                             // Array of chess pieces on the board
    private Chesspiece _currentDragging;                            // Chess piece currently being dragged
    private List<Vector2Int> availableMoves = new List<Vector2Int>();               // List of available moves for the dragged chess piece

    private List<Chesspiece> _deadWhites = new List<Chesspiece>();                   // List of dead white pieces
    private List<Chesspiece> _deadBlacks = new List<Chesspiece>();                   // List of dead black pieces
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    private SpecialMoves _specialMoves;

    private const int TileCountX = 8;                               // Tile count of X
    private const int TileCountY = 8;                               // Tile count of Y
    private GameObject[,] _tiles;                                   // array of Gameobject tiles, for searching purposes
    private MeshRenderer[,] _materials;                             // array of Mesh Renderer, for searching purposes
    private Camera _camera;                                         // Main camera, to look for mouse position
    private Vector2Int _currentHover = -Vector2Int.one;             // Vector 2 of current "hovered" tile
    private Vector3 _bounds;                                        // Chessboard bounds
    
    private bool whiteCheked;
    private bool blackCheked;

    private Chesspiece _pawnToPromote;

    [Space(10)] [Header("Unity Event")] 
    public UnityEvent OnGameStart;                                  // Unity event called when game started
    public UnityEvent OnGameEnd;                                    // Unity event called when game ended
    public UnityEvent OnSpecialMove;                                // Unity event called when a special move has been pulled
    public UnityEvent OnPromotion;                                  // Unity event called when a pawn is being promoted

    [Space(10)] [Header("Promotion Setup")]
    public MeshRenderer[] promotionPrefabs;

    #if UNITY_EDITOR
    // on validate only works in editor, thus the tag if unity_editor
    private void OnValidate()
    {
        _camera = Camera.main;
    }
    #endif
    
    private void Awake()
    {
        // generate chessboard at the start of the game
        GenerateChessBoard(_tileSize, TileCountX, TileCountY);
        
        // if on unity editor's validate failed to get main camera
        if (!_camera) _camera = Camera.main;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Called When Starting a New Game
    
    public void InitializeGame()
    {
        SpawnAllPieces();
        PositionAllPieces();
        
        //SetFirstTurn();
        StartAi();
    }

    public void SetFirstTurn(int mode)
    {
        switch (mode)
        {
            case 0:
                _currentTurn = Team.White;
                break;
            case 1:
                _currentTurn = Team.Black;
                break;
            case 2:
                int value = Random.Range(1, 101);
                _currentTurn = value >= 50 ? Team.White : Team.Black;
                break;
            default:
                Debug.LogError("No Team Selected!");
                break;
        }
        
        _tmpCurrentTurn.text = _currentTurn + "'s Turn";
        
        OnGameStart?.Invoke();
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        
        // check if the mouse is hovering over a certain tile and there is no UI element obstructing the mouse
        if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")) && !MouseOverUI.IsPointerOverUIElement())
        {
            // Get tile index
            Vector2Int hitPosition = LookupTileIndex(hit.transform.gameObject);

            // Hovering tile for the first time
            if (_currentHover == -Vector2Int.one)
            {
                //manager.PlaySFX(manager.RepositoryAudioFiles.mouseHover, 0.05f);
                
                _currentHover = hitPosition;
                _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                ChangeMaterial(_materials[hitPosition.x, hitPosition.y], _hoverMaterial);
            }
            
            // Hovering tile
            if (_currentHover != hitPosition)
            {
                //manager.PlaySFX(manager.RepositoryAudioFiles.mouseHover, 0.05f);
                
                // revert previous hover layer back to tile
                bool containtValidMove = ContainValidMove(ref availableMoves, _currentHover);
                _tiles[_currentHover.x, _currentHover.y].layer = containtValidMove ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                switch (containtValidMove)
                {
                    case true:
                        ChangeMaterial(_materials[_currentHover.x, _currentHover.y], _highlightMaterial);
                        break;
                    
                    case false:
                        ChangeMaterial(_materials[_currentHover.x, _currentHover.y], _tileMaterial);
                        break;
                }
                
                _currentHover = hitPosition;
                _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                ChangeMaterial(_materials[hitPosition.x, hitPosition.y], _hoverMaterial);
            }

            // If Mouse Button DOWN
            if (Input.GetMouseButtonDown(0))
            {
                if (_chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    Chesspiece chesspiece = _chessPieces[hitPosition.x, hitPosition.y];
                    
                    // check if player vs AI
                    if (_manager.gameMode == GameMode.vsAi && _currentTurn != _manager.playerTeam) return;

                    if (_currentTurn == _chessPieces[hitPosition.x, hitPosition.y].team)
                    {
                        _currentDragging = chesspiece;
                            //_chessPieces[hitPosition.x, hitPosition.y];

                        // Get list of tiles that the piece can move to
                        availableMoves = _currentDragging.GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);

                        // Get a List of Special Moves
                        _specialMoves = _currentDragging.GetSpecialMoves(ref _chessPieces, ref moveList, ref availableMoves);
                        
                        /*// check for check moves
                        if (chesspiece.type == ChessPieceType.King)
                        {
                            List<Vector2Int> enemyMovements = new ();

                            foreach (Chesspiece piece in _chessPieces)
                            {
                                if (piece == null) continue;
                                if (piece.team == chesspiece.team) continue;
                                List<Vector2Int> temp = piece.GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
                                        
                                enemyMovements.AddRange(temp);
                            }
                            
                            foreach (var location in enemyMovements.Where(location => availableMoves.Contains(location)))
                            {
                                availableMoves.Remove(location);
                            }
                        }*/

                        PreventCheck();
                        HighLightTiles();
                    }
                }
            }
            
            // If Mouse Button UP
            if (_currentDragging && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(_currentDragging.currentX, _currentDragging.currentY);

                if (availableMoves.Contains(new Vector2Int(hitPosition.x, hitPosition.y)))
                {
                    bool validMove = MoveTo(_currentDragging, hitPosition.x, hitPosition.y);
                
                    if (!validMove) _currentDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                }
                else
                {
                    /*bool validMove = MoveTo(_currentDragging, hitPosition.x, hitPosition.y);
                
                    if (!validMove)*/ 
                    
                    _currentDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                }

                
                _currentDragging = null;
                if (availableMoves.Count > 0) RemoveHighLightTiles();
            }
        }
        
        // if the mouse is not hovering over any tile, reset the previous "hover tile"
        else if (!MouseOverUI.IsPointerOverUIElement())
        {
            if (_currentHover != -Vector2Int.one)
            {
                // revert previous hover layer back to tile
                bool containValidMove = ContainValidMove(ref availableMoves, _currentHover);
                _tiles[_currentHover.x, _currentHover.y].layer = containValidMove ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                
                switch (containValidMove)
                {
                    case true:
                        ChangeMaterial(_materials[_currentHover.x, _currentHover.y], _highlightMaterial);
                        break;
                    
                    case false:
                        ChangeMaterial(_materials[_currentHover.x, _currentHover.y], _tileMaterial);
                        break;
                }
                
                ChangeMaterial(_materials[_currentHover.x, _currentHover.y], _tileMaterial);

                // set current hover to -1, -1
                _currentHover = -Vector2Int.one;
            }

            // If dragging a piece and let up, reset
            if (_currentDragging && Input.GetMouseButtonUp(0))
            {
                _currentDragging.SetPosition(GetTileCenter(_currentDragging.currentX, _currentDragging.currentY));
                _currentDragging = null;
                if (availableMoves.Count > 0) RemoveHighLightTiles();
            }

        }
        
        // If mouse currently dragging a piece, simulate the drag
        if (_currentDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * _yOffset);
            float distance = 0f;
            if (horizontalPlane.Raycast(ray, out distance))
                _currentDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * _dragOffset);
        }
    }
    

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Generate The Chess Board
    
    private void GenerateChessBoard(float tileSize, int tileCountX, int tileCountY)
    {
        _yOffset += transform.position.y;
        _bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + _boardCenter;
        
        _tiles = new GameObject[tileCountX, tileCountY];
        _materials = new MeshRenderer[tileCountX, tileCountY];
        
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                _tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        // spawn gameobject tile with it's x and y coordinate
        GameObject tileObject = new GameObject($"X:{x}, Y:{y}");
        tileObject.transform.parent = transform;
        
        // add mesh renderer and set its material
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer meshRenderer = tileObject.AddComponent<MeshRenderer>();
        meshRenderer.material = _tileMaterial;
        
        // designate the triangular points
        Vector3[] vert = new Vector3[4];
        vert[0] = new Vector3(x * tileSize, _yOffset, y * tileSize) - _bounds;
        vert[1] = new Vector3(x * tileSize, _yOffset, (y + 1) * tileSize) - _bounds;
        vert[2] = new Vector3((x + 1) * tileSize, _yOffset, y * tileSize) - _bounds;
        vert[3] = new Vector3((x + 1) * tileSize, _yOffset, (y + 1) * tileSize) - _bounds;

        // set the tris every 3 points, 0-1-2 and 1-3-2 to create a plane
        int[] tris = { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vert;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        // set the layer to default "Tile" layer and add collider
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        // register the materials for future usage (to switch between default and hover material)
        _materials[x, y] = meshRenderer;
        
        return tileObject;
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Spawn Chess pieces
    
    private void SpawnAllPieces()
    {
        _chessPieces = new Chesspiece[TileCountX, TileCountY];

        // white team
        _chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, Team.White);
        _chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, Team.White);
        _chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, Team.White);
        _chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, Team.White);
        _chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, Team.White);
        _chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, Team.White);
        _chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, Team.White);
        _chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, Team.White);
        for (int i = 0; i < TileCountX; i++)
            _chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, Team.White);
        
        // black team
        _chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, Team.Black);
        _chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, Team.Black);
        _chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, Team.Black);
        _chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, Team.Black);
        _chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, Team.Black);
        _chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, Team.Black);
        _chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, Team.Black);
        _chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, Team.Black);
        for (int i = 0; i < TileCountX; i++)
            _chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, Team.Black);
        
        // Debug
        /*_chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.King, Team.Black);
        _chessPieces[4, 6] = SpawnSinglePiece(ChessPieceType.Bishop, Team.Black);
        _chessPieces[2, 5] = SpawnSinglePiece(ChessPieceType.Rook, Team.Black);
        
        _chessPieces[0, 5] = SpawnSinglePiece(ChessPieceType.Pawn, Team.White);
        _chessPieces[1, 6] = SpawnSinglePiece(ChessPieceType.Pawn, Team.White);
        _chessPieces[3, 3] = SpawnSinglePiece(ChessPieceType.King, Team.White);*/
    }

    private Chesspiece SpawnSinglePiece(ChessPieceType type, Team team)
    {
        // Check for object pooling
        Chesspiece cp = null;
        switch (type)
        {
            case ChessPieceType.Pawn:
                if (_tPoolPawn.childCount > 0)
                {
                    cp = _tPoolPawn.GetChild(0).GetComponent<Chesspiece>();
                    cp.transform.SetParent(_activeChessPieces);
                }
                break;

            case ChessPieceType.Rook:
                if (_tPoolRook.childCount > 0)
                {
                    cp = _tPoolRook.GetChild(0).GetComponent<Chesspiece>();
                    cp.transform.SetParent(_activeChessPieces);
                }
                break;
            
            case ChessPieceType.Knight:
                if (_tPoolKnight.childCount > 0)
                {
                    cp = _tPoolKnight.GetChild(0).GetComponent<Chesspiece>();
                    cp.transform.SetParent(_activeChessPieces);
                }
                break;
            
            case ChessPieceType.Bishop:
                if (_tPoolBishop.childCount > 0)
                {
                    cp = _tPoolBishop.GetChild(0).GetComponent<Chesspiece>();
                    cp.transform.SetParent(_activeChessPieces);
                }
                break;
            
            case ChessPieceType.Queen:
                if (_tPoolQueen.childCount > 0)
                {
                    cp = _tPoolQueen.GetChild(0).GetComponent<Chesspiece>();
                    cp.transform.SetParent(_activeChessPieces);
                }
                break;
            
            case ChessPieceType.King:
                if (_tPoolKing.childCount > 0)
                {
                    cp = _tPoolKing.GetChild(0).GetComponent<Chesspiece>();
                    cp.transform.SetParent(_activeChessPieces);
                }
                break;
        }
        
        if (cp == null)
            cp = Instantiate(_prefabs[(int)type - 1], _activeChessPieces).GetComponent<Chesspiece>();

        cp.type = type;
        cp.team = team;
        
        switch (team)
        {
            case Team.White:
                cp.GetComponent<MeshRenderer>().material = _teamMaterials[0];
                break;
            
            case Team.Black:
                cp.GetComponent<MeshRenderer>().material = _teamMaterials[1];
                
                // rotate the black pieces
                cp.transform.eulerAngles = new Vector3(0, 180, 0);
                break;
        }

        // for editor only, rename GameObject
        #if UNITY_EDITOR
        cp.name = team + " " + type;
        #endif
        
        cp.Initialize(TileCountX, TileCountY);
        
        return cp;
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Positioning
    
    private void PositionAllPieces()
    {
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                if (_chessPieces[x,y] != null)
                    PositionSinglePiece(x, y, true);
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        _chessPieces[x, y].currentX = x;
        _chessPieces[x, y].currentY = y;
        _chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * _tileSize, _yOffset, y * _tileSize) - _bounds + new Vector3(_tileSize / 2, 0, _tileSize / 2);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Highlight Tiles
    
    private void HighLightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            _tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
            ChangeMaterial(_materials[availableMoves[i].x, availableMoves[i].y], _highlightMaterial);
        }
    }
    
    private void RemoveHighLightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            ChangeMaterial(_materials[availableMoves[i].x, availableMoves[i].y], _tileMaterial);
            _tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        
        availableMoves.Clear();
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Operations

    private bool ContainValidMove(ref List<Vector2Int> moves, Vector2Int position)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == position.x && moves[i].y == position.y)
                return true;

        return false;
    }

    private bool MoveTo(Chesspiece currentPiece, int x, int y)
    {
        Debug.Log("Move " + currentPiece.name + " to (" + x + ", " + y + ")");
        
        if (_manager.gameMode == GameMode.vsPlayer && !ContainValidMove(ref availableMoves, new Vector2Int(x, y))) 
            return false;
        
        Vector2Int previousPosition = new Vector2Int(currentPiece.currentX, currentPiece.currentY);
        currentPiece.previousPosition = previousPosition;
        
        // Check if there is another piece on the target position
        if (_chessPieces[x, y])
        {
            Chesspiece targetPiece = _chessPieces[x, y];
            if (targetPiece.team == currentPiece.team)
                return false;

            switch (targetPiece.team)
            {
                case Team.White:
                    _deadWhites.Add(targetPiece);
                    targetPiece.SetPosition(
                        new Vector3(8 * _tileSize - 0.1f, _yOffset, -1 * _tileSize + 0.2f)      // Set death piece position
                        - _bounds                                                                  // Normalize position to the center (0,0) pivot
                        + new Vector3(_tileSize / 2, 0, _tileSize / 2)                       // The center of a tile
                        + Vector3.forward * (_deathSpacing * _deadWhites.Count));                 // Spacing between death pieces

                    if (targetPiece.type == ChessPieceType.King)
                    {
                        Checkmate(Team.Black);
                    }

                    break;
                
                case Team.Black:
                    _deadBlacks.Add(targetPiece);
                    targetPiece.SetPosition(
                        new Vector3(-1 * _tileSize + 0.1f, _yOffset, 8 * _tileSize - 0.2f)       // Set death piece position
                        - _bounds                                                                   // Normalize position to the center (0,0) pivot
                        + new Vector3(_tileSize / 2, 0, _tileSize / 2)                       // The center of a tile
                        + Vector3.back * (_deathSpacing * _deadBlacks.Count));                    // Spacing between death pieces

                    if (targetPiece.type == ChessPieceType.King)
                    {
                        Checkmate(Team.White);
                    }
                    break;
            }
            
            _chessPieces[targetPiece.currentX, targetPiece.currentY] = null;
            targetPiece.SetScale(Vector3.one * _deathPieceSize);
            
            // Play SFX
            _manager.PlaySFX(_manager.RepositoryAudioFiles.moveKill, 0.4f);
            
            // 
            _popupNotification.popUpMessage.Add("<size=30><color=" + TeamColor(currentPiece) + ">"+ currentPiece.team + " " + currentPiece.type + "</color>  <color=" + TeamColor(targetPiece) + ">" + targetPiece.team + " " + targetPiece.type + "</color></size>");
            _popupNotification.ShowNotification();
        }

        _chessPieces[x, y] = currentPiece;
        _chessPieces[previousPosition.x, previousPosition.y] = null;
        
        PositionSinglePiece(x, y);

        // Check For Checkmoves
        /*List<Vector2Int> nextMove = currentPiece.GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
        foreach (Vector2Int location in nextMove)
        {
            Chesspiece chesspiece = _chessPieces[location.x, location.y];
            if (chesspiece == null) continue;
            if (chesspiece.team == currentPiece.team) continue;
            if (chesspiece.type == ChessPieceType.King)
            {
                
            }
        }*/

        // Add move list
        moveList.Add(new Vector2Int[] {previousPosition, new Vector2Int(x, y)});
        
        // Check Special Move
        ProcessSpecialMove();
        
        if(IsCheckmate())
            Checkmate(currentPiece.team);
        
        return true;
    }
    
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                if (_tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

        Debug.LogError("ERROR IN FINDING TILE INDEX");
        return -Vector2Int.one; // invalid
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Special Move
    
    private void ProcessSpecialMove()
    {
        // En Passant
        if (_specialMoves == SpecialMoves.EnPassant)
        {
            Debug.LogWarning("En Passant!");
            // current pawn
            var newMove = moveList[moveList.Count - 1];
            Chesspiece pawn = _chessPieces[newMove[1].x, newMove[1].y];
            
            // target pawn
            var targetPawnPosition = moveList[moveList.Count - 2];
            Chesspiece enemyPawn = _chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];
            
            // Check if En-Passant is valid, if yes, kill the target pawn
            if (pawn.currentX == enemyPawn.currentX)
                if (Math.Abs(pawn.currentY - enemyPawn.currentY) == 1)
                {
                    switch (enemyPawn.team)
                    {
                        case Team.White:
                            _deadWhites.Add(enemyPawn);
                            enemyPawn.SetPosition(
                                new Vector3(8 * _tileSize - 0.1f, _yOffset, -1 * _tileSize + 0.2f)      // Set death piece position
                                - _bounds                                                                  // Normalize position to the center (0,0) pivot
                                + new Vector3(_tileSize / 2, 0, _tileSize / 2)                       // The center of a tile
                                + Vector3.forward * (_deathSpacing * _deadWhites.Count));                 // Spacing between death pieces
                            break;
                        case Team.Black:
                            _deadBlacks.Add(enemyPawn);
                            enemyPawn.SetPosition(
                                new Vector3(-1 * _tileSize + 0.1f, _yOffset, 8 * _tileSize - 0.2f)       // Set death piece position
                                - _bounds                                                                   // Normalize position to the center (0,0) pivot
                                + new Vector3(_tileSize / 2, 0, _tileSize / 2)                       // The center of a tile
                                + Vector3.back * (_deathSpacing * _deadBlacks.Count));                    // Spacing between death pieces
                            break;
                    }
                    _chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            
            // Play SFX
            _manager.PlaySFX(_manager.RepositoryAudioFiles.moveKill, 0.4f);

            // Pop-up notification
            _popupNotification.popUpMessage.Add("En Passant!");
            _popupNotification.popUpMessage.Add("<size=30><color=" + TeamColor(pawn) + ">"+ pawn.team + " " + pawn.type + "</color>  <color=" + TeamColor(enemyPawn) + ">" + enemyPawn.team + " " + enemyPawn.type + "</color></size>");
            _popupNotification.ShowNotification();
            EndTurn();
        }

        // Castling
        else if (_specialMoves == SpecialMoves.Castling)
        {
            Debug.LogWarning("Castling!");
            var lastMove = moveList[moveList.Count - 1];
            int y = lastMove[1].y;
            
            //left rook
            if (lastMove[1].x == 2 && (lastMove[1].y == 0 || lastMove[1].y == 7))
            {
                Chesspiece leftRook = _chessPieces[0, y];
                _chessPieces[3, y] = leftRook;
                PositionSinglePiece(3, y);

                _chessPieces[0, y] = null;
            }
            // right rook
            else if (lastMove[1].x == 6)
            {
                Chesspiece rightRook = _chessPieces[7, y];
                _chessPieces[5, y] = rightRook;
                PositionSinglePiece(5, y);

                _chessPieces[7, y] = null;
            }

            // Play SFX
            _manager.PlaySFX(_manager.RepositoryAudioFiles.castling, 0.6f);

            // Pop-up notification
            _popupNotification.popUpMessage.Add("Castling");
            _popupNotification.ShowNotification();
            
            EndTurn();
        }

        // Promotion
        else if (_specialMoves == SpecialMoves.Promotion)
        {
            Debug.LogWarning("Promotion!");
            
            // current pawn
            var newMove = moveList[moveList.Count - 1];
            Chesspiece pawn = _chessPieces[newMove[1].x, newMove[1].y];

            switch (pawn.team)
            {
                case Team.White:
                    if (pawn.currentY == 7)
                    {
                        // vs AI
                        if (_manager.gameMode == GameMode.vsAi)
                            if (_currentTurn == _manager.playerTeam)
                            {
                                OnPromotion?.Invoke();
                                _pawnToPromote = pawn;
                                foreach (MeshRenderer meshRenderer in promotionPrefabs)
                                {
                                    ChangeMaterial(meshRenderer, _teamMaterials[0]);
                                }
                            }
                            // AI's promotion
                            else
                            {
                                _pawnToPromote = pawn;
                                Promotion(0);
                            }
                        // player vs player
                        else
                        {
                            OnPromotion?.Invoke();
                            _pawnToPromote = pawn;
                            foreach (MeshRenderer meshRenderer in promotionPrefabs)
                            {
                                ChangeMaterial(meshRenderer, _teamMaterials[0]);
                            }  
                        }
                    }
                    else
                        EndTurn();
                    break;
                
                case Team.Black:
                    if (pawn.currentY == 0)
                    {
                        // vs AI
                        if (_manager.gameMode == GameMode.vsAi)
                            if (_currentTurn == _manager.playerTeam)
                            {
                                OnPromotion?.Invoke();
                                _pawnToPromote = pawn; 
                                foreach (MeshRenderer meshRenderer in promotionPrefabs)
                                {
                                    ChangeMaterial(meshRenderer, _teamMaterials[1]);
                                }
                            }
                            // AI's promotion
                            else
                            {
                                _pawnToPromote = pawn;
                                Promotion(0);
                            }
                        // player vs player
                        else
                        {
                            OnPromotion?.Invoke();
                            _pawnToPromote = pawn; 
                            foreach (MeshRenderer meshRenderer in promotionPrefabs)
                            {
                                ChangeMaterial(meshRenderer, _teamMaterials[1]);
                            }
                        }
                    }
                    else
                        EndTurn();
                    break;
            }
            
            _manager.PlaySFX(_manager.RepositoryAudioFiles.move);
        }

        // No Special Move
        else
        {
            _manager.PlaySFX(_manager.RepositoryAudioFiles.move);
            EndTurn();
        }
    }

    public void Promotion(int index)
    {
        Chesspiece newClass = _pawnToPromote;
        
        switch (index)
        {
            case 0:
                newClass.type = ChessPieceType.Queen;
                break;
            case 1:
                newClass.type = ChessPieceType.Knight;
                break;
            case 2:
                newClass.type = ChessPieceType.Rook;
                break;
            case 3:
                newClass.type = ChessPieceType.Bishop;
                break;
        }
        
        //DestroyImmediate(_pawnToPromote.gameObject);
        _pawnToPromote.transform.SetParent(_tPoolPawn);
        
        _chessPieces[newClass.currentX, newClass.currentY] = SpawnSinglePiece(newClass.type, newClass.team);
        PositionSinglePiece(_pawnToPromote.currentX, _pawnToPromote.currentY);
        
        // Pop-up notification
        _popupNotification.popUpMessage.Add("Promotion!");
        _popupNotification.popUpMessage.Add("<size=30><color=" + TeamColor(newClass) + ">"+ newClass.team + " " + "Pawn" + "</color> Promoted to <color=" + TeamColor(newClass) + ">" + newClass.team + " " + newClass.type + "</color></size>");
        _popupNotification.ShowNotification();

        // Play SFX
        _manager.PlaySFX(_manager.RepositoryAudioFiles.promotion, 0.2f);
        
        EndTurn();
    }

    private void PreventCheck()
    {
        Chesspiece targetKing = null;
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                if (_chessPieces[x, y] != null)
                    if (_chessPieces[x, y].type == ChessPieceType.King)
                        if (_chessPieces[x, y].team == _currentDragging.team)
                            targetKing = _chessPieces[x, y];

        // Simulate movement for single piece, reference available moves is used to remove moves that will put player in check
        SimulateMoveForSinglePiece(_currentDragging, ref availableMoves, targetKing);
    }

    private void SimulateMoveForSinglePiece(Chesspiece currentPiece, ref List<Vector2Int> moves, Chesspiece targetKing)
    {
        // Save the current values, to reset after the function call
        int actualX = currentPiece.currentX;
        int actualY = currentPiece.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going through all the moves, simulate them and then check if player is in check
        for (int i = 0; i < moves.Count; i++)
        {
            // Tiles that piece can move to
            int simX = moves[i].x;
            int simY = moves[i].y;
            
            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            // Did we simulate the king's move
            if (currentPiece.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);
            
            // Copy the ChessPieces array without reference
            Chesspiece[,] simulation = new Chesspiece[TileCountX, TileCountY];
            List<Chesspiece> simulationAttackingPieces = new List<Chesspiece>();
            for (int x = 0; x < TileCountX; x++)
               for (int y = 0; y < TileCountY; y++)
                if (_chessPieces[x, y] != null)
                {
                    simulation[x, y] = _chessPieces[x, y];
                    if (simulation[x,y].team != currentPiece.team)
                        simulationAttackingPieces.Add(simulation[x,y]);
                }

            // Simulate the move
            simulation[actualX, actualY] = null;
            currentPiece.currentX = simX;
            currentPiece.currentY = simY;
            simulation[simX, simY] = currentPiece;
            
            // Did one of the piece got taken down during the simulation ?
            var deadPiece = simulationAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
                simulationAttackingPieces.Remove(deadPiece);
            
            // Get all the simulated attacking pieces move
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simulationAttackingPieces.Count; a++)
            {
                var pieceMoves = simulationAttackingPieces[a].GetAvailableMoves(ref simulation, TileCountX, TileCountY);
                for (int b = 0; b < pieceMoves.Count; b++)
                    simMoves.Add(pieceMoves[b]);
            }
            
            // Is the king in trouble ? if yes, remove the move
            if (ContainValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            // Restore the actual Current Piece's data
            currentPiece.currentX = actualX;
            currentPiece.currentY = actualY;
        }

        // Remove from the current available move list
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);
    }

    private bool IsCheckmate()
    {
        // Get the last move by searching the move list
        var lastMove = moveList[moveList.Count - 1];
        
        // Get the target team by checking the last piece that was moved
        Team targetTeam = (_chessPieces[lastMove[1].x, lastMove[1].y]).team == Team.White ? Team.Black : Team.White;

        List<Chesspiece> attackingPieces = new List<Chesspiece>();
        List<Chesspiece> defendingPieces = new List<Chesspiece>();
        Chesspiece targetKing = null;
        for (int x = 0; x < TileCountX; x++)
        for (int y = 0; y < TileCountY; y++)
            if (_chessPieces[x, y] != null)
            {
                if (_chessPieces[x, y].team == targetTeam)
                {
                    defendingPieces.Add(_chessPieces[x,y]);
                    if (_chessPieces[x, y].type == ChessPieceType.King)
                        targetKing = _chessPieces[x, y];
                }
                else
                {
                    attackingPieces.Add(_chessPieces[x,y]);
                }
            }

        // Is the king attack right now?
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
            for (int b = 0; b < pieceMoves.Count; b++)
                currentAvailableMoves.Add(pieceMoves[b]);
        }
        
        // Is the king in checked right now?
        if (targetKing != null && ContainValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            // King is under attack, Is it possible to help him?
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
                
                // Simulate movement for single piece, reference available moves is used to remove moves that will put player in check
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);

                if (defendingMoves.Count != 0)
                    return false;
            }

            // Checkmate
            return true;
        }

        return false;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Change material on mesh renderer
    
    private void ChangeMaterial(MeshRenderer meshRenderer, Material newMaterial, int materialIndex = 0)
    {
        Material[] mats = meshRenderer.materials;
        mats[materialIndex] = newMaterial;
        meshRenderer.materials = mats;
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // End Turn, Game End and Start Over

    public void EndTurn()
    {
        // change current turn
        _currentTurn = _currentTurn switch
        {
            Team.White => Team.Black,
            Team.Black => Team.White,
            _ => _currentTurn
        };
        
        Debug.Log("Current Turn: " + _currentTurn);
        _tmpCurrentTurn.text = _currentTurn + "'s Turn";

        // Check if player has any valid movements, if not, show skip button
        if (_manager.gameMode == GameMode.vsAi)
        {
            if (_manager.playerTeam == _currentTurn)
                CheckAvailableMovements();
        }
        else
        {
            CheckAvailableMovements();
        }   

        void CheckAvailableMovements()
        {
            List<Vector2Int> validMovements = new List<Vector2Int>();

            for (int x = 0; x < TileCountX; x++)
                for (int y = 0; y < TileCountY; y++)
                    if (_chessPieces[x, y] != null && _chessPieces[x,y].team == _currentTurn)
                    {
                        List<Vector2Int> pieceMoves = _chessPieces[x, y].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
                        PreventCheck(ref pieceMoves, _currentTurn, _chessPieces[x, y]);
                        
                        // Add individual piece movements to the total valid movements
                        foreach (var location in pieceMoves.Where(location => !validMovements.Contains(location)))
                        { 
                            validMovements.Add(location);
                        }
                    }
            
            // if there is no valid movement
            if (validMovements.Count == 0)
                _objSkipButton.SetActive(true);
        }
    }
    
    private void Checkmate(Team winTeam)
    {
        _objSkipButton.SetActive(false);
        if (_manager.gameMode == GameMode.vsAi)
            StopAi();
        
        OnGameEnd?.Invoke();
        
        _winningPanel.Initialize(winTeam);
        Invoke("ResetGame", 0.1f);
    }

    private void ResetGame()
    {
        foreach (Chesspiece piece in _chessPieces)
        {
            if (piece != null)
            {
                TransferChessPieceToPool(piece);
                //DestroyImmediate(piece.gameObject);
            }
        }

        for (int i = 0; i < _deadWhites.Count;)
        {
            TransferChessPieceToPool(_deadWhites[0].GetComponent<Chesspiece>());
            //DestroyImmediate(_deadWhites[0].gameObject);
            _deadWhites.RemoveAt(0);
        }
        
        for (int i = 0; i < _deadBlacks.Count;)
        {
            TransferChessPieceToPool(_deadBlacks[0].GetComponent<Chesspiece>());
            //DestroyImmediate(_deadBlacks[0].gameObject);
            _deadBlacks.RemoveAt(0);
        }
        
        availableMoves.Clear();
        moveList.Clear();
    }

    private void TransferChessPieceToPool(Chesspiece piece)
    {
        switch (piece.type)
        {
            case ChessPieceType.Pawn:
                piece.transform.SetParent(_tPoolPawn);
                break;

            case ChessPieceType.Rook:
                piece.transform.SetParent(_tPoolRook);
                break;
            
            case ChessPieceType.Knight:
                piece.transform.SetParent(_tPoolKnight);
                break;
            
            case ChessPieceType.Bishop:
                piece.transform.SetParent(_tPoolBishop);
                break;
            
            case ChessPieceType.Queen:
                piece.transform.SetParent(_tPoolQueen);
                break;
            
            case ChessPieceType.King:
                piece.transform.SetParent(_tPoolKing);
                break;
        }
        
        piece.transform.localScale = Vector3.one;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // UI
    
    string TeamColor(Chesspiece piece)
    {
        // Get team color for UI purposes
        string value = piece.team switch
        {
            Team.White => "#FFFFFF",
            Team.Black => "#000000",
            _ => ""
        };

        return value;
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // AI Logic

    public void StartAi()
    {
        if (_manager.gameMode != GameMode.vsAi) return;
        
        if (AICoroutine != null)
            StopCoroutine(AICoroutine);

        AICoroutine = AIComputation();
        StartCoroutine(AICoroutine);
    }

    private void StopAi()
    {
        if (AICoroutine != null)
            StopCoroutine(AICoroutine);
    }

    private IEnumerator AICoroutine;

    IEnumerator AIComputation()
    {
        yield return new WaitForSeconds(1);
        
        Team aiTeam = _manager.playerTeam == Team.White ? Team.Black : Team.White;
        Debug.Log("AI Team: " + aiTeam);
        
        List<Chesspiece> aiPieces = new List<Chesspiece>();
        
        while (true)
        {
            aiPieces.Clear();
            
            yield return new WaitUntil(() => _currentTurn == aiTeam);
            
            //Chesspiece targetKing = null;
            List<Vector2Int> aiAvailableMoves = new List<Vector2Int>();

            // Check for AI's available moves
            for (int x = 0; x < TileCountX; x++)
                for (int y = 0; y < TileCountY; y++)
                    if (_chessPieces[x, y] != null)
                    {
                        if (_chessPieces[x, y].team == aiTeam)
                        {
                            /*switch (aiTeam)
                            {
                                case Team.White:
                                    if (_deadWhites.Contains(_chessPieces[x, y]))
                                        continue;
                                    break;
                                case Team.Black:
                                    if (_deadBlacks.Contains(_chessPieces[x, y]))
                                        continue;
                                    break;
                            }*/
                            
                            
                            List<Vector2Int> pieceAvailableMoves = _chessPieces[x, y].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
                            PreventCheck(ref pieceAvailableMoves, aiTeam, _chessPieces[x, y]);
                            
                            if (pieceAvailableMoves.Count > 0)
                                aiPieces.Add(_chessPieces[x, y]);
                        
                            foreach (var location in pieceAvailableMoves.Where(location => !aiAvailableMoves.Contains(location)))
                            { 
                                aiAvailableMoves.Add(location);
                            }
                        }
                    }

            int highestValue = 0;
            Vector2Int target = -Vector2Int.one;
            foreach (Vector2Int location in aiAvailableMoves)
            {
                Chesspiece chessPiece = _chessPieces[location.x, location.y];
                if (chessPiece == null) continue;
                if (chessPiece.team == aiTeam) continue;

                // get the highest value target
                int currentValue = 0;
                currentValue = chessPiece.type switch
                {
                    ChessPieceType.Pawn => 1,
                    ChessPieceType.Bishop => 3,
                    ChessPieceType.Knight => 3,
                    ChessPieceType.Rook => 5,
                    ChessPieceType.Queen => 9,
                    ChessPieceType.King => 20,
                    _ => currentValue
                };

                if (currentValue > highestValue)
                {
                    highestValue = currentValue;
                    target = new Vector2Int(chessPiece.currentX, chessPiece.currentY);
                }
            }

            // move to kill
            if (highestValue != 0 && target != -Vector2Int.one)
            {
                if (!MoveToKillAI(aiTeam, target))
                    if (aiPieces.Count > 0)
                    {
                        MoveAi(aiPieces, aiTeam);
                    }
                    else
                    {
                        EndTurn();
                    }
            }
            // move
            else
            {
                if (aiPieces.Count > 0)
                {
                    MoveAi(aiPieces, aiTeam);
                }
                else
                {
                    EndTurn();
                }
            }

            yield return new WaitForSeconds(1);

            yield return null;
        }
    }
    
    private void MoveAi(List<Chesspiece> aiPieces, Team aiTeam)
    {
        int index = Random.Range(0, aiPieces.Count);
        List<Vector2Int> validMoves = aiPieces[index].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
        PreventCheck(ref validMoves, aiTeam, aiPieces[index]);

        if (validMoves.Count > 0)
        {
            _specialMoves = aiPieces[index].GetSpecialMoves(ref _chessPieces, ref moveList, ref availableMoves);
            
            int moveIndex = Random.Range(0, validMoves.Count);
            Vector2Int target = validMoves[moveIndex];

            MoveTo(aiPieces[index], target.x, target.y);
        }
        else
        {
            Debug.LogError("Move Count is 0!");
            MoveAi(aiPieces, aiTeam);
        }
    }

    private bool MoveToKillAI(Team aiTeam, Vector2Int target)
    {
        // Check for AI's available moves
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                if (_chessPieces[x, y] != null)
                {
                    if (_chessPieces[x, y].team == aiTeam)
                    {
                        List<Vector2Int> pieceAvailableMoves = _chessPieces[x, y].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY); 
                        PreventCheck(ref pieceAvailableMoves, aiTeam, _chessPieces[x, y]);

                        if (pieceAvailableMoves.Contains(target))
                        {
                            Debug.Log("Move to Kill Target at " + target);
                            MoveTo(_chessPieces[x, y], target.x, target.y);

                            return true;
                        }
                    }
                }

        return false;
    }

    private void PreventCheck(ref List<Vector2Int> moves, Team team, Chesspiece currentPiece)
    {
        Chesspiece targetKing = null;
        for (int x = 0; x < TileCountX; x++)
        for (int y = 0; y < TileCountY; y++)
            if (_chessPieces[x, y] != null)
                if (_chessPieces[x, y].type == ChessPieceType.King)
                    if (_chessPieces[x, y].team == team)
                        targetKing = _chessPieces[x, y];

        // Simulate movement for single piece, reference available moves is used to remove moves that will put player in check
        SimulateMoveForSinglePieceForAI(currentPiece, ref moves, targetKing);
    }
    
    private void SimulateMoveForSinglePieceForAI(Chesspiece currentPiece, ref List<Vector2Int> moves, Chesspiece targetKing)
    {
        // Save the current values, to reset after the function call
        int actualX = currentPiece.currentX;
        int actualY = currentPiece.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going through all the moves, simulate them and then check if player is in check
        for (int i = 0; i < moves.Count; i++)
        {
            // Tiles that piece can move to
            int simX = moves[i].x;
            int simY = moves[i].y;
            
            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            // Did we simulate the king's move
            if (currentPiece.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);
            
            // Copy the ChessPieces array without reference
            Chesspiece[,] simulation = new Chesspiece[TileCountX, TileCountY];
            List<Chesspiece> simulationAttackingPieces = new List<Chesspiece>();
            for (int x = 0; x < TileCountX; x++)
               for (int y = 0; y < TileCountY; y++)
                if (_chessPieces[x, y] != null)
                {
                    simulation[x, y] = _chessPieces[x, y];
                    if (simulation[x,y].team != currentPiece.team)
                        simulationAttackingPieces.Add(simulation[x,y]);
                }

            // Simulate the move
            simulation[actualX, actualY] = null;
            currentPiece.currentX = simX;
            currentPiece.currentY = simY;
            simulation[simX, simY] = currentPiece;
            
            // Did one of the piece got taken down during the simulation ?
            var deadPiece = simulationAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
                simulationAttackingPieces.Remove(deadPiece);
            
            // Get all the simulated attacking pieces move
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simulationAttackingPieces.Count; a++)
            {
                var pieceMoves = simulationAttackingPieces[a].GetAvailableMoves(ref simulation, TileCountX, TileCountY);
                for (int b = 0; b < pieceMoves.Count; b++)
                    simMoves.Add(pieceMoves[b]);
            }
            
            // Is the king in trouble ? if yes, remove the move
            if (ContainValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            // Restore the actual Current Piece's data
            currentPiece.currentX = actualX;
            currentPiece.currentY = actualY;
        }

        // Remove from the current available move list
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);
    }

}
