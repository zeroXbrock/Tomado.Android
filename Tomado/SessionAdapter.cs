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

using Java.Lang;

namespace Tomado {
	/// <summary>
	/// Adapter to populate Sessions list.
	/// </summary>
	public class SessionAdapter : BaseAdapter<Session> {
		List<Session> sessions;
		Activity context;
		private DeleteSessionListener deleteSessionListener;

		/// <summary>
		/// Interface to provide callback for deleting sessions.
		/// </summary>
		public interface DeleteSessionListener {
			/// <summary>
			/// Implement in containing class to delete session from list and database.
			/// </summary>
			/// <param name="position">Index of item in listview</param>
			/// <param name="ID">Item's databse ID</param>
			void OnDeleteSession(Session session);
		}

		public SessionAdapter(Activity context, List<Session> sessions, DeleteSessionListener deleteSessionListener) {
			this.context = context;
			this.sessions = sessions;
			this.deleteSessionListener = deleteSessionListener;
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
				view = context.LayoutInflater.Inflate(Resource.Layout.SessionListItem, null);
			}

			Session session = sessions[position];
			DateTime dateTime = new DateTime(session.Year, session.MonthOfYear, session.DayOfMonth, session.StartHour, session.StartMinute, 0);

			///set info for each item in listview
			//set text views
			view.FindViewById<TextView>(Resource.Id.evTitle).Text = session.Title;
			view.FindViewById<TextView>(Resource.Id.evTime).Text = (dateTime.ToShortTimeString() + "\t" + dateTime.ToShortDateString());
			view.FindViewById<Button>(Resource.Id.buttonDeleteSession).SetBackgroundResource(Resource.Drawable.ic_delete_white_24dp);

			//get delete button
			var deleteButton = view.FindViewById<Button>(Resource.Id.buttonDeleteSession);
			//only set button click events once; prevent 'first button deleting everything' issue
			if (!deleteButton.HasOnClickListeners) {

				//set delete button click
				view.FindViewById<Button>(Resource.Id.buttonDeleteSession).Click += delegate {
					//context.RunOnUiThread(() => { Toast.MakeText(context, "Delete " + session.Title, ToastLength.Short).Show(); });
					sessions.Remove(session);
					deleteSessionListener.OnDeleteSession(session);
				};
			}

			return view;
		}
	}
}