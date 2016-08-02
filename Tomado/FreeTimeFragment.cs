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

		List<FreeTime> freeTime;

		public FreeTimeFragment(GetNewFreeTimeListener getNewFreeTimeListener) {
			this.getNewFreeTimeListener = getNewFreeTimeListener;
		}

		public interface GetNewFreeTimeListener {
			void OnGetNewFreeTime(Session session);
		}

		private GetNewFreeTimeListener getNewFreeTimeListener;

		//calendar vars
		Android.Net.Uri calendarsUri;
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

		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			//get free time, make list of sessions
			freeTime = GetFreeTime();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			Dialog.SetTitle("Free time sessions");
			Dialog.Window.SetBackgroundDrawableResource(Resource.Color.base_app_complementary_color);

			View view = inflater.Inflate(Resource.Layout.FreeTimeDialog, container, false);

			listViewFreeTime = view.FindViewById<ListView>(Resource.Id.listViewFreeTimeSessions);

			ResetListAdapter();

			return view;
		}

		private void ResetListAdapter() {
			listViewFreeTime.Adapter = new FreeTimeAdapter(Activity, freeTime, this);
		}

		/// <summary>
		/// Runs when a free time session is clicked from the dialog's list
		/// </summary>
		/// <param name="session"></param>
		public void OnSessionClick(Session session) {
			//click fires more than once so check to verify that the dialog is open
			if (Dialog != null && Dialog.IsShowing) {
				Dismiss();

				//send session back
				getNewFreeTimeListener.OnGetNewFreeTime(session);
			}
		}

		//TODO: Implement me
		/// <summary>
		/// Returns a list of sessions gathered from a free time analysis of the calendar.
		/// </summary>
		/// <returns></returns>
		private List<FreeTime> GetFreeTime() {
			var freetime = new List<FreeTime>();
			var cursor = getCalendarICursor();

			//iterate through calendar
			cursor.MoveToFirst();

			//get cursor to browse calendar events
			MatrixCursor selectedEventsCursor = new MatrixCursor(calendarsProjection);
			DateTime start = DateTime.Now.ToLocalTime();
			DateTime tomorrow = DateTime.Today.ToLocalTime().AddDays(1);
			DateTime now = DateTime.Now.ToLocalTime();

			//iterate through items to find selected events
			while (cursor.MoveToNext()) {
				//start & end date define period to look in for events
				DateTime sDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(cursor.GetLong(2)).ToLocalTime();//index 2 is start date; 3 is end date
				DateTime eDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(cursor.GetLong(3)).ToLocalTime();

				if (sDate >= now && sDate < tomorrow) {
					//add item to selected events
					selectedEventsCursor.AddRow(new Java.Lang.Object[] { cursor.GetString(0), cursor.GetString(1), eDate.ToString(), sDate.ToString() });

					// if event starts after our starting search point, add free time
					if (sDate > start) {
						FreeTime freeTimeChunk = new FreeTime() { Start = start };
						freeTimeChunk.End = sDate;
						freetime.Add(freeTimeChunk);

						//reset start for next iteration
						start = eDate;
					}
				}
				// after last event, add free time from end of event until midnight, as long as event ends before midnight
				else if (eDate >= now && sDate < tomorrow && eDate < tomorrow) {
					freetime.Add(new FreeTime() { Start = eDate, End = tomorrow });
				}
			}

			if (selectedEventsCursor.Count == 0) {
				freetime.Add(new FreeTime() { Start = start, End = tomorrow });
			}

			//return list
			return freetime;
		}

		/// <summary>
		/// Returns cursor made to browse calendar events.
		/// </summary>
		/// <returns></returns>
		private ICursor getCalendarICursor() {
			// Get calendar contract
			calendarsUri = CalendarContract.Events.ContentUri;

			

			//get a cursor to browse calendar events
			var loader = new CursorLoader(Activity, calendarsUri, calendarsProjection, null, null, null);
			var cursor = (ICursor)loader.LoadInBackground(); //runs asynch

			return cursor;
		}
	}
}