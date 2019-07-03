using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;

namespace TicTacToe
{
    public class ARManager : MonoBehaviour
    {
        [SerializeField] GameObject gameBoard;

        public void Initialize(Slider gameType)
        {
            if (gameType.value == 0f)
            {
                gameObject.SetActive(true);
            }
            else
                gameObject.SetActive(false);
        }

        public void LockPlane()
        {
            gameBoard.SetActive(true);
        }
    }
}