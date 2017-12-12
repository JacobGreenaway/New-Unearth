using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MultiSourceManager : MonoBehaviour
{
    private struct LayerRange
    {
        public float Bottom;
        public float Top;

        public LayerRange(float bottom, float top)
        {
            Bottom = bottom;
            Top = top;
        }
    }

    private struct LayerValue
    {
        public int Layer;
        public int Index;
    }

    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }

    private KinectSensor _Sensor;
    private MultiSourceFrameReader _Reader;
    private Texture2D _ColorTexture;
    private Texture2D _DepthTexture;
    private RenderTexture _DepthRenderTexture;
    private ushort[] _DepthData;
    private byte[] _ColorData;
    //private byte[] _RawDepthData;
    private Color[] _DepthColorData;

    [SerializeField]
    private float _Weight;

    [SerializeField]
    private ComputeShader m_DepthComputeShader;
    private ComputeBuffer m_DepthComputeBuffer;
    private ComputeBuffer m_DepthComputeBufferPrev;
    private ComputeBuffer m_LayersComputeBuffer;
    private ComputeBuffer m_LayerRangesBuffer;
    private float[] m_DepthFloatData;
    private float[] m_DepthFloatDataPrev;
    private LayerValue[] m_Layers;
    private LayerRange[] m_LayerRanges;
    private const int LayerCount = 9;
    private int m_KernalHandle;

    public Texture2D GetColorTexture()
    {
        return _ColorTexture;
    }

    public ushort[] GetDepthData()
    {
        return _DepthData;
    }

    public Texture2D GetDepthTexture()
    {
        return _DepthTexture;
    }

    public RenderTexture GetDepthRenderTexture()
    {
        return _DepthRenderTexture;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            var colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = colorFrameDesc.Width;
            ColorHeight = colorFrameDesc.Height;

            _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            _ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

            var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
            _DepthData = new ushort[depthFrameDesc.LengthInPixels];
            //_RawDepthData = new byte[depthFrameDesc.LengthInPixels * 4];
            _DepthTexture = new Texture2D(depthFrameDesc.Width, depthFrameDesc.Height, TextureFormat.RGBAFloat, true);
            _DepthRenderTexture = new RenderTexture(depthFrameDesc.Width, depthFrameDesc.Height, 16);
            _DepthRenderTexture.wrapMode = TextureWrapMode.Clamp;
            _DepthRenderTexture.filterMode = FilterMode.Point;
            _DepthRenderTexture.enableRandomWrite = true;
            _DepthRenderTexture.Create();
            m_KernalHandle = m_DepthComputeShader.FindKernel("GenerateDepth");
            m_DepthComputeShader.SetTexture(m_KernalHandle, "Result", _DepthRenderTexture);
            m_DepthComputeBuffer = new ComputeBuffer(_DepthData.Length, (int)sizeof(float), ComputeBufferType.Raw);
            m_DepthComputeBufferPrev = new ComputeBuffer(_DepthData.Length, (int)sizeof(float), ComputeBufferType.Raw);
            m_LayersComputeBuffer = new ComputeBuffer(_DepthData.Length, (int)(sizeof(int) * 2));
            m_LayerRangesBuffer = new ComputeBuffer(LayerCount, (int)(sizeof(float) * 2));
            m_DepthFloatData = new float[depthFrameDesc.LengthInPixels];
            m_DepthFloatDataPrev = new float[depthFrameDesc.LengthInPixels];
            m_Layers = new LayerValue[depthFrameDesc.LengthInPixels];
            m_LayerRanges = new LayerRange[LayerCount];
            _DepthColorData = new Color[depthFrameDesc.LengthInPixels];

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    private void OnDestroy()
    {
        if (m_DepthComputeBuffer != null)
        {
            m_DepthComputeBuffer.Release();
            m_DepthComputeBuffer = null;
        }

        if (m_DepthComputeBufferPrev != null)
        {
            m_DepthComputeBufferPrev.Release();
            m_DepthComputeBufferPrev = null;
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                var colorFrame = frame.ColorFrameReference.AcquireFrame();
                if (colorFrame != null)
                {
                    var depthFrame = frame.DepthFrameReference.AcquireFrame();
                    if (depthFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);
                        _ColorTexture.LoadRawTextureData(_ColorData);
                        _ColorTexture.Apply();


                        depthFrame.CopyFrameDataToArray(_DepthData);

                        m_DepthFloatDataPrev = m_DepthFloatData;

                        for (int i = 0; i < _DepthData.Length; i++)
                        {
                            m_DepthFloatData[i] = (float)_DepthData[i];
                        }


                        m_DepthComputeBuffer.SetData(m_DepthFloatData);
                        m_DepthComputeBufferPrev.SetData(m_DepthFloatDataPrev);
                        m_LayersComputeBuffer.SetData(m_Layers);

                        var settings = SettingsController.Instance.Current;

                        m_LayerRanges[0] = new LayerRange(0f, settings.DeepWaterMax);
                        m_LayerRanges[1] = new LayerRange(settings.DeepWaterMax, settings.WaterMax);
                        m_LayerRanges[2] = new LayerRange(settings.WaterMax, settings.ShallowsMax);
                        m_LayerRanges[3] = new LayerRange(settings.ShallowsMax, settings.SandMax);
                        m_LayerRanges[4] = new LayerRange(settings.SandMax, settings.GrassMax);
                        m_LayerRanges[5] = new LayerRange(settings.GrassMax, settings.ForestMax);
                        m_LayerRanges[6] = new LayerRange(settings.ForestMax, settings.RockMax);
                        m_LayerRanges[7] = new LayerRange(settings.RockMax, settings.SnowMax);
                        m_LayerRanges[8] = new LayerRange(settings.SnowMax, settings.LavaMax);

                        m_LayerRangesBuffer.SetData(m_LayerRanges);

                        m_DepthComputeShader.SetBuffer(m_KernalHandle, "KinectDepthNew", m_DepthComputeBuffer);
                        m_DepthComputeShader.SetBuffer(m_KernalHandle, "KinectDepthPrev", m_DepthComputeBufferPrev);
                        m_DepthComputeShader.SetBuffer(m_KernalHandle, "ResultLayers", m_LayersComputeBuffer);
                        m_DepthComputeShader.SetBuffer(m_KernalHandle, "LayerRanges", m_LayerRangesBuffer);
                        m_DepthComputeShader.SetFloat("rangeMin", SettingsController.Instance.Current.RangeMin);
                        m_DepthComputeShader.SetFloat("rangeMax", SettingsController.Instance.Current.RangeMax);
                        m_DepthComputeShader.SetFloat("weight", _Weight);

                        depthFrame.Dispose();
                        depthFrame = null;
                    }

                    colorFrame.Dispose();
                    colorFrame = null;
                }

                frame = null;
            }
        }
        //Process every frame, not just when a kinect frame is available so smoothing occurs when the kinect is slow
        m_DepthComputeShader.Dispatch(m_KernalHandle, _DepthRenderTexture.width /8, _DepthRenderTexture.height / 8, 1); 
        // Need to manually pull the layers data back out
        m_LayersComputeBuffer.GetData(m_Layers, 0, 0, m_Layers.Length);


    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
