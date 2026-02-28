using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GemDash.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AGDDPlatformer
{
    public class GameManager :SingletonMonoBehaviour<GameManager> 
    {
        [Header("Players")]
        public PlayerController[] players;

        [Header("Level")]
        public List<Object> playerGoals;
        public bool timeStopped;
        public bool isGameComplete;
        public string firstLevel;
        public string nextLevel;

        [Header("Level Transition")]
        public GameObject startScreen;
        public GameObject endScreen;
        public GameObject gameOverScreen;
        public float startScreenTime = 1.0f;
        public float endScreenDelay = 1.0f;
        public float endScreenTime = 1.0f;

        [Header("Audio")]
        public AudioSource source;
        public AudioClip winSound;

        [Header("Cancellation")] 
        private CancellationTokenSource _token;

        public static event Action OnLevelReset;

        private new void Awake()
        {
            base.Awake();
            if (playerGoals.Count == 0)
            {
                playerGoals = FindObjectsByType(typeof(PlayerGoal),(FindObjectsSortMode)FindObjectsInactive.Exclude).ToList();
            }

            if (_token != null)
            {
                _token.Cancel();
                _token.Dispose();
            }

            _token = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(),
                CancellationToken.None);
        }

        private void Start()
        {
            Init(_token.Token).Forget();
        }

        private async UniTask Init(CancellationToken token)
        {
            timeStopped = true;

            endScreen.SetActive(false);
            gameOverScreen.SetActive(false);
            
            await UniTask.WaitForSeconds(startScreenTime, cancellationToken:token);
            
            startScreen.SetActive(true);
            startScreen.SetActive(false);

            timeStopped = false;
        }

        private void Update()
        {
            if (isGameComplete)
            {
                if (Input.GetButtonDown("Reset"))
                {
                    ResetGame();
                }
            }

            if (timeStopped)
                return;

            /* --- Check Player Goals --- */

            var allGoalsSatisfied = playerGoals.Cast<PlayerGoal>().All(playerGoal => playerGoal.isSatisfied);

            if (allGoalsSatisfied)
            {
                source.PlayOneShot(winSound);
                LevelCompleted(_token.Token).Forget();
            }

            if (Input.GetButtonDown("Reset"))
            {
                ResetLevel();
            }
        }

        private async UniTask LevelCompleted(CancellationToken token)
        {
            timeStopped = true;

            await UniTask.WaitForSeconds(endScreenTime, cancellationToken: token);

            endScreen.SetActive(true);

            await UniTask.WaitForSeconds(endScreenTime, cancellationToken: token);

            if (!string.IsNullOrEmpty(nextLevel))
            {
                SceneManager.LoadScene(nextLevel);
            }
            else
            {
                isGameComplete = true;
                gameOverScreen.SetActive(true);
            }
        }

        private void ResetGame()
        {
            SceneManager.LoadScene(firstLevel);
        }

        public void ResetLevel()
        {
            HostileSpawner.Instance.Init();
            foreach (var player in players)
            {
                player.ResetPlayer();
            }

            OnLevelReset?.Invoke();
        }
    }
}
