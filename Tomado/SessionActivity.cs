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
	public class SessionActivity : Activity {
		ListView listViewFreetime;
		List<Session> sessions;
		
		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			
			SetContentView(Resource.Layout.Schedule);

			//get ListView instance
			listViewFreetime = FindViewById<ListView>(Resource.Id.listViewFreetime);
			
			//populate ListViewFreetime
			PopulateListView();
		}

		private void PopulateListView() {
			sessions = new List<Session>();
			sessions.Add(new Session(0, 1, "first"));
			sessions.Add(new Session(2, 3, "second"));
			sessions.Add(new Session(3.5, 5, "third"));

			listViewFreetime.Adapter = new SessionAdapter(this, sessions);
		}
	}
	
}