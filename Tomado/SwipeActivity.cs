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
	[Activity(MainLauncher = true, Icon = "@drawable/icon")]
	class SwipeActivity : FragmentActivity, SessionAdapter.SessionClickListener {
		ViewPager viewPager;
		TimerFragment timerFragment;
		SessionsFragment sessionsFragment;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.SwipeLayout);

			viewPager = FindViewById<ViewPager>(Resource.Id.viewPager);
			var adapter = new FragmentAdapter(SupportFragmentManager);

			timerFragment = new TimerFragment();
			sessionsFragment = new SessionsFragment(this);
			
			adapter.AddFragment(timerFragment);
			adapter.AddFragment(sessionsFragment);

			viewPager.Adapter = adapter;
		}

		/// <summary>
		/// Sets the visible fragment in the viewpager based on index.
		/// </summary>
		/// <param name="position"></param>
		public void SetVisibleFragment(int position) {
			viewPager.SetCurrentItem(position, true);
		}

		public void OnSessionClick(Session session) {
			//modify timer view contents before switching views
			var typeView = FindViewById<TextView>(Resource.Id.textViewTimerType);
			var timeView = FindViewById<TextView>(Resource.Id.textViewTimer);

			//typeView.Text = session.Title;
			//set timeView to 25:00
			//set the actual timer
			timerFragment.OnNewTimer(session);
			//set the view to timer view
			SetVisibleFragment(0);
		}
	}
}