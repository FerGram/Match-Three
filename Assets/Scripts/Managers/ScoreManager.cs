using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    public TextMeshProUGUI scoreText;
    public int _increment = 5;
    public int CurrentScore 
    {
        get { return _currentScore; }
    }

    int _currentScore = 0;
    int _counterValue = 0;

    void Start()
    {
        UpdateScoreText(_currentScore);
    }

    public void UpdateScoreText(int scoreValue)
    {
        if (scoreText != null)
        {
            scoreText.text = scoreValue.ToString();
        }
    }

    public void AddScore(int value)
    {
        _currentScore += value;
        StartCoroutine(CountScoreRoutine());
    }

    IEnumerator CountScoreRoutine()
    {
        int iterations = 0;
        while (_counterValue < _currentScore && iterations < 10000)
        {
            _counterValue += _increment;
            UpdateScoreText(_counterValue);
            iterations++;
            yield return null;
        }
        _counterValue = _currentScore;
        UpdateScoreText(_currentScore);
    }
}
