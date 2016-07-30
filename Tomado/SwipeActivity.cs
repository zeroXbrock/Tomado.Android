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
	/// <summary>
	/// Activity that holds multiple fragments that can be swiped through.
	/// </summary>
	[Activity(MainLauncher = true, Icon = "@drawable/icon", LaunchMode=Android.Content.PM.LaunchMode.SingleTop)]
	class SwipeActivity : FragmentActivity, SessionAdapter.SessionClickListener, TimerFragment.TimerFinishListener {
		ViewPager viewPager;
		TimerFragment timerFragment;
		SessionsFragment sessionsFragment;

		AlarmReceiver alarmReceiver;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.SwipeLayout);

			viewPager = FindViewById<ViewPager>(Resource.Id.viewPager);
			var adapter = new FragmentAdapter(SupportFragmentManager);

			//make fragments for swipe view
			timerFragment = new TimerFragment(this);
			sessionsFragment = new SessionsFragment(this);
			
			//add fragments to adapter
			adapter.AddFragment(timerFragment);
			adapter.AddFragment(sessionsFragment);

			//set adapter
			viewPager.Adapter = adapter;
		}

		protected override void OnStart() {
			base.OnStart();
		}

		protected override void OnNewIntent(Intent intent) {
			base.OnNewIntent(intent);

			//check for ID; this means the activity is being started from a notification
			if (intent.GetIntExtra("ID", 0) > 0) {
				//get ID from extras
				int ID = intent.GetIntExtra("ID", 0);

				Session _session = new Session();

				//find matching session from list
				foreach (var session in sessionsFragment.Sessions) {
					if (session.ID == ID)
						_session = session;
				}

				//populate timer vars w/ our session var
				timerFragment.OnNewTimer(_session);

				//switch view to timer
				SetVisibleFragment(0);
			}
		}

		/// <summary>
		/// Sets the visible fragment in the viewpager based on index.
		/// </summary>
		/// <param name="position"></param>
		public void SetVisibleFragment(int position) {
			viewPager.SetCurrentItem(position, true);
		}

		public void OnSessionClick(Session session) {
			//set the actual timer
			timerFragment.OnNewTimer(session);
			//set the view to timer view
			SetVisibleFragment(0);
		}

		public void OnTimerFinish(Session session) {
			if (!session.Recurring)
				sessionsFragment.OnDeleteSession(session);
		}
	}
}