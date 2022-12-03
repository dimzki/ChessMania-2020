using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Chesspiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Chesspiece[,] board, int newTileCountX, int newTileCountY)
    {
        List<Vector2Int> value = new List<Vector2Int>();
        int x;
        
        // Diagonal Up Right
        x = 1;
        for (int i = currentY + 1; i < newTileCountY; i++)
        {
            Vector2Int target = new Vector2Int(currentX + x, i);
            
            if (CheckValid(ref board, target)) break;
            x++;
        }
        
        // Diagonal Up Left
        x = 1;
        for (int i = currentY + 1; i < newTileCountY; i++)
        {
            Vector2Int target = new Vector2Int(currentX - x, i);
            
            if (CheckValid(ref board, target)) break;
            x++;
        }
        
        // Diagonal Down Right
        x = 1;
        for (int i = currentY - 1; i > -1; i--)
        {
            Vector2Int target = new Vector2Int(currentX + x, i);
            
            if (CheckValid(ref board, target)) break;
            x++;
        }
        
        // Diagonal Down Left
        x = 1;
        for (int i = currentY - 1; i > -1; i--)
        {
            Vector2Int target = new Vector2Int(currentX - x, i);
            
            if (CheckValid(ref board, target)) break;
            x++;
        }

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
}
