using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainVoxelGenerator : TerrainImageGenerator
{
    protected Texture2D _noiseTexture;
    protected Texture2D _heightMap;

    [Header("TerrainVoxelGenerator.cs   VARIABLES")]
    [SerializeField] protected bool _generatePerlinNoise;

    //// The origin of the sampled area in the plane.
    //[SerializeField] protected float xOrigin;
    //[SerializeField] protected float yOrigin;

    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    [SerializeField] protected int _perlinNoiseScalar = 3;

    [SerializeField] GameObject _voxelPrefab;

    [SerializeField] protected float _heightScalar = 1.0f;
    [SerializeField] float _clampValue = 10.0f;

    List<GameObject> _voxels = new List<GameObject>();

    protected virtual void Start()
    {
        _noiseTexture = new Texture2D(_textureWidth, _textureHeight);
        _heightMap = new Texture2D(_textureWidth, _textureHeight);
    }

    // Update is called once per frame
    protected override void Update()
    {
        _timeUntilNextGen -= Time.deltaTime;
        if (_timeUntilNextGen > 0.0f) return;

        _seed = (int)Random.Range(0, 1000000.0f);

        _timeUntilNextGen = _timeBetweenGens;

        if (!_isGeneratingVoxels)
        {
            base.Update();
            _fileName = "Image_Voxels";
            _modifiedTexture = AssetCreator.GetTexture2DFromAssets(_modifiedTexture, $"{_folderPathA3}{_fileName}");
            _modifiedTexture.Apply();
        }



        //if (_generatePerlinNoise)
        //{
        //    GeneratePerlinNoiseTexture2D();

        //    //Sprite terrainSprite = Sprite.Create(_noiseTexture, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
        //    //_renderer.sprite = terrainSprite;

        //    // Note: if file exists at path, it will overwrite.                                  bilinear, read/write enabled, RGBA32
        //    _noiseTexture = AssetCreator.CreateTexture2D(_noiseTexture, $"{_folderPath}noiseTexture", false, true, false);
        //    _noiseTexture.Apply();
        //}

        if (_isGeneratingVoxels)
        {
            _fileName = "HeightMap";

            _heightMap = GenerateHeightMap();
            GenerateVoxelTerrain();

            Sprite heightMap = Sprite.Create(_heightMap, _sourceSprite.rect, new Vector2(0.5f, 0.5f));
            _renderer.sprite = heightMap;

            _heightMap = AssetCreator.CreateTexture2D(_heightMap, $"{_folderPathA3}{_fileName}", false, true, false);
            _heightMap.Apply();
        }
    }

    //IEnumerator SaveCreatedAsset(Texture2D texture2D)
    //{
    //    yield return new WaitForSeconds(1.0f);

    //    texture2D = AssetCreator.CreateTexture2D(texture2D, $"{_folderPath}HeightMap", false, true, false);
    //    texture2D.Apply();
    //}

    protected Texture2D GenerateHeightMap()
    {
        for (float y = 0; y < _textureHeight; ++y)
        {
            for (float x = 0; x < _textureWidth; ++x)
            {
                float xCoord = _seed + x / _textureWidth * _perlinNoiseScalar;
                float yCoord = _seed + y / _textureHeight * _perlinNoiseScalar;
                float perlinNoise = Mathf.PerlinNoise(xCoord, yCoord);


                perlinNoise *= _clampValue;
                // 0 -> 10

                int perlinClamped = (int)perlinNoise;
                // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9

                perlinNoise = perlinClamped / _clampValue;
                // 0 -> 1.0
                // 0.1 0.2 0.33

                _noiseTexture.SetPixel((int)x, (int)y, new Color(perlinNoise, perlinNoise, perlinNoise));
            }
        }
        _noiseTexture.Apply();

        return _noiseTexture;
    }

    protected Texture2D GenerateHeightMapWithZones()
    {
        for (float y = 0; y < _textureHeight; ++y)
        {
            for (float x = 0; x < _textureWidth; ++x)
            {
                float xCoord = _seed + x / _textureWidth * _perlinNoiseScalar;
                float yCoord = _seed + y / _textureHeight * _perlinNoiseScalar;
                float perlinNoise = Mathf.PerlinNoise(xCoord, yCoord);


                perlinNoise *= _clampValue;
                // 0 -> 10

                int perlinClamped = (int)perlinNoise;
                // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9

                perlinNoise = perlinClamped / _clampValue;
                // 0 -> 1.0
                // 0.1 0.2 0.33

                _noiseTexture.SetPixel((int)x, (int)y, new Color(perlinNoise, perlinNoise, perlinNoise));
            }
        }
        _noiseTexture.Apply();

        return _noiseTexture;
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
            for(int voxelIndex = 0; voxelIndex < sourceTexture.width * sourceTexture.height; ++voxelIndex)
            {
                GameObject voxel = GameObject.Instantiate(voxelPrefab);
                voxels.Add(voxel);
            }
            */
        }

        // Unlike Unreal Engine, Unity uses z-axis instead of y-axis
        for (float z = 0; z < _textureHeight; ++z)
        {
            for (float x = 0; x < _textureWidth; ++x)
            {
                float altitudeSample = _heightMap.GetPixel((int)x, (int)z).r;

                float y = altitudeSample * _heightScalar;

                GameObject voxel = Instantiate(_voxelPrefab);
                voxel.transform.position = new Vector3(x, y, z);

                _voxels.Add(voxel);
            }
        }
        // sample each pixel
        // place each voxel based on that pixel
    }


    protected override void OnDisable()
    {
        return;
        // Note: if file exists at path, it will overwrite.
        //_noiseTexture = AssetCreator.CreateTexture2D(_noiseTexture, $"{_folderPath}noiseTexture", false, true, false);
        //_noiseTexture.Apply();

        //_heightMap = AssetCreator.CreateTexture2D(_heightMap, $"{_folderPath}HeightMap", false, true, false);
        //_heightMap.Apply();
    }
}
