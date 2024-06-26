using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class GameEndScreenView : MonoBehaviour
{
    [SerializeField]
    private Button _gameEndButton;

    private ZenjectSceneLoader _sceneLoader;
    private AssetsManager _assetsManager;
    private ShapePlacerService _shapePlacerService;

    [Inject]
    private void Construct(ZenjectSceneLoader sceneLoader, AssetsManager assetsManager, ShapePlacerService shapePlacerService)
    {
        (_sceneLoader, _assetsManager, _shapePlacerService) = (sceneLoader, assetsManager, shapePlacerService);

        _shapePlacerService.OnPlaceForShapesDepleted += () => gameObject.SetActive(true);
        _gameEndButton.onClick.AddListener(RestartGame);
    }

    private void RestartGame()
    {
        _sceneLoader.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single,
            container => container.Bind<AssetsManager>().FromInstance(_assetsManager).AsSingle().NonLazy());
    }
}
