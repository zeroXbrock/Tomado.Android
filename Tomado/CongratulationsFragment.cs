using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;

namespace Tomado {
	public class CongratulationsFragment : Android.Support.V4.App.DialogFragment {
		string taskName = "";
		int pomodoros = 0;

		public CongratulationsFragment() { }

		public CongratulationsFragment(Session session) {
			taskName = session.Title;
			pomodoros = session.Pomodoros;
		}

		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			if (savedInstanceState != null) {
				pomodoros = savedInstanceState.GetInt("pomodoros");
				taskName = savedInstanceState.GetString("taskName");
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			Dialog.SetTitle("Congratulations!");
			Dialog.Window.SetBackgroundDrawableResource(Resource.Drawable.base_app_complementary_color_drawable);
			
			View view = inflater.Inflate(Resource.Layout.CongratulationsDialog, container, false);

			Button okButton = view.FindViewById<Button>(Resource.Id.buttonOK_congrats);

			okButton.Click += delegate {
				Dismiss();
			};

			TextView textViewCongratsMessage = view.FindViewById<TextView>(Resource.Id.textViewCongratulationsMessage);
			textViewCongratsMessage.Text = "Good job! You completed " + taskName + " in " + pomodoros.ToString() + " tomados!";

			return view;
		}

		public override void OnSaveInstanceState(Bundle outState) {
			base.OnSaveInstanceState(outState);

			outState.PutString("taskName", taskName);
			outState.PutInt("pomodoros", pomodoros);
		}
	}
}