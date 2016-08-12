using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Graphics.Drawables;
using Android.Util;

using Clans.Fab;

using Java.Lang;

namespace Tomado {
	/// <summary>
	/// Adapter to populate Sessions list.
	/// </summary>
	public class SessionAdapter : BaseAdapter<Session>, Android.Text.ITextWatcher {
		List<Session> sessions;
		Activity context;
		DeleteSessionListener deleteSessionListener;
		SessionClickListener sessionClickListener;
		ShowDeleteSessionDialogListener showDeleteSessionDialogListener;
		TimePickerDialog.IOnTimeSetListener timeSetListener; 
		DatePickerDialog.IOnDateSetListener dateSetListener;
		ShowTimePickerDialogListener timePickerListener;
		ShowDatePickerDialogListener datePickerListener;
		TitleSetListener titleSetListener;
		ClickEditButtonListener openEditViewListener;
		SetRecurrenceListener setRecurrenceListener;

		int editSessionIndex = -1;//used to open edit view for a session in list

		/// <summary>
		/// Interface to provide callback for deleting sessions.
		/// </summary>
		public interface DeleteSessionListener {
			/// <summary>
			/// Implement in containing class to delete session from list and database.
			/// </summary>
			void OnDeleteSession(Session session);
		}

		/// <summary>
		/// Interface to provide callback for clicking a session item in the list.
		/// </summary>
		public interface SessionClickListener {
			/// <summary>
			/// Implement in containing class to lauch when session is clicked.
			/// </summary>
			/// <param name="session"></param>
			void OnSessionClick(Session session);
		}

		public interface ClickEditButtonListener {
			void OnClickEditButton(int sessionIndex);
		}

		public interface SetTimeListener {
			void OnOpenTimeDialog();
		}

		public interface SetRecurrenceListener {
			void OnSetRecurrence(Session session, List<WeekdayButton> weekdayButtons);
		}

		/// <summary>
		/// Listener to handle showing delete session dialog
		/// </summary>
		public interface ShowDeleteSessionDialogListener {
			void OnShowDeleteSessionDialog(Session session);
		}

		public interface ShowTimePickerDialogListener {
			void OnShowTimePickerDialog(int sessionIndex);
		}

		public interface ShowDatePickerDialogListener {
			void OnShowDatePickerDialog(int sessionIndex);
		}

		public interface TitleSetListener {
			void OnTitleSet(int sessionIndex, string title);
		}

		public SessionAdapter(Activity context, List<Session> sessions, SessionClickListener sessionClickListener, ShowDeleteSessionDialogListener showDeleteSessionDialogListener, 
			TimePickerDialog.IOnTimeSetListener timeSetListener, DatePickerDialog.IOnDateSetListener dateSetListener, ShowDatePickerDialogListener datePickerListener, ShowTimePickerDialogListener timePickerListener, 
			TitleSetListener titleSetListener, ClickEditButtonListener openEditViewListener, SetRecurrenceListener setRecurrenceListener, string title, int editSessionIndex = -1) {
			this.context = context;
			this.sessions = sessions;
			this.sessionClickListener = sessionClickListener;
			this.showDeleteSessionDialogListener = showDeleteSessionDialogListener;
			this.timeSetListener = timeSetListener;
			this.dateSetListener = dateSetListener;
			this.datePickerListener = datePickerListener;
			this.timePickerListener = timePickerListener;
			this.editSessionIndex = editSessionIndex;
			this.titleSetListener = titleSetListener;
			this.openEditViewListener = openEditViewListener;
			this.setRecurrenceListener = setRecurrenceListener;
			this.TitleText = title;
		}

		public override long GetItemId(int position) {
			return position;
		}

		public override Session this[int position] {
			get { return sessions[position]; }
		}

		public List<Session> Sessions {
			get {
				return sessions;
			}
		}

		public override int Count {
			get { return sessions.Count; }
		}

		//var to store title text while user types
		public string TitleText {
			get;
			set;
		}

		public override View GetView(int position, View convertView, ViewGroup parent) {
			View view = convertView;//reuse existing view if available
			if (view == null) {
			}//disregard recycling

			view = context.LayoutInflater.Inflate(Resource.Layout.SessionListItem, null);

			//get layout for edit view
			ViewGroup editLayout = view.FindViewById<LinearLayout>(Resource.Id.EditSessionLayout);
			editLayout.Visibility = ViewStates.Gone;

			//get switch and recurring layout
			Switch recurringSwitch = view.FindViewById<Switch>(Resource.Id.switchRecurring_SessionListItem);
			var recurringLayout = view.FindViewById<LinearLayout>(Resource.Id.Layout_Recurring_SessionListItem);

			//get session for this list item
			Session session = sessions[position];
			DateTime dateTime = new DateTime(session.Year, session.MonthOfYear + 1, session.DayOfMonth, session.StartHour, session.StartMinute, 0);

			//get textviews
			var titleTextView = view.FindViewById<TextView>(Resource.Id.evTitle);
			var timeTextView = view.FindViewById<TextView>(Resource.Id.evTime);
			
			//get edittextviews
			var editTextTitle = view.FindViewById<EditText>(Resource.Id.editText_Title_EditSession);
			var editTextDate = view.FindViewById<EditText>(Resource.Id.editText_Date_EditSession);
			var editTextTime = view.FindViewById<EditText>(Resource.Id.editText_Time_EditSession);

			//get weekday buttons; store in a list; Sunday -> Saturday
			List<WeekdayButton> weekdayButtons = new List<WeekdayButton>();
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonSunday_Recurring), DayOfWeek.Sunday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonMonday_Recurring), DayOfWeek.Monday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonTuesday_Recurring), DayOfWeek.Tuesday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonWednesday_Recurring), DayOfWeek.Wednesday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonThursday_Recurring), DayOfWeek.Thursday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonFriday_Recurring), DayOfWeek.Friday));
			weekdayButtons.Add(new WeekdayButton(view.FindViewById<Button>(Resource.Id.buttonSaturday_Recurring), DayOfWeek.Saturday));
			
			//set text views: title and time/date
			titleTextView.Text = session.Title;

			timeTextView.Text = (dateTime.ToShortTimeString() + "\n" + dateTime.ToShortDateString());

			//get menu (toggle edit view) button
			//FloatingActionMenu editMenuButton = view.FindViewById<FloatingActionMenu>(Resource.Id.menuButton_EditSession);
			ImageButton editMenuButton = view.FindViewById<ImageButton>(Resource.Id.imageButtonEditSession);
			
			//keep track of toggle state
			bool toggled = false;

			//set button animation
			//editMenuButton.IconToggleAnimatorSet = CreateCustomAnimationMenuButton(view);
			
			//set toggle action
			if (!editMenuButton.HasOnClickListeners) {
				editMenuButton.Click += delegate {
					if (!toggled) {
						//show edit menu
						editLayout.Visibility = ViewStates.Visible;

						//change item background color
						view.SetBackgroundResource(Resource.Color.base_app_complementary_color);

						//toggle
						toggled = true;

						//update edit index
						editSessionIndex = position;

						//change button icon
						editMenuButton.SetImageResource(Resource.Drawable.ic_check_white_24dp);
					}
					else {
						//hide edit menu
						editLayout.Visibility = ViewStates.Gone;

						//change background back
						view.SetBackgroundResource(Resource.Color.base_app_color);

						//toggle
						toggled = false;

						//always set title when closing edit view
						string title = (editTextTitle.Text == "") ? editTextTitle.Hint : editTextTitle.Text;

						//fire title set event
						titleSetListener.OnTitleSet(editSessionIndex, title);

						//update edit index
						editSessionIndex = -1;

						//update recurrence list
						setRecurrenceListener.OnSetRecurrence(session, weekdayButtons);

						//change button icon
						editMenuButton.SetImageResource(Resource.Drawable.ic_edit_white_24dp);
					}

					//fire edit click event
					openEditViewListener.OnClickEditButton(editSessionIndex);
					
				};
			}

			//set weekday button clicks
			foreach (var b in weekdayButtons) {
				b.Button.Click += delegate {
					Log.Debug("weekday", b.Button.Text);
					b.Toggled = !b.Toggled;
					Log.Debug("weekday toggle", b.Toggled.ToString());
				};
			}

			if (!view.HasOnClickListeners) {
				view.LongClick += delegate {
					//show 'delete session' dialog
					showDeleteSessionDialogListener.OnShowDeleteSessionDialog(session);
				};
			}

			recurringSwitch.Click += delegate {
				recurringLayout.Visibility = recurringSwitch.Checked ? ViewStates.Visible : ViewStates.Gone;
				session.Recurring = recurringSwitch.Checked;
			};

			recurringSwitch.Checked = session.Recurring;
			recurringLayout.Visibility = (session.Recurring) ? ViewStates.Visible : ViewStates.Gone;

			var sessionLayout = view.FindViewById<LinearLayout>(Resource.Id.SessionsListItemLayout);
			if (!sessionLayout.HasOnClickListeners) {
				sessionLayout.Click += delegate {
					sessionClickListener.OnSessionClick(session);
				};
			}


			editTextTime.Click += delegate {
				//open dialog
				timePickerListener.OnShowTimePickerDialog(position);
			};
			editTextDate.Click += delegate {
				//open dialog
				datePickerListener.OnShowDatePickerDialog(position);
			};
			editTextTitle.EditorAction += delegate {
				//set session title
				titleSetListener.OnTitleSet(position, editTextTitle.Text);

				//lose focus on edittext
				editLayout.RequestFocus(FocusSearchDirection.Backward);
			};
			editTextTitle.AddTextChangedListener(this);

			
			//don't open any dialogs if index is <0; that means nothing is being edited
			if (editSessionIndex == -1)
				toggled = false;
			else //if we make an adapter with a non-neg editSessionIndex, open the edit dialog on that session
				if (editSessionIndex == position) {
					//list adapter has been reset
					//make edit layout visible
					editLayout.Visibility = ViewStates.Visible;

					//make background green
					view.SetBackgroundResource(Resource.Color.base_app_complementary_color);

					//set icon to check mark
					editMenuButton.SetImageResource(Resource.Drawable.ic_check_white_24dp);

					//toggle, duh
					toggled = true;

					//focus on title edittext; show keyboard
					//editTextTitle.ShowSoftInputOnFocus = true;
					editTextTitle.RequestFocusFromTouch();
				}
				else {
					//close menu
					editLayout.Visibility = ViewStates.Gone;

					//make background red
					view.SetBackgroundResource(Resource.Color.base_app_color);

					//set icon to pencil
					editMenuButton.SetImageResource(Resource.Drawable.ic_edit_white_24dp);
				}

			//set edittextviews to reflect session info
			if (TitleText == null || TitleText == "")
				editTextTitle.Hint = session.Title;
			else
				editTextTitle.Text = TitleText;

			editTextTime.Text = dateTime.ToShortTimeString();
			editTextDate.Text = dateTime.ToShortDateString();

			return view;
		}

		//event handlers for them keyboard events
		public void OnTextChanged(ICharSequence text, int start, int before, int count) {
			TitleText = text.ToString();
		}
		public void AfterTextChanged(Android.Text.IEditable s) {

		}
		public void BeforeTextChanged(ICharSequence s, int start, int count, int after) {

		}

		

		/*
		private AnimatorSet CreateCustomAnimationMenuButton(View rootView) {
			AnimatorSet set = new AnimatorSet();
			FloatingActionMenu menu = rootView.FindViewById<FloatingActionMenu>(Resource.Id.menuButton_EditSession);

			ObjectAnimator scaleOutX = ObjectAnimator.OfFloat(menu.MenuIconView, "scaleX", 1.0f, 0.2f);
			ObjectAnimator scaleOutY = ObjectAnimator.OfFloat(menu.MenuIconView, "scaleY", 1.0f, 0.2f);

			ObjectAnimator scaleInX = ObjectAnimator.OfFloat(menu.MenuIconView, "scaleX", 0.2f, 1.0f);
			ObjectAnimator scaleInY = ObjectAnimator.OfFloat(menu.MenuIconView, "scaleY", 0.2f, 1.0f);

			scaleOutX.SetDuration(50);
			scaleOutY.SetDuration(50);

			scaleInX.SetDuration(150);
			scaleInY.SetDuration(150);

			scaleInX.AnimationStart += (object sender, EventArgs e) => {
				menu.MenuIconView.SetImageResource(menu.IsOpened ? Resource.Drawable.ic_edit_white_24dp : Resource.Drawable.ic_check_white_24dp);
			};

			set.Play(scaleOutX).With(scaleOutY);
			set.Play(scaleInX).With(scaleInY).After(scaleOutX);
			set.SetInterpolator(new OvershootInterpolator(2));

			menu.IconToggleAnimatorSet = set;

			return set;
		}
		 */
	}
}