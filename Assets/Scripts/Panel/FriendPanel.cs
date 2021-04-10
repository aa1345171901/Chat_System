﻿using Common;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendPanel : BasePanel
{
    private GameObject messageImg;
    private GameObject friendImg;
    private GameObject friendQImg;

    private Image face;

    private RectTransform content;
    private Transform reflash;   // 用于获取更新的panel位置，下拉更新

    private GetFriendListRequest getFriendListRequest;   // 获取好友列表的request

    private Dictionary<int, (string, int)> FriendDic = null;   // 用于设置好友列表 <账号，(昵称，头像ID)>
    private List<GameObject> friendGOs = new List<GameObject>();   // 好友列表Item列表，刷新时先删除

    private bool isSF = false;    // 判断是否释放，不能发送请求太快 

    private Button addFriendImg;   // 右上角的好友添加按钮
    private Button addFriendText;   // 列表的新朋友按钮

    private void Awake()
    {
        // 获取游戏物体
        messageImg = GameObject.Find("DownColumn/messageButton1");
        friendImg = GameObject.Find("DownColumn/friendButton1");
        friendQImg = GameObject.Find("DownColumn/friendQButton1");

        // 获取组件
        content = transform.Find("Scroll View/Viewport/Content").GetComponent<RectTransform>();
        reflash = transform.Find("Scroll View/Viewport/Reflash").GetComponent<Transform>();

        face = transform.Find("TopColumn/face").GetComponent<Image>();

        addFriendImg = transform.Find("TopColumn/AddFriendBtn").GetComponent<Button>();

        // 给物体添加事件
        messageImg.GetComponent<Button>().onClick.AddListener(OnClickMainBtn);
        friendQImg.GetComponent<Button>().onClick.AddListener(OnClickFriendQBtn);

        getFriendListRequest = GetComponent<GetFriendListRequest>();

        addFriendImg.onClick.AddListener(AddFriendClick);
    }

    /// <summary>
    /// Start周期发送获取好友列表的请求,添加搜索
    /// </summary>
    private void Start()
    {
        GameObject goSearch = Instantiate(Resources.Load<GameObject>("Item/InputField"));
        goSearch.transform.SetParent(content,false);

        // 设置layout空隙
        GameObject space = Instantiate(Resources.Load<GameObject>("Item/Spacing"));
        space.transform.SetParent(content, false);

        GameObject goNewFriend = Instantiate(Resources.Load<GameObject>("Item/NewFriend"));
        goNewFriend.transform.SetParent(content, false);
        
        // 给按钮添加事件
        addFriendText = goNewFriend.GetComponent<Button>();
        addFriendText.onClick.AddListener(AddFriendClick);

        // 设置layout空隙
        space = Instantiate(Resources.Load<GameObject>("Item/Spacing"));
        space.transform.SetParent(content, false);

        // 获取本地的好友列表
        string friendsStr = PlayerPrefs.GetString(Facade.GetUserData().LoginId + ",friends");
        if (!string.IsNullOrEmpty(friendsStr))
        {
            FriendDic = DataHelper.StringToDic(friendsStr);
        }
    }

    /// <summary>
    /// 每帧调用用于异步
    /// </summary>
    private void Update()
    {
        // 释放立即更新好友列表
        if (content.localPosition.y < -50)
        {
            isSF = true;
        }
        if (content.localPosition.y >= -5 && isSF)
        {
            isSF = false;
            GetFriendList();
            DateTime ReTime = DateTime.Now;
            string ReTimeStr = "    " + ReTime.Hour + " : " + ReTime.Minute + "\n" + ReTime.Year + " / " + ReTime.Month + " / " + ReTime.Day;
            PlayerPrefs.SetString(Facade.GetUserData().LoginId + ",ReTime", ReTimeStr);   // 通过id区分不同的账号刷新时间
        }

        if (FriendDic != null)
        {
            SetFriendItem();

            // 将friends保存到本地，不刷新时就可以不访问服务器
            PlayerPrefs.SetString(Facade.GetUserData().LoginId + ",friends", DataHelper.DicToString(FriendDic));
            FriendDic = null;
        }
    }

    /// <summary>
    /// panel进入
    /// </summary>
    public override void OnEnter()
    {
        gameObject.SetActive(true);
        messageImg.transform.localScale = new Vector3(1, 1, 1);
        friendQImg.transform.localScale = new Vector3(1, 1, 1);
        EnterAnimation(friendImg);

        // 给头像赋值
        string facePath = "FaceImage/" + Facade.GetUserData().FaceId;
        Sprite faceImg = Resources.Load<Sprite>(facePath);
        face.sprite = faceImg;
    }

    /// <summary>
    /// panel继续
    /// </summary>
    public override void OnResume()
    {
        // gameObject.SetActive(true);
    }

    /// <summary>
    /// panel退出
    /// </summary>
    public override void OnExit()
    {
        gameObject.SetActive(false);
        HideAnimation(friendImg);
    }

    /// <summary>
    /// panel暂停
    /// </summary>
    public override void OnPause()
    {
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// 联系人按钮点击
    /// </summary>
    private void OnClickMainBtn()
    {
        HideAnimation(messageImg);

        // 播放动画后push
        Invoke("PushMainPanel", 0.1f);
    }

    /// <summary>
    /// 动态按钮点击
    /// </summary>
    private void OnClickFriendQBtn()
    {
        HideAnimation(friendQImg);

        // 播放动画后push
        Invoke("PushFriendQPanel", 0.1f);
    }

    /// <summary>
    /// 点击push FriendQPanel
    /// </summary>
    /// <param name="uIPanelType"></param>
    private void PushMainPanel()
    {
        uiMng.PopPanel();
        uiMng.PushPanel(UIPanelType.MainPanel);
    }

    /// <summary>
    /// 点击push friendPanel
    /// </summary>
    /// <param name="uIPanelType"></param>
    private void PushFriendQPanel()
    {
        uiMng.PopPanel();
        uiMng.PushPanel(UIPanelType.FriendQPanel);
    }

    /// <summary>
    /// 图片渐显动画
    /// </summary>
    public void EnterAnimation(GameObject go)
    {
        go.transform.localScale = new Vector3(0, 0, 0);
        go.transform.DOScale(1, 0.2f);
    }

    /// <summary>
    /// 图片渐显=隐动画
    /// </summary>
    public void HideAnimation(GameObject go)
    {
        Tween tween = go.transform.DOScale(0, 0.1f);
    }

    /// <summary>
    /// 获取好友列表
    /// </summary>
    private void GetFriendList()
    {
        try
        {
            getFriendListRequest.SendRequest(Facade.GetUserData().GetId.ToString());
        }
        catch (System.Exception e)
        {
            uiMng.ShowMessage("暂时无法更新好友列表，请检查您的网络");
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// 获取好友列表反馈
    /// </summary>
    public void OnResponseGetFriendList(ReturnCode returnCode, Dictionary<int, (string, int)> friends)
    {
        if (returnCode == ReturnCode.Success)
        {
            FriendDic = friends;
        }
        else
        {
            uiMng.ShowMessageSync("无法获取好友列表,,ԾㅂԾ,,");
        }
    }

    /// <summary>
    /// 设置好友的Item
    /// </summary>
    private void SetFriendItem()
    {
        // 设置layout大小
        Vector2 size = content.sizeDelta;
        content.sizeDelta = new Vector2(size.x, 25 + 40 * (FriendDic.Count + 1));

        int index = 0;
        foreach (var item in FriendDic)
        {
            // friendGOs里有就不用生成
            GameObject go;
            if (index < friendGOs.Count)
            {
                go = friendGOs[index++];
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(Resources.Load<GameObject>("Item/Friend"));
                friendGOs.Add(go);

                // 不自增只能生成一个物体
                index++;
            }
            string nickName = item.Value.Item1;
            int faceId = item.Value.Item2;

            // 设置子物体属性
            go.GetComponentInChildren<Text>().text = nickName;

            string facePath = "FaceImage/" + faceId;
            Sprite face = Resources.Load<Sprite>(facePath);
            go.transform.Find("FaceMask/Image").GetComponent<Image>().sprite = face;

            // 设置父物体
            go.transform.SetParent(content, false);
        }
        // 更新后好友数量少了，将后面的设为不可见
        if (index < friendGOs.Count)
        {
            for(int i = index;i < friendGOs.Count; i++)
            {
                friendGOs[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// 添加好友按钮点击
    /// </summary>
    private void AddFriendClick()
    {
        uiMng.PushPanel(UIPanelType.AddFriendPanel);
    }
}
