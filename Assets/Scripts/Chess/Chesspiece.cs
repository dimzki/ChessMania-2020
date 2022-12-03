using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public enum Team
{
    White = 0, Black = 1
}

public class Chesspiece : MonoBehaviour
{
    [ShowOnly] public Team team;
    [ShowOnly] public int currentX;
    [ShowOnly] public int currentY;
    [ShowOnly] public Vector2Int previousPosition = new Vector2Int();

    public ChessPieceType type;
    
    private Vector3 _desiredPosition;
    private Vector3 _desiredScale = Vector3.one;

    [ShowOnly] public int tileCountX;
    [ShowOnly] public int tileCountY;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, _desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, _desiredScale, Time.deltaTime * 10);
    }

    public void Initialize(int newTileCountX, int newTileCountY)
    {
        tileCountX = newTileCountX;
        tileCountY = newTileCountY;
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Chesspiece[,] board, int newTileCountX, int newTileCountY)
    {
        List<Vector2Int> value = new List<Vector2Int>();
        
        value.Add(new Vector2Int(3,3));
        value.Add(new Vector2Int(3,4));
        value.Add(new Vector2Int(4,3));
        value.Add(new Vector2Int(4,4));

        return value;
    }


    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        _desiredPosition = position;
        if (force)
            transform.position = _desiredPosition;
    }
    
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        _desiredScale = scale;
        if (force)
            transform.localScale = _desiredScale;
    }

    public virtual SpecialMoves GetSpecialMoves(ref Chesspiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        return SpecialMoves.none;
    }
}
