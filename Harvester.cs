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
                //LoadGeeTest(captchaInfo);
            }
            else
            {
                LoadReCaptcha(captchaInfo);
            }
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
