using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainAltitudeGenerator : TerrainGenerator_Base
{
    protected Texture2D _heightMap;

    [Header("ALL VARIABLES: TerrainAltitudeGenerator.cs")]
    [SerializeField] protected bool _generatePerlinNoise;

    [SerializeField] GameObject _voxelPrefab;
    [SerializeField] GameObject _voxelCheapPrefab;

    [SerializeField] List<GameObject> _voxels = new List<GameObject>();

    [SerializeField] GameObject[,] _voxelsXY;

    // 64x64 zones
    [SerializeField] int _numberOfZones = 4;
    [SerializeField] int _zoneWidth = 128;
    [SerializeField] int _zoneHeight = 128;

    [SerializeField] List<ZoneInformation> _zoneInfoList = new List<ZoneInformation>();

    const int MIN_VOXEL_HEIGHT = -1;

    [SerializeField] Texture2D _cellMap;

    [System.Serializable]
    public struct ZoneInformation
    {
        public float _heightScalar;
        public int _seed;
        public int _numberOfVoxelSteps;
    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
        public float _noiseZoom;
    }

    [Header("KEY VALUES: Adjustable to Create Terrain Height Variation")]
    [SerializeField] protected int _perlinNoiseScalar = 3;
    [SerializeField] protected float _heightScalar = 1.0f;


    ZoneInformation CoordsToZoneInformation(int x, int y)
    {
        int numberOfCols = _textureWidth / _zoneWidth; // 2

        int colIndex = x / _zoneWidth;
        int rowIndex = y / _zoneHeight;

        int zoneInformationIndex = colIndex + (rowIndex * numberOfCols);

        if (zoneInformationIndex >= _zoneInfoList.Count)
        {
            // print($"{x}, {y}, {zoneInformationIndex}");
            return _zoneInfoList[0];
        }

        return _zoneInfoList[zoneInformationIndex];
    }

    protected override void Awake()
    {
        base.Awake();

        _zoneWidth = _textureWidth / (_numberOfZones / 2);
        _zoneHeight = _textureHeight / (_numberOfZones / 2);

        _heightMap = new Texture2D(_textureWidth, _textureHeight);

        _voxelsXY = new GameObject[_textureWidth, _textureHeight];
    }

    protected void Update()
    {
        _timeUntilNextGen -= Time.deltaTime;
        if (_timeUntilNextGen > 0.0f) return;

        //seed = (int)Random.Range(0, 1000000.0f);

        _timeUntilNextGen = _timeBetweenGens;

        GenerateVoxelHeightmap();


        BlendBoundariesOfChunks(3);

        //BlurBoundariesOfChunks(13);
        //BlurBoundariesOfChunks(11);
        BlurBoundariesOfChunks(7);
        //BlurBoundariesOfChunks(7);
        // BlurBoundariesOfChunks(5);
        //BlurBoundariesOfChunks(3);

        if (_isGeneratingVoxels)
            GenerateVoxelTerrain();

        Sprite newSprite = Sprite.Create(_heightMap, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        _renderer.sprite = newSprite;


        print("DONE");
        // remove areas that dont have 10+ tiles (small zones).
        // change zones that shouldnt touch.
    }

    void GenerateVoxelHeightmap()
    {
        for (float y = 0; y < _textureHeight; ++y)
        {
            for (float x = 0; x < _textureWidth; ++x)
            {
                ZoneInformation zoneInfo = CoordsToZoneInformation((int)x, (int)y);

                float xCoord = zoneInfo._seed + x / _textureWidth * zoneInfo._noiseZoom;
                float yCoord = zoneInfo._seed + y / _textureHeight * zoneInfo._noiseZoom;
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);

                perlin *= zoneInfo._numberOfVoxelSteps;
                // 0 -> 10

                int perlinClamped = (int)perlin;
                // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9

                perlin = perlinClamped / (float)zoneInfo._numberOfVoxelSteps;
                // 0 -> 1.0
                // 0.1 0.2 0.33

                _heightMap.SetPixel((int)x, (int)y, new Color(perlin, perlin, perlin));
            }
        }
        _heightMap.Apply();
    }

    void BlurBoundariesOfChunks(int kernelWidth)
    {
        if (kernelWidth % 2 == 0) return; // this means the number is even, ignore.

        Texture2D blurTexture = new Texture2D(_textureWidth, _textureHeight);

        // I know this is redundant, but for clarity and maybe scalability if you
        // want more column zones and less row zones or something
        int numberOfCols = _textureWidth / _zoneWidth; // 2
        // int numberOfRows = _textureHeight / _zoneHeight; // 2

        for (int index = 1; index < numberOfCols; index++)
        {
            int columnX = index * _zoneWidth;
            int rowY = index * _zoneHeight;

            for (int y = 0; y < _textureHeight; ++y)
            {
                for (int x = 0; x < _textureWidth; ++x)
                {
                    bool shouldBlurPixelColumn = x > columnX - (kernelWidth / 2) && x < columnX + (kernelWidth / 2);
                    bool shouldBlurPixelRow = y > rowY - (kernelWidth / 2) && y < rowY + (kernelWidth / 2);

                    if (shouldBlurPixelColumn)
                    {
                        //int yBottomRow = y - ((kernelWidth - 1) / 2);
                        //int yTopRow = y + ((kernelWidth - 1) / 2);

                        int xLeftColumn = x - ((kernelWidth - 1) / 2);
                        int xRightColumn = x + ((kernelWidth - 1) / 2);

                        int samples = 0;           // number of pixels sampled in the kernel
                        Color sampleAdded = new Color(); // add all pixels in the kernel here
                        //for (int yOffset = yBottomRow; yOffset <= yTopRow; ++yOffset)
                        //{
                        for (int xOffset = xLeftColumn; xOffset <= xRightColumn; ++xOffset)
                        {
                            Color sample = _heightMap.GetPixel(xOffset, y);
                            sampleAdded += sample;
                            samples += 1;
                        }
                        // }

                        Color averaged = sampleAdded / samples;
                        blurTexture.SetPixel(x, y, averaged);
                    }
                    else if (shouldBlurPixelRow)
                    {
                        int yBottomRow = y - ((kernelWidth - 1) / 2);
                        int yTopRow = y + ((kernelWidth - 1) / 2);

                        //int xLeftColumn = x - ((kernelWidth - 1) / 2);
                        //int xRightColumn = x + ((kernelWidth - 1) / 2);

                        int samples = 0;           // number of pixels sampled in the kernel
                        Color sampleAdded = new Color(); // add all pixels in the kernel here

                        for (int yOffset = yBottomRow; yOffset <= yTopRow; ++yOffset)
                        {
                            //for (int xOffset = xLeftColumn; xOffset <= xRightColumn; ++xOffset)
                            //{
                            Color sample = _heightMap.GetPixel(x, yOffset);
                            sampleAdded += sample;
                            samples += 1;
                        }
                        // }

                        Color averaged = sampleAdded / samples;
                        blurTexture.SetPixel(x, y, averaged);
                    }
                    else
                    {
                        blurTexture.SetPixel(x, y, _heightMap.GetPixel(x, y));
                    }
                }
            }
        }
        blurTexture.Apply();

        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                _heightMap.SetPixel(x, y, blurTexture.GetPixel(x, y));
            }
        }
        _heightMap.Apply();
    }

    void BlendBoundariesOfChunks(int kernelWidth)
    {
        if (kernelWidth % 2 == 0) return; // this means the number is even, ignore.

        // Texture2D blendTexture = new Texture2D(_textureWidth, _textureHeight);

        Texture2D blendTexture = _heightMap;
        blendTexture.Apply();

        int numberOfCols = _textureWidth / _zoneWidth; // 2

        int blendDistance = (kernelWidth - 1) / 2;

        for (int columnIndex = 1; columnIndex < numberOfCols; columnIndex++)
        {
            int columnX = columnIndex * _zoneWidth;

            for (int y = 0; y < _textureHeight; ++y)
            {

                Color firstPixel = _heightMap.GetPixel(columnX - blendDistance, y);
                Color lastPixel = _heightMap.GetPixel(columnX + blendDistance, y);
                int colorIterator = 1;

                for (int i = columnX - blendDistance; i <= columnX + blendDistance; i++)
                {
                    Color blendedColor = ((firstPixel * (kernelWidth - colorIterator)) + (lastPixel * colorIterator)) / kernelWidth;
                    blendTexture.SetPixel(i, y, blendedColor);
                    colorIterator++;
                }


                //for (int x = 0; x < _textureWidth; ++x)
                //{
                //    bool shouldBlurPixel = x > columnX - (kernelWidth / 2) && x < columnX + (kernelWidth / 2);

                //    if (shouldBlurPixel)
                //    {
                //        int yBottomRow = y - ((kernelWidth - 1) / 2);
                //        int yTopRow = y + ((kernelWidth - 1) / 2);

                //        int xLeftColumn = x - ((kernelWidth - 1) / 2);
                //        int xRightColumn = x + ((kernelWidth - 1) / 2);

                //        int samples = 0;           // number of pixels sampled in the kernel
                //        Color sampleAdded = new Color(); // add all pixels in the kernel here
                //        for (int yOffset = yBottomRow; yOffset <= yTopRow; ++yOffset)
                //        {
                //            for (int xOffset = xLeftColumn; xOffset <= xRightColumn; ++xOffset)
                //            {
                //                Color sample = _heightMap.GetPixel(xOffset, yOffset);
                //                sampleAdded += sample;
                //                samples += 1;
                //            }
                //        }

                //        Color averaged = sampleAdded / samples;
                //        blendTexture.SetPixel(x, y, averaged);
                //    }
                //    else
                //    {
                //        blendTexture.SetPixel(x, y, _heightMap.GetPixel(x, y));
                //    }
                //}
            }
        }
        blendTexture.Apply();

        for (int y = 0; y < _textureHeight; ++y)
        {
            for (int x = 0; x < _textureWidth; ++x)
            {
                _heightMap.SetPixel(x, y, blendTexture.GetPixel(x, y));
            }
        }
        _heightMap.Apply();
    }

    void GenerateVoxelTerrain()
    {
        // delete existing voxels.

        if (_voxels.Count != 0)
        {
            foreach (GameObject voxel in _voxels)
            {
                Destroy(voxel);
            }
            /*
            for(int voxelIndex = 0; voxelIndex < _textureWidth * _textureHeight; ++voxelIndex)
            {
                GameObject voxel = GameObject.Instantiate(voxelPrefab);
                voxels.Add(voxel);
            }
            */
        }

        // Empty Out 2D Array
        //foreach (GameObject voxel in _voxels)
        //{
        //    Destroy(voxel);
        //}

        for (int x = 0; x < _textureHeight; x++)
        {

            for (int y = 0; y < _textureHeight; y++)
            {
                Destroy(_voxelsXY[x, y]);
            }
        }

        _voxelsXY = new GameObject[_textureWidth, _textureHeight];

        for (float z = 0; z < _textureHeight; ++z)
        {
            for (float x = 0; x < _textureWidth; ++x)
            {
                ZoneInformation zoneInfo = CoordsToZoneInformation((int)x, (int)z);

                float altitudeSample = _heightMap.GetPixel((int)x, (int)z).r;

                float y = altitudeSample * zoneInfo._heightScalar;
                _voxelCheapPrefab.transform.localScale = new Vector3(_voxelCheapPrefab.transform.localScale.x, y, _voxelCheapPrefab.transform.localScale.z);
                // GameObject voxel = Instantiate(_voxelCheapPrefab);
                // voxel.transform.position = new Vector3(x, y, z);

                string cellMapPath = AssetDatabase.GetAssetPath(_cellMap);

                _cellMap = (Texture2D)AssetDatabase.LoadAssetAtPath(cellMapPath, typeof(Texture2D));

                Color cellColor = _cellMap.GetPixel((int)x, (int)z);

                _voxelsXY[(int)x, (int)z] = Instantiate(_voxelCheapPrefab, new Vector3(x, y, z), Quaternion.identity);
                _voxelsXY[(int)x, (int)z].GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", cellColor);

                //_voxels.Add(_voxelsXY[(int)x, (int)z]);

                // Removed this code because it made my whole code run super slowly, too many triangles and stuff
                // But it did layer voxels below, down to a certain point.
                //for (float y = altitudeSample * zoneInfo._heightScalar; y >= MIN_VOXEL_HEIGHT; y -= 1)
                //{
                //    GameObject voxel = Instantiate(_voxelPrefab);
                //    voxel.transform.position = new Vector3(x, y, z);
                //    if (y == altitudeSample * zoneInfo._heightScalar)
                //        _voxels.Add(voxel);
                //}
            }
        }
        // sample each pixel
        // place each voxel based on that pixel
    }

}