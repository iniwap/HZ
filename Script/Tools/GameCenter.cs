// -----------------------------------------------
// Documentation: http://www.reign-studios.net/docs/unity-plugin/
// -----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Reign;
using UnityEngine.Events;

public class GameCenter : MonoBehaviour
{
	private static bool created;

	public GameObject  ReignScores_ClassicRenderer;

    public enum AchievementType{
        HZTongSheng,
        HZXiuCai,
        HZJuRen,
        HZGongShi,
        HZJinShi,
        HZTanHua,
        HZBangYan,
        HZZhuangYuan,
    }
    public enum LeaderboardType{
        HZLLX,
        HZCCK,
    }
    // ======================================================
    // NOTE about users confused over Reign-Scores
    // Reign-Scores is an API target option, just as GooglePlay version GamceCircle is.
    // Reign-Scores is NOT required to use native services like GooglePlay, GameCenter ect.
    // Its a self-hosted option you can put on any ASP.NET server you own for platforms like WP8, BB10 ect.
    // ======================================================
    void Start()
	{
    
		// make sure we don't init the same Score data twice
		if (created) return;
		created = true;


		// Leaderboards ---------------------------
		var leaderboards = new LeaderboardDesc[2]; //这里必须增加，如果增加了榜单
        string[] lbs = { "划一划", "猜一猜"};
        for (int i = 0; i < leaderboards.Length; i++)
        {
            var leaderboard = new LeaderboardDesc();
            leaderboards[i] = leaderboard;
            var reignScores_LeaderboardID = new System.Guid("f55e3210-a4fb-4223-ae4f-11b00aaa63b" + i);
            leaderboard.SortOrder = LeaderboardSortOrders.Ascending;
            leaderboard.ScoreFormat = LeaderbaordScoreFormats.Numerical;
            leaderboard.ScoreFormat_DecimalPlaces = 0;
#if UNITY_IOS
            leaderboard.ScoreTimeFormat = LeaderboardScoreTimeFormats.Centiseconds;
#else
		leaderboard.ScoreTimeFormat = LeaderboardScoreTimeFormats.Milliseconds;
#endif

            // Global
            leaderboard.ID = ""+((LeaderboardType)i);// Any unique ID value you want
            leaderboard.Desc = lbs[i];// Any desc you want

            // Editor
            leaderboard.Editor_ReignScores_ID = reignScores_LeaderboardID;// Any unique value

            // iOS
            leaderboard.iOS_ReignScores_ID = reignScores_LeaderboardID;// Any unique value
            leaderboard.iOS_GameCenter_ID = "" + ((LeaderboardType)i);// Set to your GameCenter leaderboard ID

            // Android
            leaderboard.Android_ReignScores_ID = reignScores_LeaderboardID;// Any unique value
            leaderboard.Android_GooglePlay_ID = "";// Set to your GooglePlay leaderboard ID (Not Name)
            leaderboard.Android_GameCircle_ID = "";// Set to your GameCircle leaderboard ID (Not Name)

        }
		// Achievements ---------------------------
		var achievements = new AchievementDesc[8];
        string[] achName = {"童生","秀才","举人","贡士","进士","探花","榜眼","状元"};
        string[] achDes = { "恭喜成为童生！即刚开始学习的小孩，汉字新手。",
        "恭喜成为秀才！即最低级的考试合格者称为秀才，汉字入门。",
        "恭喜成为举人！生员（秀才）应三年一度的乡试，合格者称为举人，汉字初学者。",
        "恭喜成为贡士！参加全国范围科举考试（会试）及格后获得的资格，汉字中级学者。",
        "恭喜晋升为进士！举人参加在北京的会试殿试，合格者称为进士，汉字高级学者。",
        "恭喜争得探花！殿试第三名称探花，汉字大佬。",
        "恭喜获得榜眼！殿试第二名称榜眼，汉字巨擘。",
        "恭喜荣登状元！！殿试第一名称状元，汉字至尊！"};
        for (int i = 0; i < achievements.Length; i++)
        {
            var achievement = new AchievementDesc();
            achievements[i] = achievement;
            var reignScores_AchievementID = new System.Guid("122ce53d-4a17-520f-a4fb-804ad38be87" + i);

            // Global
            achievement.ID = ""+ ((AchievementType)i);// Any unique ID value you want
            achievement.Name = achName[i];// Any name you want
            achievement.Desc = achDes[i];// Any desc you want

            // When you report an achievement you pass a PercentComplete value.
            // Example: This allows you to change that ratio to something like (0-1000) before the achievement is unlocked.
            achievement.PercentCompletedAtValue = 100;// NOTE: For GooglePlay you must match this value in the developer dashboard under "How many steps are needed?" option.

            // Mark if you want Achievement to use PercentCompleted value or not.
            // Marking this true will make the "PercentComplete" value irrelevant.
            achievement.IsIncremental = true;

            // Editor
            achievement.Editor_ReignScores_ID = reignScores_AchievementID;// Any unique value

            // iOS
            achievement.iOS_ReignScores_ID = reignScores_AchievementID;// Any unique index value
            achievement.iOS_GameCenter_ID = "" + ((AchievementType)i);// Set to your GameCenter achievement ID

            // Android
            achievement.Android_ReignScores_ID = reignScores_AchievementID;// Any unique value
            achievement.Android_GooglePlay_ID = "";// Set to your GooglePlay achievement ID (Not Name)
            achievement.Android_GameCircle_ID = "";// Set to your GameCircle achievement ID (Not Name)
        }
		// Desc ---------------------------
		const string reignScores_gameID = "B2A24047-5201-41C4-B151-0F175BB54D0E";// Get this ID from the Reign-Scores Console.
		var desc = new ScoreDesc();

        desc.ReignScores_UI = ReignScores_ClassicRenderer.GetComponent<MonoBehaviour>() as IScores_UI;
		desc.ReignScores_UI.ScoreFormatCallback += scoreFormatCallback;
		desc.ReignScores_ServicesURL = "http://localhost:5537/Services/";// Set to your server!
		desc.ReignScores_GameKey = "04E0676D-AAF8-5201-A584-DE0C1D618D84";// Set to your servers game_api_key!
		desc.ReignScores_UserKey = "CE8E55E1-F383-4F05-5201-5C89F27B7FF2";// Set to your servers user_api_key!


		desc.LeaderboardDescs = leaderboards;
		desc.AchievementDescs = achievements;

		// Editor
		desc.Editor_ScoreAPI = ScoreAPIs.ReignScores;
		desc.Editor_ReignScores_GameID = reignScores_gameID;

		// iOS
		desc.iOS_ScoreAPI = ScoreAPIs.GameCenter;
		desc.iOS_ReignScores_GameID = reignScores_gameID;

		// Android
		#if GOOGLEPLAY
		desc.Android_ScoreAPI = ScoreAPIs.GooglePlay;
		desc.Android_GooglePlay_DisableUsernameRetrieval = false;// This lets you remove the android.permission.GET_ACCOUNTS requirement if enabled
		#elif AMAZON
		desc.Android_ScoreAPI = ScoreAPIs.GameCircle;
		#else
		desc.Android_ScoreAPI = ScoreAPIs.ReignScores;
		#endif
		desc.Android_ReignScores_GameID = reignScores_gameID;

		// init
		ScoreManager.Init(desc, createdCallback);

		// <<< Reign-Scores manual methods >>>
		//ScoreManager.RequestScores(...);
		//ScoreManager.RequestAchievements(...);
		//ScoreManager.ManualLogin(...);
		//ScoreManager.ManualCreateUser(...);
	}

	private void LogoutButton_Clicked()
	{
		ScoreManager.Logout();
	}


    //重置用户成就进度，可用于删除app的情况
    public void OnResetUserAchievementsProgress(){
#if UNITY_EDITOR

#elif UNITY_IOS
        ScoreManager.ResetUserAchievementsProgress(resetUserAchievementsCallback);
#endif
    }

    //每种题目类型的最高得分
    public void OnReportScore(long score, LeaderboardType type)
    {
#if UNITY_EDITOR

#elif UNITY_IOS
        ScoreManager.ReportScore("" + type, score, reportScoreCallback);
#endif
    }

    //连续闯关数 - 不分题目类型
    public void OnReportAchievement(float percent, AchievementType type)
    {
#if UNITY_EDITOR

#elif UNITY_IOS
        ScoreManager.ReportAchievement("" + type, percent, reportAchievementCallback);
#endif
    }

    public void OnShowLeaderboards()
    {
#if UNITY_EDITOR

#elif UNITY_IOS
        ScoreManager.ShowNativeScoresPage("" + LeaderboardType.HZLLX, showNativePageCallback);
#endif
    }

    public void OnShowAchievements()
    {
#if UNITY_EDITOR

#elif UNITY_IOS
        ScoreManager.ShowNativeAchievementsPage(showNativePageCallback);
#endif
    }

    private void createdCallback(bool success, string errorMessage)
	{
		if (!success) Debug.LogError(errorMessage);
		else ScoreManager.Authenticate(authenticateCallback);
	}

	private void scoreFormatCallback(long score, out string scoreValue)
	{
		scoreValue = System.TimeSpan.FromSeconds(score).ToString();
	}

	private void authenticateCallback(bool succeeded, string errorMessage)
	{
		if (!succeeded && errorMessage != null) Debug.LogError(errorMessage);
        if (succeeded)
        {
            ScoreManager.RequestAchievements(requestAchievementsCallback);
            //ScoreManager.RequestScores("PanDuan",0,10,requestScoressCallback);
        }
	}


    [System.Serializable] public class OnRequestAchievementsCallbackEvent : UnityEvent<Achievement[]> { }
    public OnRequestAchievementsCallbackEvent OnRequestAchievementsCallback;

    private void requestAchievementsCallback(Achievement[] achievements, bool succeeded, string errorMessage)
    {
        if (succeeded)
        {
            Debug.Log("Got Achievement count: " + achievements.Length);
            foreach (var achievement in achievements)
            {
                Debug.Log(string.Format("Achievement {0} PercentCompleted {1}", achievement.ID, achievement.PercentComplete));
            }
            OnRequestAchievementsCallback.Invoke(achievements);
        }
        else
        {
            string error = "Request Achievements Error: " + errorMessage;
            Debug.LogError(error);
        }
    }

    private void requestScoressCallback(LeaderboardScore[] scores, bool succeeded, string errorMessage)
    {
        if (succeeded)
        {
            Debug.Log("Got Scores count: " + scores.Length);
            foreach (var score in scores)
            {
                Debug.Log(string.Format("Username {0} Score {1}", score.Username, score.Score));
            }
        }
        else
        {
            string error = "Request Scores Error: " + errorMessage;
            Debug.LogError(error);
        }
    }

    void showNativePageCallback(bool succeeded, string errorMessage)
	{
		Debug.Log("Show Native Page: " + succeeded);
		if (!succeeded)
		{
			Debug.LogError(errorMessage);
		}
	}


    [System.Serializable] public class OnReprotScoreCallbackEvent : UnityEvent<bool> { }
    public OnReprotScoreCallbackEvent OnReprotScoreCallback;
    void reportScoreCallback(bool succeeded, string errorMessage)
	{
		Debug.Log("Report Score Done: " + succeeded);
		if (!succeeded)
		{
			Debug.LogError(errorMessage);
		}

        OnReprotScoreCallback.Invoke(succeeded);

    }

    [System.Serializable] public class OnReprotAchievementCallbackEvent : UnityEvent<bool> { }
    public OnReprotAchievementCallbackEvent OnReprotAchievementCallback;
    void reportAchievementCallback(bool succeeded, string errorMessage)
	{
		Debug.Log("Report Achievement Done: " + succeeded);
		if (!succeeded)
		{
			Debug.LogError(errorMessage);
		}

        OnReprotAchievementCallback.Invoke(succeeded);
	}


    [System.Serializable] public class OnResetAchievementsCallbackEvent : UnityEvent<bool> { }
    public OnResetAchievementsCallbackEvent OnResetAchievementsCallback;
    void resetUserAchievementsCallback(bool succeeded, string errorMessage)
    {
        Debug.Log("Reset User Achievement Done: " + succeeded);
        if (!succeeded)
        {
            Debug.LogError(errorMessage);
        }

        OnResetAchievementsCallback.Invoke(succeeded);
    }
}
