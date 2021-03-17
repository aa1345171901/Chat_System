﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour {

    protected UIManager uiMng;
    protected Facade _facade;

    public Facade Facade
    {
        get
        {
            if (_facade == null)
            {
                _facade = Facade.Instance;
            }
            return _facade;
        }
        set
        {
            _facade = value;
        }
    }

    public UIManager UIMng
    {
        set { uiMng = value; }
    }

    /// <summary>
    /// 界面打开时的动作
    /// </summary>
	public virtual void OnEnter()
    {

    }

    /// <summary>
    /// 界面暂停，时的事件
    /// </summary>
    public virtual void OnPause()
    {

    }

    /// <summary>
    /// 界面继续的事件
    /// </summary>
    public virtual void OnResume()
    {

    }

    /// <summary>
    /// 界面退出时的事件
    /// </summary>
    public virtual void OnExit()
    {

    }
}
