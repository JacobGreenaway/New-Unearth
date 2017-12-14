using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingsInputQuadButton : MonoBehaviour
{
    [SerializeField]
    private Text m_Text;
    [SerializeField]
    private Button m_Button1;
    [SerializeField]
    private Button m_Button2;
    [SerializeField]
    private Button m_Button3;
    [SerializeField]
    private Button m_Button4;

    private Action m_Button1Action;
    private Action m_Button2Action;
    private Action m_Button3Action;
    private Action m_Button4Action;

    private void Awake()
    {
        m_Button1.onClick.AddListener(HandleButton1Click);
        m_Button2.onClick.AddListener(HandleButton2Click);
        m_Button3.onClick.AddListener(HandleButton3Click);
        m_Button4.onClick.AddListener(HandleButton4Click);
    }

    public void SetText(string text)
    {
        m_Text.text = text;
    }

    public void SetButton1Action(Action action)
    {
        m_Button1Action = action;
    }

    public void SetButton2Action(Action action)
    {
        m_Button2Action = action;
    }

    public void SetButton3Action(Action action)
    {
        m_Button3Action = action;
    }

    public void SetButton4Action(Action action)
    {
        m_Button4Action = action;
    }

    private void HandleButton1Click()
    {
        m_Button1Action?.Invoke();
    }

    private void HandleButton2Click()
    {
        m_Button2Action?.Invoke();
    }

    private void HandleButton3Click()
    {
        m_Button3Action?.Invoke();
    }

    private void HandleButton4Click()
    {
        m_Button4Action?.Invoke();
    }
}
