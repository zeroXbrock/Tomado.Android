using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Tomado {
	public class FreeTimeFragment : Android.Support.V4.App.DialogFragment, SessionAdapter.SessionClickListener {
		ListView listViewFreeTime;

		List<Session> freeTime;

		int durationHours = 0;
		int durationMinutes = 0;

		//calendar location
		Android.Net.Uri calendarsUri;

		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			//get free time, make list of sessions
			freeTime = GetFreeTimeSessions();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			Dialog.SetTitle("Free time sessions");
			
			View view = inflater.Inflate(Resource.Layout.FreeTimeDialog, container, false);

			listViewFreeTime = view.FindViewById<ListView>(Resource.Id.listViewFreeTimeSessions);

			ResetListAdapter();

			return view;
		}

		private void ResetListAdapter() {
			listViewFreeTime.Adapter = new FreeTimeAdapter(Activity, freeTime, this);
		}

		public void OnSessionClick(Session session) {

		}

		//TODO: Implement me
		/// <summary>
		/// Returns a list of sessions gathered from a free time analysis of the calendar.
		/// </summary>
		/// <returns></returns>
		private List<Session> GetFreeTimeSessions() {
			List<Session> freeTimeSessions = new List<Session>();
			var cursor = getCalendarICursor();

			//iterate through calendar

			//populate list
			freeTimeSessions = new List<Session>();
			freeTimeSessions.Add(new Session(-1, DateTime.Now, "swag", false));
			freeTimeSessions.Add(new Session(-1, DateTime.Now, "bro", false));

			//return list
			return freeTimeSessions;
		}

		/// <summary>
		/// Returns cursor made to browse calendar events.
		/// </summary>
		/// <returns></returns>
		private ICursor getCalendarICursor() {
			// Get calendar contract
			calendarsUri = CalendarContract.Events.ContentUri;

			///This chunk of code displays all calendars on the phone in a list
			//List the fields you want to use
			string[] calendarsProjection = {
											    CalendarContract.Events.InterfaceConsts.Id,
												CalendarContract.EventsColumns.Title,
												CalendarContract.Events.InterfaceConsts.Dtstart,
												CalendarContract.Events.InterfaceConsts.Dtend
										   };
			//list the fields you wanna display
			string[] sourceColumns = {
										 CalendarContract.EventsColumns.Title,
										 CalendarContract.Events.InterfaceConsts.Dtstart,
										 CalendarContract.Events.InterfaceConsts.Dtend
									 };
			//list the views you're gonna display your info with
			int[] targetResources = {
										Resource.Id.evTitle,
										Resource.Id.evTime
									};

			//get a cursor to browse calendar events
			var loader = new CursorLoader(Activity, calendarsUri, calendarsProjection, null, null, null);
			var cursor = (ICursor)loader.LoadInBackground(); //runs asynch

			return cursor;
		}
	}
}