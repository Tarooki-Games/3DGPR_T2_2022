using System.Collections;
using UnityEngine;

public class PostProcessing : PP_Base
{
    Sprite _grayScaledSprite;
    Sprite _brightenedSprite;
    
    readonly float Tau = 2 * Mathf.PI;

    bool _inputAllowed = true;

    [SerializeField] int _allowance = 10;
    [SerializeField] float _intensityOnClick = 0.5f;
    [SerializeField] float _bufferIntensity = 10.0f;

    [SerializeField] bool _hasHealthBar;

    [SerializeField][Range(0.0f, 1.0f)]
    float _health = 1.0f;

    // const float MAX_HEALTH = 100;
    // [SerializeField][Range(0.0f, MAX_HEALTH)]

    int _bufferRange = 10;
    int _startingBufferRange;
    
    float _limitEnd = 10;
    
    [SerializeField] float _amplitude = 10;
    [SerializeField] float _frequency = 1;
    [SerializeField] float _movementSpeed = 1;

    Vector3 _endPos;

    // Enemy Death Variables
    [SerializeField] bool _enemyIsDying = false;
    [SerializeField] bool _enemyIsDead = false;

    [SerializeField] float _lerpTime = 0.00001f;

    protected override void Awake()
    {
        base.Awake();

        _textureHeight += _allowance;

        _startingBufferRange = _bufferRange;
    }

    void Start()
    {
        // Create and store the sprites in Start()
        _grayScaledSprite = CreateSprite("GRAYSCALE");
        _brightenedSprite = CreateSprite("BRIGHTEN");
        _endPos = new Vector3(transform.position.x, -7.0f, transform.position.z);

    }

    void Update()
    {
        if (_inputAllowed)
        {
            // HEALTH BAR ACTIONS
            if (_hasHealthBar)
            {
                if (Input.GetKeyDown(KeyCode.D)) // D for Damage
                    _health += 0.05f;
                if (Input.GetKeyDown(KeyCode.H)) // H for Heal
                    _health -= 0.05f;
            }

            // MOUSE INPUT BOOLEANS (Checks for held button)
            bool leftMBIsHeld = Input.GetMouseButton(0);
            bool rightMBIsHeld = Input.GetMouseButton(1);

            if (!leftMBIsHeld && !rightMBIsHeld)  // Neither MouseButton is held: default setting
            {
                EnemyPostProcessing();
            }
            else if (rightMBIsHeld && !leftMBIsHeld)
            {
                _renderer.sprite = _grayScaledSprite;
            }
            else if (!rightMBIsHeld && leftMBIsHeld)
            {
                _renderer.sprite = _brightenedSprite;
            }
            else
            {
                _renderer.sprite = _sourceSprite;
            }
        }

        if (_health >= 0.95f && !_enemyIsDying)
        {
            _enemyIsDying = true;
            _inputAllowed = false;
            StartCoroutine(EnemyDeath());
        }

        if (_enemyIsDead && transform.position.y >= -6.5f)
        {
            transform.position = Vector3.Lerp(transform.position, _endPos, _lerpTime * Time.deltaTime);
        }
    }

    IEnumerator EnemyDeath()
    {
        for (int i = 0; i <= 3; i++)
        {
            _renderer.sprite = _grayScaledSprite;
            yield return new WaitForSeconds(.1f);
            _renderer.sprite = _brightenedSprite;
            yield return new WaitForSeconds(.1f);
        }
        _enemyIsDead = true;
    }

    Sprite CreateSprite(string ppEffect)
    {
        Texture2D modifiedTexture = new Texture2D(_textureWidth, _textureHeight);

        for (int y = 0; y < _textureHeight; y++)
        {
            for (int x = 0; x < _textureWidth; x++)
            {
                Color pixel = _sourceTexture.GetPixel(x, y);

                if (pixel.a >= 0.8f)
                {
                    bool isBlackPixel = pixel.r <= 0.1f && pixel.g <= 0.1f && pixel.b <= 0.1f;
                    if (isBlackPixel)
                    {
                        modifiedTexture.SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f, pixel.a));
                    }
                    else
                    {
                        if (ppEffect == "GRAYSCALE")
                        {
                            // GRAYSCALE
                            float grayScaledPixel = ReturnGreatest(pixel.r, pixel.g, pixel.b);
                            Color brightenedGrayScaledPixel = new Color(grayScaledPixel, grayScaledPixel, grayScaledPixel);
                            modifiedTexture.SetPixel(x, y, brightenedGrayScaledPixel);
                        }
                        else if (ppEffect == "BRIGHTEN")
                        {
                            // BRIGHTEN
                            Color brightenedPixel = pixel;
                            brightenedPixel.r += 0.1f;
                            brightenedPixel.g += 0.1f;
                            brightenedPixel.b += 0.1f;
                            brightenedPixel *= _intensityOnClick;
                            modifiedTexture.SetPixel(x, y, brightenedPixel);
                        }
                    }
                }
                else
                {
                    modifiedTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }
        modifiedTexture.Apply();
        Sprite modifiedSprite = Sprite.Create(modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        return modifiedSprite;
    }

    void EnemyPostProcessing()
    {
        if (_health <= 0.02)
        {
            _health = 0;
            _bufferRange = 0;
        }
        else
        {
            _bufferRange = _startingBufferRange;
            if (_health >= 1)
                _health = 1;
        }

        Texture2D modifiedTexture = new Texture2D(_textureWidth, _textureHeight);
        int lastRowToRenderRed = (int)(_textureHeight * _health);
        float lastRowToRenderRedInBuffer = lastRowToRenderRed + _bufferRange;
        float firstRowToRenderRedInBuffer = lastRowToRenderRed - _bufferRange;

        for (int y = 0; y < _textureHeight + _allowance; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                float progress = (float)x / (_textureWidth - 1);
                float lerpX = Mathf.Lerp(0, _limitEnd, progress);

                float sinY = Mathf.Sin((Tau * _frequency * lerpX) + (Time.timeSinceLevelLoad * _movementSpeed));
                int yFinal = y + (int)(sinY * _amplitude);

                Color pixel = _sourceTexture.GetPixel(x, y);

                bool pixelIsVisible = pixel.a >= 0.8f;

                if (!_hasHealthBar)
                {
                    modifiedTexture.SetPixel(x, yFinal, !pixelIsVisible ? new Color(0, 0, 0, 0) : pixel);
                }
                else
                {
                    if (pixelIsVisible)
                    {
                        bool isInBuffer = y > firstRowToRenderRedInBuffer + 1 && y < lastRowToRenderRedInBuffer - 1;
                        float greatest = ReturnGreatest(pixel.r, pixel.g, pixel.b);
                        
                        if (isInBuffer)
                        {
                            Color healthyPixel = pixel;
                            healthyPixel.r = greatest;
                            healthyPixel.g = 0.0f;
                            healthyPixel.b = 0.0f;
                            
                            // Pixel y checks to blend into image either side of the health bar
                            if (y > lastRowToRenderRed)
                                _bufferIntensity = -(y - lastRowToRenderRed - _bufferRange) / (_bufferRange/5.0f);
                            else if (y <= lastRowToRenderRed)
                                _bufferIntensity = (y - lastRowToRenderRed + _bufferRange) / (_bufferRange/4.0f);
                            if (_bufferIntensity < _intensityOnClick)
                                _bufferIntensity = _intensityOnClick;
                            healthyPixel *= _bufferIntensity;
                            
                            //Debug.Log(sinY + "   ,   " + sinYInt);
                            modifiedTexture.SetPixel(x, yFinal, healthyPixel);
                        }
                        else
                        {
                            bool pixelIsAboveHealthBar = y > lastRowToRenderRed;
                            if (pixelIsAboveHealthBar)
                            {
                                modifiedTexture.SetPixel(x, yFinal, pixel);
                            }
                            else
                            {
                                Color healthyPixel = pixel;
                                healthyPixel.r = greatest;
                                healthyPixel.g = 0.0f;
                                healthyPixel.b = 0.0f;
                                healthyPixel *= _intensityOnClick;
                                modifiedTexture.SetPixel(x, yFinal, healthyPixel);
                            }
                        }
                    }
                    else
                    {
                        modifiedTexture.SetPixel(x, yFinal, new Color(0, 0, 0, 0));
                    }
                }
            }
        }

        modifiedTexture.Apply();
        Sprite modifiedSprite = Sprite.Create(modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        _renderer.sprite = modifiedSprite;
    }
}