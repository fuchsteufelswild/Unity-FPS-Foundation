using System;
using UnityEngine;

namespace Nexora.Serialization
{
    [Serializable]
    public class SerializedTexture2D : ISerializationCallbackReceiver
    {
        [SerializeField]
        private byte[] _compressedData;

        [SerializeField]
        private int _width;

        [SerializeField]
        private int _height;

        private Texture2D _texture;

        public Texture2D Texture => _texture;

        public SerializedTexture2D(Texture2D texture)
        {
            _texture = texture;
            _texture.width = _width;
            _texture.height = _height;
            _compressedData = texture.EncodeToJPG();
        }

        public static implicit operator Texture2D(SerializedTexture2D serializedTexture)
            => serializedTexture.Texture;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if(_compressedData == null)
            {
                return;
            }

            _texture = new Texture2D(_width, _height, TextureFormat.RGB24, true);
            _texture.LoadImage(_compressedData);
            _texture.Apply();
            _compressedData = null;
        }

        
    }
}