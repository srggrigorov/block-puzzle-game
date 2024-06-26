using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField]
    private List<Transform> _shapeViewContainers;
    [SerializeField]
    private ShapeView _shapeViewPrefab;
    [SerializeField]
    private GridCellView _gridCellViewPrefab;
    [SerializeField]
    private Transform _gridContainer;
    [SerializeField]
    private GraphicRaycaster _graphicRaycaster;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<ObjectPooler>().AsSingle().NonLazy();
        Container.BindFactory<GridCellView, GridCellView.Factory>().FromComponentInNewPrefab(_gridCellViewPrefab).NonLazy();
        Container.BindInterfacesAndSelfTo<GridService>().AsSingle().WithArguments(_gridContainer).NonLazy();
        Container.Bind<ShapeGeneratorService>().AsSingle().NonLazy();
        Container.Bind<ScoreService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ShapeViewGeneratorService>().AsSingle().WithArguments(_shapeViewPrefab, _shapeViewContainers).NonLazy();
        Container.BindInterfacesAndSelfTo<ShapePlacerService>().AsSingle().WithArguments(_graphicRaycaster).NonLazy();
    }
}
