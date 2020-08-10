// -----------------------------------------------
//内购相关逻辑
// -----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Reign;
using UnityEngine.Events;

public class IAP : MonoBehaviour
{
	private static bool created;

	public static string IAP_DZTS6
    {
		get
		{
			return "com.iafun.HZ.dzts6";
		}
	}
	public static string IAP_DZTS12
   {
		get
		{
			return "com.iafun.HZ.dzts12";
		}
	}

    public static string IAP_CKDA
    {
        get
        {
            return "com.iafun.HZ.ckda";
        }
    }

    public static string IAP_MDXJ
    {
        get
        {
            return "com.iafun.HZ.mdxj";
        }
    }


    public string GetAppIDByName(string name){
        if(name == "IAP_DZTS6")
        {
            return IAP_DZTS6;
        }
        else if(name == "IAP_DZTS12")
        {
            return IAP_DZTS12;
        }
        else if(name == "IAP_CKDA")
        {
            return IAP_CKDA;
        }
        else if(name == "IAP_MDXJ")
        {
            return IAP_MDXJ;
        }

        return "";
    }

	//购买事件 - result,inappid,receipt
	[System.Serializable] public class BuyCallbackEvent : UnityEvent<bool,string,string> {}
	public BuyCallbackEvent OnBuyCallback;

	//恢复事件 - result,inappid
	[System.Serializable] public class RestoreCallbackEvent : UnityEvent<bool,string> {}
	public RestoreCallbackEvent OnRestoreCallback;

	//获取商品信息事件result,inappid
	[System.Serializable] public class InAppPriceInfoEvent : UnityEvent<bool,string> {}
	public InAppPriceInfoEvent OnGetInAppPriceInfoCallback;


	void Start()
	{
		// make sure we don't init the same IAP items twice
		if (created) return;
		created = true;

		// InApp-Purchases - NOTE: you can set different "In App IDs" for each platform.
		var inAppIDs = new InAppPurchaseID[4];
        inAppIDs[0] = new InAppPurchaseID(IAP_DZTS6, 0.99m, "$", InAppPurchaseTypes.NonConsumable);
        inAppIDs[1] = new InAppPurchaseID(IAP_DZTS12, 1.99m, "$", InAppPurchaseTypes.NonConsumable);
        inAppIDs[2] = new InAppPurchaseID(IAP_CKDA, 0.99m, "$", InAppPurchaseTypes.NonConsumable);
        inAppIDs[3] = new InAppPurchaseID(IAP_MDXJ, 2.99m, "$", InAppPurchaseTypes.NonConsumable);
        // create desc object
        var desc = new InAppPurchaseDesc();

		// Global
		desc.Testing = false;
		desc.ClearNativeCache = false;
			
		// Editor
		desc.Editor_InAppIDs = inAppIDs;
			
		// WinRT
		//desc.WinRT_InAppPurchaseAPI = InAppPurchaseAPIs.MicrosoftStore;
		//desc.WinRT_MicrosoftStore_InAppIDs = inAppIDs;
			
		// WP8
		//desc.WP8_InAppPurchaseAPI = InAppPurchaseAPIs.MicrosoftStore;
		//desc.WP8_MicrosoftStore_InAppIDs = inAppIDs;
			
		// BB10
		//desc.BB10_InAppPurchaseAPI = InAppPurchaseAPIs.BlackBerryWorld;
		//desc.BB10_BlackBerryWorld_InAppIDs = inAppIDs;
	
		// iOS
		desc.iOS_InAppPurchaseAPI = InAppPurchaseAPIs.AppleStore;
		desc.iOS_AppleStore_InAppIDs = inAppIDs;
		desc.iOS_AppleStore_SharedSecretKey = "5f43658db9914759bd687f3ec25367b6";// NOTE: Must set SharedSecretKey, even for Testing!
		// Android
		// Choose for either GooglePlay or Amazon.
		// NOTE: Use "player settings" to define compiler directives.
		#if AMAZON
		desc.Android_InAppPurchaseAPI = InAppPurchaseAPIs.Amazon;
		#elif SAMSUNG
		desc.Android_InAppPurchaseAPI = InAppPurchaseAPIs.Samsung;
		#else
		desc.Android_InAppPurchaseAPI = InAppPurchaseAPIs.GooglePlay;
		#endif

		desc.Android_GooglePlay_InAppIDs = inAppIDs;
		desc.Android_GooglePlay_Base64Key = "";// NOTE: Must set Base64Key for GooglePlay in Apps to work, even for Testing!
		//desc.Android_Amazon_InAppIDs = inAppIDs;
		//desc.Android_Samsung_InAppIDs = inAppIDs;
		//desc.Android_Samsung_ItemGroupID = "";

		// init
		InAppPurchaseManager.Init(desc, createdCallback);
	}

	//购买
	public void onBuyClick(string inAppID)
	{
		InAppPurchaseManager.MainInAppAPI.Buy(inAppID, buyAppCallback);
	}

	//已经购买过，点击恢复购买
	public void onRestoreClick()
	{
		InAppPurchaseManager.MainInAppAPI.Restore(restoreAppsCallback);
	}

	//获取内购信息
	public void onGetPriceInfoClicked()
	{
		InAppPurchaseManager.MainInAppAPI.GetProductInfo(productInfoCallback);
	}

	//判断是否已经购买，如果担心破解，可以restore购买，来确信是否已经购买
	//也可以直接访问InAppPurchaseManager.MainInAppAPI.IsPurchased (inAppID) 省去挂脚本的多余步骤
	public static bool getHasBuy(string inAppID){
        #if UNITY_IPHONE
            return InAppPurchaseManager.MainInAppAPI.IsPurchased (inAppID);
        #else
            return true;
        #endif
	}

	public void clearAllDataForTest(){
		InAppPurchaseManager.MainInAppAPI.ClearPlayerPrefData ();
	}


	/// <summary>
	/// 处理
	/// </summary>
	/// <param name="succeeded">If set to <c>true</c> succeeded.</param>
	private void createdCallback(bool succeeded)
	{
		InAppPurchaseManager.MainInAppAPI.AwardInterruptedPurchases(awardInterruptedPurchases);
	}

	private void awardInterruptedPurchases(string inAppID, bool succeeded)
	{
		int appIndex = InAppPurchaseManager.MainInAppAPI.GetAppIndexForAppID(inAppID);
		if (appIndex != -1)
		{
			//StatusText.text += "Interrupted Restore Status: " + inAppID + ": " + succeeded + " Index: " + appIndex;
			//StatusText.text += System.Environment.NewLine + System.Environment.NewLine;
		}
	}

	private void productInfoCallback(InAppPurchaseInfo[] priceInfos, bool succeeded)
	{
		if (succeeded)
		{
			foreach (var info in priceInfos)
			{
				//if (info.ID == item1) StatusText.text += string.Format("ID: {0} Price: {1}", info.ID, info.FormattedPrice);
				// 商品信息，一般用不太上
				//OnGetInAppPriceInfoCallback.Invoke(true,);
			}
		}
		else
		{
//			OnGetInAppPriceInfoCallback.Invoke(false,);
		}
	}

	void buyAppCallback(string inAppID, string receipt, bool succeeded)
	{
		int appIndex = InAppPurchaseManager.MainInAppAPI.GetAppIndexForAppID(inAppID);
		if (succeeded && appIndex != -1)
		{
			//购买成功，向界面发送购买成功，刷新界面，解除加锁，设置可用
			OnBuyCallback.Invoke(true,inAppID,receipt);
		}
		else
		{
			//购买失败，向界面发送购买失败事件，也可以不处理
			OnBuyCallback.Invoke(false,inAppID,"");
		}
	}

	void restoreAppsCallback(string inAppID, bool succeeded)
	{
		int appIndex = InAppPurchaseManager.MainInAppAPI.GetAppIndexForAppID(inAppID);
		if (appIndex != -1)
		{
			//StatusText.text += "Restore Status: " + inAppID + ": " + succeeded + " Index: " + appIndex;
			//恢复购买成功，向界面发送成功事件，刷新界面，数据应该不需要存错了？
			OnRestoreCallback.Invoke(succeeded,inAppID);
		}
		else
		{
			//恢复购买失败，向界面发送失败
			OnRestoreCallback.Invoke(false,inAppID);
		}
	}
}
