using UnityEngine;

public class PP_ChromaticAberration : PP_Base
{
    [SerializeField] Vector2Int _rOffset;
    [SerializeField] Vector2Int _gOffset;
    [SerializeField] Vector2Int _bOffset;

    [SerializeField] int _allowance = 50;

    [SerializeField] float _frequency = 3.0f;
    [SerializeField] float _intensity = 0.125f;

    private Texture2D _redTexture;
    private Texture2D _greenTexture;
    private Texture2D _blueTexture;

    private Texture2D _combinedTexture;

    protected override void Awake()
    {
        base.Awake();

        Debug.Log($"TexWidth: {_textureWidth} TexHeight: {_textureHeight}");

        _redTexture = new Texture2D(_textureWidth, _textureHeight);
        _greenTexture = new Texture2D(_textureWidth, _textureHeight);
        _blueTexture = new Texture2D(_textureWidth, _textureHeight);

        _combinedTexture = new Texture2D(_textureWidth, _textureHeight);
    }

    void Update()
    {
        // Iterate over every pixel in the texture
        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                Color sourcePixel = _sourceTexture.GetPixel(x, y);

                // Color redPixel = new Color(sourcePixel.r, 0.0f, 0.0f, sourcePixel.a);
                // Color greenPixel = new Color(sourcePixel.r, 0.0f, 0.0f, sourcePixel.a);
                // Color bluePixel = new Color(0.0f, 0.0f, sourcePixel.b, sourcePixel.a);

                float waveFormula = (Mathf.Cos(_frequency * Time.timeSinceLevelLoad + Mathf.PI/2)) * _intensity;

                _redTexture.SetPixel(Mathf.Clamp(x + (int)(_rOffset.x * waveFormula), 0, _textureWidth),
                                     Mathf.Clamp(y + (int)(_rOffset.y * waveFormula), 0, _textureHeight),
                                     new Color(sourcePixel.r, 0.0f, 0.0f, sourcePixel.a));
                _greenTexture.SetPixel(Mathf.Clamp(x + (int)(_gOffset.x * waveFormula), 0, _textureWidth),
                                       Mathf.Clamp(y + (int)(_gOffset.y * waveFormula), 0, _textureHeight),
                                       new Color(0.0f, sourcePixel.g, 0.0f, sourcePixel.a));
                _blueTexture.SetPixel(Mathf.Clamp(x + (int)(_bOffset.x * waveFormula), 0, _textureWidth),
                                      Mathf.Clamp(y + (int)(_bOffset.y * waveFormula), 0, _textureHeight),
                                      new Color(0.0f, 0.0f, sourcePixel.b, sourcePixel.a));
            }
        }

        _redTexture.Apply();
        _greenTexture.Apply();
        _blueTexture.Apply();

        // Iterate over every pixel in the texture
        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                Color rPixel = _redTexture.GetPixel(x, y);
                Color gPixel = _greenTexture.GetPixel(x, y);
                Color bPixel = _blueTexture.GetPixel(x, y);

                Color finalPixel = rPixel + gPixel + bPixel;
                // Is this redundant? as we set all texture's alpha values to sourcePixel.a in SeparateAndSetRGBPixels()
                // finalPixel.a = ReturnGreatest(rPixel.a, gPixel.a, bPixel.a);
                // Therefore, they are always the same value, see Debug.log
                // Debug.Log($"Alpha Values:   r: {rPixel.a}   g: {gPixel.a}   b:{bPixel.a}");

                finalPixel.a = ReturnGreatest(rPixel.a, gPixel.a, bPixel.a);

                _combinedTexture.SetPixel(x, y, finalPixel);
            }
        }
        _combinedTexture.Apply();

        Sprite newSprite = Sprite.Create(_combinedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        _renderer.sprite = newSprite;
    }
}
