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
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;

using Clans.Fab; //floating buttons

using Java.Lang;
using SQLite;


namespace Tomado {
	/// <summary>
	/// Fragment that display a list of Sessions.
	/// </summary>
	public class SessionsFragment : Android.Support.V4.App.Fragment, NewSessionFragment.OnGetNewSessionListener, SessionAdapter.DeleteSessionListener, SessionAdapter.SessionClickListener {
		//view instasnces
		ListView listViewSessions;
		FloatingActionButton newSessionButton;

		//listener to send click event back to activity
		SessionAdapter.SessionClickListener sessionClickListener;

		//calendar location
		Android.Net.Uri calendarsUri;
		
		//private sessions list for listview
		List<Session> _sessions;

		//accessor
		public List<Session> Sessions {
			get { return _sessions; }

		}

		//database info
		private string docsFolder;
		string pathToDatabase;
		SQLiteAsyncConnection connection;

		//constructors
		public SessionsFragment(SessionAdapter.SessionClickListener sessionClickListener){
			this.sessionClickListener = sessionClickListener;
		}
		public SessionsFragment() { /*u fuckin wot m8*/ }


		public override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			//get database path
			docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			pathToDatabase = System.IO.Path.Combine(docsFolder, "sessions.db");

			//create database
			createDatabase(pathToDatabase);

			//instantiate sessions list if first run
			if (_sessions == null)
				_sessions = new List<Session>();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			//return base.OnCreateView(inflater, container, savedInstanceState);

			//get our base layout
			View rootView = inflater.Inflate(Resource.Layout.Sessions, container, false);

			//get view instances
			listViewSessions = rootView.FindViewById<ListView>(Resource.Id.listViewSessions);
			newSessionButton = rootView.FindViewById<FloatingActionButton>(Resource.Id.buttonNewSession);
			
			//add plus icon to button
			newSessionButton.SetImageResource(Resource.Drawable.ic_add_white_24dp);

			newSessionButton.Click += delegate {
				//open new session dialog fragment (TODO: implement dialog fragment)
				NewSessionFragment fragment = new NewSessionFragment();
				ShowNewSessionDialog();
			};

			//modify layout views
			LoadSessionsFromDatabase().ContinueWith(t => {
				Activity.RunOnUiThread(() => { ResetListViewAdapter(); });
			});
			

			//return the inflated/modified base layout
			return rootView;
		}

		/// <summary>
		/// Called when user clicks delete button on a session in the list; implementation of method from OnDeleteSessionListener.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="ID"></param>
		public void OnDeleteSession(Session session) {
			DeleteSessionFromDatabase(session);
			DeleteSession(session);
			ResetListViewAdapter();
		}

		/// <summary>
		/// Event handler; called when session item from list is clicked.
		/// </summary>
		/// <param name="session"></param>
		public void OnSessionClick(Session session) {
			//send event back to activity; to load timer
			sessionClickListener.OnSessionClick(session);
		}

		/// <summary>
		/// Event handler for NewSessionFragment result
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="title"></param>
		public void OnAddNewSession(DateTime dateTime, string title, bool recurring) {
			//Add the session to _sessions list.
			Session session = AddSession(1, dateTime, title, recurring);

			//reset the listview adapter
			ResetListViewAdapter();

			//update the database
			SaveSessionToDatabase(session);

			//get correct database ID for the notification
			LoadSessionsFromDatabase().ContinueWith(t => {
				session = _sessions[_sessions.Count - 1];

				//schedule the notification
				ScheduleSessionNotification(session);
			});
		}
		
		/// <summary>
		/// Shows a NewSessionDialog fragment above the current view.
		/// </summary>
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

		/// <summary>
		/// Populate class listview with sessions.
		/// </summary>
		private void ResetListViewAdapter() {
			listViewSessions.Adapter = new SessionAdapter(Activity, _sessions, this, this);
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

			//return list
			return freeTimeSessions;
		}

		/// <summary>
		/// Adds a session to the class sessions list and returns the session it created.
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		private Session AddSession(int ID, DateTime dateTime, string title, bool recurring) {
			//add the new session
			Session session = new Session(ID, dateTime, title, recurring);
			_sessions.Add(session);

			return session;
		}
		/// <summary>
		/// Adds a session to the class sessions list and returns the session it created.
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>
		private Session AddSession(Session session) {
			DateTime datetime = new DateTime(session.Year, session.MonthOfYear, session.DayOfMonth, session.StartHour, session.StartMinute, 0);
			string title = session.Title;

			int ID = session.ID;

			bool recurring = session.Recurring;

			return AddSession(ID, datetime, title, recurring);
		}

		/// <summary>
		/// Remove session from class sessions list
		/// </summary>
		/// <param name="session"></param>
		void DeleteSession(Session session) {
			//remove session from class sessions list
			_sessions.Remove(session);
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

		#region database methods
		/// <summary>
		/// Asynchronously saves a new session to the database.
		/// </summary>
		/// <param name="pathToDatabase"></param>
		/// <param name="session"></param>
		private async void SaveSessionToDatabase(Session session) {
			var result = await insertUpdateData(session);
		}

		/// <summary>
		/// Deletes a session from the class session list (& listview) and the database
		/// </summary>
		private async void DeleteSessionFromDatabase(Session session) {
			//remove session from database
			await connection.DeleteAsync(session);
		}

		/// <summary>
		/// Asynchronously loads sessions from database to class sessions list. Result should be obtained with [result].ContinueWith(...).
		/// </summary>
		/// <param name="pathToDatabase"></param>
		/// <returns></returns>
		private async Task<string> LoadSessionsFromDatabase() {
			//clear sessions list
			_sessions = new List<Session>();

			///for each item in the database, add it to the sessions list
			//create/run query
			var records = getRecords();

			try {
				await records.ContinueWith(t => {
					foreach (var s in records.Result) {
						AddSession(s);
					}
				});
				return "Loaded sessions from database";
			}
			catch {
				return "Could not load sessions from database";
			}
		}

		/// <summary>
		/// Creates a database at the given folder path if one doesn't exist. Returns query result string.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private async Task<string> createDatabase(string path) {
			try {
				connection = new SQLiteAsyncConnection(path);
				await connection.CreateTableAsync<Session>();
				return "Database created";
			}
			catch (SQLiteException ex) {
				return ex.Message;
			}
		}

		/// <summary>
		/// Inserts a Session object into the databse or updates if matching object is already present.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		private async Task<string> insertUpdateData(Session data) {
			try {
				if (await connection.InsertAsync(data) != 0)
					await connection.UpdateAsync(data);
				return "Single data file inserted or updated";
			}
			catch (SQLiteException ex) {
				return ex.Message;
			}
		}

		/// <summary>
		/// Returns a (task) list of Sessions retrieved from database. Must be run asynchronously.
		/// Result should be retrieved with [result].ContinueWith(...).
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private async Task<List<Session>> getRecords() {
			try {
				var query = connection.Table<Session>();

				var records = await query.ToListAsync();

				return records;
			}
			catch {
				return null;
			}
		}
		#endregion

		/// <summary>
		/// Schedules a notification to launch in the future; to open a session from
		/// </summary>
		/// <param name="session"></param>
		public async void ScheduleSessionNotification(Session session) {
			Intent alarmIntent = new Intent(Activity, typeof(AlarmReceiver));
			
			alarmIntent.PutExtra("ID", session.ID);
			alarmIntent.PutExtra("title", "Tomado");
			alarmIntent.PutExtra("content", session.Title);

			PendingIntent pendingIntent = PendingIntent.GetBroadcast(Activity, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);
			AlarmManager alarmManager = (AlarmManager)Activity.GetSystemService(Context.AlarmService);
			
			if (session.Recurring) {
				//set recurring event
			}
			else {
				//set non-recurring event for session date/time
				DateTime now = DateTime.Now.ToUniversalTime();
				DateTime sessionDateTime = new DateTime(session.Year, session.MonthOfYear, session.DayOfMonth, session.StartHour, session.StartMinute, 0).ToUniversalTime();
				long intervalTicks = sessionDateTime.Ticks - now.Ticks;
				long intervalMillis = intervalTicks / TimeSpan.TicksPerMillisecond;

				alarmManager.SetExact(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + intervalMillis, pendingIntent);//sessionDateTime.Ticks/TimeSpan.TicksPerMillisecond
			}
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