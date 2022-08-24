using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PP_Base : MonoBehaviour
{
    protected SpriteRenderer _renderer;
    protected Sprite _sourceSprite;
    protected Texture2D _sourceTexture;
    protected Texture2D _modifiedTexture;

    protected int _textureWidth;
    protected int _textureHeight;

    // Kernel Data
    // protected float[,] _kernel;

    protected readonly float[,] _identityKernel = new float[3, 3] { { 1.0f, 1.0f, 1.0f, },
                                                                    { 1.0f, 1.0f, 1.0f, },
                                                                    { 1.0f, 1.0f, 1.0f, }, };
    protected readonly float[,] _boxBlurKernel = new float[3, 3] { { 1.0f/9, 1.0f/9, 1.0f/9, },
                                                                   { 1.0f/9, 1.0f/9, 1.0f/9, },
                                                                   { 1.0f/9, 1.0f/9, 1.0f/9, } };
    protected readonly float[,] _gaussianBlurKernel = new float[3, 3] { { 1.0f/16, 2.0f/16, 1.0f/16, },
                                                                      { 2.0f/16, 4.0f/16, 2.0f/16, },
                                                                      { 1.0f/16, 2.0f/16, 1.0f/16, }, };
    protected readonly float[,] _ridgeDetectionKernel = new float[3, 3] { { -1.0f, -1.0f, -1.0f, },
                                                                { -1.0f, 8.0f, -1.0f, },
                                                                { -1.0f, -1.0f, -1.0f, }, };

    protected float[,] _sharpenKernel = new float[3, 3] { { 0f, -1.0f, 0f, },
                                                { -1.0f, 5.0f, -1.0f, },
                                                { 0f, -1.0f, 0f, }, };

    protected float[,] _9x9BoxBlur = new float[9, 9];

    [SerializeField] bool _isGlow = false;
    // [SerializeField] bool _isDebug = false;

    // Could I just use this value? ...as running through a kernel is pointless when all float values in the kernel are the same
    // float _blurValue9x9 = 1.0f / 81;

    [SerializeField] [Range(3, 9)] protected int _nRows = 3; // This is the number of rows the kernel will have. Same as columns for n * n kernel.
    int _previousFrameKernelWidth = 3;

    protected Texture2D GetKernelBlurSprite()
    {
        // Checks to avoid updating the blur texture if it is unecessary:
        // _nRows must be an odd number as it references all surrounding pixels from the pixel in memory.
        if (_nRows % 2 == 0) return _modifiedTexture; // if this means the number is even, then return.
        if (_nRows == _previousFrameKernelWidth) return _modifiedTexture; // only moves past this point if the _nRows value changes. 

        // Re-generate and update the _renderer.sprite
        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                int yBottomRow = y - ((_nRows - 1) / 2);
                int yTopRow = y + ((_nRows - 1) / 2);

                int xLeftColumn = x - ((_nRows - 1) / 2);
                int xRightColumn = x + ((_nRows - 1) / 2);

                int samples = 0;           // number of pixels sampled in the kernel
                Color sampleAdded = new Color(); // add all pixels in the kernel here
                for (int yOffset = yBottomRow; yOffset <= yTopRow; ++yOffset)
                {
                    for (int xOffset = xLeftColumn; xOffset <= xRightColumn; ++xOffset)
                    {
                        Color sample = _sourceTexture.GetPixel(xOffset, yOffset);
                        sampleAdded += sample;
                        samples += 1;
                    }
                }

                Color averaged = sampleAdded / samples;
                _modifiedTexture.SetPixel(x, y, averaged);
            }
        }
        _previousFrameKernelWidth = _nRows;

        _modifiedTexture.Apply();

        return _modifiedTexture;
    }

    protected virtual void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _sourceSprite = _renderer.sprite;
        _sourceTexture = _sourceSprite.texture;
        _textureWidth = _sourceTexture.width;
        _textureHeight = _sourceTexture.height;

        // _modifiedTexture = new Texture2D(_textureWidth, _textureHeight);

        // CREATING THE 9x9 BOX BLUR KERNEL
        //for (int y = 0; y < 9; y++)
        //{
        //    for (int x = 0; x < 9; x++)
        //    {
        //        _9x9BoxBlur[x, y] = 1.0f / 81;
        //        Debug.Log($"_9x9BoxBlur[{x}, {y}] = {_9x9BoxBlur[x, y]}");
        //    }
        //}
    }
    protected float ReturnGreatest(float a, float b, float c)
    {
        if (a > b)
            return a > c ? a : c; // return: a > c ? true = a : false = c;
        else
        {
            return b > c ? b : c;
        }
    }

    // Way too expensive
    protected Texture2D Apply9x9Kernel(Texture2D source, float[,] kernel)
    {
        _modifiedTexture = new Texture2D(_textureWidth, _textureHeight);

        int xOffset = 4;
        int yOffset = 4;

        for (int y = 0; y < _textureHeight; y++)
        {
            for (int x = 0; x < _textureWidth; x++)
            {
                Color pixel = source.GetPixel(x, y);
                Color added = pixel;

                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        added += source.GetPixel(x - xOffset, y + yOffset) * kernel[i, j];
                        Debug.Log($"i: {i}   ,   j: {j}");
                    }
                }
                
                added.a = pixel.a;

                // Color averaged = (topLeft + topMiddle + topRight + 
                //                  centreLeft + centreMiddle + centreRight + 
                //                  bottomLeft + bottomMiddle + bottomRight) / 9;

                //averaged.a = centreMiddle.a;

                _modifiedTexture.SetPixel(x, y, added);
            }
        }
        _modifiedTexture.Apply();

        return _modifiedTexture;

    }

    protected Texture2D ApplyKernel(Texture2D source, float [,] kernel)
    {
        _modifiedTexture = new Texture2D(_textureWidth, _textureHeight);

        // float[,] kernel

        for (int y = 0; y < _textureHeight; y++)
        {
            for (int x = 0; x < _textureWidth; x++)
            {
                Color pixel = source.GetPixel(x, y);

                // Row Above current pixel (top)
                // Fixed mistake from tutorial class top y value should be y + 1 not y - 1
                Color topLeft = source.GetPixel(x - 1, y + 1) * kernel[0, 0];
                Color topMiddle = source.GetPixel(x, y + 1) * kernel[0, 1];
                Color topRight = source.GetPixel(x + 1, y + 1) * kernel[0, 2];

                // Current Row of pixel (centre)
                Color centreLeft = source.GetPixel(x - 1, y) * kernel[1, 0];
                Color centreMiddle = source.GetPixel(x, y) * kernel[1, 1];
                Color centreRight = source.GetPixel(x + 1, y) * kernel[1, 2];

                // Row below current pixel (bottom)
                Color bottomLeft = source.GetPixel(x - 1, y - 1) * kernel[2, 0];
                Color bottomMiddle = source.GetPixel(x, y - 1) * kernel[2, 1];
                Color bottomRight = source.GetPixel(x + 1, y - 1) * kernel[2, 2];

                Color added = topLeft + topMiddle + topRight +
                              centreLeft + centreMiddle + centreRight +
                              bottomLeft + bottomMiddle + bottomRight;

                centreMiddle.a = 1.0f;
                //added.a = pixel.a;

                // Color averaged = (topLeft + topMiddle + topRight + 
                //                  centreLeft + centreMiddle + centreRight + 
                //                  bottomLeft + bottomMiddle + bottomRight) / 9;

                //averaged.a = centreMiddle.a;

                _modifiedTexture.SetPixel(x, y, added);

            }
        }
        _modifiedTexture.Apply();

        return _modifiedTexture;
    }

    protected Texture2D Combine(Texture2D a, Texture2D b)
    {
        Texture2D combined = new Texture2D(a.width, a.height);

        for (int y = 0; y < a.height; y++)
        {
            for (int x = 0; x < a.width; x++)
            {
                Color sourcePixelA = a.GetPixel(x, y);
                Color sourcePixelB = b.GetPixel(x, y);

                if (_isGlow)
                {
                    if (sourcePixelA.a == 0.0f)
                        combined.SetPixel(x, y, sourcePixelB);
                    else if (sourcePixelB.a  == 0.0f)
                        combined.SetPixel(x, y, sourcePixelA);
                    else
                        combined.SetPixel(x, y, sourcePixelA + sourcePixelB);
                }
                else
                    combined.SetPixel(x, y, sourcePixelA + sourcePixelB);
            }
        }
        combined.Apply();

        return combined;
    }
}
