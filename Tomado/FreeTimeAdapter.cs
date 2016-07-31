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
	public class FreeTimeAdapter : BaseAdapter<Session> {
		SessionAdapter.SessionClickListener sessionClickListener;
		Activity context;
		List<Session> freetime;
		int durationHours, durationMinutes;
		LinearLayout freeTimeListItem;

		public FreeTimeAdapter() { }

		public FreeTimeAdapter(Activity context, List<Session> freetime, SessionAdapter.SessionClickListener sessionClickListener) {
			this.sessionClickListener = sessionClickListener;
			this.freetime = freetime;
			this.context = context;
		}

		public override Session this[int position] {
			get { return freetime[position]; }
		}

		public override long GetItemId(int position) {
			return position;
		}

		public override int Count {
			get { return freetime.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent) {
			View view = convertView;

			if (view == null) {
				view = context.LayoutInflater.Inflate(Resource.Layout.FreeTimeListItem, null);
			}

			TextView textViewTime = view.FindViewById<TextView>(Resource.Id.textViewTime);
			TextView textViewDuration = view.FindViewById<TextView>(Resource.Id.textViewDuration);
			freeTimeListItem = view.FindViewById<LinearLayout>(Resource.Id.freeTimeListItem);

			Session freeTimeSession = freetime[position];

			int year = freeTimeSession.Year;
			int month = freeTimeSession.MonthOfYear;
			int day = freeTimeSession.DayOfMonth;
			int hour = freeTimeSession.StartHour;
			int minute = freeTimeSession.StartMinute;

			DateTime time = new DateTime(year, month, day, hour, minute, 0);
			
			textViewTime.Text = time.ToShortTimeString();
			textViewDuration.Text = durationHours + ":" + durationMinutes;

			freeTimeListItem.Click += delegate {
				sessionClickListener.OnSessionClick(freeTimeSession);
			};

			return view;
		}
	}
}