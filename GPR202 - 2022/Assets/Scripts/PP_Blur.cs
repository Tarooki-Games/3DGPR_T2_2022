using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PP_Blur : PP_Base
{
    [SerializeField] string _effect = "BOX_BLUR";

    [SerializeField] Canvas _canvas;
    [SerializeField] Slider _sharpnessSlider;
    [SerializeField] TMP_Text _sharpValTMP;
    [SerializeField] Slider _ridgeSlider;
    [SerializeField] TMP_Text _ridgeValTMP;

    [SerializeField] bool _isSharpen = false;
    [SerializeField] private bool _isRidge;

    protected override void Awake()
    {
        base.Awake();

        _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        foreach (Slider s in _canvas.GetComponentsInChildren<Slider>())
        {
            if (s.name == "SharpnessSlider")
            {
                _sharpnessSlider = s;
                _sharpValTMP = s.GetComponentInChildren<TMP_Text>();
            }
            else if (s.name == "RidgeSlider")
            {
                _ridgeSlider = s;
                _ridgeValTMP = s.GetComponentInChildren<TMP_Text>();
            }
        }
    }

    void Start()
    {
        _renderer.sprite = Sprite.Create(ApplyKernel(_sourceTexture, GetKernel(_effect)), _sourceSprite.rect, new Vector2(0.5f, 0.5f));

        //Adds a listener to the main slider and invokes a method when the value changes.
        _sharpnessSlider.onValueChanged.AddListener(delegate { UpdateSprite(); });
        _ridgeSlider.onValueChanged.AddListener(delegate { UpdateSprite(); });
    }

    float[,] GetKernel(string effect)
    {
        if (effect == "BOX_BLUR")
            return _boxBlurKernel;
        else if (effect == "GAUSSIAN_BLUR")
            return _gaussianBlurKernel;
        else if (effect == "SHARPEN")
        {
            _isSharpen = true;
            _sharpValTMP.SetText($"{_sharpnessSlider.value}");
            return GetSharpenKernel();
        }
        else if (effect == "RIDGE")
        {
            _isRidge = true;
            _ridgeValTMP.SetText($"{_ridgeSlider.value}");
            return GetRidgeKernel();
        }
        else
            return _identityKernel;
    }

    public void UpdateSprite()
    {
        if (_isSharpen)
        {
            _renderer.sprite = Sprite.Create(ApplyKernel(_sourceTexture, GetKernel("SHARPEN")), _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        }
        if (_isRidge)
        {
            _renderer.sprite = Sprite.Create(ApplyKernel(_sourceTexture, GetKernel("RIDGE")), _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        }
    }

    float[,] GetSharpenKernel()
    {
        // Not Sure why but the _sharpenKernel values are being changed despite me never directly assigning to them.
        // Therefore, I need to reset them to it's base value before anything else.
        _sharpenKernel = new float[3, 3] { { 0f, -1.0f, 0f, },
                                           { -1.0f, 5, -1.0f, },
                                           { 0f, -1.0f, 0f, }, };

        float[,] kernel = _sharpenKernel;

        for (int i = 0; i <= 2; i++)
        {
            for (int j = 0; j <= 2; j++)
            {
                kernel[i, j] *= _sharpnessSlider.value;

                Debug.Log($"kernel Value: {kernel[i, j]}        _sharpenKernel: {_sharpenKernel[i,j]}");
            }
        }

        return kernel;
    }

    float[,] GetRidgeKernel()
    {
        _ridgeDetectionKernel[1, 1] = 4 + _ridgeSlider.value;

        for (int i = 0; i <= 2; i++)
        {
            for (int j = 0; j <= 2; j++)
            {
                Debug.Log($"_ridgeKernel: {_ridgeDetectionKernel[i, j]}");
            }
        }
        
        return _ridgeDetectionKernel;
    }
}
