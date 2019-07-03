using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe
{
    public class GameBoard : MonoBehaviour
    {
        public enum GameType
        {
            Simple,
            AR
        }

        public enum PlayerSide
        {
            X,
            O
        }

        public enum GameMode
        {
            SinglePlayer,
            TwoPlayer
        }

        [SerializeField] GameObject tilePrefab;
        [SerializeField] UI uiManager;

        public static GameBoard Instance { get; private set; }

        private PlayerSide player1Side, player2Side;
        public GameType gameType;
        public GameMode gameMode;

        private Dictionary<string, int[,]> turnsDictionary;
        private int boardSize;
        private string currentTurn;
        
        private void Awake()
        {
            Instance = this;
        }

        #region Initialization and Reset
        public void Initialize()
        {
            InitializeGame();
            GenerateBoard();
        }

        private void InitializeGame()
        {
            string _settings = PlayerPrefs.GetString("Settings");
            Settings settings = JsonUtility.FromJson<Settings>(_settings);
            boardSize = settings.size;

            player1Side = (settings.side == 0) ? PlayerSide.X : PlayerSide.O;
            gameType = (settings.type == 0) ? GameType.AR : GameType.Simple;
            if (gameType == GameType.AR)
            {
                uiManager.arPanel.gameObject.SetActive(true);
                transform.SetParent(uiManager.arParent, true);
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.SetParent(null, true);
                transform.localScale = Vector3.one;
                gameObject.SetActive(true);
            }

            gameMode = (settings.mode == 0) ? GameMode.SinglePlayer : GameMode.TwoPlayer;
            if (gameMode == GameMode.TwoPlayer)
                ChangeTurn();

            player2Side = (player1Side == PlayerSide.X) ? PlayerSide.O : PlayerSide.X; // if single player, player 2 is AI

            GameOver = false;
        }

        private void GenerateBoard()
        {
            if (EmptyTiles == null)
                EmptyTiles = new List<Tile>();
            if (turnsDictionary == null)
                turnsDictionary = new Dictionary<string, int[,]>();

            turnsDictionary.Add("X", EmptyTwoDimentionalInt());
            turnsDictionary.Add("O", EmptyTwoDimentionalInt());

            for (int z = 0; z < boardSize; z++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    CreateTile(new Vector3(x, 0, z));
                }
            }
        }

        private void CreateTile(Vector3 position)
        {
            Transform tile = Instantiate(tilePrefab).transform;
            tile.SetParent(transform, true);
            tile.localScale = Vector3.one * 0.8f;
            tile.localPosition = position;
            tile.gameObject.name = string.Format("Tile ({0},{1},{2})", position.x, position.y, position.z);

            EmptyTiles.Add(tile.GetComponent<Tile>());
        }

        public void ResetGame()
        {
            currentTurn = "";
            GameOver = false;

            if (gameMode == GameMode.TwoPlayer)
                ChangeTurn();

            if (EmptyTiles == null)
                EmptyTiles = new List<Tile>();
            if (turnsDictionary == null)
                turnsDictionary = new Dictionary<string, int[,]>();

            turnsDictionary["X"] = EmptyTwoDimentionalInt();
            turnsDictionary["O"] = EmptyTwoDimentionalInt();

            foreach (Tile tile in transform.GetComponentsInChildren<Tile>())
            {
                print(tile.gameObject.name);
                tile.GetComponent<BoxCollider>().enabled = true;
                EmptyTiles.Add(tile);
            }
        }

        public int[,] EmptyTwoDimentionalInt()
        {
            int[,] emptyArray = new int[9, 9];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    emptyArray[i, j] = 0;
                }
            }

            return emptyArray;
        }
        #endregion

        #region Setters and Getters
        public List<Tile> EmptyTiles { set; get; }

        public List<Tile> ActiveTiles { set; get; }

        public bool GameOver { set; get; }

        public string CurrentTurn
        {
            set { currentTurn = value; }

            get
            {
                if (gameMode == GameMode.SinglePlayer)
                {
                    return player1Side.ToString();
                }
                else
                {
                    return currentTurn;
                }
            }
        }
        #endregion

        #region Turns and Victory Conditions
        public void AddTurn(string key, GameObject tile)
        {
            if (gameType == GameType.Simple)
                turnsDictionary[key][(int)tile.transform.position.x, (int)tile.transform.position.z] = 1;
            else
            {
                int x = int.Parse(tile.name.Substring(6, 1));
                int z = int.Parse(tile.name.Substring(10, 1));

                turnsDictionary[key][x, z] = 1;
            }
        }

        public void ChangeTurn()
        {
            if (currentTurn == "")
                currentTurn = player1Side.ToString();
            else
                currentTurn = (CurrentTurn == player1Side.ToString()) ? player2Side.ToString() : player1Side.ToString();

            if (gameMode == GameMode.TwoPlayer)
            {
                if (EmptyTiles != null && EmptyTiles.Count == 0)
                {
                    EndGame();
                }
                else
                {
                    string feedback = (CurrentTurn == player1Side.ToString()) ? "Player 1 turn" : "Player 2 turn";
                    uiManager.Feedback(feedback);
                }
            }
        }

        public void AITurn()
        {
            if (gameMode == GameMode.TwoPlayer) return;

            if (EmptyTiles.Count == 0)
            {
                EndGame();
                return;
            }

            int randomTileIndex = Random.Range(0, EmptyTiles.Count);
            Tile tile = EmptyTiles[randomTileIndex];
            tile.GetComponent<BoxCollider>().enabled = false;

            ObjectPooler.Instance.SpawnFromPool(player2Side.ToString(), tile.transform.position, tile.transform);

            GameBoard.Instance.EmptyTiles.Remove(tile);

            AddTurn(player2Side.ToString(), tile.gameObject);

            CheckWinner(player2Side.ToString());
        }

        public bool CheckWinner(string key)
        {
            Debug.Log("checking winner: " + key);
            // must be valid in 1 of 8 conditions to be declared a winner
            // winValue must be 3 to be a winner, each tile is marked 1 when placed by "X" or "O"
            int winValue = 0; 

            /* check rows
             * possible combinations
             * X | X | X         - | - | -        - | - | -
             * - | - | -    or   X | X | X   or   - | - | -
             * - | - | -         - | - | -        X | X | X
            */ 
            for (int r = 0; r < boardSize; r++)
            {
                winValue = 0;

                for (int c = 0; c < boardSize; c++) { winValue += turnsDictionary[key][c, r]; }

                if (winValue == boardSize)
                {
                    EndGame(key);
                    return true;
                }
            }
            
            /* check columns
             * possible combinations
             * X | - | -        - | X | -       - | - | X
             * X | - | -   or   - | X | -   or  - | - | X
             * X | - | -        - | X | -       - | - | X
             */
            for (int c = 0; c < boardSize; c++)
            {
                winValue = 0;
                for (int r = 0; r < boardSize; r++) { winValue += turnsDictionary[key][c, r]; }

                if (winValue == boardSize)
                {
                    EndGame(key);
                    return true;
                }
            }

            /* check diagonal up
             * possible combination
             * - | - | X
             * - | X | -
             * X | - | -
             */
            winValue = 0;
            for (int c = 0, r = 0; c < boardSize; c++, r++)
            {
                winValue += turnsDictionary[key][c, r];
            }

            if (winValue == boardSize)
            {
                EndGame(key);
                return true;
            }

            /* check diagonal down
             * possible combination
             * X | - | -
             * - | X | -
             * - | - | X
             */
            winValue = 0;
            for (int c=0, r= boardSize - 1; c < boardSize; c++,r--) { winValue += turnsDictionary[key][c, r]; }

            if (winValue == boardSize)
            {
                EndGame(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EndGame(string key = "")
        {
            string feedback = "";

            if (key == "")
            {
                feedback = "Draw";
            }
            else
            {
                if (gameMode == GameMode.SinglePlayer)
                {
                    feedback = (player1Side.ToString() == key) ? "You Win!" : "You Lose";
                }
                else
                {
                    feedback = (player1Side.ToString() == key) ? "Player 1 Wins!" : "Player 2 Wins";
                }
            }

            uiManager.Feedback(feedback);
            GameOver = true;
        }
        #endregion
    }
}