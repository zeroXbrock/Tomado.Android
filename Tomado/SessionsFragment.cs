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

using Java.Lang;
using SQLite;


namespace Tomado {
	public class SessionsFragment : Android.Support.V4.App.Fragment, NewSessionFragment.OnGetNewSessionListener {
		//view instasnces
		ListView listViewSessions;
		Button newSessionButton;

		Android.Net.Uri calendarsUri;
		
		//private sessions list for listview
		List<Session> _sessions;

		//accessor
		public List<Session> Sessions {
			get { return _sessions; }

		}

		private string docsFolder;
		string pathToDatabase;

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
			newSessionButton = rootView.FindViewById<Button>(Resource.Id.buttonNewSession);

			newSessionButton.Click += delegate {
				//open new session dialog fragment (TODO: implement dialog fragment)
				NewSessionFragment fragment = new NewSessionFragment();
				ShowNewSessionDialog();
			};

			//modify layout views
			
			LoadSessionsFromDatabase(pathToDatabase).ContinueWith(t => {
				Activity.RunOnUiThread(() => { ResetListViewAdapter(); });
			});
			
			

			//return the inflated/modified base layout
			return rootView;
		}

		//event handler for NewSessionFragment
		/// <summary>
		/// Event handler for NewSessionFragment result
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="title"></param>
		public void OnAddNewSession(DateTime dateTime, string title) {
			//Add the session to _sessions list.
			Session session = AddSession(1, dateTime, title);

			//reset the listview adapter
			ResetListViewAdapter();

			//update the database
			SaveSessionToDatabase(pathToDatabase, session);
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
		private void ResetListViewAdapter() {
			listViewSessions.Adapter = new SessionAdapter(Activity, _sessions);
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

		//add session to sessions list and returns the session it created
		/// <summary>
		/// Adds a session to the _sessions list.
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		private Session AddSession(int ID, DateTime dateTime, string title) {
			//add the new session
			Session session = new Session(ID, dateTime, title);
			_sessions.Add(session);

			return session;
		}
		/// <summary>
		/// Adds a session to the _sessions list.
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>
		private Session AddSession(Session session) {
			DateTime datetime = new DateTime(session.Year, session.MonthOfYear, session.DayOfMonth, session.StartHour, session.StartMinute, 0);
			string title = session.Title;

			int ID = _sessions.Count+1;
			return AddSession(ID, datetime, title);
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

		//DEPRECATED
		//asynchronously saves session list items to database; either adding new items or updating new values on existing items
		private async void SaveSessionsToDatabase(string pathToDatabase) {
			//for each session in our sessions list, do an insert/update on the DB

			foreach (var s in _sessions) {
				var result = await insertUpdateData(s, pathToDatabase);
				//Log.Debug("SaveSessionsToDatabase result", result.ToString());
			}
		}

		private async void SaveSessionToDatabase(string pathToDatabase, Session session) {
			var result = await insertUpdateData(session, pathToDatabase);
		}

		//asynchronously loads sessions from database to sessions list
		private async Task<string> LoadSessionsFromDatabase(string pathToDatabase) {
			//clear sessions list
			_sessions = new List<Session>();

			///for each item in the database, add it to the sessions list
			//create/run query
			var records = getRecords(pathToDatabase);

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

		private async Task<string> createDatabase(string path) {
			try {
				var connection = new SQLiteAsyncConnection(path);
				await connection.CreateTableAsync<Session>();
				return "Database created";
			}
			catch (SQLiteException ex) {
				return ex.Message;
			}
		}

		private async Task<string> insertUpdateData(Session data, string path) {
			try {
				var db = new SQLiteAsyncConnection(path);
				if (await db.InsertAsync(data) != 0)
					await db.UpdateAsync(data);
				return "Single data file inserted or updated";
			}
			catch (SQLiteException ex) {
				return ex.Message;
			}
		}

		private async Task<List<Session>> getRecords(string path) {
			try {
				var db = new SQLiteAsyncConnection(path);
				//var records = await db.ExecuteAsync("SELECT * FROM Person");

				var query = db.Table<Session>();

				var records = await query.ToListAsync();

				return records;
			}
			catch {
				return null;
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