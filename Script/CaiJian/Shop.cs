/**************************************/
//FileName: Setting.cs
//Author: wtx
//Data: 14/01/2019
//Describe:  设置界面
/**************************************/
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class Shop: MonoBehaviour{
    // Use this for initialization
    void Start()
    {

    }

    void OnDestroy()
    {

    }
    void OnEnable()
    {
        InitBuy();
    }
    void OnDisable()
    {

    }


    private bool _HasInited = false;


    public void OnInit()
    {
        //
        if(!_HasInited){

        }
        //
        _HasInited = true;

    }

    public IAP _iap;
    public GameObject _restoreBtn;

    public Button _Dzts6Btn;
    public Button _Dzts12Btn;
    public Button _CkdaBtn;
    public Button _MtxjBtn;

    public GameObject _Dzts6Lock;
    public GameObject _Dzts12Lock;
    public GameObject _CkdaLock;
    public GameObject _MtxjLock;

    public void InitBuy(){
        bool hasBuyDzts6 = IAP.getHasBuy(IAP.IAP_DZTS6);
        bool hasBuyDzts12 = IAP.getHasBuy(IAP.IAP_DZTS12);
        bool hasBuyCkda = IAP.getHasBuy(IAP.IAP_CKDA);
        bool hasBuyMtxj = IAP.getHasBuy(IAP.IAP_MDXJ);

        _Dzts6Btn.interactable = !hasBuyDzts6;
        _Dzts12Btn.interactable = !hasBuyDzts12;
        _CkdaBtn.interactable = !hasBuyCkda;
        _MtxjBtn.interactable = !hasBuyMtxj;

        _Dzts6Lock.SetActive(!hasBuyDzts6);
        _Dzts12Lock.SetActive(!hasBuyDzts12);
        _CkdaLock.SetActive(!hasBuyCkda);
        _MtxjLock.SetActive(!hasBuyMtxj);

        if (IAP.getHasBuy(IAP.IAP_CKDA) 
           && IAP.getHasBuy(IAP.IAP_MDXJ)
           && IAP.getHasBuy(IAP.IAP_DZTS6)
           && IAP.getHasBuy(IAP.IAP_DZTS12))
        {
            _restoreBtn.SetActive(false);
        }
        else{
            _restoreBtn.SetActive(true);
        }
    }

    public void OnBuyBtn(string appID)
    {
        if (!_ProcessingPurchase)
        {
            ShowToast("正在发起购买，请稍候...");

            _ProcessingPurchase = true;
            _iap.onBuyClick(_iap.GetAppIDByName(appID));
        }
        else
        {
            ShowToast("购买正在处理进行中，请稍候...");
        }

    }

    // 恢复购买
    public void OnRestoreBtn(){
        if (!_ProcessingPurchase)
        {
            ShowToast("正在发起恢复购买，请稍候...");

            _ProcessingPurchase = true;
            _iap.onRestoreClick();
        }
        else
        {
            ShowToast("恢复购买正在处理中，请稍候...");
        }
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        ShowToast(content, Define.BG_COLOR, showTime, delay);
    }

    public void ShowToast(string content, Color c, float showTime = 2.0f, float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = c;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }

    //-----------------------内购-------------------------------
    private bool _ProcessingPurchase = false;
    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        _ProcessingPurchase = false;
        if (ret)
        {
            // 购买成功，根据inAppID，刷新界面
            if (!string.IsNullOrEmpty(receipt))
            {
                //说明是真正的购买，其他可能是正在购买或者已经购买过了
            }

            UnLockBuy(inAppID);

            OnReportEvent(ret, inAppID, EventReport.BuyType.BuySuccess);
        }
        else
        {
            //简单提示，购买失败
            ShowToast("购买失败，请稍候再试：(");
            OnReportEvent(ret, inAppID, EventReport.BuyType.BuyFail);
        }
    }

    public void OnRestoreCallback(bool ret, string inAppID)
    {
        _ProcessingPurchase = false;

        if (ret)
        {
            UnLockBuy(inAppID,true);
        }
        else
        {
            //MessageBoxManager.Show("", "恢复购买失败，请确认是否购买过？");
        }

        OnReportEvent(ret, inAppID, EventReport.BuyType.BuyRestore);
    }

    public void OnGetInAppPriceInfoCallback(bool ret, string info)
    {
        if (ret)
        {
            // 
        }
        else
        {
            //
        }
    }

    private void UnLockBuy(string inAppID,bool restore = false)
    {
        //当前不在商城界面，不需要处理动画
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (inAppID == IAP.IAP_CKDA)
        {
            _CkdaBtn.interactable = false;
            //如果lock不可见，不必执行动画
            if (_CkdaLock.activeSelf){
                DoUnLockAni(_CkdaLock, ()=>{
                    _CkdaLock.SetActive(false);
                    _CkdaBtn.interactable = false;
                });
            }
            if(restore)
            {
                ShowToast("恢复购买【查看答案】成功，每日可使用6次哟～");
            }
            else
            {
                ShowToast("购买【查看答案】成功，每日可使用6次哟～");
            }
        }
        else if (inAppID == IAP.IAP_MDXJ)
        {
            _MtxjBtn.interactable = false;
            //如果lock不可见，不必执行动画
            if (_MtxjLock.activeSelf)
            {
                DoUnLockAni(_MtxjLock, () => {
                    _MtxjLock.SetActive(false);
                    _MtxjBtn.interactable = false;
                });
            }

            if (restore)
            {
                ShowToast("恢复购买【谜底详解】成功，快去【猜一猜】使用吧～");
            }
            else
            {
                ShowToast("购买【谜底详解】成功，快去【猜一猜】使用吧～");
            }
        }
        else if (inAppID == IAP.IAP_DZTS6)
        {
            _Dzts6Btn.interactable = false;
            //如果lock不可见，不必执行动画
            if (_Dzts6Lock.activeSelf)
            {
                DoUnLockAni(_Dzts6Lock, () => {
                    _Dzts6Lock.SetActive(false);
                    _Dzts6Btn.interactable = false;
                });
            }

            if (IAP.getHasBuy(IAP.IAP_DZTS12))
            {
                if(restore)
                {
                    ShowToast("恢复购买【单件提示6】成功，每日可用次数：<b>12+6次+免费3次</b>");
                }
                else
                {
                    ShowToast("购买【单件提示6】成功，每日可用次数：<b>12+6次+免费3次</b>");
                }
            }
            else
            {
                if(restore)
                {
                    ShowToast("恢复购买【单件提示6】成功，每日可用次数：<b>6次+免费3次</b>");
                }
                else
                {
                    ShowToast("购买【单件提示6】成功，每日可用次数：<b>6次+免费3次</b>");
                }
            }
        }
        else if (inAppID == IAP.IAP_DZTS12)
        {
            _Dzts12Btn.interactable = false;
            //如果lock不可见，不必执行动画
            if (_Dzts12Lock.activeSelf)
            {
                DoUnLockAni(_Dzts12Lock, () => {
                    _Dzts12Lock.SetActive(false);
                    _Dzts12Btn.interactable = false;
                });
            }
            if (IAP.getHasBuy(IAP.IAP_DZTS6))
            {
                if (restore)
                {
                    ShowToast("恢复购买【单件提示12】成功，每日可用次数：<b>6+12次+免费3次</b>");
                }
                else
                {
                    ShowToast("购买【单件提示12】成功，每日可用次数：<b>6+12次+免费3次</b>");
                }
            }
            else
            {
                if(restore)
                {
                    ShowToast("恢复购买【单件提示12】成功，每日可用次数：<b>12次+免费3次</b>");
                }
                else
                {
                    ShowToast("购买【单件提示12】成功，每日可用次数：<b>12次+免费3次</b>");
                }
            }
        }

        if (IAP.getHasBuy(IAP.IAP_CKDA)
           && IAP.getHasBuy(IAP.IAP_MDXJ)
           && IAP.getHasBuy(IAP.IAP_DZTS6)
           && IAP.getHasBuy(IAP.IAP_DZTS12))
        {
            _restoreBtn.SetActive(false);
        }
        else
        {
            _restoreBtn.SetActive(true);
        }
    }

    private void DoUnLockAni(GameObject lockObj,Action cb = null){
        Image[] ll = lockObj.GetComponentsInChildren<Image>(true);

        Image lockImg = ll[0];
        Image unlockImg = ll[1];
        foreach(var img in ll){
            if(img.name == "LockImg"){
                lockImg = img;
            }
            if(img.name == "UnLockImg"){
                unlockImg = img;
            }
        }

        Sequence mySequence = DOTween.Sequence();
        unlockImg.gameObject.SetActive(true);
        mySequence
            .Append(unlockImg.DOFade(0.0f, 0.0f))
            .Append(unlockImg.transform.DOScale(1.5f, 0.0f))
            .Append(lockImg.DOFade(0.0f, 0.5f))
            .Join(lockImg.transform.DOScale(1.5f, 0.5f))
            .SetEase(Ease.InSine)
            .Append(unlockImg.DOFade(1.0f, 0.8f))
            .Join(unlockImg.transform.DOScale(1.0f, 0.8f))
            .Append(unlockImg.transform.DOShakeRotation(1.0f,45.0f))
            .Append(unlockImg.DOFade(0.0f, 0.5f))
            .SetEase(Ease.InSine)
            .OnComplete(()=>{

            if(cb != null){
                    cb();
            }
        });
    }


    [System.Serializable] public class OnEventReport : UnityEvent<string> { }
    public OnEventReport ReportEvent;
    public void OnReportEvent(bool success,string inAppID,
                            EventReport.BuyType buyType)
    {

        EventReport.EventType eventType = EventReport.EventType.NoneType;
        if (inAppID == IAP.IAP_CKDA)
        {
            eventType = EventReport.EventType.CkdaBuyClick;
        }
        else if (inAppID == IAP.IAP_MDXJ)
        {
            eventType = EventReport.EventType.MtxjBuyClick;
        }
        else if (inAppID == IAP.IAP_DZTS6)
        {
            eventType = EventReport.EventType.Dzts6BuyClick;
        }
        else if (inAppID == IAP.IAP_DZTS12)
        {
            eventType = EventReport.EventType.Dzts12BuyClick;
        }

        ReportEvent.Invoke(buyType +"_"+ eventType+"_"+ success);
    }
}
