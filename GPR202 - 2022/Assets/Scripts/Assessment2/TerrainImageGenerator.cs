using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainImageGenerator : TerrainGenerator_Base
{
    [Header("TerrainImageGenerator.cs   VARIABLES")]

    [SerializeField] protected bool _filter;
    [SerializeField] protected bool _readable;
    [SerializeField] protected bool _format;

    [SerializeField] protected Color _grass;     // 0.0 - 0.4
    //[SerializeField] Color _swamp;   // 
    [SerializeField] protected Color _forest;    // 0.4 - 0.7
    [SerializeField] protected Color _lava;      // 0.7 - 0.8
    [SerializeField] protected Color _dungeon;   // 0.8 - 0.9 
    [SerializeField] protected Color _abyss;     // 0.9 - 1.0

    [SerializeField] protected Color _water;
    [SerializeField] protected Color _waterBank;

    [SerializeField] protected bool _generateBasicZones;
    [SerializeField] protected bool _clean;
    [SerializeField] protected bool _isBlurry;
    [SerializeField] [Range(3.0f, 9.0f)] protected float _blurStrength = 3;
    [SerializeField] protected bool _hasRivers;
    [SerializeField] protected int _totalRivers;
    [SerializeField] int _riverAddedWidth = 1;
    [SerializeField] bool _hasWaterBanks;
    [SerializeField] int _waterBanksWidth = 3;
    [SerializeField] bool _hasForest;
    [SerializeField] int _forestWidth = 7;

    [SerializeField] protected bool _hasReachedCenter;

    protected const int ZONE_LAYER = 0;
    protected const int RANDOM_ZONE_LAYER = 1;
    protected const int RIVER_LAYER = 2;

    [SerializeField] protected int _outerRings = 1;
    [SerializeField] protected int _minSurroundingMatches = 2;

    // LinkedList<GeneratedRiverCell> generatedRiverCells = new LinkedList<GeneratedRiverCell>();


    protected float Noise(int x, int y, int layer, int seed)
    {
        Random.InitState(1000000); // Random.InitState(42);
        int randomValue1 = (int)Random.Range(1.0f, 10000.0f);
        Random.InitState(232323);
        int randomValue2 = (int)Random.Range(1.0f, 10000.0f);
        Random.InitState(98989989);
        int randomValue3 = (int)Random.Range(1.0f, 10000.0f);
        Random.InitState(123123123);
        int randomValue4 = (int)Random.Range(1.0f, 10000.0f);

        Random.InitState((x + randomValue1) * (y + randomValue2) * (layer + randomValue3) * (seed + randomValue4));
        float noise = Random.Range(0.0f, 0.999999999999999f);
        return noise;
    }

    protected Color NoiseToZone(float noiseValue)
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

    protected void GenerateBasicZones()
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
                    if (x == 0 || y == 0)
                    {
                        randomlyChosen = randomZoneColor;
                    }
                    else
                    {
                        Color[] bagOfZones = { l, bl, b, br, l, bl, b, br, randomZoneColor, l, bl, b, br, l, bl, b, br };
                        float randomNumber = Noise(x, y, RANDOM_ZONE_LAYER, _seed);
                        int randomIndex = (int)Mathf.Floor(randomNumber * (bagOfZones.Length - 1));
                        randomlyChosen = bagOfZones[randomIndex];
                    }
                }
                _modifiedTexture.SetPixel(x, y, randomlyChosen);
            }
        }
        _modifiedTexture.filterMode = FilterMode.Point;

        _modifiedTexture.Apply();
    }

    protected void CleanUpZones(int outerRings, int minMatches, bool finalPass)
    {
        // print(_modifiedTexture.GetPixel(0, 0));

        // remove all tiles that do not have neighbouring tiles of the same type.

        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                int matchCount = 0;
                Color c = _modifiedTexture.GetPixel(x, y);
                if (c == _water)
                    continue;
                // Iterates through surrounding tiles
                for (int i = -outerRings; i <= outerRings; ++i)
                {
                    for (int j = -outerRings; j <= outerRings; ++j)
                    {
                        if (i == 0 && j == 0)
                            continue;
                        if (c == _modifiedTexture.GetPixel(x + i, y + j))
                        {
                            // checks surrounding tiles for matches
                            matchCount++;
                            if (matchCount >= minMatches)
                                break;
                        }
                    }
                    if (matchCount >= minMatches)
                        break;
                }
                // once it has iterated thrugh all tiles or found the minimum matches it will...
                if (matchCount >= minMatches)
                    continue; // either continue or...
                else // set the pixel to a new pixel from one of the 3 pixels in the row below
                {
                    if (!finalPass)
                        _modifiedTexture.SetPixel(x, y, _modifiedTexture.GetPixel(x + (int)Random.Range(-1.0f, 1.0f), y - 1));
                    else
                        _modifiedTexture.SetPixel(x, y, _modifiedTexture.GetPixel(x - 1, y));
                }
            }
        }
        _modifiedTexture.Apply();
    }

    protected void BlurTerrain(int kernelWidth)
    {
        if (kernelWidth % 2 == 0) return; // this means the number is event, ignore.

        Texture2D blurTexture = new Texture2D(_textureWidth, _textureHeight);

        // Re-generate
        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                int yBottomRow = y - ((kernelWidth - 1) / 2);
                int yTopRow = y + ((kernelWidth - 1) / 2);

                int xLeftColumn = x - ((kernelWidth - 1) / 2);
                int xRightColumn = x + ((kernelWidth - 1) / 2);

                int samples = 0;           // number of pixels sampled in the kernel
                Color sampleAdded = new Color(); // add all pixels in the kernel here
                for (int yOffset = yBottomRow; yOffset <= yTopRow; ++yOffset)
                {
                    for (int xOffset = xLeftColumn; xOffset <= xRightColumn; ++xOffset)
                    {
                        Color sample = _modifiedTexture.GetPixel(xOffset, yOffset);
                        sampleAdded += sample;
                        samples += 1;
                    }
                }

                Color averaged = sampleAdded / samples;
                blurTexture.SetPixel(x, y, averaged);
            }
        }
        blurTexture.Apply();

        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                _modifiedTexture.SetPixel(x, y, blurTexture.GetPixel(x, y));
            }
        }
        _modifiedTexture.Apply();
    }

    protected struct GeneratedRiverCell
    {
        public int x;
        public int y;
        public int direction;
    };

    protected void GenerateRiver(int offset)
    {
        LinkedList<GeneratedRiverCell> generatedRiverCells = new LinkedList<GeneratedRiverCell>();

        int seed = 362122325 + offset;

        // Generate a random start pixel on a random edge.

        // 0 is up
        int upOffsetX = 0;
        int upOffsetY = 0;
        // 1 is up diag
        int fwdUpDiagOffsetX = 0;
        int fwdUpDiagOffsetY = 0;
        // 2 is down diag
        int fwdDownDiagOffsetX = 0;
        int fwdDownDiagOffsetY = 0;
        // 3 is fwd
        int fwdOffsetX = 0;
        int fwdOffsetY = 0;

        int randomX = 0;
        int randomY = 0;
        float randomEdge = Noise(0, 0, RIVER_LAYER, seed);

        if (randomEdge < 0.25f) // LHS
        {
            upOffsetX = 0;
            upOffsetY = 1;
            fwdUpDiagOffsetX = 1;
            fwdUpDiagOffsetY = 1;
            fwdOffsetX = 1;
            fwdOffsetY = 0;
            fwdDownDiagOffsetX = 1;
            fwdDownDiagOffsetY = -1;

            int edgeLength = _textureHeight;
            randomX = 0;

            float noise = Noise(1, 1, RIVER_LAYER, seed);
            // 0 -> 0.99999

            float scaled = noise * edgeLength;
            // 0 -> 63.9999999

            float floor = Mathf.Floor(scaled);
            // 0 -> 63

            randomY = (int)floor;
        }
        else if (randomEdge < 0.75f) // RHS
        {
            upOffsetX = 0;
            upOffsetY = -1;

            fwdUpDiagOffsetX = -1;
            fwdUpDiagOffsetY = -1;

            fwdOffsetX = -1;
            fwdOffsetY = 0;

            fwdDownDiagOffsetX = -1;
            fwdDownDiagOffsetY = 1;

            int edgeLength = _textureHeight;
            randomX = _textureWidth - 1;

            float noise = Noise(1, 1, RIVER_LAYER, seed);
            // 0 -> 0.99999

            float scaled = noise * edgeLength;
            // 0 -> 63.9999999

            float floor = Mathf.Floor(scaled);
            // 0 -> 63

            randomY = (int)floor;
        }
        else if (randomEdge < 0.5f) // TOP
        {
            upOffsetX = 1;
            upOffsetY = 0;

            fwdUpDiagOffsetX = 1;
            fwdUpDiagOffsetY = -1;

            fwdOffsetX = 0;
            fwdOffsetY = -1;

            fwdDownDiagOffsetX = -1;
            fwdDownDiagOffsetY = -1;

            int edgeLength = _textureWidth;
            randomY = _textureHeight - 1;

            float noise = Noise(1, 1, RIVER_LAYER, seed);
            // 0 -> 0.99999

            float scaled = noise * edgeLength;
            // 0 -> 63.9999999

            float floor = Mathf.Floor(scaled);
            // 0 -> 63

            randomX = (int)floor;
        }
        else if (randomEdge < 1.0f) // BOT
        {
            upOffsetX = 1;
            upOffsetY = 0;

            fwdUpDiagOffsetX = 1;
            fwdUpDiagOffsetY = 1;

            fwdOffsetX = 0;
            fwdOffsetY = 1;

            fwdDownDiagOffsetX = -1;
            fwdDownDiagOffsetY = 1;

            int edgeLength = _textureWidth;
            randomY = 0;

            float noise = Noise(1, 1, RIVER_LAYER, seed);
            // 0 -> 0.99999

            float scaled = noise * edgeLength;
            // 0 -> 63.9999999

            float floor = Mathf.Floor(scaled);
            // 0 -> 63

            randomX = (int)floor;
        }

        // Generate random direction.
        {
            /*
            float random = Noise(2, 2, RIVER_LAYER, seed);
            // 0 -> 0.999
            float scaled = random * 5;
            // 0 -> 4.9999
            float floor = Mathf.Floor(scaled);
            // 0 -> 4
            int randomDirection = (int)floor;
            */


        }

        GeneratedRiverCell first = new GeneratedRiverCell
        {
            x = randomX,
            y = randomY,
            direction = 2
        };
        generatedRiverCells.AddLast(first);

        GeneratedRiverCell second = new GeneratedRiverCell
        {
            x = randomX + fwdOffsetX,
            y = randomY + fwdOffsetY,
            direction = 2
        };
        generatedRiverCells.AddLast(second);

        // print(first.x);
        // print(first.y);
        // print(second.x);
        // print(second.y);

        // 3. be a drunk emu
        GeneratedRiverCell current = second;
        int i = 0;
        bool hasReachedCenter = false;
        while (current.x != 0 && current.x < _textureWidth && current.y != 0 && current.y < _textureHeight)
        {
            ++i;

            GeneratedRiverCell previous = generatedRiverCells.Last.Value;
            current = new GeneratedRiverCell();

            // Generate random direction.
            float random = Noise(2 + i, 2 + i, RIVER_LAYER, seed);
            // 0 -> 0.999

            float scaled;

            int quarterTexture = _textureWidth / 4;

            if (!hasReachedCenter && (current.x <= quarterTexture || current.x >= _textureWidth - quarterTexture || current.y <= quarterTexture || current.y >= _textureWidth - quarterTexture))
                scaled = random * 5;
            else
            {
                hasReachedCenter = true;
                scaled = random * 7;
            }

            float floor = Mathf.Floor(scaled);

            int randomDirection = (int)floor;

            current.direction = randomDirection;

            if (randomDirection == 0) // UP
            {
                current.x += previous.x + upOffsetX;
                current.y += previous.y + upOffsetY;
            }
            else if (randomDirection == 1) // FwdUpDiag
            {
                current.x += previous.x + fwdUpDiagOffsetX;
                current.y += previous.y + fwdUpDiagOffsetY;
            }
            else if (randomDirection == 2) // FWD
            {
                current.x += previous.x + fwdOffsetX;
                current.y += previous.y + fwdOffsetY;
            }
            else if (randomDirection == 3) // FwdDownDiag
            {
                current.x += previous.x + fwdDownDiagOffsetX;
                current.y += previous.y + fwdDownDiagOffsetY;
            }
            else if (randomDirection == 4) // DOWN
            {
                current.x += previous.x - upOffsetX;
                current.y += previous.y - upOffsetY;
            }
            else if (randomDirection == 5) // BwdDownDiag
            {
                current.x += previous.x - fwdUpDiagOffsetX;
                current.y += previous.y - fwdUpDiagOffsetY;
            }
            else if (randomDirection == 6) // BwdUpDiag
            {
                current.x += previous.x - fwdDownDiagOffsetX;
                current.y += previous.y - fwdDownDiagOffsetY;
            }

            print(current.x);
            print(current.y);

            generatedRiverCells.AddLast(current);
        }

        print(generatedRiverCells.Count);

        foreach (GeneratedRiverCell generatedRiverCell in generatedRiverCells)
        {
            for (int j = -_riverAddedWidth; j <= _riverAddedWidth; ++j)
            {
                for (int k = -_riverAddedWidth; k <= _riverAddedWidth; ++k)
                {
                    _modifiedTexture.SetPixel(generatedRiverCell.x + j, generatedRiverCell.y + k, _water);
                }
            }


            // int riverBankPixelDistance = _riverBedWidth + _riverAddedWidth;

            //for (int j = -riverBankPixelDistance; j <= riverBankPixelDistance; ++j)
            //{
            //    int xjCurrent = generatedRiverCell.x + j;
            //    for (int k = -riverBankPixelDistance; k <= riverBankPixelDistance; ++k)
            //    {
            //        int ykCurrent = generatedRiverCell.y + k;
            //        if (ykCurrent >= _textureWidth || xjCurrent >= _textureHeight || ykCurrent < 0 || xjCurrent < 0)
            //        {
            //            continue;
            //        }
            //        if (riverTexture2D.GetPixel(xjCurrent, ykCurrent) != _water)
            //        {
            //            if ((Mathf.Abs(j) <= _riverAddedWidth && Mathf.Abs(k) <= _riverAddedWidth))
            //                riverTexture2D.SetPixel(xjCurrent, ykCurrent, _water);
            //            else if (Mathf.Abs(j) > _riverAddedWidth && Mathf.Abs(k) > _riverAddedWidth)
            //                riverTexture2D.SetPixel(xjCurrent, ykCurrent, _waterBank);
            //            else
            //                continue;
            //        }
            //        continue;
            //    }
            //}
            //for (int j = -_riverAddedWidth; j <= _riverAddedWidth; ++j)
            //{
            //    int xjCurrent = generatedRiverCell.x + j;
            //    for (int k = -_riverAddedWidth; k <= _riverAddedWidth; ++k)
            //    {
            //        int ykCurrent = generatedRiverCell.y + k;
            //        if (ykCurrent >= _textureWidth|| xjCurrent >= _textureHeight || ykCurrent < 0 || xjCurrent < 0
            //            || riverTexture2D.GetPixel(xjCurrent, ykCurrent) == _water)
            //        {
            //            continue;
            //        }
            //        riverTexture2D.SetPixel(xjCurrent, xjCurrent, _water);
            //    }
            //    // continue;
            //}
        }
        _modifiedTexture.Apply();

        Debug.Log("RiverComplete");
    }

    void CreateWaterBank(int kernelWidth)
    {
        if (kernelWidth % 2 == 0) return; // this means the number is event, ignore.

        // Re-generate
        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                Color pixelColor = _modifiedTexture.GetPixel(x, y);
                if (pixelColor == _water || pixelColor == _waterBank)
                    continue;

                int yBottomRow = y - ((kernelWidth - 1) / 2);
                int yTopRow = y + ((kernelWidth - 1) / 2);

                int xLeftColumn = x - ((kernelWidth - 1) / 2);
                int xRightColumn = x + ((kernelWidth - 1) / 2);

                bool waterFound = false;
                for (int yOffset = yBottomRow; yOffset <= yTopRow; ++yOffset)
                {
                    for (int xOffset = xLeftColumn; xOffset <= xRightColumn; ++xOffset)
                    {
                        Color offsetPixelColor = _modifiedTexture.GetPixel(xOffset, yOffset);
                        if (offsetPixelColor == _water)
                        {
                            waterFound = true;
                            _modifiedTexture.SetPixel(x, y, _waterBank);
                            break;
                        }
                    }
                    if (waterFound)
                        break;
                }
            }
        }
        _modifiedTexture.Apply();
    }

    void CreateWaterBanks()
    {
        // print(_modifiedTexture.GetPixel(0, 0));

        // remove all tiles that do not have neighbouring tiles of the same type.

        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                Color pixelColor = _modifiedTexture.GetPixel(x, y);
                if (CompareColors(_water, pixelColor) || CompareColors(_waterBank, pixelColor))
                    continue;

                bool waterFound = false;
                // Iterates through surrounding tiles
                for (int i = -_waterBanksWidth; i <= _waterBanksWidth; ++i)
                {
                    for (int j = -_waterBanksWidth; j <= _waterBanksWidth; ++j)
                    {
                        if (i == 0 && j == 0)
                        {
                            Debug.Log($"{pixelColor}   x: {x} , y: {y}");
                            continue;
                        }
                        else if (x + i < 0 || x + i >= _textureWidth || y + j < 0 || y + j >= _textureHeight)
                            continue;

                        Color pixCol = _modifiedTexture.GetPixel(x + i, y + j);

                        if (CompareColors(_water, pixCol))
                        {
                            waterFound = true;
                            //_modifiedTexture.SetPixel(x, y, _waterBank);
                            break;
                        }
                    }
                    if (waterFound)
                        break;
                }
                if (waterFound)
                    _modifiedTexture.SetPixel(x, y, _waterBank);
            }
        }
        _modifiedTexture.Apply();

        Debug.Log("Water: " + _water);
    }
    
    bool CompareColors(Color colorA, Color colorB)
    {
        if (Mathf.Round(colorA.r * 10.0f) * 0.1f == Mathf.Round(colorB.r * 10.0f) * 0.1f &&
            Mathf.Round(colorA.g * 10.0f) * 0.1f == Mathf.Round(colorB.g * 10.0f) * 0.1f &&
            Mathf.Round(colorA.b * 10.0f) * 0.1f == Mathf.Round(colorB.b * 10.0f) * 0.1f)
            return true;
        else
            return false;
    }

    void CreateForestOffWaterBanks()
    {
        // print(_modifiedTexture.GetPixel(0, 0));

        // remove all tiles that do not have neighbouring tiles of the same type.

        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                Color pixelColor = _modifiedTexture.GetPixel(x, y);
                if (CompareColors(_water, pixelColor) || CompareColors(_waterBank, pixelColor) || CompareColors(_forest, pixelColor))
                    continue;

                bool edgeFound = false;
                // Iterates through surrounding tiles
                for (int i = -_forestWidth; i <= _forestWidth; ++i)
                {
                    for (int j = -_forestWidth; j <= _forestWidth; ++j)
                    {
                        if (i == 0 && j == 0)
                        {
                            Debug.Log($"{pixelColor}   x: {x} , y: {y}");
                            continue;
                        }
                        else if (x + i < 0 || x + i >= _textureWidth || y + j < 0 || y + j >= _textureHeight)
                            continue;

                        Color pixCol = _modifiedTexture.GetPixel(x + i, y + j);

                        if (CompareColors(_water, pixCol) || CompareColors(_waterBank, pixCol))
                        {
                            edgeFound = true;
                            //_modifiedTexture.SetPixel(x, y, _waterBank);
                            break;
                        }
                    }
                    if (edgeFound)
                        break;
                }
                if (edgeFound)
                    _modifiedTexture.SetPixel(x, y, _forest);
            }
        }

        Debug.Log("Water: " + _waterBank);

        _modifiedTexture.Apply();
    }

    protected virtual void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _renderer.sprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
            Debug.Log("Rendered");
        }

        _timeUntilNextGen -= Time.deltaTime;
        if (_timeUntilNextGen > 0.0f) return;

        _seed = (int)Random.Range(0, 1000000.0f);

        _timeUntilNextGen = _timeBetweenGens;

        _modifiedTexture = new Texture2D(_textureWidth, _textureHeight);
        _modifiedTexture.Apply();

        if (_generateBasicZones)
        {
            GenerateBasicZones();

            Sprite basicZonesSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
            _renderer.sprite = basicZonesSprite;
        }

        if (_clean)
        {
            for (int i = 0; i < _totalRivers; i++)
            {
                CleanUpZones(_outerRings, _minSurroundingMatches, false);

                Sprite cleanUpSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
                _renderer.sprite = cleanUpSprite;
            }
            // final clean pass
            CleanUpZones(1, 4, true);
        }

        if (_hasRivers)
        {
            for (int i = 0; i < _totalRivers; i++)
            {
                Debug.Log("Generated River");
                GenerateRiver(_seed + i * (int)_timeUntilNextGen);

                _modifiedTexture.Apply();
                // Note: if file exists at path, it will overwrite.
                // _modifiedTexture = AssetCreator.SaveTexture2D(_modifiedTexture, $"{_folderPathA3}{_fileName}", true, true, true);
                // _modifiedTexture.Apply();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // _modifiedTexture = (Texture2D)AssetDatabase.LoadAssetAtPath($"{_folderPathA3}{_fileName}.bmp", typeof(Texture2D));
                // _modifiedTexture.Apply();

                // Note: if file exists at path, it will overwrite.
                //_modifiedTexture = AssetCreator.CreateTexture2D(_modifiedTexture, $"{_folderPathA3}{_fileName}", _filter, _readable, _format);
                //_modifiedTexture.Apply();
            }

            //for (int i = 0; i < 3; i++)
            //{
            //    // final clean pass
            //    CleanUpZones(1, 4, true);
            //}


            Sprite riverSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
            _renderer.sprite = riverSprite;
        }

        if (_hasWaterBanks)
        {
            CreateWaterBanks();
            _modifiedTexture.Apply();

            Sprite waterBankSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
            _renderer.sprite = waterBankSprite;
        }

        if (_hasForest)
        {
            CreateForestOffWaterBanks();
            //_modifiedTexture.Apply();

            Sprite forestSprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
            _renderer.sprite = forestSprite;
        }

        if (_isBlurry)
        {
            BlurTerrain((int)_blurStrength);

            Sprite blurrySprite = Sprite.Create(_modifiedTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
            _renderer.sprite = blurrySprite;
        }

    }

    protected virtual void OnDisable()
    {
        return;
        //    //// Note: if file exists at path, it will overwrite.
        //    //_modifiedTexture = AssetCreator.SaveTexture2D(_modifiedTexture, $"{_folderPathA3}{_fileName}", true, true, true);
        //    //_modifiedTexture.Apply();
        //    //AssetDatabase.SaveAssets();
        //    //AssetDatabase.Refresh();
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