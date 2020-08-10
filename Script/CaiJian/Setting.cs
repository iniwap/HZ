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
using Lean.Touch;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Mono.Data.Sqlite;

public class Setting: MonoBehaviour{
    // Use this for initialization
    void Start()
    {

    }

    void OnDestroy()
    {
        _DB.CloseConnection();

    }
    void OnEnable()
    {

    }
    void OnDisable()
    {

    }

    public enum SETTING_KEY{
        START_CNT,
        OPEN_SETTING_CNT,
        SHOW_APP_CNT,
        SHOW_AUTHOR_LINE,
        USE_HZ_ANI,
        SHOW_LINE_SEPARATORE,
        SPEED_LEVEL,
        ALIGNMENT_TYPE,
        SHOW_SJ_LINE,
        LIKE_ID_LIST,
        HAS_SHOW_GESTURE,
        AUTHOR,
        SHOW_SPLASH,
        HIGHEST_SCORE,//最高得分
        HIGHEST_CG,//最多连续闯关
        ACHIEVEMENT_COMPLETE_PERCENT,//成就完成比
        ALL_HIGHEST_CG,//所有最高闯关数


        //购买相关
        SHOW_PERDAY_CNT_RECORD,//保存每日更新时间信息
        SHOW_DAAN_CNT,//查看答案次数
        BUY_SHOWDAAN_ANI,//引导购买动画次数，5次
        SHOW_DZTS_6_12_FREE_ACH_CNT,//包含6、12、免费以及成就，保存为字符串
        BUY_SHOWDZTS_ANI,//引导购买动画次数，5次

        SHOW_ADJUST_PARAM_TIP,//提示再点一次可以调节更多参数
        SHOW_CHANGSJ_TIP,//提示再点一次可以调节更多参数
        SHOW_CHANGECOLOR_TIP,//提示再点一次可以调节更多参数
        SHOW_CHANGEZS_SJCOLOR_TIP,//提示修改诗句颜色

        START_COLOR,//默认启动色

        //划一划模式数据记录
        TOTAL_SELECT_RIGHT_CNT,//总划对成语数量
        TOTAL_SELECT_ERROR_CNT,//总划错成语数量
        FIND_CY_CNT,//总发现成语

        //猜一猜模式数据记录
        CCK_FINISH_CNT,//已经完成的题目数量
        CCK_ALL_FINISH_CNT,//所有完成猜谜数量，包括重复，在达到全部完成后，开始随机出题
        CCK_FIXED_HZ,//未完成之前的插入字必须固定，否则可通过来回重进对比差异得到结果
    }

    private bool _HasInited = false;
    private static SQLiteHelper _DB;

    public struct sFindHZ
    {
        public int id;
        public int hzId;
        public string hz;
    }

    private static List<sFindHZ> _FindHZList = new List<sFindHZ>();
    public void OnInit()
    {
        //
        if(!_HasInited){
            string qstr = "";
#if UNITY_EDITOR
            qstr = "data source=db.db";
#elif UNITY_IOS
        qstr = "data source=" + Application.persistentDataPath + "/db.db";
#endif
            _DB = new SQLiteHelper(qstr);
            _DB.CreateTable(Define.FIND_HZ_TABLE_NAME, new string[] { "ID","HZID","HZ"},
                new string[] { "INTEGER primary key autoincrement", "INTEGER", "TEXT" });

            SqliteDataReader reader = _DB.ReadFullTable(Define.FIND_HZ_TABLE_NAME);
            while (reader.Read())
            {
                sFindHZ item;
                item.id = reader.GetInt32(reader.GetOrdinal("ID"));
                item.hzId = reader.GetInt32(reader.GetOrdinal("HZID"));
                item.hz = reader.GetString(reader.GetOrdinal("HZ"));

                _FindHZList.Add(item);

                HZManager.GetInstance().UpdateSHZFindState(true, item.hz);

            }
        }
        //
        _HasInited = true;

    }

    public static int GetFindHZCnt()
    {
        return _FindHZList.Count;
    }
    public static List<sFindHZ> GetFindHZList()
    {
        return _FindHZList;
    }

    public static bool CheckHasFind(string hz)
    {
        bool ext = false;

        SqliteDataReader reader = _DB.ReadTable(Define.FIND_HZ_TABLE_NAME,
                                                     new string[] { "ID", "HZID", "HZ" },
                                                     new string[] { "HZ" }, new string[] { "=" }, new string[] {"'"+ hz+"'" });

        if (reader.Read())
        {
            ext = true;
        }
        else
        {
            List<string> hzInfo = HZManager.GetInstance().GetSHZByHZ(hz);

            sFindHZ item;
            item.id = _FindHZList.Count;
            item.hzId = int.Parse(hzInfo[(int)HZManager.eSHZCName.HZ_ID]);
            item.hz = hz;

            _FindHZList.Add(item);

            _DB.InsertValues(Define.FIND_HZ_TABLE_NAME,
                    new string[] { "NULL", hzInfo[0],"'"+hz+"'" });
        }

        return ext;
    }
    public static string GetFindHZByIndex(int index)
    {
        return _FindHZList[index].hz;
    }
    //----------------------参数获取--------------------------------------------

    public static bool GetShowSplash()
    {
        return getPlayerPrefs(""+SETTING_KEY.SHOW_SPLASH,0) == 1;
    }

    public LeanFingerSwipe _swpieToBack;
    public void EnableSwpie(){
        _swpieToBack.enabled = true;
    }

    //----------------------数据存取--------------------------------------------
    public static void setPlayerPrefs(string key,string value,bool immediately = false)
    {
		PlayerPrefs.SetString (key,value);
        if(immediately)
        {
            PlayerPrefs.Save();
        }
	}

	public static string getPlayerPrefs(string key,string dft)
    {
		return PlayerPrefs.GetString (key, dft);
	}

	public static void setPlayerPrefs(string key,int value, bool immediately = false)
    {
		PlayerPrefs.SetInt (key,value);
        if (immediately)
        {
            PlayerPrefs.Save();
        }
    }

	public static int getPlayerPrefs(string key,int dft)
    {
		return PlayerPrefs.GetInt(key, dft);
	}

	public static void setPlayerPrefs(string key,float value, bool immediately = false)
    {
		PlayerPrefs.SetFloat (key,value);
        if (immediately)
        {
            PlayerPrefs.Save();
        }
    }

	public static float getPlayerPrefs(string key,float dft)
    {
		return PlayerPrefs.GetFloat (key, dft);
	}

	public static void delPlayerPrefs(string key)
    {
		PlayerPrefs.DeleteKey(key);
	}

	public static void delAllPlayerPrefs(){
		PlayerPrefs.DeleteAll();
    }
    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
