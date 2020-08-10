/**************************************/
//FileName: XTHZ.cs
//Author: wtx
//Data: 03/27/2019
//Describe:  选题模式使用的汉字，主要用于生成显示诗句汉字
/**************************************/
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class XTHZ: MonoBehaviour{

    // Use this for initialization
    private int _HZID;
    public  Text _HZText;
    public Text _IDText;//这个id是诗句顺序id，不是方阵索引
    public Image _IDImage;
    private bool _IsSelect;
    private bool _cantRestForFind;
    private bool _hasIDTag;//cck没有id标志，所以不显示

    public GameObject _hasFindObj;
    public  Image _hasFindTag;
    public Image _hasFindBg;

    private float _SPEED = 0.4f;

    void Start()
    {

    }

    void OnDestroy()
    {

    }
    void OnEnable()
    {

    }
    void OnDisable()
    {

    }

    public void Update()
    {

    }

    public void InitHZ(int hzId,string hz,bool hasFind = false/*仅适用于划一划*/){
        _hasIDTag = true;
        _cantRestForFind = false;
        _HZID = hzId;
        _HZText.text = hz;
        //_IDText.text = ""+hzId;
        _IsSelect = false;

        if(hasFind)
        {
            _hasFindObj.SetActive(true);
            _hasFindTag.color = new Color(_hasFindTag.color.r, _hasFindTag.color.g, _hasFindTag.color.b,0.0f);
            _hasFindBg.color = new Color(_hasFindBg.color.r, _hasFindBg.color.g, _hasFindBg.color.b, 0.0f);

        }

        _HZText.color = new Color(Define.FONT_COLOR_DARK.r, Define.FONT_COLOR_DARK.g, Define.FONT_COLOR_DARK.b,0.0f);

        _IDImage.color = new Color(0f, 0f, 0f, 0f);//000,30
        _IDText.color = new Color(Define.FONT_COLOR_DARK.r, Define.FONT_COLOR_DARK.g, Define.FONT_COLOR_DARK.b, 0.0f);

    }

    public void SetIsSelect(bool s ,bool needAni = true){
        _IsSelect = s;

        if(!needAni )return;

        if (_IsSelect)
        {
            _HZText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            DOTween.Kill("HZUnSelect" + _HZID);
            DOTween.Kill("HZSelect" + _HZID);
            Sequence hzTextAni = DOTween.Sequence();

            Sequence hzTextAni2 = DOTween.Sequence();
            hzTextAni2.Append(_HZText.transform.DOScale(0.8f, _SPEED))
                      .Append(_HZText.transform.DOScale(1.2f, _SPEED))
                      .SetEase(Ease.OutBounce)
                      .Append(_HZText.transform.DOScale(1.0f, _SPEED));

            hzTextAni.SetId("HZSelect"+ _HZID);
            hzTextAni.Append(hzTextAni2)
                     .Join(_HZText.DOColor(Define.FONT_COLOR_LIGHT, _SPEED))
                     .Join(_IDImage.DOFade(30 / 255f, _SPEED))
                     .Join(_IDText.DOFade(1.0f, _SPEED));

        }
        else{
            DOTween.Kill("HZUnSelect" + _HZID);
            DOTween.Kill("HZSelect" + _HZID);

            _HZText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            Sequence hzTextAni = DOTween.Sequence();
            hzTextAni.SetId("HZUnSelect" + _HZID);
            hzTextAni
                .Append(_HZText.DOColor(Define.FONT_COLOR_DARK, _SPEED))
                .Join(_IDImage.DOFade(0f, _SPEED))
                .Join(_IDText.DOFade(0f, _SPEED));
                
        }
    }

    public void DisableIDTag()
    {
        _hasIDTag = false;
        _IDImage.gameObject.SetActive(false);
        _IDText.gameObject.SetActive(false);
    }

    public void UpdateSelectID(int id){
        _IDText.text = "" + id;
    }

    public bool GetHasFind()
    {
        return _hasFindObj.activeSelf;
    }
    public void UpdateFind()
    {
        _hasFindObj.SetActive(true);
        DoHZFindFade(127 / 255f, 0.5f);
        DoHZFindBgFade(1.0f, 0.5f);
    }

    public bool GetIsSelect(){
        return _IsSelect;
    }

    public void SetCantRestFinded(bool can)
    {
        _cantRestForFind = can;
    }
    public bool GetCantRestFinded()
    {
        return _cantRestForFind;
    }
    public void ResetHZ()
    {
        if (_cantRestForFind) return;

        DOTween.Kill("HZUnSelect" + _HZID);
        DOTween.Kill("HZSelect" + _HZID);

        _IsSelect = false;
        _HZText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        _HZText.color = Define.FONT_COLOR_DARK;

        HideID();
    }

    //用于结束时的全部隐藏
    public void HideID(bool ani = false){
        if (!ani)
        {
            Color c2 = Define.FONT_COLOR_DARK;
            _IDImage.color = new Color(0f, 0f, 0f, 0f);
            _IDText.color = new Color(c2.r, c2.g, c2.b, 0.0f);
        }else{
            _IDImage.DOFade(0f, _SPEED);
            _IDText.DOFade(0f, _SPEED);
        }
    }

    public Tween DoHZColor(Color v, float t)
    {
        return _HZText.DOColor(v, t);
    }

    public Tween DoHZFindFade(float v, float t)
    {
        return _hasFindTag.DOFade(v, t);
    }

    public Tween DoHZFindBgFade(float v, float t)
    {
        return _hasFindBg.DOFade(v, t);
    }

    public Tween DoHZFade(float v,float t){

        return _HZText.DOFade(v,t);
    }

    public Tween DoHZIDFade(float v, float t)
    {
        return _IDText.DOFade(v, t);
    }

    public Tween DoHZIDBGFade(float v, float t)
    {
        return _IDImage.DOFade(v, t);
    }

    //这里不处理本身的，为了更好的控制
    void OnTriggerEnter(Collider c)
    {
        //进入触发器执行的代码
    }
    void OnCollisionEnter(Collision collision)
    {
        //进入碰撞器执行的代码

    }

    public Text GetHZText()
    {
        return _HZText;
    }

    public string GetHZ(){
        return _HZText.text;
    }
    public int GetHZID(){
        return _HZID;
    }
}
