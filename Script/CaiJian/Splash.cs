using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System;

public class Splash : MonoBehaviour
{
    public void Start()
    {
        //test
        //Setting.delAllPlayerPrefs();
        //加载资源
        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.SR, false, (HZManager.eLoadResType type) =>
        {
           // Debug.Log("=====>字根数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.COLOR, false, (HZManager.eLoadResType type) =>
        {
            // Debug.Log("=====>颜色数据库加载完毕");
        });


        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.SZM, true, (HZManager.eLoadResType type) =>
        {
            //  Debug.Log("=====>带提示字谜数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.SHZ, true, (HZManager.eLoadResType type) =>
        {
            OnInit.Invoke();// 先初始化，也可以在退出启动界面完成时再调用
            ExitSplash();
        });
    }

    public UnityEvent OnInit;

    public Image _Sp;
    public Image _Border;
    public Text _AppName;
    public Image _bg;

    public UnityEvent OnExitSplash;
    //结束显示启动场景
    public void ExitSplash()
    {
        float t = 0.5f;

        Sequence mySequence = DOTween.Sequence();
        mySequence
            //.AppendInterval(1.0f)
            .Append(_Sp.DOFade(0.0f, t))
            .Join(_Border.DOFade(0.0f,t))
            .Join(_AppName.DOFade(0.0f,t))

            .Join(_bg.DOColor(Define.BG_COLOR, t))
            .SetEase(Ease.InSine)
            .OnComplete(()=>{
                this.gameObject.SetActive(false);
                OnExitSplash.Invoke();
                InitSJDB();
            });
    }

    public  void InitSJDB()
    {
        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.TANGSHI, true, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("唐诗数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.SONGCI, true, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("宋词数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.GUSHI, true, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("古诗数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.SHIJING, true, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("诗经数据库加载完毕");
        });

        //加载成语大数据资源
        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.CHENGYU, true, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("=====>成语数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.CAICHENGYU, true, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("=====>成语数据库加载完毕");
        });
    }
}
