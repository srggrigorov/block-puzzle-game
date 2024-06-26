using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string _gameSceneName;

    private ZenjectSceneLoader _sceneLoader;

    [Inject]
    private void Construct(ZenjectSceneLoader sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    async private void Awake()
    {
        AssetsManager assetsManager = new AssetsManager();
        await assetsManager.InitializeAsync();
        await assetsManager.LoadModulesSettings();

        _sceneLoader.LoadSceneAsync(_gameSceneName, LoadSceneMode.Single,
            container => container.Bind<AssetsManager>().FromInstance(assetsManager).AsSingle().NonLazy());
    }
}
