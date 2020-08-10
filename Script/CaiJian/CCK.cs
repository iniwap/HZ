/*
 * 划一划
*/

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

using HanZi;

public class CCK : TiMu
{
    public override void Start()
    {

    }

    public override void OnEnable()
    {
        _CanExist = true;//任何时刻均可退出
        CheckBuyState();
    }

    public GameObject _MTXJObj;//
    public Button _NextBtn;
    public Button _buyMTXJBtn;
    protected override void CheckNotCommonBuyState()
    {
        //猜一猜 需要实现该接口
        _buyMTXJBtn.gameObject.SetActive(!IAP.getHasBuy(IAP.IAP_MDXJ));
    }

    protected override void OnBuyNotCommonCallback(string inAppID)
    {
        //猜一猜 需要实现该接口
        if (inAppID == IAP.IAP_MDXJ)
        {
            //如果lock不可见，不必执行动画
            if (_buyMTXJBtn.gameObject.activeSelf)
            {
                DoUnLockAni(_buyMTXJBtn.gameObject, () =>
                {
                    _buyMTXJBtn.gameObject.SetActive(false);
                    CheckNotCommonBuyState();
                });
            }
        }
    }

    public override void OnChangeZTTimeType(Game.ZTTimeType zzt)
    {

    }

    public override void InitTiMu(Action cb)
    {
        _ChuangGuan = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.CCK_FINISH_CNT, 0);
        if (_ChuangGuan >= HZManager.GetInstance().GetSZMCnt())
        {
            _ChuangGuan = HZManager.GetInstance().GenerateRandomInt(0, HZManager.GetInstance().GetSZMCnt());
        }

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


    private List<string> _CurrentData = new List<string>();

    private List<int> _CurrentIndex = new List<int>();// 主要用于答案显示
    private int _CurrentSelectCYIndex = -1;// 主要用于记录当前划选的字
    private int _CurrentSelectedCnt = 0;//当前已经划上去的字数

    private int XT_HZ_MATRIX = Define.MAX_XT_HZ_MATRIX;

    private void InitTiMu(bool first, Action cb = null)
    {
        ///////////////////数据初始化//////////////////////
        List<string> hzList = InitCY();
        // 初始化界面
        InitUI();

        ////////////////将诗句汉字按照一定的搜索顺序插入到方阵中///////////////////////
        //支持8个方向，需要记录这个列表序列 用于答案提示，可能会遇到汉字相同，但是路径不同的情况
        int currentIndex = HZManager.GetInstance().GenerateRandomInt(0, XT_HZ_MATRIX * XT_HZ_MATRIX);
        int col = currentIndex % XT_HZ_MATRIX;
        int row = currentIndex / XT_HZ_MATRIX;
        //生成字序列 -- 汉字方阵 根据诗句长度动态变化大小
        float wh = GetGridSizeWH();
        int fontSize = _HZPrefab.GetComponentInChildren<Text>().fontSize;
        float s = wh / _HZPrefab.GetComponent<RectTransform>().rect.width;
        GridLayoutGroup gl = _HZContent.GetComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(wh, wh);
        for (int i = 0; i < hzList.Count; i++)
        {
            //顶部汉字
            GameObject Hz = Instantiate(_HZPrefab, _HZContent) as GameObject;
            Hz.SetActive(true);

            XTHZ xtHZ = Hz.GetComponent<XTHZ>();
            xtHZ.InitHZ(i, hzList[i]);
            _HZList.Add(Hz);

            xtHZ.DisableIDTag();

            RectTransform hzrt = Hz.GetComponent<RectTransform>();
            hzrt.sizeDelta = new Vector2(wh, wh);

            BoxCollider bc = Hz.GetComponent<BoxCollider>();
            bc.size = new Vector3(wh / 2, wh / 2, 1);

            Hz.GetComponentInChildren<Text>().fontSize = (int)(s * fontSize);

            //该处无字，不应该存在这种情况
            if (hzList[i] == "")
            {
                bc.enabled = false;//禁用碰撞检测
            }
        }

        if (!first)
        {
            OnZuoTiTimer(true);
            ShowGameRule();
        }
        else
        {
            ShowGameRule(0);
        }

        if (cb != null)
        {
            cb();
        }
    }

    public Transform _MiTiTransform;
    public GameObject _NanDu;
    public Text _FinishCntText;
    public GameObject _FindJgInfo;
    private List<GameObject> _ResultJG = new List<GameObject>();//结果构件列表
    public GameObject _ResultJGPrefabs;
    public Transform _ResultJGTranform;
    public Text _ResultAniText;
    private void InitUI()
    {
        DestroyObj(_ResultJG);
        DestroyObj(_HZList);
        _TipBrush.gameObject.SetActive(false);
        DisableBottomBtn(false);
        //只要新生成题目，分数计时一定是0
        _CurrentSelectedCnt = 0;
        _CurrentSelectCYIndex = -1;
        //ShowGameRule();
        _CurrentDZTSCnt = 0;

        //隐藏全部rst
        _RstInfo.gameObject.SetActive(false);
        _ResultJGInfo.SetActive(false);

        _MTXJObj.SetActive(false);
        _NextBtn.gameObject.SetActive(false);


        DestroyObj(_tmpRstTextList);

        //--------------初始化划选构件结果列表----------------------------

        InitResultJG();
        _FindJgInfo.SetActive(true);
        Text[] fjit = _FindJgInfo.GetComponentsInChildren<Text>();
        Image[] fjii = _FindJgInfo.GetComponentsInChildren<Image>();
        for (int i = 0; i < 4; i++)
        {
            fjit[i].color = new Color(fjit[i].color.r, fjit[i].color.g, fjit[i].color.b, 200 / 255f);
            fjii[i].color = new Color(fjii[i].color.r, fjii[i].color.g, fjii[i].color.b, 1.0f);
        }

        //如果没有格子总数发生变化，是不需要每次调用的，事实上这里是固定的
        InitMatrixSP();

        //设置谜题相关信息
        FitDialogContentText(_MiTiTransform, _CurrentData[(int)HZManager.eSZMCName.HZ_MITI], _miHZPrafab, 1);

        //设置难度
        int nd = int.Parse(_CurrentData[(int)HZManager.eSZMCName.HZ_NANDU]);
        int star1 = nd / 2;
        int star0 = 5 - star1;
        int halfstar = nd % 2;
        for (int s = 0; s < 5; s++)
        {
            GameObject objStar0 = _NanDu.transform.Find("Star0" + (s + 1)).gameObject;
            objStar0.gameObject.SetActive(false);

            GameObject objStar1 = _NanDu.transform.Find("Star1" + (s + 1)).gameObject;
            objStar1.gameObject.SetActive(false);

            GameObject objHalfStar = _NanDu.transform.Find("HalfStar" + (s + 1)).gameObject;
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

        _FinishCntText.text = "完成度 " + Setting.getPlayerPrefs("" + Setting.SETTING_KEY.CCK_FINISH_CNT, 0)
            + "/" + HZManager.GetInstance().GetSZMCnt();
    }

    private void InitResultJG()
    {
        string jgdec = _CurrentData[(int)HZManager.eSZMCName.HZ_END];
        for (int i = 0; i < jgdec.Length; i++)
        {
            GameObject rstjg = Instantiate(_ResultJGPrefabs, _ResultJGTranform) as GameObject;
            rstjg.SetActive(true);
            _ResultJG.Add(rstjg);

            Text t = rstjg.GetComponentInChildren<Text>();
            t.text = "";
            t.color = new Color(t.color.r, t.color.g, t.color.b, 0.0f);

            Image img = rstjg.GetComponentInChildren<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.0f);

            Button btn = rstjg.GetComponent<Button>();
            btn.interactable = false;

            rstjg.name = "" + i;
            btn.onClick.AddListener(delegate () {
                this.OnResultCYBtnClick(rstjg);
            });

        }

        //需要设置大小
        GridLayoutGroup lyt = _ResultJGTranform.GetComponent<GridLayoutGroup>();
        RectTransform rt = _ResultJGTranform.GetComponent<RectTransform>();
        RectTransform jgrt = _ResultJGPrefabs.GetComponent<RectTransform>();

        float cs = (rt.rect.width - (jgdec.Length + 1) * lyt.spacing.x) / jgdec.Length;
        cs = cs > jgrt.rect.width ? jgrt.rect.width : cs;
        lyt.cellSize = new Vector2(cs, cs);
    }

    private float GetResultJGCenterX()
    {
        float x = 0;

        int cnt = _CurrentIndex.Count;

        //0         0.5     1    1.5     2    2.5   
        // 1-> -0.5,2->0 ,3->0.5,4->1,5->1.5,6->2
        RectTransform jgrt = _ResultJG[0].GetComponent<RectTransform>();
        GridLayoutGroup lyt = _ResultJGTranform.GetComponent<GridLayoutGroup>();

        x = _ResultJG[0].transform.position.x - jgrt.rect.width / 2 + (jgrt.rect.width * cnt / 2 + lyt.spacing.x * (cnt - 1) / 2);

        return x;
    }

    //获取所有划选构件汉字
    public List<Text> GetAllResultCYText()
    {
        List<Text> ret = new List<Text>();

        for (int i = 0; i < _ResultJG.Count; i++)
        {
            ret.Add(_ResultJG[i].GetComponentInChildren<Text>());
        }

        return ret;
    }

    //获取所有划选到构件字符串
    public string GetResultJG()
    {
        string ret = "";

        for (int i = 0; i < _ResultJG.Count; i++)
        {
            Text cy = _ResultJG[i].GetComponentInChildren<Text>();
            ret += cy.text;
        }

        return ret;
    }
    //找一个空位置
    public int GetEmptyResultCY()
    {
        int ret = -1;

        for (int i = 0; i < _ResultJG.Count; i++)
        {
            Text cy = _ResultJG[i].GetComponentInChildren<Text>();
            if (cy.text == "")
            {
                ret = i;
                break;
            }
        }

        return ret;
    }
    public void OnResultCYBtnClick(GameObject btn)
    {
        int index = int.Parse(btn.name);

        Text cy = _ResultJG[index].GetComponentInChildren<Text>();

        if (cy.text != "" && _CurrentSelectedCnt != 0)
        {
            BackOne(index);
        }
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

    private List<string> InitCY()
    {
        XT_HZ_MATRIX = 5;

        List<string> allJG = new List<string>();

        _CurrentData.Clear();
        _CurrentIndex.Clear();

        // 设置格数以及插入干扰字数量
        int fcnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.CCK_FINISH_CNT, 0);

        //获取当前进度的谜语 
        _CurrentData.AddRange(HZManager.GetInstance().GetSZM(_ChuangGuan));

        List<string> HZ = HZManager.GetInstance().GetSHZByHZ(_CurrentData[(int)HZManager.eSZMCName.HZ_MIDI]);

        string jgdec = HZ[(int)HZManager.eSHZCName.HZ_JGDEC];

        _CurrentData.Add(jgdec);

        if (fcnt >= HZManager.GetInstance().GetSZMCnt())
        {
            //随机模式采用6格模式
            XT_HZ_MATRIX = 6;
            allJG = GetAllHZ(jgdec);//当前是随机模式，不再使用固定随机插入
        }
        else
        {
            //需要使用之前已经生成的固定插入
            string fixedHZ = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.CCK_FIXED_HZ, "");
            if (fixedHZ == "")
            {
                allJG = GetAllHZ(jgdec);
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.CCK_FIXED_HZ, String.Join("#", allJG.ToArray()));
            }
            else
            {
                allJG.AddRange(fixedHZ.Split('#'));
            }
        }

        //由于单道题有多个谜题，需要选择一个
        string[] tm = _CurrentData[(int)HZManager.eSZMCName.HZ_MITI].Split('#');
        string[] js = _CurrentData[(int)HZManager.eSZMCName.HZ_JIESHI].Split('#');

        if (tm.Length != js.Length)
        {
            _CurrentData[(int)HZManager.eSZMCName.HZ_MITI] = tm[0];
            _CurrentData[(int)HZManager.eSZMCName.HZ_JIESHI] = js[0];
        }
        else
        {
            //自由选题的时候才随机题目，否则只用第一题
            if (fcnt >= HZManager.GetInstance().GetSZMCnt())
            {
                int idx = HZManager.GetInstance().GenerateRandomInt(0, tm.Length);
                _CurrentData[(int)HZManager.eSZMCName.HZ_MITI] = tm[idx];
                _CurrentData[(int)HZManager.eSZMCName.HZ_JIESHI] = js[idx];
            }
            else
            {
                _CurrentData[(int)HZManager.eSZMCName.HZ_MITI] = tm[0];
                _CurrentData[(int)HZManager.eSZMCName.HZ_JIESHI] = js[0];
            }
        }

        //未填满的位置插入空字
        for (int i = 0; i < XT_HZ_MATRIX * XT_HZ_MATRIX - allJG.Count; i++)
        {
            allJG.Add("");
        }

        //洗牌汉字列表
        ShuffleSJList(allJG);


        //查找谜底拆分所在的位置放入cindex
        List<string> tmp = new List<string>();
        //这里有重复的字的时候有问题，需要找到不同的位置的
        foreach (var md in jgdec)
        {
            for (int i = 0; i < allJG.Count; i++)
            {
                if ("" + md == allJG[i])
                {
                    bool findext = false;
                    foreach (var ext in tmp)
                    {
                        if (i + "" + md == ext)
                        {
                            findext = true;
                            break;
                        }
                    }

                    if (!findext)
                    {
                        _CurrentIndex.Add(i);
                        tmp.Add(i + "" + md);
                        break;
                    }
                }
            }
        }

        return allJG;
    }

    private List<string> GetAllHZ(string jgdec)
    {
        List<string> allHZ = new List<string>();

        int cnt = _ChuangGuan;
        if (cnt + jgdec.Length > XT_HZ_MATRIX * XT_HZ_MATRIX)
        {
            cnt = XT_HZ_MATRIX * XT_HZ_MATRIX - jgdec.Length;
        }

        foreach (var jg in jgdec)
        {
            allHZ.Add("" + jg);
        }

        List<string> cys = new List<string>();
        //取
        for (int i = 0; i < cnt; i++)
        {
            cys.Add(HZManager.GetInstance().GetSHZ()[(int)HZManager.eSHZCName.HZ_JGDEC]);
        }

        for (int i = 0; i < cnt; i++)
        {
            bool endAdd = false;
            foreach (var jg in cys[i])
            {
                if (allHZ.Count >= cnt + jgdec.Length)
                {
                    endAdd = true;
                    break;
                }

                allHZ.Add("" + jg);
            }

            if (endAdd)
            {
                break;
            }
        }

        return allHZ;
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

    //划选结束
    public void OnPencilSubmit()
    {
        OnPencilSubmit(false);
    }
    private void OnPencilSubmit(bool timeout)
    {
        //有选择汉字，每次只能选择一个
        if (_CurrentSelectCYIndex != -1)
        {
            _CurrentSelectedCnt++;

            if (_CurrentSelectedCnt > _CurrentIndex.Count)
            {

                XTHZ xTHZ0 = _HZList[_CurrentSelectCYIndex].GetComponentInChildren<XTHZ>();
                xTHZ0.SetIsSelect(false, false);
                xTHZ0.ResetHZ();
                _CurrentSelectedCnt = _CurrentIndex.Count;
                _CurrentSelectCYIndex = -1;
                ShowToast("已经选满了构件，请删除或清空后再选～");
                return;
            }

            DisableAllGameBtnWhenAni(true);
            //未选完，执行单个字的运动动画
            Sequence resultAni = DOTween.Sequence();

            //第一个字点击时，隐藏“猜一构件”
            //这个应该选从0开始第一个空的，而不是cnt位置的
            if (_CurrentSelectedCnt == 1 && _FindJgInfo.activeSelf)
            {
                Sequence hideAni = DOTween.Sequence();
                Text[] fjit = _FindJgInfo.GetComponentsInChildren<Text>();
                Image[] fjii = _FindJgInfo.GetComponentsInChildren<Image>();

                for (int i = 0; i < 4; i++)
                {
                    hideAni.AppendInterval(0.05f * (i + 1));
                    hideAni.Join(fjit[i].DOFade(0.0f, 0.2f))
                           .Join(fjii[i].DOFade(0.0f, 0.2f));
                }
                hideAni.OnComplete(() => {
                    _FindJgInfo.SetActive(false);

                    for (int i = 0; i < _ResultJG.Count; i++)
                    {
                        _ResultJG[i].GetComponent<Image>().DOFade(1.0f, 0.2f);
                    }
                });

                resultAni.Join(hideAni);
            }

            int blankID = GetEmptyResultCY();
            Text cy = _ResultJG[blankID].GetComponentInChildren<Text>();

            XTHZ xTHZ = _HZList[_CurrentSelectCYIndex].GetComponentInChildren<XTHZ>();
            xTHZ.GetComponent<BoxCollider>().enabled = false;//禁用碰撞检测
            xTHZ.ResetHZ();//先重置掉动画，防止颜色变化
            xTHZ.SetCantRestFinded(true);//不能被重置
            float s = cy.rectTransform.rect.width / xTHZ.GetHZText().rectTransform.rect.width;

            Sequence resultAni2 = DOTween.Sequence();
            resultAni2.AppendInterval(0.5f)
                      .Join(xTHZ.DoHZColor(Define.YELLOW, 0.2f))
                      .Join(xTHZ.DoHZIDFade(0.0f, 0.2f))
                      .Join(xTHZ.DoHZIDBGFade(0.0f, 0.2f))
                      .Append(xTHZ.GetHZText().transform.DOMove(cy.transform.position, 0.4f))
                      .Join(xTHZ.GetHZText().transform.DOScale(s, 0.4f))
                      .SetEase(Ease.OutSine);

            resultAni.Join(resultAni2);

            resultAni.OnComplete(() =>
            {
                Sequence endAni = DOTween.Sequence();
                XTHZ xTHZ2 = _HZList[_CurrentSelectCYIndex].GetComponentInChildren<XTHZ>();
                cy.text = xTHZ2.GetHZText().text;
                cy.name = "" + xTHZ2.GetHZID();//设置id，方便后退
                endAni.Append(xTHZ2.DoHZFade(0f, 0.2f))
                      .Join(cy.DOFade(200 / 255.0f, 0.2f));


                endAni.OnComplete(() => {

                    XTHZ xTHZ3 = _HZList[_CurrentSelectCYIndex].GetComponentInChildren<XTHZ>();
                    xTHZ3.GetHZText().text = "";

                    if (_CurrentSelectedCnt == _CurrentIndex.Count)
                    {
                        //执行判断是否是正确
                        //正确则结束
                        //无须顺序一致
                        string oriJG = _CurrentData[(int)HZManager.eSZMCName.HZ_END];
                        string rstJG = GetResultJG();
                        List<char> ojg = new List<char>();
                        ojg.AddRange(oriJG.ToCharArray());
                        ojg.Sort();
                        List<char> rjg = new List<char>();
                        rjg.AddRange(rstJG.ToCharArray());
                        rjg.Sort();
                        oriJG = "";
                        rstJG = "";
                        foreach (var o in ojg) oriJG += "" + o;
                        foreach (var r in rjg) rstJG += "" + r;

                        if (oriJG.Equals(rstJG))
                        {
                            UpdateScore();//更新分数

                            Sequence makeHZAni = DOTween.Sequence();
                            //首先要执行构件合成字的动画，
                            for (int i = 0; i < _CurrentSelectedCnt; i++)
                            {
                                Text t = _ResultJG[i].GetComponentInChildren<Text>();
                                XTHZ xTHZ4 = _HZList[int.Parse(t.name)].GetComponentInChildren<XTHZ>();
                                Text thz = xTHZ4.GetHZText();
                                thz.text = t.text;
                                thz.color = new Color(thz.color.r, thz.color.g, thz.color.b, 0.0f);

                                makeHZAni.Join(t.DOFade(0.0f, 0.3f))
                                         .Join(thz.DOFade(200 / 255.0f, 0.3f));
                            }

                            makeHZAni.AppendInterval(0.01f);

                            float x = GetResultJGCenterX();
                            for (int i = 0; i < _CurrentSelectedCnt; i++)
                            {
                                Text t = _ResultJG[i].GetComponentInChildren<Text>();
                                XTHZ xTHZ4 = _HZList[int.Parse(t.name)].GetComponentInChildren<XTHZ>();
                                Text thz = xTHZ4.GetHZText();
                                makeHZAni.Join(thz.DOFade(0.0f, 0.5f))
                                         .Join(thz.transform.DOMoveX(x, 0.5f));
                            }

                            _ResultAniText.transform.position = new Vector3(x, _ResultJG[0].transform.position.y, _ResultJG[0].transform.position.z);
                            _ResultAniText.gameObject.SetActive(true);
                            _ResultAniText.transform.localScale = Vector3.zero;
                            _ResultAniText.color = new Color(_ResultAniText.color.r, _ResultAniText.color.g, _ResultAniText.color.b, 0.0f);
                            _ResultAniText.text = _CurrentData[(int)HZManager.eSZMCName.HZ_MIDI];
                            makeHZAni.Append(_ResultAniText.transform.DOScale(1.0f, 0.5f))
                                     .Join(_ResultAniText.DOFade(1.0f, 0.5f))
                                     .SetEase(Ease.OutBounce);

                            makeHZAni.OnComplete(DoResultCYAni);

                        }
                        else
                        {
                            //不正确，提示不正确
                            ShowToast("回答不正确，请删除后重新选择哦～");
                            Sequence errorAni = DOTween.Sequence();
                            for (int i = 0; i < _ResultJG.Count; i++)
                            {
                                Image[] error = _ResultJG[i].GetComponentsInChildren<Image>(true);
                                foreach (var img in error)
                                {
                                    if (img.name == "error")
                                    {
                                        img.gameObject.SetActive(true);
                                        img.transform.localScale = new Vector3(0f, 0f, 0f);
                                        img.color = new Color(img.color.r, img.color.g, img.color.b, 0.0f);
                                        errorAni.Join(img.DOFade(1.0f, 1.0f))
                                                .Join(img.transform.DOScale(1.0f, 1.0f))
                                                .SetEase(Ease.OutBounce);
                                    }
                                }
                            }
                            errorAni.AppendInterval(0.5f);
                            errorAni.OnComplete(() => {
                                _CurrentSelectCYIndex = -1;
                                DisableAllGameBtnWhenAni(false);

                                for (int i = 0; i < _ResultJG.Count; i++)
                                {
                                    Image[] error = _ResultJG[i].GetComponentsInChildren<Image>(true);
                                    foreach (var img in error)
                                    {
                                        if (img.name == "error")
                                        {
                                            img.gameObject.SetActive(false);
                                        }
                                    }
                                }
                            });
                        }
                    }
                    else
                    {
                        _CurrentSelectCYIndex = -1;
                        DisableAllGameBtnWhenAni(false);
                        //按钮可以点击
                        _ResultJG[blankID].GetComponent<Button>().interactable = true;
                    }
                });
            });
        }
    }
    //铅笔选中汉字 - 保存选中列表，有次序
    //单字提交，原则上没有取消选中的动作
    public void OnPencilSelect(bool isSelect, int index)
    {
        if (index >= 0 && index < _HZList.Count)
        {
            XTHZ xtHZ = _HZList[index].GetComponent<XTHZ>();
            //如果是选
            if (isSelect)
            {
                if (_CurrentSelectCYIndex == -1)
                {
                    _CurrentSelectCYIndex = index;
                    xtHZ.SetIsSelect(true);
                }
                else
                {
                    //删除之前所有的选中

                    XTHZ xtHZ2 = _HZList[_CurrentSelectCYIndex].GetComponent<XTHZ>();
                    xtHZ2.SetIsSelect(false);

                    _CurrentSelectCYIndex = index;
                    xtHZ.SetIsSelect(true);
                }
            }
            else
            {
                xtHZ.SetIsSelect(false);
                _CurrentSelectCYIndex = -1;
            }
        }
    }

    private enum eGameTips
    {
        TOAST_TIPS,
        MAIN_INFO,
    }
    public Text _MainInfo;//主要提示，规则等关键信息提示
    private void DoTipInfoAni(Text infoText, string info)
    {
        infoText.DOFade(0.0f, 0.5f)
        .OnComplete(() =>
        {
            infoText.text = info;

            infoText.DOFade(200 / 255f, 0.5f);
        });
    }

    private string[] GameRule = {
            "贴士：点选下方构件，组成汉字，完成解谜。",
            "贴士：当做完所有题目时，进入随机选题模式，采用6阶，36格。",
            "贴士：解谜数越多，猜一猜榜单越靠前，包括重复题目。",
            "贴士：随着解谜数的增加，插入构件逐渐增加至满。",
            "贴士：锻炼思维能力，不一定非要从解谜的角度解谜哟。",
            "贴士：谜底详解是一个非常不错的扩展汉字知识的功能。",
            "贴士：清空、删除以及点击已选构件均可取消选中。",
            "贴士：任何问题、建议、反馈均可在帮助界面联系我们。",
            "贴士：猜字模式更能加强大家对于汉字构字的认知。"
        };

    //仅在过关时切换
    private void ShowGameRule(int index = -1)
    {
        //除此之外，需要在特定对关卡，需要显示特定信息，主要包括
        //成就、划选数增加、干扰字插入等
        if (index != -1)
        {
            ShowGameTips(eGameTips.MAIN_INFO, GameRule[index]);
        }
        else
        {
            ShowGameTips(eGameTips.MAIN_INFO, GameRule[HZManager.GetInstance().GenerateRandomInt(0, GameRule.Length)]);
        }
    }

    private void ShowGameTips(eGameTips type, string info)
    {
        if (type == eGameTips.MAIN_INFO)
        {
            DoTipInfoAni(_MainInfo, info);
        }
        else if (type == eGameTips.TOAST_TIPS)
        {
            ShowToast(info);
        }
    }

    public override void OnZuoTiTimer(bool start)
    {
        if (!start)
        {
            return;
        }

        Sequence resultAni = DOTween.Sequence();
        for (int i = 0; i < _HZList.Count; i++)
        {
            resultAni.Join(_HZList[i].GetComponentInChildren<Text>().DOFade(1.0f, 0.5f));
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
            _DisableBottom.DOFade(100 / 255.0f, 0.5f);
        }
        else
        {
            //隐藏
            _DisableBottom.DOFade(0.0f, 0.5f).OnComplete(() => _DisableBottom.gameObject.SetActive(false));
        }
    }

    //禁用所有按钮，当动画执行当时候，后续可以优化此处
    public void DisableAllGameBtnWhenAni(bool dis)
    {
        SetCanShowPencil(!dis);
        DisableBottomBtn(dis);
        for (int i = 0; i < _ResultJG.Count; i++)
        {
            _ResultJG[i].GetComponent<Button>().interactable = !dis;
        }
    }

    public void DoResultCYAni()
    {
        //DestroyObj(_HZList);
        //禁用下方的按钮
        //正确，
        //next
        _NextBtn.gameObject.SetActive(true);
        _MTXJObj.SetActive(true);
        _NextBtn.GetComponent<ScaleAni>().enabled = false;
        _NextBtn.interactable = false;
        _NextBtn.transform.localScale = Vector3.zero;
        _MTXJObj.transform.localScale = Vector3.zero;
        _RstInfo.gameObject.SetActive(true);

        string findHZ = _ResultAniText.text;

        List<string> hz = HZManager.GetInstance().GetSHZByHZ(findHZ);
        string findFY = hz[(int)HZManager.eSHZCName.HZ_PINYIN];
        string findJs = hz[(int)HZManager.eSHZCName.HZ_JIESHI];
        string findEn = hz[(int)HZManager.eSHZCName.HZ_ENGLISH];
        string findJG = hz[(int)HZManager.eSHZCName.HZ_JGDEC];
        int findND = int.Parse(_CurrentData[(int)HZManager.eSZMCName.HZ_NANDU]);


        //所有划选结果汉字
        List<Text> jss = new List<Text>();//解释
        //其他还有米字格，难度星，tags，分割线
        //搜索所有节点
        _ResultJGInfo.SetActive(true);

        //汉字
        Text hzText = _ResultJGInfo.transform.Find("HZ/HZ/HZ").GetComponent<Text>();
        hzText.text = findHZ;
        hzText.color = new Color(hzText.color.r, hzText.color.g, hzText.color.b, 0.0f);

        //拼音
        string[] tpys = findFY.Split('#');
        Text pyText = _ResultJGInfo.transform.Find("HZ/PY").GetComponent<Text>();
        pyText.text = tpys[0];
        pyText.color = new Color(pyText.color.r, pyText.color.g, pyText.color.b, 0.0f);

        Text morePy = _ResultJGInfo.transform.Find("HZ/MorePY").GetComponent<Text>();
        morePy.text = "";
        morePy.color = new Color(morePy.color.r, morePy.color.g, morePy.color.b, 0.0f);
        if (tpys.Length == 1)
        {
            morePy.gameObject.SetActive(false);
        }
        else
        {
            morePy.gameObject.SetActive(true);
            for (int j = 1; j < tpys.Length; j++)
            {
                if (j != tpys.Length - 1)
                {
                    morePy.text += tpys[j] + "、";
                }
                else
                {
                    morePy.text += tpys[j];
                }
            }
        }

        //难度
        int star1 = findND / 2;
        int star0 = 5 - star1;
        int halfstar = findND % 2;
        for (int s = 0; s < 5; s++)
        {
            GameObject objStar0 = _ResultJGInfo.transform.Find("HZ/ND/NanDu/Star0" + (s + 1)).gameObject;
            objStar0.gameObject.SetActive(false);

            GameObject objStar1 = _ResultJGInfo.transform.Find("HZ/ND/NanDu/Star1" + (s + 1)).gameObject;
            objStar1.gameObject.SetActive(false);

            GameObject objHalfStar = _ResultJGInfo.transform.Find("HZ/ND/NanDu/HalfStar" + (s + 1)).gameObject;
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
        Text jgText = _ResultJGInfo.transform.Find("HZ/JG").GetComponent<Text>();
        jgText.text = findJG;
        jgText.color = new Color(jgText.color.r, jgText.color.g, jgText.color.b, 0.0f);

        //填充解释并获取
        Transform jieshi = _ResultJGInfo.transform.Find("JieShi");
        FitDialogContentText(jieshi, findJs.Replace('#', ';'), _resultHZPrafab);
        jss.AddRange(jieshi.GetComponentsInChildren<Text>());
        foreach (var tjs in jss)
        {
            tjs.color = new Color(tjs.color.r, tjs.color.g, tjs.color.b, 0.0f);
        }

        //英文
        Text enText = _ResultJGInfo.transform.Find("En").GetComponent<Text>();
        enText.text = findEn.Replace('#', ',');
        enText.color = new Color(enText.color.r, enText.color.g, enText.color.b, 0.0f);
        if (enText.text == "N/A")
        {
            enText.gameObject.SetActive(false);
        }
        else
        {
            enText.gameObject.SetActive(true);
        }



        _RstInfo.color = new Color(_RstInfo.color.r, _RstInfo.color.g, _RstInfo.color.b, 0.0f);

        Image[] rstInfoImg = _ResultJGInfo.GetComponentsInChildren<Image>();
        for (int j = 0; j < rstInfoImg.Length; j++)
        {
            //Tag,TagText,Image,SP,Bg,Line
            if (rstInfoImg[j].name == "Tag"
               || rstInfoImg[j].name == "Image"
              || rstInfoImg[j].name == "SP"
              || rstInfoImg[j].name == "Bg"
               || rstInfoImg[j].name == "Line")
            {
                rstInfoImg[j].color = new Color(rstInfoImg[j].color.r, rstInfoImg[j].color.g, rstInfoImg[j].color.b, 0.0f);
            }
        }

        Text[] rstInfoText = _ResultJGInfo.GetComponentsInChildren<Text>();
        for (int j = 0; j < rstInfoText.Length; j++)
        {
            if (rstInfoText[j].name == "TagText")
            {
                rstInfoText[j].color = new Color(rstInfoText[j].color.r, rstInfoText[j].color.g, rstInfoText[j].color.b, 0.0f);
            }
        }

        //延时一帧执行，否则获取到的位置不正确
        Sequence delayAni = DOTween.Sequence();
        delayAni.AppendInterval(0.1f).OnComplete(() => {

            Sequence nextAni = DOTween.Sequence();

            //所有干扰字需要隐藏
            for (int i = 0; i < _HZList.Count; i++)
            {
                XTHZ xTHZ = _HZList[i].GetComponentInChildren<XTHZ>();
                if (xTHZ.GetHZ() != "")
                {
                    //干扰字
                    nextAni.Join(xTHZ.DoHZFade(0, 0.3f))
                            .Join(xTHZ.DoHZFindFade(0.0f, 0.3f))
                            .Join(xTHZ.DoHZFindBgFade(0.0f, 0.3f));
                }
            }

            nextAni.Join(_RstInfo.DOFade(200 / 255.0f, 0.3f));


            nextAni.AppendInterval(0.01f);

            Sequence cyAni = DOTween.Sequence();

            cyAni.Append(_ResultAniText.transform.DOMove(hzText.transform.position, 0.5f));

            for (int j = 0; j < rstInfoImg.Length; j++)
            {
                //Tag,TagText,Image,SP,Bg,Line
                if (rstInfoImg[j].name == "Tag")
                {
                    cyAni.Join(rstInfoImg[j].DOFade(10 / 255.0f, 0.5f));
                }
                else if (rstInfoImg[j].name == "Image")
                {
                    cyAni.Join(rstInfoImg[j].DOFade(200 / 255.0f, 0.5f));
                }
                else if (rstInfoImg[j].name == "SP")
                {
                    cyAni.Join(rstInfoImg[j].DOFade(50 / 255.0f, 0.5f));
                }
                else if (rstInfoImg[j].name == "Bg")
                {
                    cyAni.Join(rstInfoImg[j].DOFade(1.0f, 0.5f));
                }
                else if (rstInfoImg[j].name == "Line")
                {
                    cyAni.Join(rstInfoImg[j].DOFade(127 / 255.0f, 0.5f));
                }
            }

            for (int j = 0; j < rstInfoText.Length; j++)
            {
                if (rstInfoText[j].name == "TagText")
                {
                    cyAni.Join(rstInfoText[j].DOFade(100 / 255.0f, 0.5f));
                }
            }

            cyAni.Append(_ResultAniText.DOFade(0.0f, 0.3f))
                 .Join(hzText.DOFade(1.0f, 0.3f));

            cyAni.Append(jgText.DOFade(1.0f, 0.3f));

            cyAni.Append(pyText.DOFade(200 / 255f, 0.3f))
                 .Join(morePy.DOFade(1.0f, 0.3f));


            Sequence jsAni = DOTween.Sequence();
            for (int j = 0; j < jss.Count; j++)
            {
                jsAni.Join(jss[j].DOFade(200 / 255f, 0.3f));
            }

            cyAni.Append(jsAni);

            cyAni.Append(enText.DOFade(200 / 255f, 0.2f));

            nextAni.Join(cyAni);

            nextAni.Append(_MTXJObj.transform.DOScale(1.0f, 0.5f))
                   .Append(_NextBtn.transform.DOScale(1.0f, 0.5f))
                   .OnComplete(() => {
                       _NextBtn.GetComponent<ScaleAni>().enabled = true;
                       _NextBtn.interactable = true;
                   });
        });
    }
    //结束时必须删除 
    public Image _RstInfo;
    public GameObject _ResultJGInfo;
    public GameObject _miHZPrafab;
    public GameObject _resultHZPrafab;
    private List<GameObject> _tmpRstTextList = new List<GameObject>();
    private void FitDialogContentText(Transform content, string str, GameObject hzPrafab, int fixRow = 0)
    {
        List<string> jsstr = new List<string>();
        foreach (var s in str)
        {
            jsstr.Add("" + s);
        }

        float hzSize = hzPrafab.GetComponentInChildren<Text>().transform.GetComponent<RectTransform>().rect.width;

        RectTransform jsrt = content.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = content.GetComponentInChildren<GridLayoutGroup>();
        float spaceArea = (jsLyt.padding.top + jsLyt.padding.bottom) * jsrt.rect.width;
        float jscellSize = (float)Math.Sqrt(((jsrt.rect.width - hzSize) * (jsrt.rect.height - hzSize) - spaceArea) / jsstr.Count);

        //如果只有一行，不能再减去一行
        if (jsrt.rect.height - hzSize <= hzSize || fixRow != 0)//谜题固定行数
        {
            jscellSize = (jsrt.rect.width - jsLyt.padding.left - jsLyt.padding.right) / jsstr.Count;
        }

        jscellSize = jscellSize > hzSize ? hzSize : jscellSize;
        jsLyt.cellSize = new Vector2(jscellSize, jscellSize);

        for (int i = 0; i < jsstr.Count; i++)
        {
            GameObject txt = Instantiate(hzPrafab, content) as GameObject;
            txt.SetActive(true);
            txt.GetComponentInChildren<Text>().text = jsstr[i];
            _tmpRstTextList.Add(txt);
        }
    }

    public void GoNextBtnClick()
    {
        _NextBtn.GetComponent<ScaleAni>().enabled = false;
        _NextBtn.interactable = false;
        Sequence nextAni = DOTween.Sequence();
        nextAni.Append(_NextBtn.transform.DOScale(0.0f, 0.5f));

        nextAni.SetEase(Ease.OutSine);
        nextAni.OnComplete(() =>
        {
            InitTiMu(false);
        });
    }
    public override void OnSwipe(Define.SWIPE_TYPE type)
    {
        if (type == Define.SWIPE_TYPE.RIGHT)
        {
            //当处于结束界面时，可以右滑切题
            if (_NextBtn.gameObject.activeSelf && _NextBtn.interactable)
            {
                GoNextBtnClick();
            }
        }
    }

    public void OnMTXJBtnClick()
    {
        //禁止点选汉字
        ShowCYInfo("谜底详解", _CurrentData[(int)HZManager.eSZMCName.HZ_JIESHI], null);
    }

    public Image _TipBrush;
    [System.Serializable] public class OnShowClickWaveEvent : UnityEvent<Transform> { }
    public OnShowClickWaveEvent OnShowClickWave;

    //单件提示，需要处理已经有选字的情况
    public void DoTip()
    {
        //查找第一个不等于答案的字的位置，如果没有字则将提示字移动到这个位置，如果有字则返回该字并将正确的字移动到这个位置
        List<Text> currentCYTextList = GetAllResultCYText();
        string currentCY = GetResultJG();
        int index = -1;
        if (!_FindJgInfo.activeSelf)
        {
            for (int i = 0; i < _ResultJG.Count; i++)
            {
                //无顺序要求，只需判断是否在里面即可
                if (currentCYTextList[i].text == "")
                {
                    index = i;
                    break;
                }
                else if (!_CurrentData[(int)HZManager.eSZMCName.HZ_END].Contains(currentCYTextList[i].text))
                {
                    index = i;
                    break;
                }
            }

            if (currentCYTextList[index].text != "")
            {
                //退回该字
                BackOne(index, DoTipOne);

                //立即返回
                return;
            }
        }
        else
        {
            //还没有选过任何字
            index = 0;
        }

        if (index < 0 || index > _ResultJG.Count)
        {
            ShowToast("非常抱歉，出错了，请重试或者重开一局");
            return;
        }

        //执行_currentcyindex里的index
        DoTipOne(index);
    }

    private void DoTipOne(int index)
    {
        DisableAllGameBtnWhenAni(true);

        int useIndex = -1;
        List<Text> currentJg = GetAllResultCYText();

        List<int> tmpCurrent = new List<int>();
        tmpCurrent.AddRange(_CurrentIndex);

        //首先把选上去的删掉
        string jg = _CurrentData[(int)HZManager.eSZMCName.HZ_END];
        for (int i = tmpCurrent.Count - 1; i >= 0; i--)
        {
            for (int j = currentJg.Count - 1; j >= 0; j--)
            {
                XTHZ tmpHZ = _HZList[tmpCurrent[i]].GetComponent<XTHZ>();

                if ("" + tmpHZ.GetHZID() == currentJg[j].name)
                {
                    tmpCurrent.RemoveAt(i);
                    currentJg.RemoveAt(j);
                    break;
                }
            }
        }

        if (tmpCurrent.Count == 0)
        {
            DisableAllGameBtnWhenAni(false);
            ShowToast("抱歉出错了，请重试或者返回重进～");
            return;
        }

        //把没有被选上去的，且和剩余的一样的删掉
        for (int i = tmpCurrent.Count - 1; i >= 0; i--)
        {
            for (int j = currentJg.Count - 1; j >= 0; j--)
            {
                if ("" + jg[i] == currentJg[j].text)
                {
                    tmpCurrent.RemoveAt(i);
                    currentJg.RemoveAt(j);
                    break;
                }
            }
        }

        if (tmpCurrent.Count == 0)
        {
            DisableAllGameBtnWhenAni(false);
            ShowToast("抱歉出错了，请重试或者返回重进～");
            return;
        }

        useIndex = tmpCurrent[0];

        //第一个字的提示，时间达到1/4时间时，超过25关不再提示第一个字
        XTHZ xTHZ = _HZList[useIndex].GetComponent<XTHZ>();
        Text hzText = _HZList[useIndex].GetComponentInChildren<Text>();
        Vector3 pos = new Vector3(hzText.transform.position.x + 50, hzText.transform.position.y + 50, 0.0f);

        _TipBrush.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.SetId("DoTip");
        _TipBrush.color = new Color(_TipBrush.color.r, _TipBrush.color.g, _TipBrush.color.b, 0.0f);
        _TipBrush.transform.position = pos;
        sequence.Append(_TipBrush.DOFade(1.0f, 0.2f));

        xTHZ.ResetHZ();
        _CurrentSelectCYIndex = useIndex;
        xTHZ.SetIsSelect(true, false);
        xTHZ.UpdateSelectID(1);//更新划选次序id

        sequence.Append(hzText.transform.DOShakeScale(0.5f, 0.2f));

        sequence.Append(_TipBrush.DOFade(0.0f, 0.2f))
                .OnComplete(OnPencilSubmit);

        OnShowClickWave.Invoke(hzText.transform);
    }

    //猜谜模式仅仅有完成题目数
    //也可以增加一个累计完成题目数
    private void UpdateScore()
    {
        _ChuangGuan++;

        int fcnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.CCK_FINISH_CNT, 0);
        int all_fcnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.CCK_ALL_FINISH_CNT, 0);

        if (fcnt >= HZManager.GetInstance().GetSZMCnt())
        {
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.CCK_FINISH_CNT, HZManager.GetInstance().GetSZMCnt());
        }
        else
        {
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.CCK_FINISH_CNT, fcnt + 1);
        }

        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.CCK_ALL_FINISH_CNT, all_fcnt + 1);

        //过关了，重置保存的当前插入字
        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.CCK_FIXED_HZ, "");

        ReportScore(all_fcnt + 1);

        _FinishCntText.text = "完成度 " + Setting.getPlayerPrefs("" + Setting.SETTING_KEY.CCK_FINISH_CNT, 0)
            + "/" + HZManager.GetInstance().GetSZMCnt();

    }

    protected override int GetCurrentScore()
    {
        return _ChuangGuan;//得分就是当前已经收集的构件个数
    }

    public override void AbandonGame(Action cb = null)
    {
        DestroyObj(_HZList);
        DestroyObj(_tmpRstTextList);

        _CanExist = true;
        gameObject.SetActive(false);

        _Pencil.DisablePencil();

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

    //删除一个已经选择字
    private void BackOne(int index, Action<int> cb = null)
    {

        Text cy = _ResultJG[index].GetComponentInChildren<Text>(true);
        //_ResultJG[index].interactable = false;

        _CurrentSelectedCnt--;
        DisableAllGameBtnWhenAni(true);

        //返回这个
        XTHZ xTHZ = _HZList[int.Parse(cy.name)].GetComponent<XTHZ>();
        Sequence backAni = DOTween.Sequence();
        xTHZ.GetHZText().text = cy.text;
        xTHZ.SetIsSelect(false, false);
        xTHZ.SetCantRestFinded(false);
        xTHZ.GetComponent<BoxCollider>().enabled = true;//启用碰撞检测


        backAni
            .Append(cy.DOFade(0.0f, 0.2f))
            .Join(xTHZ.DoHZFade(1.0f, 0.2f))
            .Append(xTHZ.GetHZText().transform.DOLocalMove(new Vector3(0f, 0f, 0f), 0.4f))
            .Join(xTHZ.GetHZText().transform.DOScale(1.0f, 0.4f))
            .Append(xTHZ.GetHZText().DOColor(Define.FONT_COLOR_DARK, 0.2f)
            .SetEase(Ease.InSine)
            .OnComplete(() => {
                cy.text = "";

                if (cb != null)
                {
                    cb(index);
                }
                else
                {
                    DisableAllGameBtnWhenAni(false);
                }

            }));
    }
    public void BackOneBtnClick()
    {
        if (_CurrentSelectedCnt == 0)
        {
            return;
        }

        for (int i = _ResultJG.Count - 1; i >= 0; i--)
        {
            Text cy = _ResultJG[i].GetComponentInChildren<Text>(true);
            if (cy.text != "")
            {
                BackOne(i);
                break;
            }
        }
    }

    //清空选择的字
    private void BackAll(Action cb = null)
    {
        _CurrentSelectedCnt = 0;
        DisableAllGameBtnWhenAni(true);
        Sequence backAllAni = DOTween.Sequence();
        for (int i = _ResultJG.Count - 1; i >= 0; i--)
        {
            Text cy = _ResultJG[i].GetComponentInChildren<Text>(true);

            if (cy.text != "")
            {
                //返回这个
                XTHZ xTHZ = _HZList[int.Parse(cy.name)].GetComponent<XTHZ>();
                Sequence backAni = DOTween.Sequence();
                xTHZ.GetHZText().text = cy.text;
                xTHZ.SetCantRestFinded(false);
                xTHZ.SetIsSelect(false, false);
                xTHZ.GetComponent<BoxCollider>().enabled = true;//启用碰撞检测


                backAni
                    .AppendInterval(0.1f * (i + 1))
                    .Append(cy.DOFade(0.0f, 0.2f))
                    .Join(xTHZ.DoHZFade(1.0f, 0.2f))
                    .Append(xTHZ.GetHZText().transform.DOLocalMove(new Vector3(0f, 0f, 0f), 0.4f))
                    .Join(xTHZ.GetHZText().transform.DOScale(1.0f, 0.4f))
                    .Append(xTHZ.GetHZText().DOColor(Define.FONT_COLOR_DARK, 0.2f)
                    .OnComplete(() => {
                        cy.text = "";
                    }));

                backAllAni.Join(backAni);
            }
        }

        backAllAni.OnComplete(() => {
            if (cb != null)
            {
                cb();
            }
            else
            {
                DisableAllGameBtnWhenAni(false);
            }
        });
    }
    public void BackAllBtnClick()
    {
        if (_CurrentSelectedCnt == 0) return;
        BackAll();
    }

    private int _CurrentDZTSCnt = 0;
    public void OnDZTSClick()
    {
        if (_CurrentDZTSCnt >= _ResultJG.Count - 1)
        {
            ShowToast("为确保谜题价值，每题使用次数须<b>少于总构件数</b>。");
            return;
        }

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

        dztsCnt[0] = "" + currentLeftShowDZTSCnt;
        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DZTS_6_12_FREE_ACH_CNT, String.Join("#", dztsCnt));

        _leftDZTSTime.text = "+" + currentLeftShowDZTSCnt;
        _CurrentDZTSCnt++;
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

        DisableAllGameBtnWhenAni(true);

        if (_FindJgInfo.activeSelf)
        {
            DoCKDAAni(true);
        }
        else
        {
            BackAll(() => DoCKDAAni(false));
        }
    }

    private void DoCKDAAni(bool hide)
    {
        UpdateScore();

        //隐藏所有汉字
        Sequence resultAni = DOTween.Sequence();
        //防止选中动画没有结束
        if (hide)
        {
            Sequence hideAni = DOTween.Sequence();
            Text[] fjit = _FindJgInfo.GetComponentsInChildren<Text>();
            Image[] fjii = _FindJgInfo.GetComponentsInChildren<Image>();

            for (int i = 0; i < 4; i++)
            {
                hideAni.Join(fjit[i].DOFade(0.0f, 0.2f))
                       .Join(fjii[i].DOFade(0.0f, 0.2f));
            }
            hideAni.OnComplete(() => {
                _FindJgInfo.SetActive(false);

                for (int i = 0; i < _ResultJG.Count; i++)
                {
                    _ResultJG[i].GetComponent<Image>().DOFade(1.0f, 0.2f);
                }
            });

            resultAni.Join(hideAni);
        }

        resultAni.AppendInterval(0.1f);

        for (int i = 0; i < _ResultJG.Count; i++)
        {
            Text cy = _ResultJG[i].GetComponentInChildren<Text>();

            XTHZ xTHZ = _HZList[_CurrentIndex[i]].GetComponentInChildren<XTHZ>();
            xTHZ.GetComponent<BoxCollider>().enabled = false;//禁用碰撞检测
            xTHZ.ResetHZ();//先重置掉动画，防止颜色变化
            xTHZ.SetCantRestFinded(true);//不能被重置
            float s = cy.rectTransform.rect.width / xTHZ.GetHZText().rectTransform.rect.width;

            Sequence resultAni2 = DOTween.Sequence();
            resultAni2.AppendInterval(0.1f * (i + 1))
                      .Join(xTHZ.DoHZColor(Define.YELLOW, 0.2f))
                      .Join(xTHZ.DoHZIDFade(0.0f, 0.2f))
                      .Join(xTHZ.DoHZIDBGFade(0.0f, 0.2f))
                      .Append(xTHZ.GetHZText().transform.DOMove(cy.transform.position, 0.4f))
                      .Join(xTHZ.GetHZText().transform.DOScale(s, 0.4f))
                      .SetEase(Ease.OutSine);

            resultAni.Join(resultAni2);
        }

        resultAni.OnComplete(() =>
        {
            for (int i = 0; i < _ResultJG.Count; i++)
            {
                Text cy = _ResultJG[i].GetComponentInChildren<Text>();
                XTHZ xTHZ2 = _HZList[_CurrentIndex[i]].GetComponentInChildren<XTHZ>();
                cy.text = xTHZ2.GetHZText().text;
                cy.name = "" + xTHZ2.GetHZID();//设置id，方便后退
            }

            Sequence makeHZAni = DOTween.Sequence();

            makeHZAni.AppendInterval(0.4f).AppendInterval(0.1f);

            float x = GetResultJGCenterX();
            for (int i = 0; i < _ResultJG.Count; i++)
            {
                Text t = _ResultJG[i].GetComponentInChildren<Text>();
                XTHZ xTHZ4 = _HZList[int.Parse(t.name)].GetComponentInChildren<XTHZ>();
                Text thz = xTHZ4.GetHZText();
                makeHZAni.Join(thz.DOFade(0.0f, 0.5f))
                         .Join(thz.transform.DOMoveX(x, 0.5f));
            }

            _ResultAniText.transform.position = new Vector3(x, _ResultJG[0].transform.position.y, _ResultJG[0].transform.position.z);
            _ResultAniText.gameObject.SetActive(true);
            _ResultAniText.transform.localScale = Vector3.zero;
            _ResultAniText.color = new Color(_ResultAniText.color.r, _ResultAniText.color.g, _ResultAniText.color.b, 0.0f);
            _ResultAniText.text = _CurrentData[(int)HZManager.eSZMCName.HZ_MIDI];
            makeHZAni.Append(_ResultAniText.transform.DOScale(1.0f, 0.5f))
                     .Join(_ResultAniText.DOFade(1.0f, 0.5f))
                     .SetEase(Ease.OutBounce);

            makeHZAni.OnComplete(DoResultCYAni);

        });
    }

    #endregion

    #region 算法部分
    //-----------------内部使用接口-----------------------

    private string GetFmtCY(HZManager.eLoadResType cyType)
    {
        string cy = "";

        if (cyType == HZManager.eLoadResType.CAICHENGYU)
        {
            List<string> ccy = HZManager.GetInstance().GetCaiCY();
            cy = ccy[(int)HZManager.eCCYCName.CY_MIDI];
        }
        else if (cyType == HZManager.eLoadResType.CHENGYU)
        {
            List<string> ccy = HZManager.GetInstance().GetChengYu();
            cy = ccy[(int)HZManager.eChengYuCName.CY_CHENGYU];
        }

        return cy;
    }

    #endregion
}
