using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Chesspiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Chesspiece[,] board, int newTileCountX, int newTileCountY)
    {
        List<Vector2Int> value = new List<Vector2Int>();

        // upward movement
        for (int i = currentY + 1; i < newTileCountY; i++)
            if (board[currentX, i] == null)
            {
                value.Add(new Vector2Int(currentX, i));
            }
            else
            {
                // check if enemy
                if (board[currentX, i].team != team) 
                    value.Add(new Vector2Int(currentX, i));
                break;
            }
        
        // backward movement
        for (int i = currentY - 1; i > -1; i--)
            if (board[currentX, i] == null)
            {
                value.Add(new Vector2Int(currentX, i));
            }
            else
            {
                // check if enemy
                if (board[currentX, i].team != team) 
                    value.Add(new Vector2Int(currentX, i));
                break;
            }
        
        // left movement
        for (int i = currentX + 1; i < newTileCountY; i++)
            if (board[i, currentY] == null)
            {
                value.Add(new Vector2Int(i, currentY));
            }
            else
            {
                // check if enemy
                if (board[i, currentY].team != team) 
                    value.Add(new Vector2Int(i, currentY));
                break;
            }
        
        // right movement
        for (int i = currentX - 1; i > -1; i--)
            if (board[i, currentY] == null)
            {
                value.Add(new Vector2Int(i, currentY));
            }
            else
            {
                // check if enemy
                if (board[i, currentY].team != team) 
                    value.Add(new Vector2Int(i, currentY));
                break;
            }


        return value;
    }
}
