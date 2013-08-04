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

		Dialog dialog;
		TextView dialogMessage;

		EditText emailOrUsernameEditText, passwordEditText;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);

			//
			// Setup Dialog
			//
			dialog = new Dialog(this);

			LinearLayout dialogLayout = new LinearLayout(this);
			dialogLayout.Orientation = Orientation.Vertical;
			dialog.SetContentView(dialogLayout);

			dialogMessage = new TextView(this);
			dialogLayout.AddView(dialogMessage);

			Button dialogCloseButton = new Button(this);
			dialogCloseButton.Text = "Close";
			dialogCloseButton.Click += CloseDialog;
			dialogLayout.AddView(dialogCloseButton);


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
		
		void ShowDialog(String message)
		{
			//dialog.SetTitle("");
			dialogMessage.Text = message;
			dialog.Show();
		}
		/*
		void ShowDialog(String title, String message)
		{
			dialog.SetTitle(title);
			dialogMessage.Text = message;
			dialog.Show();
		}
		*/
		void CloseDialog(Object sender, EventArgs e)
		{
			dialog.Dismiss();
		}

		void SignIn(Object sender, EventArgs e)
		{
			String emailOrUsername = emailOrUsernameEditText.Text;
			String password = passwordEditText.Text;

			if (String.IsNullOrEmpty(emailOrUsername))
			{
				ShowDialog("Missing Email/Username");
				return;
			}
			if (String.IsNullOrEmpty(password))
			{
				ShowDialog("Missing Password");
				return;
			}

			// Trim whitespace
			emailOrUsername = emailOrUsername.Trim();
			password = password.Trim();

			String message = connection.SignIn(emailOrUsername, password);
			if (message == null)
			{
				ShowDialog("Login Success");
			}
			else
			{
				ShowDialog(message);
			}
		}
	}
}
