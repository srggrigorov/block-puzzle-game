using Dreamloft.Game.Minigames.BlockGame;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Grid = Dreamloft.Game.Minigames.BlockGame.Grid;

namespace Dreamloft.Game.Ui.Screens
{
    public class BlockGameScreenComponent : MonoBehaviour
    {
        public event Action OnGameLost;


        [SerializeField]
        private BlockGameDifficultySettings difficultySettings;
        [SerializeField]
        private ShapesViewGeneratorComponent shapesViewGenerator;
        [SerializeField]
        private GridViewComponent gridView;
        [SerializeField]
        private TMP_Text scoreText;
        [SerializeField]
        private GameObject endGameScreen;
        [SerializeField]
        private Button endScreenButton;
        [SerializeField]
        private Transform[] shapeViewContainers;
        private Grid grid;
        private ShapeGenerator shapeGenerator;
        private ReactiveCollection<ShapeViewComponent> shapeViews = new();
        private GraphicRaycaster raycaster;
        private PointerEventData pointerEventData;
        private List<RaycastResult> raycastResults = new();
        private Vector2Int[] gridCellsForFilling;
        private int score;
        private bool gameEnded;
        private BlockGameDifficultyStats currentDifficultyStats;

        private void Awake()
        {
            SetUp();
        }

        public void SetUp()
        {
            endScreenButton.onClick.AddListener(RestartGame);
            gameEnded = false;
            AddScore(0);
            if (!difficultySettings.TryGetDifficultyStats(0, out currentDifficultyStats))
            {
                Debug.LogError("No entry difficulty stats for the block game detected!");
            }

            raycaster = GetComponentInParent<GraphicRaycaster>();
            pointerEventData = new PointerEventData(EventSystem.current);
            grid = new Grid();
            shapeGenerator = new ShapeGenerator();
            gridView.CreateGridView();
            CreateRandomShapes(currentDifficultyStats);
            shapeViews.ObserveCountChanged()
                .Subscribe(count =>
                {
                    if (gameEnded)
                    {
                        return;
                    }

                    switch (count)
                    {
                        case > 0 when shapeViews.Count(ShapeCanBePlaced) < 1:
                            OnGameLost?.Invoke();
                            EndGame();
                            return;
                        case > 0:
                            return;
                        default:
                        {
                            if (difficultySettings.TryGetDifficultyStats(score, out var newDifficultyStats))
                            {
                                currentDifficultyStats = newDifficultyStats;
                            }

                            CreateRandomShapes(currentDifficultyStats);
                            break;
                        }
                    }
                }).AddTo(this);
        }

        private void ShapeViewDragged(ShapeViewComponent shapeView)
        {
            pointerEventData.position = shapeView.StartCell.position;
            raycastResults.Clear();
            raycaster.Raycast(pointerEventData, raycastResults);
            var gridCellView = raycastResults?.Select(x => x.gameObject.GetComponent<GridCellView>()).FirstOrDefault();
            if (gridCellView != null)
            {
                if (gridCellsForFilling?.Length > 0 &&
                    gridCellsForFilling[0] != gridCellView.CellIndex)
                {
                    foreach (Vector2Int cell in gridCellsForFilling)
                    {
                        gridView.GridCellViews[cell.x, cell.y].SetEmptyColor();
                    }
                }

                if (grid.ShapeCanBePlaced(gridCellView.CellIndex, shapeView.Shape, out gridCellsForFilling))
                {
                    foreach (Vector2Int cell in gridCellsForFilling)
                    {
                        gridView.GridCellViews[cell.x, cell.y].SetCanBeFilledColor();
                    }
                }

                return;
            }

            if (gridCellsForFilling != null)
            {
                foreach (Vector2Int cell in gridCellsForFilling)
                {
                    gridView.GridCellViews[cell.x, cell.y].SetEmptyColor();
                }

                gridCellsForFilling = null;
            }
        }

        private void ShapeViewReleased(ShapeViewComponent shapeView)
        {
            if (gridCellsForFilling == null)
            {
                return;
            }

            HashSet<Vector2Int> allCellsToEmpty = new();
            foreach (var cell in gridCellsForFilling)
            {
                grid.Cells[cell.x, cell.y].isFilled = true;
                gridView.GridCellViews[cell.x, cell.y].SetFilledColor();
            }

            Vector2Int[] cellsToEmpty;
            HashSet<int> checkedColumns = new(), checkedRows = new();
            HashSet<(Vector2Int Min, Vector2Int Max)> checkedSquares = new();
            foreach (var cell in gridCellsForFilling)
            {
                if (!checkedColumns.Contains(cell.x))
                {
                    checkedColumns.Add(cell.x);
                    if (grid.IsColumnFilled(cell, out cellsToEmpty))
                    {
                        allCellsToEmpty.UnionWith(cellsToEmpty);
                    }
                }

                if (!checkedRows.Contains(cell.y))
                {
                    checkedRows.Add(cell.y);
                    if (grid.IsRowFilled(cell, out cellsToEmpty))
                    {
                        allCellsToEmpty.UnionWith(cellsToEmpty);
                    }
                }

                (Vector2Int Min, Vector2Int Max) squareBorders =
                    (new Vector2Int(cell.x / 3 * 3, cell.y / 3 * 3),
                        new Vector2Int(cell.x / 3 * 3 + 3, cell.y / 3 * 3 + 3));
                if (!checkedSquares.Contains(squareBorders))
                {
                    checkedSquares.Add(squareBorders);
                    if (grid.IsSquareFilled(cell, out cellsToEmpty))
                    {
                        allCellsToEmpty.UnionWith(cellsToEmpty);
                    }
                }
            }

            foreach (var cell in allCellsToEmpty)
            {
                grid.Cells[cell.x, cell.y].isFilled = false;
                gridView.GridCellViews[cell.x, cell.y].SetEmptyColor();
            }

            AddScore(allCellsToEmpty.Count > 0 ? allCellsToEmpty.Count : gridCellsForFilling.Length);
            gridCellsForFilling = null;
            shapeViews.Remove(shapeView);
            Destroy(shapeView.gameObject);
        }

        private void AddScore(int points)
        {
            score += points;
            scoreText.text = score.ToString();
        }

        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void EndGame()
        {
            gameEnded = true;
            endGameScreen.SetActive(true);
        }

        private void CreateRandomShapes(BlockGameDifficultyStats currentDifficultyStats)
        {
            foreach (var container in shapeViewContainers)
            {
                ShapeViewComponent shapeView =
                    shapesViewGenerator.CreateShapeView(
                        shapeGenerator.CreateRandomShape(currentDifficultyStats.GetRandomShapeCellsCount()));

                Transform shapeViewTransform = shapeView.transform;
                shapeViewTransform.SetParent(container, false);
                shapeViewTransform.localScale = Vector3.one * 0.5f;
                shapeView.OnPointerUpEvent += ShapeViewDragged;
                shapeView.OnDragObservable.Subscribe(_ => { ShapeViewDragged(shapeView); }).AddTo(shapeView)
                    .AddTo(this);
                shapeView.OnPointerUpEvent += ShapeViewReleased;
                shapeViews.Add(shapeView);
            }
        }

        private bool ShapeCanBePlaced(ShapeViewComponent shapeView)
        {
            bool canBePlaced = grid.ShapeCanBePlacedAnywhere(shapeView.Shape);
            shapeView.SetInteractable(canBePlaced);
            return canBePlaced;
        }
    }
}