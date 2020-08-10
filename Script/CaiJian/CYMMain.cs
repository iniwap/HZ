using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using Reign;
using System;
using HanZi;

public class CYMMain : MonoBehaviour
{
    public void Start()
    {

    }

    public GameObject _thirdParty;

    //应该只作数据的初始化
    public void OnInit()
    {

        _thirdParty.SetActive(true);
        this.gameObject.SetActive(true);

        InitDayHanZi();


    }

    //启动界面显示结束，此时开始执行动画
    public void OnExitSplash()
    {

        //执行icon动画 以及启动后的界面展开动画
        DoMainUIAni(() => {
            //检测启动次数
            int cnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.START_CNT, 0);
            if (cnt <= 0) //只显示一次
            {
                _startMask.SetActive(true);
                //首次启动
                ShowChangeCYTips();
                Invoke("LoginGameCenter", 2.0f);//2s 以后登陆gamecenter
            }
            else
            {
                LoginGameCenter();//立即登陆gamecenter
            }
            //启动的时候特殊设置一次
            _Find.SetActive(HZManager.GetInstance().GetSHZByHZ(_HZText.text)[(int)HZManager.eSHZCName.HZ_FIND] == "1");

            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.START_CNT, cnt + 1);
        });
    }

    public GameObject _Find;
    public GameObject _DayCY;
    public GameObject _MainMenu;
    public GameObject _TopMenu;
    public Text _FindCYCnt;
    public Image _FindCYCntIcon;
    public Image _FinishPercentProgress;
    public Text _FinishPercentText;
    private void DoMainUIAni(Action cb = null)
    {
        _DayCY.transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
        _MainMenu.transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
        AutoFitUI af = _TopMenu.GetComponent<AutoFitUI>();
        _TopMenu.transform.localPosition = new Vector3(0, af.GetUIPos().y + 200,0);
        Sequence mySequence = DOTween.Sequence();
        //显示rule
        mySequence
            .Append(_DayCY.transform.DOScale(1.0f, 1.0f))
            .Join(_MainMenu.transform.DOScale(1.0f, 1.0f))
            .Join(_TopMenu.transform.DOLocalMoveY(af.GetUIPos().y, 1.0f))
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                if (cb != null)
                {
                    cb();
                }
            });


        float speed = 1.5f;
        //刷新游戏按钮上的游戏数据信息
        string findCYCnt = ""+Setting.getPlayerPrefs("" + Setting.SETTING_KEY.FIND_CY_CNT, 0);
        if(_FindCYCnt.text != findCYCnt)
        {
            Sequence hzSeq = DOTween.Sequence();
            Quaternion r = _FindCYCntIcon.transform.localRotation;

            hzSeq
                .Append(_FindCYCntIcon.transform.DOLocalRotate(new Vector3(r.x, 180, r.z), speed))
                .Join(_FindCYCnt.DOText(findCYCnt, speed))
                .SetEase(Ease.InSine)
                .Append(_FindCYCntIcon.transform.DOLocalRotate(new Vector3(r.x, 360, r.z), speed))
                .SetEase(Ease.OutSine);
        }

        //刷新游戏按钮上的游戏数据信息
        int cckFinishCYCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.CCK_FINISH_CNT, 0);
        float rate = (float)Math.Round(100.0000 * cckFinishCYCnt / HZManager.GetInstance().GetSZMCnt(), 1);


        _FinishPercentProgress.transform.DOScaleX(rate/100, 1.0f).SetEase(Ease.OutSine);
        _FinishPercentText.DOText(""+rate + "%", 1.0f);
    }

    //切换成语动画，目前暂时只支持四字成语翻转动画，其他后续支持
    private void DoChangeHZAni(string hz,string fy,string js,string zj,int nd)
    {

        _HZText.transform.DOKill(false);
        _PYText.transform.DOKill(false);
        _MorePYText.transform.DOKill(false);

        //难度
        int star1 = nd / 2;
        int star0 = 5 - star1;
        int halfstar = nd % 2;
        for (int s = 0; s < 5; s++)
        {
            GameObject objStar0 = _DayCY.transform.Find("CY/NanDu/Star0" + (s + 1)).gameObject;
            objStar0.gameObject.SetActive(false);

            GameObject objStar1 = _DayCY.transform.Find("CY/NanDu/Star1" + (s + 1)).gameObject;
            objStar1.gameObject.SetActive(false);

            GameObject objHalfStar = _DayCY.transform.Find("CY/NanDu/HalfStar" + (s + 1)).gameObject;
            objHalfStar.gameObject.SetActive(false);


            if (s < star1)
            {
                objStar1.gameObject.SetActive(true);
            }
            if (s == star1)
            {
                if (halfstar != 0)
                {
                    objHalfStar.gameObject.SetActive(true);
                }
                else
                {
                    objStar0.gameObject.SetActive(true);
                }
            }
            if (s > star1)
            {
                objStar0.gameObject.SetActive(true);
            }
        }

        //设置是否已经探索到的
        _Find.SetActive(HZManager.GetInstance().GetSHZByHZ(_HZText.text)[(int)HZManager.eSHZCName.HZ_FIND] == "1");


        string[] pys = fy.Split('#');

        DOTween.Kill("DoChangeHZAni", false);
        
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("DoChangeHZAni");
        float speed = 0.5f;

        Sequence hzSeq = DOTween.Sequence();
        Quaternion r = _HZText.transform.localRotation;

        hzSeq
            .Append(_HZText.transform.DOLocalRotate(new Vector3(r.x, 180, r.z), speed))
            .Join(_HZText.DOText(hz, speed))
            .Join(_HZText.DOFade(0.05f, speed))
            .Join(_HZText.transform.DOScale(0.8f, speed))
            .SetEase(Ease.InSine)
            .Append(_HZText.transform.DOLocalRotate(new Vector3(r.x, 360, r.z), speed))
            .Join(_HZText.DOFade(1.0f, speed))
            .Join(_HZText.transform.DOScale(1.0f, speed))
            .SetEase(Ease.OutSine);
            

        hzSeq.OnComplete(()=>{
            _PYText.text = pys[0];
            if (pys.Length > 1)
            {
                string pystr = "";
                for (int i = 1; i < pys.Length;i++)
                {
                    if(i == pys.Length - 1)
                    {
                        pystr = pystr + pys[i];
                    }
                    else
                    {
                        pystr = pystr + pys[i] + "、";
                    }
                }

                _MorePYText.text = pystr;

                _MorePYText.gameObject.SetActive(true);
            }
            else
            {
                _MorePYText.gameObject.SetActive(false);
            }
        });

        mySequence.Join(hzSeq)
                  .Join(_PYText.DOFade(0.0f, 1.0f))
                  .Join(_MorePYText.DOFade(0.0f, 1.0f));

        mySequence.Append(_PYText.DOFade(1.0f, 0.2f));

        if (pys.Length > 1)
        {
            mySequence.Join(_MorePYText.DOFade(200/255.0f, 0.2f));
        }

        mySequence.OnComplete(()=>{
            _leanSwipe.SetActive(true);
        });
    }

    public Text _HZText;
    public Text _PYText;
    public Text _MorePYText;
    public Text _ENText;
    private void InitDayHanZi()
    {
        SetDayHZ();
        InvokeRepeating("RefreshHanZi", 1.0f, 1.0f);//开启倒计时
    }

    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
    }

    private List<GameObject> _jieShiList = new List<GameObject>();
    public GameObject _hzPrafab;
    public Transform _jieShiTransform;
    public Image _JSTagBg;
    public Image _ENTagBg;
    public Text _JSTagText;
    public Text _ENTagText;

    public  void SetDayHZ(){

        //此时禁用切换操作，直到本次操作完成
        _leanSwipe.SetActive(false);

        List<string> hzInfo = new List<string>();

        string hz = "";
        string faYin = "";
        string jieShi = "";
        string en = "";

        hzInfo = HZManager.GetInstance().GetSHZ();
        //hzInfo = HZManager.GetInstance().GetSHZByHZ("好");
        hz = hzInfo[(int)HZManager.eSHZCName.HZ_HZ];
        faYin = hzInfo[(int)HZManager.eSHZCName.HZ_PINYIN];
        jieShi = hzInfo[(int)HZManager.eSHZCName.HZ_JIESHI];
        en = hzInfo[(int)HZManager.eSHZCName.HZ_ENGLISH];


        //首先隐藏原来的
        Sequence infoSeq = DOTween.Sequence();
        foreach (var jsHZ in _jieShiList)
        {
            Text tt = jsHZ.GetComponentInChildren<Text>();
            infoSeq.Join(tt.DOFade(0.0f,0.4f));
        }
        infoSeq.Join(_ENText.DOFade(0.0f, 0.2f));

        infoSeq.Join(_JSTagBg.DOFade(30/255.0f, 0.2f));
        infoSeq.Join(_JSTagText.DOFade(200 / 255.0f, 0.2f));
        infoSeq.Join(_ENTagBg.DOFade(30/255.0f, 0.2f));
        infoSeq.Join(_ENTagText.DOFade(200/255.0f, 0.2f));

        infoSeq.AppendInterval(0.1f);
        infoSeq.SetEase(Ease.OutSine);

        infoSeq.OnComplete(()=>{
            DestroyObj(_jieShiList);

            List<string> jss = new List<string>();
            jss.AddRange(jieShi.Split('#'));

            List<int> searching = new List<int>();

            if (jss.Count != 1)
            {
                for (int i = 0; i < jss.Count; i++)
                {
                    jss[i] = (i + 1) + "" + jss[i];
                }
            }

            int MaxHZSize = 60;
            int MinHZSize = 30;
            int perLineCnt = 0;

            RectTransform jsrt = _jieShiTransform.GetComponentInChildren<RectTransform>();
            GridLayoutGroup jsLyt = _jieShiTransform.GetComponentInChildren<GridLayoutGroup>();

            //找到最大的一个满足要求的大小
            float jscellSize = MaxHZSize;
            for (int i = MaxHZSize; i >= MinHZSize;i--)
            {
                int LineCnt = 0;
                for (int j = 0; j < jss.Count; j++)
                {

                    perLineCnt = (int)(jsrt.rect.width / i);
                    LineCnt += jss[j].Length / perLineCnt;
                    LineCnt += jss[j].Length % perLineCnt == 0?0:1;
                }

                int maxLineCnt = (int)(jsrt.rect.height / i);
                if (LineCnt <= maxLineCnt)
                {
                    jscellSize = i;
                    break;
                }
            }

            jsLyt.cellSize = new Vector2(jscellSize, jscellSize);

            //插入空格
            if (jss.Count != 1)
            {
                for (int i = 0; i < jss.Count; i++)
                {

                    int len = jss[i].Length;

                    if (len % perLineCnt != 0)
                    {
                        int leftCnt = perLineCnt - len % perLineCnt;

                        for (int c = 0; c < leftCnt; c++)
                        {
                            jss[i] = jss[i] + " ";//
                        }
                    }
                }
            }

            for (int i = 0; i < jss.Count; i++)
            {
                string index = "1";
                int startIndex = 0;

                if(jss.Count != 1)
                {
                    if (i >= 9)
                    {
                        startIndex = 2;
                        jss[i] = jss[i] + " ";//需要再插入一个空格
                        index = jss[i].Substring(0, 2);
                    }
                    else
                    {
                        startIndex = 1;
                        index = "" + jss[i][0];
                    }

                    GameObject indexText = Instantiate(_hzPrafab, _jieShiTransform) as GameObject;
                    indexText.SetActive(true);
                    indexText.GetComponentInChildren<Text>().text = "<b>" + index + ".</b>";
                    _jieShiList.Add(indexText);
                }

                for (int j = startIndex; j < jss[i].Length;j++)
                {
                    GameObject txt = Instantiate(_hzPrafab, _jieShiTransform) as GameObject;
                    txt.SetActive(true);
                    txt.GetComponentInChildren<Text>().text = "" + jss[i][j];

                    _jieShiList.Add(txt);
                }
            }

            _ENText.text = en.Replace('#', ',');

            Sequence infoSeq2 = DOTween.Sequence();
            foreach (var jsHZ in _jieShiList)
            {
                Text tt = jsHZ.GetComponentInChildren<Text>();
                infoSeq2.Join(tt.DOFade(1.0f, 0.5f));
            }

            infoSeq2.Join(_ENText.DOFade(1.0f, 0.5f));
            infoSeq2.Join(_JSTagBg.DOFade(10 / 255.0f, 0.5f));
            infoSeq2.Join(_JSTagText.DOFade(100 / 255.0f, 0.5f));
            infoSeq2.Join(_ENTagBg.DOFade(10 / 255.0f, 0.5f));
            infoSeq2.Join(_ENTagText.DOFade(100 / 255.0f, 0.5f));

            infoSeq2.SetEase(Ease.InSine);
        });

        int nd = 1 + 10 * int.Parse(hzInfo[(int)HZManager.eSHZCName.HZ_ID]) / HZManager.GetInstance().GetSHZCnt();
        nd = nd > 10 ? 10 : nd;

        DoChangeHZAni(hz,faYin,jieShi,en, nd);
    }

    //---------------------定时更新成语---------------------------
    public Text _cyClock;
    private void RefreshHanZi()
    {
        int leftTime = int.Parse(_cyClock.text);
        leftTime--;
        _cyClock.text = "" + leftTime;

        if(leftTime < 0){
            _cyClock.text = "0";//保持0，除非可以切换

            RestDayCY();
        }
    }

    private void RestDayCY(){
        //当前不可切换，需要等待动画完成
        if (!_leanSwipe.activeSelf)
        {
            return;
        }

        //重置一条成语
        _cyClock.text = "" + 30;

        SetDayHZ();
    }

    public GameObject _startMask;//for fix mask bug
    public void CheckStartCnt(){

    }
    public void ShowChangeCYTips()
    {
        _mask.GetComponent<MaskTips>().ShowChangeCYTips(Define.BG_COLOR);
        _startMask.SetActive(false);
    }

    public void ShowDragTips()
    {
        _mask.GetComponent<MaskTips>().ShowDragTips(Define.BG_COLOR);
        _startMask.SetActive(false);
    }

    public void CheckGestureTips(){
        //显示一次手势操作
        int cnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HAS_SHOW_GESTURE, 0);
        if (cnt <= 1)
        {
            _mask.GetComponent<MaskTips>().ShowGestureTips(Define.BG_COLOR);
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.HAS_SHOW_GESTURE, cnt + 1);
        }
    }

    //------------------------手势操作-------------------------------------
    public GameObject _leanSelect;
    public GameObject _leanTouch;
    public GameObject _leanSwipe;

    public void CheckIsScrolling(){
        _leanSwipe.SetActive(true);
    }
    //菜单滑动时候，阻止切换操作，否则有误
    public void OnScrollViewValueChanged(Vector2 pos){
        CancelInvoke("CheckIsScrolling");
        _leanSwipe.SetActive(false);
        Invoke("CheckIsScrolling",0.2f);
    }
  
    public bool CheckCanSwipe()
    {
        // 设置界面显示时，不能切换
        if (_setting.gameObject.activeSelf 
            || _likePanel.gameObject.activeSelf
            ||_GamePanel.activeSelf
            ||_HelpPanel.activeSelf
            ||_ShopPanel.activeSelf
            ||_DictPanel.activeSelf
            || _WritePaperPanel.activeSelf)
        {
            return false;
        }

        return true;
    }

    public void ShowSwipeTip(bool leftRight){
        if (leftRight)
        {
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGSJ_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT)
            {
                // 不再提示，有点多余且乱
                if(HZManager.GetInstance().GenerateRandomInt(0,2) == 0)
                {
                   // ShowToast("除了切换学习成语外，还可以<b>猜一猜、划一划</b>玩成语：)", Define.BG_COLOR, 3f, 0.5f);
                }
                else
                {
                   // ShowToast("每个成语停留30s，等不及可以<b>左右滑动</b>切换哟～", Define.BG_COLOR, 2f, 0.5f);
                }

                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGSJ_TIP, tipCnt + 1);
            }
        }
        else
        {
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGECOLOR_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT)
            {
                //ShowToast("如果<b>看不清诗词</b>，请点击<b>装饰->诗文</b>修改字体颜色", Define.BG_COLOR,3f,0.5f);
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGECOLOR_TIP, tipCnt + 1);
            }
        }
    }

    public void OnSwipeUp()
    {
        if (CheckCanSwipe() && !_isInGaming)
        {
            //ResetUsingLike();
        }
        else{
            if(_isInGaming && !_HelpPanel.activeSelf){
                OnSwipe.Invoke(Define.SWIPE_TYPE.UP);
            }
        }
    }
    public void OnSwipeDown()
    {
        if (CheckCanSwipe() && !_isInGaming)
        {
            //ResetUsingLike();
        }
        else
        {
            if (_isInGaming && !_HelpPanel.activeSelf)
            {
                OnSwipe.Invoke(Define.SWIPE_TYPE.DOWN);
            }
        }
    }

    [Serializable] public class OnSwipeEvent : UnityEvent<Define.SWIPE_TYPE> { }
    public OnSwipeEvent OnSwipe;//切换考题
    public void OnSwipeLeft()
    {
        //正在学习模式
        if (_GamePanel.activeSelf && !_HelpPanel.activeSelf)
        {
            OnSwipe.Invoke(Define.SWIPE_TYPE.LEFT);
        }
        else
        {
            if (CheckCanSwipe())
            {
                RestDayCY();//换一条成语
                ShowSwipeTip(true);
            }
        }
    }

    public void OnSwipeRight()
    {
        //正在学习模式
        if (_GamePanel.activeSelf && !_HelpPanel.activeSelf)
        {
            OnSwipe.Invoke(Define.SWIPE_TYPE.RIGHT);
        }
        else
        {
            if (CheckCanSwipe())
            {
                RestDayCY();//换一条成语
                ShowSwipeTip(true);
            }
        }
    }

    public GameObject _GameCenter;
    public void LoginGameCenter()
    {
        if (!_GameCenter.activeSelf)
        {
            _GameCenter.SetActive(true);
        }
    }

    public void OnCCKBtnClick()
    {
        OpenGame(Game.GameType.CCK);
    }
    public void OnLLXBtnClick()
    {
        OpenGame(Game.GameType.LLX);
    }
    //顶部菜单动画
    public GameObject _mask;
    public GameObject _GamePanel;
    //打开教育练习界面
    [Serializable] public class OnOpenGameEvent : UnityEvent<Game.GameType> { }
    public OnOpenGameEvent OnOpenGame;
    private void OpenGame(Game.GameType gameType)
    {
        _GamePanel.SetActive(true);
        gameObject.SetActive(false);

        OnOpenGame.Invoke(gameType);
        LoginGameCenter();
    }

    public void OnCloseGame(Game.GameType gameType)
    {
        _GamePanel.SetActive(false);
        gameObject.SetActive(true);
        DoMainUIAni();
    }

    public void OnSaveCaiJian()
    {
        //需要首先隐藏ui
        //to do
        Sequence mySequence = DOTween.Sequence();

        mySequence
            //.Append(_TopMenu.transform.DOLocalMoveY(1920 / 2 + _TopMenu.GetComponent<RectTransform>().rect.height * FitUI.GetXScale(_TopMenu) + FitUI.GetOffsetYIphoneX(true), 0.4f))
            .SetEase(Ease.InSine)
            .AppendInterval(0.1f)
            .OnComplete(() =>
            {
                StartCoroutine(CaptureScreen());
            });
    }

    private IEnumerator CaptureScreen()
    {
        yield return new WaitForEndOfFrame();
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        // do something with texture

        var data = texture.EncodeToJPG();
        string fn = "CJ.jpg";
        StreamManager.SaveFile(fn, data, FolderLocations.Pictures, ((bool succeeded) =>
        {
            if (succeeded)
            {
                Tween topMenu = _TopMenu.transform.DOLocalMoveY(1920 / 2 - FitUI.GetOffsetYIphoneX(true),
                                                                0.4f);

                _mask.GetComponent<MaskTips>().ShowSaveDone(Define.BG_COLOR, topMenu,()=>{
                    SocialManager.Share(data, "HanZi", "#成语迷#", "成语迷", "#成语迷#", SocialShareDataTypes.Image_PNG);
                });
            };
        })
        );

        // cleanup
        Destroy(texture);
    }


#region 设置界面相关
    public GameObject _setting;
    public void OnOpenSetting(bool open)
    {
       // OpenPanel(_setting,open);
    }

    public GameObject _HelpPanel;

    public GameObject _radicalPrafab;
    private List<GameObject> _tmpRstTextList = new List<GameObject>();
    public Transform _HelpRadicalContent;

    public void OnOpenHelpPanel(bool open)
    {
        RefreshHelpRadical();

        OpenPanel(_HelpPanel, open);
    }

    public void RefreshHelpRadical(bool random = false)
    {
        DestroyObj(_tmpRstTextList);


        List<List<string>> rdata = new List<List<string>>();

        int RCnt = HZManager.GetInstance().GetRadicalCnt();

        if(random)
        {
            List<int> indexs = HZManager.GetInstance().GenerateRandomNoRptIntList(44,0, RCnt);
            foreach(var index in indexs)
            {
                rdata.Add(HZManager.GetInstance().GetRadical(index));
            }

            rdata.Sort((a, b) =>
            {
                return b[3].Length - a[3].Length;
            });
        }
        else
        {
            for (int i = 0; i < 44; i++)
            {
                rdata.Add(HZManager.GetInstance().GetRadical(i));
            }
        }
        

        float hzSize = _radicalPrafab.GetComponentInChildren<Text>().transform.GetComponent<RectTransform>().rect.width;

        RectTransform jsrt = _HelpRadicalContent.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = _HelpRadicalContent.GetComponentInChildren<GridLayoutGroup>();
        float spaceArea = (jsLyt.padding.top + jsLyt.padding.bottom) * jsrt.rect.width;
        float jscellSize = (float)Math.Sqrt(((jsrt.rect.width - hzSize) * (jsrt.rect.height - hzSize) - spaceArea) / rdata.Count);
        jscellSize = jscellSize > hzSize ? hzSize : jscellSize;
        jsLyt.cellSize = new Vector2(jscellSize, jscellSize);

        for (int i = 0; i < rdata.Count; i++)
        {
            GameObject txt = Instantiate(_radicalPrafab, _HelpRadicalContent) as GameObject;
            txt.SetActive(true);

            Text t = txt.GetComponentInChildren<Text>();
            t.text = rdata[i][1];
            _tmpRstTextList.Add(txt);

            if(rdata[i][3].Length >= 100)
            {
                t.color = Define.RED;

            }
            else if (rdata[i][3].Length >= 50 && rdata[i][3].Length < 100)
            {
                t.color = Define.YELLOW;
            }
            else if (rdata[i][3].Length >= 10 && rdata[i][3].Length < 50)
            {
                t.color = Define.BLUE;
            }
            else if (rdata[i][3].Length >= 5 && rdata[i][3].Length < 10)
            {
                t.color = Define.GREEN;
            }
            else if (rdata[i][3].Length >= 2 && rdata[i][3].Length < 5)
            {
                t.color = Define.PURPLE;
            }
            else
            {
                //原色
                t.color = Define.FONT_COLOR_LIGHT;
            }
        }
    }

    public GameObject _ShopPanel;
    public void OnOpenShop(bool open)
    {
        OpenPanel(_ShopPanel, open);
    }

    public GameObject _DictPanel;
    public void OnOpenDict(bool open)
    {
        OpenPanel(_DictPanel, open);
    }

    public GameObject _WritePaperPanel;
    public void OnOpenWritePaper(bool open)
    {
        OpenPanel(_WritePaperPanel, open);
    }

    private void OpenPanel(GameObject panel,bool open){
        Sequence mySequence = DOTween.Sequence();

        _mask.SetActive(true);
        panel.SetActive(true);
        _leanSelect.SetActive(false);

        float toX = 0;
        if (open)
        {
            panel.transform.localPosition = new Vector3(FitUI.GetIsPad() ? 1080 * 1.5f : 1080.0f,
                                              panel.transform.localPosition.y,
                                              panel.transform.localPosition.z);
            toX = 0.0f;
            mySequence
                .Append(panel.transform.DOLocalMoveX(toX, 1.2f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    panel.gameObject.SetActive(open);
                    _mask.SetActive(false);
                    _leanSelect.SetActive(true);
                });

        }
        else
        {
            panel.transform.localPosition = new Vector3(0,
                                              panel.transform.localPosition.y,
                                              panel.transform.localPosition.z);
            toX = 1080;//Screen.width;
            if (FitUI.GetIsPad())
            {
                toX = 1080 * 1.5f;//Screen.width;
            }

            mySequence
                .Append(panel.transform.DOLocalMoveX(toX, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    panel.gameObject.SetActive(open);
                    _mask.SetActive(false);
                    _leanSelect.SetActive(true);
                });
        }
    }
#endregion

#region 收藏界面相关
    //打开收藏界面
    public Like _likePanel;
    public void OnOpenLike(bool open)
    {
        Sequence mySequence = DOTween.Sequence();

        float toX = 0;
        GameObject likes = _likePanel.transform.Find("Likes").gameObject;
        Image bg = _likePanel.transform.Find("Bg").gameObject.GetComponent<Image>();
        if (open)
        {
            string title = "";
            string content = "";

            OnInitLike.Invoke(0, title, content);

            //打开动画
            _leanTouch.SetActive(!open);
            _likePanel.gameObject.SetActive(open);

            likes.transform.localPosition = new Vector3(1080 * 1.5f,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = ((FitUI.DESIGN_HEIGHT / Screen.height) * Screen.width) / 2;
            mySequence
                .Append(bg.DOFade(0.0f, 0.0f))
                .AppendInterval(0.1f)//避免第一次显示时的短暂白屏
                .Append(likes.transform.DOLocalMoveX(toX, 1.2f))
                .SetEase(Ease.OutBounce)
                .Join(bg.DOFade(100 / 255.0f, 1.2f))
                .OnComplete(() =>
                {
                    //
                });

        }
        else
        {
            //关闭动画
            likes.transform.localPosition = new Vector3(((FitUI.DESIGN_HEIGHT / Screen.height) * Screen.width) / 2,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = 1080 * 1.5f;//Screen.width;

            mySequence
                .Append(likes.transform.DOLocalMoveX(toX, 0.5f))
                .SetEase(Ease.InSine)
                .Join(bg.DOFade(0.0f, 0.5f))
                .OnComplete(() =>
                {
                    _leanTouch.SetActive(!open);
                    _likePanel.gameObject.SetActive(open);
                });
        }
    }

    [Serializable] public class OnInitLikeEvent : UnityEvent<int, string, string> { }
    public OnInitLikeEvent OnInitLike;

    public void OnLikeItemClick(string id)
    {
        LikeItem.sLikeItem li = _likePanel.GetLikeItem(id);

        OnOpenLike(false);

        ShowToast("将主界面显示的成语换成点击的收藏成语", new Color(127, 127, 127,255), 3.0f);
        
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        ShowToast(content, Define.BG_COLOR, showTime, delay);
    }

    public void ShowToast(string content,Color c, float showTime = 2.0f, float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = c;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }

    #endregion

    private bool _isInGaming = false;
    public void OnInGaming(bool gameing){
        _isInGaming = gameing;
    }

 
#region 预览图制作相关
    ////////---------------------------制作屏幕截图------------------------------
    /// 
    /// 
    //获取屏幕截图用于应用市场
    public void GetScreenShot(bool showTop)
    {

        if (!showTop)
        {
            Sequence mySequence = DOTween.Sequence();

            mySequence
                .Append(_TopMenu.transform.DOLocalMoveY(1920 / 2 + _TopMenu.GetComponent<RectTransform>().rect.height * FitUI.GetXScale(_TopMenu) + FitUI.GetOffsetYIphoneX(true), 0.4f))
                .SetEase(Ease.InSine)
                .AppendInterval(0.1f)
                .OnComplete(() =>
                {
                    StartCoroutine(CaptureScreen2(showTop));
                });
        }
        else
        {
            StartCoroutine(CaptureScreen2(showTop));
        }
    }

    private IEnumerator CaptureScreen2(bool showTop)
    {
        yield return new WaitForEndOfFrame();
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        // do something with texture

        var data = texture.EncodeToJPG();

        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

        string fn = Convert.ToInt64(ts.TotalSeconds).ToString() + ".jpg";
        StreamManager.SaveFile(fn, data, FolderLocations.Pictures, ((bool succeeded) =>
        {
            if (succeeded)
            {
                if (!showTop)
                {
                    Tween topMenu = _TopMenu.transform.DOLocalMoveY(1920 / 2 - FitUI.GetOffsetYIphoneX(true),
                                                                    0.4f);
                }
            };
        })
        );

        // cleanup
        Destroy(texture);
    }
#endregion
}
