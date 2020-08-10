/**************************************/
//FileName: FitUI.cs
//Author: wtx
//Data: 05/06/2018
//Describe:  多分辨适配
/**************************************/
using System;
using UnityEngine;
using DG.Tweening;


public class FitUI: MonoBehaviour{

	[System.Serializable]
	public enum UI_TYPE{
		TOP_MENU,
        MID_MENU,
		BOTTOM_MENU,
	};

	[System.Serializable]
	public enum PANEL_TYPE{
		MAIN_PANEL,
		SETTING_PANEL,
        MASK_PANEL,
        SPLASH_PANEL,
        LIKE_PANEL,
	};

    public bool topNeedFixIphoneX = true;

	public UI_TYPE _uiType;
	public PANEL_TYPE _panelType;


    public GameObject _topMenu;//setting panel need
    public GameObject _bottomMenu;//setting panel need


    // Use this for initialization
    void Start () {
        WIDTH = Screen.width;
        HEIGHT = Screen.height;
        OnFitUI();
	}

	void OnDestroy(){
		
	}
	void OnEnable(){

	}
	void OnDisable(){
		
	}


	public void Update()
	{
	}

    public static float DESIGN_WIDTH = 1080.0f;
    public static float DESIGN_HEIGHT = 1920.0f;

    private int WIDTH;
    private int HEIGHT;
    private void OnFitUI(){
        switch(_uiType){
            case UI_TYPE.TOP_MENU:

                float sx = gameObject.transform.GetComponent<RectTransform>().rect.width/ DESIGN_WIDTH;
                gameObject.transform.localScale = new Vector3(sx, sx, gameObject.transform.localScale.z);
                if(getIsIPhoneX()){
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                gameObject.transform.position.y - IPHONEX_TOP_OFFSET,
                                                               gameObject.transform.position.z);
                }
                break;
            case UI_TYPE.MID_MENU:
                if(_panelType == PANEL_TYPE.SETTING_PANEL){
                    float sxb = gameObject.transform.GetComponent<RectTransform>().rect.width / DESIGN_WIDTH;

                    gameObject.transform.localScale = new Vector3(sxb, sxb, gameObject.transform.localScale.z);


                    if (getIsIPhoneX())
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y - IPHONEX_TOP_OFFSET 
                                                                    +_topMenu.GetComponent<RectTransform>().rect.height * (1 - sxb),
                                                                   gameObject.transform.position.z);

                        /*
                        float d = _bottomMenu.GetComponent<RectTransform>().rect.height * (1 - sxb) 
                                             + IPHONEX_TOP_OFFSET 
                                             + IPHONEX_BOTTOM_OFFSET
                                             + _topMenu.GetComponent<RectTransform>().rect.height * (1 - sxb);

                    */

                        //因为x系列足够长，不必再处理列表高度
                    }else{
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y
                                                                    + _topMenu.GetComponent<RectTransform>().rect.height * (1 - sxb),
                                                                    gameObject.transform.position.z);


                        //fuck pad
                        if (GetIsPad())
                        {
                            sxb = 0.9f;
                            gameObject.transform.localScale = new Vector3(sxb, sxb, gameObject.transform.localScale.z);
                        }

                        return;
                    }
                }else if(_panelType == PANEL_TYPE.LIKE_PANEL){

                    float th = _topMenu.GetComponent<RectTransform>().rect.height * _topMenu.transform.localScale.y;

                    float sxx = (DESIGN_HEIGHT - th) / gameObject.GetComponent<RectTransform>().rect.height;

                    //0.8f
                    if (getIsIPhoneX())
                    {
                        gameObject.transform.localScale = new Vector3(sxx * 0.8f,
                                                                      sxx * 0.8f, 
                                                                      gameObject.transform.localScale.z);

    

                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                            gameObject.transform.position.y - IPHONEX_TOP_OFFSET,
                                           gameObject.transform.position.z);

                    }else{

                        gameObject.transform.localScale = new Vector3(sxx, sxx, gameObject.transform.localScale.z);

                        if (GetIsPad())
                        {
                            //
                            float h = gameObject.GetComponent<RectTransform>().rect.height * (1 - sxx);
                            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                                            gameObject.transform.localPosition.y - h,
                                                                            gameObject.transform.localPosition.z);
                        }
                    }


                }else if(_panelType == PANEL_TYPE.MAIN_PANEL)
                {
                    //dh->h  Screen
                    float sx1 = GetFitUIScale();
                    gameObject.transform.localScale = new Vector3(sx1, sx1, gameObject.transform.localScale.z);

                    if (GetIsPad())
                    {
                        //
                        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                                        gameObject.transform.localPosition.y - 50,
                                                                        gameObject.transform.localPosition.z);
                    }

                }
                break;
            case UI_TYPE.BOTTOM_MENU:
                if(_panelType == PANEL_TYPE.MAIN_PANEL){
                    if (getIsIPhoneX())
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y + IPHONEX_BOTTOM_OFFSET,
                                                                   gameObject.transform.position.z);
                    }
                }else if(_panelType== PANEL_TYPE.SETTING_PANEL){
                    float sxb = gameObject.transform.GetComponent<RectTransform>().rect.width / DESIGN_WIDTH;
                    gameObject.transform.localScale = new Vector3(sxb, sxb, gameObject.transform.localScale.z);
                    if (getIsIPhoneX())
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y + IPHONEX_BOTTOM_OFFSET,
                                                                   gameObject.transform.position.z);
                    }
                }

                break;
        }
    }

    public static float GetXScale(GameObject obj){
        return obj.transform.GetComponent<RectTransform>().rect.width / DESIGN_WIDTH;
    }

    public static float GetOffsetYIphoneX(bool top){
        if(getIsIPhoneX())
        {
            if(top){
                return IPHONEX_TOP_OFFSET;
            }else{
                return IPHONEX_BOTTOM_OFFSET;
            }
        }

        return 0;
    }

    public static float GetFitUIScale(){

        float s = Screen.height / DESIGN_HEIGHT;
        float w = s * DESIGN_WIDTH;
        float sx1 = Screen.width / w;

        if(GetIsPad()){
            sx1 *= 0.9f;
        }

        return sx1;
    }

	// 外部直接使用
	public static int IPHONEX_TOP_OFFSET = 132;
	public static int IPHONEX_BOTTOM_OFFSET = 102;
    public static bool GetIsIphoneXByScreenSize()
    {
        if ((Screen.width == 1125 && Screen.height == 2436)
            || (Screen.width == 828 && Screen.height == 1792)
            || (Screen.width == 1242 && Screen.height == 2688))
        {
            return true;
        }

        return false;
    }

    public static bool getIsIPhoneX()
    {
        if (SystemInfo.deviceModel.Contains("iPhone10,3")
            || SystemInfo.deviceModel.Contains("iPhone10,6")
            || SystemInfo.deviceModel.Contains("iPhone11,2")
           || SystemInfo.deviceModel.Contains("iPhone11,4")
           || SystemInfo.deviceModel.Contains("iPhone11,6")
           || SystemInfo.deviceModel.Contains("iPhone11,8")
           || GetIsIphoneXByScreenSize())
        {
            return true;
        }

        return false;
    }

    public static bool GetIsPad(){
        if ((Screen.width == 768 && Screen.height == 1024)
            || (Screen.width == 1536 && Screen.height == 2048)
            || (Screen.width == 640 && Screen.height == 960)
            || (Screen.width == 320 && Screen.height == 480)
            || (Screen.width == 2048 && Screen.height == 2732))
        {
            return true;
        }

        return false;
    }
}
