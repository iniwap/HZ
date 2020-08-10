/**************************************/
//FileName: Tool.cs
//Author: wtx
//Data: 23/03/2018
//Describe:  统计事件相关
/**************************************/
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJson;
using Reign;
using Umeng;

public class EventReport: MonoBehaviour{

	public enum EventType{
		NoneType,
        Dzts6BuyClick,
        Dzts12BuyClick,
        CkdaBuyClick,
        JmslBuyClick,
        MtxjBuyClick,
    };
	// Use this for initialization
	void Start () {
		#if UNITY_ANDROID

		#elif UNITY_IPHONE		

		GA.StartWithAppKeyAndReportPolicyAndChannelId ("5d4fc890570df3cc1c00031c", Analytics.ReportPolicy.BATCH,"App Store");

#else

#endif
        //仍旧需要
        //UnityAppController.mm文件,使用头文件#import "UNUMConfigure.h"并在didFinishLaunchingWithOptions中添加
        //[UNUMConfigure initWithAppkey:@"5d4fc890570df3cc1c00031c" channel:@"App Store"];
    }

    void OnDestroy(){
		
	}
	void OnEnable(){

	}
	void OnDisable(){
		
	}

	public void OnEventReport(EventType type){
		GA.Event (""+type);
	}
	public void OnEventReport(string type){
		GA.Event (type);
	}

	public enum BuyType{
		BuySuccess,//购买笔刷成功,0
		BuyFail,//购买笔刷失败,0
		BuyRestore,
	};
}
