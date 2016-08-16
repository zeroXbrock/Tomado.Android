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
	public class FreeTimeAdapter : BaseAdapter<FreeTime> {
		SessionAdapter.SessionClickListener sessionClickListener;
		Activity context;
		List<FreeTime> freetime;
		long durationHours, durationMinutes;
		LinearLayout freeTimeListItem;

		public FreeTimeAdapter() { }

		public FreeTimeAdapter(Activity context, List<FreeTime> freetime, SessionAdapter.SessionClickListener sessionClickListener) {
			this.sessionClickListener = sessionClickListener;
			this.freetime = freetime;
			this.context = context;
		}

		public override FreeTime this[int position] {
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

			FreeTime ft = freetime[position];

			DateTime time = ft.Start;

			long durationInTicks = (ft.End.Ticks - ft.Start.Ticks);
			durationHours = durationInTicks / TimeSpan.TicksPerHour;
			durationMinutes = (durationInTicks - (durationHours * TimeSpan.TicksPerHour)) / TimeSpan.TicksPerMinute;

			Session freeTimeSession = new Session(1, time, "New Session", null);
			
			textViewTime.Text = time.ToShortTimeString();
			textViewDuration.Text = durationHours + " hours and " + durationMinutes + " minutes";

			freeTimeListItem.Click += delegate {
				sessionClickListener.OnSessionClick(freeTimeSession);
			};

			return view;
		}
	}

	public struct FreeTime {
		private DateTime start;
		private DateTime end;

		public DateTime Start {
			get {
				return start;
			}
			set {
				start = value;
			}
		}
		public DateTime End {
			get {
				return end;
			}
			set {
				end = value;
			}
		}
	}
}