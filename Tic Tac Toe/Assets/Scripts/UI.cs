using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TicTacToe
{
    public class UI : MonoBehaviour
    {
        [SerializeField] Slider playerSide, gameType, gameMode, boardSize;
        [SerializeField] TMP_Text feedbackText;
        public Transform arParent;
        public Transform arPanel;
        public new Camera camera;

        private void Awake()
        {
            LoadSettings();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        #region Changing Scenes
        public void Feedback(string feedback)
        {
            feedbackText.text = feedback;
            feedbackText.gameObject.SetActive(true);
        }

        public void PlayAgain()
        {
            feedbackText.gameObject.SetActive(false);
            ObjectPooler.Instance.ResetPool();
            GameBoard.Instance.ResetGame();
        }

        public void MainMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        }

        public void AdjustCamera()
        {
            float x = (boardSize.value - 1) * 0.5f;

            camera.transform.localPosition = new Vector3(x, camera.transform.localPosition.y, camera.transform.localPosition.z);
            camera.fieldOfView = 10 * boardSize.value;
        }
        #endregion

        #region Loading and Updating Settings
        private void LoadSettings()
        {
            if (!PlayerPrefs.HasKey("Settings")) return;

            string _settings = PlayerPrefs.GetString("Settings");
            Settings settings = JsonUtility.FromJson<Settings>(_settings);

            playerSide.value = settings.side;
            gameType.value = settings.type;
            gameMode.value = settings.mode;
            boardSize.value = settings.size;
        }

        public void UpdateSettings()
        {
            Settings settings = new Settings();
            settings.side = (int)playerSide.value;
            settings.type = (int)gameType.value;
            settings.mode = (int)gameMode.value;
            settings.size = (int)boardSize.value;

            PlayerPrefs.SetString("Settings", JsonUtility.ToJson(settings));
        }

        public void UpdateBoardSize(TMP_Text sizeText)
        {
            sizeText.text = boardSize.value.ToString();
        }
        #endregion
    }

    public class Settings
    {
        public int side;
        public int type;
        public int mode;
        public int size;
    }
}