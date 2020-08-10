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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Lean.Touch;
using SimpleJson;
using System.Threading;


public class Dict: MonoBehaviour{
    // Use this for initialization
    void Start()
    {
        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        float nw = Screen.width / sh;
        float ipad = FitUI.GetIsPad() ? 0.9f : 1.0f;

        RectTransform rt = _ResultSJInfo.transform.Find("ResultSJInfo").GetComponent<RectTransform>();
        nw = nw * ipad - 40;

        rt.localScale = new Vector3(nw / rt.sizeDelta.y, nw / rt.sizeDelta.y, 1.0f);
    }

    public Text _inputText;
    private HZManager.SearchType _searchType = HZManager.SearchType.HZ;
    public void OnDropDownValueChanged(int v)
    {
        _searchType = (HZManager.SearchType)v;
        if (_searchType == HZManager.SearchType.PY)
        {
            ShowToast("拼音<b>不要带声调</b>，直接用对应字母，另ü用v代替",3f);
        }
        else if (_searchType == HZManager.SearchType.CY)
        {
            ShowToast("输入某个字，查找与此字组成词语的汉字");
        }
        else if (_searchType == HZManager.SearchType.GJ)
        {
            ShowToast("偏旁、部首、组成部分均可查询");
        }
        else if (_searchType == HZManager.SearchType.SX)
        {
            ShowToast("查询网络上流行的拼音首字母缩写");
        }
    }

    private HZManager.eShiCi _CurrentSearchTSSCType = HZManager.eShiCi.TANGSHI;
    public void OnTSSCDropDownValueChanged(int v)
    {
        _CurrentSearchTSSCType = (HZManager.eShiCi)(v+1);
        if (_CurrentSearchTSSCType == HZManager.eShiCi.TANGSHI)
        {
            ShowToast("当前查询唐诗，请输入字/词/句");
        }
        else if (_CurrentSearchTSSCType == HZManager.eShiCi.SONGCI)
        {
            ShowToast("当前查询宋词，请输入字/词/句");
        }
        else if (_CurrentSearchTSSCType == HZManager.eShiCi.GUSHI)
        {
            ShowToast("当前查询古诗，请输入字/词/句");
        }
        else if (_CurrentSearchTSSCType == HZManager.eShiCi.SHIJING)
        {
            ShowToast("当前查询诗经，请输入字/词/句");
        }
    }

    public enum eCYHZPosType
    {
        eFirst,
        eSecond,
        eThird,
        eFourth,
        eFull,
    }
    private eCYHZPosType _CurrentSearchCYHZPosType = eCYHZPosType.eFirst;
    public void OnCYDropDownValueChanged(int v)
    {
        _CurrentSearchCYHZPosType = (eCYHZPosType)(v);
        if (_CurrentSearchCYHZPosType == eCYHZPosType.eFirst)
        {
            ShowToast("当前查询<b>首字</b>为某个字的成语");
        }
        else if (_CurrentSearchCYHZPosType == eCYHZPosType.eSecond)
        {
            ShowToast("当前查询<b>次字</b>为某个字的成语");
        }
        else if (_CurrentSearchCYHZPosType == eCYHZPosType.eThird)
        {
            ShowToast("当前查询<b>第三个字</b>为某个字的成语");
        }
        else if (_CurrentSearchCYHZPosType == eCYHZPosType.eFourth)
        {
            ShowToast("当前查询<b>末字</b>为某个字的成语");
        }
        else if (_CurrentSearchCYHZPosType == eCYHZPosType.eFull)
        {
            ShowToast("当前为模糊查询，不确定的字用～代替，共4字");
        }
    }

    public LeanFingerSwipe _swipeToBack;
    public void OnEnableSwipeToBack(bool en)
    {
        _swipeToBack.enabled = en;
    }

    public GameObject _HZClickToClose;
    public GameObject _CYClickToClose;
    public GameObject _ResultHZ;
    public void OnClickToCloseHZ()
    {
        //可以动画隐藏
        ShowDialog(false, eSearchType.eHZ);
    }
    public void OnClickToCloseCY()
    {
        //可以动画隐藏
        ShowDialog(false, eSearchType.eCY);
    }

    public GameObject _HZPrefab;
    public GameObject _CYPrefab;
    public GameObject _SJPrefab;
    public Transform _HZContent;
    private List<GameObject> _HZBtnList = new List<GameObject>();
    private bool _isSearching = false;
    public enum eSearchType
    {
        eHZ,
        eCY,
        eTSSC,
        eSX,//拼音首字母缩写
    }

    private eSearchType _CurrentSearchType = eSearchType.eHZ;
    public Text _placeHoderText;
    public GameObject _hzDropDown;
    public GameObject _tsscDropDown;
    public GameObject _cyDropDown;
    public void OnSearchTypeDropDownValueChanged(int v)
    {
        _CurrentSearchType = (eSearchType)v;
        if (_CurrentSearchType == eSearchType.eHZ)
        {
            _placeHoderText.text = "输入汉字、拼音、英文、构件等查询";
            ShowToast("当前查询汉字，更加全面的字典。");
            _hzDropDown.SetActive(true);
            _tsscDropDown.SetActive(false);
            _cyDropDown.SetActive(false);
        }
        else if (_CurrentSearchType == eSearchType.eCY)
        {
            _placeHoderText.text = "输入单个汉字或者4个字查询成语";
            ShowToast("当前查询成语，成语接龙必备。");
            _hzDropDown.SetActive(false);
            _tsscDropDown.SetActive(false);
            _cyDropDown.SetActive(true);
        }
        else if (_CurrentSearchType == eSearchType.eTSSC)
        {
            _placeHoderText.text = "输入字/句部分，相同字超过一半可查到";
            ShowToast("当前查询诗词，飞花令必备。");
            _hzDropDown.SetActive(false);
            _tsscDropDown.SetActive(true);
            _cyDropDown.SetActive(false);
        }
        else if (_CurrentSearchType == eSearchType.eSX)
        {
            _placeHoderText.text = "输入拼音首字母缩写，例：xswl";
            ShowToast("当前查询拼音缩写，网络流行语必备。");
            _hzDropDown.SetActive(false);
            _tsscDropDown.SetActive(false);
            _cyDropDown.SetActive(false);
        }
    }

    public void OnSearchBtnClick()
    {
        if (_isSearching)
        {
            ShowToast("正在查询中，请稍后再试...");
            return;
        }

        switch (_CurrentSearchType)
        {
            case eSearchType.eHZ:
                if (!SearchHZ())
                {
                    return;
                }
                break;
            case eSearchType.eCY:
                if (!SearchCY())
                {
                    return;
                }
                break;
            case eSearchType.eTSSC:
                if (!SearchTSSC())
                {
                    return;
                }
                break;
            case eSearchType.eSX:
                if (!SearchSX())
                {
                    return;
                }
                break;
        }

        DestroyObj(_HZBtnList);
    }

    private bool SearchHZ()
    {
        Regex regChina = new Regex("^[^\x00-\xFF]");
        Regex regEnglish = new Regex("^[a-zA-Z]");

        switch (_searchType)
        {
            case HZManager.SearchType.HZ:
                if (!regChina.IsMatch(_inputText.text))
                {
                    ShowToast("请输入正确的汉字");//这里不确定范围，只提示
                }

                if (_inputText.text.Length != 1)
                {
                    ShowToast("无效的汉字个数，请输入1个汉字");
                    return false;
                }
                break;
            case HZManager.SearchType.PY:
                if (_inputText.text.Length < 1 || _inputText.text.Length > 6)
                {
                    ShowToast("无效的拼音长度，请重新输入1-6个拼音字母");
                    return false;
                }
                if (!regEnglish.IsMatch(_inputText.text))
                {
                    ShowToast("请输入纯英文字母拼音");
                    return false;
                }

                break;
            case HZManager.SearchType.EN:
                if (_inputText.text.Length < 1 || _inputText.text.Length > 20)
                {
                    ShowToast("无效的英文单词长度，请重新输入1-20个字母");
                    return false;
                }
                if (!regEnglish.IsMatch(_inputText.text))
                {
                    ShowToast("请输入纯英文字母");
                    return false;
                }
                break;
            case HZManager.SearchType.GJ:
                if (_inputText.text.Length != 1)
                {
                    ShowToast("无效的构件个数，请输入1个构件");
                    return false;
                }
                break;
            case HZManager.SearchType.CY:
                if (_inputText.text.Length != 1)
                {
                    ShowToast("词语查询属于模糊查询，请输入1个汉字");
                    return false;
                }
                break;
        }

        _isSearching = true;
        //根据类型执行查询，这里需要采用多线程，并即时更新界面
        //直到完成
        _tip.color = new Color(_tip.color.r, _tip.color.g, _tip.color.b, 0.5f);
        _tip.gameObject.SetActive(true);
        _tip.DOFade(0f, 0.5f).OnComplete(() => {
            _tip.gameObject.SetActive(false);
            HZManager.GetInstance().SearchHZ(_searchType, _inputText.text, SearchingHZCB, () => {
                _isSearching = false;
                if (_HZBtnList.Count == 0)
                {
                    string info = "没有查询到相应的汉字，请重新输入 :(";
                    if (_searchType == HZManager.SearchType.GJ)
                    {
                        info = "此构件不构成任何汉字哟～";
                    }
                    else if (_searchType == HZManager.SearchType.CY)
                    {
                        info = "未能查到与此字组成词语的汉字 :(";
                    }

                    ShowToast(info);
                }
            });
        });

        return true;
    }

    private bool SearchCY()
    {
        Regex regChina = new Regex("^[^\x00-\xFF]");

        string search = _inputText.text;
        search = search.Replace("~", "～");

        if (!regChina.IsMatch(search))
        {
            ShowToast("请输入正确的汉字，不能包含除了<b>～</b>以外的符号", 3f);
            return false;
        }

        if (_CurrentSearchCYHZPosType == eCYHZPosType.eFull)
        {
            if (search.Length != 4)
            {
                ShowToast("全字查询必须输入4个字，不确定的字用<b>～</b>代替");
                return false;
            }
        }
        else
        {
            if (search.Length != 1)
            {
                ShowToast("包含某字成语查询，请输入1个汉字");
                return false;
            }

            if (_CurrentSearchCYHZPosType == eCYHZPosType.eFirst)
            {
                search = search + "～～～";
            }
            else if (_CurrentSearchCYHZPosType == eCYHZPosType.eSecond)
            {
                search = "～" + search + "～～";
            }
            else if (_CurrentSearchCYHZPosType == eCYHZPosType.eThird)
            {
                search = "～～" + search + "～";
            }
            else if (_CurrentSearchCYHZPosType == eCYHZPosType.eFourth)
            {
                search = "～～～" + search;
            }
        }

        _isSearching = true;
        //根据类型执行查询，这里需要采用多线程，并即时更新界面
        //直到完成
        _tip.color = new Color(_tip.color.r, _tip.color.g, _tip.color.b, 0.5f);
        _tip.gameObject.SetActive(true);
        _tip.DOFade(0f, 0.5f).OnComplete(() => {
            _tip.gameObject.SetActive(false);
            HZManager.GetInstance().SearchCY(search, SearchingCYCB, () => {
                _isSearching = false;
                if (_HZBtnList.Count == 0)
                {
                    string info = "没有查询到成语，请重新输入 :(";

                    ShowToast(info);
                }
            });
        });

        return true;
    }
    private bool SearchTSSC()
    {
        Regex regChina = new Regex("^[^\x00-\xFF]");
        Regex regEnglish = new Regex("^[a-zA-Z]");

        if (!regChina.IsMatch(_inputText.text))
        {
            ShowToast("请输入正确的汉字，不然会查不到哟 :(");//这里不确定范围，只提示
        }

        if (_inputText.text.Length < 1 || _inputText.text.Length > 7)
        {
            ShowToast("请输入诗句/单字/词语，1-7个汉字");
            return false;
        }

        _isSearching = true;
        //根据类型执行查询，这里需要采用多线程，并即时更新界面
        //直到完成
        _tip.color = new Color(_tip.color.r, _tip.color.g, _tip.color.b, 0.5f);
        _tip.gameObject.SetActive(true);
        _tip.DOFade(0f, 0.5f).OnComplete(() => {
            _tip.gameObject.SetActive(false);
            HZManager.GetInstance().SearchTSSC(_CurrentSearchTSSCType, _inputText.text, SearchingTSSCCallback, () => {
                _isSearching = false;
                if (_HZBtnList.Count == 0)
                {
                    string info = "没有查询到包含搜索内容的诗词，请重新输入 :(";
                    ShowToast(info);
                }
            });
        });

        return true;
    }

    [Serializable]
    public class SXData
    {
        public string name;
        public List<string> trans;

        public static SXData deserialize(string json)
        {
            return JsonUtility.FromJson<SXData>(json);
        }
    };
    private bool SearchSX()
    {
        Regex regChina = new Regex("^[^\x00-\xFF]");
        Regex regEnglish = new Regex("^[a-zA-Z]");

        if (_inputText.text.Length < 2 || _inputText.text.Length > 10)
        {
            ShowToast("无效的拼音缩写长度，请重新输入2-10个拼音首字字母组合");
            return false;
        }
        if (!regEnglish.IsMatch(_inputText.text))
        {
            ShowToast("请输入纯英文字母拼音首字母缩写");
            return false;
        }

        if (_inputText.text.ToLower().Contains("i"))
        {
            ShowToast("拼音首字母没有为i的汉字，请重新输入");
            return false;
        }
        if (_inputText.text.ToLower().Contains("u"))
        {
            ShowToast("拼音首字母没有为u的汉字，请重新输入");
            return false;
        }
        if (_inputText.text.ToLower().Contains("v"))
        {
            ShowToast("拼音首字母没有为v的汉字，请重新输入");
            return false;
        }

        _isSearching = true;
        //根据类型执行查询，这里需要采用多线程，并即时更新界面
        //直到完成
        _tip.color = new Color(_tip.color.r, _tip.color.g, _tip.color.b, 0.5f);
        _tip.gameObject.SetActive(true);
        _tip.DOFade(0f, 0.5f).OnComplete(() => {
            _tip.gameObject.SetActive(false);

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("text",_inputText.text);

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    string ret = Define.Post("https://lab.magiconch.com/api/nbnhhsh/guess", dic);

                    Loom.QueueOnMainThread((param) =>
                    {
                        SimpleJson.JsonArray jsa = SimpleJson.SimpleJson.DeserializeObject(ret) as SimpleJson.JsonArray;
                        if (jsa == null || jsa.Count == 0)
                        {
                            ShowToast("无法搜索，请尝试拼音查询");
                            return;
                        }

                        SXData sxData = SXData.deserialize(jsa[0].ToString());
                        if (sxData.trans.Count != 0)
                        {
                            SearchingHZPYCB(sxData.trans);
                        }
                        else
                        {
                            ShowToast("未找到相应的拼音缩写转译。");
                        }
                        _isSearching = false;
                    }, null);

                });

                thread.Start();
            });
        });

        return true;
    }

    public Text _tip;
    public void SearchingHZPYCB(List<string> pyList)
    {
        //150,150
        foreach(var hzs in pyList)
        {
            GameObject hzObj = Instantiate(_SJPrefab, _HZContent) as GameObject;
            hzObj.SetActive(true);
            _HZBtnList.Add(hzObj);

            Text hzText = hzObj.GetComponentInChildren<Text>();
            //这里可以多重颜色来表达汉字的常见度
            hzText.text = hzs;
            //显示动画
            hzObj.transform.localScale = Vector3.zero;
            hzObj.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
        }

        //设置高度
        int row = _HZBtnList.Count / 2;
        if (_HZBtnList.Count % 2 != 0)
        {
            row += 1;
        }

        RectTransform jsrt = _HZContent.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = _HZContent.GetComponentInChildren<GridLayoutGroup>();
        float h = jsLyt.cellSize.y * row + jsLyt.padding.top + jsLyt.padding.bottom + jsLyt.spacing.y * row;
        jsrt.sizeDelta = new Vector2(jsrt.sizeDelta.x, h);
        jsLyt.cellSize = new Vector2(490, 150);

    }
    public void SearchingHZCB(List<string> hzInfo)
    {
        //150,150

        if (hzInfo.Count == 0) return;

        int freq = int.Parse(hzInfo[(int)HZManager.eSHZCName.HZ_ID]);

        GameObject hzObj = Instantiate(_HZPrefab, _HZContent) as GameObject;
        hzObj.SetActive(true);
        _HZBtnList.Add(hzObj);

        Button hzBtn = hzObj.GetComponent<Button>();
        hzBtn.onClick.AddListener(delegate () {
            ShowHZInfo(freq);
        });

        Text hzText = hzObj.GetComponentInChildren<Text>();
        //这里可以多重颜色来表达汉字的常见度
        hzText.text = hzInfo[(int)HZManager.eSHZCName.HZ_HZ];

        hzText.color = Define.GetTextColorByFreq(freq);

        //设置高度
        int row = _HZBtnList.Count / 6 + 1;
        RectTransform jsrt = _HZContent.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = _HZContent.GetComponentInChildren<GridLayoutGroup>();
        float h = jsLyt.cellSize.y * row + jsLyt.padding.top + jsLyt.padding.bottom + jsLyt.spacing.y * row;
        jsrt.sizeDelta = new Vector2(jsrt.sizeDelta.x, h);

        //显示动画
        hzObj.transform.localScale = Vector3.zero;
        hzObj.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);

        jsLyt.cellSize = new Vector2(150,150);
    }

    private void ShowHZInfo(int hzID)
    {
        List<string> hz = HZManager.GetInstance().GetSHZ(hzID);

        string findHZ = hz[(int)HZManager.eSHZCName.HZ_HZ];
        string findFY = hz[(int)HZManager.eSHZCName.HZ_PINYIN];
        string findJs = hz[(int)HZManager.eSHZCName.HZ_JIESHI];
        string findEn = hz[(int)HZManager.eSHZCName.HZ_ENGLISH];
        string findJG = hz[(int)HZManager.eSHZCName.HZ_JGDEC];

        //所有划选结果汉字
        List<Text> jss = new List<Text>();//解释
        //其他还有米字格，难度星，tags，分割线
        //搜索所有节点
        //汉字
        Text hzText = _ResultHZ.transform.Find("HZ/HZ/HZ").GetComponent<Text>();
        hzText.text = findHZ;

        //拼音
        string[] tpys = findFY.Split('#');
        Text pyText = _ResultHZ.transform.Find("HZ/PY").GetComponent<Text>();
        pyText.text = tpys[0];

        Text morePy = _ResultHZ.transform.Find("HZ/MorePY").GetComponent<Text>();
        morePy.text = "";
        Text tagMore = _ResultHZ.transform.Find("HZ/MorePY/Tag/TagText").GetComponent<Text>();
        if (tpys.Length == 1)
        {
            tagMore.text = "音";
        }
        else
        {
            tagMore.text = "多";
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
        //难度
        int nd = 1 + 10 * int.Parse(hz[(int)HZManager.eSHZCName.HZ_ID]) / HZManager.GetInstance().GetSHZCnt();
        nd = nd > 10 ? 10 : nd;
        int star1 = nd / 2;
        int halfstar = nd % 2;
        for (int s = 0; s < 5; s++)
        {
            GameObject objStar0 = _ResultHZ.transform.Find("HZ/ND/NanDu/Star0" + (s + 1)).gameObject;
            objStar0.gameObject.SetActive(false);

            GameObject objStar1 = _ResultHZ.transform.Find("HZ/ND/NanDu/Star1" + (s + 1)).gameObject;
            objStar1.gameObject.SetActive(false);

            GameObject objHalfStar = _ResultHZ.transform.Find("HZ/ND/NanDu/HalfStar" + (s + 1)).gameObject;
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
        if (hz[(int)HZManager.eSHZCName.HZ_FIND] == "1")
        {
            _ResultHZ.transform.Find("HZ/Find").GetComponentInChildren<Text>().text = "此字已经被探索";
            _ResultHZ.transform.Find("HZ/Find/Viewport").gameObject.SetActive(true);
            _ResultHZ.transform.Find("HZ/Find").GetComponentInChildren<Text>().color = Define.GREEN;
        }
        else
        {
            _ResultHZ.transform.Find("HZ/Find").GetComponentInChildren<Text>().text = "此字尚未被探索";
            _ResultHZ.transform.Find("HZ/Find/Viewport").gameObject.SetActive(false);
            _ResultHZ.transform.Find("HZ/Find").GetComponentInChildren<Text>().color = Define.BLUE;
        }

        //结构
        Text jgText = _ResultHZ.transform.Find("HZ/JG").GetComponent<Text>();
        jgText.text = findJG;

        //填充解释并获取
        SetJieShi(findJs);

        //英文
        Text enText = _ResultHZ.transform.Find("En").GetComponent<Text>();
        enText.text = findEn.Replace('#', ',');
        /*
        if (enText.text == "N/A")
        {
            enText.gameObject.SetActive(false);
        }
        else
        {
            enText.gameObject.SetActive(true);
        }
        */
        ShowDialog(true, eSearchType.eHZ);
    }

    public GameObject _jsHzPrafab;
    public Transform _jieShiTransform;
    private void SetJieShi(string jieShi)
    {
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
        for (int i = MaxHZSize; i >= MinHZSize; i--)
        {
            int LineCnt = 0;
            for (int j = 0; j < jss.Count; j++)
            {

                perLineCnt = (int)(jsrt.rect.width / i);
                LineCnt += jss[j].Length / perLineCnt;
                LineCnt += jss[j].Length % perLineCnt == 0 ? 0 : 1;
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

            if (jss.Count != 1)
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

                GameObject indexText = Instantiate(_jsHzPrafab, _jieShiTransform) as GameObject;
                indexText.SetActive(true);
                indexText.GetComponentInChildren<Text>().text = "<b>" + index + ".</b>";
                _jieShiList.Add(indexText);
            }

            for (int j = startIndex; j < jss[i].Length; j++)
            {
                GameObject txt = Instantiate(_jsHzPrafab, _jieShiTransform) as GameObject;
                txt.SetActive(true);
                txt.GetComponentInChildren<Text>().text = "" + jss[i][j];

                _jieShiList.Add(txt);
            }
        }
    }
    public Image _HZDialogImg;
    public Image _CYDialogImg;
    private void ShowDialog(bool show,eSearchType type)
    {
        GameObject result = _ResultHZ;
        Image dialogImg = _HZDialogImg;
        GameObject clickToClose = _HZClickToClose;
        if (type == eSearchType.eHZ)
        {
            result = _ResultHZ;
            dialogImg = _HZDialogImg;
            clickToClose = _HZClickToClose;
        }
        else if (type == eSearchType.eCY)
        {
            result = _ResultCY;
            dialogImg = _CYDialogImg;
            clickToClose = _CYClickToClose;
        }
        else if (type == eSearchType.eTSSC)
        {
            return;
        }

        if (result.activeSelf == show)
        {
            return;
        }

        clickToClose.SetActive(true);
        clickToClose.GetComponent<Button>().interactable = false;
        dialogImg.gameObject.SetActive(true);
        Sequence mySequence = DOTween.Sequence();

        if (show)
        {
            mySequence
                .Append(result.transform.DOScale(0.0f, 0.0f))
                .Join(dialogImg.DOFade(0.0f, 0.0f))
                .AppendCallback(() => result.SetActive(true))
                .Append(result.transform.DOScale(1.0f, 0.5f))
                .Join(dialogImg.DOFade(127 / 255.0f, 0.3f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() => clickToClose.GetComponent<Button>().interactable = true);
        }
        else
        {
            mySequence
                .Append(result.transform.DOScale(0.0f, 0.3f))
                .Join(dialogImg.DOFade(0.0f, 0.3f))
                .SetEase(Ease.InSine).OnComplete(() => {
                    result.gameObject.SetActive(false);
                    clickToClose.SetActive(false);
                    dialogImg.gameObject.SetActive(false);
                });
        }
    }

    //-----------成语---------------------------
    public void SearchingCYCB(List<string> hzInfo)
    {
        //235,120

        if (hzInfo.Count == 0) return;

        GameObject hzObj = Instantiate(_CYPrefab, _HZContent) as GameObject;
        hzObj.SetActive(true);
        _HZBtnList.Add(hzObj);

        Button hzBtn = hzObj.GetComponent<Button>();
        hzBtn.onClick.AddListener(delegate () {
            ShowCYInfo(hzInfo);
        });

        Text hzText = hzObj.GetComponentInChildren<Text>();
        //这里可以多重颜色来表达汉字的常见度
        hzText.text = hzInfo[(int)HZManager.eChengYuCName.CY_CHENGYU];

        //设置高度
        int row = _HZBtnList.Count / 4;
        if (_HZBtnList.Count % 4 != 0)
        {
            row++;
        }

        RectTransform jsrt = _HZContent.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = _HZContent.GetComponentInChildren<GridLayoutGroup>();
        float h = jsLyt.cellSize.y * row + jsLyt.padding.top + jsLyt.padding.bottom + jsLyt.spacing.y * row;
        jsrt.sizeDelta = new Vector2(jsrt.sizeDelta.x, h);

        jsLyt.cellSize = new Vector2(235, 120);

        //显示动画
        hzObj.transform.localScale = Vector3.zero;
        hzObj.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
    }

    public GameObject _ResultCY;
    private void ShowCYInfo(List<string> cy)
    {
        DestroyObj(_tmpRstTextList);
        //设置要显示的成语信息
        string findCY = cy[(int)HZManager.eChengYuCName.CY_CHENGYU];

        string findFY = "读音：" + cy[(int)HZManager.eChengYuCName.CY_FAYIN];
        string findJs = "解释：" + cy[(int)HZManager.eChengYuCName.CY_JIESHI];
        string findZj = "例句·出处：" + cy[(int)HZManager.eChengYuCName.CY_ZAOJU];


        //所有划选结果汉字
        List<Text> cys = new List<Text>();
        List<Text> jss = new List<Text>();//所有解释
        List<Text> zjs = new List<Text>();//所有造句

        //成语，需要移动到划选结果位置
        cys.AddRange(_ResultCY.transform.Find("CY").GetComponentsInChildren<Text>());
        for (int i = 0; i < 4; i++)
        {
            cys[i].text = "" + findCY[i];
        }

        //填充解释并获取
        Transform jieshi = _ResultCY.transform.Find("JieShi");
        FitDialogContentText(jieshi, findJs, _HZInfoPrafab);
        jss.AddRange(jieshi.GetComponentsInChildren<Text>());

        //填充造句
        Transform zaoju = _ResultCY.transform.Find("LiJu");
        FitDialogContentText(zaoju, findZj, _HZInfoPrafab);
        zjs.AddRange(zaoju.GetComponentsInChildren<Text>());

        //读音、解释、造句
        Text fy = _ResultCY.transform.Find("FaYin").GetComponent<Text>();
        fy.text = findFY;

        ShowDialog(true, eSearchType.eCY);
    }
    //结束时必须删除 
    public GameObject _HZInfoPrafab;
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

    //---------------诗词-----------------------
    public void SearchingTSSCCallback(List<string> scInfo)
    {
        //490,150

        if (scInfo.Count == 0) return;

        GameObject hzObj = Instantiate(_SJPrefab, _HZContent) as GameObject;
        hzObj.SetActive(true);
        _HZBtnList.Add(hzObj);

        Button hzBtn = hzObj.GetComponent<Button>();
        hzBtn.onClick.AddListener(delegate () {
            ShowSJInfo(scInfo, true);
        });

        Text hzText = hzObj.GetComponentInChildren<Text>();
        //这里可以多重颜色来表达汉字的常见度
        string sj = scInfo[(int)HZManager.eTSSCColName.TSSC_END];//最后一条存放搜索到的相应诗句，用于按钮缩略显示
        hzText.text = sj.Replace("，", "，\n");

        //设置高度
        int row = _HZBtnList.Count / 2;
        if (_HZBtnList.Count % 2 != 0)
        {
            row += 1;
        }

        RectTransform jsrt = _HZContent.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = _HZContent.GetComponentInChildren<GridLayoutGroup>();
        float h = jsLyt.cellSize.y * row + jsLyt.padding.top + jsLyt.padding.bottom + jsLyt.spacing.y * row;
        jsrt.sizeDelta = new Vector2(jsrt.sizeDelta.x, h);
        jsLyt.cellSize = new Vector2(490, 150);

        //显示动画
        hzObj.transform.localScale = Vector3.zero;
        hzObj.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
    }

    private List<GameObject> _jieShiList = new List<GameObject>();

    public Text _TiMuText;
    public Text _ZZCDText;
    List<GameObject> _sjList = new List<GameObject>();
    public GameObject _sjPrefabs;
    public GameObject _ywPrefabs;
    public Transform _sjInfoContent;
    private void SetSJ(List<string> sjInfo)
    {
        DestroyObj(_sjList);
        string tm = sjInfo[(int)HZManager.eTSSCColName.TSSC_TiMu];
        string zzcd = sjInfo[(int)HZManager.eTSSCColName.TSSC_ChaoDai] + "·" + sjInfo[(int)HZManager.eTSSCColName.TSSC_ZuoZhe];
        string nr = sjInfo[(int)HZManager.eTSSCColName.TSSC_NeiRong];
        string yw = "  译文：" + sjInfo[(int)HZManager.eTSSCColName.TSSC_YiWen];

        int totalLineCnt = 2;

        _TiMuText.text = tm;
        _ZZCDText.text = zzcd;
        int cnt = 0;
        List<string> sj = new List<string>();
        nr = nr.Replace("#", "");
        for (int i = 0; i < nr.Length; i++)
        {
            cnt++;
            if ("" + nr[i] == "。" || "" + nr[i] == "；" || "" + nr[i] == "！")
            {
                sj.Add(nr.Substring(i + 1 - cnt, cnt));
                cnt = 0;
            }
            else if ("" + nr[i] == "？")
            {
                string subSj = nr.Substring(i + 1 - cnt, cnt);
                if (subSj.Contains("，"))// 问号是前半句，不能做单句处理
                {
                    sj.Add(subSj);
                    cnt = 0;
                }
            }
        }

        foreach (var s in sj)
        {
            GameObject sjText = Instantiate(_sjPrefabs, _sjInfoContent) as GameObject;
            sjText.SetActive(true);
            sjText.GetComponentInChildren<Text>().text = s;
            _sjList.Add(sjText);
        }

        totalLineCnt += sj.Count;

        yw = yw.Replace("N/A", "暂无。");

        List<string> ywList = new List<string>();
        int everyLineHZCnt = 770 / 32;
        int lcnt = yw.Length / everyLineHZCnt;  //770/44 = 17.5
        bool toolong = true;
        for (int c = 32; c >= 16; c--)
        {
            everyLineHZCnt = 770 / c;
            lcnt = yw.Length / everyLineHZCnt;  //770/44 = 17.5

            if (totalLineCnt + lcnt > 80)
            {
                continue;
            }

            toolong = false;

            break;
        }

        if (!toolong)
        {
            if (yw.Length % everyLineHZCnt != 0) lcnt += 1;

            for (int i = 0; i < lcnt; i++)
            {
                if (i == lcnt - 1)
                {
                    ywList.Add(yw.Substring(i * everyLineHZCnt, yw.Length - i * everyLineHZCnt));
                }
                else
                {
                    ywList.Add(yw.Substring(i * everyLineHZCnt, everyLineHZCnt));
                }
            }

            foreach (var s in ywList)
            {
                GameObject sjText = Instantiate(_ywPrefabs, _sjInfoContent) as GameObject;
                sjText.SetActive(true);
                sjText.GetComponentInChildren<Text>().text = s;
                _sjList.Add(sjText);
            }
        }

        totalLineCnt += ywList.Count;

        //设置大小
        RectTransform rt = _sjInfoContent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 100 * totalLineCnt);
    }

    public Image _SJDialogImg;
    public GameObject _ResultSJInfo;
    public void OnClickToCloseTSSC()
    {
        //可以动画隐藏
        ShowSJInfo(null, false);
    }
    public void ShowSJInfo(List<string> sjInfo, bool open)
    {
        if (_ResultSJInfo.activeSelf == open) return;

        Sequence mySequence = DOTween.Sequence();

        float toX = 0;

        GameObject likes = _ResultSJInfo.transform.Find("ResultSJInfo").gameObject;
        RectTransform rt = likes.GetComponent<RectTransform>();

        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        float nw = Screen.width / sh;

        if (open)
        {
            SetSJ(sjInfo);
            //设置参数面板标题
            //打开动画
            _ResultSJInfo.SetActive(true);

            likes.transform.localPosition = new Vector3(nw / 2 + rt.rect.width * rt.localScale.x,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = nw / 2;
            mySequence
                .Append(likes.transform.DOLocalMoveX(toX, 1.0f))
                .Join(_SJDialogImg.DOFade(127 / 255.0f, 0.5f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    //
                });

        }
        else
        {
            //关闭动画
            likes.transform.localPosition = new Vector3(nw / 2,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = nw / 2 + rt.rect.width * rt.localScale.x;

            mySequence
                .Append(likes.transform.DOLocalMoveX(toX, 0.5f))
                .Join(_SJDialogImg.DOFade(0f, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _ResultSJInfo.SetActive(false);
                });
        }
    }
    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
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
}
