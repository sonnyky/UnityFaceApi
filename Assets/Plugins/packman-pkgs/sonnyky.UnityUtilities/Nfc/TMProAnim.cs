using TMPro;
using UnityEngine;

public class TMProAnim : MonoBehaviour
{
    public string Text;
    public float CharactersPerSecond;

    private TMP_Text _textMesh;

    private Color32 _textColor;

    private TMP_TextInfo _textInfo;
    private TMP_CharacterInfo[] _characterInfo;
    private Mesh _mesh;
    private Color32[] _vertexColors;
    private CanvasRenderer _canvasRenderer;

    private float _currentTime;

    private int _lastIndex;

    void Start()
    {
        _textMesh = GetComponent<TMP_Text>();



        if (_textMesh == null)
        {
            _textMesh = GetComponent<TMP_Text>();
        }

        _textColor = _textMesh.color;
        _textMesh.color = new Color32(_textColor.r, _textColor.g, _textColor.b, 0);
        _textMesh.text = Text;

        _textMesh.ForceMeshUpdate();

        _textInfo = _textMesh.textInfo;
        _mesh = _textMesh.mesh;
        _vertexColors = _textInfo.meshInfo[0].colors32;
        _characterInfo = _textInfo.characterInfo;
    }

    void Update()
    {
        var count = _textInfo.characterCount;

        var currentIndex = (int)(_currentTime * CharactersPerSecond);

        if (currentIndex >= count)
        {
            if (_lastIndex != currentIndex)
            {
                _textMesh.color = _textColor;
                _lastIndex = currentIndex;
            }

            return;
        }

        // Only change colors of visible characters.
        if (_characterInfo[currentIndex].isVisible)
        {
            if (_lastIndex != currentIndex)
            {
                SetCharacterColor(_lastIndex, _textColor);
            }

            _lastIndex = currentIndex;

            var t = _currentTime % (1 / CharactersPerSecond);
            var c = new Color32(_textColor.r, _textColor.g, _textColor.b, (byte)(_textColor.a * t));

            SetCharacterColor(currentIndex, c);

            _mesh.colors32 = _vertexColors;

            // Canvas system requires using SetMesh every time the content of the mesh changes.
            if ((_textMesh as TextMeshProUGUI) != null)
                _textMesh.canvasRenderer.SetMesh(_mesh);
        }

        _currentTime += Time.deltaTime;
    }

    private void SetCharacterColor(int index, Color32 color)
    {
        int vi = _characterInfo[index].vertexIndex;

        _vertexColors[vi + 0] = color;
        _vertexColors[vi + 1] = color;
        _vertexColors[vi + 2] = color;
        _vertexColors[vi + 3] = color;
    }
}