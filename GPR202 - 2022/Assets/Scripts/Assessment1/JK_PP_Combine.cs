using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JK_PP_Combine : PP_Base
{
    [SerializeField] string _firstEffect = "BOX_BLUR";
    [SerializeField] string _secondEffect = "RIDGE";
    [SerializeField] bool _isPartyTime = false;

    [SerializeField] Button _partyToggleBtn;
    [SerializeField] bool _experiment = false;

    public bool IsPartyTime { get => _isPartyTime; set => _isPartyTime = value; }

    void TogglePartyMode() => IsPartyTime = !IsPartyTime;

    void Start()
    {
        _partyToggleBtn.onClick.AddListener(TogglePartyMode);

        Texture2D firstEffectTexture = ApplyKernel(_sourceTexture, GetKernel(_firstEffect));
        Texture2D secondEffectTexture = ApplyKernel(firstEffectTexture, GetKernel(_secondEffect));
        Texture2D redifyTexture = Redify(secondEffectTexture);

        Texture2D combined;

        if (_experiment)
        {
            combined = Combine(redifyTexture, secondEffectTexture);
        }
        else
        {
            combined = Combine(redifyTexture, _sourceTexture);
        }

        Sprite _modifiedSprite = Sprite.Create(combined, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        _renderer.sprite = _modifiedSprite;
    }

    void Update()
    {
        if (IsPartyTime)
        {
            Color randomColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);

            Texture2D firstEffectTexture = ApplyKernel(_sourceTexture, GetKernel(_firstEffect));
            Texture2D secondEffectTexture = ApplyKernel(firstEffectTexture, GetKernel(_secondEffect));
            Texture2D discoTexture = DoDiscoOutline(secondEffectTexture, randomColor);

            Texture2D combined;
            if (_experiment)
            {
                combined = Combine(discoTexture, firstEffectTexture);
            }
            else
            {
                combined = Combine(discoTexture, _sourceTexture);
            }

            Sprite _modifiedSprite = Sprite.Create(combined, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
            _renderer.sprite = _modifiedSprite;
        }
    }

    float[,] GetKernel(string effect)
    {
        if (effect == "BOX_BLUR")
            return _boxBlurKernel;
        else if (effect == "GAUSSIAN_BLUR")
            return _gaussianBlurKernel;
        else if (effect == "SHARPEN")
            return _sharpenKernel;
        else if (effect == "RIDGE")
            return _ridgeDetectionKernel;
        else
            return _identityKernel;
    }

    //Texture2D ApplyKernel(Texture2D source, float[,] kernel)
    //{
    //    Texture2D mutated = new Texture2D(source.width, source.height);

    //    for (int y = 0; y < source.height; y++)
    //    {
    //        for (int x = 0; x < source.width; x++)
    //        {
    //            Color tl = source.GetPixel(x - 1, y + 1) * kernel[0, 0];
    //            Color t = source.GetPixel(x, y + 1) * kernel[0, 1];
    //            Color tr = source.GetPixel(x + 1, y + 1) * kernel[0, 2];
    //            Color l = source.GetPixel(x - 1, y) * kernel[1, 0];
    //            Color c = source.GetPixel(x, y) * kernel[1, 1];
    //            Color r = source.GetPixel(x + 1, y) * kernel[1, 2];
    //            Color bl = source.GetPixel(x - 1, y - 1) * kernel[2, 0];
    //            Color b = source.GetPixel(x, y - 1) * kernel[2, 1];
    //            Color br = source.GetPixel(x + 1, y - 1) * kernel[2, 2];

    //            Color combined = tl + t + tr + l + c + r + bl + b + br;
    //            c.a = 1.0f;

    //            mutated.SetPixel(x, y, combined);
    //        }
    //    }
    //    mutated.Apply();

    //    return mutated;
    //}

    Texture2D Redify(Texture2D source)
    {
        Texture2D mutated = new Texture2D(source.width, source.height);

        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                Color sourcePixel = source.GetPixel(x, y);

                Color mutatedPixel;
                if (sourcePixel.a > 0.1f)
                {
                    mutatedPixel = new Color(1.0f, 0.0f, 0.0f, sourcePixel.a);
                }
                else
                {
                    mutatedPixel = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                }

                mutated.SetPixel(x, y, mutatedPixel);
            }
        }
        mutated.Apply();

        return mutated;
    }

    Texture2D DoDiscoOutline(Texture2D source, Color applyColor)
    {
        Texture2D mutated = new Texture2D(source.width, source.height);

        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                Color sourcePixel = source.GetPixel(x, y);

                Color mutatedPixel;
                if (sourcePixel.a > 0.1f)
                {
                    mutatedPixel = new Color(applyColor.r, applyColor.g, applyColor.b, sourcePixel.a);
                }
                else
                {
                    mutatedPixel = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                }

                mutated.SetPixel(x, y, mutatedPixel);
            }
        }
        mutated.Apply();

        return mutated;
    }
}
