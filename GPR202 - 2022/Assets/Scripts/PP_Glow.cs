using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PP_Glow : PP_Base
{
    [SerializeField] TMP_Text _waveValueText;

    Texture2D _emissionMapRed;
    Texture2D _diffuseMapRed;
    Texture2D _glowMapRed;
    Texture2D _glowAndSourceTextureRed;
    Texture2D _glowIntenseTextureRed;
    Texture2D _emissionMapBlue;
    Texture2D _diffuseMapBlue;
    Texture2D _glowMapBlue;
    Texture2D _glowAndSourceTextureBlue;
    Texture2D _glowIntenseTextureBlue;

    [SerializeField] bool _isBlue = false;

    float _waveFormula = 1.0f;

    // public bool IsBlue => _isBlue = _waveFormula >= 0;

    [SerializeField] int _step;

    [SerializeField] float _frequency = 3.0f;
    [SerializeField] float _intensity = 0.125f;

    [SerializeField] bool IsBlue { get => _waveFormula >= 0; }

    protected override void Awake()
    {
        base.Awake();

        _emissionMapRed = new Texture2D(_textureWidth, _textureHeight);
        _diffuseMapRed = new Texture2D(_textureWidth, _textureHeight);
        _emissionMapBlue = new Texture2D(_textureWidth, _textureHeight);
        _diffuseMapBlue = new Texture2D(_textureWidth, _textureHeight);
    }

    void Start()
    {
        CreateAndApplyEmissionMaps();

        if (_step == 0)
        {
            if (_isBlue)
            {
                CreateAndRenderSprite(_emissionMapBlue);
                return;
            }
            else
            {
                CreateAndRenderSprite(_emissionMapRed);
                return;
            }
        }

        // DIFFUSE MAP CREATION FROM EMISSION MAP TEXTURE2D
        CreateAndApplyDiffuseMap(_diffuseMapBlue, _emissionMapBlue);
        CreateAndApplyDiffuseMap(_diffuseMapRed, _emissionMapRed);

        if (_step == 1)
        {
            if (_isBlue)
            {
                CreateAndRenderSprite(_diffuseMapBlue);
                return;
            }
            else
            {
                CreateAndRenderSprite(_diffuseMapRed);
                return;
            }
        }

        _glowMapBlue = ApplyKernel(_diffuseMapBlue, _boxBlurKernel);
        _glowMapRed = ApplyKernel(_diffuseMapRed, _boxBlurKernel);
        _glowMapBlue.Apply();
        _glowMapRed.Apply();

        if (_step == 2)
        {
            if (_isBlue)
            {
                CreateAndRenderSprite(_glowMapBlue);
                return;
            }
            else
            {
                CreateAndRenderSprite(_glowMapRed);
                return;
            }
        }
    }

    void Update()
    {
        if (_step >= 3)
        {
            _waveFormula = (Mathf.Cos(_frequency * Time.timeSinceLevelLoad + Mathf.PI)) * _intensity;
            _waveValueText.SetText(_waveFormula.ToString("0.000"));
            // increase intensity of glow
            _glowIntenseTextureRed = new Texture2D(_textureWidth, _textureHeight);
            _glowIntenseTextureBlue = new Texture2D(_textureWidth, _textureHeight);

            for (int y = 0; y < _textureHeight; ++y)
            {
                for (int x = 0; x < _textureWidth; ++x)
                {
                    Color glowPixelBlue;
                    Color glowPixelRed;
                    Color intensePixelRed;
                    Color intensePixelBlue;

                    if (IsBlue)
                    {
                        glowPixelBlue = _glowMapBlue.GetPixel(x, y);
                        intensePixelBlue = glowPixelBlue * _waveFormula;
                        intensePixelBlue.a = glowPixelBlue.a;
                        _glowIntenseTextureBlue.SetPixel(x, y, intensePixelBlue);
                    }
                    else
                    {
                        glowPixelRed = _glowMapRed.GetPixel(x, y);
                        intensePixelRed = glowPixelRed * (-_waveFormula);
                        intensePixelRed.a = glowPixelRed.a;
                        _glowIntenseTextureRed.SetPixel(x, y, intensePixelRed);
                    }
                }
            }
            if (IsBlue)
                _glowIntenseTextureBlue.Apply();
            else
                _glowIntenseTextureRed.Apply();

            if (_step == 3)
            {
                if (IsBlue)
                {
                    CreateAndRenderSprite(_glowIntenseTextureBlue);
                    return;
                }
                else
                {
                    CreateAndRenderSprite(_glowIntenseTextureRed);
                    return;
                }
            }

            if (IsBlue)
            {
                _glowAndSourceTextureBlue = Combine(_glowIntenseTextureBlue, _sourceTexture);
            }
            else
            {
                _glowAndSourceTextureRed = Combine(_glowIntenseTextureRed, _sourceTexture);
            }

            if (_step == 4)
            {
                if (IsBlue)
                {
                    CreateAndRenderSprite(_glowAndSourceTextureBlue);
                    return;
                }
                else
                {
                    CreateAndRenderSprite(_glowAndSourceTextureRed);
                    return;
                }
            }
        }
        else
            return;
    }

    private void CreateAndRenderSprite(Texture2D texture)
    {
        Sprite newSprite = Sprite.Create(texture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        _renderer.sprite = newSprite;
        return;
    }

    private void CreateAndApplyDiffuseMap(Texture2D diffuseMap, Texture2D emissionMap)
    {
        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                Color sourcePixel = _sourceTexture.GetPixel(x, y);
                Color emissionMapPixel = emissionMap.GetPixel(x, y);

                Color diffusePixelColor = new Color
                {
                    r = sourcePixel.r,
                    g = sourcePixel.g,
                    b = sourcePixel.b,
                    a = emissionMapPixel.a
                };

                diffuseMap.SetPixel(x, y, diffusePixelColor);
            }
        }
        diffuseMap.Apply();
    }

    private void CreateAndApplyEmissionMaps()
    {
        Color noGlow = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        // EMISSION MAP CREATION FROM SOURCE TEXTURE2D
        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                Color sourcePixel = _sourceTexture.GetPixel(x, y);

                // PSEUDO CODE FOR EMMISSION MAP:
                /* if (pixel is red)
                 *    pixel = white;
                 * else
                 *    pixel = black;
                 * */

                if (sourcePixel.b > sourcePixel.g + sourcePixel.r)
                {
                    Color _blueIntensity = new Color(sourcePixel.b, sourcePixel.b, sourcePixel.b, sourcePixel.b);
                    _emissionMapBlue.SetPixel(x, y, _blueIntensity);
                }
                else
                    _emissionMapBlue.SetPixel(x, y, noGlow);

                if (sourcePixel.r > sourcePixel.g + sourcePixel.b)
                {
                    Color _redIntensity = new Color(sourcePixel.r, sourcePixel.r, sourcePixel.r, sourcePixel.r);
                    _emissionMapRed.SetPixel(x, y, _redIntensity);
                }
                else
                    _emissionMapRed.SetPixel(x, y, noGlow);
            }
        }
        _emissionMapBlue.Apply();
        _emissionMapRed.Apply();
    }
}
