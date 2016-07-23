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
	public class SessionAdapter : BaseAdapter<Session> {
		List<Session> sessions;
		Activity context;

		public SessionAdapter(Activity context, List<Session> sessions) {
			this.context = context;
			this.sessions = sessions;
		}

		public override long GetItemId(int position) {
			return position;
		}

		public override Session this[int position] {
			get { return sessions[position]; }
		}

		public override int Count {
			get { return sessions.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent) {
			View view = convertView;//reuse existing view if available
			if (view == null) {
				view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem2, null);
			}

			view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = sessions[position].Title;
			view.FindViewById<TextView>(Android.Resource.Id.Text2).Text = (sessions[position].StartHour + ":" + sessions[position].StartMinute 
				+ " - " + sessions[position].EndHour + ":" + sessions[position].EndMinute);

			return view;
		}
	}
}