using UnityEngine;
using Windows.Kinect;

public class UnearthDepthController : MonoBehaviour
{
    private KinectSensor m_Sensor;
    private DepthFrameReader m_Reader;
    private ushort[] m_DepthData;
    private RenderTexture m_DepthRenderTexture;

    [SerializeField]
    private float m_SmoothingWeight;
    [SerializeField]
    private ComputeShader m_DepthComputeShader;
    private ComputeBuffer m_DepthComputeBuffer0;
    private ComputeBuffer m_DepthComputeBuffer1;
    private ComputeBuffer m_DepthComputeBuffer2;
    private ComputeBuffer m_DepthComputeBuffer3;

    private float[][] m_DepthFloatData;
    private int currentBuffer = 0;
    private const int BufferLength = 4;

    private int m_Width;
    private int m_Height;

    private int m_KernalHandle;

    public ushort[] GetDepthBuffer()
    {
        return m_DepthData;
    }

    public RenderTexture GetDepthRenderTexture()
    {
        return m_DepthRenderTexture;
    }

    private void Start()
    {
        m_Sensor = KinectSensor.GetDefault();

        if(m_Sensor != null)
        {
            m_Reader = m_Sensor.DepthFrameSource.OpenReader();
            var depthFrameDesc = m_Sensor.DepthFrameSource.FrameDescription;
            // Init Raw Data array
            m_DepthData = new ushort[depthFrameDesc.LengthInPixels];
            // Init Depth Render Texture for compute shader
            m_DepthRenderTexture = new RenderTexture(depthFrameDesc.Width, depthFrameDesc.Height, 16);
            m_DepthRenderTexture.wrapMode = TextureWrapMode.Clamp;
            m_DepthRenderTexture.filterMode = FilterMode.Point;
            m_DepthRenderTexture.enableRandomWrite = true;
            m_DepthRenderTexture.Create();
            // Init compute shader and buffers
            m_KernalHandle = m_DepthComputeShader.FindKernel("GenerateDepth");
            m_DepthComputeShader.SetTexture(m_KernalHandle, "Result", m_DepthRenderTexture);
            m_DepthComputeBuffer0 = new ComputeBuffer(m_DepthData.Length, (int)sizeof(float), ComputeBufferType.Raw);
            m_DepthComputeBuffer1 = new ComputeBuffer(m_DepthData.Length, (int)sizeof(float), ComputeBufferType.Raw);
            m_DepthComputeBuffer2 = new ComputeBuffer(m_DepthData.Length, (int)sizeof(float), ComputeBufferType.Raw);
            m_DepthComputeBuffer3 = new ComputeBuffer(m_DepthData.Length, (int)sizeof(float), ComputeBufferType.Raw);
            m_DepthFloatData = new float[BufferLength][];

            m_Width = depthFrameDesc.Width;
            m_Height = depthFrameDesc.Height;
            for(int i = 0; i < BufferLength; i++)
            {
                m_DepthFloatData[i] = new float[depthFrameDesc.LengthInPixels];
            }

            if (!m_Sensor.IsOpen)
            {
                m_Sensor.Open();
            }
        }
    }

    private void Update()
    {
        bool updated = false;
        if(m_Reader != null)
        {
            DepthFrame frame = m_Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(m_DepthData);
                frame.Dispose();
                frame = null;

                currentBuffer = GetBufferOffset(1);
                for(int i = 0; i < m_DepthData.Length; i++)
                {
                    m_DepthFloatData[currentBuffer][i] = m_DepthData[i];
                }

                updated = true;
            }
        }
        if(!updated)
        {
            // Force forward and copy previous buffer
            var prevBuffer = currentBuffer;
            currentBuffer = GetBufferOffset(1);
            for (int i = 0; i < m_DepthData.Length; i++)
            {
                m_DepthFloatData[currentBuffer][i] = m_DepthFloatData[prevBuffer][i];
            }
        }
        //Process every frame, not just when a kinect frame is available so smoothing occurs when the kinect is slow
        
        m_DepthComputeBuffer0.SetData(m_DepthFloatData[currentBuffer]);
        m_DepthComputeBuffer1.SetData(m_DepthFloatData[GetBufferOffset(1)]);
        m_DepthComputeBuffer2.SetData(m_DepthFloatData[GetBufferOffset(2)]);
        m_DepthComputeBuffer3.SetData(m_DepthFloatData[GetBufferOffset(3)]);
        m_DepthComputeShader.SetBuffer(m_KernalHandle, "KinectDepth0", m_DepthComputeBuffer0);
        m_DepthComputeShader.SetBuffer(m_KernalHandle, "KinectDepth1", m_DepthComputeBuffer1);
        m_DepthComputeShader.SetBuffer(m_KernalHandle, "KinectDepth2", m_DepthComputeBuffer2);
        m_DepthComputeShader.SetBuffer(m_KernalHandle, "KinectDepth3", m_DepthComputeBuffer3);
        m_DepthComputeShader.SetFloat("rangeMin", SettingsController.Instance.Current.RangeMin);
        m_DepthComputeShader.SetFloat("rangeMax", SettingsController.Instance.Current.RangeMax);
        m_DepthComputeShader.SetInt("width", m_Width);
        m_DepthComputeShader.SetInt("height", m_Height);

        m_DepthComputeShader.SetFloat("weight", m_SmoothingWeight);
        m_DepthComputeShader.Dispatch(m_KernalHandle, m_DepthRenderTexture.width, m_DepthRenderTexture.height, 1);
    }

    private int GetBufferOffset(int offset)
    {
        var off = currentBuffer + offset;
        while(off >= BufferLength)
        {
            off -= BufferLength;
        }
        return off;
    }
    
    private void OnDestroy()
    {
        if (m_DepthComputeBuffer0 != null)
        {
            m_DepthComputeBuffer0.Release();
            m_DepthComputeBuffer0 = null;
        }

        if (m_DepthComputeBuffer1 != null)
        {
            m_DepthComputeBuffer1.Release();
            m_DepthComputeBuffer1 = null;
        }

        if(m_DepthComputeBuffer2 != null)
        {
            m_DepthComputeBuffer2.Release();
            m_DepthComputeBuffer2 = null;
        }

        if(m_DepthComputeBuffer3 != null)
        {
            m_DepthComputeBuffer3.Release();
            m_DepthComputeBuffer3 = null;
        }
    }

    void OnApplicationQuit()
    {
        if (m_Reader != null)
        {
            m_Reader.Dispose();
            m_Reader = null;
        }

        if (m_Sensor != null)
        {
            if (m_Sensor.IsOpen)
            {
                m_Sensor.Close();
            }

            m_Sensor = null;
        }
    }
}
