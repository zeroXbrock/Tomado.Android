using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;

namespace Tomado {
	class DeleteSessionFragment : Android.Support.V4.App.DialogFragment {
		Session session;
		string title;
		SessionAdapter.DeleteSessionListener deleteSessionListener; //to fire off the delete event

		public DeleteSessionFragment() { }

		public DeleteSessionFragment(Session session, SessionAdapter.DeleteSessionListener deleteSessionListener) {
			this.session = session;
			this.deleteSessionListener = deleteSessionListener;
			title = session.Title;
		}

		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			if (savedInstanceState != null)
				title = savedInstanceState.GetString("title");
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			View view = inflater.Inflate(Resource.Layout.DeleteSessionDialog, container);

			Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

			//get views
			TextView textView = view.FindViewById<TextView>(Resource.Id.textViewDeleteSession);
			Button yesButton = view.FindViewById<Button>(Resource.Id.buttonYes_DeleteSession);
			Button noButton = view.FindViewById<Button>(Resource.Id.buttonNo_DeleteSession);

			textView.Text = "Delete \"" + title + "\"?";

			yesButton.Click += delegate {
				deleteSessionListener.OnDeleteSession(session);
				Dismiss();
			};

			noButton.Click += delegate {
				Dismiss();
			};

			return view;
		}

		public override void OnSaveInstanceState(Bundle outState) {
			base.OnSaveInstanceState(outState);

			outState.PutString("title", title);
		}
	}
}