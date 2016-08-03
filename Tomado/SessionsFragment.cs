using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views.InputMethods;

using Clans.Fab; //floating buttons

using Java.Lang;
using SQLite;


namespace Tomado {
	/// <summary>
	/// Fragment that display a list of Sessions.
	/// </summary>
	public class SessionsFragment : Android.Support.V4.App.Fragment, FreeTimeFragment.GetNewFreeTimeListener, NewSessionFragment.GetNewSessionListener, 
									SessionAdapter.DeleteSessionListener, SessionAdapter.SessionClickListener, SessionAdapter.ShowDeleteSessionDialogListener,
									DatePickerDialog.IOnDateSetListener, TimePickerDialog.IOnTimeSetListener,
									SessionAdapter.ShowTimePickerDialogListener, SessionAdapter.ShowDatePickerDialogListener, SessionAdapter.TitleSetListener,
									SessionAdapter.ClickEditButtonListener {
		//view instances
		ListView listViewSessions;
		FloatingActionButton newSessionButton, searchButton;
		FloatingActionMenu newSessionMenu;
		SwipeRefreshLayout swipeRefreshLayout;

		//keep track of item being edited
		int editIndex = -1;

		//listener to send click event back to activity
		SessionAdapter.SessionClickListener sessionClickListener;

		
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
			createDatabase(pathToDatabase).ContinueWith(t => {
				
			});

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

			swipeRefreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.SwipeRefreshLayout_Sessions);
			
			newSessionMenu = rootView.FindViewById<FloatingActionMenu>(Resource.Id.menu_newSession);

			//set listview mode to allow overscroll
			listViewSessions.OverScrollMode = OverScrollMode.Always;
			
			newSessionButton = new FloatingActionButton(Activity);
			newSessionButton.SetImageResource(Resource.Drawable.ic_add_white_24dp);
			newSessionButton.LabelText = "New Session";

			searchButton = new FloatingActionButton(Activity);
			searchButton.SetImageResource(Resource.Drawable.ic_search_white_24dp);
			searchButton.LabelText = "Find free time";

			newSessionButton.Click += delegate {
				//open new session dialog fragment
				//NewSessionFragment fragment = new NewSessionFragment();
				ShowNewSessionDialog();
			};

			searchButton.Click += delegate {
				ShowFreeTimeDialog();
			};

			newSessionMenu.SetMenuButtonColorNormalResId(Resource.Color.base_app_complementary_color);

			newSessionMenu.AddMenuButton(newSessionButton);
			newSessionMenu.AddMenuButton(searchButton);

			//swipeRefreshLayout.SetOnRefreshListener(this);
			swipeRefreshLayout.Refresh += OnRefresh;

			//get sessions from database and update listView
			LoadSessionsFromDatabase().ContinueWith(t => {
				Activity.RunOnUiThread(() => { ResetListViewAdapter(); });
			});			

			//return the inflated/modified base layout
			return rootView;
		}

		/// <summary>
		/// Method to keep track of session being edited
		/// </summary>
		/// <param name="index"></param>
		private void UpdateEditIndex(int index) {
			editIndex = index;
		}

		/// <summary>
		/// Called when datepickerdialog closes w/ OK and was started from this context
		/// </summary>
		/// <param name="datePicker"></param>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <param name="day"></param>
		public void OnDateSet(DatePicker datePicker, int year, int month, int day) {
			DeleteSessionFromDatabase(_sessions[editIndex].ID);

			//update _sessions
			_sessions[editIndex].Year = year;
			_sessions[editIndex].MonthOfYear = month;
			_sessions[editIndex].DayOfMonth = day;

			//update database w/ new session info
			SaveSessionToDatabase(_sessions[editIndex]);

			//reset adapter & edit view on session in list
			ResetListViewAdapter(editIndex);

			//scroll to new item
			listViewSessions.SetSelection(listViewSessions.Count - 1);
		}

		/// <summary>
		/// Called when timepickerdialog closes w/ OK and was started from this context
		/// </summary>
		/// <param name="timePicker"></param>
		/// <param name="hour"></param>
		/// <param name="minute"></param>
		public void OnTimeSet(TimePicker timePicker, int hour, int minute){
			DeleteSessionFromDatabase(_sessions[editIndex].ID);

			//update _sessions
			_sessions[editIndex].StartHour = hour;
			_sessions[editIndex].StartMinute = minute;

			//update database w/ new session info
			SaveSessionToDatabase(_sessions[editIndex]);

			//reset adapter, open edit view on session in list
			ResetListViewAdapter(editIndex);

			//scroll to new item
			listViewSessions.SetSelection(listViewSessions.Count - 1);
		}
		int lastSessionIndex = -1;

		public void OnClickEditButton(int sessionIndex) {
			UpdateEditIndex(sessionIndex);

			ResetListViewAdapter(sessionIndex);

			if (sessionIndex >= 0)
				lastSessionIndex = sessionIndex;
			//scroll to item being edited
			listViewSessions.SetSelection(lastSessionIndex);
			//listViewSessions.SetSelectionFromTop

			//close KB
			var mgr = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
			mgr.HideSoftInputFromWindow(View.WindowToken, 0);
		}

		public void OnTitleSet(int sessionIndex, string title) {
			if (title != "") {
				DeleteSessionFromDatabase(_sessions[sessionIndex].ID);

				_sessions[sessionIndex].Title = title;

				ResetListViewAdapter(sessionIndex);

				SaveSessionToDatabase(_sessions[sessionIndex]);

				//scroll to new item
				listViewSessions.SetSelection(sessionIndex);
			}
			
			//close KB
			var mgr = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
			mgr.HideSoftInputFromWindow(View.WindowToken, 0);

			
		}

		public void OnShowDatePickerDialog(int sessionIndex) {
			UpdateEditIndex(sessionIndex);

			DatePickerDialogFragment dialog = new DatePickerDialogFragment(Context, DateTime.Now, this);
			dialog.Show(FragmentManager, "dialog");
		}

		public void OnShowTimePickerDialog(int sessionIndex) {
			UpdateEditIndex(sessionIndex);

			TimePickerDialogFragment dialog = new TimePickerDialogFragment(Context, DateTime.Now, this);
			dialog.Show(FragmentManager, "dialog");
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
			//close fab menu button
			newSessionMenu.Close(true);

			//Add the session to _sessions list.
			Session session = AddSession(1, dateTime, title, recurring);

			//reset the listview adapter
			ResetListViewAdapter(editIndex);

			//update the database
			SaveSessionToDatabase(session);

			//get correct database ID for the notification
			LoadSessionsFromDatabase().ContinueWith(t => {
				session = _sessions[_sessions.Count - 1];

				//schedule the notification
				ScheduleSessionNotification(session);
			});
		}

		public void OnShowDeleteSessionDialog(Session session) {
			ShowDeleteSessionDialog(session);
		}

		/// <summary>
		/// Overload of matching function to accept session.
		/// </summary>
		/// <param name="session"></param>
		private void OnAddNewSession(Session session) {
			DateTime dateTime = new DateTime(session.Year, session.MonthOfYear, session.DayOfMonth, session.StartHour, session.StartMinute, 0);
			string title = session.Title;
			bool recurring = session.Recurring;

			OnAddNewSession(dateTime, title, recurring);
		}
		
		/// <summary>
		/// Shows a NewSessionDialog fragment above the current view.
		/// </summary>
		private void ShowNewSessionDialog() {
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
		/// Opens a new free time dialog.
		/// </summary>
		private void ShowFreeTimeDialog() {
			Android.Support.V4.App.FragmentTransaction ft = FragmentManager.BeginTransaction();

			//some code to remove any existing dialogs
			Android.Support.V4.App.Fragment prev = FragmentManager.FindFragmentByTag("dialog");
			if (prev != null) {
				ft.Remove(prev);
			}

			ft.AddToBackStack(null);

			//create and show dialog
			FreeTimeFragment dialog = new FreeTimeFragment(this);
			
			dialog.SetTargetFragment(this, 0);

			dialog.Show(FragmentManager, "dialog");
		}

		/// <summary>
		/// Shows a dialog asking user to delete session
		/// </summary>
		private void ShowDeleteSessionDialog(Session session) {
			Android.Support.V4.App.FragmentTransaction ft = FragmentManager.BeginTransaction();

			//some code to remove any existing dialogs
			Android.Support.V4.App.Fragment prev = FragmentManager.FindFragmentByTag("dialog");
			if (prev != null) {
				ft.Remove(prev);
			}

			ft.AddToBackStack(null);

			//create and show dialog
			var dialog = new DeleteSessionFragment(session, this);

			dialog.SetTargetFragment(this, 0);
			

			dialog.Show(FragmentManager, "dialog");
		}

		/// <summary>
		/// Populate class listview with sessions.
		/// </summary>
		private void ResetListViewAdapter(int editSessionIndex = -1) {
			listViewSessions.Adapter = new SessionAdapter(Activity, _sessions, this, this, this, this, this, this, this, this, editSessionIndex);
		}

		/// <summary>
		/// Handler for SwipeRefreshView
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public async void OnRefresh(object sender, EventArgs e) {
			//load sessions from database
			await LoadSessionsFromDatabase();
			Activity.RunOnUiThread(() => { ResetListViewAdapter(); });
			swipeRefreshLayout.Refreshing = false;
		}

		/// <summary>
		/// Called when user creates a new freetime session
		/// </summary>
		/// <param name="session"></param>
		public void OnGetNewFreeTime(Session session) {
			//update index
			UpdateEditIndex(_sessions.Count);
			
			//add the session
			OnAddNewSession(session);

			//scroll to new item
			listViewSessions.SetSelection(listViewSessions.Count - 1);

			//show keyboard
			//Activity.Window.SetSoftInputMode(SoftInput.StateVisible);
			//var mgr = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
			//mgr.ShowSoftInputFromInputMethod(View.ApplicationWindowToken, ShowFlags.Forced);
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

		private async void DeleteSessionFromDatabase(int ID) {
			await connection.ExecuteScalarAsync<int>("DELETE FROM Session WHERE ID=" + ID + ";");
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