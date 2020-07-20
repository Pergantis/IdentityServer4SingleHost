using System;
using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using System.Net.Http;
using System.Security.Policy;
using IdentityModel.Jwk;
using IdentityModel.OidcClient.Browser;

namespace IdentityServer4SingleHost.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView _output;
        private static LoginResult _result;
        private OidcClientOptions _options;
        private static OidcClient _oidcClient;
        private static string _accessToken;
        private static string _refreshToken;
        private static string _identityToken;
        private string _authority = "http://mylocalhost.com:5000";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            #if DEBUG
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (certificate.Issuer.Equals("CN=localhost"))
                    return true;
                return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
            };
            #endif

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            var loginButton = FindViewById<Button>(Resource.Id.LoginButton);
            loginButton.Click += _loginButton_Click;

            var apiButton = FindViewById<Button>(Resource.Id.ApiButton);
            apiButton.Click += _apiButton_Click;

            var refreshButton = FindViewById<Button>(Resource.Id.RefreshButton);
            refreshButton.Click += _refreshButton_Click;

            var logoutButton = FindViewById<Button>(Resource.Id.LogoutButton);
            logoutButton.Click += _logoutButton_Click;

            _output = FindViewById<TextView>(Resource.Id.Output);

            ShowResults();

            _options = new OidcClientOptions
            {
                Authority = _authority,
                ClientId = "xamarin.native.hybrid.android",
                ClientSecret = "aV3ry$tr0nGP@$$w0rD^@F0rAndR0id",
                Scope = "openid api.mobile.user offline_access",
                RedirectUri = "app.native.android://callback",
                PostLogoutRedirectUri = "app.native.android://callback",
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                Browser = new ChromeCustomTabsBrowser(this),
                Flow = OidcClientOptions.AuthenticationFlow.Hybrid,
                Policy = new Policy
                {
                    // debugging απο emulator
                    Discovery = new DiscoveryPolicy { RequireHttps = false } // NOTE: this would be true for production
                    //Discovery = new DiscoveryPolicy { RequireHttps = true } // NOTE: this would be true for production
                }
            };
        }

        private async void _logoutButton_Click(object sender, EventArgs e)
        {
            var logoutRequest = new LogoutRequest()
            {
                IdTokenHint = _identityToken
            };

            _oidcClient= new OidcClient(_options);

           var logoutResult = await _oidcClient.LogoutAsync(logoutRequest);

           _accessToken = null;
           _refreshToken = null;

           StartActivity(GetType());
        }

        private async void _loginButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                 _oidcClient= new OidcClient(_options);
                 LoginRequest request = new LoginRequest()
                 {
                     // Use the comments to connect with external provider at once
                     FrontChannelExtraParameters = new Dictionary<string, string>()
                     {
                         //{"acr_values", "idp:Google tenant:Admin"}
                         //{"acr_values", "idp:Google"},
                         //{"login_hint", "admin@user.com" }
                     }

                 };

                _result = await _oidcClient.LoginAsync(request);
                _accessToken = _result.AccessToken;
                _refreshToken = _result.RefreshToken;
                _identityToken = _result.IdentityToken;

                // used to redisplay this app if it's hidden by browser
                StartActivity(GetType());
            }
            catch (Exception ex)
            {
                Log("Exception: " + ex.Message, true);
                Log(ex.ToString());
            }
        }

        private void ShowResults()
        {
            if (_result != null)
            {
                if (_result.IsError)
                {
                    Log("Error:" + _result.Error, true);
                }
                else
                {
                    Log("Claims:", true);
                    foreach (var claim in _result.User.Claims)
                    {
                        Log($"   {claim.Type}:{claim.Value}");
                    }
                    Log("Access Token: " + _result.AccessToken);
                    Log("Refresh Token: " + _result.RefreshToken);
                }
            }
        }

        private async void _apiButton_Click(object sender, EventArgs e)
        {
            if (_result?.IsError == false)
            {
                var apiUrl = _authority + "/tempdata";

                var client = new HttpClient();
                client.SetBearerToken(_accessToken);

                try
                {
                    var result = await client.GetAsync(apiUrl);
                    if (result.IsSuccessStatusCode)
                    {
                        Log("API Results:", true);

                        var json = await result.Content.ReadAsStringAsync();
                        Log(json);
                    }
                    else
                    {
                        Log("API Error: " + (int)result.StatusCode, true);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception: " + ex.Message, true);
                    Log(ex.ToString());
                }
            }
            else
            {
                Log("Login to call API");
            }
        }

        private async void _refreshButton_Click(object sender, EventArgs e)
        {
            if (_refreshToken != null)
            {
                var result = await _oidcClient.RefreshTokenAsync(_result.RefreshToken, null);

                Log("Refresh Token Result", clear: true);
                if (result.IsError)
                {
                    Log("Error: " + result.Error);
                    return;
                }

                _accessToken = result.AccessToken;
                _refreshToken = result.RefreshToken;

                //_result.RefreshTokenHandler = result.RefreshToken;
                //_result.AccessToken = result.AccessToken;

                Log("Access Token: " + _accessToken);
                Log("Refresh Token: " + _refreshToken);
            }
            else
            {
                Log("No Refresh Token", true);
            }
        }

        public void Log(string msg, bool clear = false)
        {
            if (clear)
            {
                _output.Text = "";
            }
            else
            {
                _output.Text += "\r\n";
            }

            _output.Text += msg;
        }
	}
}
