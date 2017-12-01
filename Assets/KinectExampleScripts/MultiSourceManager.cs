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
    private ushort[] _DepthData;
    private byte[] _ColorData;
    private byte[] _RawDepthData;
    private Color[] _DepthColorData;

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
            _RawDepthData = new byte[depthFrameDesc.LengthInPixels * 4];
            _DepthTexture = new Texture2D(depthFrameDesc.Width, depthFrameDesc.Height, TextureFormat.RGBAFloat, false);
            _DepthColorData = new Color[depthFrameDesc.LengthInPixels];

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
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
                        ushort largestValue = 0;
                        for (int i = 0; i < _DepthData.Length; ++i)
                        {
                            if(_DepthData[i]> largestValue)
                            {
                                largestValue = _DepthData[i];
                            }
                            var startIndex = 4 * i;
                            _RawDepthData[startIndex] = (byte)((_DepthData[i] / 8000f)*255);
                            _RawDepthData[startIndex + 1] = _RawDepthData[startIndex];
                            _RawDepthData[startIndex + 2] = _RawDepthData[startIndex];
                            _RawDepthData[startIndex + 3] = (byte)255;

                            _DepthColorData[i] = new Color(_DepthData[i] / 8000f, 0f, 0f, 1f);
                        }
                        //_DepthTexture.LoadRawTextureData(_RawDepthData);
                        _DepthTexture.SetPixels(_DepthColorData);
                        _DepthTexture.Apply();

                        depthFrame.Dispose();
                        depthFrame = null;
                    }

                    colorFrame.Dispose();
                    colorFrame = null;
                }

                frame = null;
            }
        }
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
