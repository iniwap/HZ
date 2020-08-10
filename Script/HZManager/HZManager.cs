/**************************************/
//FileName: HZManager.cs
//Author: wtx
//Data: 03/12/2018
//Describe: 汉字检索底层算法处理
/**************************************/

using System;
using System.Data;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;

public class HZManager{
    #region 字段名字
    // 为了更规范，外部使用以下接口访问字段
    public static string UC
    {
        get
        {
            return "UC";
        }
    }
    public static string HZ
    {
        get
        {
            return "HZ";
        }
    }
    public static string JGDEC
    {
        get
        {
            return "JGDEC";
        }
    }
    public static string UDEC
    {
        get
        {
            return "UDEC";
        }
    }
    public static string JGUDEC
    {
        get
        {
            return "JGUDEC";
        }
    }
    public static string RCNT
    {
        get
        {
            return "RCNT";
        }
    }
    public static string JGID
    {
        get
        {
            return "JGID";
        }
    }
    public static string FREQ
    {
        get
        {
            return "FREQ";
        }
    }
    public static string CFREQ
    {
        get
        {
            return "CFREQ";
        }
    }
    public static string PINYIN
    {
        get
        {
            return "PINYIN";
        }
    }
    public static string JIESHI
    {
        get
        {
            return "JIESHI";
        }
    }
    public static string ENGLISH
    {
        get
        {
            return "ENGLISH";
        }
    }
    public static string FIND
    {
        get
        {
            return "FIND";
        }
    }
    public static string CIYU
    {
        get
        {
            return "CIYU";
        }
    }

    //以下字谜数据库字段
    public static string TIMU
    {
        get
        {
            return "TIMU";
        }
    }
    public static string NANDU
    {
        get
        {
            return "NANDU";
        }
    }

    public static string DAAN
    {
        get
        {
            return "DAAN";
        }
    }

    //以下字根数据库字段
    public static string RADICAL
    {
        get
        {
            return "RADICAL";
        }
    }

    public static string INHZ
    {
        get
        {
            return "INHZ";
        }
    }


    public static string ENCODE
    {
        get
        {
            return "ENCODE";
        }
    }

    public static string HZCNT
    {
        get
        {
            return "HZCNT";
        }
    }

    public static string DEC
    {
        get
        {
            return "DEC";
        }
    }


    //以下为唐诗宋词字段
    public static string CHAODAI
    {
        get
        {
            return "CHAODAI";
        }
    }
    public static string ZUOZHE
    {
        get
        {
            return "ZUOZHE";
        }
    }
    public static string NEIRONG
    {
        get
        {
            return "NEIRONG";
        }
    }
    public static string YIWEN
    {
        get
        {
            return "YIWEN";
        }
    }
    public static string ZZJIANJIE
    {
        get
        {
            return "ZZJIANJIE";
        }
    }

    //中日传统色颜色数据库
    public static string NAME
    {
        get
        {
            return "NAME";
        }
    }
    public static string HEX
    {
        get
        {
            return "HEX";
        }
    }
    public static string R
    {
        get
        {
            return "R";
        }
    }
    public static string G
    {
        get
        {
            return "G";
        }
    }
    public static string B
    {
        get
        {
            return "B";
        }
    }
    public static string CN
    {
        get
        {
            return "CN";
        }
    }

    /*
    public static string PINYIN
    {
        get
        {
            return "PINYIN";
        }
    }
    */

    //以下为猜谜语字段
    public static string MITI
    {
        get
        {
            return "MITI";
        }
    }
    //NANDU
    public static string MIGE
    {
        get
        {
            return "MIGE";
        }
    }
    public static string TISHI
    {
        get
        {
            return "TISHI";
        }
    }
    public static string JIEMI
    {
        get
        {
            return "JIEMI";
        }
    }
    public static string MIDI
    {
        get
        {
            return "MIDI";
        }
    }
    public static string FAYIN
    {
        get
        {
            return "FAYIN";
        }
    }
    //JIESHI
    public static string ZAOJU
    {
        get
        {
            return "ZAOJU";
        }
    }

    public static string CHENGYU
    {
        get
        {
            return "CHENGYU";
        }
    }
    public static string CHUCHU
    {
        get
        {
            return "CHUCHU";
        }
    }

    //注意：值越大频率越低，即字越不常见
    //汉字使用频率最大最小值
    public static int HZFREQ_HIGH
    {
        get
        {
            return 1;
        }
    }

    public static int HZFREQ_LOW
    {
        get
        {
            return 8000;
        }
    }
    #endregion

    //['UC','HZ','JGDEC','JGUDEC','UDEC','RCNT','JGID','PINYIN','JIESHI','ENGLISH','CIYU']
    public enum eCSVColName
    {
        UC,//unicode-16  作为汉字/部件的唯一标识，界面务必用此字段关联，不要保存其他信息
        HZ,//汉字-唯一
        JGDEC,//带字体结构的字根拆分，【该字段仅用于调试，查看，发布版本会删除】
        JGUDEC,//带字体结构的unicode拆分，用于获取拆分序列组装界面汉字
        UDEC,//不带字体结构的unicode拆分，用于检索界面组件序列是否构成汉字
        RCNT,//构件数，优化检索速度
        JGID,//字形id
        FREQ,//词语常见程度，数字越小，越常见
        CFREQ,//字根构字数
        PINYIN,//拼音
        JIESHI,//解释
        ENGLISH,//英文
        CIYU,//相关词语
    }

    //汉字结构
    public enum eHZJieGou
    {
        ERROR = -1,
        NO_USE,//无法拆解，没有意义的字（即不构成其他字）
        DAN_TI,//字根
        ZUO_YOU,//左右结构
        SHANG_XIA,//上下结构
        ZUO_ZHONG_YOU,//左中右结构
        SHANG_ZHONG_XIA,//上中下结构
        QUAN_BAO_WEI,//全包围结构
        SHANG_BAO_WEI,//上包围结构
        XIA_BAO_WEI,//下包围结构
        ZUO_BAO_WEI,//左包围结构
        ZUO_SHANG_BAO_WEI,//左上包围结构
        YOU_SHANG_BAO_WEI,//右围结构
        ZUO_XIA_BAO_WEI,//左下包围结构
        QIAN_TAO,//嵌套结构
    }

    private static HZManager _instance = null;
    private static DataTable _hzDataTable = null;
    private static DataTable _radicalDataTable = null;
    private static DataTable _tsDataTable = null;
    private static DataTable _scDataTable = null;
    private static DataTable _gsDataTable = null;
    private static DataTable _sjDataTable = null;
    private static DataTable _colorDataTable = null;
    private static DataTable _caiCYDataTable = null;
    private static DataTable _chengYuDataTable = null;
    private static DataTable _sHZDataTable = null;//精简版汉字数据库
    private static DataTable _sZMDataTable = null;//合并版字谜库
    private static DataTable _sRDataTable = null;//合并版字根库

    private static Boolean _inited = false;
    private static Boolean _rdbInited = false;
    private static Boolean _tsdbInited = false;//唐诗
    private static Boolean _scdbInited = false;//宋词
    private static Boolean _gsdbInited = false;//古诗
    private static Boolean _sjdbInited = false;//诗经
    private static Boolean _colordbInited = false;
    private static Boolean _caiCYdbInited = false;
    private static Boolean _chengYudbInited = false;
    private static Boolean _sHZdbInited = false;
    private static Boolean _sZMdbInited = false;
    private static Boolean _sRdbInited = false;


    private HZManager(){
		
	}

	public static HZManager GetInstance(){
		if(_instance == null)
		{
			_instance = new HZManager();
		}

		return _instance;
	}

    public enum eLoadResType{
        HZDB,
        RADICALDB,
        COLOR,
        TANGSHI,
        SONGCI,
        GUSHI,
        SHIJING,
        CAICHENGYU,//猜成语
        CHENGYU,//成语

        //汉字应用使用//
        SR,
        SHZ,//精简版汉字
        SZM,//精简版字谜
    }

    //字库加载过于耗时，这里采用多线程，防止阻塞ui
    //加载文字库
    public void LoadRes(eLoadResType type, bool async = false, Action<eLoadResType> callBack = null){

        if (type == eLoadResType.HZDB)
        {
            if (_inited) return;

            _inited = true;

            TextAsset binAsset = Resources.Load("db/hzdb", typeof(TextAsset)) as TextAsset;
            string hzDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _hzDataTable = GetDataTable(hzDBCache, false);
                //设置主键
                //_hzDataTable.Rows.Find 速度比select快，仅对主键有效
                _hzDataTable.PrimaryKey = new DataColumn[] { _hzDataTable.Columns["UC"] };
                //无法对udec创建主键，因为有些字组合uc相同，但是字体结构不同，也就是不是唯一的

                // 用Loom的方法在Unity主线程中调用ui组件
                Loom.QueueOnMainThread((param) =>
                    {
                        if (callBack != null)
                        {
                            callBack(type);
                        }
                    }, null);
                });

                thread.Start();
            });
        }else if(type == eLoadResType.RADICALDB){
            LoadRadicalDB(async, callBack);
        }else if (type == eLoadResType.COLOR){
            LoadColorDB(async, callBack);
        }else if (type == eLoadResType.TANGSHI){
            LoadTSDB(async, callBack); 
        }else if (type == eLoadResType.SONGCI){
            LoadSCDB(async, callBack);
        }else if (type == eLoadResType.GUSHI){
            LoadGSDB(async, callBack);
        }else if (type == eLoadResType.SHIJING){
            LoadSJDB(async, callBack);
        }else if (type == eLoadResType.CAICHENGYU){
            LoadCaiCYDB(async, callBack);
        }else if (type == eLoadResType.CHENGYU){
            LoadChengYuDB(async, callBack);
        }
        else if (type == eLoadResType.SHZ)
        {
            LoadSHZDB(async, callBack);
        }

        else if (type == eLoadResType.SZM)
        {
            LoadSZMDB(async, callBack);
        }
        else if (type == eLoadResType.SR)
        {
            LoadSRDB(async, callBack);
        }
    }
    //加载字根库
    private void LoadRadicalDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_rdbInited) return;

        _rdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/rdb", typeof(TextAsset)) as TextAsset;
            string cyDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _radicalDataTable = GetDataTable(cyDBCache, false);
                    _radicalDataTable.PrimaryKey = new DataColumn[] { _radicalDataTable.Columns["UC"] };

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _rdbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.RADICALDB);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _radicalDataTable = GetDataTable("db/rdb", true);
            _radicalDataTable.PrimaryKey = new DataColumn[] { _radicalDataTable.Columns["UC"] };

            if (callBack != null)
            {
                callBack(eLoadResType.RADICALDB);
            }
        }
    }

    //加载唐诗库
    private void LoadTSDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_tsdbInited) return;

        _tsdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/tsdb", typeof(TextAsset)) as TextAsset;
            string cyDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _tsDataTable = GetDataTable(cyDBCache, false);

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _tsdbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.TANGSHI);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _tsDataTable = GetDataTable("db/tsdb", true);
            if (callBack != null)
            {
                callBack(eLoadResType.TANGSHI);
            }
        }

    }

    //加载宋词库
    private void LoadSCDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_scdbInited) return;

        _scdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/scdb", typeof(TextAsset)) as TextAsset;
            string cyDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _scDataTable = GetDataTable(cyDBCache, false);

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _scdbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.SONGCI);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _scDataTable = GetDataTable("db/scdb", true);
            if (callBack != null)
            {
                callBack(eLoadResType.SONGCI);
            }
        }
    }

    //加载古诗库
    private void LoadGSDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_gsdbInited) return;

        _gsdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/gsdb", typeof(TextAsset)) as TextAsset;
            string cyDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _gsDataTable = GetDataTable(cyDBCache, false);

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _gsdbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.GUSHI);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _gsDataTable = GetDataTable("db/gsdb", true);
            if (callBack != null)
            {
                callBack(eLoadResType.GUSHI);
            }
        }
    }

    //加载诗经库
    private void LoadSJDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_sjdbInited) return;

        _sjdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/sjdb", typeof(TextAsset)) as TextAsset;
            string cyDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _sjDataTable = GetDataTable(cyDBCache, false);

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _sjdbInited = true;
                        if (callBack != null)
                        {
                            callBack(eLoadResType.SHIJING);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _sjDataTable = GetDataTable("db/sjdb", true);
            if (callBack != null)
            {
                callBack(eLoadResType.SHIJING);
            }
        }
    }

    //加载传统色数据库
    private void LoadColorDB(bool async = false, Action<eLoadResType> callBack = null)
    {

        if (_colordbInited) return;

        _colordbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/colordb", typeof(TextAsset)) as TextAsset;
            string cyDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _colorDataTable = GetDataTable(cyDBCache, false);

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _colordbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.COLOR);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _colorDataTable = GetDataTable("db/colordb", true);
            if (callBack != null)
            {
                callBack(eLoadResType.COLOR);
            }
        }

    }

    private void LoadCaiCYDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_caiCYdbInited) return;

        _caiCYdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/caicy", typeof(TextAsset)) as TextAsset;
            string cyDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _caiCYDataTable = GetDataTable(cyDBCache, false);
                    //设置主键
                    _caiCYDataTable.PrimaryKey = new DataColumn[] { _caiCYDataTable.Columns[MIDI] };//MIDI

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _caiCYdbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.CAICHENGYU);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _caiCYDataTable = GetDataTable("db/caicy", true);
            if (callBack != null)
            {
                callBack(eLoadResType.CAICHENGYU);
            }

            //设置主键
            _caiCYDataTable.PrimaryKey = new DataColumn[] { _caiCYDataTable.Columns[MIDI] };//MIDI
        }
    }

    private void LoadChengYuDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_chengYudbInited) return;

        _chengYudbInited = true;


        if(async)
        {
            TextAsset binAsset = Resources.Load("db/chengyu", typeof(TextAsset)) as TextAsset;
            string cyDBCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _chengYuDataTable = GetDataTable(cyDBCache, false);
                    //设置主键
                    _chengYuDataTable.PrimaryKey = new DataColumn[] { _chengYuDataTable.Columns[CHENGYU] };//MIDI

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _chengYudbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.CHENGYU);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _chengYuDataTable = GetDataTable("db/chengyu", true);
            if (callBack != null)
            {
                callBack(eLoadResType.CHENGYU);
            }

            //设置主键
            _chengYuDataTable.PrimaryKey = new DataColumn[] { _chengYuDataTable.Columns[CHENGYU] };//MIDI
        }

    }

    private void LoadSZMDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_sZMdbInited) return;

        _sZMdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/zmdb", typeof(TextAsset)) as TextAsset;
            string dbCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _sZMDataTable = GetDataTable(dbCache, false);
                    //设置主键
                    _sZMDataTable.PrimaryKey = new DataColumn[] { _sZMDataTable.Columns[MIDI] };//MIDI

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _sZMdbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.SZM);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _sZMDataTable = GetDataTable("db/zmdb", true);
            if (callBack != null)
            {
                callBack(eLoadResType.SZM);
            }

            //设置主键
            _sZMDataTable.PrimaryKey = new DataColumn[] { _sZMDataTable.Columns[MIDI] };//MIDI
        }

    }

    private void LoadSRDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_sRdbInited) return;

        _sRdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/rdb", typeof(TextAsset)) as TextAsset;
            string dbCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _sRDataTable = GetDataTable(dbCache, false);
                    //设置主键
                    _sRDataTable.PrimaryKey = new DataColumn[] { _sRDataTable.Columns[RADICAL] };//MIDI

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        if (callBack != null)
                        {
                            callBack(eLoadResType.SR);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {
            _sRDataTable = GetDataTable("db/rdb", true);
            if (callBack != null)
            {
                callBack(eLoadResType.SR);
            }

            //设置主键
            _sRDataTable.PrimaryKey = new DataColumn[] { _sRDataTable.Columns[RADICAL] };//MIDI
        }

    }

    private void LoadSHZDB(bool async = false, Action<eLoadResType> callBack = null)
    {
        if (_sHZdbInited) return;

        _sHZdbInited = true;


        if (async)
        {
            TextAsset binAsset = Resources.Load("db/shz", typeof(TextAsset)) as TextAsset;
            string dbCache = binAsset.text;

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    _sHZDataTable = GetDataTable(dbCache, false);
                    //设置主键
                    _sHZDataTable.PrimaryKey = new DataColumn[] { _sHZDataTable.Columns[HZ] };//MIDI

                    // 用Loom的方法在Unity主线程中调用ui组件
                    Loom.QueueOnMainThread((param) =>
                    {
                        _sHZdbInited = true;

                        if (callBack != null)
                        {
                            callBack(eLoadResType.SHZ);
                        }
                    }, null);
                });

                thread.Start();
            });
        }
        else
        {

            _sHZDataTable = GetDataTable("db/shz", true);
            //设置主键
            _sHZDataTable.PrimaryKey = new DataColumn[] { _sHZDataTable.Columns[HZ] };//HZ

            if (callBack != null)
            {
                callBack(eLoadResType.SHZ);
            }
        }
    }


    private DataTable GetDataTable(string res,bool needLoad)
    {
        DataTable dataTable = new DataTable();
        string[] lineArray = { };
        if (needLoad)
        {
            TextAsset binAsset = Resources.Load(res, typeof(TextAsset)) as TextAsset;
            //读取每一行的内容  
            lineArray = binAsset.text.Split("\r"[0]);
        }else{
            lineArray = res.Split("\r"[0]);
        }

        //记录每行记录中的各字段内容
        string[] aryLine;
        //标示列数
        int columnCount = 0;
        //标示是否是读取的第一行
        bool IsFirst = true;
        
        //逐行读取CSV中的数据
        foreach (var strLine in lineArray)
        {
            aryLine = CSVstrToAry(strLine);//strLine.Split(',');
            if (IsFirst == true)
            {
                IsFirst = false;
                columnCount = aryLine.Length;
                //创建列
                for (int i = 0; i < columnCount; i++)
                {
                    DataColumn dc = new DataColumn(aryLine[i]);
                    dataTable.Columns.Add(dc);
                }
            }
            else
            {
                //fuck here,末尾多出一个换行
                if (strLine.Length == 0 || strLine.Equals("\n"))
                    continue;

                DataRow dr = dataTable.NewRow();
                for (int j = 0; j < columnCount; j++)
                {
                    //注意：7和8行必须转换为数字
                    if (dataTable.Columns[j].ToString() == FREQ
                       || dataTable.Columns[j].ToString() == CFREQ
                        || dataTable.Columns[j].ToString() == HZCNT)
                    {
                        dr[j] = int.Parse(aryLine[j].Replace("\n", ""));
                    }
                    else
                    {
                        dr[j] = aryLine[j].Replace("\n", "");
                    }
                }
                dataTable.Rows.Add(dr);
            }
        }

        return dataTable;
    }

    private string[] CSVstrToAry(string strLine)
    {
        string strItem = string.Empty;
        int semicolonFlg = 0;//单数时肯定不是某一列的结束位置
        List<string> lstStr = new List<string>();
        string strA = string.Empty;

        for (int i = 0; i < strLine.Length; i++)
        {
            strA = strLine.Substring(i, 1);

            if (strA == "\"") semicolonFlg += 1;

            if (semicolonFlg == 2) semicolonFlg = 0;

            if (strA == "," && semicolonFlg == 0)
            {
                if (strItem.Contains("\""))
                {
                    strItem = strItem.Replace("\"\"", @"""");//CSV中引号也会有转义,单引号会转换为双引号
                    if (strItem.StartsWith("\"", StringComparison.Ordinal)
                        && strItem.EndsWith("\"", StringComparison.Ordinal))
                    {
                        strItem = strItem.Substring(1, strItem.Length - 2);
                    }
                }

                lstStr.Add(strItem);
                strItem = string.Empty;
            }
            else
            {
                strItem += strA;
            }
        }

        if (strItem.Length > 0)
        {
            if (strItem.Contains("\""))
            {
                strItem = strItem.Replace("\"\"", @"""");//CSV中引号也会有转义,单引号会转换为双引号
                if (strItem.StartsWith("\"", StringComparison.Ordinal)
                    && strItem.EndsWith("\"", StringComparison.Ordinal))
                {
                    strItem = strItem.Substring(1, strItem.Length - 2);
                }
            }
            lstStr.Add(strItem);
        }

        return lstStr.ToArray();
    }

    public DataTable GetHZDataTable()
    {
        return _hzDataTable;
    }

    //参数一：字段类型
    //参数二：字段值(可以是部分，比如解释的部分-字段仅限解释/英文等)
    public DataRow[] GetHZ(eCSVColName type,string value){
        DataRow[] drs = {};
        switch (type){
            case eCSVColName.UC:
                drs = _hzDataTable.Select("UC = '" + value + "'");
                break;
            case eCSVColName.HZ:
                drs = _hzDataTable.Select("HZ = '" + value+"'");
                break;
            case eCSVColName.UDEC:
                drs = _hzDataTable.Select("UDEC = '" + value+"'");
                break;
            /*带结构的字段仅仅适合于获取，用于汉字重建，不用于检索：'JGDEC','JGUDEC'*/
            case eCSVColName.PINYIN:
                drs = _hzDataTable.Select("PINYIN LIKE  "
                                          +"'%," + value+ ",%'"
                                          + " OR "
                                          +"'"+value+"'"
                                          +" OR "
                                          + "'" + value + ",%'"
                                          + " OR "
                                          + "'%," + value + "'"
                                         );
                break;
            case eCSVColName.JGID:
                drs = _hzDataTable.Select("JGID = '" + value+"'");
                break;
            case eCSVColName.JIESHI:
                drs = _hzDataTable.Select("JIESHI LIKE  "
                          + "'%" + value + "%'"
                         );
                break;
            case eCSVColName.ENGLISH:
                drs = _hzDataTable.Select("ENGLISH LIKE  "
                                          + "'%" + value + "%'"
                                         );
                break;
            case eCSVColName.CIYU:
                drs = _hzDataTable.Select("CIYU LIKE  "
                                          + "'%" + value + "%'"
                                         );
                break;
        }

        return drs;
    }

    /// <summary>
    /// 随机获取一定数量的汉字
    /// </summary>
    /// <returns>The random radical.</returns>
    /// <param name="num">Number.</param>
    public List<string> GetRandomHZ(int num)
    {
        List<string> hz = new List<string>();

        List<int> indexs = GenerateRandomIntList(num, 0, _hzDataTable.Rows.Count);

        foreach (var index in indexs)
        {
            DataRow dr = _hzDataTable.Rows[index];
            if (dr != null)
            {
                hz.Add(dr[UC].ToString());
            }
        }

        return hz;
    }

    //获取汉字的中文
    //参数：unicode
    //返回：汉字
    public string GetHZ(string uc)
    {
        string hz = "";

        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
             hz = (string)dr[HZ];
        }

        return hz;
    }
    /// <summary>
    /// Gets the udec.
    /// </summary>
    /// <returns>The udec.</returns>
    /// <param name="uc">Uc.</param>
    public string GetUDEC(string uc)
    {
        string udec = "";

        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
            udec = (string)dr[UDEC];
        }

        return udec;
    }

    //获取汉字的中文解释
    //参数：unicode
    //返回：字符串数组，所有解释，不同读音时
    public string[] GetJieShi(string uc){
        string[] jsList = { "无"};

        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
            //这里可以进一步优化，因为解释是"#"分割的
            string js = (string)dr["JIESHI"];
            if(!js.Equals("N/A")){
                jsList = js.Split('#');
            }
        }

        return jsList;
    }

    //获取汉字的英文
    //参数：unicode

    public string[] GetEnglish(string uc)
    {
        string[] enList = { "无"};

        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
            //这里可以进一步优化，因为解释是";"和","分割的,逗号表示相同意思的英文分割
            string en = (string)dr["ENGLISH"];

            if (!en.Equals("N/A"))
            {
                enList = en.Split(';');
            }
        }

        return enList;
    }

    //获取汉字的常用词语
    //参数：unicode
    public string[] GetCiYu(string uc)
    {
        string[] cyList = { "无" };

        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
            //这里可以进一步优化，因为解释是";"和","分割的,逗号表示相同意思的英文分割
            string cy = (string)dr["CIYU"];

            if (!cy.Equals("N/A"))
            {
                cyList = cy.Split(';');
            }
        }

        return cyList;
    }
    //获取汉字的拼音
    //参数：unicode
    public string[] GetPinYin(string uc)
    {
        string[] pyList = { "无" };

        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
            string py = (string)dr["PINYIN"];

            if (!py.Equals("N/A"))
            {
                pyList = py.Split(',');
            }
        }

        return pyList;
    }

    //获取汉字的结构
    //参数：unicode
    public eHZJieGou GetJieGou(string uc)
    {
        eHZJieGou jgType = eHZJieGou.ERROR;

        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
            string jg = (string)dr["JGID"];

            if (!jg.Equals("N/A"))
            {
                jgType = (eHZJieGou)int.Parse(jg);
            }
        }

        return jgType;
    }

    //获取汉字的unicode
    //参数：汉字
    public string GetUC(string hz)
    {
        string uc = "";

        DataRow[] drs = _hzDataTable.Select("hz = '" + hz + "'");

        if (drs.Length != 0)
        {
            string bs = (string)drs[0]["UC"];

            if (!bs.Equals("N/A"))
            {
                uc = bs;
            }
        }

        return uc;
    }

    //判断是否为合法字
    //参数：字的unicode
    public Boolean IsLegalHZ(string uc)
    {
        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
            return false;
        }

        return true;
    }

    //获取字体拆分序列
    //参数：字的unicode
    //返回的拆分序列如果长度==1，说明已经不可以继续拆解，这个信息很有用
    public List<string> GetDecomposedList(string uc, bool containJG = false)
    {
        List<string> dlst = new List<string>();
        DataRow dr = _hzDataTable.Rows.Find(uc);
        if (dr != null)
        {
            string jgudec = dr[containJG?JGUDEC:UDEC].ToString();
            string[] jglst = jgudec.Split(';');

            dlst = jglst.ToList();
        }

        return dlst;
    }

    //获取某个汉字的彻底分解序列
    //参数：uc，cdList：保到的list,containJG:是否包含拆解结构符,默认否

    public void GetCompleteDecomposedList(string uc,List<string> cdList,bool containJG = false)
    {
        List<string> fr = GetDecomposedList(uc, containJG);
        if(fr.Count == 1)
        {
            cdList.Add(fr[0]);

            return;
        }

        foreach(var cmpt in fr){
            if (CheckHZJG(cmpt) != eHZJieGou.ERROR)
            {
                cdList.Add(cmpt);
            }
            else
            {
                GetCompleteDecomposedList(cmpt, cdList);
            }
        }

        return;
    }

    //该函数用于获取到拆分序列时，进行每个项目的判断
    //检查拆分序列的某项是否是结构符，如果是返回结构，如果不是返回err类型
    //返回结构类型为err时，说明是字根
    //参数：拆分序列项
    public eHZJieGou CheckHZJG(string decomposedElement){

        //序列字符长度大于2，说明不是结构字符
        if (decomposedElement.Length <= 2)
        {
            int jgId;
            bool result = int.TryParse(decomposedElement, out jgId);
            if (result)
            {
                return (eHZJieGou)jgId;
            }
        }

        return eHZJieGou.ERROR;
    }

    /// <summary>
    /// 获取该汉字构成其他汉字的数量，如果是0，说明该字已经不能再合成别的字
    /// </summary>
    /// <returns><c>true</c>, if CF req was gotten, <c>false</c> otherwise.</returns>
    public int GetCFreq(string uc){
        int cfreq = 0;

        DataRow dr = _hzDataTable.Rows.Find(uc);

        if (dr != null)
        {
            string tmp = "" + dr[CFREQ];
            cfreq = int.Parse(tmp);
        }

        return cfreq;
    }

    public Boolean CheckIsRadical(string uc){
        return GetRadical(uc).Length != 0;
    }
    //--------------------------玩法&算法定制-------------------------------------
    //获取指定汉字常见度排名范围的汉字，用于某种场景初始化
    //参数：freqH:使用频率排名高，freqL:使用频率排名低,注意：大小是反的，范围1-8000
    //说明：通过此接口获取汉字uc列表后，可以自行取其中部分，然后调用GetDecomposedList接口，获取对应的拆分序列
    public List<string> GetFreqHZList(int freqH,int freqL){
        int freq_h = freqH;
        int freq_l = freqL;

        if(freqH > freqL)
        {
            freq_h = freqL;
            freq_l = freqH;
        }

        List<string> hzlst = new List<string>();

        string cnd = "";
        if(freqL > HZFREQ_LOW){
            cnd = "FREQ >= " + HZFREQ_LOW + "";//非常生僻的字
        }
        else{
            cnd = "FREQ < " + freq_l + " AND FREQ >= " + freq_h;
        }

        DataRow[] drs = _hzDataTable.Select(cnd);


        for (int i = 0; i < drs.Length; i++){
            hzlst.Add(drs[i][UC].ToString());
        }

        return hzlst;
    }

    //------------------------字根玩法定制----------------------------------------
    //获取笔画组组成的字根
    //参数：encodeList-笔画编码序列，complete：是否要求笔画顺序，默认否。要求笔画顺序需要完全匹配
    //现有笔画及其对应的编码如下：
    //'一':'0','丿':'1','丨':'2','㇏':'3','┐':'4','丶':'5','亅':'6','冖':'7','乛':'8','ㄥ':'9','乙':'A','㇉':'B','ㄋ':'C','乁':'D','乀':'E','ㄣ':'F','𠃌':'G','㇇':'H','乚':'I','⺄':'J','レ':'K','ㄑ':'L','㇀':'M','𠃊':'N','ノ':'O'}
    //编码均为字符

    //如果不能构成字根，则返回空，否则返回对应的字根的UC列表，可能有多个
    public List<string> GetCombinedRadicalByENCODE(List<string> encodeList, bool complete = false)
    {
        LoadRes(eLoadResType.RADICALDB);

        List<string> radicalUCList = new List<string>();

        if(complete){
            string encodeStr = string.Join("", encodeList.ToArray());

            DataRow[] drs = _radicalDataTable.Select("ENCODE = '"+ encodeStr + "'");

            foreach (var row in drs)
            {
                radicalUCList.Add(row[UC].ToString());
            }
        }
        else{
            for (int i = 0; i < _radicalDataTable.Rows.Count;i++)
            {
                var ENCODEs = _radicalDataTable.Rows[i][ENCODE].ToString().ToList();
                if (ENCODEs.Count != encodeList.Count) continue;

                ENCODEs.Sort();
                string sortEncodes = new string(ENCODEs.ToArray());

                encodeList.Sort();
                string sortEncodeStr = string.Join("",encodeList.ToArray());

                if(sortEncodeStr.Equals(sortEncodes)){
                    radicalUCList.Add(_radicalDataTable.Rows[i][UC].ToString());
                }
            }
        }

        return radicalUCList;
    }

    //获取字根汉字
    //参数：uc
    public string GetRadical(string uc)
    {
        LoadRes(eLoadResType.RADICALDB);

        DataRow dr = _radicalDataTable.Rows.Find(uc);
        if (dr != null)
        {
            return dr[RADICAL].ToString();
        }
        return "";
    }

    //获取字根的拆分序列，这里只返回编码形式，请勿使用实际笔画，（运算和跨平台都存在不可预知的问题)
    //参数：uc
    //返回拆分序列-编码后的，界面需要自行绑定笔画和编码之间的对应关系
    public List<string> GetRadicalDecomposedList(string uc)
    {
        LoadRes(eLoadResType.RADICALDB);

        List<string> dec = new List<string>();

        DataRow dr = _radicalDataTable.Rows.Find(uc);
        if (dr != null)
        {
            var enl = dr[ENCODE].ToString().ToList();
            foreach(var en in enl){
                dec.Add(""+en);
            }
        }


        return dec;
    }

    //仅测试用
    public string GetRadicalDecomposed(string uc)
    {
        LoadRes(eLoadResType.RADICALDB);

        DataRow dr = _radicalDataTable.Rows.Find(uc);
        if (dr != null)
        {
            return dr[DEC].ToString();
        }


        return "";
    }

    //随机获取一定数量的字根分拆序列
    //参数：num-获取字根数量
    //返回值：所有字根的分拆序列
    public List<string> GetRandomBH(int num){

        LoadRes(eLoadResType.RADICALDB);

        List<string> decomposedList = new List<string>();

        List<int> indexs = GenerateRandomIntList(num,0, _radicalDataTable.Rows.Count);

        foreach(var index in indexs){
            DataRow dr = _radicalDataTable.Rows[index];
            if(dr != null){

                List<char> ecs = dr[ENCODE].ToString().ToList();
                foreach (var ec in ecs)
                {
                    decomposedList.Add(""+ec);
                }
            }
        }

        return decomposedList;
    }
    /// <summary>
    /// 随机获取一定数量的字根
    /// </summary>
    /// <returns>The random radical.</returns>
    /// <param name="num">Number.</param>
    public List<string> GetRandomRadical(int num)
    {
        LoadRes(eLoadResType.RADICALDB);

        List<string> radicals = new List<string>();

        List<int> indexs = GenerateRandomIntList(num, 0, _radicalDataTable.Rows.Count);

        foreach (var index in indexs)
        {
            DataRow dr = _radicalDataTable.Rows[index];
            if (dr != null)
            {
                radicals.Add(dr[UC].ToString());
            }
        }

        return radicals;
    }

    //---------------------------唐诗宋词玩法-------------------------------------
    //原则可以根据玩法返回数据类型，而不是整体返回所有数据，因为数据仍需要一次"#"分割操作
    /*
     *诗词玩法较为广泛，大致可以分为：
     *1，随机一首诗，截取某一句（长度原因，建议只采用一句)，扣掉部分字，让通过组装汉字的形式完成填空
     *2，随机一首诗，显示，让通过组装汉字形式回答：作者、朝代、标题（可以有所提示)
     *3，给定标题，通过组装形式背诵出该诗的一句或者多句，灵活多变
     *4，给定作者，通过组装形式背诵出该作者的一句或者多句诗，可以有背出多句加分等操作
     *5，可以提供附加提示，译文、作者简介等等增值信息
    */
    public enum eShiCi{
        ALL,//所有中随机一个
        TANGSHI,//唐诗
        SONGCI,//宋词
        GUSHI,//古诗
        SHIJING,//诗经
        //...
    }
    /// <summary>
    /// 获取一条诗词记录，界面可以保存记录id，以方便查询
    /// </summary>
    /// <returns>选中的诗词记录列表，依次为：ID、标题、朝代、作者、诗词、译文（不全)、作者简介</returns>
    /// <param name="type">获取诗还是词</param>
    public enum eTSSCColName
    {
        TSSC_ID,//id --基本没有意义，因为诗词分别属于不同的数据库
        TSSC_TiMu,//标题
        TSSC_ChaoDai,//朝代
        TSSC_ZuoZhe,//作者
        TSSC_NeiRong,//正文
        TSSC_YiWen,//译文
        TSSC_ZZJianJie,//作者简介
        TSSC_END,//结束用于外部添加额外信息
    }
    /// <summary>
    /// 获取一条诗词记录，界面可以保存记录id，以方便查询
    /// </summary>
    /// <returns>选中的诗词记录列表，依次为：ID、标题、朝代、作者、诗词、译文（不全)、作者简介</returns>
    /// <param name="type">获取诗还是词</param>
    public List<string> GetTSSC(eShiCi type,int scID = -1/*默认为随机获取一条*/)
    {
        List<string> ret = new List<string>();

        if(type == eShiCi.ALL)
            type = (eShiCi)GenerateRandomInt((int)eShiCi.TANGSHI, (int)eShiCi.SHIJING + 1);

        int index = 0;
        DataRow dr = null;
        if (type == eShiCi.TANGSHI){

            LoadTSDB();
            if (scID != -1){
                if (scID >= 0 && scID < _tsDataTable.Rows.Count){
                    dr = _tsDataTable.Rows[scID];
                }
            }
            else{
                index = GenerateRandomInt(0, _tsDataTable.Rows.Count);
                dr = _tsDataTable.Rows[index];
            }
        }
        else if(type == eShiCi.SONGCI){

            LoadSCDB();
            if (scID != -1){
                if(scID >= 0 && scID < _scDataTable.Rows.Count){
                    dr = _scDataTable.Rows[scID];
                }
            }else{
                index = GenerateRandomInt(0, _scDataTable.Rows.Count);
                dr = _scDataTable.Rows[index];
            }
        }
        else if (type == eShiCi.GUSHI)
        {

            LoadGSDB();
            if (scID != -1)
            {
                if (scID >= 0 && scID < _gsDataTable.Rows.Count)
                {
                    dr = _gsDataTable.Rows[scID];
                }
            }
            else
            {
                index =  GenerateRandomInt(0, _gsDataTable.Rows.Count);
                dr = _gsDataTable.Rows[index];
            }
        }
        else if (type == eShiCi.SHIJING)
        {

            LoadSJDB();
            if (scID != -1)
            {
                if (scID >= 0 && scID < _sjDataTable.Rows.Count)
                {
                    dr = _sjDataTable.Rows[scID];
                }
            }
            else
            {
                index = GenerateRandomInt(0, _sjDataTable.Rows.Count);
                dr = _sjDataTable.Rows[index];
            }
        }

        if (dr != null){
            ret.Add("" + index);
            ret.Add("" + dr[MITI]);
            ret.Add("" + dr[CHAODAI]);
            ret.Add("" + dr[ZUOZHE]);
            ret.Add("" + dr[NEIRONG]);
            ret.Add("" + dr[YIWEN]);
            ret.Add("" + dr[ZZJIANJIE]);
        }

        return ret;
    }

    /// <summary>
    /// 获取格式化后的诗、词
    /// </summary>
    /// <returns>返回诗词语句列表，每项相当于换行</returns>
    /// <param name="tssc">原始诗词</param>
    public List<string> GetFmtShiCi(string tssc){
        List<string> ret = new List<string>();

        //new char[3] { '。', '！','？' }
        ret = tssc.Split('#').ToList();

        return ret;
    }
    /// <summary>
    /// 获取诗句的字数，不包括标点符号
    /// </summary>
    /// <returns>返回汉字数</returns>
    /// <param name="tssc">该诗句为选中的诗</param>
    public int GetHZCnt(string tssc){
        int cnt = 0;

        foreach (var hz in tssc){

            // 此处判断不一定准确
            if (!CheckIsHZ(hz))
            {
                continue;
            }


            //也可以通过判断字的有效性来决定是否是标点符号，更加准确
            //但该函数效率过低，不太允许使用
            /*
            if(GetUC(""+hz) == ""){
                continue;
            }
            */

            cnt++;
        }

        return cnt;
    }
    /// <summary>
    /// 检测诗句的元素是否是汉字还是标点
    /// </summary>
    /// <returns>是否为汉字</returns>
    /// <param name="hz">Hz.</param>
    public Boolean CheckIsHZ(char hz){
        if (hz == '“'
            || hz == '”'
            || hz == '、'
            || hz == '，'
            || hz == '。'
            || hz == '；'
            || hz == '！'
            || hz == '？'
            || hz == '：'
            || hz == '《'
            || hz == '》'
            || hz == '·')
        {
                return false;
        }

        return true;
    }

    public Boolean CheckIsHZ(string hz)
    {
        if (hz == "“"
            || hz == "”"
            || hz == "、"
            || hz == "，"
            || hz == "。"
            || hz == "；"
            || hz == "！"
            || hz == "？"
            || hz == "："
            || hz == "《"
            || hz == "》"
            || hz == "·")
        {
            return false;
        }

        return true;
    }
    public enum eColorType{
        JP,//0
        CN,//1
        BOTH,//2
    };
    //--------------------------中日传统色----------------------------------------
    //该数据库主要用于最终生成的字/句/诗词背景色，以及游戏中的背景色
    //该数据库为经典配色，非常有使用价值。
    //原则可以根据组成的字/诗句去配合对应或者相关的颜色，使其更加具有交互性
    /// <summary>
    /// Gets the color.
    /// </summary>
    /// <returns>颜色信息列表，分别为：中/日名字、拼音/英文、hex,r,g,b,type,id</returns>
    /// <param name="type">Type = 1为中国传统色，0为日本传统色，默认-1为两者随机</param>
    public List<string> GetColor(eColorType type = eColorType.BOTH){

        LoadRes(eLoadResType.COLOR);

        List<string> color = new List<string>();

        if(type == eColorType.BOTH){
            int index  = GenerateRandomInt(0, _colorDataTable.Rows.Count);

            color.Add(_colorDataTable.Rows[index][NAME].ToString());
            color.Add(_colorDataTable.Rows[index][PINYIN].ToString());
            color.Add(_colorDataTable.Rows[index][HEX].ToString());
            color.Add(_colorDataTable.Rows[index][R].ToString());
            color.Add(_colorDataTable.Rows[index][G].ToString());
            color.Add(_colorDataTable.Rows[index][B].ToString());
            color.Add(_colorDataTable.Rows[index][CN].ToString());
            color.Add(""+index);
        }
        else{
       
            DataRow[] drs = _colorDataTable.Select("CN = '" + (int)type  + "'");

            int index = GenerateRandomInt(0, drs.Length);

            color.Add(drs[index][NAME].ToString());
            color.Add(drs[index][PINYIN].ToString());
            color.Add(drs[index][HEX].ToString());
            color.Add(drs[index][R].ToString());
            color.Add(drs[index][G].ToString());
            color.Add(drs[index][B].ToString());
            color.Add(drs[index][CN].ToString());
            color.Add("" + index);
        }

        return color;
    }

    // 根据id获取颜色
    public List<string> GetColorByID(int id){
        LoadRes(eLoadResType.COLOR);
        List<string> color = new List<string>();
        if (id < _colorDataTable.Rows.Count){
            color.Add(_colorDataTable.Rows[id][NAME].ToString());
            color.Add(_colorDataTable.Rows[id][PINYIN].ToString());
            color.Add(_colorDataTable.Rows[id][HEX].ToString());
            color.Add(_colorDataTable.Rows[id][R].ToString());
            color.Add(_colorDataTable.Rows[id][G].ToString());
            color.Add(_colorDataTable.Rows[id][B].ToString());
            color.Add(_colorDataTable.Rows[id][CN].ToString());
            color.Add("" + id);
        }
        else{
            //bug
            color.Add(_colorDataTable.Rows[283][NAME].ToString());
            color.Add(_colorDataTable.Rows[283][PINYIN].ToString());
            color.Add(_colorDataTable.Rows[283][HEX].ToString());
            color.Add(_colorDataTable.Rows[283][R].ToString());
            color.Add(_colorDataTable.Rows[283][G].ToString());
            color.Add(_colorDataTable.Rows[283][B].ToString());
            color.Add(_colorDataTable.Rows[283][CN].ToString());
            color.Add("" + 0);
        }

        return color;
    }

    public int GetCNColorCnt()
    {
        LoadRes(eLoadResType.COLOR);

        DataRow[] drs = _colorDataTable.Select("CN = '1'");

        return drs.Length;
    }
    //-----------------------------成语谜-------------------------------------
    //获取一条猜成语
    public enum eCCYCName{
        CY_ID,//猜成语id
        CY_MITI,//猜成语谜题
        CY_NADU,//猜成语难度
        CY_MIGE,//猜成语谜格
        CY_TISHI,//猜成语-解谜思路
        CY_JIEMI,//猜成语-谜底详解
        CY_MIDI,//猜成语-谜底
        CY_FAYIN,//猜成语-读音
        CY_JIESHI,//猜成语-解释
        CY_ZAOJU,//猜成语-例句
    }

    //检查成语数据库是否加载完成
    public bool GetChengYuDBHasLoad(){
        return _chengYudbInited;
    }

    //获取猜成语数据总条数
    public int GetCCYCnt()
    {
        LoadRes(eLoadResType.CAICHENGYU);
        return _caiCYDataTable.Rows.Count;
    }

    //获取指定索引的猜成语
    public List<string> GetCaiCY(int index)
    {
        List<string> ccys = new List<string>();

        LoadRes(eLoadResType.CAICHENGYU);

        if (index < _caiCYDataTable.Rows.Count)
        {
            ccys.Add("" + index);//id
            ccys.Add(_caiCYDataTable.Rows[index][MITI].ToString());
            ccys.Add(_caiCYDataTable.Rows[index][NANDU].ToString());
            ccys.Add(_caiCYDataTable.Rows[index][MIGE].ToString());
            ccys.Add(_caiCYDataTable.Rows[index][TISHI].ToString());
            ccys.Add(_caiCYDataTable.Rows[index][JIEMI].ToString());
            ccys.Add(_caiCYDataTable.Rows[index][MIDI].ToString());
            ccys.Add(_caiCYDataTable.Rows[index][FAYIN].ToString());
            ccys.Add(_caiCYDataTable.Rows[index][JIESHI].ToString());
            ccys.Add(_caiCYDataTable.Rows[index][ZAOJU].ToString());
        }

        return ccys;
    }

    //获取一条随机猜成语
    public List<string> GetCaiCY()
    {
        LoadRes(eLoadResType.CAICHENGYU);

        List<string> ccys = new List<string>();

        int index = GenerateRandomInt(0, _caiCYDataTable.Rows.Count);

        return GetCaiCY(index);
    }

    //普通成语获取，非猜谜语
    //获取一条成语
    public enum eChengYuCName
    {
        CY_ID,//猜成语id
        CY_CHENGYU,//猜成语谜题
        CY_FAYIN,//猜成语-读音
        CY_JIESHI,//猜成语-解释
        CY_CHUCHU,//猜成语-出处
        CY_ZAOJU,//猜成语-例句
    }
    //获取一条成语
    public List<string> GetChengYu()
    {
        List<string> ccys = new List<string>();

        if(!_chengYudbInited){
            return ccys;
        }

        int index = GenerateRandomInt(0, _chengYuDataTable.Rows.Count);

        ccys.Add("" + index);//id
        ccys.Add(_chengYuDataTable.Rows[index][CHENGYU].ToString());
        ccys.Add(_chengYuDataTable.Rows[index][FAYIN].ToString());
        ccys.Add(_chengYuDataTable.Rows[index][JIESHI].ToString());
        ccys.Add(_chengYuDataTable.Rows[index][CHUCHU].ToString());
        ccys.Add(_chengYuDataTable.Rows[index][ZAOJU].ToString());

        return ccys;
    }


    //注意该返回只包括成语基本信息，不包括猜成语相关信息
    //检查某个四字词语是否是成语
    public List<string> CheckIsChengYu(string cy){

        List<string> ret = new List<string>();

        if (cy.Length != 4) return ret;

        DataRow dr = _chengYuDataTable.Rows.Find(cy);

        if (dr != null)
        {
            ret.Add("" + _chengYuDataTable.Rows.IndexOf(dr));//id
            ret.Add(dr[CHENGYU].ToString());
            ret.Add(dr[FAYIN].ToString());
            ret.Add(dr[JIESHI].ToString());
            ret.Add(dr[CHUCHU].ToString());
            ret.Add(dr[CHUCHU].ToString());//例句多数为空，采用出处
        }
        else
        {
            dr = _caiCYDataTable.Rows.Find(cy);
            if(dr != null)
            {
                ret.Add("" + _caiCYDataTable.Rows.IndexOf(dr));//id
                ret.Add(dr[MIDI].ToString());
                ret.Add(dr[FAYIN].ToString());
                ret.Add(dr[JIESHI].ToString());
                ret.Add(dr[ZAOJU].ToString());
                ret.Add(dr[ZAOJU].ToString());
            }
        }

        return ret;
    }
    //搜索成语
    public void SearchCY(string search,
        Action<List<string>> searchingCallback,
        Action fininshCallback)
    {
        //子线程中进行搜索，不会阻塞ui
        Loom.RunAsync(() =>
        {
            Thread thread = new Thread(() =>
            {
                int cnt = _chengYuDataTable.Rows.Count;
                for (int i = 0; i < cnt; i++)
                {
                    DataRow dr = _chengYuDataTable.Rows[i];

                    //超过一半的字和原成语相同，则认为是正确的查询结果
                    if (CheckIfRightSearchCY(search, dr[CHENGYU].ToString()))
                    {
                        List<string> ret = new List<string>();
                        ret.Add("" + i);//id
                        ret.Add(dr[CHENGYU].ToString());
                        ret.Add(dr[FAYIN].ToString());
                        ret.Add(dr[JIESHI].ToString());
                        ret.Add(dr[CHUCHU].ToString());
                        ret.Add(dr[CHUCHU].ToString());//例句多数为空，采用出处

                        Loom.QueueOnMainThread((param) =>
                        {
                            searchingCallback.Invoke(ret);//检索到汉字
                        }, null);
                    }
                }

                cnt = _caiCYDataTable.Rows.Count;
                for (int i = 0; i < cnt; i++)
                {
                    DataRow dr = _caiCYDataTable.Rows[i];

                    if (CheckIfRightSearchCY(search, dr[MIDI].ToString()))
                    {
                        List<string> ret = new List<string>();
                        ret.Add("" + i);//id
                        ret.Add(dr[MIDI].ToString());
                        ret.Add(dr[FAYIN].ToString());
                        ret.Add(dr[JIESHI].ToString());
                        ret.Add(dr[ZAOJU].ToString());
                        ret.Add(dr[ZAOJU].ToString());

                        Loom.QueueOnMainThread((param) =>
                        {
                            searchingCallback.Invoke(ret);//检索到汉字
                        }, null);
                    }
                }

                Loom.QueueOnMainThread((param) =>
                {
                    fininshCallback.Invoke();//检索完成
                }, null);
            });

            thread.Start();
        });
    }
    private bool CheckIfRightSearchCY(string search, string tcy)
    {
        bool right = true;

        for (int i = 0; i < 4; i++)
        {
            if ("" + search[i] == "～") continue;

            if (search[i] != tcy[i])
            {
                right = false;
                break;
            }
        }

        return right;
    }
    //--------------------------汉字--------------------------------------------
    public enum eSHZCName
    {
        HZ_ID,//汉字id
        HZ_HZ,//汉字
        HZ_JGDEC,//汉字-拆分
        HZ_CFREQ,//汉字-构成其他字数量
        HZ_PINYIN,//汉字-拼音
        HZ_JIESHI,//汉字-解释
        HZ_ENGLISH,//汉字-英文
        HZ_FIND,
        HZ_RCNT,//汉字-组件数，内部使用，外部可以直接计算得出
        HZ_END,
    }

    public int  GetSHZCnt()
    {
        return _sHZDataTable.Rows.Count;
    }
    //获取指定索引的汉字
    public List<string> GetSHZ(int id = -1/*默认随机选一条*/)
    {
        List<string> hzs = new List<string>();

        if (!_sHZdbInited)
        {
            return hzs;
        }

        if (id >= _sHZDataTable.Rows.Count)
        {
            return hzs;
        }


        int index = id;
        if(index == -1)
        {
            index = GenerateRandomInt(0, _sHZDataTable.Rows.Count);
        }

        hzs.Add("" + index);//id
        hzs.Add(_sHZDataTable.Rows[index][HZ].ToString());
        hzs.Add(_sHZDataTable.Rows[index][JGDEC].ToString());
        hzs.Add(_sHZDataTable.Rows[index][CFREQ].ToString());
        hzs.Add(_sHZDataTable.Rows[index][PINYIN].ToString());
        hzs.Add(_sHZDataTable.Rows[index][JIESHI].ToString());
        hzs.Add(_sHZDataTable.Rows[index][ENGLISH].ToString());
        hzs.Add(_sHZDataTable.Rows[index][FIND].ToString());
        hzs.Add(_sHZDataTable.Rows[index][RCNT].ToString());

        return hzs;
    }


    public bool UpdateSHZFindState(bool find, string hz)
    {
        bool ret = false;
        if (!_sHZdbInited)
        {
            return ret;
        }

        DataRow dr = _sHZDataTable.Rows.Find(hz);

        if (dr != null)
        {
            dr[FIND] = find ? "1" : "0";
            ret = true;
        }

        return ret;
    }

    public List<string> GetSHZByHZ(string hz)
    {
        List<string> hzs = new List<string>();

        if (!_sHZdbInited)
        {
            return hzs;
        }


        DataRow dr = _sHZDataTable.Rows.Find(hz);

        if (dr != null)
        {
            hzs.Add("" + _sHZDataTable.Rows.IndexOf(dr));//id
            hzs.Add(dr[HZ].ToString());
            hzs.Add(dr[JGDEC].ToString());
            hzs.Add(dr[CFREQ].ToString());
            hzs.Add(dr[PINYIN].ToString());
            hzs.Add(dr[JIESHI].ToString());
            hzs.Add(dr[ENGLISH].ToString());
            hzs.Add(dr[FIND].ToString());
            hzs.Add(dr[RCNT].ToString());
        }

        return hzs;
    }

    //解决查询效果过低的bug
    public void FixBugGetSHZByJGDEC()
    {
        _sHZDataTable.Select("RCNT = '2'");
    }

    //此处使用主键是拆分序列的数据表
    public List<string> GetSHZByJGDEC(string jgdec,bool needOrder = true)
    {
        List<string> ret = new List<string>();

        string sql = "";

        sql = "RCNT = '" + jgdec.Length + "'";

        //字根数量为2时，直接查询更快速
        if (jgdec.Length == 2)
        {
            if(needOrder)
            {
                sql = sql + " AND JGDEC = '" + jgdec +"'";
            }
            else
            {
                sql = sql + " AND ( JGDEC = '" + jgdec[0] + "" + jgdec[1] + "'"
                        + " OR JGDEC = '" + jgdec[1] + "" + jgdec[0] + "')";
            }

            DataRow[] drs = _sHZDataTable.Select(sql);
            foreach (var dr in drs)
            {
                
                ret.Add("" + _sHZDataTable.Rows.IndexOf(dr));//id
                ret.Add(dr[HZ].ToString());
                ret.Add(dr[JGDEC].ToString());
                ret.Add(dr[CFREQ].ToString());
                ret.Add(dr[PINYIN].ToString());
                ret.Add(dr[JIESHI].ToString());
                ret.Add(dr[ENGLISH].ToString());
                ret.Add(dr[FIND].ToString());
                ret.Add(dr[RCNT].ToString());

                break;//取第一个
            }
        }
        else if (jgdec.Length == 1)//单个的时候是单个字，直接查找字即可
        {
            ret = GetSHZByHZ(jgdec);
        }
        else
        {

            DataRow[] drs = _sHZDataTable.Select(sql);

            foreach (var row in drs)
            {
                if(needOrder)
                {
                    if (row[JGDEC].ToString() == jgdec)
                    {

                        ret.Add("" + _sHZDataTable.Rows.IndexOf(row));//id
                        ret.Add(row[HZ].ToString());
                        ret.Add(row[JGDEC].ToString());
                        ret.Add(row[CFREQ].ToString());
                        ret.Add(row[PINYIN].ToString());
                        ret.Add(row[JIESHI].ToString());
                        ret.Add(row[ENGLISH].ToString());
                        ret.Add(row[FIND].ToString());
                        ret.Add(row[RCNT].ToString());


                        break; //支持把重复的也找出来，这里只取第一个
                    }
                }
                else
                {
                    var JGDECs = row[JGDEC].ToString().ToList();

                    for (int i = 0; i < jgdec.Length; i++)
                    {
                        JGDECs.Remove(jgdec[i]);
                    }

                    if (JGDECs.Count == 0)
                    {

                        ret.Add("" + _sHZDataTable.Rows.IndexOf(row));//id
                        ret.Add(row[HZ].ToString());
                        ret.Add(row[JGDEC].ToString());
                        ret.Add(row[CFREQ].ToString());
                        ret.Add(row[PINYIN].ToString());
                        ret.Add(row[JIESHI].ToString());
                        ret.Add(row[ENGLISH].ToString());
                        ret.Add(row[FIND].ToString());
                        ret.Add(row[RCNT].ToString());


                        break; //支持把重复的也找出来，这里只取第一个
                    }
                }
            }
        }

        return ret;
    }

    public enum eSZMCName
    {
        HZ_ID,//字谜id
        HZ_MITI,//字谜
        HZ_NANDU,//字谜-难度
        HZ_MIDI,//字谜-谜底
        HZ_JIESHI,//字谜-解释
        HZ_END,//外部附加字段，仅外部自行使用
    }

    public int GetSZMCnt()
    {
        if (!_sZMdbInited)
        {
            LoadSZMDB();
        }

        return _sZMDataTable.Rows.Count;
    }

    //暂时不采用无解释的字谜了
    public List<string> GetSZM(int id = -1/*默认随机选一条*/)
    {
        List<string> zms = new List<string>();

        if (!_sZMdbInited)
        {
            LoadSZMDB();
        }

        if (id >= _sZMDataTable.Rows.Count)
        {
            return zms;
        }


        int index = id;
        if (index == -1)
        {
            index = GenerateRandomInt(0, _sZMDataTable.Rows.Count);
        }

        zms.Add("" + index);//id

        if(_sZMDataTable.Rows[index] == null)
        {
            return zms;
        }

        zms.Add(_sZMDataTable.Rows[index][MITI].ToString());
        zms.Add(_sZMDataTable.Rows[index][NANDU].ToString());
        zms.Add(_sZMDataTable.Rows[index][MIDI].ToString());
        zms.Add(_sZMDataTable.Rows[index][JIESHI].ToString());


        return zms;
    }

    public int GetRadicalCnt()
    {
        if (!_sRdbInited)
        {
            LoadSRDB();
        }

        return _sRDataTable.Rows.Count;
    }

    public List<string> GetRadical(int id = -1)
    {
        List<string> ret = new List<string>();

        if (!_sRdbInited)
        {
            LoadSRDB();
        }

        if (id >= _sRDataTable.Rows.Count)
        {
            return ret;
        }


        int index = id;
        if (index == -1)
        {
            index = GenerateRandomInt(0, _sRDataTable.Rows.Count);
        }

        ret.Add("" + index);//id
        ret.Add(_sRDataTable.Rows[index][RADICAL].ToString());
        ret.Add(_sRDataTable.Rows[index][HZCNT]+"");
        ret.Add(_sRDataTable.Rows[index][INHZ].ToString());

        return ret;
    }

    public bool CheckIsSRadical(string radical)
    {
        bool ret = false;

        if (!_sRdbInited)
        {
            return ret;
        }


        DataRow dr = _sRDataTable.Rows.Find(radical);

        if (dr != null)
        {
            ret = true;
        }

        return ret;
    }

    //查询汉字功能
    public enum SearchType
    {
        HZ,
        PY,
        EN,
        GJ,
        CY,
        SX,//拼音首字母缩写功能
    }

    private string[] YY = {
        "ā","á","à","ǎ","ē","é","ě","è","ī","í","ǐ","ì","ō","ó","ǒ","ò","ū","ú","ǔ","ù","ü","ǘ","ǚ","ǜ","ń","ň"
    };
    private char[] ReplaceYY = {
        'a','e','i','o','u','v','n'
    };
    public void SearchHZ(SearchType type,string search,
        Action<List<string>> searchingCallback,
        Action fininshCallback)
    {
        List<string> YuanYin = YY.ToList();
        //子线程中进行搜索，不会阻塞ui
        Loom.RunAsync(() =>
        {
            Thread thread = new Thread(() =>
            {
                //汉字是唯一的，直接查找
                if (type == SearchType.HZ)
                {
                    Loom.QueueOnMainThread((param) =>
                    {
                        searchingCallback.Invoke(GetSHZByHZ(search));//检索到汉字
                    }, null);
                }
                else
                {
                    int cnt = _sHZDataTable.Rows.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        DataRow dr = _sHZDataTable.Rows[i];
                        bool searched = false;

                        switch (type)
                        {
                            case SearchType.PY:
                                string[] pys = dr[PINYIN].ToString().Split('#');
                                //需要替换带音标的为普通字母
                                foreach (var py in pys)
                                {
                                    string tsearch = py;
                                    foreach (var yy in py)
                                    {
                                        if (!((yy >= 'a' && yy <= 'z') || (yy >= 'A' && yy <= 'Z')))
                                        {
                                            //说明是音标
                                            tsearch = tsearch.Replace(yy,ReplaceYY[YuanYin.IndexOf(""+yy) / 4]);
                                            break;
                                        }
                                    }

                                    if (search.ToLower() == tsearch.ToLower())
                                    {
                                        searched = true;
                                        break;
                                    }
                                }
                                break;
                            case SearchType.EN:
                                string[] ens = dr[ENGLISH].ToString().Split('#');
                                foreach (var en in ens)
                                {
                                    if (en == search)
                                    {
                                        searched = true;
                                        break;
                                    }
                                }
                                break;
                            case SearchType.GJ:
                                if (dr[JGDEC].ToString().Contains(search))
                                {
                                    searched = true;
                                }
                                break;
                            case SearchType.CY:
                                if (dr[JIESHI].ToString().Contains("～"+ search)
                                 || dr[JIESHI].ToString().Contains(search + "～"))
                                {
                                    searched = true;
                                }
                                break;

                        }

                        if (searched)
                        {
                            List<string> ret = new List<string>();
                            ret.Add("" + i);
                            ret.Add(dr[HZ].ToString());
                            ret.Add(dr[JGDEC].ToString());
                            ret.Add(dr[CFREQ].ToString());
                            ret.Add(dr[PINYIN].ToString());
                            ret.Add(dr[JIESHI].ToString());
                            ret.Add(dr[ENGLISH].ToString());
                            ret.Add(dr[FIND].ToString());
                            ret.Add(dr[RCNT].ToString());

                            Loom.QueueOnMainThread((param) =>
                            {
                                searchingCallback.Invoke(ret);//检索到汉字
                            }, null);
                        }
                    }
                }

                Loom.QueueOnMainThread((param) =>
                {
                    fininshCallback.Invoke();//检索完成
                }, null);
            });

            thread.Start();
        });
    }
    //搜索汉字拼音首字母缩写序列
    public void SearchHZPY(string search,
                        Action<List<string>> searchingCallback,
                        Action fininshCallback)
    {
        //子线程中进行搜索，不会阻塞ui
        Loom.RunAsync(() =>
        {
            Thread thread = new Thread(() =>
            {
                int cnt = _sHZDataTable.Rows.Count;
                List<string> ret = new List<string>();
                foreach (var spy in search)
                {
                    string retPY = "";
                    for (int i = 0; i < cnt; i++)
                    {
                        DataRow dr = _sHZDataTable.Rows[i];
                        string[] pys = dr[PINYIN].ToString().Split('#');
                        
                        foreach (var py in pys)
                        {
                            if (spy == py[0])
                            {
                                //符合的汉字，添加进去
                                string retHZ = dr[HZ].ToString();
                                if (!retPY.Contains(retHZ))
                                {
                                    retPY += retHZ;
                                }
                                break;
                            }
                        }
                    }
                    ret.Add(retPY);
                    
                }

                Loom.QueueOnMainThread((param) =>
                {
                    searchingCallback.Invoke(ret);//检索到汉字
                }, null);

                Loom.QueueOnMainThread((param) =>
                {
                    fininshCallback.Invoke();//检索完成
                }, null);
            });

            thread.Start();
        });
    }
    /****************************核心算法****************************************/
    //判断传入UCs是否构成某个汉字
    //参数：unicode序列，无顺序要求
    //参数：jgID,即想要得到的汉字的字体结构类型，如果不限制，则默认
    //返回构成的汉字UC，如果不能构成汉字，Count = 0

    //可以支持带结构的uc序列，但二者不能混用。带结构匹配分两种情况：完全匹配以及无顺序全匹配
    public List<string> GetCombinedHZByUC(List<string> unicodeList,eHZJieGou jgID = eHZJieGou.ERROR){
    
        List<string> ret = new List<string>();

        string sql = "";
        string JGIDStr = ((int)jgID).ToString();

        if (jgID == eHZJieGou.ERROR)
        {
            sql = "RCNT = '" + unicodeList.Count + "'";
        }
        else
        {
            sql = "RCNT = '" + unicodeList.Count + "' AND JGID = '" + JGIDStr + "'";
        }

        //字根数量为2时，直接查询更快速
        if (unicodeList.Count == 2)
        {
            sql = sql + " AND ( UDEC = '" + unicodeList[0] + ";" + unicodeList[1] + "'"
                + " OR UDEC = '" + unicodeList[1] + ";" + unicodeList[0] + "')";

            DataRow[] drs = _hzDataTable.Select(sql);
            foreach (var dr in drs)
            {
                ret.Add(dr[UC].ToString());
            }
        }
        else
        {

            DataRow[] drs = _hzDataTable.Select(sql);

            foreach (var row in drs)
            {
                var UDECs = row["UDEC"].ToString().Split(';').ToList();

                for (int i = 0; i < unicodeList.Count; i++)
                {
                    UDECs.Remove(unicodeList[i]);
                }

                if (UDECs.Count == 0)
                {
                    ret.Add(row[UC].ToString());
                    // break; //支持把重复的也找出来
                }
            }
        }

        return ret;
    }

    //考虑到不同环境字符显示以及编码格式问题，建议不要使用此接口，所有检索仅通过unicode来完成
    //该接口仅用于测试
    //传入汉字形式
    public List<string> GetCombinedHZByRadical(List<string> chs, eHZJieGou jgID = eHZJieGou.ERROR)
    {
        List<string> bsIDs = new List<string>();
        DataRow[] drs = { };

        foreach (var ch in chs){
            bsIDs.Add(GetUC(ch));
        }

        return GetCombinedHZByUC(bsIDs, jgID);
    }

    //查询诗词功能
    public void SearchTSSC(eShiCi type, string search,
        Action<List<string>> searchingCallback,
        Action fininshCallback)
    {
        //子线程中进行搜索，不会阻塞ui
        Loom.RunAsync(() =>
        {
            Thread thread = new Thread(() =>
            {
                int cnt = 0;
                if (type == eShiCi.TANGSHI)
                {
                    cnt = _tsDataTable.Rows.Count;
                }
                else if (type == eShiCi.SONGCI)
                {
                    cnt = _scDataTable.Rows.Count;
                }
                else if (type == eShiCi.GUSHI)
                {
                    cnt = _gsDataTable.Rows.Count;
                }
                else if (type == eShiCi.SHIJING)
                {
                    cnt = _sjDataTable.Rows.Count;
                }

                for (int i = 0; i < cnt; i++)
                {
                    DataRow dr = null;
                    if (type == eShiCi.TANGSHI)
                    {
                        dr = _tsDataTable.Rows[i];
                    }
                    else if (type == eShiCi.SONGCI)
                    {
                        dr = _scDataTable.Rows[i];
                    }
                    else if (type == eShiCi.GUSHI)
                    {
                        dr = _gsDataTable.Rows[i];
                    }
                    else if (type == eShiCi.SHIJING)
                    {
                        dr = _sjDataTable.Rows[i];
                    }

                    string[] nrs = dr[NEIRONG].ToString().Split('#');

                    foreach (var tnr in nrs)
                    {
                        bool searched = false;
                        int num = 0;
                        string ttnr = tnr;
                        foreach (var s in search)
                        {
                            for (int n = 0; n < ttnr.Length; n++)
                            {
                                if (s == ttnr[n])
                                {
                                    ttnr = ttnr.Remove(n, 1);
                                    num++;
                                    break;
                                }
                            }
                        }

                        if (num > search.Length / 2) searched = true;

                        if (searched)
                        {
                            List<string> ret = new List<string>();
                            ret.Add("" + i);
                            ret.Add("" + dr[TIMU]);
                            ret.Add("" + dr[CHAODAI]);
                            ret.Add("" + dr[ZUOZHE]);
                            ret.Add("" + dr[NEIRONG]);
                            ret.Add("" + dr[YIWEN]);
                            ret.Add("" + dr[ZZJIANJIE]);
                            ret.Add(tnr);

                            Loom.QueueOnMainThread((param) =>
                            {
                                searchingCallback.Invoke(ret);//检索到汉字
                            }, null);

                            break;
                        }
                    }
                }

                Loom.QueueOnMainThread((param) =>
                {
                    fininshCallback.Invoke();//检索完成
                }, null);
            });

            thread.Start();
        });
    }
    //------------------------自动搜索合法组装序列---------------------------------
    private int X = 7;
    private int Y = 7;
    private int N = 2;
    private int COL = 15;
    private List<string> IDS = null;
    private Action<bool,int> SearchingCb = null;//参数：当前节点是否合法

    /// <summary>
    /// 自动搜索一定区域内合法的笔画/字根组装序列
    /// </summary>
    /// <param name="x">搜索起始点坐标x</param>
    /// <param name="y">搜索起始点坐标y</param>
    /// <param name="n">搜索半径</param>
    /// <param name="col">因为是一维数组表示二维，所以需要提供列数</param>
    /// <param name="ids">整个笔画/字根/字编码数组（可以从界面笔画/字根/字节点获取</param>
    /// <param name="fininshCallback">搜索完成回调，向ui主线程返回搜索结果</param>
    /// <param name="searchingCallback">搜索中回调，如果搜索复杂度较大，可通过该回调给予提示</param>
    public void SearchCombinedHZ(int x,int y,int n,int col,List<string> ids,
                        Action<List<int>> fininshCallback,
                        Action<bool,int> searchingCallback = null)
    {
        //初始化参数
        X = x;
        Y = y;
        N = n;
        COL = col;

        if (IDS != null){
            IDS.Clear();
        }else{
            IDS = new List<string>();
        }
        IDS.AddRange(ids);
        SearchingCb = searchingCallback;

        List<int> xy = new List<int>();
        for (int i = 0; i < ids.Count; i++)
        {
            xy.Add(0);//全部设置为未搜索
        }
        xy[Y * COL + X] = 1;

        List<int> searched = new List<int>
        {
            Y * COL + X
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

    private List<int> GetNextSearch(List<int> xy, int x, int y)
    {
        List<int> searching = new List<int>();
        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= COL) continue;
            if (i <= X - N || i >= X + N) continue;

            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= COL) continue;// 超出边界
                if (j <= Y - N || j >= Y + N) continue;//超出搜索半径

                if (i == x && j == y) continue;//同一个点

                if (xy[j * COL + i] == 0)
                {
                    searching.Add(j * COL + i);
                }
            }
        }

        return searching;
    }


    private bool DoSearch(List<int> xy, List<int> searching, List<int> searched, int x, int y)
    {
        //根节点
        if (searching == null)
        {
            return DoSearch(xy, GetNextSearch(xy, x, y), searched, x, y);
        }

        //检测是否合成字根/字
        if (CheckCurrent(searched))
        {
            return true;
        }

        //去除不符合规则的searching
        string fst = IDS[searched[0]];
        for (int i = searching.Count - 1; i >= 0; i--)
        {
            //当前点和第一个搜索点不是一个类型的笔画/字根/字
            string curt = IDS[searching[i]];

            if (fst.Length != curt.Length)
            {
                searching.RemoveAt(i);//删除
            }
        }

        //没有可以搜索的，终止//已经到达死角或者终结
        if (searching.Count == 0 || searched.Count >= N * N /*范围内全部节点*/)
        {
            return false;
        }

        //对当前的进行检测
        foreach (var sc in searching)
        {
            int xx = sc % COL;
            int yy = sc / COL;

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

            if (DoSearch(xy, GetNextSearch(xy, xx, yy), searched, xx, yy))
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

    private bool CheckCurrent(List<int> searched)
    {
        List<string> selectBHList = new List<string>();

        foreach (var sced in searched)
        {
            selectBHList.Add(IDS[sced]);
        }

        List<string> cbHZ = new List<string>();
        if (selectBHList[0].Length == 1) // 是不是笔画
        {
            cbHZ = GetCombinedRadicalByENCODE(selectBHList);
        }
        else
        {
            //根节点如果不是笔画就不作为单独的合成字
            if(selectBHList.Count == 1)
            {
                return false;
            }else{
                cbHZ = GetCombinedHZByUC(selectBHList);
            }
        }

        if (cbHZ.Count != 0)
        {
            return true;
        }

        return false;
    }

    //-----------------------一些功能性接口---------------------------------------

    /// <summary>
    /// 获取所有可能bsid组合
    /// </summary>
    /// <returns>合成的所有笔顺id</returns>
    /// <param name="list">List.</param>
    /// <param name="s">S.</param>
    public List<string> getAllSortList(List<string> list, string s)
    {
        if (list.Count == 1)
        {
            List<string> tmpList = new List<string>();
            string tmps = s + list[0];
            tmpList.Add(tmps);
            return tmpList;
        }
        List<string> allList = new List<string>();
        for (int i = 0; i < list.Count; i++)
        {
            List<string> tmpList = new List<string>();
            tmpList.AddRange(list);
            string tmp = tmpList[i];
            tmpList.RemoveAt(i);
            List<string> tmpList2 = getAllSortList(tmpList, s + tmp);
            allList.AddRange(tmpList2);
        }
        return allList;
    }

    /// <summary>
    ///生成指定数量的随机码（数字）
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public List<int> GenerateRandomIntList(int num,int min,int max)
    {
        List<int> randomIntList = new List<int>();
        for (var i = 0; i < num; i++)
        {
            randomIntList.Add(GenerateRandomInt(min,max));
        }
        return randomIntList;
    }

    /// <summary>
    /// 生成不重复的随机数序列
    /// </summary>
    /// <returns>返回生成列表，存在效率问题，对于没有不重复要求的请使用上面的接口</returns>
    /// <param name="num">数量</param>
    /// <param name="min">最小值.</param>
    /// <param name="max">最大值</param>
    public List<int> GenerateRandomNoRptIntList(int num,int min,int max){
        Hashtable hashtable = new Hashtable();
        for (int i = 0; hashtable.Count < num; i++)
        {
            int nValue = GenerateRandomInt(min,max);
            if (!hashtable.ContainsValue(nValue))
            {
                hashtable.Add(nValue, nValue);
            }
        }

        List<int> ret = new List<int>();
        foreach(int k in hashtable.Keys){
            ret.Add(k);
        }

        return ret;
    }

    /// <summary>
    /// 获取单个随机数
    /// </summary>
    /// <returns>The random int.</returns>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Max.</param>
    public int GenerateRandomInt(int min, int max)
    {
        var r = new System.Random(Guid.NewGuid().GetHashCode());
        return r.Next(min, max);
    }

    /// <summary>
    /// 随机排列数组元素
    /// </summary>
    /// <param name="myList"></param>
    /// <returns></returns>
    public  List<string> ListRandom(List<string> myList)
    {
        int index = 0;
        string temp = "";
        for (int i = 0; i < myList.Count; i++)
        {

            index = GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
        return myList;
    }

    //------------------------测试使用，检测代码耗时-------------------------------
    private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public void StartWatch()
    {
        stopwatch.Reset();
        stopwatch.Start(); // 开始监视代码运行时间
    }

    public void StopWatch(string step = "")
    {
        stopwatch.Stop(); // 停止监视
        TimeSpan timespan = stopwatch.Elapsed; // 获取当前实例测量得出的总时间
        double hours = timespan.TotalHours; // 总小时
        double minutes = timespan.TotalMinutes; // 总分钟
        double seconds = timespan.TotalSeconds; // 总秒数
        double milliseconds = timespan.TotalMilliseconds; // 总毫秒数

        Debug.Log(step+"耗时：" + seconds);
    }

}