// -----------------------------------------------
//内购相关逻辑
// -----------------------------------------------

using Reign;
using UnityEngine;
using com.mob;
using UnityEngine.Events;

namespace HanZi
{
    public class Utils : MonoBehaviour
    {
        public void SendEmail()
        {
            EmailManager.Send("ia_fun@163.com", "建议反馈", "任何建议、反馈、问题、合作，欢迎联系我们～");
        }

        public  void OpenWeibo()
        {
            //
            Application.OpenURL("http://weibo.com/u/3845616134");
        }

        public void OpenProducts()
        {
            //
            Application.OpenURL("http://appstore.com/hangzhouiafuntechnologycoltd");
        }

        public void OpenTipix()
        {
            openStore("717545399");// Pass in your AppID "xxxxxxxxx"
        }

        public void OpenMapix()
        {
            openStore("941023990");// Pass in your AppID "xxxxxxxxx"
        }

        public void OpenTipixel()
        {
            openStore("1325480389");// Pass in your AppID "xxxxxxxxx"
        }

        public void OpenGSSJ()
        {
            openStore("1449364884");// Pass in your AppID "xxxxxxxxx"
        }

        public void Donate()
        {
            string url = "alipayqr://platformapi/startapp?saId=10000007&clientVersion=3.7.0.0718&qrcode=HTTPS://QR.ALIPAY.COM/FKX07597YLUWKETRZIV510";

            if (ShareRECIOS.canOpenUrl(url))
            {
                Application.OpenURL(url);
            }
            else
            {
                MessageBoxManager.Show("", "感谢点赞支持！");
            }
        }

        // 好评
        public void OpenCYM()
        {
            openStore("1450896243", true);
        }
        private void openStore(string appId, bool forReview = false)
        {
            var desc = new MarketingDesc();

            desc.Editor_URL = "";// Any full URL
            desc.Win8_PackageFamilyName = "";// This is the "Package family name" that can be found in your "Package.appxmanifest".
            desc.WP8_AppID = "";// This is the "App ID" that can be found in your "Package.appxmanifest" under "Package Name".
                                // NOTE: For Windows Phone 8.0 you don't need to set anything...

            desc.iOS_AppID = appId;// Pass in your AppID "xxxxxxxxx"
            desc.BB10_AppID = "";// You pass in your AppID "xxxxxxxx".

            desc.Android_MarketingStore = MarketingStores.GooglePlay;
            desc.Android_GooglePlay_BundleID = "";// Pass in your bundle ID "com.Company.AppName"
            desc.Android_Amazon_BundleID = "";// Pass in your bundle ID "com.Company.AppName"
            desc.Android_Samsung_BundleID = "";// Pass in your bundle ID "com.Company.AppName"

            if (forReview)
            {
                MarketingManager.OpenStoreForReview(desc);
            }
            else
            {
                MarketingManager.OpenStore(desc);
            }
        }
    }
}
