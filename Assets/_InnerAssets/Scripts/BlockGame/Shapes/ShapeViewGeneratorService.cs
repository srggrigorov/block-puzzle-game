using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ShapeViewGeneratorService : IInitializable
{
    public IEnumerable<ShapeView> ShapeViews => _shapeViews;

    private readonly List<Transform> _shapeViewContainers;
    private readonly ObjectPooler _objectPooler;
    private readonly ShapeView _shapeViewPrefab;
    private readonly ShapeGeneratorService _shapeGeneratorService;
    private readonly DifficultySettings _difficultySettings;
    private readonly ScoreService _scoreService;
    private readonly HashSet<ShapeView> _shapeViews = new HashSet<ShapeView>();

    private DifficultyStats _currentDifficultyStats;

    public ShapeViewGeneratorService(ObjectPooler objectPooler, ShapeView shapeViewPrefab,
        ShapeGeneratorService shapeGeneratorService, List<Transform> shapeViewContainers,
        AssetsManager assetsManager, ScoreService scoreService)
    {
        (_objectPooler, _shapeViewPrefab, _shapeGeneratorService, _shapeViewContainers, _scoreService) =
            (objectPooler, shapeViewPrefab, shapeGeneratorService, shapeViewContainers, scoreService);

        _difficultySettings = assetsManager.GetModuleSettings<DifficultySettings>();
    }

    private ShapeView CreateShapeView(Shape shape)
    {
        ShapeView shapeView = (ShapeView)_objectPooler.Spawn(_shapeViewPrefab);
        shapeView.CreateShape(shape);
        return shapeView;
    }

    public void CreateRandomShapes()
    {
        _currentDifficultyStats = _difficultySettings.TryGetDifficultyStats(
            _scoreService.Score, out var difficultyStats) ? difficultyStats.Value : _currentDifficultyStats;

        foreach (var container in _shapeViewContainers)
        {
            ShapeView shapeView =
                CreateShapeView(_shapeGeneratorService.CreateRandomShape(_currentDifficultyStats.GetRandomShapeCellsCount()));

            Transform shapeViewTransform = shapeView.transform;
            shapeViewTransform.SetParent(container, false);
            shapeViewTransform.localPosition = Vector3.zero;
            shapeViewTransform.localScale = Vector3.one * 0.5f;
            _shapeViews.Add(shapeView);
        }
    }

    public void Initialize()
    {
        CreateRandomShapes();
    }
}
