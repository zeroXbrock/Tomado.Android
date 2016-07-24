using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;

namespace Tomado {
	public class SessionsFragment : Android.Support.V4.App.Fragment, NewSessionFragment.OnGetNewSessionListener {
		ListView listViewSessions;
		Button newSessionButton;
		Android.Net.Uri calendarsUri;
		
		//private sessions list for listview
		List<Session> _sessions;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			//return base.OnCreateView(inflater, container, savedInstanceState);

			//get our base layout
			View rootView = inflater.Inflate(Resource.Layout.Sessions, container, false);

			//get view instances
			listViewSessions = rootView.FindViewById<ListView>(Resource.Id.listViewSessions);
			newSessionButton = rootView.FindViewById<Button>(Resource.Id.buttonNewSession);

			newSessionButton.Click += delegate {
				//open new session dialog fragment (TODO: implement dialog fragment)
				NewSessionFragment fragment = new NewSessionFragment();
				ShowNewSessionDialog();

			};

			//modify layout views
			GetSessions();
			PopulateListView();

			//return the inflated/modified base layout
			return rootView;
		}

		public void OnAddNewSession(DateTime dateTime, string title) {
			AddSession(dateTime, title);
		}

		void ShowNewSessionDialog() {
			Android.Support.V4.App.FragmentTransaction ft = FragmentManager.BeginTransaction();
			
			//some code to remove any existing dialogs
			Android.Support.V4.App.Fragment prev = FragmentManager.FindFragmentByTag("dialog");
			if (prev != null) {
				ft.Remove(prev);
			}
			
			ft.AddToBackStack(null);

			//create and show dialog
			NewSessionFragment dialog = new NewSessionFragment();

			dialog.SetTargetFragment(this, 0);

			dialog.Show(FragmentManager, "dialog");
		}

		//populate listview with sessions
		private void PopulateListView() {
			listViewSessions.Adapter = new SessionAdapter(Activity, Sessions);
		}

		//TODO: implement me
		//retrieve stored sessions from IS
		private void GetSessions() {
			if (_sessions == null || _sessions.Count == 0) {
				_sessions = new List<Session>();

				//Keep these dummy ones here until you get persistent storage working
				_sessions.Add(new Session(12, 30, 2016, 7, 23, "study math"));
				_sessions.Add(new Session(2, 0, 2016, 7, 23, "jack off"));
				_sessions.Add(new Session(3, 30, 2016, 7, 23, "eat 4 sandwiches"));
			}
		}

		//TODO: Implement me
		private List<Session> GetFreeTimeSessions() {
			List<Session> freeTimeSessions = new List<Session>();
			var cursor = getCalendarICursor();

			//iterate through calendar

			//populate list

			//return list
			return freeTimeSessions;
		}

		private void AddSession(DateTime date, string title) {
			//add the new session
			Session session = new Session(date, title);
			_sessions.Add(session);

			//reset the listview adapter
			PopulateListView();
		}

		//get cursor to browse calendar events
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
										Resource.Id.evStart,
										Resource.Id.evEnd
									};

			//get a cursor to browse calendar events
			var loader = new CursorLoader(Activity, calendarsUri, calendarsProjection, null, null, null);
			var cursor = (ICursor)loader.LoadInBackground(); //runs asynch

			return cursor;
		}

		

		public List<Session> Sessions {
			get { return _sessions; }

		}
	}

	public class DatePickerDialogFragment : Android.Support.V4.App.DialogFragment {
		private readonly Context _context;
		private DateTime _date;
		private readonly DatePickerDialog.IOnDateSetListener _listener;

		public DatePickerDialogFragment(Context context, DateTime date, DatePickerDialog.IOnDateSetListener listener){
			_context = context;
			_date = date;
			_listener = listener;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState) {
			var dialog = new Android.App.DatePickerDialog(_context, _listener, _date.Year, _date.Month, _date.Day);
			return dialog;
		}
	}

	public class TimePickerDialogFragment : Android.Support.V4.App.DialogFragment {
		private readonly Context _context;
		private DateTime _time;
		private readonly TimePickerDialog.IOnTimeSetListener _listener;

		public TimePickerDialogFragment(Context context, DateTime time, TimePickerDialog.IOnTimeSetListener listener){
			_context = context;
			_time = time;
			_listener = listener;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState) {
			var dialog = new Android.App.TimePickerDialog(_context, _listener, DateTime.Now.Hour, DateTime.Now.Minute, false);
			return dialog;
		}
	}
}