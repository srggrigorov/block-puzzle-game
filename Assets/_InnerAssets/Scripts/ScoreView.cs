using TMPro;
using UnityEngine;
using Zenject;

public class ScoreView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _scoreText;

    [Inject]
    private void Construct(ScoreService scoreService)
    {
        scoreService.OnScoreChanged += score => _scoreText.text = score.ToString();
    }
}
