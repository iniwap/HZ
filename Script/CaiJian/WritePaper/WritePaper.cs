using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine.UI;
using Reign;
using System.Collections;
using DG.Tweening;

public class WritePaper : MonoBehaviour
{
    public WritePencil _writePencil;

    public RawImage _paper;

    public GameObject _colorBtnPrefabs;
    public Transform _colorBtnContent;
    private List<GameObject> _colorBtns = new List<GameObject>();
    public Image _CurrentColorImg;
    public Text _CurrentColorText;

    //420个，默认20个 -- > 4000个字->400
    private int _CurrentFindHZCnt = 20;
    public GameObject _fdTips;
    public void OnAddFindHZ(string hz)
    {
        _fdTips.SetActive(false);

        int maxcnt = HZManager.GetInstance().GetCNColorCnt();

        if (Setting.GetFindHZCnt() >= 4000)
        {
            _CurrentFindHZCnt = maxcnt;
        }
        else
        {
            _CurrentFindHZCnt = Setting.GetFindHZCnt() / 10 + 20;
        }

        //需要将不合适的颜色删除
        //_CurrentFindHZCnt = maxcnt;

        //判断是否有变化
        if (_CurrentFindHZCnt > _colorBtns.Count)
        {
            string cn = AddColorBtn(_CurrentFindHZCnt - 1, false);
            UpdateColorContentSize(_colorBtnContent, _CurrentFindHZCnt);

            List<string> cinfo = HZManager.GetInstance().GetColorByID(_CurrentFindHZCnt - 1);
            Color c = new Color(int.Parse(cinfo[3]) / 255f, int.Parse(cinfo[4]) / 255f, int.Parse(cinfo[5]) / 255f);

            ShowToast("恭喜解锁第" + _CurrentFindHZCnt + "种<b>写字</b>颜色<b>[<color=" + Define.GetHexColor(c)+">" + cn + "</color>]</b>", 3f);
        }
        else
        {
            //
        }

        int page = Setting.GetFindHZCnt() / PER_PAGE_HZ_CNT;
        if (Setting.GetFindHZCnt() % PER_PAGE_HZ_CNT != 0) page++;

        OnClickClosePage(false);
        _CurrentShowPage = 0;
        //页面增加
        if (page > _PageBtnList.Count)
        {
            AddPage(page, page);
            if (_PageBtnList.Count > 1)
            {
                _PageBtnList[_PageBtnList.Count - 1].transform.SetSiblingIndex(_PageBtnList[_PageBtnList.Count - 2].transform.GetSiblingIndex() + 1);
            }
        }
        else
        {
            //更新字数
            int hz_cnt = Setting.GetFindHZCnt() % PER_PAGE_HZ_CNT;
            if (hz_cnt == 0) hz_cnt = PER_PAGE_HZ_CNT;
            _PageBtnList[_PageBtnList.Count - 1].transform.Find("JD").GetComponent<Text>().text = hz_cnt + "/" + PER_PAGE_HZ_CNT;
        }

        UpdateColorContentSize(_HZPageContent, _PageBtnList.Count);
    }

    public GameObject _HZPrefabs;
    public GameObject _PagePrefabs;
    public Transform _HZPageContent;
    private List<GameObject> _PageBtnList = new List<GameObject>();
    private List<GameObject> _FDBtnList = new List<GameObject>();
    private int PER_PAGE_HZ_CNT = 100;
    private int _CurrentShowPage = 0;
    private void InitPage()
    {
        int page = Setting.GetFindHZCnt()/PER_PAGE_HZ_CNT;

        if (Setting.GetFindHZCnt() % PER_PAGE_HZ_CNT != 0) page++;

        for (int i = 1; i <= page; i++)
        {
            AddPage(i,page);
        }

        UpdateColorContentSize(_HZPageContent, _PageBtnList.Count);
    }

    //只有点击展开页面到时候才会添加，当再次点击的时候，删除
    private void AddPage(int i,int page)
    {
        GameObject obj = Instantiate(_PagePrefabs, _HZPageContent) as GameObject;
        obj.SetActive(true);
        _PageBtnList.Add(obj);

        int pageCnt = PER_PAGE_HZ_CNT;
        if (i < page)
        {
            pageCnt = PER_PAGE_HZ_CNT;
        }
        else
        {
            pageCnt = Setting.GetFindHZCnt() % PER_PAGE_HZ_CNT;
            if (pageCnt == 0)
            {
                pageCnt = PER_PAGE_HZ_CNT;
            }
        }

        obj.transform.Find("JD").GetComponent<Text>().text = pageCnt + "/" + PER_PAGE_HZ_CNT;
        obj.transform.Find("Page").GetComponent<Text>().text = "第\n" + i + "\n页";

        Button objBtn = obj.GetComponent<Button>();
        objBtn.onClick.AddListener(delegate () {
            OnClickOpenPage(obj);
        });
    }

    //只有当页面展开当时候才会添加汉字
    private void AddHZ(string hz,int zOrder)
    {
        List<string> hzInfo = HZManager.GetInstance().GetSHZByHZ(hz);

        GameObject obj = Instantiate(_HZPrefabs, _HZPageContent) as GameObject;
        obj.SetActive(true);
        _FDBtnList.Add(obj);

        int hzID = int.Parse(hzInfo[(int)HZManager.eSHZCName.HZ_ID]);
        Color c = Define.GetTextColorByFreq(hzID);
        Color tc = Define.GetUIFontColorByBgColor(c, Define.eFontAlphaType.FONT_ALPHA_128);
        obj.transform.Find("ND").GetComponent<Image>().color = c;
        obj.transform.Find("HZ").GetComponent<Text>().text = hzInfo[(int)HZManager.eSHZCName.HZ_HZ];
        string[] pys = hzInfo[(int)HZManager.eSHZCName.HZ_PINYIN].Split('#');
        obj.transform.Find("PY").GetComponent<Text>().text = pys[0];
        obj.transform.Find("DEC").GetComponent<Text>().text = hzInfo[(int)HZManager.eSHZCName.HZ_JGDEC];
        obj.transform.Find("HZ").GetComponent<Text>().color = tc;
        obj.transform.Find("PY").GetComponent<Text>().color = tc;
        obj.transform.Find("DEC").GetComponent<Text>().color = tc;

        Button objBtn = obj.GetComponent<Button>();
        objBtn.onClick.AddListener(delegate () {
            ShowHZInfo(hzID);
        });

        obj.transform.SetSiblingIndex(_PageBtnList[_CurrentShowPage - 1].transform.GetSiblingIndex() + zOrder);
    }
    public void OnClickOpenPage(GameObject page)
    {
        int selectPage = int.Parse(page.transform.Find("Page").GetComponent<Text>().text.Split('\n')[1]);

        if (_CurrentShowPage == selectPage)
        {
            OnClickClosePage(true);
            _CurrentShowPage = 0;

            UpdateColorContentSize(_HZPageContent,_PageBtnList.Count);

            return;
        }
        else if(_CurrentShowPage != 0)
        {
            OnClickClosePage(false);
        }

        _CurrentShowPage = selectPage;
        int hzCnt = int.Parse(page.transform.Find("JD").GetComponent<Text>().text.Split('/')[0]);

        int zOrder = 1;
        for (int i = 0; i < hzCnt; i++)
        {
            AddHZ(Setting.GetFindHZByIndex(i + (_CurrentShowPage - 1) * PER_PAGE_HZ_CNT), zOrder);
            zOrder++;
        }

        for (int i = _CurrentShowPage + 1; i <= _PageBtnList.Count; i++)
        {
            _PageBtnList[i - 1].transform.SetSiblingIndex(_PageBtnList[i - 1].transform.GetSiblingIndex() + PER_PAGE_HZ_CNT);
        }

        UpdateColorContentSize(_HZPageContent, _FDBtnList.Count + _PageBtnList.Count);
    }

    public void OnClickClosePage(bool ani)
    {
        DestroyObj(_FDBtnList);//可以有动画
        for (int i = 0; i < _PageBtnList.Count; i++)
        {
            _PageBtnList[i].transform.SetSiblingIndex(i);
        }
    }

    public void OnInit()
    {
        _fdTips.SetActive(Setting.GetFindHZCnt() == 0);
        if (Setting.GetFindHZCnt() > 0)
        {
            InitPage();
        }

        int maxcnt = HZManager.GetInstance().GetCNColorCnt();

        if (Setting.GetFindHZCnt() >= 4000)
        {
            _CurrentFindHZCnt = maxcnt;
        }
        else
        {
            _CurrentFindHZCnt = Setting.GetFindHZCnt() / 10 + 20;
        }

        //需要将不合适的颜色删除
        //_CurrentFindHZCnt = maxcnt;

        for (int i = 0; i < _CurrentFindHZCnt; i++)
        {
            AddColorBtn(i,true);
        }

        UpdateColorContentSize(_colorBtnContent, _CurrentFindHZCnt);

    }

    private void UpdateColorContentSize(Transform content,int cnt)
    {
        RectTransform jsrt = content.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = content.GetComponentInChildren<GridLayoutGroup>();
        float w = jsLyt.cellSize.x * cnt + jsLyt.padding.left + jsLyt.padding.right + jsLyt.spacing.x * (cnt - 1);
        jsrt.sizeDelta = new Vector2(w, jsrt.sizeDelta.y);
    }
    private string AddColorBtn(int cid,bool isInit)
    {
        List<string> cinfo = HZManager.GetInstance().GetColorByID(cid);

        GameObject hzObj = Instantiate(_colorBtnPrefabs, _colorBtnContent) as GameObject;
        hzObj.SetActive(true);
        _colorBtns.Add(hzObj);

        hzObj.GetComponentInChildren<Text>().text = GetFmtColorText(cinfo[0]);

        Color c = new Color(int.Parse(cinfo[3]) / 255f, int.Parse(cinfo[4]) / 255f, int.Parse(cinfo[5]) / 255f);
        hzObj.GetComponent<Image>().color = c;
        hzObj.GetComponentInChildren<Text>().color = Define.GetUIFontColorByBgColor(c, Define.eFontAlphaType.FONT_ALPHA_128);
        hzObj.name = "" + cid;

        Button hzBtn = hzObj.GetComponent<Button>();
        hzBtn.onClick.AddListener(delegate () {
            OnClickColorBtn(hzObj);
        });

        if (isInit)
        {
            if(cinfo[0] == "春梅红")
                OnClickColorBtn(hzObj);
        }

        return cinfo[0];
    }
    private string GetFmtColorText(string nm)
    {
        // string txt = cid+"\n<b>·</b>\n";
        string txt = "";
        if (nm.Length == 1)
        {
            txt = nm;
        }
        else
        {
            for (int j = 0; j < nm.Length; j++)
            {
                if (j != nm.Length - 1)
                {
                    txt = txt + nm[j] + "\n";
                }
                else
                {
                    txt = txt + nm[j];
                }
            }
        }

        return txt;
    }
    public void OnClickColorBtn(GameObject btn)
    {
        List<string> cinfo = HZManager.GetInstance().GetColorByID(int.Parse(btn.name));
        Color c = new Color(int.Parse(cinfo[3]) / 255f, int.Parse(cinfo[4]) / 255f, int.Parse(cinfo[5]) / 255f,1f);

        _writePencil.UpdateWriteColor(c,c);
        _CurrentColorImg.color = c;
        _CurrentColorText.text = GetFmtColorText(cinfo[0]);
        _CurrentColorText.color = Define.GetUIFontColorByBgColor(c,Define.eFontAlphaType.FONT_ALPHA_128);
    }

    public void OnCurrentColorBtnClick()
    {
        ShowToast("<b>划一划</b>模式每探索10个新字，可解锁一种颜色");
    }

    public void OnClearBtnClick()
    {
        _writePencil.ClearWriteLine();
    }

    public void OnEnable()
    {
        if(_writePencil != null)
            _writePencil.enabled = true;
    }

    public void OnDisable()
    {
        if(_writePencil != null)
            _writePencil.enabled = false;
    }

    public void Start()
    {
        Invoke("SetUIScale",3.0f);//原则上只需要设置一次
    }

    private float _UIScale = 1.0f;
    private void SetUIScale()
    {
        _UIScale = gameObject.transform.Find("Mid").localScale.x * Screen.height / AutoFitUI.DESIGN_HEIGHT;

        RectTransform picAreaRt = _paper.GetComponent<RectTransform>();
        int ssw = (int)(picAreaRt.rect.width * _UIScale);
        _StartX = _paper.transform.position.x - ssw / 2;
        _StartY = _paper.transform.position.y - ssw / 2;
        _ScreenRect = new Rect(_StartX, _StartY, ssw, ssw);

        _writePencil.SetWriteRect(_ScreenRect);

        _writePencil.SetPaperPosOff(new Vector2(-_StartX,  -_StartY), _UIScale);
    }


    //显示字的详细信息
    public GameObject _ClickToClose;
    public void OnClickToClose()
    {
        //可以动画隐藏
        ShowDialog(false);
    }
    public GameObject _ResultHZ;
    private List<GameObject> _jieShiList = new List<GameObject>();
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
        ShowDialog(true);
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
    public Image _dialogImg;
    private void ShowDialog(bool show)
    {
        if (_ResultHZ.activeSelf == show)
        {
            return;
        }
        _ClickToClose.SetActive(true);
        _ClickToClose.GetComponent<Button>().interactable = false;
        _dialogImg.gameObject.SetActive(true);
        Sequence mySequence = DOTween.Sequence();

        if (show)
        {
            mySequence
                .Append(_ResultHZ.transform.DOScale(0.0f, 0.0f))
                .Join(_dialogImg.DOFade(0.0f, 0.0f))
                .AppendCallback(() => _ResultHZ.SetActive(true))
                .Append(_ResultHZ.transform.DOScale(1.0f, 0.8f))
                .Join(_dialogImg.DOFade(127 / 255.0f, 0.4f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() => _ClickToClose.GetComponent<Button>().interactable = true);
        }
        else
        {
            mySequence
                .Append(_ResultHZ.transform.DOScale(0.0f, 0.3f))
                .Join(_dialogImg.DOFade(0.0f, 0.3f))
                .SetEase(Ease.InSine).OnComplete(() => {
                    _ResultHZ.gameObject.SetActive(false);
                    _ClickToClose.SetActive(false);
                    _dialogImg.gameObject.SetActive(false);
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


    //分享写的字
    private Rect _ScreenRect;
    private float _StartX;
    private float _StartY;
    public void OnShareBtnClick()
	{
        RectTransform picAreaRt = _paper.GetComponent<RectTransform>();
        int ssw = (int)(picAreaRt.rect.width * _UIScale);
        StartCoroutine(CaptureScreen(_ScreenRect, ssw, ssw));
	}

	private IEnumerator CaptureScreen(Rect screenRect, int ssw, int ssh)
	{
		yield return new WaitForEndOfFrame();

        Texture2D screenShot = new Texture2D(ssw, ssh);
        screenShot.ReadPixels(screenRect, 0, 0);
        screenShot.Apply();

        var data = screenShot.EncodeToJPG();

        string fn = "CJ.jpg";
		StreamManager.SaveFile(fn, data, FolderLocations.Pictures, ((bool succeeded) =>
		{
			if (succeeded)
			{
                SocialManager.Share(data, "HanZi", "#汉字#", "汉字", "#汉字#", SocialShareDataTypes.Image_JPG);
            };
		})
		);

        // cleanup
        Destroy(screenShot);
        screenShot = null;
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
}
