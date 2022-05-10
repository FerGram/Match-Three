using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public bool dotsCollapsed = true;
    public int movesLeft = 30;
    public int scoreGoal = 10000;
    public ScreenFader screenFader;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI movesLeftText;

    [Header("Message Window")]
    public MessageWindow messageWindow;
    public Sprite winIcon;
    public Sprite loseIcon;
    public Sprite goalIcon;

    private Board _board;
    private bool _isReadyToBegin = false;
    private bool _isGameOver = false;
    private bool _isWinner = false;
    private bool _isReadyToReload = false;

    private static int _sceneCount = 0;

    void Start()
    {
        _board = FindObjectOfType<Board>().GetComponent<Board>();

        Scene scene = SceneManager.GetActiveScene();
        if (levelNameText != null)
        {
            levelNameText.text = scene.name;
        }

        UpdateMovesLeft();

        StartCoroutine(ExecuteGameLoop());
    }

    public void BeginGame()
    {
        _isReadyToBegin = true;
        messageWindow.GetComponent<RectXFormMover>().MoveXOut();
    }
    public void ChangeScene()
    {
        _isReadyToReload = true;
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine(StartGameRoutine());
        yield return StartCoroutine(PlayGameRoutine());
        yield return StartCoroutine(EndGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        if (messageWindow != null)
        {
            messageWindow.GetComponent<RectXFormMover>().MoveXIn();
            messageWindow.ShowMessage(goalIcon, "Score goal\n" + scoreGoal.ToString(), "Start");
        }

        while (!_isReadyToBegin)
        {
            yield return null;
        }

        if (screenFader != null)
        {
            screenFader.FadeOff();
        }

        yield return new WaitForSeconds(0.5f);
        if (_board != null)
        {
            _board.SetUpBoard();
        }
    }
    IEnumerator PlayGameRoutine()
    {
        while (!_isGameOver)
        {
            if (movesLeft <= 0 && dotsCollapsed)
            {
                if (scoreGoal > ScoreManager.Instance.CurrentScore)
                {
                    yield return new WaitForSeconds(2f);
                    _isGameOver = true;
                    _isWinner = false;
                }
                if (scoreGoal <= ScoreManager.Instance.CurrentScore)
                {
                    yield return new WaitForSeconds(2f);
                    _isGameOver = true;
                    _isWinner = true;
                }
            }
            yield return null;
        }
    }
    IEnumerator EndGameRoutine()
    {
        _isReadyToReload = false;

        if (screenFader != null)
        {
            screenFader.FadeOn();
        }
        if (_isWinner)
        {
            if (messageWindow != null)
            {
                messageWindow.GetComponent<RectXFormMover>().MoveXIn();
                messageWindow.ShowMessage(winIcon, "YOU MADE IT!", "Next Level");

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayRandomWinSound();
                }

                while (!_isReadyToReload)
                {
                    yield return null;
                }

                messageWindow.GetComponent<RectXFormMover>().MoveXOut();
                yield return new WaitForSeconds(2f);
                _sceneCount = _sceneCount + 1 % SceneManager.sceneCount;
                SceneManager.LoadScene(_sceneCount);
            }
        }
        else
        {
            messageWindow.GetComponent<RectXFormMover>().MoveXIn();
            messageWindow.ShowMessage(loseIcon, "Level Failed", "Retry");

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayRandomLoseSound();
            }

            while (!_isReadyToReload)
            {
                yield return null;
            }

            messageWindow.GetComponent<RectXFormMover>().MoveXOut();
            yield return new WaitForSeconds(2f);

            _sceneCount = _sceneCount + 1 % SceneManager.sceneCount;
            SceneManager.LoadScene(_sceneCount);
        }
        yield return null;
    }

    public void UpdateMovesLeft()
    {
        if (movesLeftText != null)
        {
            movesLeftText.text = "Moves : " + movesLeft;
        }
    }
}
