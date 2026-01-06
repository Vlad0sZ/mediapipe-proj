using System;
using SensorPack.KinectCore.Runtime.Managers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace SensorPack.KinectCore.Runtime.Behaviours
{
    public class SilhouetteMeshGenerator : MonoBehaviour
    {
        private readonly struct ImageSize
        {
            public readonly int Width;
            public readonly int Height;
            public readonly int Size;

            public ImageSize(int width, int height)
            {
                Width = width;
                Height = height;
                Size = width * height;
            }
        }

        [Range(1, 8)] public int sampleSize = 4; // Снижаем разрешение для оптимизации

        [Tooltip("Target mesh width in meters.")]
        public float meshWidth = 5.12f;

        [Tooltip("Target mesh height in meters.")]
        public float meshHeight = 4.24f;

        [FormerlySerializedAs("_f")] [SerializeField] private float maxDistance = 1f;

        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private bool filter1;
        [SerializeField] private bool filter2;
        [SerializeField] private bool cpu;

        public Texture2D maskTexture;

        private Mesh _mesh;

        private Vector2[] _uvs;
        private Vector3[] _vertices;
        private int[] _triangles;

        private KinectManager _manager;
        private BackgroundRemovalManager _removalManager;

        private ComputeBuffer _vertexZBuffer;
        private ComputeBuffer _depthDataBuffer;
        private ComputeBuffer _colorCoordsBuffer;
        private int _csMainKernel;

        private Vector2[] _colorCoords;
        private ImageSize _depthSize;
        private ImageSize _colorSize;


        public Texture alpha;
        public Texture body;
        public Texture depth;
        private static readonly int Vertices = Shader.PropertyToID("Vertices");
        private static readonly int DepthData = Shader.PropertyToID("DepthData");
        private static readonly int ColorCoords = Shader.PropertyToID("ColorCoords");
        private static readonly int MaskTexture = Shader.PropertyToID("MaskTexture");
        private static readonly int Width = Shader.PropertyToID("Width");
        private static readonly int Height = Shader.PropertyToID("Height");
        private static readonly int ColorWidth = Shader.PropertyToID("ColorWidth");
        private static readonly int ColorHeight = Shader.PropertyToID("ColorHeight");
        private static readonly int SampleSize = Shader.PropertyToID("SampleSize");
        private static readonly int MaxWidth = Shader.PropertyToID("MaxWidth");
        private static readonly int FilterPass = Shader.PropertyToID("FilterPass");

        private void OnDestroy()
        {
            _vertexZBuffer?.Release();
            _depthDataBuffer?.Release();
            _colorCoordsBuffer?.Release();
        }

        void Start()
        {
            _manager = KinectManager.Instance;
            _removalManager = BackgroundRemovalManager.Instance;

            if (_manager == null)
            {
                Debug.LogError("Kinect manager is null");
                this.enabled = false;
                return;
            }

            if (_removalManager == null)
            {
                Debug.LogError("BackgroundRemovalManager is null");
                this.enabled = false;
                return;
            }

            if (_manager != null)
            {
                var depthWidth = _manager.GetDepthImageWidth();
                var depthHeight = _manager.GetDepthImageHeight();
                
                var colorWidth = _manager.GetColorImageWidth();
                var colorHeight = _manager.GetColorImageHeight();

                _colorSize = new ImageSize(colorWidth, colorHeight);
                _depthSize = new ImageSize(depthWidth, depthHeight);
                _colorCoords = new Vector2[_depthSize.Size];


                CreateMesh(_depthSize.Width / sampleSize, _depthSize.Height / sampleSize);
            }


            if (computeShader != null)
            {
                _csMainKernel = computeShader.FindKernel("CSMain");

                int vertexCount = (_depthSize.Width / sampleSize) * (_depthSize.Height / sampleSize);
                _vertexZBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);
                _vertexZBuffer.SetData(_vertices);

                _colorCoordsBuffer = new ComputeBuffer(_colorCoords.Length, sizeof(float) * 2);
                _colorCoordsBuffer.SetData(_colorCoords);
            }

            GetComponent<Renderer>().material.mainTexture = _manager.GetUsersClrTex();
        }

        void CreateMesh(int width, int height)
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
            _uvs = new Vector2[width * height];
            _vertices = new Vector3[width * height];
            _triangles = new int[6 * ((width - 1) * (height - 1))];

            float scaleX = meshWidth / width;
            float scaleY = meshHeight / height;

            int triangleIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width) + x;

                    float xScaled = x * scaleX - meshWidth / 2;
                    float yScaled = y * scaleY - meshHeight / 2;

                    _uvs[index] = new Vector2((float)x / width, (float)y / height);
                    _vertices[index] = new Vector3(xScaled, -yScaled, 0);

                    // Skip the last row/col
                    if (x != (width - 1) && y != (height - 1))
                    {
                        int topLeft = index;
                        int topRight = topLeft + 1;
                        int bottomLeft = topLeft + width;
                        int bottomRight = bottomLeft + 1;

                        _triangles[triangleIndex++] = topLeft;
                        _triangles[triangleIndex++] = topRight;
                        _triangles[triangleIndex++] = bottomLeft;
                        _triangles[triangleIndex++] = bottomLeft;
                        _triangles[triangleIndex++] = topRight;
                        _triangles[triangleIndex++] = bottomRight;
                    }
                }
            }

            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;
            _mesh.triangles = _triangles;
            _mesh.RecalculateNormals();
        }

        void Update()
        {
            if (_manager == null || _removalManager == null)
            {
                Debug.LogWarning("Kinect manager or Background Removal was null.");
                return;
            }


            var sensordata = _manager.GetSensorData();
            if (alpha == null)
                alpha = sensordata.alphaBodyTexture;

            if (body == null)
                body = sensordata.bodyIndexTexture;

            if (depth == null)
                depth = sensordata.depthImageTexture;

            var depthData = _manager.GetRawDepthMap();
            Texture texture = _removalManager.GetForegroundTex();

            if (texture == null)
                return;

            if (maskTexture == null || maskTexture.width != texture.width || maskTexture.height != texture.height)
            {
                if (maskTexture != null)
                    UnityEngine.Object.Destroy(maskTexture);

                maskTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            }

            UpdateTexture(maskTexture, texture);

            if (cpu)
                UpdateVertices(maskTexture, depthData);

            ApplyComputeShader(depthData);

            _mesh.uv = _uvs;
            _mesh.vertices = _vertices;
            _mesh.RecalculateNormals();
        }

        private void ApplyComputeShader(ushort[] depthData)
        {
            uint[] depthDataUint = Array.ConvertAll(depthData, item => (uint)item);

            // Создаем/обновляем буфер глубины
            if (_depthDataBuffer == null || _depthDataBuffer.count != depthData.Length)
            {
                _depthDataBuffer?.Release();
                _depthDataBuffer = new ComputeBuffer(depthData.Length, sizeof(uint));
            }

            _depthDataBuffer.SetData(depthDataUint);
            _vertexZBuffer.SetData(_vertices);
            _colorCoordsBuffer.SetData(_colorCoords);

            // Устанавливаем параметры
            computeShader.SetBuffer(_csMainKernel, Vertices, _vertexZBuffer);
            computeShader.SetBuffer(_csMainKernel, DepthData, _depthDataBuffer);
            computeShader.SetBuffer(_csMainKernel, ColorCoords, _colorCoordsBuffer);

            computeShader.SetTexture(_csMainKernel, MaskTexture, maskTexture);
            computeShader.SetInt(Width, _depthSize.Width);
            computeShader.SetInt(Height, _depthSize.Height);
            computeShader.SetInt(ColorWidth, _colorSize.Width);
            computeShader.SetInt(ColorHeight, _colorSize.Height);
            computeShader.SetInt(SampleSize, sampleSize);
            computeShader.SetFloat(MaxWidth, 2000);


            if (filter1)
            {
                // Запускаем проход удаления шума
                computeShader.SetInt(FilterPass, 0);
                computeShader.Dispatch(_csMainKernel,
                    Mathf.CeilToInt(_depthSize.Width / 8f),
                    Mathf.CeilToInt(_depthSize.Height / 8f),
                    1);
            }

            if (filter2)
            {
                // Запускаем проход сглаживания
                computeShader.SetInt(FilterPass, 1);
                computeShader.Dispatch(_csMainKernel,
                    Mathf.CeilToInt(_depthSize.Width / 8f),
                    Mathf.CeilToInt(_depthSize.Height / 8f),
                    1);
            }

            _vertexZBuffer.GetData(_vertices);
        }

        private void UpdateVertices(Texture2D silhouetteMask, ushort[] depthData)
        {
            var pixels = silhouetteMask.GetPixelData<Color32>(0);

            if (silhouetteMask.width == _colorSize.Width && silhouetteMask.height == _colorSize.Height)
            {
                UpdateAsColorMask(depthData, pixels);
            }
            else if (silhouetteMask.width == _depthSize.Width && silhouetteMask.height == _depthSize.Height)
            {
                UpdateAsDepthMask(depthData, pixels);
            }
            else
            {
                throw new InvalidOperationException(
                    $"mask size is not a depth size or color size: ({silhouetteMask.width}x{silhouetteMask.height})");
            }
        }

        private void UpdateAsDepthMask(ushort[] depthData, NativeArray<Color32> pixels) =>
            OnUpdate(depthData, pixels, (depthIndex) => depthIndex);

        private void UpdateAsColorMask(ushort[] depthData, NativeArray<Color32> pixels)
        {
            if (_manager.MapDepthFrameToColorCoords(ref _colorCoords) == false)
                return;

            OnUpdate(depthData, pixels, (depthIndex) =>
            {
                Vector2 colorCoord = _colorCoords[depthIndex];

                if (!IsValidCoord(colorCoord) || (colorCoord.x < 0) || (colorCoord.y < 0) ||
                    colorCoord.x >= _colorSize.Width || colorCoord.y >= _colorSize.Height)
                    return -1;

                int pixelIndex = ((int)colorCoord.y * _colorSize.Width) + (int)colorCoord.x;
                return pixelIndex;
            });
        }

        private void OnUpdate(ushort[] depthData, NativeArray<Color32> pixels, Func<int, int> depthIndexToPixelIndex)
        {
            for (int y = 0; y < _depthSize.Height; y += sampleSize)
            {
                for (int x = 0; x < _depthSize.Width; x += sampleSize)
                {
                    int indexX = x / sampleSize;
                    int indexY = y / sampleSize;
                    int smallIndex = (indexY * (_depthSize.Width / sampleSize)) + indexX;
                    int depthPixel = (y * _depthSize.Width) + x;
                    int pixelIndex = depthIndexToPixelIndex(depthPixel);
                    if (pixelIndex < 0)
                    {
                        _vertices[smallIndex].z = 0;
                        continue;
                    }

                    var colorCoord = _colorCoords[depthPixel];
                    _uvs[smallIndex] = new Vector2(
                        colorCoord.x / _colorSize.Width, colorCoord.y / _colorSize.Height);

                    Color32 maskColor = pixels[pixelIndex];
                    if (maskColor.a == 0)
                    {
                        _vertices[smallIndex].z = 0;
                    }
                    else
                    {
                        ushort avg = depthData[depthPixel];
                        if (avg == ushort.MaxValue || avg == ushort.MinValue || avg >= maxDistance)
                            avg = 0;

                        _vertices[smallIndex].z = -(avg / maxDistance);
                    }
                }
            }
        }

        private bool IsValidCoord(Vector2 coord) =>
            !float.IsInfinity(coord.x) && !float.IsInfinity(coord.y);

        private static void UpdateTexture(Texture2D texture2D, Texture texture)
        {
            if (texture is RenderTexture renderTexture)
            {
                ConvertRenderTextureToTexture2D(texture2D, renderTexture);
            }
            else if (texture is Texture2D t2)
            {
                var colors = t2.GetRawTextureData();
                texture2D.LoadRawTextureData(colors);
                texture2D.Apply();
            }
        }

        public static void ConvertRenderTextureToTexture2D(Texture2D texture2D, RenderTexture renderTexture)
        {
            // Создаём новую Texture2D с теми же размерами, что и RenderTexture
            // Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

            // Устанавливаем активную RenderTexture
            RenderTexture.active = renderTexture;

            // Копируем пиксели из RenderTexture в Texture2D
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            // Сбрасываем активную RenderTexture
            RenderTexture.active = null;
        }
    }
}