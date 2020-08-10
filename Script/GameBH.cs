using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;


public class GameBH : MonoBehaviour
{
    public Transform _HZContent;
    public GameObject _hzPrefabs;

    public void Start()
    {
        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.SHZ,true, (HZManager.eLoadResType type)=>{
            Init();
        });
    }

    public void Init()
    {
        for (int i = 8800 -1-2; i >= 6000; i--)
        {
            //顶部汉字
            GameObject Hz = Instantiate(_hzPrefabs, _HZContent) as GameObject;
            Hz.SetActive(true);

            Text[] t2 = Hz.GetComponentsInChildren<Text>();
            t2[0].text = HZManager.GetInstance().GetSHZ(i)[(int)HZManager.eSHZCName.HZ_HZ];
            t2[1].text = ""+(i+1);
        }
    }
}
