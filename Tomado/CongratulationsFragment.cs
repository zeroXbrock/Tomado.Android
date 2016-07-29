using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;

namespace Tomado {
	public class CongratulationsFragment : Android.Support.V4.App.DialogFragment {
		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			Dialog.SetTitle("Congratulations!");

			View view = inflater.Inflate(Resource.Layout.CongratulationsDialog, container, false);

			Button okButton = view.FindViewById<Button>(Resource.Id.buttonOK_congrats);

			okButton.Click += delegate {
				Dismiss();
			};

			TextView textViewCongratsMessage = view.FindViewById<TextView>(Resource.Id.textViewCongratulationsMessage);
			

			return view;
		}
	}
}