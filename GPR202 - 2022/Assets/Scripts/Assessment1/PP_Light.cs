using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PP_Light : PP_Base
{
    float secondsUntilNextRender = 0.0f;
    [SerializeField] float secondsBetweenRenders = 0.05f;
    
    // [SerializeField] float _intensityOnClick = 0.5f;
    [SerializeField] float _frequency = 3.0f;
    [SerializeField] float _brightnessThreshold = 0.5f;
    [SerializeField] float _intensity = 0.125f;
    [SerializeField] float _powerOf = 3.0f;

    void Update()
    {
        // Do Not Render every frame, render once every now and then
        secondsUntilNextRender -= Time.deltaTime;
        if (secondsUntilNextRender > 0.0f)
            return;
        secondsUntilNextRender = secondsBetweenRenders;

        // Used cosine instead of sine and changed -1.6f for half of Pi to be exactly at 0, 0 and also removing a variable.
        // previous waveFormula = (Mathf.Sin(Time.timeSinceLevelLoad - (Mathf.PI/2)) + 1) * _intensity: 
        float waveFormula = (Mathf.Cos(_frequency * Time.timeSinceLevelLoad + Mathf.PI) + 1) * _intensity;

        _renderer.sprite = CreateSprite(waveFormula);
    }

    Sprite CreateSprite(float waveFormula)
    {
        _modifiedTexture = new Texture2D(_textureWidth, _textureHeight);

        for (int y = 0; y < _textureHeight; y++)
        {
            for (int x = 0; x < _textureWidth; x++)
            {
                Color pixel = _sourceTexture.GetPixel(x, y);
                // Color modifiedPixel = new Color(waveFormula + pixel.r, waveFormula + pixel.g, waveFormula + pixel.b, pixel.a);
                // _modifiedTexture.SetPixel(x, y, modifiedPixel);

                // Purely Mathematical Implementation of the below if else statement :O
                Color modifiedPixel = pixel;
                modifiedPixel.r += _brightnessThreshold + waveFormula;
                modifiedPixel.g += _brightnessThreshold + waveFormula;
                modifiedPixel.b += _brightnessThreshold + waveFormula;
                modifiedPixel.r = Mathf.Pow(modifiedPixel.r, _powerOf);
                modifiedPixel.g = Mathf.Pow(modifiedPixel.g, _powerOf);
                modifiedPixel.b = Mathf.Pow(modifiedPixel.b, _powerOf);
                modifiedPixel.r -= _brightnessThreshold + waveFormula;
                modifiedPixel.g -= _brightnessThreshold + waveFormula;
                modifiedPixel.b -= _brightnessThreshold + waveFormula;

                _modifiedTexture.SetPixel(x, y, modifiedPixel);

                /*
                // TO DO: Research Processor Branching
                bool isPixelBright = (pixel.r + pixel.g + pixel.b) / 3 > _brightnessThreshold;
                if(isPixelBright)
                {
                    Color modifiedPixel = new Color(pixel.r + waveFormula, pixel.g + waveFormula, pixel.b + waveFormula, pixel.a);
                    _modifiedTexture.SetPixel(x, y, modifiedPixel);
                }
                else
                {
                    Color modifiedPixel = new Color(pixel.r - waveFormula, pixel.g - waveFormula, pixel.b - waveFormula, pixel.a);
                    _modifiedTexture.SetPixel(x, y, modifiedPixel);
                }
                */
            }
        }
        _modifiedTexture.Apply();

        Sprite _modifiedSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        return _modifiedSprite;
    }
}
