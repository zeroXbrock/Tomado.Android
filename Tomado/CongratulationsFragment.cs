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

			Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
			
			Dialog.Window.SetLayout(WindowManagerLayoutParams.MatchParent, Resource.String.DialogHeight);
			
			View view = inflater.Inflate(Resource.Layout.CongratulationsDialog, container, false);

			Button okButton = view.FindViewById<Button>(Resource.Id.buttonOK_congrats);
			Button okButtonInvisible = view.FindViewById<Button>(Resource.Id.buttonOKInvisible_congrats);

			okButton.Clickable = false;
			okButtonInvisible.Click += delegate {
				Dismiss();
			};

			var textViewCongratsTitle = view.FindViewById<TextView>(Resource.Id.textViewCongratulationsTitle);
			TextView textViewCongratsMessage = view.FindViewById<TextView>(Resource.Id.textViewCongratulationsMessage);

			textViewCongratsTitle.Text = taskName;
			textViewCongratsMessage.Text = "Completed in " + pomodoros.ToString() + " sessions!";

			return view;
		}

		public override void OnSaveInstanceState(Bundle outState) {
			base.OnSaveInstanceState(outState);

			outState.PutString("taskName", taskName);
			outState.PutInt("pomodoros", pomodoros);
		}
	}
}