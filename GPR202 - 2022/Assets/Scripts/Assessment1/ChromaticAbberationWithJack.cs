using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChromaticAbberationWithJack : MonoBehaviour
{
    SpriteRenderer _renderer;
    Sprite sourceSprite;
    Texture2D sourceTexture;
    //Texture2D mutatedTexture;
    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        sourceSprite = _renderer.sprite;
        sourceTexture = sourceSprite.texture;
        
    }

    void Update()
    {
        Texture2D redTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        Texture2D greenTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        Texture2D blueTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

        // Iterate over every pixel in the texture
        for (int y = 0; y < sourceTexture.height; ++y)
        {
            for(int x = 0; x < sourceTexture.width; ++x)
            {
                Color sourcePixel = sourceTexture.GetPixel(x, y);


                redTexture.SetPixel(x - (int)(10.0f * Mathf.Sin(Time.timeSinceLevelLoad)), y - (int)((10 * Mathf.Sin(Time.timeSinceLevelLoad))), new Color(sourcePixel.r, 0.0f, 0.0f, sourcePixel.a));
                greenTexture.SetPixel(x + (int)(10.0f * Mathf.Sin(Time.timeSinceLevelLoad)), y + (int)(10 * Mathf.Sin(Time.timeSinceLevelLoad)), new Color(0.0f, sourcePixel.g, 0.0f, sourcePixel.a));
                blueTexture.SetPixel(x + (int)(10.0f * Mathf.Sin(Time.timeSinceLevelLoad)), y - (int)(10.0f * Mathf.Sin(Time.timeSinceLevelLoad)), new Color(0.0f, 0.0f, sourcePixel.b, sourcePixel.a));
            }
        }
        redTexture.Apply();
        greenTexture.Apply();
        blueTexture.Apply();

        Texture2D combinedTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        for (int y = 0; y < sourceTexture.height; ++y)
        {
            for (int x = 0; x < sourceTexture.width; ++x)
            {
                Color redPixel = redTexture.GetPixel(x, y);
                Color greenPixel = greenTexture.GetPixel(x, y);
                Color bluePixel = blueTexture.GetPixel(x, y);

                Color finalPixel = redPixel + greenPixel + bluePixel;
                finalPixel.a = redPixel.a/3.0f + greenPixel.a/3.0f + bluePixel.a / 3.0f;

                combinedTexture.SetPixel(x, y, finalPixel);
            }
        }
        combinedTexture.Apply();

        Sprite newSprite = Sprite.Create(combinedTexture, sourceSprite.rect, new Vector2(0.5f, 0.5f));
        _renderer.sprite = newSprite;
    }
}
