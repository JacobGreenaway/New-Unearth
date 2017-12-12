using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MultiSourceManager : MonoBehaviour
{
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
    private int _MinRange;
    [SerializeField]
    private int _MaxRange;
    [SerializeField]
    private float _Weight;

    [SerializeField]
    private ComputeShader m_DepthComputeShader;
    private ComputeBuffer m_DepthComputeBuffer;
    private ComputeBuffer m_DepthComputeBufferPrev;
    private float[] m_DepthFloatData;
    private float[] m_DepthFloatDataPrev;
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
            m_DepthFloatData = new float[depthFrameDesc.LengthInPixels];
            m_DepthFloatDataPrev = new float[depthFrameDesc.LengthInPixels];
            _DepthColorData = new Color[depthFrameDesc.LengthInPixels];

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    private void OnDestroy()
    {
        if(m_DepthComputeBuffer != null)
        {
            m_DepthComputeBuffer.Release();
            m_DepthComputeBuffer = null;
        }

        if(m_DepthComputeBufferPrev != null)
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
                        m_DepthComputeShader.SetBuffer(m_KernalHandle, "KinectDepthNew", m_DepthComputeBuffer);
                        m_DepthComputeShader.SetBuffer(m_KernalHandle, "KinectDepthPrev", m_DepthComputeBufferPrev);
                        m_DepthComputeShader.SetFloat("rangeMin", (float)_MinRange);
                        m_DepthComputeShader.SetFloat("rangeMax", (float)_MaxRange);
                        m_DepthComputeShader.SetFloat("weight", _Weight);
                        //m_DepthComputeShader.SetInt("width", _DepthRenderTexture.width);
                        // m_DepthComputeShader.SetInt("rangeMin", _MinRange);
                        //m_DepthComputeShader.SetInt("rangeMax", _MaxRange);
                        //m_DepthComputeShader.Dispatch(m_KernalHandle, _DepthRenderTexture.width/8, _DepthRenderTexture.height/8, 1);

                        //ushort largestValue = 0;
                        //for (int i = 0; i < _DepthData.Length; ++i)
                        //{
                        //    //    if (_DepthData[i] > largestValue)
                        //    //    {
                        //    //        largestValue = _DepthData[i];
                        //    //    }
                        //    //    //var startIndex = 4 * i;
                        //    //    //_RawDepthData[startIndex] = (byte)((_DepthData[i] / 8000f)*255);
                        //    //    //_RawDepthData[startIndex + 1] = _RawDepthData[startIndex];
                        //    //    // _RawDepthData[startIndex + 2] = _RawDepthData[startIndex];
                        //    //    //_RawDepthData[startIndex + 3] = (byte)255;

                        //    float data = _DepthData[i] < _MinRange || _DepthData[i] > _MaxRange ? 0f : ((_DepthData[i] - _MinRange) / (float)(_MaxRange - _MinRange));

                        //    _DepthColorData[i] = new Color(data, 0f, 0f, 1f);
                        //}
                        ////_DepthTexture.LoadRawTextureData(_RawDepthData);
                        //_DepthTexture.SetPixels(_DepthColorData);
                        //_DepthTexture.Apply();

                        depthFrame.Dispose();
                        depthFrame = null;
                    }

                    colorFrame.Dispose();
                    colorFrame = null;
                }

                frame = null;
            }
        }
        m_DepthComputeShader.Dispatch(m_KernalHandle, _DepthRenderTexture.width / 8, _DepthRenderTexture.height / 8, 1);
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
