/*
 * 划一划
*/

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Events;
using Reign;

using HanZi;

public class LLX : TiMu
{
    public override void Start()
    {

    }

    public override void OnEnable()
    {
        _CanExist = true;//任何时刻均可退出
        CheckBuyState();
    }

    public override void OnChangeZTTimeType(Game.ZTTimeType zzt)
    {

    }

    public override void InitTiMu(Action cb)
    {
        //只有开始进入的时候需要初始化分数面板，切换题目的时候会自动刷新
        InitScore();
        InitTiMu(true, cb);
    }

    public XTPencil _Pencil;
    public void SetCanShowPencil(bool can)
    {
        _Pencil.SetCanShowPencil(can);
    }
    private List<GameObject> _HZList = new List<GameObject>();
    public Transform _HZContent;
    public GameObject _HZPrefab;


    private List<List<string>> _CurrentData = new List<List<string>>();
    private int _CurrentAllZJCnt = 0;//所有的组件总数

    private List<int> _CurrentSelectIndex = new List<int>();// 主要用于记录当前划选的字

    private int XT_HZ_MATRIX = Define.MAX_XT_HZ_MATRIX;

    private void InitTiMu(bool first, Action cb = null)
    {
        //首先选择诗句
        XT_HZ_MATRIX = 5;
        XT_HZ_MATRIX = XT_HZ_MATRIX > Define.MAX_XT_HZ_MATRIX ? Define.MAX_XT_HZ_MATRIX : XT_HZ_MATRIX;//超过最大没有意义，难度过高
        
        ///////////////////诗词数据初始化//////////////////////
        InitCY();
        // 初始化界面
        InitUI();

        ////////////////将诗句汉字按照一定的搜索顺序插入到方阵中///////////////////////
        //支持8个方向，需要记录这个列表序列 用于答案提示，可能会遇到汉字相同，但是路径不同的情况
        int currentIndex = HZManager.GetInstance().GenerateRandomInt(0, XT_HZ_MATRIX * XT_HZ_MATRIX);
        int col = currentIndex % XT_HZ_MATRIX;
        int row = currentIndex / XT_HZ_MATRIX;
        ///////////////////将剩下空着的方阵插入其他随机汉字///////////////////////////

        SearchSJPath(col, row, XT_HZ_MATRIX, (List<int> path) =>
        {
            //生成汉字序列
            string[] hzList = new string[XT_HZ_MATRIX * XT_HZ_MATRIX];
            for (int i = 0; i < hzList.Length; i++)
            {
                hzList[i] = "";
            }

             //设置搜索出的路径诗句
            string allZJ = "";
            for (int d = 0; d < _CurrentData.Count;d++)
            {
                int plen = allZJ.Length;
                allZJ = allZJ + _CurrentData[d][(int)HZManager.eSHZCName.HZ_JGDEC];

                string index = "";
                for (int l = plen; l < allZJ.Length;l++)
                {
                    if(l != allZJ.Length - 1)
                    {
                         index = index + path[l] + "#";
                    }
                    else
                    {
                         index = index + path[l];
                    }
                }
                _CurrentData[d].Add(index);//把对应索引保存到数据结构中，为了后续超找使用

            }
            for (int j = 0; j < path.Count; j++)
            {
                hzList[path[j]] = "" + allZJ[j];
            }

            //插入干扰字
            InsertDisturbHZ(hzList);

            ////////////////////////////////////////////////////////////////////
            //生成字序列 -- 汉字方阵 根据诗句长度动态变化大小
            float wh = GetGridSizeWH();
            int fontSize = _HZPrefab.GetComponentInChildren<Text>().fontSize;
            float s = wh / _HZPrefab.GetComponent<RectTransform>().rect.width;
            GridLayoutGroup gl = _HZContent.GetComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(wh, wh);
            for (int i = 0; i < hzList.Length; i++)
            {
                //顶部汉字
                GameObject Hz = Instantiate(_HZPrefab, _HZContent) as GameObject;
                Hz.SetActive(true);

                XTHZ xtHZ = Hz.GetComponent<XTHZ>();
                bool hasFind = false;
                if(hzList[i] != "" && !HZManager.GetInstance().CheckIsSRadical(hzList[i]))
                {
                    try
                    {
                        hasFind = HZManager.GetInstance().GetSHZByHZ(hzList[i])[(int)HZManager.eSHZCName.HZ_FIND] == "1";
                    }
                    catch
                    {
                        Debug.Log("查询出错："+ hzList[i]);
                    }
                }
                xtHZ.InitHZ(i, hzList[i], hasFind);
                _HZList.Add(Hz);

                RectTransform hzrt = Hz.GetComponent<RectTransform>();
                hzrt.sizeDelta = new Vector2(wh, wh);

                BoxCollider bc = Hz.GetComponent<BoxCollider>();
                bc.size = new Vector3(wh / 2, wh / 2, 1);

                Hz.GetComponentInChildren<Text>().fontSize = (int)(s * fontSize);

                //该处无字
                if (hzList[i] == "")
                {
                    bc.enabled = false;//禁用碰撞检测
                }
            }

            /////  由于是多线程，必须在这里重置/////
            if (!first)
            {
                OnZuoTiTimer(true);
            }

            if (cb != null)
            {
                cb();
            }
        });

        HZManager.GetInstance().FixBugGetSHZByJGDEC();//首先随便调用一次，解决后续查询效率过低问题
    }

    private void InitUI()
    {
        DestroyObj(_HZList);
        _TipBrush.gameObject.SetActive(false);
        _GoNextBtn.gameObject.SetActive(false);
        DisableBottomBtn(false);
        //只要新生成题目，分数计时一定是0
        _ScoreTime = 0;
        _NeedReportAchievement = false;
        _CurrentDZTSUsedCnt = 0;

        _SearchDifficult = 0.0f;
        _DisturbCnt = 0;//干扰字个数
        _DisturbDifficult = eDisturbDifficultType.EASY;

        CancelInvoke("ShowNoteInfo");
        InvokeRepeating("ShowNoteInfo", 10.0f, 10.0f);
        ShowGameRule();


        Text[] scs = _ScorePanel.GetComponentsInChildren<Text>();
        Image[] sps = _ScorePanel.GetComponentsInChildren<Image>();
        foreach (var sc in scs)
        {
            sc.color = new Color(sc.color.r,sc.color.g,sc.color.b,200/255.0f);
        }
        foreach (var sp in sps)
        {
            sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, 50/255.0f);
        }

        //隐藏全部rst
        _RstInfo.gameObject.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            _ResultCYInfo[i].SetActive(false);
        }
        DestroyObj(_tmpRstTextList);

        //--------------初始化划选汉字结果列表----------------------------
        List<Text> rstCY = new List<Text>();
        for (int i = 0; i < 4; i++)
        {
            rstCY.AddRange(_ResultCY[i].GetComponentsInChildren<Text>(true));
        }
        for (int i = 0; i < rstCY.Count; i += 5)
        {
            rstCY[i].text = "划";
            if (i / 4 == 0)
            {
                rstCY[i+1].text = "一";
            }
            else if (i / 4 == 1)
            {
                rstCY[i+1].text = "二";
            }
            else if (i / 4 == 2)
            {
                rstCY[i+1].text = "三";
            }
            else if (i / 4 == 3)
            {
                rstCY[i+1].text = "四";
            }
            rstCY[i+2].text = "汉";
            rstCY[i + 3].text = "字";
            rstCY[i].gameObject.SetActive(true);
            rstCY[i + 1].gameObject.SetActive(true);
            rstCY[i + 2].gameObject.SetActive(true);
            rstCY[i + 3].gameObject.SetActive(true);

            rstCY[i+4].text = "字";
            rstCY[i+4].gameObject.SetActive(false);
        }


        //划选汉字还是使用黄色，都是白色文字过多，不好识别

        for (int i = 0; i < rstCY.Count; i ++)
        {
            rstCY[i].DOFade(200/255.0f,0.5f);
        }

        for (int i = 0; i < 4;i++)
        {
            _ResultCY[i].gameObject.SetActive(false);
            _ResultCY[i].interactable = false;
        }
        for (int i = 0; i < _CurrentData.Count; i++)
        {
            _ResultCY[i].gameObject.SetActive(true);
        }


        //如果没有格子总数发生变化，是不需要每次调用的，事实上这里是固定的
        InitMatrixSP();
        //设置当前做题时间
        InitDiffcult();
    }

    public List<Text> GetEmptyResultCY(out int index)
    {
        List<Text> ret = new List<Text>();

        index = 0;

        for (int i = 0; i < 4; i++)
        {
            if (_ResultCY[i].gameObject.activeSelf)
            {
                Text[] cy = _ResultCY[i].GetComponentsInChildren<Text>(true);
                if (!cy[4].gameObject.activeSelf)
                {
                    index = i;
                    ret.AddRange(cy);
                    break;
                }
            }
        }

        return ret;
    }

    //获取所有还未划选的汉字
    public List<Button> GetAllEmptyResultCY()
    {
        List<Button> ret = new List<Button>();

        for (int i = 0; i < 4; i++)
        {
            if (_ResultCY[i].gameObject.activeSelf)
            {
                Text[] cy = _ResultCY[i].GetComponentsInChildren<Text>(true);
                if (!cy[4].gameObject.activeSelf)
                {
                    ret.Add(_ResultCY[i]);
                }
            }
        }

        return ret;
    }
    //获取所有划选汉字TEXT
    public List<Text> GetAllResultHZText()
    {
        List<Text> ret = new List<Text>();

        for (int i = 0; i < 4; i++)
        {
            if (_ResultCY[i].gameObject.activeSelf)
            {
                Text[] cy = _ResultCY[i].GetComponentsInChildren<Text>(true);
                if (cy[4].gameObject.activeSelf)//必须可见
                    ret.Add(cy[4]);
            }
        }

        return ret;
    }

    //获取所有划选到汉字
    public List<string> GetAllResultHZ()
    {
        List<string> ret = new List<string>();

        for (int i = 0; i < 4; i++)
        {
            if (_ResultCY[i].gameObject.activeSelf)
            {
                Text[] cy = _ResultCY[i].GetComponentsInChildren<Text>(true);

                if(cy[4].gameObject.activeSelf)//必须可见
                    ret.Add(cy[4].text);
            }
        }

        return ret;
    }

    //初始化分割线，根据方阵大小变化
    public GameObject _MatrixHSP;
    public GameObject _MatrixVSP;
    public GameObject _Matrix;
    public GameObject _MHSP;//分割线，假如固定为4
    public GameObject _MVSP;//分割线，假如固定为4
    private void InitMatrixSP()
    {
        Image[] hsp = _MatrixHSP.GetComponentsInChildren<Image>(true);
        Image[] vsp = _MatrixVSP.GetComponentsInChildren<Image>(true);

        HorizontalLayoutGroup hl = _MatrixVSP.GetComponent<HorizontalLayoutGroup>();
        VerticalLayoutGroup vl = _MatrixHSP.GetComponent<VerticalLayoutGroup>();

        for (int i = 0; i < hsp.Length; i++)
        {
            hsp[i].gameObject.SetActive(false);
            vsp[i].gameObject.SetActive(false);
        }

        for (int i = 0; i <= XT_HZ_MATRIX; i++)
        {
            hsp[i].gameObject.SetActive(true);
            vsp[i].gameObject.SetActive(true);
        }

        float wh = GetGridSizeWH();
        hl.spacing = wh;
        vl.spacing = wh;
    }

    private float GetGridSizeWH()
    {
        RectTransform mhsp = _MHSP.GetComponent<RectTransform>();
        RectTransform mvsp = _MVSP.GetComponent<RectTransform>();
        RectTransform mrt = _Matrix.GetComponent<RectTransform>();
        float wh = (mrt.rect.width - mhsp.rect.height * (XT_HZ_MATRIX + 1)) / XT_HZ_MATRIX;

        return wh;
    }

    private void InitCY()
    {
        _CurrentData.Clear();
        _CurrentSelectIndex.Clear();
        _CurrentAllZJCnt = 0;
        int cyNum = 1;
        //1,2,3,4
        //100个以内汉字 只需要找1个
        //1000以内只需要找2个
        //10000以内需要找3个汉字
        //多于10000个需要找4个汉字
        int maxRange = 0;
        if (_ChuangGuan < 100)
        {
            cyNum = 1;
            maxRange = 500;
        }
        else if (_ChuangGuan >= 100 && _ChuangGuan < 1000)
        {
            cyNum = 2;
            maxRange = 2000;
        }
        else if (_ChuangGuan >= 1000 && _ChuangGuan < 5000)
        {
            cyNum = 3;
            maxRange = 8000;
        }
        else if (_ChuangGuan >= 5000)
        {
            cyNum = 4;
            maxRange = HZManager.GetInstance().GetSHZCnt();//全范围随机
        }
        


        for (int i = 1; i <= cyNum; i++)
        {
            _CurrentData.Add(HZManager.GetInstance().GetSHZ(HZManager.GetInstance().GenerateRandomInt(0, maxRange)));
            _CurrentAllZJCnt += _CurrentData[i - 1][(int)HZManager.eSHZCName.HZ_JGDEC].Length;

            if(_CurrentAllZJCnt > (XT_HZ_MATRIX - 1) * (XT_HZ_MATRIX - 1) /*不要超过16个字，更好的控制难度，以及防止搜索过慢*/)
            {
                // 
                _CurrentAllZJCnt -= _CurrentData[i - 1][(int)HZManager.eSHZCName.HZ_JGDEC].Length;
                _CurrentData.RemoveAt(i - 1);
                break;//超过最大长度就提示，即使当前需要的是四个字，也只能采用三个
            }

        }


       // foreach (var cy in _CurrentData)
       // {
         //   Debug.Log(cy[(int)HZManager.eSHZCName.HZ_JGDEC]);
        //}

    }
    public List<string> GetCY(HZManager.eLoadResType cyType = HZManager.eLoadResType.CAICHENGYU)
    {
        List<string> cy = new List<string>();

        if (cyType == HZManager.eLoadResType.CAICHENGYU)
        {
            List<string> ccy = HZManager.GetInstance().GetCaiCY();

            cy.Add(ccy[(int)HZManager.eCCYCName.CY_ID]);
            cy.Add(ccy[(int)HZManager.eCCYCName.CY_MIDI]);
            cy.Add(ccy[(int)HZManager.eCCYCName.CY_FAYIN]);
            cy.Add(ccy[(int)HZManager.eCCYCName.CY_JIESHI]);
            cy.Add(ccy[(int)HZManager.eCCYCName.CY_ZAOJU]);
            cy.Add(ccy[(int)HZManager.eCCYCName.CY_ZAOJU]);
        }
        else if (cyType == HZManager.eLoadResType.CHENGYU)
        {
            cy = HZManager.GetInstance().GetChengYu();
        }

        return cy;
    }

    public Button[] _ResultCY;//结果汉字列表

    public void OnClickResultCYBtn(int index)
    {

        Text  hz = _ResultCY[index].GetComponentsInChildren<Text>(true)[4];
        if (!hz.gameObject.activeSelf) return;


        List<string> CYInfo = new List<string>(); 

        //首选查是否在预设里，
        foreach(var d in _CurrentData)
        {
            if(hz.text.Equals(d[(int)HZManager.eSHZCName.HZ_HZ])){
                CYInfo.AddRange(d);
                break;
            }
        }

        //不存再查数据库
        if (CYInfo.Count == 0)
        {
            CYInfo.AddRange(HZManager.GetInstance().GetSHZByHZ(hz.text));
            if(CYInfo.Count == 0)
            {
                ShowToast("抱歉出错了，未查到相关汉字信息～");
            }
            else
            {
                ShowCYInfoDialog(CYInfo);
            }
        }
        else
        {
            ShowCYInfoDialog(CYInfo);
        }

    }
    private void ShowCYInfoDialog(List<string> CYInfo)
    {
        string js = CYInfo[(int)HZManager.eSHZCName.HZ_JIESHI];
        string jg = CYInfo[(int)HZManager.eSHZCName.HZ_JGDEC];
        string en = CYInfo[(int)HZManager.eSHZCName.HZ_ENGLISH];
        string py = CYInfo[(int)HZManager.eSHZCName.HZ_PINYIN].Split('#')[0];
        string content = js;
        content = content.Replace('#',';');
        SetCanShowPencil(false);
        ShowCYInfo(CYInfo[(int)HZManager.eSHZCName.HZ_HZ]+"·" + py, content, (MaskTips.eDialogBtnType type) => {
            SetCanShowPencil(true);
        });
    }

    //划选结束
    public void OnPencilSubmit()
    {
        OnPencilSubmit(false);
    }
    private void OnPencilSubmit(bool timeout)
    {
        //有选择汉字，则判断是否为正确的诗句
        if (_CurrentSelectIndex.Count != 0)
        {
            SetCanShowPencil(false);
            DisableBottomBtn(true);

            //判断是否连通，如果不连通，视为取消选择
            if (CheckIfConnect())
            {
                //检测划选序列是否构成一个或者多个汉字
                List<string> right = CheckIfRight();

                List<string> hzs = new List<string>();
                if(right.Count != 0)
                {
                    hzs.Add(right[(int)HZManager.eSHZCName.HZ_HZ]);
                }

                UpdateScore(hzs);

                if (right.Count == 0)
                {
                    ClearSelectCYIndex();
                    ShowToast("所划选不能<b>顺序正确</b>的组成汉字，请再想想看～");
                    //此处认为失败，需要增加总的划选次数
                }
                else
                {
                    //隐藏所有汉字
                    Sequence resultAni = DOTween.Sequence();
                    //防止选中动画没有结束
                    resultAni.AppendInterval(0.4f).AppendInterval(0.0f);
                    //选出来的正确汉字大于等于预设的，结束当前关卡
                    //仅执行划选出的汉字动画，并隐藏选中的
                    int ResultCYID = 0;
                    List<Text> rstCY =  GetEmptyResultCY(out ResultCYID);

                    if(rstCY.Count == 0)
                    {
                        //出错了
                        ShowToast("发生了未知错误，将开始下一题");
                        resultAni.OnComplete(DoResultCYAni);
                        return;
                    }

                    // 将方阵中所有与该字相同的组件设置为已找到过
                    if(_ChuangGuan < 100)
                    {
                        if (_CurrentSelectIndex.Count == 1)
                        {
                            // 单字，如果已经被找到了，再次划选给予提示
                            XTHZ xTHZ = _HZList[_CurrentSelectIndex[0]].GetComponentInChildren<XTHZ>();
                            if (xTHZ.GetHasFind())
                            {
                                int r = HZManager.GetInstance().GenerateRandomInt(0, 5);
                                if(r == 0)
                                {
                                    ShowToast("已探索的字不会增加探索数，请尽量合成更复杂的字。");
                                }
                                else if(r == 1)
                                {
                                    ShowToast("已经探索过的右上角标有记号，重复划选不会增加探索数。");
                                }
                                else if (r == 2)
                                {
                                    ShowToast("探索认识更多字，应避免简单划选单个已探索字。");
                                }
                                else if (r == 3)
                                {
                                    ShowToast("尽量合成更复杂的字，提升探索数，获取更高成就。");
                                }
                            }
                        }
                    }

                    for (int i = 0; i < _HZList.Count; i++)
                    {
                        XTHZ xTHZ = _HZList[i].GetComponentInChildren<XTHZ>();
                        if(xTHZ.GetHZ() != "" && !xTHZ.GetCantRestFinded() && xTHZ.GetHZ() == hzs[0])
                        {
                            xTHZ.UpdateFind();
                        }
                    }
                    

                    for (int i = 0; i < 4; i++)
                    {
                        Sequence resultAni1 = DOTween.Sequence();
                        resultAni1.AppendInterval(0.15f * (i + 1))
                                  .Join(rstCY[i].DOFade(0.0f, 0.4f))
                                  .SetEase(Ease.OutSine);
                        resultAni.Join(resultAni1);

                    }

                    for (int i = 0; i < _CurrentSelectIndex.Count; i++)
                    {

                        XTHZ xTHZ = _HZList[_CurrentSelectIndex[i]].GetComponentInChildren<XTHZ>();
                        xTHZ.GetComponent<BoxCollider>().enabled = false;//禁用碰撞检测
                        xTHZ.SetCantRestFinded(true);//不能被重置

                        Sequence resultAni2 = DOTween.Sequence();
                        resultAni2.AppendInterval(0.2f * (i + 1))
                                  .Join(xTHZ.DoHZColor(Define.YELLOW, 0.4f))
                                  .Join(xTHZ.DoHZIDFade(0.0f, 0.4f))
                                  .Join(xTHZ.DoHZIDBGFade(0.0f, 0.4f))
                                  .Append(xTHZ.GetHZText().transform.DOMove(rstCY[4].transform.position, 0.8f))
                                  .Join(xTHZ.GetHZText().transform.DOScale(0.2f, 0.8f))
                                  .Join(xTHZ.DoHZFade(0.0f, 0.8f))
                                  .Join(xTHZ.DoHZFindFade(0.0f, 0.8f))
                                  .Join(xTHZ.DoHZFindBgFade(0.0f, 0.8f))
                                  .SetEase(Ease.OutSine);

                        resultAni.Join(resultAni2);
                    }

                    //需要清除CurrentIndex中已经找到的，注意此时的CurrentIndex已经不连通了
                    resultAni.OnComplete(() =>
                    {
                        Sequence endAni = DOTween.Sequence();
                        _ResultCY[ResultCYID].interactable = true;//将对应汉字按钮设置成可以点击
                        rstCY[4].text = right[(int)HZManager.eSHZCName.HZ_HZ];
                        rstCY[4].color = new Color(rstCY[4].color.r, rstCY[4].color.g, rstCY[4].color.b,0.0f);
                        rstCY[4].gameObject.SetActive(true);
                        for (int i = 0; i < 4; i++)
                        {
                            rstCY[i].gameObject.SetActive(false);
                        }

                        endAni.Append(rstCY[4].DOFade(200 / 255.0f, 0.5f));

                        endAni.OnComplete(()=>{
                            for (int i = 0; i < _CurrentSelectIndex.Count; i++)
                            {
                                XTHZ xTHZ = _HZList[_CurrentSelectIndex[i]].GetComponentInChildren<XTHZ>();
                                xTHZ.GetHZText().text = "";
                            }

                            if (GetAllEmptyResultCY().Count == 0)
                            {
                                //结束当前题目
                                DoResultCYAni();
                            }
                            else
                            {
                                SetCanShowPencil(true);
                                _CurrentSelectIndex.Clear();

                                DisableBottomBtn(false);
                            }
                        });
                    });
                }
            }
            else
            {
                ClearSelectCYIndex();
                // 给出提示，划选不连通，请重新划选
                ShowToast("所划选构件<b>不连通</b>，存在跳划，请重新划选。");
            }
        }
    }
    private void ClearSelectCYIndex()
    {
        for (int i = _CurrentSelectIndex.Count - 1; i >= 0; i--)
        {
            XTHZ xtHZ2 = _HZList[_CurrentSelectIndex[i]].GetComponent<XTHZ>();
            xtHZ2.SetIsSelect(false);
            _CurrentSelectIndex.RemoveAt(i);
        }

        _CurrentSelectIndex.Clear();

        //1s后才能操作
        CancelInvoke("DelayEnablePencil");
        Invoke("DelayEnablePencil", 1.0f);
    }
    public  void DelayEnablePencil()
    {
        SetCanShowPencil(true);
        DisableBottomBtn(false);
    }
    //铅笔选中汉字 - 保存选中列表，有次序
    public void OnPencilSelect(bool isSelect, int index)
    {
        if (index >= 0 && index < _HZList.Count)
        {
            XTHZ xtHZ = _HZList[index].GetComponent<XTHZ>();
            //如果是选
            if (isSelect)
            {
                _CurrentSelectIndex.Add(index);
                xtHZ.SetIsSelect(true);
                xtHZ.UpdateSelectID(_CurrentSelectIndex.Count);//更新划选次序id
            }
            else
            {
                //如果是取消选中，则取消该点后面的所有汉字
                if (_CurrentSelectIndex.Count >= 2)
                {

                    int startIndex = -1;
                    for (int i = 0; i < _CurrentSelectIndex.Count; i++)
                    {
                        if (_CurrentSelectIndex[i] == index)
                        {
                            startIndex = i;
                        }
                    }

                    for (int i = _CurrentSelectIndex.Count - 1; i > startIndex; i--)
                    {
                        XTHZ xtHZ2 = _HZList[_CurrentSelectIndex[i]].GetComponent<XTHZ>();
                        xtHZ2.SetIsSelect(false);
                        _CurrentSelectIndex.RemoveAt(i);
                    }
                    //
                    if (_CurrentSelectIndex.Count == 1)
                    {
                        //仅剩下一个的时候，全部取消选中
                        xtHZ.SetIsSelect(false);
                        _CurrentSelectIndex.Clear();
                    }
                }
                else
                {
                    //仅剩下一个
                    if (_CurrentSelectIndex.Count == 1)
                    {
                        xtHZ.SetIsSelect(false);
                        _CurrentSelectIndex.Clear();
                    }
                    else
                    {
                        //不可能出现
                    }
                }
            }
        }
    }

    private enum eGameTips{
        TOAST_TIPS,
        MAIN_INFO,
        NOTE_TIPS,
    }
    public Text _MainInfo;//主要提示，规则等关键信息提示
    public Text _NoteInfo;//次要提示，主要一些说明性的
    private void DoTipInfoAni(Text infoText,string info)
    {
        infoText.DOFade(0.0f, 0.5f)
        .OnComplete(() =>
        {
            infoText.text = info;

            infoText.DOFade(200/255f, 0.5f);
        });
    }

    private string[] GameRule = {
            "规则：划选组件构成汉字，顺序需一致，不能跳划，松开确认。",
            "规则：划选终极目标是找到尽可能多的汉字，认识更多的字。",
            "规则：探索数不包含重复汉字，尽量避免划选单字。",
            "规则：每题汉字数有1、2、3、4个，共4档次。",
            "规则：已探索单字右上角会被标记，但仍可单独选",
            "规则：划对汉字量越多，划一划排行榜排名越靠前。",
            "规则：探索数决定成就等级，难度随探索数增加。"
        };

    private string[] GameNote = {
            "贴士：8方向包括上下左右及左上、左下、右下、右上。",
            "贴士：划选不符合规则的尝试并不会计入划选次数。",
            "贴士：任何问题/建议/反馈，请在帮助页联系我们。",
            "贴士：随着探索汉字数增加，游戏难度会持续增加。",
            "贴士：点击上方划选出的汉字，可以查看详细信息。",
            "贴士：查看答案功能每日可用6次，请到商城购买。",
            "贴士：单件提示分6/12两档，同购为18次。另每日免费3次。",
            "贴士：查看答案功能直接给出答案，并计算得分。",
            "贴士：达到一定探索数时，会插入干扰字。",
            "贴士：不同探索数分别获得对应成就，最高为状元",
            "贴士：达到一定探索数时，每题最高需划出4个汉字。",
            "贴士：每题结束后，需要手动点击或右划下一题。",
            "贴士：划选非出题汉字，可能会出现死局情况，可返回重来。",
            "贴士：更多功能正在开发中，欢迎推荐给朋友。",
            "贴士：回划可以取消选中，数字角标代表顺序。",
            "贴士：尽量划构件数多且复杂的字以更有效的提升探索数",
            "贴士：划选需要上下左右以及斜方向连通，跳划无效。",
            "贴士：每天可用免费单件提示次数=3+2*成就等级。",
            "贴士：点击非字区域视为取消划选，包括使用了单件提示时。",
            "贴士：探索难度经过精心设计，会随着探索数而趋向于变难。",
            "贴士：难度算法会改变划选数、连通难度、干扰字、干扰相关度等。",
            "贴士：重复点单构件解题虽能增加划对数，但不会改变探索数。",
        };
    //小提示，轮番播放，没有自动触发操作
    private void ShowNoteInfo()
    {
        int index = HZManager.GetInstance().GenerateRandomInt(0, GameNote.Length);

        ShowGameTips(eGameTips.NOTE_TIPS, GameNote[index]);
    }
    //仅在过关时切换
    private void ShowGameRule()
    {
        //除此之外，需要在特定对关卡，需要显示特定信息，主要包括
        //成就、划选数增加、干扰字插入等
        int index = HZManager.GetInstance().GenerateRandomInt(0, GameRule.Length);

        ShowGameTips(eGameTips.MAIN_INFO, GameRule[index]);
    }

    private void ShowGameTips(eGameTips type,string info)
    {

        if(type == eGameTips.MAIN_INFO){
            DoTipInfoAni(_MainInfo, info);
        }
        else if(type == eGameTips.NOTE_TIPS){
            DoTipInfoAni(_NoteInfo, info);
        }else if(type == eGameTips.TOAST_TIPS){
            ShowToast(info);
        }
    }

    public override void OnZuoTiTimer(bool start)
    {
        if(!start)
        {
            return;
        }

        Sequence resultAni = DOTween.Sequence();
        for (int i = 0; i < _HZList.Count; i++)
        {
            XTHZ xTHZ = _HZList[i].GetComponentInChildren<XTHZ>();

            resultAni.Join(xTHZ.DoHZFade(1.0f, 0.5f))
                     .Join(xTHZ.DoHZFindFade(127/255f, 0.5f))
                     .Join(xTHZ.DoHZFindBgFade(1.0f, 0.5f));
        }

        _Pencil.GetComponent<Image>().color = new Color(Define.FONT_COLOR_LIGHT.r, 
                                                        Define.FONT_COLOR_LIGHT.g, 
                                                        Define.FONT_COLOR_LIGHT.b, 0.0f);


        resultAni.OnComplete(() => {
            _Pencil.gameObject.SetActive(true);
            SetCanShowPencil(true);
        });
    }

    protected override void DoZuoTiTimeClock()
    {

    }

    public GameObject _ScorePanel;
    public Image _GoNextBtn;
    public Image _DisableBottom;
    private void DisableBottomBtn(bool dis)
    {
        _DisableBottom.DOKill(true);
        if (dis)
        {
            //显示
            _DisableBottom.gameObject.SetActive(true);
            _DisableBottom.color = new Color(_DisableBottom.color.r,
                                             _DisableBottom.color.g, 
                                             _DisableBottom.color.b,
                                             0.0f);
            _DisableBottom.DOFade(100/255.0f, 0.5f);
        }
        else
        {
            //隐藏
            _DisableBottom.DOFade(0.0f, 0.5f).OnComplete(()=> _DisableBottom.gameObject.SetActive(false));
        }
    }
    private List<string> GetHZInfo(string hz)
    {
        List<string> ret = new List<string>();

        bool ext = false;
        for (int j = 0; j < _CurrentData.Count; j++)
        {
            if (hz.Equals(_CurrentData[j][(int)HZManager.eSHZCName.HZ_HZ]))
            {
                ret.AddRange(_CurrentData[j]);
                ext = true;
                break;
            }
        }

        if (!ext)
        {
            List<string> checkRet = HZManager.GetInstance().GetSHZByHZ(hz);
            if (checkRet.Count != 0)
            {
                ret.AddRange(checkRet);
            }
        }

        return ret;
    }

    public Text []_ResultAniText;
    public void DoResultCYAni()
    {
        //DestroyObj(_HZList);
        //禁用下方的按钮
        //正确，
        //next
        _GoNextBtn.gameObject.SetActive(true);
        _GoNextBtn.color = new Color(_GoNextBtn.color.r, _GoNextBtn.color.g, _GoNextBtn.color.b,0.0f);
        Button nextBtn = _GoNextBtn.GetComponentInChildren<Button>();
        nextBtn.GetComponent<ScaleAni>().enabled = false;
        nextBtn.interactable = false;
        nextBtn.transform.localScale = new Vector3(0f, 0f, 1f);
        _RstInfo.gameObject.SetActive(true);

        List<string> findHZ = GetAllResultHZ();
        List<string> findFY = new List<string>();
        List<string> findJs = new List<string>();
        List<string> findEn = new List<string>();
        List<string> findJG = new List<string>();
        List<int> findND = new List<int>();
        int CNT = HZManager.GetInstance().GetSHZCnt();
        //GetHZInfo
        for (int i = 0; i < findHZ.Count;i++)
        {
            List<string> thzInfo = GetHZInfo(findHZ[i]);
            findFY.Add(thzInfo[(int)HZManager.eSHZCName.HZ_PINYIN]);
            findJs.Add(thzInfo[(int)HZManager.eSHZCName.HZ_JIESHI]);
            findEn.Add(thzInfo[(int)HZManager.eSHZCName.HZ_ENGLISH]);
            findJG.Add(thzInfo[(int)HZManager.eSHZCName.HZ_JGDEC]);


            int nd = 1 + 10 * int.Parse(thzInfo[(int)HZManager.eSHZCName.HZ_ID]) / CNT;
            nd = nd > 10 ? 10 : nd;
            findND.Add(nd);
        }

        //所有划选结果汉字
        List<Text> findCYText = GetAllResultHZText();
        List<Text> hzs = new List<Text>();
        List<Text> pys = new List<Text>();
        List<Text> jgs = new List<Text>();
        List<List<Text>> jss = new List<List<Text>>();//所有解释
        List<Text> ens = new List<Text>();//英文

        //其他还有米字格，难度星，tags，分割线
        //搜索所有节点
        for (int i = 0; i < findHZ.Count;i++)
        {
            _ResultCYInfo[i].SetActive(true);

            //汉字
            Text hzText = _ResultCYInfo[i].transform.Find("HZ/HZ/HZ").GetComponent<Text>();
            hzText.text = findHZ[i];
            Color []c = {Define.RED,Define.GREEN,Define.BLUE,Define.PURPLE};
            hzText.color = new Color(c[i].r, c[i].g, c[i].b,0.0f);
            hzs.Add(hzText);

            //拼音
            string [] tpys = findFY[i].Split('#');
            Text pyText = _ResultCYInfo[i].transform.Find("HZ/PY").GetComponent<Text>();
            pyText.text = tpys[0];
            pyText.color = new Color(pyText.color.r, pyText.color.g, pyText.color.b, 0.0f);
            pys.Add(pyText);
            Text morePy = _ResultCYInfo[i].transform.Find("HZ/MorePY").GetComponent<Text>();
            morePy.text = "";
            morePy.color = new Color(morePy.color.r, morePy.color.g, morePy.color.b, 0.0f);
            if (tpys.Length == 1)
            {
                morePy.gameObject.SetActive(false);
            }
            else
            {
                morePy.gameObject.SetActive(true);
                for (int j = 1; j < tpys.Length;j++)
                {
                    if(j != tpys.Length - 1)
                    {
                        morePy.text += tpys[j] + "、";
                    }
                    else
                    {
                        morePy.text += tpys[j];
                    }
                }
            }
            pys.Add(morePy);

            //难度
            int star1 = findND[i] / 2;
            int star0 = 5 - star1;
            int halfstar = findND[i] % 2;
            for (int s = 0; s < 5; s++)
            {
                GameObject objStar0 = _ResultCYInfo[i].transform.Find("HZ/ND/NanDu/Star0" + (s+1)).gameObject;
                objStar0.gameObject.SetActive(false);

                GameObject objStar1 = _ResultCYInfo[i].transform.Find("HZ/ND/NanDu/Star1" + (s + 1)).gameObject;
                objStar1.gameObject.SetActive(false);

                GameObject objHalfStar = _ResultCYInfo[i].transform.Find("HZ/ND/NanDu/HalfStar" + (s + 1)).gameObject;
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

            //结构
            Text jgText = _ResultCYInfo[i].transform.Find("HZ/JG").GetComponent<Text>();
            jgText.text = findJG[i];
            jgText.color = new Color(jgText.color.r, jgText.color.g, jgText.color.b, 0.0f);
            jgs.Add(jgText);

            //填充解释并获取
            List<Text> hzjs = new List<Text>();
            Transform jieshi = _ResultCYInfo[i].transform.Find("JieShi");
            FitDialogContentText(jieshi, findJs[i].Replace('#',';'));
            hzjs.AddRange(jieshi.GetComponentsInChildren<Text>());
            jss.Add(hzjs);
            foreach(var tjs in hzjs)
            {
                tjs.color = new Color(tjs.color.r, tjs.color.g, tjs.color.b, 0.0f);
            }

            //英文
            Text enText = _ResultCYInfo[i].transform.Find("En").GetComponent<Text>();
            enText.text = findEn[i].Replace('#',',');
            enText.color = new Color(enText.color.r, enText.color.g, enText.color.b, 0.0f);
            ens.Add(enText);
            if(enText.text == "N/A")
            {
                enText.gameObject.SetActive(false);
            }
            else
            {
                enText.gameObject.SetActive(true);
            }
        }


        _RstInfo.color = new Color(_RstInfo.color.r, _RstInfo.color.g, _RstInfo.color.b, 0.0f);
        for (int i = 0; i < findHZ.Count; i++)
        {
            Image[] rstInfoImg = _ResultCYInfo[i].GetComponentsInChildren<Image>();
            for (int j = 0; j < rstInfoImg.Length; j++)
            {
                //Tag,TagText,Image,SP,Bg,Line
                if(rstInfoImg[j].name == "Tag" 
                   || rstInfoImg[j].name == "Image"
                  || rstInfoImg[j].name == "SP"
                  || rstInfoImg[j].name == "Bg"
                   || rstInfoImg[j].name == "Line")
                {
                    rstInfoImg[j].color = new Color(rstInfoImg[j].color.r, rstInfoImg[j].color.g, rstInfoImg[j].color.b, 0.0f);
                }
            }

            Text[] rstInfoText = _ResultCYInfo[i].GetComponentsInChildren<Text>();
            for (int j = 0; j < rstInfoText.Length; j++)
            {
                if (rstInfoText[j].name == "TagText")
                {
                    rstInfoText[j].color = new Color(rstInfoText[j].color.r, rstInfoText[j].color.g, rstInfoText[j].color.b, 0.0f);
                }
            }
        }

        //延时一帧执行，否则获取到的位置不正确
        Sequence delayAni = DOTween.Sequence();
        delayAni.AppendInterval(0.1f).OnComplete(() =>{

            Sequence nextAni = DOTween.Sequence();
            Text[] scs = _ScorePanel.GetComponentsInChildren<Text>();
            Image[] sps = _ScorePanel.GetComponentsInChildren<Image>();
            foreach (var sc in scs)
            {
                nextAni.Join(sc.DOFade(0.2f, 0.3f));
            }
            foreach (var sp in sps)
            {
                nextAni.Join(sp.DOFade(0.2f, 0.3f));
            }

            //所有干扰字需要隐藏
            for (int i = 0; i < _HZList.Count;i++)
            {
                XTHZ xTHZ = _HZList[i].GetComponentInChildren<XTHZ>();
                if(xTHZ.GetHZ() != "" && !xTHZ.GetCantRestFinded())
                {
                    //干扰字
                    nextAni.Join(xTHZ.DoHZFade(0, 0.3f))
                            .Join(xTHZ.DoHZFindFade(0.0f, 0.3f))
                            .Join(xTHZ.DoHZFindBgFade(0.0f, 0.3f));
                }
            }

            nextAni.Join(_RstInfo.DOFade(200 / 255.0f, 0.3f));
            for (int i = 0; i < findCYText.Count; i++)
            {

                findCYText[i].gameObject.SetActive(false);
                _ResultAniText[i].text = findCYText[i].text;
                _ResultAniText[i].gameObject.SetActive(true);
                _ResultAniText[i].transform.position = findCYText[i].transform.position;
                _ResultAniText[i].color = new Color(_ResultAniText[i].color.r, 
                                                    _ResultAniText[i].color.g, 
                                                    _ResultAniText[i].color.b,200/255.0f);
            }

            nextAni.AppendInterval(0.01f);
            for (int i = 0; i < findCYText.Count; i++)
            {
                Sequence cyAni = DOTween.Sequence();

                cyAni.AppendInterval(0.05f * i)
                     .Append(_ResultAniText[i].transform.DOMove(hzs[i].transform.position, 0.5f));
                     
                Image[] rstInfoImg = _ResultCYInfo[i].GetComponentsInChildren<Image>();
                for (int j = 0; j < rstInfoImg.Length; j++)
                {
                    //Tag,TagText,Image,SP,Bg,Line
                    if (rstInfoImg[j].name == "Tag")
                    {
                        cyAni.Join(rstInfoImg[j].DOFade(10/255.0f, 0.5f));
                    }
                    else if(rstInfoImg[j].name == "Image")
                    {
                        cyAni.Join(rstInfoImg[j].DOFade(200 / 255.0f, 0.5f));
                    }
                    else if(rstInfoImg[j].name == "SP")
                    {
                        cyAni.Join(rstInfoImg[j].DOFade(50 / 255.0f, 0.5f));
                    }
                    else if(rstInfoImg[j].name == "Bg")
                    {
                        cyAni.Join(rstInfoImg[j].DOFade(1.0f, 0.5f));
                    }
                    else if(rstInfoImg[j].name == "Line")
                    {
                        cyAni.Join(rstInfoImg[j].DOFade(127/255.0f, 0.5f));
                    }
                }

                Text[] rstInfoText = _ResultCYInfo[i].GetComponentsInChildren<Text>();
                for (int j = 0; j < rstInfoText.Length; j++)
                {
                    if (rstInfoText[j].name == "TagText")
                    {
                        cyAni.Join(rstInfoText[j].DOFade(100/255.0f, 0.5f));
                    }
                }

                cyAni.Append(_ResultAniText[i].DOFade(0.0f, 0.3f))
                     .Join(hzs[i].DOFade(1.0f, 0.3f));

                cyAni.Append(jgs[i].DOFade(1.0f, 0.3f));

                cyAni.Append(pys[2 * i].DOFade(200/255f, 0.3f))
                     .Join(pys[i * 2 + 1].DOFade(1.0f, 0.3f));


                Sequence jsAni = DOTween.Sequence();
                for (int j = 0; j < jss[i].Count; j++)
                {
                    jsAni.Join(jss[i][j].DOFade(200/255f, 0.3f));
                }

                cyAni.Append(jsAni);

                cyAni.Append(ens[i].DOFade(200/255f, 0.2f));

                nextAni.Join(cyAni);
            }

            nextAni.Append(nextBtn.transform.DOScale(1.0f, 0.5f))
                   .Join(_GoNextBtn.DOFade(100 / 255.0f, 0.5f)).OnComplete(()=>{
                        nextBtn.GetComponent<ScaleAni>().enabled = true;
                        nextBtn.interactable = true;
                   });
        });
    }
    //结束时必须删除 
    public Image _RstInfo;
    public GameObject []_ResultCYInfo;
    public GameObject _hzPrafab;
    private List<GameObject> _tmpRstTextList = new List<GameObject>(); 
    private void FitDialogContentText(Transform content, string str)
    {
        List<string> jsstr = new List<string>();
        foreach (var s in str)
        {
            jsstr.Add("" + s);
        }

        float hzSize = _hzPrafab.GetComponentInChildren<Text>().transform.GetComponent<RectTransform>().rect.width;

        RectTransform jsrt = content.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = content.GetComponentInChildren<GridLayoutGroup>();
        float spaceArea = (jsLyt.padding.top + jsLyt.padding.bottom) * jsrt.rect.width;
        float jscellSize = (float)Math.Sqrt(((jsrt.rect.width - hzSize) * (jsrt.rect.height - hzSize) - spaceArea) / jsstr.Count);
        jscellSize = jscellSize > hzSize ? hzSize : jscellSize;
        jsLyt.cellSize = new Vector2(jscellSize, jscellSize);

        for (int i = 0; i < jsstr.Count; i++)
        {
            GameObject txt = Instantiate(_hzPrafab, content) as GameObject;
            txt.SetActive(true);
            txt.GetComponentInChildren<Text>().text = jsstr[i];
            _tmpRstTextList.Add(txt);
        }
    }

    public void GoNextBtnClick()
    {
        Button nextBtn = _GoNextBtn.GetComponentInChildren<Button>();
        nextBtn.GetComponent<ScaleAni>().enabled = false;
        nextBtn.interactable = false;
        Sequence nextAni = DOTween.Sequence();
        nextAni.Append(nextBtn.transform.DOScale(0.0f, 0.5f));
        Text[] scs = _ScorePanel.GetComponentsInChildren<Text>();
        Image[] sps = _ScorePanel.GetComponentsInChildren<Image>();
        foreach (var sc in scs)
        {
            nextAni.Join(sc.DOFade(200.0f/255, 0.5f));
        }
        foreach (var sp in sps)
        {
            nextAni.Join(sp.DOFade(50 / 255.0f, 0.5f));
        }
        nextAni.SetEase(Ease.OutSine);
        nextAni.OnComplete(()=>
        {
            InitTiMu(false);
        });
    }

    public override void OnSwipe(Define.SWIPE_TYPE type)
    {
        if (type == Define.SWIPE_TYPE.RIGHT)
        {
            //当处于结束界面时，可以右滑切题
            Button nextBtn = _GoNextBtn.GetComponentInChildren<Button>();
            if (_GoNextBtn.gameObject.activeSelf && nextBtn.interactable)//当可以点击时
            {
                GoNextBtnClick();
            }
        }
    }

    public Image _TipBrush;
    [System.Serializable] public class OnShowClickWaveEvent : UnityEvent<Transform> { }
    public OnShowClickWaveEvent OnShowClickWave;
    private int _CurrentDZTSUsedCnt = 0;
    //如果非独体字则使用这个提示
    public void DoFirstTip()
    {
        _CurrentSelectIndex.Clear();

        DisableBottomBtn(true);
        SetCanShowPencil(false);
        //从cdata随机找一个提示

        int CurrentIndex = -1;
        if(_CurrentData.Count > 0)
        {
            string idx = _CurrentData[HZManager.GetInstance().GenerateRandomInt(0, _CurrentData.Count)][(int)HZManager.eSHZCName.HZ_END];
            string[] ids = idx.Split('#');
            CurrentIndex = int.Parse(ids[0]);
        }
        else
        {
            //第一次提示不会出现这种情况
            //需要从方阵找到独体字，如果找不到，说明出错了，无法使用
            for (int m = 0; m < XT_HZ_MATRIX * XT_HZ_MATRIX; m++)
            {
                XTHZ xTHZ0 = _HZList[m].GetComponentInChildren<XTHZ>();
                string hz = xTHZ0.GetHZ();
                if (hz != "" && !xTHZ0.GetCantRestFinded())
                {
                    if (HZManager.GetInstance().GetSHZByHZ(hz).Count != 0)
                    {
                        CurrentIndex = m;
                    }
                }
            }
        }

        if(CurrentIndex == -1)
        {
            //
            ShowToast("已经没有可以提示的字了，请进入下一题");
            //InitTiMu(false);
            return;
        }


        XTHZ xTHZ = _HZList[CurrentIndex].GetComponent<XTHZ>();
        Text hzText = _HZList[CurrentIndex].GetComponentInChildren<Text>();
        Vector3 pos = new Vector3(hzText.transform.position.x + 50, hzText.transform.position.y + 50, 0.0f);

        _TipBrush.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.SetId("DoFirstTip");
        _TipBrush.color = new Color(_TipBrush.color.r, _TipBrush.color.g, _TipBrush.color.b, 0.0f);
        _TipBrush.transform.position = pos;
        sequence.Append(_TipBrush.DOFade(1.0f, 0.5f));

        sequence.Append(xTHZ.DoHZColor(Define.YELLOW, 0.5f))
                .Join(hzText.transform.DOShakeScale(0.5f, 0.2f));


        int x = CurrentIndex % XT_HZ_MATRIX;
        int y = CurrentIndex / XT_HZ_MATRIX;
        List<int> moving = new List<int>();
        //上
        if (y - 1 >= 0)
        {
            moving.Add((y - 1) * XT_HZ_MATRIX + x);
        }
        //右上
        if (y - 1 >= 0 && x + 1 < XT_HZ_MATRIX)
        {
            moving.Add((y - 1) * XT_HZ_MATRIX + x + 1);
        }

        //右
        if (x + 1 < XT_HZ_MATRIX)
        {
            moving.Add(y * XT_HZ_MATRIX + x + 1);
        }

        //右下
        if (y + 1 < XT_HZ_MATRIX && x + 1 < XT_HZ_MATRIX)
        {
            moving.Add((y + 1) * XT_HZ_MATRIX + x + 1);
        }
        //下
        if (y + 1 < XT_HZ_MATRIX)
        {
            moving.Add((y + 1) * XT_HZ_MATRIX + x);
        }

        // 左下
        if (y + 1 < XT_HZ_MATRIX && x - 1 >= 0)
        {
            moving.Add((y + 1) * XT_HZ_MATRIX + x - 1);
        }

        //左
        if (x - 1 >= 0)
        {
            moving.Add(y * XT_HZ_MATRIX + x - 1);
        }

        //左上
        if (x - 1 >= 0 && y - 1 >= 0)
        {
            moving.Add((y - 1) * XT_HZ_MATRIX + x - 1);
        }

        for (int p = 0; p < moving.Count; p++)
        {
            Text hzText2 = _HZList[moving[p]].GetComponentInChildren<Text>();
            if (hzText2.text == "") continue;
            Vector3 pos2 = new Vector3(hzText2.transform.position.x + 50, hzText2.transform.position.y + 50, 0.0f);
            float speed = 0.5f / ((float)(Math.Sqrt(p + 4) / Math.Sqrt(p + 1)));
            sequence.Append(_TipBrush.transform.DOMove(pos2, speed))
                    .Join(hzText2.transform.DOScale(1.2f, speed))
                    .Append(_TipBrush.transform.DOMove(pos, speed))
                    .Join(hzText2.transform.DOScale(1.0f, speed));
        }

        sequence.Append(_TipBrush.DOFade(0.0f, 0.2f))
                .OnComplete(() =>
                {
                    _TipBrush.gameObject.SetActive(false);
                    DisableBottomBtn(false);
                    SetCanShowPencil(true);

                    if (int.Parse(_leftDZTSTime.text.Split('+')[1]) < 3)
                    {
                        ShowToast("已提示首构件及可划选方向");
                    }
                });

        OnShowClickWave.Invoke(hzText.transform);
    }

    // 非独体字采用这个提示，但仅提示首个构件
    public void DoTip()
    {
        _CurrentDZTSUsedCnt++;
        if (_CurrentDZTSUsedCnt == 1)
        {
            DoFirstTip();
            return;
        }

        _CurrentSelectIndex.Clear();
        //动画执行部分提示字，此时不能操作
        SetCanShowPencil(false);
        DisableBottomBtn(true);

        int CurrentIndex = -1;
        if (_CurrentData.Count > 0)
        {
            string idx = _CurrentData[HZManager.GetInstance().GenerateRandomInt(0, _CurrentData.Count)][(int)HZManager.eSHZCName.HZ_END];
            string[] ids = idx.Split('#');
            CurrentIndex = int.Parse(ids[0]);
        }
        else
        {
            //第一次提示不会出现这种情况
            //需要从方阵找到独体字，如果找不到，说明出错了，无法使用
            for (int m = 0; m < XT_HZ_MATRIX * XT_HZ_MATRIX; m++)
            {
                XTHZ xTHZ0 = _HZList[m].GetComponentInChildren<XTHZ>();
                string hz = xTHZ0.GetHZ();
                if (hz != "" && !xTHZ0.GetCantRestFinded())
                {
                    if (HZManager.GetInstance().GetSHZByHZ(hz).Count != 0)
                    {
                        CurrentIndex = m;
                    }
                }
            }
        }

        if (CurrentIndex == -1)
        {
            //
            ShowToast("已经没有可以提示的字了，请进入下一题");
            //InitTiMu(false);
            return;
        }

        _TipBrush.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.SetId("DoTip");
        Color c = Define.FONT_COLOR_LIGHT;
        float speed = 1.5f;

        XTHZ xTHZ = _HZList[CurrentIndex].GetComponent<XTHZ>();

        Text hzText = _HZList[CurrentIndex].GetComponentInChildren<Text>();
        Vector3 pos = new Vector3(hzText.transform.position.x + 50, hzText.transform.position.y + 50, 0.0f);
        sequence.Append(xTHZ.DoHZColor(Define.YELLOW, speed))
                .Join(hzText.transform.DOShakeScale(speed, 0.2f));

            _TipBrush.color = new Color(_TipBrush.color.r, _TipBrush.color.g, _TipBrush.color.b, 0.0f);
            _TipBrush.transform.position = pos;
            sequence.Join(_TipBrush.DOFade(1.0f, speed ))
                    .Join(_TipBrush.transform.DOShakePosition(speed));

            //执行动画，结束时，方可点击
        sequence.OnComplete(() =>
        {
            _TipBrush.gameObject.SetActive(false);
            DisableBottomBtn(false);
            SetCanShowPencil(true);
        });
    }


    private float _SearchDifficult = 0.0f;
    private int _DisturbCnt = 0;//干扰字个数
    private enum eDisturbDifficultType
    {
        EASY,//只取组件序列中的一个组件
        NORMAL,//取组件序列中的一半组件
        HARD,//只最后一个或者第一个组件不取
    }
    private eDisturbDifficultType _DisturbDifficult = eDisturbDifficultType.EASY;
    //根据通过数，更新阅读题目的时间以及字掉落速度
    private void InitDiffcult()
    {
        //每100题增加一个干扰字，3万题->30个
        int blank = XT_HZ_MATRIX * XT_HZ_MATRIX - _CurrentAllZJCnt;
        // <100 只有搜索难度，不会插入干扰字
        if (_ChuangGuan < 100)
        {
            _SearchDifficult = 1 - 4 * Mathf.Pow((_ChuangGuan + 1) / 100.0f - 0.5f, 2);
            float logDif = Mathf.Log(_ChuangGuan + 1, 100);//0->1

            //25 - 4 = 21 个空白字
            //最多有2个干扰字
            _DisturbCnt = (int)(logDif * blank / 2);

            int df = (int)(100 * logDif) + 1;
            float tmp =  Mathf.Log(df,100);

            if (tmp > 0.5f)
            {
                _DisturbDifficult = eDisturbDifficultType.NORMAL;
            }
            else
            {
                _DisturbDifficult = eDisturbDifficultType.EASY;
            }

        }
        else if (_ChuangGuan >= 100 && _ChuangGuan < 1000)
        {
            _SearchDifficult = 1 - 4 * Mathf.Pow((_ChuangGuan + 1) / 1000.0f - 0.5f, 2);
            float logDif = Mathf.Log(_ChuangGuan + 1, 1000);//log101->1开始，而不是从最小，这样有一定阶梯

            //25-8 = 17个空白字 ，插入1/4干扰字 4个字
            //最多 1/3 ?
            _DisturbCnt = (int)(logDif * blank *3 / 4);

            int df = (int)(100 * logDif) + 1;
            float tmp = Mathf.Log(df,100);
            if (tmp < 0.33f)
            {
                _DisturbDifficult = eDisturbDifficultType.EASY;
            }
            else if(tmp > 0.66f)
            {
                _DisturbDifficult = eDisturbDifficultType.HARD;
            }
            else
            {
                _DisturbDifficult = eDisturbDifficultType.NORMAL;
            }
        }
        else if (_ChuangGuan >= 1000 && _ChuangGuan < 5000)
        {
            _SearchDifficult = 1 - 4 * Mathf.Pow((_ChuangGuan + 1) / 5000.0f - 0.5f, 2);
            float logDif = Mathf.Log(_ChuangGuan + 1, 5000);//log1001->1
            //25-12 = 13 空白字  插入3/4空白字 9个字
            //最多
            _DisturbCnt = (int)(logDif * 7 * blank / 8);

            int df = (int)(100 * logDif) + 1;
            float tmp = Mathf.Log(df,100);
            if (tmp > 0.5f)
            {
                _DisturbDifficult = eDisturbDifficultType.HARD;
            }
            else
            {
                _DisturbDifficult = eDisturbDifficultType.NORMAL;
            }
        }
        else if (_ChuangGuan >= 5000)
        {
            // 25-16 = 9 空白字
            _SearchDifficult = (float)((3 + Define.RandGauss(0.0,1.0)) / 6);//采用正态分布

            if(_SearchDifficult < 0)
            {
                _SearchDifficult = 0;
            }
            else if (_SearchDifficult > 1)
            {
                _SearchDifficult = 1;
            }


            _DisturbCnt = blank;//超过5000条汉字，干扰字插满
            _DisturbDifficult = eDisturbDifficultType.HARD;//全是3字汉字插入，这样难度其实很大，应该都是看着像汉字但是缺一个字，如果不足，依次减少
        }

        //大于8000时将全部插入字根，不能存在非答案以外的选字
    }

    //错误的时候没有汉字
    public Text _RightRateText;// 正确率
    public Text _RightCntText;
    public Text _FindCntText;//


    public override void OnInit()
    {

    }

    private void InitScore()
    {
        int total_right = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.TOTAL_SELECT_RIGHT_CNT, 0);
        int total_error = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.TOTAL_SELECT_ERROR_CNT, 0);


        double rate = 0;
        if (total_right + total_error != 0)
        {
            rate = Math.Round(100.0000 * total_right / (total_right + total_error), 2);
        }
        _RightRateText.text = "" + rate + "%";
        _RightCntText.text = "" + total_right;

        _FindCntText.text = "" + Setting.GetFindHZCnt();

        _ChuangGuan = Setting.GetFindHZCnt();
    }

    public  override void ReportScoreCallback(bool successed)
    {
        if (_NeedReportAchievement)
        {
            //此时可以上报成就
            ReportAchievement(Setting.GetFindHZCnt());
        }
    }

    public override void ReportAchievementCallback(bool successed)
    {
        GameCenter.AchievementType type = GameCenter.AchievementType.HZTongSheng;

        for (int i = (int)GameCenter.AchievementType.HZTongSheng; i <= (int)GameCenter.AchievementType.HZZhuangYuan; i++)
        {
            if (Setting.getPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + "" + ((GameCenter.AchievementType)i), 0f) < 100f)
            {
                type = ((GameCenter.AchievementType)i);
                break;
            }
        }
        //Setting.GetFindHZCnt();
        float percent = 100f * Setting.GetFindHZCnt() / Define.ACHIEVEMENT_SCORE[(int)type];
        if(percent >= 100f)
        {
            //type成就获得
            Setting.setPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + "" + type, 100f);
            if(percent > 100f)
            {
                //需要设置当前
                if((int)type < (int)GameCenter.AchievementType.HZZhuangYuan)
                {
                    Setting.setPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + "" + (GameCenter.AchievementType)((int)type + 1), Setting.GetFindHZCnt() / Define.ACHIEVEMENT_SCORE[(int)type + 1]);
                }
            }
        }
        else
        {
            //说明没有新成就，普通上报
            Setting.setPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + "" + type, percent);
        }
    }

    private bool _NeedReportAchievement = false;
    //这里可能有效率问题，需要优化
    [System.Serializable] public class OnAddFindHZEvent : UnityEvent<string> { }
    public OnAddFindHZEvent OnAddFindHZ;
    private  void UpdateScore(List<string> hz)
    {
        int total_right = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.TOTAL_SELECT_RIGHT_CNT, 0);
        int total_error = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.TOTAL_SELECT_ERROR_CNT, 0);

        //划选正确
        if (hz.Count > 0)
        {
            total_right += hz.Count;
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.TOTAL_SELECT_RIGHT_CNT, total_right);
            ReportScore(total_right);

            DoScoreTextAni(_RightCntText, "" + total_right, 0.6f);

            //这里需要检测划选的汉字是否已经存在于之前的发现列表里，不存在则增加发现数，否则不变
            foreach (var st in hz)
            {
                if (!Setting.CheckHasFind(st))
                {
                    DoScoreTextAni(_FindCntText, "" + Setting.GetFindHZCnt(), 0.3f);

                    _ChuangGuan = Setting.GetFindHZCnt();//设置闯关数等于发现的汉字数

                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.FIND_CY_CNT, _ChuangGuan);//保存发现汉字数量，为了主界面使用

                    HZManager.GetInstance().UpdateSHZFindState(true, st);

                    //此时可以上报成就
                    // ReportAchievement(Setting.GetFindHZCnt());//需要等待响应才能上报
                    _NeedReportAchievement = true;
                    OnAddFindHZ.Invoke(st);
                }
            }
        }
        else
        {
            total_error += 1;
            //不正确，只需要增加错误次数，更新正确率即可
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.TOTAL_SELECT_ERROR_CNT, total_error);
        }

        double rate = 0;
        if (total_right + total_error != 0)
        {
            rate = Math.Round(100.0000 * total_right / (total_right + total_error), 2);
        }

        DoScoreTextAni(_RightRateText, "" + rate + "%", 1.0f);
    }

    private void DoScoreTextAni(Text t,string s,float delay)
    {
        Sequence textAniSeq = DOTween.Sequence();
        textAniSeq.AppendInterval(delay)
                  .Append(t.DOText(s,0.5f))
                  .Join(t.transform.DOScale(1.2f,0.5f))
                  .Append(t.DOFade(0.5f,0.5f))
                  .Join(t.transform.DOScale(0.8f, 0.5f))
                  .Append(t.DOFade(1.0f, 0.5f))
                  .Join(t.transform.DOScale(1.0f, 0.5f))
                  .SetEase(Ease.OutBounce);
    }

    protected override int GetCurrentScore()
    {
        return _ChuangGuan;//得分就是当前已经收集的汉字个数
    }

    public override void AbandonGame(Action cb = null)
    {
        DestroyObj(_HZList);
        DestroyObj(_tmpRstTextList);

        _CanExist = true;
        gameObject.SetActive(false);

        _Pencil.DisablePencil();

        CancelInvoke("ShowNoteInfo");//停止轮播小提示
        //这里可以稍微延时，防止闪屏错觉
        Invoke("GameEnd", 0.1f);
    }

    public override void ExistGame()
    {
        AbandonGame();
    }

    protected override void StopAllClock()
    {

    }

    protected override string GetStudyName()
    {
        return gameObject.name;
    }

    //已经结束了，不能开启可划选
    public override void OpenHelpPanel(bool open)
    {
        if (!_RstInfo.gameObject.activeSelf)
        {
            SetCanShowPencil(!open);
        }
        else
        {
            SetCanShowPencil(false);
        }
    }

    #region 选填题

    public void OnDZTSClick()
    {

        string dzts = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, "3#0#0#3#0");
        string[] dztsCnt = dzts.Split('#');
        int currentLeftShowDZTSCnt = int.Parse(dztsCnt[0]);

        if (currentLeftShowDZTSCnt > 0)
        {
            currentLeftShowDZTSCnt--;

            if (currentLeftShowDZTSCnt == 0)
            {
                //答案次数已经用完了
                if (!IAP.getHasBuy(IAP.IAP_DZTS6) && !IAP.getHasBuy(IAP.IAP_DZTS12))
                {
                    _buyDZTSBtn.gameObject.SetActive(true);
                    _leftDZTSTime.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            ShowToast("今天次数已用完(明天重置)，若未购买请购买单6或12");
            return;
        }

        dztsCnt[0] = ""+currentLeftShowDZTSCnt;
        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT,String.Join("#", dztsCnt));

        _leftDZTSTime.text = "+" + currentLeftShowDZTSCnt;
        //执行提示

        DoTip();
    }
    public void OnCKDAClick()
    {
        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
        if (currentLeftShowDaanCnt > 0)
        {
            currentLeftShowDaanCnt--;
        }
        else
        {
            //答案次数已经用完了
            ShowToast("今天次数已用完，请明天再试(次日重置)");
            return;
        }
        
        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, currentLeftShowDaanCnt);

        _leftDaanTime.text = "+" + currentLeftShowDaanCnt;

        SetCanShowPencil(false);
        DisableBottomBtn(true);

        List<Button> notFind = GetAllEmptyResultCY();//没有找到

        //隐藏所有汉字
        Sequence resultAni = DOTween.Sequence();
        //防止选中动画没有结束
        resultAni.AppendInterval(0.4f).AppendInterval(0.0f);

        //剩余可用直接使用的字
        int errCnt = 0;
        int rightCnt = 0;
        List<string> right = new List<string>();

        if (_CurrentData.Count > 0)
        {
            if(_CurrentData.Count >= notFind.Count)
            {
                //全部可用从现有的获取
                rightCnt = notFind.Count;
            }
            else
            {
                //只有部分可以从现有的获取，其余需要从方阵中获取
                errCnt = notFind.Count - _CurrentData.Count;
                rightCnt = _CurrentData.Count;
            }
        }
        else
        {
            //需要从方阵中获取了
            errCnt = notFind.Count;
        }

        for (int i = 0; i < rightCnt; i++)
        {
            right.Add(_CurrentData[i][(int)HZManager.eSHZCName.HZ_HZ]);
            string[] idx = _CurrentData[i][(int)HZManager.eSHZCName.HZ_END].Split('#');

            for (int j = 0; j < idx.Length; j++)
            {
                int index = int.Parse(idx[j]);
                XTHZ xTHZ = _HZList[index].GetComponentInChildren<XTHZ>();

                xTHZ.SetCantRestFinded(true);

                Sequence resultAni2 = DOTween.Sequence();
                resultAni2.AppendInterval(0.15f * (i + 1))
                          .Join(xTHZ.DoHZColor(Define.YELLOW, 0.4f))
                          .Join(xTHZ.DoHZIDFade(0.0f, 0.4f))
                          .Join(xTHZ.DoHZIDBGFade(0.0f, 0.4f));

                if (j == 0)
                {
                    if (notFind[i].gameObject.activeSelf)
                    {
                        Text[] cy = notFind[i].GetComponentsInChildren<Text>(true);
                        for (int n = 0; n < 4; n++)
                        {
                            resultAni2.Join(cy[n].DOFade(0.0f, 0.2f));
                        }
                    }
                }

                resultAni2
                          .Append(xTHZ.GetHZText().transform.DOMove(notFind[i].transform.Find("Text Info/HZ").position, 0.8f))
                          .Join(xTHZ.GetHZText().transform.DOScale(0.2f, 0.8f))
                          .Join(xTHZ.DoHZFade(0.0f, 0.8f))
                          .Join(xTHZ.DoHZFindFade(0.0f, 0.8f))
                          .Join(xTHZ.DoHZFindBgFade(0.0f, 0.8f));
                int current = i;
                if (j == idx.Length - 1)
                {
                    resultAni2.AppendCallback(() =>
                    {
                        Text[] cy = notFind[current].GetComponentsInChildren<Text>(true);
                        for (int n = 0; n < 4; n++)
                        {
                            cy[n].gameObject.SetActive(false);
                        }

                        cy[4].text = _CurrentData[current][(int)HZManager.eSHZCName.HZ_HZ];
                        cy[4].color = new Color(cy[4].color.r, cy[4].color.g, cy[4].color.b, 0.0f);
                        cy[4].gameObject.SetActive(true);

                        cy[4].DOFade(200 / 255.0f, 0.5f);
                    });
                }

                resultAni2.SetEase(Ease.OutSine);
                resultAni.Join(resultAni2);
            }
        }
        //需要从方阵中找出的 ---这里只能寻找独体字，否则有较大难度了，暂时不支持
        int finErrCnt = 0;
        for (int i = notFind.Count - errCnt; i < notFind.Count; i++)
        {
            for (int m = 0; m < XT_HZ_MATRIX * XT_HZ_MATRIX;m++)
            {
                XTHZ xTHZ = _HZList[m].GetComponentInChildren<XTHZ>();
                string hz = xTHZ.GetHZ();
                if(hz != "" && !xTHZ.GetCantRestFinded())
                {
                    if(HZManager.GetInstance().GetSHZByHZ(hz).Count != 0)
                    {
                        finErrCnt++;
                        right.Add(hz);

                        xTHZ.SetCantRestFinded(true);

                        Sequence resultAni2 = DOTween.Sequence();
                        resultAni2.AppendInterval(0.15f * (i + 1))
                                  .Join(xTHZ.DoHZColor(Define.YELLOW, 0.4f))
                                  .Join(xTHZ.DoHZIDFade(0.0f, 0.4f))
                                  .Join(xTHZ.DoHZIDBGFade(0.0f, 0.4f));


                        if (notFind[i].gameObject.activeSelf)
                        {
                            Text[] cy = notFind[i].GetComponentsInChildren<Text>(true);
                            for (int n = 0; n < 4; n++)
                            {
                                resultAni2.Join(cy[n].DOFade(0.0f, 0.2f));
                            }
                        }


                        resultAni2
                                  .Append(xTHZ.GetHZText().transform.DOMove(notFind[i].transform.Find("Text Info/HZ").position, 0.8f))
                                  .Join(xTHZ.GetHZText().transform.DOScale(0.2f, 0.8f))
                                  .Join(xTHZ.DoHZFade(0.0f, 0.8f))
                                   .Join(xTHZ.DoHZFindFade(0.0f, 0.8f))
                                  .Join(xTHZ.DoHZFindBgFade(0.0f, 0.8f));
                        int current = i;

                        resultAni2.AppendCallback(() =>
                        {
                            Text[] cy = notFind[current].GetComponentsInChildren<Text>(true);
                            for (int n = 0; n < 4; n++)
                            {
                                cy[n].gameObject.SetActive(false);
                            }

                            cy[4].text = hz;
                            cy[4].color = new Color(cy[4].color.r, cy[4].color.g, cy[4].color.b, 0.0f);
                            cy[4].gameObject.SetActive(true);

                            cy[4].DOFade(200 / 255.0f, 0.5f);
                        });


                        resultAni2.SetEase(Ease.OutSine);
                        resultAni.Join(resultAni2);
                        break;
                    }
                }
            }
        }

        if(finErrCnt < errCnt)
        {
            //无法全部找到，说明被划错了，无法找到更多满足题目要求的，给予提示
            ShowToast("由于您划选导致的非连通性，无法找到全部答案。");
        }

        UpdateScore(right);

        resultAni.AppendInterval(0.3f)
            .OnComplete(DoResultCYAni);

    }
    #endregion

    #region 算法部分
    //-----------------内部使用接口-----------------------
    //---------------------诗句路径搜索-----------------------------------
    private int X = 7;
    private int Y = 7;
    private int N = 5;
    private Action<bool, int> SearchingCb = null;//参数：当前节点是否合法

    /// <summary>
    /// 自动搜索一定区域内合法的笔画/字根组装序列
    /// </summary>
    /// <param name="x">搜索起始点坐标x</param>
    /// <param name="y">搜索起始点坐标y</param>
    /// <param name="n">搜索半径</param>
    /// <param name="fininshCallback">搜索完成回调，向ui主线程返回搜索结果</param>
    /// <param name="searchingCallback">搜索中回调，如果搜索复杂度较大，可通过该回调给予提示</param>
    public void SearchSJPath(int x, int y, int n,
                        Action<List<int>> fininshCallback,
                        Action<bool, int> searchingCallback = null)
    {
        //初始化参数
        X = x;
        Y = y;
        N = n;

        SearchingCb = searchingCallback;

        List<int> xy = new List<int>();
        for (int i = 0; i < XT_HZ_MATRIX * XT_HZ_MATRIX; i++)
        {
            xy.Add(0);//全部设置为未搜索
        }

        //将起始字点设置为已经搜索
        xy[Y * XT_HZ_MATRIX + X] = 1;
        List<int> searched = new List<int>
        {
            Y * XT_HZ_MATRIX + X
        };

        //子线程中进行搜索，不会阻塞ui
        Loom.RunAsync(() =>
        {
            Thread thread = new Thread(() =>
            {
                if (!DoSearch(xy, null, searched, X, Y))
                {
                    //找不到合法序列，清空searched列表
                    searched.Clear();
                }

                Loom.QueueOnMainThread((param) =>
                {
                    if (fininshCallback != null)
                    {
                        fininshCallback(searched);//返回合法的索引id，即对应到界面节点
                    }
                }, null);
            });

            thread.Start();
        });
    }

    struct SD
    {
        public int index;
        public float priority;
    }

    private List<int> GetNextSearch(List<int> xy, int x, int y, List<int> path)
    {
        List<int> searching = new List<int>();
        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= XT_HZ_MATRIX) continue;
            if (i <= X - N || i >= X + N) continue;

            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= XT_HZ_MATRIX) continue;// 超出边界
                if (j <= Y - N || j >= Y + N) continue;//超出搜索半径

                if (i == x && j == y) continue;//同一个点

                if (xy[j * XT_HZ_MATRIX + i] == 0)
                {
                    searching.Add(j * XT_HZ_MATRIX + i);
                }
            }
        }

        //可搜索的点少于2个，直接返回该点
        if (searching.Count <= 1)
        {
            return searching;
        }

        //洗牌搜索序列，防止固定的方向
        ShuffleSJList(searching);

        //优选搜索周围空白的字最多的字
        List<SD> sds = new List<SD>();
        for (int i = 0; i < searching.Count; i++)
        {
            SD sd;
            sd.index = searching[i];
            sd.priority = GetHZPriority(searching[i], xy, path);
            sds.Add(sd);
        }

        sds.Sort((a, b) =>
        {
            //return a.priority > b.priority?1:0;//升排列，也就是周围字最多，最难的路线

            if(a.priority > b.priority)
            {
                return 1;
            }
            else if (a.priority < b.priority)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        });

        searching.Clear();
        foreach (var sd in sds)
        {
            searching.Add(sd.index);
        }

        //要把一样的值进行一定的交换处理，否则都一样，选择的顺序也就有了规律

        int sindex = (int)((1 - _SearchDifficult) * searching.Count);//0=< 1-_SearchDifficult <1
        if (sindex >= searching.Count - 1) sindex = searching.Count - 1;

        List<int> searching1 = new List<int>();
        List<int> searching2 = new List<int>();

        for (int i = 0; i < sindex; i++)
        {
            searching1.Add(searching[i]);
        }

        for (int i = sindex; i < searching.Count; i++)
        {
            searching2.Add(searching[i]);
        }

        searching.Clear();
        searching.AddRange(searching2);// 优先往简单的方向搜索
        searching1.Reverse();
        searching.AddRange(searching1);//再往越来越难的方向搜索

        return searching;
    }

    // 获取文字的搜索权重
    private int GetHZPriority(int index, List<int> xy, List<int> path)
    {
        int p = 0;
        int p1 = 0;
        int p2 = 0;

        int x = index % XT_HZ_MATRIX;
        int y = index / XT_HZ_MATRIX;


        //使用距离加权难度系数
        for (int i = 0; i < path.Count; i++)
        {
            int xx = path[i] % XT_HZ_MATRIX;
            int yy = path[i] / XT_HZ_MATRIX;
            p1 += (int)Math.Sqrt((xx - x) * (xx - x) + (yy - y) * (yy - y));
        }

        //使用文字密集成度加权难度系数
        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= XT_HZ_MATRIX) continue;
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= XT_HZ_MATRIX) continue;// 超出边界

                if (i == x && j == y) continue;//同一个点

                if (xy[j * XT_HZ_MATRIX + i] == 0)
                {
                    p2++;
                }
            }
        }


        //100关以内，适当降低p2的比重，否则会出现往中间集中，难度过高
        //达到插入干扰字以后，搜索权重的比例，按照干扰字的增多降低，直到为0
        //这个控制是为当需要插入字时，诗句更分散，难度更高
        //否则当搜索难度最高时，诗句全部靠在一起，插入干扰字失去意义
        //p2 = (int)(p2 * (0.5f + _SearchDifficult * 0.5f));//0.625-1.0

        p2 = (int)(p2 * _SearchDifficult);

        // 需要找到更加科学的 两个参数的 比例或者公式，这里是简单相加
        p = p1 + p2;

        return p;
    }

    //文字密集成度
    private float GetHZPriority(string[] xy, int index)
    {
        float p = 0;
        float p2 = 0;
        float p3 = 0;

        int x = index % XT_HZ_MATRIX;
        int y = index / XT_HZ_MATRIX;


        //周围为1的点
        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= XT_HZ_MATRIX) continue;
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= XT_HZ_MATRIX) continue;// 超出边界

                if (i == x && j == y) continue;//同一个点

                ////这里如果有字则增加权重，而不是空
                /// 这样可以保证插入字干扰性更大，否则在边界时误差太大
                if (xy[j * XT_HZ_MATRIX + i] != "")
                {
                    p++;
                }
            }
        }

        //距离为2的点
        for (int i = x - 2; i <= x + 2; i++)
        {
            if (i < 0 || i >= XT_HZ_MATRIX) continue;
            for (int j = y - 2; j <= y + 2; j++)
            {
                if (j < 0 || j >= XT_HZ_MATRIX) continue;// 超出边界
                if (i == x && j == y) continue;//同一个点

                if (Math.Abs(i - x) < 2 && Math.Abs(j - y) < 2) continue;//距离小于2

                ////这里如果有字则增加权重，而不是空
                /// 这样可以保证插入字干扰性更大，否则在边界时误差太大
                if (xy[j * XT_HZ_MATRIX + i] != "")
                {
                    p2++;
                }
            }
        }

        //距离为3的点
        for (int i = x - 3; i <= x + 3; i++)
        {
            if (i < 0 || i >= XT_HZ_MATRIX) continue;
            for (int j = y - 3; j <= y + 3; j++)
            {
                if (j < 0 || j >= XT_HZ_MATRIX) continue;// 超出边界
                if (i == x && j == y) continue;//同一个点

                if (Math.Abs(i - x) < 3 && Math.Abs(j - y) < 3) continue;//距离小于2

                ////这里如果有字则增加权重，而不是空
                /// 这样可以保证插入字干扰性更大，否则在边界时误差太大
                if (xy[j * XT_HZ_MATRIX + i] != "")
                {
                    p3++;
                }
            }
        }

        return p + (float)Math.Sqrt(p2)/2 + (float)(Math.Sqrt(Math.Sqrt(p3)))/3;
    }
    private bool DoSearch(List<int> xy, List<int> searching, List<int> searched, int x, int y)
    {
        //根节点
        if (searching == null)
        {
            return DoSearch(xy, GetNextSearch(xy, x, y, searched), searched, x, y);
        }

        //检测是否搜索完成
        if (searched.Count == _CurrentAllZJCnt)
        {
            return true;
        }

        //没有可以搜索的，终止//已经到达死角或者终结
        if (searching.Count == 0 || searched.Count >= N * N /*范围内全部节点*/)
        {
            return false;
        }

        //对当前的进行检测
        foreach (var sc in searching)
        {
            int xx = sc % XT_HZ_MATRIX;
            int yy = sc / XT_HZ_MATRIX;

            searched.Add(sc);
            xy[sc] = 1;

            //这里做一些视觉效果
            Loom.QueueOnMainThread((param) =>
            {
                if (SearchingCb != null)
                {

                    SearchingCb(true, sc);
                }
            }, null);

            //System.Threading.Thread.Sleep(1000);

            if (DoSearch(xy, GetNextSearch(xy, xx, yy, searched), searched, xx, yy))
            {
                return true;
            }
            else
            {
                //这个节点不合法，后退到前一个
                xy[sc] = 0;
                searched.RemoveAt(searched.Count - 1);

                Loom.QueueOnMainThread((param) =>
                {
                    if (SearchingCb != null)
                    {
                        SearchingCb(false, sc);
                    }
                }, null);
            }
        }

        return false;
    }

    //返回正确的汉字
    private List<string> CheckIfRight()
    {
        List<string> ret = new List<string>();

        //选中的汉字
        string stCY = "";
        for (int i = 0; i < _CurrentSelectIndex.Count;i++)
        {
            stCY += _HZList[_CurrentSelectIndex[i]].GetComponentInChildren<Text>().text;
        }

        //首先检测是否存在于预选中的汉字中，不存在再从数据库中检查
        bool isCorrectCY = false;
        bool needDelIfInDb = false;
        for (int j = _CurrentData.Count - 1; j >= 0; j--)
        {
            //如果有一个组件和对应data中保存的一致，但不是该字，则删除data以及ci，因为已经破坏了
            //只有当选择的也构成字时才删除，否则不会删除

            string[] idx = _CurrentData[j][(int)HZManager.eSHZCName.HZ_END].Split('#');

            if(idx.Length != _CurrentSelectIndex.Count)
            {
                //肯定不正确
                //需要查询是否有部分索引存在于cdata中，有则需要删除，如果同时构成字的话
                bool ext = false;
                foreach(var id in idx)
                {
                    foreach(var index in _CurrentSelectIndex)
                    {
                        if(int.Parse(id) == index)
                        {
                            ext = true;
                            break;
                        }
                    }
                    if(ext)
                    {
                        needDelIfInDb = true;
                        break;
                    }
                }
            }
            else
            {
                //可能正确
                int cnt = 0;
                for (int d = 0; d < idx.Length; d++)
                {
                    if (int.Parse(idx[d]) != _CurrentSelectIndex[d])
                    {
                        break;
                    }
                    else
                    {
                        cnt++;
                    }
                }

                //完全一致，说明对应，可以删除
                if(cnt == idx.Length)
                {
                    ret = _CurrentData[j];
                    isCorrectCY = true;
                    //原则上 currentdata 数据也可以删除了
                    _CurrentData.RemoveAt(j);

                    break;
                }
                else if(cnt != idx.Length)
                {
                    //完全不同
                    if(cnt == 0)
                    {
                        //不用处理，和预设数据无关
                    }
                    else if(cnt < idx.Length)
                    {
                        //部分相同，如果同时构成字则需要删除
                        needDelIfInDb = true;
                    }
                }
            }
        }

        if(!isCorrectCY)
        {
            List<string> checkRet = HZManager.GetInstance().GetSHZByJGDEC(stCY);
            if (checkRet.Count != 0)
            {
                //需要删除被划乱的cdata
                if(needDelIfInDb)
                {
                    for (int j = _CurrentData.Count - 1; j >= 0; j--)
                    {
                        string[] idx = _CurrentData[j][(int)HZManager.eSHZCName.HZ_END].Split('#');
                        foreach(var index in _CurrentSelectIndex)/*事实上不可能存在重复的id，正常应该找到一个删除一个也，但_CurrentSelectIndex还要使用，多做几次无效循环*/
                        {
                            bool find = false;
                            foreach (var id in idx)
                            {
                                if(index == int.Parse(id))
                                {
                                    find = true;
                                    _CurrentData.RemoveAt(j);//从cdata中删除，因为有组件已经被选掉了
                                }
                            }

                            if (find) break;
                        }
                    }
                }

                ret = checkRet;
            }
        }

        return ret;
    }

    //检测序列是否连通
    public bool CheckIfConnect()
    {
        bool conn = true;

        for (int i = 0; i <= _CurrentSelectIndex.Count - 2; i++)
        {
            if (!CheckIfConnect(_CurrentSelectIndex[i], _CurrentSelectIndex[i + 1]))
            {
                conn = false;
                break;
            }
        }

        return conn;
    }

    public bool CheckIfConnect(int p1, int p2)
    {
        bool conn = false;

        int col1 = p1 % XT_HZ_MATRIX;
        int row1 = p1 / XT_HZ_MATRIX;

        int col2 = p2 % XT_HZ_MATRIX;
        int row2 = p2 / XT_HZ_MATRIX;

        if ((col1 == col2 && row1 - 1 == row2)
           || (col1 + 1 == col2 && row1 - 1 == row2)
          || (col1 + 1 == col2 && row1 == row2)
          || (col1 + 1 == col2 && row1 + 1 == row2)
          || (col1 == col2 && row1 + 1 == row2)
          || (col1 - 1 == col2 && row1 + 1 == row2)
          || (col1 - 1 == col2 && row1 == row2)
          || (col1 - 1 == col2 && row1 - 1 == row2)
          )
        {
            conn = true;
        }

        return conn;
    }

    //插入干扰字
    private void InsertDisturbHZ(string[] hzList)
    {

        List<int> searching = new List<int>();

        if (_DisturbCnt == 0) return;

        int blankCnt = XT_HZ_MATRIX * XT_HZ_MATRIX - _CurrentAllZJCnt;
        //已经填满了，不会出现，通用判断
        if (blankCnt == 0) return;


        //获取符合规则的汉字干扰
        List<string> otherHz = new List<string>();
        List<string> cys = new List<string>();
        //需要根据当前闯关数改变干扰字的成分，达到8000时，只会插入字根，即必须完全匹配答案才能过关

        int radicalCnt = _DisturbCnt * _ChuangGuan / 8000;
        int[,] LEVEL = { { 0,14}, {14,24}, {24,54 }, {54,78 }, { 78,100}, { 100,107} };
        for (int i = 0; i < radicalCnt;i++)
        {
            //字根有6个等级
            int level = (int)(5*(float)((3 + Define.RandGauss(0.0, 1.0)) / 6));//采用正态分布

            if (level < 0)
            {
                level = 0;
            }
            else if (level > 5)
            {
                level = 5;
            }

            //这里简单起见，直接记录字根等级索引范围
            otherHz.Add(HZManager.GetInstance().GetRadical(HZManager.GetInstance().GenerateRandomInt(LEVEL[level,0], LEVEL[level,1]))[1]);
        }

        int letfDisturbCnt = _DisturbCnt - radicalCnt;


        //获取letfDisturbCnt条,用于生成干扰字
        for (int i = 0; i < letfDisturbCnt; i++)
        {
            cys.Add(HZManager.GetInstance().GetSHZ()[(int)HZManager.eSHZCName.HZ_JGDEC]);
        }

        cys.Sort((a,b) =>
        {
            return b.Length - a.Length;//升序
        });

        //从cys选取符合规则的放入干扰字
        if (_DisturbDifficult == eDisturbDifficultType.EASY)
        {
            foreach (var cy in cys)
            {
                otherHz.Add("" + cy[HZManager.GetInstance().GenerateRandomInt(0, cy.Length)]);
            }
        }
        else if (_DisturbDifficult == eDisturbDifficultType.NORMAL)
        {
            foreach (var cy in cys)
            {
                bool addDone = false;
                if (cy.Length > 1)
                {
                    int half = cy.Length / 2;
                    int startIndex = HZManager.GetInstance().GenerateRandomInt(0, half);
                    string sbstr = cy.Substring(startIndex,half);

                    foreach(var ss in sbstr)
                    {
                        otherHz.Add(""+ss);
                        if(otherHz.Count == _DisturbCnt)
                        {
                            addDone = true;
                            break;
                        }
                    }
                }
                if (addDone) break;
            }

            if(otherHz.Count < _DisturbCnt)
            {
                //没有找满，此时最好是随机插入字根
                //插入等于1的
                foreach (var cy in cys)
                {
                    if (cy.Length == 1)
                    {
                        otherHz.Add(cy);
                        if (otherHz.Count == _DisturbCnt) break;
                    }
                }
            }
        }
        else if (_DisturbDifficult == eDisturbDifficultType.HARD)
        {
            foreach (var cy in cys)
            {
                bool addDone = false;
                if (cy.Length > 1)
                {
                    int startIndex = HZManager.GetInstance().GenerateRandomInt(0, 2);
                    string sbstr = cy.Substring(startIndex, cy.Length - 1);

                    foreach (var ss in sbstr)
                    {
                        otherHz.Add("" + ss);
                        if (otherHz.Count == _DisturbCnt)
                        {
                            addDone = true;
                            break;
                        }
                    }
                }
                if (addDone) break;
            }

            if (otherHz.Count < _DisturbCnt)
            {
                //没有找满，此时最好是随机插入字根
                //插入等于1的
                foreach (var cy in cys)
                {
                    if (cy.Length == 1)
                    {
                        otherHz.Add(cy);
                        if (otherHz.Count == _DisturbCnt) break;
                    }
                }
            }
        }
        else
        {
            ShowToast("抱歉出错了，请返回主界面再回来～");
            return;
        }


        // 洗牌
        ShuffleSJList(otherHz);

        //需要根据当前的干扰字个数插入
        for (int i = 0; i < hzList.Length; i++)
        {
            if (hzList[i] == "")
            {
                searching.Add(i);
            }
        }
        
        //优选搜索周围空白的字最多的字
        List<SD> sds = new List<SD>();
        for (int i = 0; i < searching.Count; i++)
        {
            SD sd;
            sd.index = searching[i];
            sd.priority = GetHZPriority(hzList, searching[i]);
            sds.Add(sd);
        }

        sds.Sort((a, b) =>
        {
            if(b.priority > a.priority)
            {
                return 1;
            }
            else if (b.priority < a.priority)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        });

        searching.Clear();
        foreach (var sd in sds)
        {
            searching.Add(sd.index);
        }

        ////由于不好处理 干扰字 放到不连通的位
        /// 所以直接全部优先从最难的字开始放置
        /*
        int sindex = (int)(_DisturbCntSearchDifficult * searching.Count);//0=< 1-_SearchDifficult <1
        if (sindex >= searching.Count - 1) sindex = searching.Count - 1;

        List<int> searching1 = new List<int>();
        List<int> searching2 = new List<int>();

        for (int i = 0; i < sindex; i++)
        {
            searching1.Add(searching[i]);
        }

        for (int i = sindex; i < searching.Count; i++)
        {
            searching2.Add(searching[i]);
        }

        searching.Clear();
        searching.AddRange(searching2);// 优先往难的方向搜索
        searching1.Reverse();
        searching.AddRange(searching1);//再往越来越简单的方向搜索
*/

        int cnt = 0;
        for (int i = 0; i < searching.Count; i++)
        {
            if (cnt >= _DisturbCnt) break;
            hzList[searching[i]] = otherHz[cnt];
            cnt++;
        }

        return;
    }

#endregion
}
