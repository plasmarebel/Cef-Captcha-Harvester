using CefSharp;
using CefSharp.Wpf;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;

namespace Venom_AIO.FrontEnd.Pages.CaptchaHarvester
{
    /// <summary>
    /// Interaction logic for HarvesterWindow.xaml
    /// </summary>
    public partial class HarvesterWindow : Window
    {
        ChromiumWebBrowser chromeBrowser;
        public bool solvingCaptcha = false;
        internal static HarvesterWindow main;
        private BackEnd.Captcha.CaptchaInformation.RootObject captchaInfo;
        private bool pollingResponse = false;
        private bool autoClick = true;
        public HarvesterWindow()
        {
            InitializeComponent();
            main = this;
            LoadBrowser();
        }

        private async void LoadBrowser()
        {
            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings()
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36",                   
                };
                Cef.Initialize(settings);
            }

            chromeBrowser = new ChromiumWebBrowser
            {
                RequestContext = new RequestContext(),
                RequestHandler = new RequestHandlerExt(this)
            };
            browserContainer.Content = chromeBrowser;
            await Task.Delay(1000);
        }

        private void CaptchaHide(bool unhide)
        {
            if(unhide == true)
            {
                hideGrid.Dispatcher.Invoke(new Action(() => { hideGrid.Visibility = Visibility.Hidden; }));
            }
            else
            {
                hideGrid.Dispatcher.Invoke(new Action(() => { hideGrid.Visibility = Visibility.Visible;}));
            }
        }

        public async void LoadCaptcha(BackEnd.Captcha.CaptchaInformation.RootObject captchaInfo)
        {
            while (!chromeBrowser.IsInitialized)
            {
                await Task.Delay(111);
            }
            this.captchaInfo = captchaInfo;

            chromeBrowser.Load("https://" + new Uri(captchaInfo.url).Host + "/");
            await Task.Delay(1000);

            if(captchaInfo.geeTestInfo != null)
            {
                LoadGeeTest(captchaInfo);
            }
            else
            {
                LoadReCaptcha(captchaInfo);
            }
        }

        private void LoadGeeTest(BackEnd.Captcha.CaptchaInformation.RootObject captchaInfo)
        {
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync("document.querySelector(\"html\").innerHTML = `<title>gt-curtain</title> <style> body { margin: 0; } .one { height: 100%; width: 330px; min-height: 186px; display: inline-block; text-align: center; } .gt_holder.float { display: inline-block; margin-top: 150px; } .one > * { vertical-align: bottom; } canvas { display: inline-block; vertical-align: top; } .btn { margin-top: 20px; border: 1px solid #080; border-radius: 10px; height: 50px; line-height: 50px; font-size: 32px; text-align: center; cursor: pointer; } .btn:hover { background-color: #080; color: #FFFFFF; } #refresh { background-color: #080; } </style></head><body><div id=\"geetest\"></div><div> <a class=\"btn\" id=\"curtain-btn\">refresh curtain embed</a> <a class=\"btn\" id=\"slide-btn\">refresh slide embed</a> <a class=\"btn\" id=\"canvas-btn\">refresh canvas embed</a> <a class=\"btn\" id=\"refresh0\">Geetest[0].refresh()</a></div><div id=\"log\"></div><script src=\"http://api.geetest.com/get.php?callback=cb\"></script><!--<script src=\"static/js/geetest.4.0.2.js\"></script>--><script>// setTimeout(cb, 1000); var cb = function () { var ele = function (str) { if (document.querySelector) { return document.querySelector(str); } else if (str.indexOf('#') > -1) { return document.getElementById(str.replace('#', '')); } else if (str.indexOf('.') > -1) { return document.getElementsByClassName(str.replace('.', ''))[0]; } else { return document.getElementsByTagName(str)[0]; } }; var gtEle = ele('#geetest'); var logEle = ele('#log'); var newGeetest = function (type, product, gt, a) { var config = { product: product, gt: gt, lang: 'zh-cn', width: '300', a: a }; var captchaObj = new Geetest(config); var ele = document.createElement('div'); ele.className = 'one'; var box = document.createElement('div'); ele.appendChild(box); captchaObj.appendTo(box); if (product === 'popup') { var btn = document.createElement('div'); btn.className = 'btn'; btn.appendChild(document.createTextNode('按钮（' + type + '）')); ele.appendChild(btn); captchaObj.bindOn(btn); } gtEle.appendChild(ele); return captchaObj; }; var log = function (str) { logEle.appendChild(document.createElement('br')); logEle.appendChild(document.createTextNode(str)); }; log(document.getElementById('gt_lib').src); var oldApis = ['gt_custom_ajax', 'gt_custom_refresh', 'gt_custom_error', 'onGeetestLoaded']; for (var i = 0, len = oldApis.length; i < len; i = i + 1) { (function (i) { window[oldApis[i]] = function () { log('oldApi: ' + oldApis[i]); } }(i)); } var apis = ['onReady', 'onRefresh', 'onStatusChange', 'onSuccess', 'onError', 'onFail']; var mode = location.href.split('?')[1] || '3'; var gt = '" + captchaInfo.geeTestInfo.gt + "'; var slideEmbed = newGeetest('slide', 'embed', gt, false); var slideFloat = newGeetest('slide', 'float', gt, false); var slidePopup = newGeetest('slide', 'popup', gt, false); for (var i = 0, len = apis.length; i < len; i = i + 1) { (function (i) { slideEmbed[apis[i]](function () { log('slider embed api: ' + apis[i]); }) }(i)); } var curtainEmbed, curtainFloat, curtainPopup; if (mode === '6' || mode === '9') { gt = '7fa05624d84017d681bcbe16036a1267'; curtainEmbed = newGeetest('slide', 'embed', gt, false); curtainFloat = newGeetest('slide', 'float', gt, false); curtainPopup = newGeetest('slide', 'popup', gt, false); for (var i = 0, len = apis.length; i < len; i = i + 1) { (function (i) { curtainEmbed[apis[i]](function () { log('curtain embed api: ' + apis[i]); }) }(i)); } } var canvasEmbed, canvasFloat, canvasPopup; if (mode === '9') { gt = 'ad872a4e1a51888967bdb7cb45589605'; canvasEmbed = newGeetest('slide', 'embed', gt, true); canvasFloat = newGeetest('slide', 'float', gt, true); canvasPopup = newGeetest('slide', 'popup', gt, true); for (var i = 0, len = apis.length; i < len; i = i + 1) { (function (i) { canvasEmbed[apis[i]](function () { log('canvas embed api: ' + apis[i]); }) }(i)); } } var curtainBtn = ele('#curtain-btn'); var slideBtn = ele('#slide-btn'); var canvasBtn = ele('#canvas-btn'); curtainBtn.addEventListener('click', function () { curtainEmbed.refresh(); }); slideBtn.addEventListener('click', function () { slideEmbed.refresh(); }); canvasBtn.addEventListener('click', function () { canvasEmbed.refresh(); }); var refresh0 = ele('#refresh0'); refresh0.addEventListener('click', function () { window.GeeTest[0].refresh(); }); window.slideEmbed = slideEmbed; window.slideFloat = slideFloat; window.slidePopup = slidePopup; window.curtainEmbed = curtainEmbed; window.curtainFloat = curtainFloat; window.curtainPopup = curtainPopup; window.canvasEmbed = canvasEmbed; window.canvasFloat = canvasFloat; window.canvasPopup = canvasPopup; };</script></body>");
            //chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync("var script = document.createElement('script');");
            //chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync("script.setAttribute('src', 'https://github.com/GeeTeam/gt3-node-sdk/blob/master/demo/static/libs/gt.js');");
            //chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync("document.head.appendChild(script)");
            //chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync("function(data) {initGeetest({ gt: " + captchaInfo.geeTestInfo.gt +",challenge: " + captchaInfo.geeTestInfo.challenge + ",offline: 0,new_captcha: 1, api_server: 'api-na.geetest.com', product: 'float',lang: 'en-us',http: 'https' + '://'}, function(captchaObj) { captchaObj.appendTo('#captcha-box'); }); ");
        }

        private async void LoadReCaptcha(BackEnd.Captcha.CaptchaInformation.RootObject captchaInfo)
        {
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(@"document.querySelector('html').innerHTML = `
                <body bgcolor='#232120'>                
                        <form action='/submit' method='POST' style='margin: auto; margin-top: 100px; width: 300px;'>
                <div id='captchaFrame'>
                <div class='g-recaptcha' id='captchaFrame' data-sitekey='" + captchaInfo.recaptchaInfo.sitekey + @"' data-callback='recaptchaCallback' style='height: 78px;'></div>                
                </div>
                </form></body>`");
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync("var script = document.createElement('script');");
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync("script.setAttribute('src', 'https://www.google.com/recaptcha/api.js');");
            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync("document.head.appendChild(script)");

            await Task.Delay(1000);
            CaptchaHide(true);

            if (autoClick)
            {
                await Task.Delay(1500);
                chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(45, 105, MouseButtonType.Left, false, 1, CefEventFlags.None);
                await Task.Delay(10);
                chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(45, 105, MouseButtonType.Left, true, 1, CefEventFlags.None);
            }
        }

        public async void PollForRecaptchaResponse()
        {
            if (pollingResponse == false)
            {
                pollingResponse = true;
                while (captchaInfo.solvedRecaptcha.recaptcaResponse == null)
                {
                    await chromeBrowser.GetMainFrame().EvaluateScriptAsync("document.getElementById('g-recaptcha-response').value;").ContinueWith(x =>
                    {
                         var response = x.Result;
                         if (response.Success && response.Result != null && response.Result.ToString() != "")
                         {
                             captchaInfo.solvedRecaptcha.recaptcaResponse = response.Result.ToString();
                         }
                    });

                    await Task.Delay(555);
                }

                await chromeBrowser.GetCookieManager().DeleteCookiesAsync();
                solvingCaptcha = false;
                CaptchaHide(false);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            solvingCaptcha = true;
            chromeBrowser.Dispose();
            Windows.MainWindow.main.harvesterWindows.Remove(this);
        }
    }

    public class RequestHandlerExt : IRequestHandler
    {
        private HarvesterWindow harvester;
        public RequestHandlerExt(HarvesterWindow harvester)
        {
            this.harvester = harvester;
        }
        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            return false;
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            if (request.Url.Contains("recaptcha/api2/userverify"))
            {
                harvester.PollForRecaptchaResponse();
            }

            return null;
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            return false;
        }

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            return false;
        }

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
        {
        }

        public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
        }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }


        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }
    }
}
