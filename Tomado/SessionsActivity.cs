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

namespace Tomado {
	[Activity(Label = "Sessions")]
	public class SessionsActivity : Activity {
		ListView listViewFreetime;
		List<Session> sessions;
		Button newSessionButton;
		
		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			
			SetContentView(Resource.Layout.Sessions);

			//get ListView instance
			listViewFreetime = FindViewById<ListView>(Resource.Id.listViewFreetime);

			newSessionButton = FindViewById<Button>(Resource.Id.buttonNewSession);
			newSessionButton.Click += delegate {
				StartActivityForResult(typeof(NewSessionActivity), 1);
			};
			
			//populate ListViewFreetime
			PopulateListView();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			base.OnActivityResult(requestCode, resultCode, data);

			if (resultCode == Result.Ok) {
				AddSession(data);
			}
		}

		private void PopulateListView() {
			sessions = new List<Session>();
			sessions.Add(new Session(12, 30, 1, 0, "first"));
			sessions.Add(new Session(2, 0, 3, 0, "second"));
			sessions.Add(new Session(3, 30, 5, 0, "third"));

			listViewFreetime.Adapter = new SessionAdapter(this, sessions);
		}

		//get data from intent and add a new session to the list
		private void AddSession(Intent data) {
			int startHour, startMinute, endHour, endMinute;

			string title = data.GetStringExtra("title");
			startHour = data.GetIntExtra("startHour", -1);
			startMinute = data.GetIntExtra("startMinute", -1);
			endHour = data.GetIntExtra("endHour", -1);
			endMinute = data.GetIntExtra("endMinute", -1);

			sessions.Add(new Session(startHour, startMinute, endHour, endMinute, title));
			listViewFreetime.Adapter = new SessionAdapter(this, sessions);
		}
	}
	
}