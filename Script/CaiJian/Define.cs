//常量定义
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

public static class Define
{
    public enum SWIPE_TYPE{
        LEFT,
        RIGHT,
        UP,
        DOWN,
    }

    public static Color FINDE_FONT_COLOR = new Color(106/255f,107/255f,111/255f);

    public static Color BG_COLOR = new Color(31 / 255.0f, 32 / 255.0f, 34 / 255.0f);
    public static Color YELLOW = new Color(230 / 255.0f, 192 / 255.0f, 38 / 255.0f);
    public static Color DARK_YELLOW = new Color(252 / 255.0f, 187 / 255.0f, 79 / 255.0f);
    public static Color GRAY_STAR = new Color(178 / 255.0f, 178 / 255.0f, 178 / 255.0f);
    public static Color FONT_COLOR_LIGHT = new Color(224 / 255.0f, 224 / 255.0f, 226 / 255.0f);
    public static Color FONT_COLOR_DARK = new Color(146 / 255.0f, 147 / 255.0f, 151 / 255.0f);
    public static Color BG_COLOR_DARK = new Color(50 / 255.0f, 51 / 255.0f, 53 / 255.0f);

    public static Color RED = new Color(255 / 255.0f, 87 / 255.0f, 86 / 255.0f);
    public static Color GREEN = new Color(86 / 255.0f, 192 / 255.0f, 44 / 255.0f);
    public static Color BLUE = new Color(67 / 255.0f, 166 / 255.0f, 255 / 255.0f);
    public static Color PURPLE = new Color(212 / 255.0f, 132 / 255.0f, 255 / 255.0f);

    //背景明暗色的时候行分割线的颜色，这里用于默认情况
    public static Color DARKBG_SP_COLOR = new Color(20 / 255.0f, 20 / 255.0f, 20 / 255.0f, 200/255.0f);
    public static Color LIGHTBG_SP_COLOR = new Color(125 / 255.0f, 18 / 255.0f, 18 / 255.0f, 1.0f);

    public enum eFontAlphaType{
        FONT_ALPHA_255,
        FONT_ALPHA_200,
        FONT_ALPHA_128,
        FONT_ALPHA_50,
        FONT_ALPHA_0,
    }

    public static int[] ACHIEVEMENT_SCORE = {10/*童生*/,50/*秀才*/,100/*举人*/,500/*贡士*/,1000/*进士*/,2000/*探花*/,5000/*榜眼*/,8000/*状元*/};

    public static int MAX_LIKE_NUM = 10;

    public static int BASE_SCORE_10S = 10;//答题时间10s的基础分为10分
    public static int PLUS_SCORE_ALL = 3;
    public static int PLUS_SCORE_TANGSHI = 0;
    public static int PLUS_SCORE_SONGCI = 1;
    public static int PLUS_SCORE_GUSHI = 1;
    public static int PLUS_SCORE_SHIJING = 2;

    //排序模式，题目范围额外加分和其他模式不同
    public static int PX_PLUS_SCORE_TANGSHI = 1;
    public static int PX_PLUS_SCORE_SONGCI = 2;
    public static int PX_PLUS_SCORE_SHIJING = 0;

    public static int FREEUSE_DAAN_PER_DAY = 6;
    public static int FREEUSE_DZTS6_PER_DAY = 6;
    public static int FREEUSE_DZTS12_PER_DAY = 12;
    public static int FREEUSE_DZTSFREE_PER_DAY = 3;

    public static float PAI_XU_HZ_DOWNLEFT_SPEED = 1.0f;
    public static float PAI_XU_HZ_MOVECENTER_SPEED = 4.0f;

    public static int MAX_XT_HZ_MATRIX = 8;

    public static int XT_NO_TIP = 50;

    public static int MAX_TRY_CNT = 3;

    public static int BASIC_TIP_CNT = 3;

    public static int DEFAULT_START_COLORID = 283;

    public static int DEFAULT_HS_SIZE = 6;
    public static int MIN_DEFAULT_HS_SIZE = 0;//->0
    public static int MAX_DEFAULT_HS_SIZE = 24;//->2

    public static string FIND_HZ_TABLE_NAME = "findhz";

    public static Color GetLightColor(Color c,bool needAlpha = false){
        Color lc = c * 1.1f;

        if(lc.r > 1.0f && lc.g > 1.0f && lc.b > 1.0f)
        {
            lc = new Color(0.96f, 0.96f, 0.96f, lc.a);
        }

        if(!needAlpha){
            lc = new Color(lc.r, lc.g, lc.b, 1.0f);
        }

        return lc;
    }

    public static Color GetDarkColor(Color c, bool needAlpha = false)
    {
        Color lc = c * 0.9f;
        if (!needAlpha)
        {
            lc = new Color(lc.r, lc.g, lc.b, 1.0f);
        }
        return lc;
    }

    public static Color GetUIFontColorByBgColor(Color bgColor, eFontAlphaType fa)
    {
        Color c = Color.black;

        float l = 0.3f * bgColor.r + 0.6f * bgColor.g + 0.1f * bgColor.b;

        float a = 1.0f;
        switch(fa){
            case eFontAlphaType.FONT_ALPHA_0:
                a = 0.0f;
                break;
            case eFontAlphaType.FONT_ALPHA_128:
                a = 0.5f;
                break;
            case eFontAlphaType.FONT_ALPHA_200:
                a = 200/255.0f;
                break;
            case eFontAlphaType.FONT_ALPHA_255:
                a = 1.0f;
                break;
            case eFontAlphaType.FONT_ALPHA_50:
                a = 50/255.0f;
                break;
        }


        //进一步修正alpha
        float b = GetBrightness(bgColor);
        if(b > 100.0f / 255.0f && b < 150.0f / 255.0f){
            a = b + 50 / 255.0f;
        }

        //亮色的时候使用黑色字体
        if(GetIfUIFontBlack(bgColor))
        {
            c = new Color(50 / 255.0f, 50 / 255.0f, 50 / 255.0f, a);
        }else{
            c = new Color(200 / 255.0f, 200 / 255.0f, 200 / 255.0f, a);
        }

        return c;
    }

    public static float GetBrightness(Color bgColor)
    {
        float l = 0.3f * bgColor.r + 0.6f * bgColor.g + 0.1f * bgColor.b;
        return l;
    }

    public static bool GetIfUIFontBlack(Color bgColor)
    {
        if(GetBrightness(bgColor) > 100/255.0f){//亮度低于100才使用白色字体，否则有些刺眼

            return true;
        }

        return false;
    }

    public static Color GetFixColor(Color c){
        Color bc = c;
        float off = 100 / 255.0f;
        if (GetBrightness(c) < off)
        {
            bc = new Color(c.r + off / 4, c.g + off / 4, c.b + off / 4, c.a);
        }

        return bc;
    }
    /// <summary>
    /// 获取网络日期时间
    /// </summary>
    /// <returns></returns>
    public static void GetNetDateTime(Action <string> cb)
    {
        WebRequest request = null;
        WebResponse response = null;
        WebHeaderCollection headerCollection = null;
        string datetime = string.Empty;
        try
        {
            request = WebRequest.Create("http://www.baidu.com");
            request.Timeout = 3000;
            //request.UseDefaultCredentials = true;
            //request.Credentials = CredentialCache.DefaultCredentials;
            request.Credentials = CredentialCache.DefaultNetworkCredentials;

            response = request.GetResponse();
            headerCollection = response.Headers;
            foreach (var h in headerCollection.AllKeys)
            { 
                if (h == "Date") 
                { 
                    datetime = headerCollection[h]; 
                } 
            }
            cb(datetime);
        }
        catch (Exception) 
        {
            cb(datetime); 
        }
        finally
        {
            if (request != null)
            { 
                request.Abort(); 
            }
            if (response != null)
            { 
                response.Close();
            }
            if (headerCollection != null)
            { 
                headerCollection.Clear(); 
            }
        }
    }

    public static string Post(string url, Dictionary<string, string> dic)
    {
        string result = "";
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        req.Timeout = 3000;
        req.Credentials = CredentialCache.DefaultNetworkCredentials;

        req.Method = "POST";
        req.ContentType = "application/x-www-form-urlencoded";
        #region 添加Post 参数
        StringBuilder builder = new StringBuilder();
        int i = 0;
        foreach (var item in dic)
        {
            if (i > 0)
                builder.Append("&");
            builder.AppendFormat("{0}={1}", item.Key, item.Value);
            i++;
        }
        byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
        req.ContentLength = data.Length;
        using (Stream reqStream = req.GetRequestStream())
        {
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
        }
        #endregion
        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        Stream stream = resp.GetResponseStream();
        //获取响应内容
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            result = reader.ReadToEnd();
        }
        return result;

    }

    //生成正态分布的随机数
    public static double RandGauss(double u, double d)
    {
        double u1, u2, z, x;
        if (d <= 0)
        {

            return u;
        }
        u1 = (new System.Random(GetRandomSeed())).NextDouble();
        u2 = (new System.Random(GetRandomSeed())).NextDouble();

        z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);

        x = u + d * z;
        return x;
    }

    public static int GetRandomSeed()
    {
        byte[] bytes = new byte[4];
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        rng.GetBytes(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    public static Color GetTextColorByFreq(int freq)
    {
        Color c = FONT_COLOR_LIGHT;
        if (freq <= 100)
        {
            c = RED;
        }
        else if (freq > 100 && freq <= 500)
        {
            c = YELLOW;
        }
        else if (freq > 500 && freq <= 1000)
        {
            c = BLUE;
        }
        else if (freq > 1000 && freq <= 2000)
        {
            c = GREEN;
        }
        else if (freq > 2000 && freq <= 5000)
        {
            c = PURPLE;
        }

        return c;
    }

    public static int GetIntColor(float c)
    {
        return (int)(c * 255);
    }
    public static string GetHexColor(Color c)
    {
        return "#" + GetIntColor(c.r).ToString("X2") + GetIntColor(c.g).ToString("X2") + GetIntColor(c.b).ToString("X2");
    }
}
