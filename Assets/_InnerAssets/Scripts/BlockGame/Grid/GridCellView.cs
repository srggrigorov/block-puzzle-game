using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class GridCellView : MonoBehaviour
{
    [FormerlySerializedAs("background")]
    [SerializeField]
    private Image _background;
    [FormerlySerializedAs("emptyColor1")]
    [SerializeField]
    private Color _emptyColor1;
    [FormerlySerializedAs("emptyColor2")]
    [SerializeField]
    private Color _emptyColor2;
    [FormerlySerializedAs("canBeFilledColor")]
    [SerializeField]
    private Color _canBeFilledColor;
    [FormerlySerializedAs("filledColor")]
    [SerializeField]
    private Color _filledColor;
    private Color _emptyColor;
    [FormerlySerializedAs("CellIndex")]
    public Vector2Int cellIndex;

    public void SetBackgroundColor(bool first) => _emptyColor = _background.color = first ? _emptyColor1 : _emptyColor2;
    public void SetEmptyColor() => _background.color = _emptyColor;
    public void SetCanBeFilledColor() => _background.color = _canBeFilledColor;
    public void SetFilledColor() => _background.color = _filledColor;

    public class Factory : PlaceholderFactory<GridCellView>
    {
    }
}
