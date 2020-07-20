using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace IdentityServer4SingleHost.Droid
{

    [Activity(Label = "OpenEmailAppCallback")]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "app.native.android",
        DataHost = "open_email_app")]
    public class OpenEmailAppCallback : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.open_email_app);

            // Create your application here
            var openEmailButton = FindViewById<Button>(Resource.Id.OpenEmailApp);
            openEmailButton.Click += OpenEmailButtonOnClick;

        }

        private void OpenEmailButtonOnClick(object sender, EventArgs e)
        {
            //TODO Ανοιγμα Email App

            PackageManager packageManager = PackageManager;

            var emailIntent = new Intent(Intent.ActionSend);

            IList<ResolveInfo> activities =
                packageManager.QueryIntentActivities(emailIntent, 0);

            if (activities.Count == 0) {
                //display alert indicating there are no camera apps
            }
            else {
                //launch the cameraIntent
                StartActivity(emailIntent);
            }


        }
    }
}