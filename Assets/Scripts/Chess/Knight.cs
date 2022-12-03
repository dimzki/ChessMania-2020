using System.Collections.Generic;
using UnityEngine;

public class Knight : Chesspiece
{
    public override List<Vector2Int> GetAvailableMoves(ref Chesspiece[,] board, int newTileCountX, int newTileCountY)
    {
        List<Vector2Int> value = new List<Vector2Int>();
        Vector2Int target;
        
        // front right movement
        target = new Vector2Int(currentX + 1, currentY + 2);
        CheckValid(ref board);
        
        // front left movement
        target = new Vector2Int(currentX - 1, currentY + 2);
        CheckValid(ref board);
        
        // back right movement
        target = new Vector2Int(currentX + 1, currentY - 2);
        CheckValid(ref board);
        
        // back left movement
        target = new Vector2Int(currentX - 1, currentY - 2);
        CheckValid(ref board);
        
        // left up movement
        target = new Vector2Int(currentX - 2, currentY + 1);
        CheckValid(ref board);
        
        // left down movement
        target = new Vector2Int(currentX - 2, currentY - 1);
        CheckValid(ref board);
        
        // right up movement
        target = new Vector2Int(currentX + 2, currentY + 1);
        CheckValid(ref board);
        
        // right down movement
        target = new Vector2Int(currentX + 2, currentY - 1);
        CheckValid(ref board);

        void CheckValid(ref Chesspiece[,] board)
        {
            if (target.x >= newTileCountX || target.y >= newTileCountY || target.x <= -1 || target.y <= -1) return;
            
            if (board[target.x, target.y] == null)
                value.Add(target);
            else if (board[target.x, target.y].team != team)
                value.Add(target);
        }

        return value;
    }
}
