﻿using UnityEngine;
using System.Collections;

namespace TinyFrameWork
{
    public class UITopBar : UIBaseWindow
    {
        private GameObject btnReturn;
        private GameObject btnShowMsg;

        public override void InitWindowOnAwake()
        {
            this.windowID = WindowID.WindowID_TopBar;
            this.windowData.windowType = UIWindowType.Fixed;

            base.InitWindowOnAwake();
            btnReturn = GameUtility.FindDeepChild(this.gameObject, "TopLeft/Btn_Rtn").gameObject;
            btnShowMsg = GameUtility.FindDeepChild(this.gameObject, "TopRight/Meg/Sprite").gameObject;

            UIEventListener.Get(btnReturn).onClick = delegate
            {
                UICenterMasterManager.GetInstance().ReturnWindow();
            };

            // message box Test.
            UIEventListener.Get(btnShowMsg).onClick = delegate
            {
                // UIManager.GetInstance().ShowMessageBox("Hello World!");
                
                //UIManager.GetInstance().ShowMessageBox(
                //    "Do you want to quit this game?", 
                //    "Yes",
                //    delegate
                //    {
                //        Debug.Log("Message Box click YES.");
                //        UIManager.GetInstance().CloseMessageBox();
                //    },
                //    "No",
                //    delegate
                //    {
                //        Debug.Log("Message Box click NO.");
                //        UIManager.GetInstance().CloseMessageBox();
                //    });

                UICenterMasterManager.GetInstance().ShowMessageBox(
                    "You are yourself, please don't lose confidence.",
                    "Sure!",
                    delegate
                    {
                        UICenterMasterManager.GetInstance().CloseMessageBox();
                    });
            };
        }

        public override void ShowWindow()
        {
            this.gameObject.SetActive(true);
        }
    }
}

