using UnityEditor;
using UnityEngine;

public class TerrainGenerator_Base : MonoBehaviour
{
    protected SpriteRenderer _renderer;
    protected Sprite _sourceSprite;
    protected Texture2D _sourceTexture;
    protected Texture2D _modifiedTexture;

    protected int _textureWidth;
    protected int _textureHeight;

    protected string _folderPathA2 = "Assets/Sprites/Assessment2/";
    protected string _folderPathA3 = "Assets/Sprites/Assessment3/";

    [Header("ALL VARIABLES: TerrainGenerator_Base.cs")]
    [SerializeField] protected string _fileName;

    [SerializeField] protected float _timeUntilNextGen = 1.0f;
    [SerializeField] protected float _timeBetweenGens = 10000000.0f;
    [SerializeField] protected int _seed = 42069;

    [SerializeField] protected bool _isGeneratingVoxels;

    protected virtual void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _sourceSprite = _renderer.sprite;
        _sourceTexture = _sourceSprite.texture;
        _textureWidth = _sourceTexture.width;
        _textureHeight = _sourceTexture.height;
        _modifiedTexture = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGB24, false);
    }

    protected virtual void ManageTimeAndSeed()
    {
        _timeUntilNextGen -= Time.deltaTime;
        if (_timeUntilNextGen > 0.0f) return;

        _seed = (int)Random.Range(0, 1000000.0f);

        _timeUntilNextGen = _timeBetweenGens;
    }
}
