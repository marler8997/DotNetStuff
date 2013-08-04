using System;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace CampaignApp
{
	[Activity (Label = "The Campaign App", MainLauncher = true)]
	public class SignInActivity : Activity
	{
		readonly CampaignAppServerConnection connection = new CampaignAppServerConnection();

        //
        // Dialog
        //
		Dialog dialog;
        LinearLayout dialogLayout;
		TextView dialogMessage;
        Button dialogCloseButton;
        Boolean dialogHasCloseButton;

        //
        // Sign In
        //
		EditText emailOrUsernameEditText, passwordEditText;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			//
			// Setup Dialog
			//
			dialog = new Dialog(this);

			dialogLayout = new LinearLayout(this);
			dialogLayout.Orientation = Orientation.Vertical;
			dialog.SetContentView(dialogLayout);
            dialogHasCloseButton = false;

			dialogMessage = new TextView(this);
			dialogLayout.AddView(dialogMessage);

			dialogCloseButton = new Button(this);
			dialogCloseButton.Text = "Close";
			dialogCloseButton.Click += CloseDialog;


			//
			// Setup SignIn
			//
			LinearLayout layout = new LinearLayout(this);
			layout.Orientation = Orientation.Vertical;
			layout.SetBackgroundColor (Color.Aqua);

			TextView title = new TextView(this);
			title.Text = "Campaign App";
			layout.AddView(title);

			emailOrUsernameEditText = new EditText (this);
			emailOrUsernameEditText.Hint = "Email or username";
			emailOrUsernameEditText.InputType = Android.Text.InputTypes.TextVariationEmailAddress;
			layout.AddView (emailOrUsernameEditText);

			passwordEditText = new EditText (this);
			passwordEditText.Hint = "Password";
			passwordEditText.InputType = Android.Text.InputTypes.TextVariationPassword;
			layout.AddView (passwordEditText);

			Button signInButton = new Button (this);
			signInButton.Text = "Sign In";
			signInButton.Click += SignIn;
			layout.AddView(signInButton);

			//
			// Horizontal Bar 1
			//
			LinearLayout horzBar1 = new LinearLayout (this);
			layout.AddView (horzBar1);

			CheckBox rememberMe = new CheckBox (this);
			rememberMe.Text = "Remember me";
			horzBar1.AddView (rememberMe);

			TextView separator = new TextView (this);
			separator.Text = " | ";
			horzBar1.AddView (separator);

			TextView forgotPassword = new TextView(this);
			forgotPassword.Text = "Forgot password?";
			horzBar1.AddView(forgotPassword);

			//
			// Sign Up
			//




			SetContentView(layout);
		}
		
		void ShowDialog(String message, Boolean closeButton)
		{
            if (closeButton)
            {
                if (!dialogHasCloseButton)
                {
                    dialogLayout.AddView(dialogCloseButton);
                    dialogHasCloseButton = true;
                }
            }
            else
            {
                if (dialogHasCloseButton)
                {
                    dialogLayout.RemoveView(dialogCloseButton);
                    dialogHasCloseButton = false;
                }
            }

			dialogMessage.Text = message;
			dialog.Show();
		}

        void HandleException(Exception e)
        {
            ShowDialog(e.GetType() + " occured", true);
        }

		void CloseDialog(Object sender, EventArgs e)
		{
			dialog.Dismiss();
		}


        void SignInSuccess()
        {
            RunOnUiThread(() => {
                ShowDialog("You are signed in", true);
            });
        }
        void SignInFailed(String message)
        {
            RunOnUiThread(() => {
                ShowDialog(message, true);
            });
        }
		void SignIn(Object sender, EventArgs e)
		{
			String emailOrUsername = emailOrUsernameEditText.Text;
			String password = passwordEditText.Text;

			if (String.IsNullOrEmpty(emailOrUsername))
			{
				ShowDialog("Missing Email/Username", true);
				return;
			}
			if (String.IsNullOrEmpty(password))
			{
				ShowDialog("Missing Password", true);
				return;
			}

            ShowDialog("Signing in...", false);

			// Trim whitespace
			emailOrUsername = emailOrUsername.Trim();
			password = password.Trim();

            connection.SignIn(emailOrUsername, password, SignInSuccess, SignInFailed);
		}
	}
}
