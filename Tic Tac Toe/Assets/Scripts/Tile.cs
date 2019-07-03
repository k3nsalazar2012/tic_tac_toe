using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe
{
    public class Tile : MonoBehaviour
    {
        void OnMouseDown()
        {
            if (GameBoard.Instance.GameOver) return;

            this.GetComponent<BoxCollider>().enabled = false;

            ObjectPooler.Instance.SpawnFromPool(GameBoard.Instance.CurrentTurn, transform.position, transform);
            GameBoard.Instance.EmptyTiles.Remove(this);
            GameBoard.Instance.AddTurn(GameBoard.Instance.CurrentTurn, gameObject);

            if(!GameBoard.Instance.CheckWinner(GameBoard.Instance.CurrentTurn))
            {
                if(GameBoard.Instance.gameMode == GameBoard.GameMode.SinglePlayer)
                    GameBoard.Instance.AITurn();
                else
                   GameBoard.Instance.ChangeTurn();
            }             
        }
    }
}