using System.Collections.Generic;
using UnityEngine;

public class King : Chesspiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Chesspiece[,] board, int newTileCountX, int newTileCountY)
    {
        List<Vector2Int> value = new List<Vector2Int>();
        Vector2Int target = new Vector2Int();
        bool valid;

        // forward
        target = new Vector2Int(currentX, currentY + 1);
        valid = CheckValid(ref board, target);

        // backward
        target = new Vector2Int(currentX, currentY - 1);
        valid = CheckValid(ref board, target);
        
        // right
        target = new Vector2Int(currentX + 1, currentY);
        valid = CheckValid(ref board, target);

        // left
        target = new Vector2Int(currentX - 1, currentY);
        valid = CheckValid(ref board, target);
        
        // forward right
        target = new Vector2Int(currentX + 1, currentY + 1);
        valid = CheckValid(ref board, target);
        
        // forward left
        target = new Vector2Int(currentX -1, currentY + 1);
        valid = CheckValid(ref board, target);

        // backward Right
        target = new Vector2Int(currentX + 1, currentY - 1);
        valid = CheckValid(ref board, target);
        
        // backward left
        target = new Vector2Int(currentX - 1, currentY - 1);
        valid = CheckValid(ref board, target);

        bool CheckValid(ref Chesspiece[,] board, Vector2Int target)
        {
            if (target.x >= newTileCountX || target.y >= newTileCountY || target.x <= -1 || target.y <= -1) return true;
            
            // if no piece on the target tile
            if (board[target.x, target.y] == null)
                value.Add(target);
            
            // if there is own piece on the target tile
            else if (board[target.x, target.y].team == team)
            {
                return true;
            }
            
            // if there is enemy piece on the target tile
            else if (board[target.x, target.y].team != team)
            {
                value.Add(target);
                return true;
            }

            return false;
        }

        return value;
    }

    public override SpecialMoves GetSpecialMoves(ref Chesspiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        SpecialMoves specialMoves = SpecialMoves.none;

        // check the starting position of both sides' king piece
        var kingMove = moveList.Find(move => move[0].x == 4 && move[0].y == (team == Team.White ? 0 : 7));
        var leftRook = moveList.Find(move => move[0].x == 0 && move[0].y == (team == Team.White ? 0 : 7));
        var rightRook = moveList.Find(move => move[0].x == 7 && move[0].y == (team == Team.White ? 0 : 7));

        int y = team == Team.White ? 0 : 7;
        
        if (kingMove == null && currentX == 4)
        {
            // check for enemy moves that can prevent castling
            List<Vector2Int> enemyMovements = new List<Vector2Int>();
            foreach (Chesspiece piece in board)
            {
                if (piece != null && piece.team != team)
                {
                    List<Vector2Int> temp = piece.GetAvailableMoves(ref board, tileCountX, tileCountY);
                    enemyMovements.AddRange(temp);
                }
            }

            foreach (var location in enemyMovements)
            {
                if (availableMoves.Contains(location))
                    availableMoves.Remove(location);
            }
            
            //left rook
            if (leftRook == null)
                if (board[0,y] != null)
                    if (board[0,y].type == ChessPieceType.Rook)
                        if (board[0,y].team == team)
                            if (board[1, y] == null && board[2, y] == null && board[3, y] == null)
                                if (!enemyMovements.Contains(new Vector2Int(1, y)) && !enemyMovements.Contains(new Vector2Int(2, y)) && !enemyMovements.Contains(new Vector2Int(3, y)))
                                {
                                    availableMoves.Add(new Vector2Int(2, 0));
                                    specialMoves = SpecialMoves.Castling;
                                }
            //right rook
            if (rightRook == null)
                if (board[7,y] != null)
                    if (board[7,y].type == ChessPieceType.Rook)
                        if (board[7,y].team == team)
                            if (board[5, y] == null && board[6, y] == null)
                                if (!enemyMovements.Contains(new Vector2Int(5, y)) && !enemyMovements.Contains(new Vector2Int(6, y)))
                                {
                                    availableMoves.Add(new Vector2Int(6, y));
                                    specialMoves = SpecialMoves.Castling;
                                }
        }

        return specialMoves;
    }
}
