using UnityEngine;
using UnityEngine.Tilemaps;


[DefaultExecutionOrder(-1)]
public class PreviewLayer: MonoBehaviour
{
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private GameObject _previewPrefab;
    [SerializeField]
    private Sprite _previewSprite;
    [SerializeField]
    private Sprite _puzzleSprite;
    [SerializeField]
    private Transform _previewBgImage;
    [SerializeField]
    private Transform _previewNextBgImage;
    [SerializeField]
    private Transform _previewPuzzleBgImage;
    private Vector3 staticPreviewScale = new Vector3(30f, 30f);
    private Vector3 smallPreviewScale = new Vector3(18f, 18f);
    private readonly float pixelOffset = 30f;
    private readonly float smallPixelOffset = 18f;

    private TetrisItem currentItem;
    private TetrisItem nextItem;
    private int activeCount;

    private void Awake() {
        for(int i = 0; i < 4; i++) {
            var previewGameObj = Instantiate(_previewPrefab, transform);
            previewGameObj.GetComponent<SpriteRenderer>().enabled = false;

            var previewStaticObj = Instantiate(_previewPrefab, _previewBgImage);
            previewStaticObj.transform.localScale = staticPreviewScale;
            previewStaticObj.GetComponent<SpriteRenderer>().enabled = false;

            var previewNextStaticObj = Instantiate(_previewPrefab, _previewNextBgImage);
            previewNextStaticObj.transform.localScale = smallPreviewScale;
            previewNextStaticObj.GetComponent<SpriteRenderer>().enabled = false;
        }
        activeCount = 0;

        for (int i = 0; i < 25; i++) {
            var previewStaticObj = Instantiate(_previewPrefab, _previewPuzzleBgImage);
            previewStaticObj.transform.localScale = smallPreviewScale;
            previewStaticObj.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void SetPreviewItem(TetrisItem item) {
        currentItem = item;
    }

    public void SetNextPreviewItem(TetrisItem item) {
        nextItem = item;
    }

    public void ShowGhostPreview(ShapeInt shape, Vector3 worldCoords) {
        var coords = _tilemap.WorldToCell(worldCoords);

        transform.position = _tilemap.CellToWorld(coords) + new Vector3(0.5f, 0.5f, 0); // + item.TileOffset;

        for (int i = 0; i < shape.matrix.Count; i++) {
            transform.GetChild(i).localPosition = new Vector3(shape.matrix[i].x, shape.matrix[i].y);
            var _previewRenderer = transform.GetChild(i).GetComponent<SpriteRenderer>();
            _previewRenderer.sprite = _previewSprite;
            _previewRenderer.enabled = true;
        }
        activeCount = shape.matrix.Count;
    }

    public void ClearGhostPreview() {
        for(int i = 0; i < activeCount; i++) {
            transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
        }
        activeCount = 0;
    }

    public void ShowStaticPreview() {
        if (currentItem == null)
            return;

        var shape = ShapePrefabs.GetShapeInt(currentItem.shapeIdx);
        for (int i = 0; i < 4; i++) {
            var _previewRenderer = _previewBgImage.GetChild(i).GetComponent<SpriteRenderer>();
            if (i < shape.matrix.Count) {
                _previewBgImage.GetChild(i).localPosition = new Vector3(shape.matrix[i].x * pixelOffset, shape.matrix[i].y * pixelOffset);
                _previewRenderer.sprite = currentItem.Sprite;
                _previewRenderer.enabled = true;
            } else {
                _previewRenderer.enabled = false;
            }
        }
    }

    public void ShowNextPreview() {
        if (nextItem == null)
            return;

        var shape = ShapePrefabs.GetShapeInt(nextItem.shapeIdx);
        for (int i = 0; i < 4; i++) {
            var _previewRenderer = _previewNextBgImage.GetChild(i).GetComponent<SpriteRenderer>();
            if (i < shape.matrix.Count) {
                _previewNextBgImage.GetChild(i).localPosition = new Vector3(shape.matrix[i].x * smallPixelOffset, shape.matrix[i].y * smallPixelOffset);
                _previewRenderer.sprite = nextItem.Sprite;
                _previewRenderer.enabled = true;
            } else {
                _previewRenderer.enabled = false;
            }
        }
    }

    public void ShowNextPuzzlePreview(ShapeInt shape) {
        for (int i = 0; i < 25; i++) {
            var _previewRenderer = _previewPuzzleBgImage.GetChild(i).GetComponent<SpriteRenderer>();
            if (i < shape.matrix.Count) {
                _previewPuzzleBgImage.GetChild(i).localPosition = new Vector3(shape.matrix[i].x * smallPixelOffset, shape.matrix[i].y * smallPixelOffset);
                _previewRenderer.sprite = _puzzleSprite;
                _previewRenderer.enabled = true;
            } else {
                _previewRenderer.enabled = false;
            }
        }
    }
}