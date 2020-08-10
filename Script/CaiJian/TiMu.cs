using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Reign;
using HanZi;

public class TiMu : MonoBehaviour
{
    public virtual void Start()
    {

    }

    public virtual void OnEnable()
    {
        _CanExist = false;
        CheckBuyState();
    }

    public virtual void OnInit()
    {

    }

    public virtual void InitTiMu(Action cb)
    {

    }
    public virtual void OnSwipe(Define.SWIPE_TYPE type){

    }

    protected int _TotalScore = 0;
    protected int _ChuangGuan = 0;
    //假设10s的基础分是10分，那么20s的基础分只有5分
    protected virtual int GetCurrentScore()
    {
        int ztt = (int)ZTTime - _ScoreTime;

        //耗时后得分
        int cs = Define.BASE_SCORE_10S * (int)Game.ZTTimeType.ZT_10S / (int)ZTTime;
        cs = cs * ztt / (int)ZTTime;

        //连续闯关加分
        cs += _ChuangGuan;

        return cs;
    }

    protected Game.ZTTimeType ZTTime;
    public virtual void OnChangeZTTimeType(Game.ZTTimeType zzt)
    {
        ZTTime = zzt;
    }
    protected int _ScoreTime = 0;//记录当前答题耗时 //对于排序题，该参数含义不同，需要重写处理
    protected int _CurrentPassTime = 0;
    public virtual void OnZuoTiTimer(bool start)
    {
        Transform t = gameObject.transform.Find("TiMu/Time/BellDark");

        if (!start)
        {
            _CurrentPassTime = 0;
            CancelInvoke("DoZuoTiTimeClock");

            if (t != null)
            {
                t.localRotation = new Quaternion(0.0f,0.0f,0.0f,0.0f);
                t.GetComponentInChildren<Text>().transform.localScale = new Vector3(1.0f,1.0f,1.0f);
            }

            return;
        }
        _CurrentPassTime = 0;

        if (t != null)
        {
            t.GetComponentInChildren<Text>().text = "" + (int)ZTTime;
        }

        RestTiMu();
        //剩余5s摇铃
        //
        //大于5s，仅执行倒计时
        InvokeRepeating("DoZuoTiTimeClock", 1.0f, 1.0f);
    }

    protected void RestTiMu(){
        Transform tm = gameObject.transform.Find("TiMu");//TiText
        if (tm != null)
        {
            GameObject tmobj = tm.Find("TiText").gameObject;
            Text[] timu = tmobj.GetComponentsInChildren<Text>();
            foreach (var tt in timu)
            {
               // tt.color = _BgLightColor;// 重置透明度
            }
        }
    }

    protected virtual void DoZuoTiTimeClock()
    {
        _CurrentPassTime += 1;
        _ScoreTime += 1;

        if(_ScoreTime >= (int)ZTTime){
            _ScoreTime = (int)ZTTime;
        }

        Transform t = gameObject.transform.Find("TiMu/Time/BellDark");
        if (t != null)
        {
            int ztt = (int)ZTTime - _CurrentPassTime;
            int half = (int)Game.ZTTimeType.ZT_10S / 2;
            if (ztt > half)
            {
                t.GetComponentInChildren<Text>().text = "" + ztt;
            }
            else if (ztt <= half && ztt > 0)
            {
                t.GetComponentInChildren<Text>().text = "" + ztt;
                t.DOShakeRotation(0.5f, (half - ztt) / half * 100 + 20);
            }
            else
            {
                //超时了
                t.GetComponentInChildren<Text>().text = "0";

                if (ztt == 0)
                {
                    Transform tm = gameObject.transform.Find("TiMu");//TiText
                    if (tm != null)
                    {
                        GameObject tmobj = tm.Find("TiText").gameObject;
                        Text[] timu = tmobj.GetComponentsInChildren<Text>();

                        GameObject tmclockobj = tm.Find("FailClockTiText").gameObject;
                        tmclockobj.SetActive(true);
                        tmclockobj.GetComponent<Text>().text = "请在<size=64><b>" + GetThinkTime() + "s</b></size>内作答，否则视为闯关失败！";

                        Text tmc = tmclockobj.GetComponent<Text>();
                        tmc.color = new Color(tmc.color.r, tmc.color.g, tmc.color.b, 0.0f);

                        Sequence mySequence = DOTween.Sequence();
                        mySequence.SetId("DoZuoTiTimeClockAni"+GetStudyName());
                        foreach (var tt in timu)
                        {
                            mySequence.Join(tt.DOFade(0.0f, 1.0f));
                        }

                        mySequence
                            .Append(tmc.DOFade(1.0f, 0.5f))
                            .OnComplete(() =>
                            {
                                //请在10s内给出答案，否则视为闯关失败
                                DoFailClockAni(true);
                            });
                    }
                }
                else
                {
                    OnZuoTiTimer(false);//停止
                }
            }

            if (ztt < (int)Game.ZTTimeType.ZT_10S && ztt > 0)
            {
                float s = (half * 2 - ztt) / (half * 2) + 0.5f;
                t.GetComponentInChildren<Text>().transform.DOShakeScale(0.5f, new Vector3(s, s, 1.0f));
            }
        }
    }

    protected int _CurrentFailClockTime = 0;
    protected void DoFailClockAni(bool start,bool needStopAni = false)
    {
        _CurrentFailClockTime = 0;

        if (!start)
        {
            CancelInvoke("DoFailClock");


            if (needStopAni)
            {
                //
                DOTween.Kill("DoZuoTiTimeClockAni" + GetStudyName());
            }

            return;
        }

        DoShowDaanBtnAni();
        InvokeRepeating("DoFailClock", 1.0f, 1.0f);
    }
    public UnityEvent OnHideDialog;
    protected void DoFailClock()
    {
        _CurrentFailClockTime += 1;
        int fc = GetThinkTime() - _CurrentFailClockTime;

        Transform tm = gameObject.transform.Find("TiMu");//TiText
        if (tm != null)
        {
            GameObject tmclockobj = tm.Find("FailClockTiText").gameObject;
            if (fc > 0)
            {
                tmclockobj.GetComponent<Text>().text = "请在<size=64><b>" + fc + "s</b></size>内作答，否则视为闯关失败！";
            }
            else
            {
                tmclockobj.GetComponent<Text>().text = "您已经超时，闯关失败！";
                DoFailClockAni(false);

                //关闭弹窗，如果存在的话

                OnHideDialog.Invoke();

                //闯关失败
                TimeOutFail();
            }
        }
    }

    protected int GetThinkTime(){
        if(GetStudyName() == "TianKong"){
            return (int)Game.ZTTimeType.ZT_20S;
        }

        return (int)Game.ZTTimeType.ZT_10S / 2;
    }
    //超时失败
    protected virtual void TimeOutFail(){

    }

    public virtual void ReportScoreCallback(bool successed)
    {

    }

    public virtual void ReportAchievementCallback(bool successed)
    {

    }


    protected virtual void SaveScore(){
        int hs = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_SCORE + GetStudyName(), 0);
        int lx = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_CG + GetStudyName(), 0);

        int hlx = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.ALL_HIGHEST_CG, 0);

        _ChuangGuan += 1;
        int cs = GetCurrentScore();
        _TotalScore += cs;

        if (lx < _ChuangGuan)
        {
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_CG + GetStudyName(), _ChuangGuan);
        }

        if (hs < _TotalScore)
        {
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_SCORE + GetStudyName(), _TotalScore);

            ReportScore(_TotalScore);
        }

        if(hlx < _ChuangGuan){
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.ALL_HIGHEST_CG, _ChuangGuan);
        }

        //不能一起上报，成就需要等到 一定时机上报
        //保存更新最高闯关数
        //ReportAchievement();
    }

    //放弃当前游戏
    public virtual void AbandonGame(Action cb = null){

    }

    //结算面板倒计时的时候可以返回
    public virtual void ExistGame()
    {

    }

    protected virtual void StopAllClock()
    {

    }

    public virtual void OpenHelpPanel(bool open)
    {

    }

    protected bool _CanExist = false;
    public bool GetCanExist(){
        return _CanExist;
    }

    public UnityEvent OnGameEnd;
    protected void GameEnd(){
        OnGameEnd.Invoke();
    }

    [System.Serializable] public class OnShowMaskEvent : UnityEvent<bool> { }
    public OnShowMaskEvent OnShowMask;
    public void ShowMask(bool show){
        OnShowMask.Invoke(show);
    }

    [System.Serializable] public class OnMakeXJEvent : UnityEvent<int, string> { }
    public OnMakeXJEvent OnMakeXJ;
    public void OnMakeXinJian(int sjID,string currentSJ)
    {
        OnMakeXJ.Invoke(sjID,currentSJ);
    }

    //最高得分
    [System.Serializable] public class OnReportScoreEvent : UnityEvent<long> { }
    public OnReportScoreEvent OnReportScore;
    public void ReportScore(long score)
    {
        OnReportScore.Invoke(score);
    }

    //连续闯关数 - 不分题目类型
    [System.Serializable] public class OnReportAchievementEvent : UnityEvent<float, GameCenter.AchievementType> { }
    public OnReportAchievementEvent OnReportAchievement;
    public void ReportAchievement(int score)
    {
        GameCenter.AchievementType type = GameCenter.AchievementType.HZTongSheng;
        float percent = 0;

        bool allComplete = true;
        for (int i = (int)GameCenter.AchievementType.HZTongSheng; i <= (int)GameCenter.AchievementType.HZZhuangYuan; i++){
            if(Setting.getPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + ""+((GameCenter.AchievementType)i), 0f) < 100f){
                type = ((GameCenter.AchievementType)i);
                allComplete = false;
                break;
            }
        }

        //所有成就已经获得，不再上报
        if (allComplete) 
            return;

        percent = 100.0f * score / Define.ACHIEVEMENT_SCORE[(int)type];
        
        if(percent > 100f){
            percent = 100f;
        }

        OnReportAchievement.Invoke(percent, type);
    }


    //两种模式都有查看答案和单次提示，在timu父类实现即可
    //其他几种需要对应的玩法里实现
    public Button _buyDaanBtn;
    public Text _leftDaanTime;//剩余查看答案次数
    public Button  _ckdaBtn;

    public Button _buyDZTSBtn;
    public Text _leftDZTSTime;//单次提示btn
    public Button _dztsBtn;

    //只有未购买的时候才提示可以查看答案，引导购买
    protected virtual void DoShowDaanBtnAni(){
        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
        if(currentLeftShowDaanCnt <= 0){
            //如果剩余次数为0，不必再提示 查看答案动画
            return;
        }

        if (!IAP.getHasBuy(IAP.IAP_CKDA)){
            //如果没有购买
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.BUY_SHOWDAAN_ANI, 0);
            if(tipCnt >= 3){
                return;
            }

            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.BUY_SHOWDAAN_ANI, tipCnt + 1);

            Image[] ll = _buyDaanBtn.GetComponentsInChildren<Image>(true);
            Image bg = ll[1];
            Image lockImg = ll[2];

            Sequence sq = DOTween.Sequence();
            sq
                .Append(lockImg.DOFade(0.1f, 0.5f))
                .Join(lockImg.transform.DOScale(2.0f, 0.5f))
                .Join(bg.DOFade(0.1f, 0.5f))
                .Append(_ckdaBtn.transform.DOShakePosition(1.0f, 5))
                .Append(lockImg.DOFade(1.0f, 0.5f))
                .Join(lockImg.transform.DOScale(1.0f, 0.5f))
                .Join(bg.DOFade(100 / 255.0f, 0.5f));
        }
        else{
            //已经购买的动画
            _ckdaBtn.transform.DOShakePosition(1.0f, 5);
        }
    }

    protected virtual void DoShowDZTSBtnAni()
    {
        string dzts = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, "3#0#0#3#0");
        string[] dztsCnt = dzts.Split('#');
        int currentLeftShowDZTSCnt = int.Parse(dztsCnt[0]);

        if (currentLeftShowDZTSCnt <= 0)
        {
            //如果剩余次数为0，不必再提示 查看答案动画
            return;
        }

        if (!IAP.getHasBuy(IAP.IAP_DZTS6) && !IAP.getHasBuy(IAP.IAP_DZTS12))
        {
            //如果没有购买
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.BUY_SHOWDZTS_ANI, 0);
            if (tipCnt >= 3)
            {
                return;
            }

            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.BUY_SHOWDAAN_ANI, tipCnt + 1);

            Image[] ll = _buyDaanBtn.GetComponentsInChildren<Image>(true);
            Image bg = ll[1];
            Image lockImg = ll[2];

            Sequence sq = DOTween.Sequence();
            sq
                .Append(lockImg.DOFade(0.1f, 0.5f))
                .Join(lockImg.transform.DOScale(2.0f, 0.5f))
                .Join(bg.DOFade(0.1f, 0.5f))
                .Append(_dztsBtn.transform.DOShakePosition(1.0f, 5))
                .Append(lockImg.DOFade(1.0f, 0.5f))
                .Join(lockImg.transform.DOScale(1.0f, 0.5f))
                .Join(bg.DOFade(100 / 255.0f, 0.5f));
        }
        else
        {
            //已经购买的动画
            _dztsBtn.transform.DOShakePosition(1.0f, 5);
        }
    }

    public void UpdateLeftDaanTime(int n)
    {
        _leftDaanTime.text = "+"+n;
    }

    public void UpdateLeftDZTSTime(int n)
    {
        _leftDZTSTime.text = "+" + n;
    }

    protected void CheckBuyState(){
        if(IAP.getHasBuy(IAP.IAP_CKDA)){
            _buyDaanBtn.gameObject.SetActive(false);
            _leftDaanTime.gameObject.SetActive(true);
        }
        else{
            _buyDaanBtn.gameObject.SetActive(true);
            _leftDaanTime.gameObject.SetActive(false);
        }

        if (IAP.getHasBuy(IAP.IAP_DZTS6) || IAP.getHasBuy(IAP.IAP_DZTS12))
        {
            _buyDZTSBtn.gameObject.SetActive(false);
            _leftDZTSTime.gameObject.SetActive(true);
        }
        else
        {
            if(_leftDZTSTime.text != "+0")
            {
                _buyDZTSBtn.gameObject.SetActive(false);
                _leftDZTSTime.gameObject.SetActive(true);
            }
            else
            {
                _buyDZTSBtn.gameObject.SetActive(true);
                _leftDZTSTime.gameObject.SetActive(false);
            }
        }

        CheckNotCommonBuyState();
    }


    protected  virtual void CheckNotCommonBuyState()
    {
        //猜一猜 需要实现该接口
    }


    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        if (ret)
        {
            if (inAppID == IAP.IAP_CKDA)
            {
                //如果lock不可见，不必执行动画
                if (_buyDaanBtn.gameObject.activeSelf)
                {
                    DoUnLockAni(_buyDaanBtn.gameObject, () =>
                    {
                        _buyDaanBtn.gameObject.SetActive(false);
                        CheckBuyState();
                    });
                }
            }
            else if (inAppID == IAP.IAP_DZTS6)//  默认购买6次的
            {
                if (_buyDZTSBtn.gameObject.activeSelf)
                {
                    DoUnLockAni(_buyDZTSBtn.gameObject, () =>
                    {
                        _buyDZTSBtn.gameObject.SetActive(false);
                        CheckBuyState();
                    });
                }
            }
            else
            {
                OnBuyNotCommonCallback(inAppID);
            }
        }
    }

    protected virtual void OnBuyNotCommonCallback(string inAppID)
    {
        //猜一猜 需要实现该接口
    }

    public void OnRestoreCallback(bool ret, string inAppID)
    {
        if (ret)
        {
            OnBuyCallback(true, inAppID,"");//直接复用
        }
    }

    protected void DoUnLockAni(GameObject lockObj, Action cb = null)
    {
        Image[] ll = lockObj.GetComponentsInChildren<Image>(true);

        Image bg = ll[1];
        Image lockImg = ll[2];
        Image unlockImg = ll[3];
        foreach (var img in ll)
        {
            if (img.name == "BuyLockTagLight")
            {
                lockImg = img;
            }
            if (img.name == "BuyUnLockTagLight")
            {
                unlockImg = img;
            }
            if (img.name == "Bg")
            {
                bg = img;
            }
        }

        Sequence mySequence = DOTween.Sequence();
        unlockImg.gameObject.SetActive(true);
        mySequence
            .Append(unlockImg.DOFade(0.0f, 0.0f))
            .Append(unlockImg.transform.DOScale(1.5f, 0.0f))
            .Append(lockImg.DOFade(0.0f, 0.3f))
            .Join(lockImg.transform.DOScale(1.5f, 0.3f))
            .SetEase(Ease.InSine)
            .Append(unlockImg.DOFade(1.0f, 0.5f))
            .Join(unlockImg.transform.DOScale(1.0f, 0.5f))
            .Append(unlockImg.transform.DOShakeRotation(1.0f, 45.0f))
            .Append(unlockImg.DOFade(0.0f, 0.3f))
            .Join(bg.DOFade(0.0f, 0.6f))
            .SetEase(Ease.InSine)
            .OnComplete(() => {

                if (cb != null)
                {
                    cb();
                }
            });
    }

    [System.Serializable] public class OnEventReport : UnityEvent<string> { }
    public OnEventReport ReportEvent;
    public void OnReportEvent(bool success,
                            EventReport.BuyType buyType)
    {
        ReportEvent.Invoke(buyType + "_" + EventReport.EventType.CkdaBuyClick + "_" + success);
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f,float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = Define.BG_COLOR;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }

    [System.Serializable] public class OnShowDialogEvent : UnityEvent<Color, MaskTips.DialogParam> { }
    public OnShowDialogEvent OnShowDialog;
    protected virtual void ShowCYInfo(string cy,string content, Action<MaskTips.eDialogBtnType> callBack)
    {
        //显示到begin
        MaskTips.DialogParam dp;
        dp.cancelBtn = "取消";
        dp.okBtn = "确定";
        dp.content = content;
        dp.title = cy;
        dp.type = MaskTips.eDialogType.NO_BTN;
        dp.cb = callBack;

        OnShowDialog.Invoke(Define.BG_COLOR, dp);
    }
    //-----------------------一些接口------------------------
    protected virtual string GetStudyName(){
       // Debug.Log("ERROR GetStudyName:" + gameObject.name);//error if call here
        return gameObject.name;
    }

    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
    }

    public void ShuffleSJList(List<GameObject> myList)
    {
        int index = 0;
        GameObject temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }

    public void ShuffleSJList(List<string> myList)
    {
        int index = 0;
        string temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }

    public void ShuffleSJList(List<int> myList)
    {
        int index = 0;
        int temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }

    public void ShuffleSJList(List<object> myList)
    {
        int index = 0;
        object temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }
}
