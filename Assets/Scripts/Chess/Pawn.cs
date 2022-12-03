using System.Collections.Generic;
using UnityEngine;

public class Pawn : Chesspiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Chesspiece[,] board, int newTileCountX, int newTileCountY)
    {
        List<Vector2Int> value = new List<Vector2Int>();

        // Movement Direction, pawn can only move forward
        int direction = team == Team.White ? 1 : -1;

        // dont proceed if white pawn on Y = 7 or black pawn on Y = 0
        if (direction == 1 ? currentY == 7 : currentY == 0)
            return value;
        
        // Straight movement
        if (board[currentX, currentY + direction] == null)
        {
            // 1 step movement
            value.Add(new Vector2Int(currentX, currentY + direction));

            // check for 2 step movement
            switch (team)
            {
                case Team.White:
                    if (currentY == 1 && board[currentX, currentY + direction * 2] == null)
                        value.Add(new Vector2Int(currentX, currentY + direction * 2));
                    break;
                
                case Team.Black:
                    if (currentY == 6 && board[currentX, currentY + direction * 2] == null)
                        value.Add(new Vector2Int(currentX, currentY + direction * 2));
                    break;
            }
        }

        // Diagonal attack left
        if (currentX > 0)
        {
            Chesspiece cp1 = board[currentX - 1, currentY + direction];
            if (cp1 != null && cp1.team != team)
                value.Add(new Vector2Int( currentX - 1, currentY + direction));
        }

        // Diagonal attack right
        if (currentX < 7)
        {
            Chesspiece cp2 = board[currentX + 1, currentY + direction];
            if (cp2 != null && cp2.team != team)
                value.Add(new Vector2Int( currentX + 1, currentY + direction));
        }
        
        return value;
    }

    public override SpecialMoves GetSpecialMoves(ref Chesspiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == Team.White) ? 1 : -1;
        
        // En Passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            
            // check if the last piece moved was a pawn
            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn)
            {
                // if the last move was a 2 step forward by the pawn
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)
                {
                    // check if the last pawn was not from current team (Debug Check)
                    if (board[lastMove[1].x, lastMove[1].y].team != team)
                    {
                        // if both pawns are on the same Y
                        if (lastMove[1].y == currentY)
                        {
                            // if both pawns are adjacent to each others
                            if (lastMove[1].x == currentX - 1 || lastMove[1].x == currentX + 1)
                            {
                                availableMoves.Add(new Vector2Int(lastMove[1].x, currentY + direction));
                                return SpecialMoves.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        // Promotion
        if (direction == 1 ? currentY == 6 : currentY == 1)
        {
            return SpecialMoves.Promotion;
        }

        return SpecialMoves.none;
    }
}
