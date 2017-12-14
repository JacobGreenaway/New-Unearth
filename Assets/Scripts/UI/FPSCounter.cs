using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

    static string[] stringsFrom00To99 = {
        "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
        "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
        "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
        "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
        "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
        "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
        "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
        "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
        "90", "91", "92", "93", "94", "95", "96", "97", "98", "99"
    };

    [SerializeField]
    private Text m_Text;

    private int m_AvgFps;

    private int m_FrameRange = 60;
    private int[] m_FpsBuffer;
    private int m_FpsBufferIndex;

    

    private void InitBuffer()
    {
        if(m_FrameRange <= 0)
        {
            m_FrameRange = 1;
        }
        m_FpsBuffer = new int[m_FrameRange];
        m_FpsBufferIndex = 0;
    }

    private void Update()
    {
        if(m_FpsBuffer == null || m_FpsBuffer.Length != m_FrameRange)
        {
            InitBuffer();
        }
        // Update Buffer
        m_FpsBuffer[m_FpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
        if(m_FpsBufferIndex >= m_FrameRange)
        {
            m_FpsBufferIndex = 0;
        }
        // Calc avg
        int sum = 0;
        for(int i = 0; i < m_FrameRange; i++)
        {
            sum += m_FpsBuffer[i];
        }
        m_AvgFps = sum / m_FrameRange;
        m_Text.text = stringsFrom00To99[Mathf.Clamp(m_AvgFps, 0, 99)];
    }
}
