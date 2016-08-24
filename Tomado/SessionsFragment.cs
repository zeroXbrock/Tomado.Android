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
									SessionAdapter.ClickEditButtonListener, SessionAdapter.SetRecurrenceListener {
		//view instances
		ListView listViewSessions;
		FloatingActionButton newSessionButton, searchButton;
		FloatingActionMenu newSessionMenu;
		SwipeRefreshLayout swipeRefreshLayout;
		View rootView;

		//will be view instances
		DatePickerDialogFragment dateDialog;
		TimePickerDialogFragment timeDialog;

		//keep track of item being edited
		int editIndex = -1;
		float editSessionY;
		string title = "";

		//listener to send click event back to activity
		SessionAdapter.SessionClickListener sessionClickListener;

		//listview state info
		IParcelable listViewState;
				
		//private sessions list for listview
		List<Session> _sessions;

		int lastSessionIndex = -1;
		int lastSessionID = -1;
		Session lastSession;
		
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

			if (savedInstanceState != null)
				editIndex = savedInstanceState.GetInt("editIndex");

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

			if (savedInstanceState != null){ //NA b/c app is non-rotatable
				//get editIndex
				editIndex = savedInstanceState.GetInt("editIndex");
				title = savedInstanceState.GetString("title");
			}

			//get our base layout
			rootView = inflater.Inflate(Resource.Layout.Sessions, container, false);

			//get view instances
			listViewSessions = rootView.FindViewById<ListView>(Resource.Id.listViewSessions);
			//listViewSessions.ContextClick += delegate { HideFabMenu(); }; //LAGS LISTVIEW... DISABLING UNTIL ANOTHER METHOD IS FOUND
			

			swipeRefreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.SwipeRefreshLayout_Sessions);
			
			newSessionMenu = rootView.FindViewById<FloatingActionMenu>(Resource.Id.menu_newSession);

			//set listview mode to allow overscroll
			listViewSessions.OverScrollMode = OverScrollMode.Always;

			//add footer to listview
			AddFooter(inflater);

			
			//instantiate views
			newSessionButton = new FloatingActionButton(Activity);
			newSessionButton.SetImageResource(Resource.Drawable.ic_add_white_24dp);
			newSessionButton.LabelText = "New Session";

			searchButton = new FloatingActionButton(Activity);
			searchButton.SetImageResource(Resource.Drawable.ic_search_white_24dp);
			searchButton.LabelText = "Find free time";

			//set button events
			newSessionButton.Click += delegate {
				//open new session dialog fragment
				ShowNewSessionDialog();
			};

			searchButton.Click += delegate {
				ShowFreeTimeDialog();
			};

			//set menu button style
			newSessionMenu.SetMenuButtonColorNormalResId(Resource.Color.base_app_complementary_color);

			//add buttons to menu
			newSessionMenu.AddMenuButton(newSessionButton);
			newSessionMenu.AddMenuButton(searchButton);

			//set refresh event
			swipeRefreshLayout.Refresh += OnRefresh;

			//get sessions from database and update listView
			LoadSessionsFromDatabase().ContinueWith(t => {
				Activity.RunOnUiThread(() => { 
					ResetListViewAdapter(); //listViewSessions is guaranteed to be instantiated now
					//set edit view if there is one
					if (editIndex > 0 && savedInstanceState != null) {
						var adapter = (SessionAdapter)listViewSessions.Adapter;

						//get item at index
						var itemView = adapter.GetView(editIndex, null, null);

						//call edit button click to open the edit menu
						itemView.FindViewById<ImageButton>(Resource.Id.imageButtonEditSession).CallOnClick();
					}
				});
			});

			//return the inflated/modified base layout
			return rootView;
		}

		public override void OnSaveInstanceState(Bundle outState) {
			base.OnSaveInstanceState(outState);

			outState.PutString("title", title);
			outState.PutInt("editIndex", editIndex);
		}

		private void HideFabMenu() {
			if (newSessionMenu.IsOpened)
				newSessionMenu.Close(true);
		}

		private void AddFooter(LayoutInflater inflater) {
			//get projected height
			
			int height = newSessionMenu.MeasuredHeight * 3; //being lazy for now

			//inflate the footer
			LinearLayout footer = (LinearLayout)inflater.Inflate(Resource.Layout.ListViewFooter, listViewSessions, false);
			
			//get footer layout params
			var layoutParams = footer.LayoutParameters;

			//adjust params
			layoutParams.Height = height;
			footer.RequestLayout();			

			//add footer to listview
			listViewSessions.AddFooterView(footer);
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
		}
		

		public void OnClickEditButton(int sessionIndex) {
			//use editindex to check for recently added freetime
			if (sessionIndex >= 0) {
				//store last index used as well as the original session; to check for any changes to it
				lastSessionIndex = sessionIndex;
				lastSession = new Session(_sessions[sessionIndex].ID, 
					_sessions[sessionIndex].StartHour, _sessions[sessionIndex].StartMinute,
					_sessions[sessionIndex].Year, _sessions[sessionIndex].MonthOfYear, _sessions[sessionIndex].DayOfMonth,
					_sessions[sessionIndex].Title, _sessions[sessionIndex].RecurringDays);

				//used for primitive method
				editSessionY = 800;

				listViewState = listViewSessions.OnSaveInstanceState();
			}
			else {
				//update notification info on close edit view
				//if it is recurring, it'll be set by the recurrence listener
				if (!_sessions[editIndex].Recurring)
					ScheduleSessionNotification(_sessions[editIndex]);
			}

			ResetListViewAdapter(sessionIndex);

			if (listViewState != null)
				listViewSessions.OnRestoreInstanceState(listViewState);

			UpdateEditIndex(sessionIndex);

			//close KB
			HideKeyboard();
		}

		void AdjustListViewScroll() {
			//listViewSessions.SetSelection(lastSessionIndex);
			listViewSessions.SetSelectionFromTop(lastSessionIndex, (int)editSessionY);
		}

		/// <summary>
		/// Updates title of session at given index with given title.
		/// </summary>
		/// <param name="sessionIndex"></param>
		/// <param name="title"></param>
		public void OnTitleSet(int sessionIndex, string title) {
			if (title != "") {
				_sessions[sessionIndex].Title = title;

				DeleteSessionFromDatabase(_sessions[sessionIndex].ID).ContinueWith(t => {
					
					SaveSessionToDatabase(_sessions[sessionIndex]);
				});

				ResetListViewAdapter(sessionIndex);
			}
			
			//close KB
			HideKeyboard();
		}

		/// <summary>
		/// Updates given session with new values from weekdayButtons.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="weekdayButtons"></param>
		public void OnSetRecurrence(int sessionIndex, Session session, List<DayOfWeek> recurringDays) {
			//make changes if the weekday lists are any different
			if (session.RecurringDays == null || !recurringDays.SequenceEqual<DayOfWeek>(session.RecurringDays)) {
				//set recurrence on session
				_sessions[sessionIndex].RecurringDays = recurringDays;
				
				//delete old session from database
				DeleteSessionFromDatabase(_sessions[sessionIndex].ID).ContinueWith(async t => {
					//save new session to database
					await SaveSessionToDatabase(_sessions[sessionIndex]);
				});

				//schedule that notification
				ScheduleSessionNotification(_sessions[sessionIndex], recurringDays);
			}
		}

		public void OnShowDatePickerDialog(int sessionIndex) {
			UpdateEditIndex(sessionIndex);

			Session session = _sessions[sessionIndex];

			DateTime dateTime = new DateTime(session.Year, session.MonthOfYear, session.DayOfMonth);

			dateDialog = new DatePickerDialogFragment(Context, dateTime, this);
			
			//set dialog to retain instance; prevents it from crashing the app
			dateDialog.RetainInstance = true; 
			dateDialog.Show(FragmentManager, "dialog");
		}

		public void OnShowTimePickerDialog(int sessionIndex) {
			UpdateEditIndex(sessionIndex);

			Session session = _sessions[sessionIndex];

			DateTime dateTime = new DateTime(session.Year, session.MonthOfYear, session.DayOfMonth, session.StartHour, session.StartMinute, 0);

			timeDialog = new TimePickerDialogFragment(Context, dateTime, this);

			timeDialog.RetainInstance = true;
			timeDialog.Show(FragmentManager, "dialog");
		}

		/// <summary>
		/// Called when user clicks delete button on a session in the list; implementation of method from OnDeleteSessionListener.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="ID"></param>
		public void OnDeleteSession(Session session) {
			DeleteSession(session);

			ResetListViewAdapter();

			DeleteSessionFromDatabase(session);
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
		public void OnAddNewSession(DateTime dateTime, string title, List<DayOfWeek> recurringDays = null) {
			//close fab menu button
			newSessionMenu.Close(true);

			//Add the session to _sessions list.
			Session session = AddSession(1, dateTime, title, recurringDays);

			//reset the listview adapter
			ResetListViewAdapter(editIndex);

			//update the database
			SaveSessionToDatabase(session);

			//get correct database ID for the notification
			LoadSessionsFromDatabase().ContinueWith(t => {
				session = _sessions[_sessions.Count - 1];

				//schedule the notification if launched from new session dialog
				if (editIndex < 0)
					ScheduleSessionNotification(session);
			});
		}

		/// <summary>
		/// Overload of matching function to accept session.
		/// </summary>
		/// <param name="session"></param>
		private void OnAddNewSession(Session session) {
			DateTime dateTime = new DateTime(session.Year, session.MonthOfYear, session.DayOfMonth, session.StartHour, session.StartMinute, 0);
			string title = session.Title;
			var recurringDays = session.RecurringDays;

			OnAddNewSession(dateTime, title, recurringDays);
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

			session.MonthOfYear--;

			//add the session
			OnAddNewSession(session);
		}

		/// <summary>
		/// Called when receiving event to show delete session dialog
		/// </summary>
		/// <param name="session"></param>
		public void OnShowDeleteSessionDialog(Session session) {
			ShowDeleteSessionDialog(session);
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

			dialog.RetainInstance = true;

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

			dialog.RetainInstance = true;
			
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
		/// Populate class listview with sessions and re-scrolls the listview.
		/// </summary>
		private void ResetListViewAdapter(int editSessionIndex = -1) {
			listViewSessions.Adapter = new SessionAdapter(Activity, _sessions, this, this, this, this, this, this, this, this, this, title, editSessionIndex);
		}

		private void CancelSessionNotification(int ID) {
			var manager = NotificationManager.FromContext(Context);
			manager.Cancel(ID);
		}

		/// <summary>
		/// Adds a session to the class sessions list and returns the session it created.
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		private Session AddSession(int ID, DateTime dateTime, string title, List<DayOfWeek> recurringDays) {
			//add the new session
			Session session = new Session(ID, dateTime, title, recurringDays);
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

			List<DayOfWeek> recurringDays = session.RecurringDays;

			return AddSession(ID, datetime, title, recurringDays);
		}

		/// <summary>
		/// Remove session from class sessions list
		/// </summary>
		/// <param name="session"></param>
		void DeleteSession(Session session) {
			//remove session from class sessions list
			if (_sessions.Remove(session))
				Log.Debug("remove", "success");
			else {
				//try to delete item by ID match
				if (DeleteSession(session.ID))
					Log.Debug("remove", "success by ID match");
				else
					Log.Debug("remove", "fail");
			}
		}

		bool DeleteSession(int ID) {
			foreach (var s in _sessions) {
				if (s.ID == ID) {
					return _sessions.Remove(s);
				}
			}
			return false;
		}

		void HideKeyboard() {
			var mgr = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
			mgr.HideSoftInputFromWindow(View.WindowToken, 0);
		}

		void ShowKeyboard() {

		}

		#region database methods
		/// <summary>
		/// Asynchronously saves a new session to the database.
		/// </summary>
		/// <param name="pathToDatabase"></param>
		/// <param name="session"></param>
		private async Task<string> SaveSessionToDatabase(Session session) {
			session.RecurringDaysCSV = session.ParseDaysToCSV(session.RecurringDays);
			
			return await insertUpdateData(session);
		}

		/// <summary>
		/// Deletes a session from the class session list (& listview) and the database
		/// </summary>
		private async Task<int> DeleteSessionFromDatabase(Session session) {
			//remove session from database
			return await connection.DeleteAsync(session);
		}

		/// <summary>
		/// Deletes session from the database and removes its notification.
		/// </summary>
		/// <param name="ID"></param>
		private async Task<int> DeleteSessionFromDatabase(int ID) {
			CancelSessionNotification(ID);
			
			return await connection.ExecuteScalarAsync<int>("DELETE FROM Session WHERE ID=" + ID + ";");
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
						s.RecurringDays = s.ParseCSVToDays(s.RecurringDaysCSV);
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
		public void ScheduleSessionNotification(Session session, List<DayOfWeek> recurringDays = null) {
			if (lastSession == null || !Session.IdenticalSessions(session, lastSession)) {
				Intent alarmIntent = new Intent(Activity, typeof(AlarmReceiver));

				alarmIntent.PutExtra("ID", session.ID);
				alarmIntent.PutExtra("title", "Tomado");
				alarmIntent.PutExtra("content", session.Title);

				//makes new notification or updates pre-existing one
				PendingIntent pendingIntent = PendingIntent.GetBroadcast(Activity, 1, alarmIntent, PendingIntentFlags.UpdateCurrent); //ID:1 for session notifications
				AlarmManager alarmManager = (AlarmManager)Activity.GetSystemService(Context.AlarmService);

				DateTime now = DateTime.Now;
				DateTime sessionDateTime = new DateTime(session.Year, session.MonthOfYear + 1, session.DayOfMonth, session.StartHour, session.StartMinute, 0);

				if (session.Recurring && recurringDays != null) {
					//set recurring event

					long ticksPerWeek = TimeSpan.TicksPerDay * 7;
					long millisPerWeek = ticksPerWeek / TimeSpan.TicksPerMillisecond;

					//strictly the date of the session; at midnight
					DateTime sessionDate = new DateTime(sessionDateTime.Year, sessionDateTime.Month, sessionDateTime.Day, 0, 0, 0);

					//ticks from midnight to time of event
					long ticksFromMidnight = sessionDateTime.Ticks - sessionDate.Ticks;

					foreach (var i in recurringDays) {
						sessionDate = new DateTime(sessionDateTime.Year, sessionDateTime.Month, sessionDateTime.Day, 0, 0, 0);

						//days from now until toggled weekday
						int daysUntilEvent = 0;
						while (sessionDate.DayOfWeek != i) {
							sessionDate = sessionDate.AddDays(1);
							daysUntilEvent++;
						}


						//get calendar to set alarm
						Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
						calendar.Set(Java.Util.CalendarField.HourOfDay, session.StartHour);
						calendar.Set(Java.Util.CalendarField.Minute, session.StartMinute);
						calendar.Add(Java.Util.CalendarField.DayOfMonth, daysUntilEvent);

						long alarmInterval = AlarmManager.IntervalDay * 7;

						//set alarm for this occurence
						alarmManager.SetRepeating(AlarmType.RtcWakeup, calendar.TimeInMillis, alarmInterval, pendingIntent);
					}
				}
				else {
					//set non-recurring event for session date/time

					long startTicks = sessionDateTime.Ticks - now.Ticks;
					long startMillis = startTicks / TimeSpan.TicksPerMillisecond;

					alarmManager.SetExact(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + startMillis, pendingIntent);
				}
			}
		}
		
	}
}