using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainImageGenerator : TerrainGenerator_Base
{
    float _timeUntilNextGen = 2.0f;
    float _timeBetweenGens = 2.0f;

    [SerializeField] bool _filter;
    [SerializeField] bool _readable;
    [SerializeField] bool _format;
    
    [SerializeField] Color _grass;     // 0.0 - 0.4
    //[SerializeField] Color _swamp;   // 
    [SerializeField] Color _forest;    // 0.4 - 0.7
    [SerializeField] Color _lava;      // 0.7 - 0.8
    [SerializeField] Color _dungeon;   // 0.8 - 0.9 
    [SerializeField] Color _abyss;     // 0.9 - 1.0

    const int ZONE_LAYER = 0;
    const int RANDOM_ZONE_LAYER = 1;

    [SerializeField] int _seed = 42069;
    [SerializeField] bool _clean;

    float Noise(int x, int y, int layer, int seed)
    {
        Random.seed = 1000000;
        int randomValue1 = (int)Random.Range(1.0f, 10000.0f);
        Random.seed = 232323;
        int randomValue2 = (int)Random.Range(1.0f, 10000.0f);
        Random.seed = 98989989;
        int randomValue3 = (int)Random.Range(1.0f, 10000.0f);
        Random.seed = 123123123;
        int randomValue4 = (int)Random.Range(1.0f, 10000.0f);

        Random.seed = (x + randomValue1) * (y + randomValue2) * (layer + randomValue3) * (seed + randomValue4);
        float noise = Random.Range(0.0f, 0.999999999999999f);
        return noise;
    }

    Color NoiseToZone(float noiseValue)
    {
        if (noiseValue < 0.4f) // 25%
            return _grass;
        else if (noiseValue < 0.7f) // 20%
            return _forest;
        else if (noiseValue < 0.8f) // 25%
            return _lava;
        else if (noiseValue < 0.9f) // 15%
            return _dungeon;
        else // 10%
            return _abyss;
    }

    //void Start()
    //{
    //    GenerateBasicZones();

    //    Sprite terrainSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
    //    _renderer.sprite = terrainSprite;

    //    if (_clean)
    //    {
    //        RemoveDiagonalSingleCellZones();

    //        // _timeUntilNextGen = _timeBetweenGens;

    //        terrainSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
    //        _renderer.sprite = terrainSprite;
    //    }
    //}

    void Update()
    {
        _timeUntilNextGen -= Time.deltaTime;
        if (_timeUntilNextGen > 0.0f) return;

        _seed = (int)Random.Range(0, 1000000.0f);

        _timeUntilNextGen = _timeBetweenGens;

        GenerateBasicZones();

        // RemoveDiagonalSingleCellZones();

        Sprite terrainSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        _renderer.sprite = terrainSprite;

        // Note: if file exists at path, it will overwrite.
        _ = AssetCreator.CreateTexture2D(_modifiedTexture, $"{_folderPath}{_fileName}.bmp", _filter, _readable, _format);
    }

    void GenerateBasicZones()
    {
        // Generate original zone type
        float originalZoneType = Noise(0, 0, ZONE_LAYER, _seed);
        _modifiedTexture.SetPixel(0, 0, NoiseToZone(originalZoneType));

        for (int y = 0; y < _modifiedTexture.height; ++y)
        {
            for (int x = 0; x < _modifiedTexture.width; ++x)
            {
                if (x == 0 && y == 0) continue; // skip the original zone.

                Color l = _modifiedTexture.GetPixel(x - 1, y);
                Color bl = _modifiedTexture.GetPixel(x - 1, y - 1);
                Color b = _modifiedTexture.GetPixel(x, y - 1);
                Color br = _modifiedTexture.GetPixel(x + 1, y - 1);

                float randomZoneType = Noise(x, y, ZONE_LAYER, _seed);
                Color randomZoneColor = NoiseToZone(randomZoneType);

                Color randomlyChosen = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                while (randomlyChosen == new Color(0.0f, 0.0f, 0.0f, 0.0f))
                {
                    Color[] bagOfZones = { l, bl, b, br, l, bl, b, br, l, bl, b, br, l, bl, b, br, randomZoneColor };
                    float randomNumber = Noise(x, y, RANDOM_ZONE_LAYER, _seed);
                    int randomIndex = (int)Mathf.Floor(randomNumber * bagOfZones.Length);
                    randomlyChosen = bagOfZones[randomIndex];
                }
                _modifiedTexture.SetPixel(x, y, randomlyChosen);
            }
        }
        _modifiedTexture.filterMode = FilterMode.Point;

        _modifiedTexture.Apply();
    }

    void RemoveDiagonalSingleCellZones()
    {
        // print(_modifiedTexture.GetPixel(0, 0));

        // remove all tiles that do not have neighbouring tiles of the same type.
        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                //Color tl = _modifiedTexture.GetPixel(x - 1, y + 1);
                Color t = _modifiedTexture.GetPixel(x, y + 1);
                //Color tr = _modifiedTexture.GetPixel(x + 1, y + 1);
                Color l = _modifiedTexture.GetPixel(x - 1, y);
                Color c = _modifiedTexture.GetPixel(x, y);
                Color r = _modifiedTexture.GetPixel(x + 1, y);
                //Color bl = _modifiedTexture.GetPixel(x - 1, y - 1);
                Color b = _modifiedTexture.GetPixel(x, y - 1);
                //Color br = _modifiedTexture.GetPixel(x + 1, y - 1);

                if (c == t || c == l || c == r || c == b)
                    continue;

                _modifiedTexture.SetPixel(x, y, l);
            }
        }
        _modifiedTexture.Apply();
    }

    private void OnDisable()
    {
        // Note: if file exists at path, it will overwrite.
        AssetCreator.CreateTexture2D(_modifiedTexture, $"{_folderPath}{_fileName}.bmp", _filter, _readable, _format);
    }
}


/*
 * 
                int matchCount = 0;
                if (c == t)
                    matchCount++;
                if (c == l)
                    matchCount++;
                if (c == r)
                    matchCount++;
                if (c == b)
                    matchCount++;

                if (matchCount >= 2)
                    continue;

 * 
*/


//#########################//
//#                       #//
//#   CODE OF CONFUSION   #//
//#                       #//
//#########################//

// Why this failed to work when I feel like it was exactly the same as yours?
// Followed tutoril at half speed to check it too lol

//float originalZoneType = Noise(0, 0, ZONE_LAYER, _seed);
//_modifiedTexture.SetPixel(0, 0, NoiseToZone(originalZoneType));

//for (int y = 0; y < _textureHeight; y++)
//{
//    for (int x = 0; x < _textureWidth; x++)
//    {
//        if (x == 0 && y == 0)
//            continue; // skip pixel(0, 0)

//        // Row Above current pixel (top)
//        // Fixed mistake from tutorial class top y value should be y + 1 not y - 1

//        // Current pixel (centre)
//        Color centreLeft   = _modifiedTexture.GetPixel(x - 1, y);
//        Color bottomLeft   = _modifiedTexture.GetPixel(x - 1, y - 1);
//        Color bottomMiddle = _modifiedTexture.GetPixel(x    , y - 1);
//        Color bottomRight  = _modifiedTexture.GetPixel(x + 1, y - 1);

//        float randomZoneType = Noise(x, y, ZONE_LAYER, _seed);
//        Color randomZoneColor = NoiseToZone(randomZoneType);

//        Color randomlyChosen = new Color(0.0f, 0.0f, 0.0f, 0.0f);
//        while (randomlyChosen == new Color(0.0f, 0.0f, 0.0f, 0.0f))
//        {
//            // Love this name XD
//            Color[] bagOfZones = { centreLeft, bottomLeft, bottomMiddle, bottomRight, randomZoneColor };
//            float randomNumber = Noise(x, y, RANDOM_ZONE_LAYER, _seed);
//            int randomIndex = (int)Mathf.Floor(randomNumber * bagOfZones.Length);
//            randomlyChosen = bagOfZones[randomIndex];
//        }

//        _modifiedTexture.SetPixel(x, y, randomlyChosen);
//    }
//}

//// This will overwrite the _sourceTexture in Assets/... Folder
//_modifiedTexture.Apply();



/*
Color c = _modifiedTexture.GetPixel(x, y);
Color l = _modifiedTexture.GetPixel(x - 1, y);
int matchCount = 0;
for (int i = -1; i <= 1; i++)
{
    for (int j = -1; j <= 1; j++)
    {
        if (c == _modifiedTexture.GetPixel(x + i, y + j))
            matchCount++;

        if (matchCount >= 3)
            break;
    }
    if (matchCount >= 3)
        break;
}

if (matchCount >= 3)
    continue;
else
    _modifiedTexture.SetPixel(x, y, l);
*/