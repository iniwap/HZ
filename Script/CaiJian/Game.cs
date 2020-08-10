using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using Reign;
using System.Collections;


namespace HanZi
{
    public class Game : MonoBehaviour
    {
        public enum GameType
        {
            CCK,
            LLX,
            GAME_END,
        }

        public void Start()
        {
            ChangeZTTimeType(ZTTimeType.ZT_10S);
        }

        //做题限时
        public enum ZTTimeType
        {
            ZT_10S = 10,
            ZT_20S = 20,
            ZT_30S = 30,
            ZT_60S = 60,
        }

        //诗词学习模式均为双句测试，即上，下句形。不考虑其他形式。
        //当然如果有扩展的需求，可以考虑增加整篇等形式，包括作者、朝代、题目等

        public GameType _GameType;

        public GameObject[] _GameCtrl;//子游戏控制

        public void OnEnable()
        {
        }

        //只能进来一次
        public void OnInit()
        {
            //加载已经发现的汉字列表
            _GameCtrl[(int)GameType.LLX].GetComponent<TiMu>().OnInit();
        }

        public void OpenGame(GameType gameType)
        {
            _GameType = gameType;
            _isInGaming = true;

            OnShowMask.Invoke(true);
            OnInGaming.Invoke(true);

            //刷新每日使用次数
            UpdatePerDayUseCnt(eUpdatePerDayUseCntType.UPDATE_NORMAL, "");

            Invoke("GameStart",0.1f);
        }

        //左右滑动
        //只适用于选填排序
        public void OnSwipe(Define.SWIPE_TYPE type)
        {
            //只有排序模式才处理滑动操作
            if(gameObject.activeSelf)
            {
                _GameCtrl[(int)_GameType].GetComponent<TiMu>().OnSwipe(type);
            }
        }

        [Serializable] public class OnChangeZTTimeTypeEvent : UnityEvent<ZTTimeType> { }
        public OnChangeZTTimeTypeEvent OnChangeZTTimeType;

        public void ChangeZTTimeType(ZTTimeType zzt)
        {
            OnChangeZTTimeType.Invoke(zzt);
        }

        [System.Serializable] public class OnShowDialogEvent : UnityEvent<Color, MaskTips.DialogParam> { }
        public OnShowDialogEvent OnShowDialog;

        [Serializable] public class OnCloseGameEvent : UnityEvent<GameType> { }
        public OnCloseGameEvent OnCloseGame;
        public void OnClickBack()
        {

            if (_GameType == GameType.LLX)
            {
                //连句（选填）模式，铅笔不能显示此时
                _GameCtrl[(int)_GameType].GetComponent<LLX>().SetCanShowPencil(false);
            }
            else if(_GameType == GameType.CCK)
            {
                _GameCtrl[(int)_GameType].GetComponent<CCK>().SetCanShowPencil(false);
            }

            if (_GameCtrl[(int)_GameType].GetComponent<TiMu>().GetCanExist())
            {
                //先结束当前游戏
                _GameCtrl[(int)_GameType].GetComponent<TiMu>().ExistGame();
                //返回主界面
            }
            else
            {
                //显示到begin
                MaskTips.DialogParam dp;
                dp.cancelBtn = "取消";
                dp.okBtn = "确定";
                dp.content = "确定放弃当前挑战吗？";
                dp.type = MaskTips.eDialogType.OK_CANCEL_BTN;
                dp.title = "温馨提示";
                dp.cb = (MaskTips.eDialogBtnType type) =>
                {
                    if (type == MaskTips.eDialogBtnType.OK)
                    {
                    //先结束当前游戏
                        _GameCtrl[(int)_GameType].GetComponent<TiMu>().AbandonGame();

                        OnCloseGame.Invoke(_GameType);
                    }
                    else
                    {
                    //关闭弹窗
                }
                };

                OnShowDialog.Invoke(Define.BG_COLOR, dp);
            }
        }


        public GameObject _GO;
        public GameObject _TopMenu;
        [System.Serializable] public class OnShowMaskEvent : UnityEvent<bool> { }
        public OnShowMaskEvent OnShowMask;

        [System.Serializable] public class OnInGamingEvent : UnityEvent<bool> { }
        public OnInGamingEvent OnInGaming;
        private bool _isInGaming = false;
        public void GameStart()
        {
            _GO.SetActive(true);
            AutoFitUI af = _TopMenu.GetComponent<AutoFitUI>();

            _TopMenu.transform.localPosition = new Vector3(0, af.GetUIPos().y + 200, 0);

            //执行reay go动画
            //结束后开始考试
            Sequence mySequence = DOTween.Sequence();
            Image bg = _GO.GetComponentInChildren<Image>();
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0);
            Text[] rg = _GO.GetComponentsInChildren<Text>();

            //fuck here 
            float offset = Screen.width + 100;
            if (FitUI.GetIsPad())
            {
                offset += 200;
            }

            rg[0].transform.localPosition = new Vector3(-offset,
                                                        rg[0].transform.localPosition.y,
                                                        rg[0].transform.localPosition.z);
            rg[1].transform.localPosition = new Vector3(-offset,
                                                rg[1].transform.localPosition.y,
                                                rg[1].transform.localPosition.z);


            mySequence
                .Append(bg.DOFade(255 / 255.0f, 0.5f))
                .Join(rg[0].transform.DOLocalMoveX(0, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(InitTiMu);//显示ready的时候生成题目

        }

        public void OnGameEnd()
        {
            OnCloseGame.Invoke(_GameType);
            OnInGaming.Invoke(false);
            _isInGaming = false;
        }

        //此时需要禁用画笔 - 打开帮助和字典均调用这个接口
        public void OnOpenHelpPanel(bool open)
        {
            if (!gameObject.activeSelf) return;
            _GameCtrl[(int)_GameType].GetComponent<TiMu>().OpenHelpPanel(open);
        }

        [System.Serializable] public class OnMakeXJEvent : UnityEvent<int, string, HZManager.eShiCi> { }
        public OnMakeXJEvent OnMakeXinJian;
        public void OnMakeXJ(int sjID, string currentSJ, HZManager.eShiCi fw)
        {
            OnMakeXinJian.Invoke(sjID, currentSJ, fw);
        }

        private void InitTiMu()
        {
            _GameCtrl[(int)_GameType].GetComponent<TiMu>().InitTiMu(() =>
            {
                Image bg = _GO.GetComponentInChildren<Image>();
                Text[] rg = _GO.GetComponentsInChildren<Text>();
                //fuck here 
                float offset = Screen.width + 100;
                if (FitUI.GetIsPad())
                {
                    offset += 200;
                }

                Sequence mySequence = DOTween.Sequence();
                mySequence
                    .AppendInterval(0.5f)//由于初始化题目会有一定耗时，这里只停留0.5s即可
                    .Append(rg[0].transform.DOLocalMoveX(offset, 0.3f))
                    .AppendInterval(0.1f)
                    .Append(rg[1].transform.DOLocalMoveX(0, 0.4f))
                    .AppendInterval(1.0f)
                    .Append(rg[1].transform.DOLocalMoveX(offset, 0.2f))
                    .Join(bg.DOFade(0.0f, 0.2f))
                    .AppendInterval(0.1f)
                    .SetEase(Ease.InSine)
                    .OnComplete(() =>
                    {
                        _GO.SetActive(false);
                        ShowStudy();
                    });
            });
        }

        private void ShowStudy()
        {
            for (int i = 0; i < (int)GameType.GAME_END; i++)
            {
                if (i == (int)_GameType)
                {
                    _GameCtrl[i].SetActive(true);
                    _GameCtrl[i].transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
                    Sequence mySequence = DOTween.Sequence();
                    AutoFitUI af = _TopMenu.GetComponent<AutoFitUI>();

                    //显示rule
                    mySequence
                        .Append(_GameCtrl[i].transform.DOScale(1.0f, 1.0f))
                        .Join(_TopMenu.transform.DOLocalMoveY(af.GetUIPos().y, 1.0f))
                        .SetEase(Ease.OutBounce)
                        .OnComplete(() =>
                        {
                        //倒计时，应该在开始答题时启动
                        OnShowMask.Invoke(false);
                        //启动该游戏定时器
                        _GameCtrl[(int)_GameType].GetComponent<TiMu>().OnZuoTiTimer(true);
                        });
                }
                else
                {
                    _GameCtrl[i].SetActive(false);
                }
            }
        }

        //排行榜相关处理
        [System.Serializable] public class ReportScoreEvent : UnityEvent<long, GameCenter.LeaderboardType> { }
        public ReportScoreEvent ReportScore;
        public void OnReportScore(long score)
        {
            ReportScore.Invoke(score, GetLBType());
        }

        //连续闯关数 - 不分题目类型
        [System.Serializable] public class ReportAchievementEvent : UnityEvent<float, GameCenter.AchievementType> { }
        public ReportAchievementEvent ReportAchievement;
        public void OnReportAchievement(float percent, GameCenter.AchievementType type)
        {
            //
            ReportAchievement.Invoke(percent, type);
        }

        [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
        public OnShowToastEvent OnShowToast;
        public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
        {
            Toast.ToastData data;
            data.c = Define.BG_COLOR;
            data.delay = delay;
            data.im = true;
            data.showTime = showTime;
            data.content = content;

            OnShowToast.Invoke(data);
        }

        //上报分数响应，只有响应了才能上报成就，二者不能同时上报
        public void OnReportScoreCallback(bool successed)
        {
            //if (gameObject.activeSelf) //即使不在游戏界面，也需要上报
            {
                _GameCtrl[(int)_GameType].GetComponent<TiMu>().ReportScoreCallback(successed);
            }
        }

        public void OnReprotAchievementCallback(bool successed)
        {
            //当前获取的成就个数
            int prev = GetCurrentAch();

            //只有划一划控制成就
            _GameCtrl[(int)GameType.LLX].GetComponent<TiMu>().ReportAchievementCallback(successed);
            
            //上报之后获取的成就个数
            int affter = GetCurrentAch();

            //Debug.Log("之后成就数:" + affter);

            if (affter > prev) //有新的成就获取
            {
                string dzts = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, "3#0#0#3#0");/*总数、单6、单12、免费、非上报成就*/
                string[] dztsCnt = dzts.Split('#');
                int cnt = int.Parse(dztsCnt[0]) + 2 * affter;
                dztsCnt[0] = "" + cnt;

                //上报的更新不要设置最后面的成就，只是为了启动登陆使用的
                //int ach_cnt = int.Parse(dztsCnt[4]) + 1;
                //dztsCnt[4] = ""+ ach_cnt;

                //由于是依次获取成就，所以此处只对当前次数+1即可
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, String.Join("#", dztsCnt));
                //仍然需要更新界面
                if (_isInGaming)
                {
                    _GameCtrl[(int)_GameType].GetComponent<TiMu>().UpdateLeftDZTSTime(cnt);
                }

                // Debug.Log("获取成就后次数:" + cnt);
                //获得了成就
                _CurrentAT = (GameCenter.AchievementType)(affter - 1);

                //延时显示提示，为了避开与颜色解锁的提示重合，后续可以优化为队列的提示
                Invoke("DelayShowToast",3f);
            }
        }

        private GameCenter.AchievementType _CurrentAT;
        private void DelayShowToast()
        {
            switch (_CurrentAT)
            {
                case GameCenter.AchievementType.HZTongSheng:
                    ShowToast("童生每日免费单件提示次数+2，继续努力成为<b>秀才</b>");
                    break;
                case GameCenter.AchievementType.HZXiuCai:
                    ShowToast("秀才每日免费单件提示次数+4，继续努力成为<b>举人</b>");
                    break;
                case GameCenter.AchievementType.HZJuRen:
                    ShowToast("举人每日免费单件提示次数+6，继续努力成为<b>贡士</b>");
                    break;
                case GameCenter.AchievementType.HZGongShi:
                    ShowToast("贡士每日免费单件提示次数+8，继续努力成为<b>进士</b>");
                    break;
                case GameCenter.AchievementType.HZJinShi:
                    ShowToast("进士每日免费单件提示次数+10，继续努力成为<b>探花</b>");
                    break;
                case GameCenter.AchievementType.HZTanHua:
                    ShowToast("探花每日免费单件提示次数+12，继续努力成为<b>榜眼</b>");
                    break;
                case GameCenter.AchievementType.HZBangYan:
                    ShowToast("榜眼每日免费单件提示次数+14，继续努力成为<b>状元</b>");
                    break;
                case GameCenter.AchievementType.HZZhuangYuan:
                    ShowToast("状元每日免费单件提示次数+16，获最高成就，继续探索吧！");
                    break;
            }
        }

        public void OnRequestAchievementsCallback(Achievement[] achievements)
        {
            //启动登陆，这肯定会先来一次，没有上报之前
           UpdatePerDayUseCnt(eUpdatePerDayUseCntType.UPDATE_ACHIEVEMENT,"");
        }

        public UnityEvent OnResetUserAchievementsProgress;
        //初始化可用【查看答案】次数
        public enum eUpdatePerDayUseCntType{
            UPDATE_NORMAL,//普通更新，即没有成就以及购买行为，同天是不会改变
            UPDATE_ACHIEVEMENT,//成就更新
            UPDATE_BUY,//购买更新
        }
        private void UpdatePerDayUseCnt(eUpdatePerDayUseCntType type, string inAppID = "")
        {
            Define.GetNetDateTime((string dt) =>
            {
            //没有获取到，极有可能没有联网，这种情况无法使用【查看答案】次数
                if (dt == string.Empty)
                {
                    //同步时间错误，没有联网
                    MessageBoxManager.Show("", "同步时间错误，请确认可访问网络并彻底退出重新打开，否则可能无法正常使用【查看答案】等功能");

                    return;
                }

                string rec = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_PERDAY_CNT_RECORD, "");

                DateTime datetime = Convert.ToDateTime(dt);
                int day = datetime.Day;
                int month = datetime.Month;
                int year = datetime.Year;

                int ckdaCnt = 0;
                string  dztsCnt = "3#0#0#3#0";
                if (rec == "") //第一次打开
                {
                    ckdaCnt = GetPerDayShowCKDACnt();
                    dztsCnt = GetPerDayShowDDZTSCnt();
                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, ckdaCnt);
                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, dztsCnt);
                    //设置时间
                    rec = "" + year + "#" + month + "#" + day;

                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_PERDAY_CNT_RECORD, rec);

                    //Debug.Log("第一次打开："+cnt);

                    //reset user achievement
                    OnResetUserAchievementsProgress.Invoke();
                }
                else
                {
                    string[] ymd = rec.Split('#');
                    int oyear = int.Parse(ymd[0]);
                    int omonth = int.Parse(ymd[1]);
                    int oday = int.Parse(ymd[2]);

                    if (oyear == year && omonth == month && oday == day)
                    {
                        //同一天，不处理使用次数
                        ckdaCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
                        dztsCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, dztsCnt);
                        //Debug.Log("同一天打开：" + cnt);

                        if (type == eUpdatePerDayUseCntType.UPDATE_BUY)
                        {
                            if(inAppID == IAP.IAP_CKDA)
                            {
                                if (ckdaCnt == 0)//只有当次数为0时，才认为可以更新，否则应该是错误情况
                                {
                                    ckdaCnt = Define.FREEUSE_DAAN_PER_DAY;
                                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, ckdaCnt);
                                }
                            }
                            else if(inAppID == IAP.IAP_DZTS6)
                            {
                                string[] dzts = dztsCnt.Split('#');
                                if(dzts[1] == "0")//未更新过，则增加
                                {
                                    dzts[1] = ""+Define.FREEUSE_DZTS6_PER_DAY;
                                    int tcnt = int.Parse(dzts[0]) + Define.FREEUSE_DZTS6_PER_DAY;
                                    dzts[0] = ""+ tcnt;

                                    dztsCnt = String.Join("#", dzts);
                                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, dztsCnt);
                                }
                            }
                            else if (inAppID == IAP.IAP_DZTS12)
                            {
                                string[] dzts = dztsCnt.Split('#');
                                if (dzts[2] == "0")//未更新过，则增加
                                {
                                    dzts[2] = "" + Define.FREEUSE_DZTS12_PER_DAY;
                                    int tcnt = int.Parse(dzts[0]) + Define.FREEUSE_DZTS12_PER_DAY;
                                    dzts[0] = "" + tcnt;

                                    dztsCnt = String.Join("#", dzts);
                                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, dztsCnt);
                                }
                            }
                        }
                        else if(type == eUpdatePerDayUseCntType.UPDATE_ACHIEVEMENT)
                        {
                            //成就只会影响 
                            string[] dzts = dztsCnt.Split('#');
                            if (dzts[4] == "0")//未更新过，则增加
                            {
                                int cntach = 2 * GetCurrentAch();
                                dzts[4] = "" + cntach;
                                int tcnt = int.Parse(dzts[0]) + cntach;
                                dzts[0] = "" + tcnt;

                                dztsCnt = String.Join("#", dzts);
                                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, dztsCnt);
                            }
                        }
                        else
                        {
                            //普通更新不会导致次数变化
                        }
                    }
                    else
                    {
                        //不是同一天，重置使用次数
                        ckdaCnt = GetPerDayShowCKDACnt();
                        dztsCnt = GetPerDayShowDDZTSCnt();
                        rec = "" + year + "#" + month + "#" + day;
                        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, ckdaCnt);
                        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, dztsCnt);
                        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_PERDAY_CNT_RECORD, rec);
                        //Debug.Log("不同一天打开：" + cnt);
                    }
                }

                //游戏具体界面已经显示的时候
                if (_isInGaming)
                {
                    _GameCtrl[(int)_GameType].GetComponent<TiMu>().UpdateLeftDaanTime(ckdaCnt);

                    _GameCtrl[(int)_GameType].GetComponent<TiMu>().UpdateLeftDZTSTime(int.Parse(dztsCnt.Split('#')[0]));
                }
            });
        }

        private int GetPerDayShowCKDACnt()
        {
            int cnt = 0;
            if (IAP.getHasBuy(IAP.IAP_CKDA))
            {
                cnt += Define.FREEUSE_DAAN_PER_DAY;
            }

            return cnt;
        }
        private string GetPerDayShowDDZTSCnt()
        {
            int cnt6 = 0;
            int cnt12 = 0;
            int cntfree = 3;
            int cntach = 0;

            int cnt = 0;

            if(IAP.getHasBuy(IAP.IAP_DZTS6))
            {
                cnt6 = Define.FREEUSE_DZTS6_PER_DAY;
            }

            if (IAP.getHasBuy(IAP.IAP_DZTS12))
            {
                cnt12 = Define.FREEUSE_DZTS12_PER_DAY;
            }

            cntfree = Define.FREEUSE_DZTSFREE_PER_DAY;
            //额外增加对应成就的次数

            cntach = 2*GetCurrentAch();

            cnt = cnt6 + cnt12 + cntfree + cntach;

            return cnt + "#"+cnt6+"#"+cnt12+"#"+cntfree+"#"+cntach;
        }
        private int GetCurrentAch()
        {
            int currentAch = 0;
            for (int i = (int)GameCenter.AchievementType.HZZhuangYuan; i >= (int)GameCenter.AchievementType.HZTongSheng; i--)
            {
                if (Setting.getPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + "" + ((GameCenter.AchievementType)i), 0f) >= 100f)
                {
                    currentAch = i + 1;
                    break;
                }
            }

            return currentAch;
        }

        public void OnBuyCallback(bool ret, string inAppID, string receipt)
        {
            if (!ret) return;//购买失败
            //此处应该刷新使用次数
            if (inAppID == IAP.IAP_CKDA
               || inAppID == IAP.IAP_DZTS6
               || inAppID == IAP.IAP_DZTS12)
            {
                UpdatePerDayUseCnt(eUpdatePerDayUseCntType.UPDATE_BUY, inAppID);
            }

            //原则上不在学习界面，可以不处理
            if (gameObject.activeSelf)
            {
                _GameCtrl[(int)_GameType].GetComponent<TiMu>().OnBuyCallback(ret, inAppID, receipt);
            }
        }

        public void OnRestoreCallback(bool ret, string inAppID)
        {
            if (!ret) return;//购买失败

            //此处应该刷新使用次数
            if (inAppID == IAP.IAP_CKDA
               || inAppID == IAP.IAP_DZTS6
               || inAppID == IAP.IAP_DZTS12)
            {
                UpdatePerDayUseCnt(eUpdatePerDayUseCntType.UPDATE_BUY, inAppID);
            }

            //
            if (gameObject.activeSelf)
            {
                _GameCtrl[(int)_GameType].GetComponent<TiMu>().OnRestoreCallback(ret, inAppID);
            }
        }

        public void OnResetUserAchievementsCallback(bool succeeded)
        {
            //do nothing
        }
        //------------------一些接口-----------------------
        private GameType GetStudyType(string studyType)
        {
            if (studyType == "" + GameType.LLX)
            {
                return GameType.LLX;
            }
            else if (studyType == "" + GameType.CCK)
            {
                return GameType.CCK;
            }

            return GameType.CCK;
        }

        public GameCenter.LeaderboardType GetLBType()
        {
            if (_GameType == GameType.LLX)
            {
                return GameCenter.LeaderboardType.HZLLX;
            }
            else if (_GameType == GameType.CCK)
            {
                return GameCenter.LeaderboardType.HZCCK;
            }


            return GameCenter.LeaderboardType.HZLLX;
        }

        //分享游戏截图
        public void OnShareGame()
        {
            //需要首先隐藏ui
            //to do
            Sequence mySequence = DOTween.Sequence();

            mySequence
                .Append(_TopMenu.transform.DOLocalMoveY(1920 / 2 + _TopMenu.GetComponent<RectTransform>().rect.height * FitUI.GetXScale(_TopMenu) + FitUI.GetOffsetYIphoneX(true), 0.4f))
                .SetEase(Ease.InSine)
                .AppendInterval(0.1f)
                .OnComplete(() =>
                {
                    StartCoroutine(CaptureScreen());
                });
        }

        public GameObject _mask;
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

                    _mask.GetComponent<MaskTips>().ShowSaveDone(Define.BG_COLOR, topMenu, () => {
                        SocialManager.Share(data, "HanZi", "#成语迷#", "成语迷", "#成语迷#", SocialShareDataTypes.Image_PNG);
                    });
                };
            })
            );

            // cleanup
            Destroy(texture);
        }
    }
}
