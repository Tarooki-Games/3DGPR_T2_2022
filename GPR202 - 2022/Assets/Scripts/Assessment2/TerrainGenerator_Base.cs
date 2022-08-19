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

    protected string _folderPath = "Assets/Sprites/Assessment2/";
    [SerializeField] protected string _fileName;

    protected virtual void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _sourceSprite = _renderer.sprite;
        _sourceTexture = _sourceSprite.texture;
        _textureWidth = _sourceTexture.width;
        _textureHeight = _sourceTexture.height;
        _modifiedTexture = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGB24, false);
    }
}
