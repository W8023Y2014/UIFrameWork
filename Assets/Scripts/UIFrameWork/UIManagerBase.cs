﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TinyFrameWork
{
    /// <summary>
    /// IUIManager
    ///     界面管理基类
    ///     管理子界面
    ///          
    ///
    /// </summary>
    public abstract class UIManagerBase : MonoBehaviour
    {
        protected Dictionary<int, UIBaseWindow> allWindows;
        protected Dictionary<int, UIBaseWindow> shownWindows;
        protected Stack<BackWindowSequenceData> backSequence;
        // 当前显示活跃界面
        protected UIBaseWindow curShownNormalWindow = null;
        // 上一活跃界面
        protected UIBaseWindow lastShownNormalWindow = null;

        // 是否等待关闭结束
        // 开启:等待界面关闭结束,处理后续逻辑
        // 关闭:不等待界面关闭结束，处理后续逻辑
        protected bool isNeedWaitHideOver = false;

        // 管理的界面ID
        protected int managedWindowId = 0;

        // 界面按MinDepth排序
        protected class CompareBaseWindow : IComparer<UIBaseWindow>
        {
            public int Compare(UIBaseWindow left, UIBaseWindow right)
            {
                return left.MinDepth - right.MinDepth;
            }
        }

        protected virtual void Awake()
        {
            if (allWindows == null)
                allWindows = new Dictionary<int, UIBaseWindow>();
            if (shownWindows == null)
                shownWindows = new Dictionary<int, UIBaseWindow>();
            if (backSequence == null)
                backSequence = new Stack<BackWindowSequenceData>();
        }

        public virtual UIBaseWindow GetGameWindow(WindowID id)
        {
            if (!IsWindowInControl(id))
                return null;
            if (allWindows.ContainsKey((int)id))
                return allWindows[(int)id];
            else
                return null;
        }

        public virtual T GetGameWindowScript<T>(WindowID id) where T : UIBaseWindow
        {
            UIBaseWindow baseWindow = GetGameWindow(id);
            if (baseWindow != null)
                return (T)baseWindow;
            return (T)((object)null);
        }

        /// <summary>
        /// 初始化当前界面管理类
        /// </summary>
        public virtual void InitWindowManager()
        {
            if (allWindows != null)
                allWindows.Clear();
            if (shownWindows != null)
                shownWindows.Clear();
            if (backSequence != null)
                backSequence.Clear();
        }

        /// <summary>
        /// 显示界面
        /// </summary>
        /// <param name="id">界面ID</param>
        public virtual void ShowWindow(WindowID id, ShowWindowData data = null)
        {

        }

        /// <summary>
        /// Delay 显示界面
        /// </summary>
        /// <param name="delayTime"> 延迟时间</param>
        /// <param name="id"> 界面ID</param>
        /// <param name="data">显示数据</param>
        public virtual void ShowWindowDelay(float delayTime, WindowID id, ShowWindowData data = null)
        {
            StartCoroutine(_ShowWindowDelay(delayTime, id, data));
        }

        private IEnumerator _ShowWindowDelay(float delayTime, WindowID id, ShowWindowData data = null)
        {
            yield return new WaitForSeconds(delayTime);
            ShowWindow(id, data);
        }

        protected virtual UIBaseWindow ReadyToShowBaseWindow(WindowID id, ShowWindowData showData = null)
        {
            return null;
        }

        /// <summary>
        /// 显示界面，方面在现实之前做其他操作
        /// </summary>
        protected virtual void RealShowWindow(UIBaseWindow baseWindow, WindowID id)
        {
            baseWindow.ShowWindow();
            shownWindows[(int)id] = baseWindow;
            if (baseWindow.windowData.windowType == UIWindowType.Normal)
            {
                // 改变当前显示Normal窗口
                lastShownNormalWindow = curShownNormalWindow;
                curShownNormalWindow = baseWindow; 
            }
        }

        protected void ShowWindowForBack(WindowID id)
        {
            if (!this.IsWindowInControl(id))
            {
                Debug.Log("UIManager has no control power of " + id.ToString());
                return;
            }
            if (shownWindows.ContainsKey((int)id))
                return;

            UIBaseWindow baseWindow = GetGameWindow(id);
            baseWindow.ShowWindow();
            shownWindows[(int)baseWindow.GetID] = baseWindow;
        }

        /// <summary>
        /// Hide target window
        /// </summary>
        /// <param name="id"></param>
        public virtual void HideWindow(WindowID id, Action onCompleted = null)
        {
            CheckDirectlyHide(id, onCompleted);
        }

        protected virtual void CheckDirectlyHide(WindowID id, Action onComplete)
        {
            if (!IsWindowInControl(id))
            {
                Debug.Log("UIRankManager has no control power of " + id.ToString());
                return;
            }
            if (!shownWindows.ContainsKey((int)id))
                return;

            if (!isNeedWaitHideOver)
            {
                if (onComplete != null)
                    onComplete();

                shownWindows[(int)id].HideWindow(null);
                shownWindows.Remove((int)id);
                return;
            }

            if (shownWindows.ContainsKey((int)id))
            {
                if (onComplete != null)
                {
                    onComplete += delegate
                    {
                        shownWindows.Remove((int)id);
                    };
                    shownWindows[(int)id].HideWindow(onComplete);
                }
                else
                {
                    shownWindows[(int)id].HideWindow(onComplete);
                    shownWindows.Remove((int)id);
                }
            }
        }

        /// <summary>
        /// 返回逻辑
        /// </summary>
        public virtual bool ReturnWindow()
        {
            return false;
        }

        private bool ReturnWindowManager(UIBaseWindow baseWindow)
        {
            // Recursion call to return windowManager
            // if the current window has windowManager just call current's windowManager ReturnWindowManager
            UIManagerBase baseWindowManager = baseWindow.GetWindowManager;
            bool isValid = false;
            if (baseWindowManager != null)
                isValid = baseWindowManager.ReturnWindow();
            return isValid;
        }

        protected bool RealReturnWindow()
        {
            if (backSequence.Count == 0)
            {
                // 如果当前BackSequenceData 不存在返回数据
                // 检测当前Window的preWindowId是否指向上一级合法指定菜单

                // if BackSequenceData is null
                // Check window's preWindowId
                // if preWindowId defined just move to target Window(preWindowId)
                if (curShownNormalWindow == null)
                    return false;
                if (ReturnWindowManager(curShownNormalWindow))
                    return true;

                WindowID preWindowId = curShownNormalWindow.GetPreWindowID;
                if (preWindowId != WindowID.WindowID_Invaild)
                {
                    HideWindow(curShownNormalWindow.GetID, delegate
                    {
                        ShowWindow(preWindowId, null); 
                    });
                }
                else
                    Debug.LogWarning("## CurrentShownWindow " + curShownNormalWindow.GetID + " preWindowId is " + WindowID.WindowID_Invaild);
                return false;
            }
            BackWindowSequenceData backData = backSequence.Peek();
            if (backData != null)
            {
                if (ReturnWindowManager(backData.hideTargetWindow))
                    return true;

                WindowID hideId = backData.hideTargetWindow.GetID;
                if (backData.hideTargetWindow != null && shownWindows.ContainsKey((int)hideId))
                    HideWindow(hideId, delegate
                    {
                        if (backData.backShowTargets != null)
                        {
                            for (int i = 0; i < backData.backShowTargets.Count; i++)
                            {
                                WindowID backId = backData.backShowTargets[i];
                                ShowWindowForBack(backId);
                                if (i == backData.backShowTargets.Count - 1)
                                {
                                    Debug.Log("change currentShownNormalWindow : " + backId);
                                    {
                                        this.lastShownNormalWindow = this.curShownNormalWindow;
                                        this.curShownNormalWindow = GetGameWindow(backId); 
                                    }
                                }
                            }
                        }
                        backSequence.Pop();
                    });
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Clear the back sequence data
        /// </summary>
        public void ClearBackSequence()
        {
            if (backSequence != null)
                backSequence.Clear();
        }

        /// <summary>
        /// Destroy all window
        /// </summary>
        public virtual void ClearAllWindow()
        {
            if (allWindows != null)
            {
                foreach (KeyValuePair<int, UIBaseWindow> window in allWindows)
                {
                    UIBaseWindow baseWindow = window.Value;
                    baseWindow.DestroyWindow();
                }
                allWindows.Clear();
                shownWindows.Clear();
                backSequence.Clear();
            }
        }

        protected void HideAllShownWindow(bool includeFixed = true)
        {
            List<WindowID> removedKey = null;

            if (!includeFixed)
            {
                foreach (KeyValuePair<int, UIBaseWindow> window in shownWindows)
                {
                    if (window.Value.windowData.windowType == UIWindowType.Fixed)
                        continue;

                    if (removedKey == null)
                        removedKey = new List<WindowID>();

                    removedKey.Add((WindowID)window.Key);
                    window.Value.HideWindowDirectly();
                }

                if (removedKey != null)
                {
                    for (int i = 0; i < removedKey.Count; i++)
                        shownWindows.Remove((int)removedKey[i]);
                }
            }
            else
            {
                foreach (KeyValuePair<int, UIBaseWindow> window in shownWindows)
                    window.Value.HideWindowDirectly();
                shownWindows.Clear();
            }
        }

        // check window control
        protected bool IsWindowInControl(WindowID id)
        {
            int targetId = 1 << ((int)id);
            return ((managedWindowId & targetId) == targetId);
        }

        // add window to target manager
        protected void AddWindowInControl(WindowID id)
        {
            int targetId = 1 << ((int)id);
            managedWindowId |= targetId;
        }

        // init the Manager's control window
        protected abstract void InitWindowControl();
        public virtual void ResetAllInControlWindows()
        {
        }
    }
}