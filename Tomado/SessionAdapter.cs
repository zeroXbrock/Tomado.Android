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
using Android.Views.Animations;

using Clans.Fab;

using Java.Lang;

namespace Tomado {
	/// <summary>
	/// Adapter to populate Sessions list.
	/// </summary>
	public class SessionAdapter : BaseAdapter<Session>, Android.Text.ITextWatcher, Animator.IAnimatorListener {
		List<Session> sessions;
		Activity context;
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
		const int editAnimationDuration = 200;

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
			void OnSetRecurrence(int sessionIndex, Session session, List<DayOfWeek> recurringDays);
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

		class SessionViewHolder : Java.Lang.Object {
			//get layout for edit view
			public LinearLayout EditLayout { get; set; }

			//get textviews
			public TextView TitleTextView { get; set; }
			public TextView	TimeTextView  { get; set; }
			public TextView DateTextView  { get; set; }

			//get edittextviews
			public EditText EditTextTitle { get; set; } 
			public EditText EditTextDate  { get; set; }
			public EditText EditTextTime  { get; set; }

			//get menu (toggle edit view) button
			public ImageButton EditMenuButton { get; set; }
			
			//get recurring view instance
			public RecurringView RecurringView { get; set; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent) {
			View view = convertView;
			SessionViewHolder viewHolder = null;

			if (view != null)
				viewHolder = view.Tag as SessionViewHolder;

			//reuse existing view if available
			if (viewHolder == null) {
				LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
				view = inflater.Inflate(Resource.Layout.SessionListItem, null);

				viewHolder = new SessionViewHolder();

				//get layout for edit view
				viewHolder.EditLayout = view.FindViewById<LinearLayout>(Resource.Id.EditSessionLayout);

				//get textconvertViews
				viewHolder.TitleTextView = view.FindViewById<TextView>(Resource.Id.evTitle);
				viewHolder.DateTextView = view.FindViewById<TextView>(Resource.Id.evDate);
				viewHolder.TimeTextView = view.FindViewById<TextView>(Resource.Id.evTime);

				//get edittextviews
				viewHolder.EditTextTitle = view.FindViewById<EditText>(Resource.Id.editText_Title_EditSession);
				viewHolder.EditTextDate = view.FindViewById<EditText>(Resource.Id.editText_Date_EditSession);
				viewHolder.EditTextTime = view.FindViewById<EditText>(Resource.Id.editText_Time_EditSession);

				//get menu (toggle edit view) button
				//FloatingActionMenu editMenuButton = view.FindViewById<FloatingActionMenu>(Resource.Id.menuButton_EditSession);
				viewHolder.EditMenuButton = view.FindViewById<ImageButton>(Resource.Id.imageButtonEditSession);

				//get recurring view instance
				viewHolder.RecurringView = view.FindViewById<RecurringView>(Resource.Id.RecurringView_EditSession);

				view.Tag = viewHolder;
			}

			#region view instance definitions

			//get layout for edit view
			ViewGroup editLayout = viewHolder.EditLayout;

			//get textviews
			var titleTextView = viewHolder.TitleTextView;
			var dateTextView = viewHolder.DateTextView;
			var timeTextView = viewHolder.TimeTextView;
			
			//get edittextviews
			var editTextTitle = viewHolder.EditTextTitle;
			var editTextDate = viewHolder.EditTextDate;
			var editTextTime = viewHolder.EditTextTime;

			//get menu (toggle edit view) button
			//FloatingActionMenu editMenuButton = view.FindViewById<FloatingActionMenu>(Resource.Id.menuButton_EditSession);
			ImageButton editMenuButton = viewHolder.EditMenuButton;

			//keep track of toggle state
			bool toggled = false;

			//get recurring view instance
			var recurringView = viewHolder.RecurringView;

			#endregion

			//get session for this list item
			Session session = sessions[position];
			DateTime dateTime = new DateTime(session.Year, session.MonthOfYear + 1, session.DayOfMonth, session.StartHour, session.StartMinute, 0);

			//all editLayouts gone by default
			editLayout.Visibility = ViewStates.Gone;

			//set text views: title and time/date
			titleTextView.Text = session.Title;

			timeTextView.Text = dateTime.ToShortTimeString();
			dateTextView.Text = ToDateClause(dateTime, session.RecurringDays);

			//set toggle states for weekdays
			if (session.Recurring) {
				recurringView.SetRecurringWeekdays(session.RecurringDays);
			}

			//set toggle action
			if (!editMenuButton.HasOnClickListeners) {
				editMenuButton.Click += delegate {
					if (!toggled) {
						//toggle
						toggled = true;

						//update edit index
						editSessionIndex = position;

						//change button icon
						editMenuButton.SetImageResource(Resource.Drawable.ic_check_white_24dp);
					}
					else {
						//toggle
						toggled = false;

						//always set title when closing edit view
						string title =  editTextTitle.Text;

						//fire title set event
						titleSetListener.OnTitleSet(editSessionIndex, title);

						//update recurrence list w/ current index
						setRecurrenceListener.OnSetRecurrence(editSessionIndex, session, recurringView.GetRecurringWeekdays());
						
						//reset edit index
						editSessionIndex = -1;

						//change button icon
						editMenuButton.SetImageResource(Resource.Drawable.ic_edit_white_24dp);
					}

					//fire edit click event
					openEditViewListener.OnClickEditButton(editSessionIndex);
				};
			}

			if (!view.HasOnClickListeners) {
				view.LongClick += delegate {
					//show 'delete session' dialog
					showDeleteSessionDialogListener.OnShowDeleteSessionDialog(session);
				};
			}

			var sessionLayout = view.FindViewById<LinearLayout>(Resource.Id.SessionsListItemLayout);
			if (!sessionLayout.HasOnClickListeners) {
				sessionLayout.Click += delegate {
					if (!toggled)
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
			if (editSessionIndex <= -1)
				toggled = false;
			else //if we make an adapter with a non-neg editSessionIndex, open the edit dialog on that session
				if (editSessionIndex == position) {					
					view.SetBackgroundResource(Resource.Color.base_app_complementary_color);
					editMenuButton.SetImageResource(Resource.Drawable.ic_check_white_24dp);

					view.HasTransientState = true;
					
					

					if (editLayout.Visibility == ViewStates.Gone) {
						editLayout.Alpha = 0f;
						editLayout.SetY(-220);
						editLayout.Visibility = ViewStates.Visible;
						EditViewAnimateOpen(editLayout);
					}
					
					//toggle, duh
					toggled = true;

					//focus on title edittext; show keyboard
					//editTextTitle.ShowSoftInputOnFocus = true;
					editTextTitle.RequestFocusFromTouch();
				}
				else {
					/*run animation:
					 *	hide edit menu
					 *	change background to base_app_color
					 *	change icon to ic_edit_white_24dp
					 */
					view.SetBackgroundResource(Resource.Color.base_app_color);
					editMenuButton.SetImageResource(Resource.Drawable.ic_edit_white_24dp);

					if (editLayout.Visibility == ViewStates.Visible)
						EditViewAnimateClose(editLayout);
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

		//methods to handle edit view animations
		void EditViewAnimateOpen(ViewGroup editLayout) {
			editLayout.Animate()
				.Alpha(1.0f)
				.TranslationY(editLayout.Height)
				.SetDuration(editAnimationDuration)
				.SetInterpolator(new DecelerateInterpolator());
		}

		void EditViewAnimateClose(ViewGroup editLayout) {
			editLayout.Animate()
				.TranslationY(0f)
				.Alpha(0f)
				.SetDuration(editAnimationDuration)
				.SetInterpolator(new AccelerateInterpolator());
		}

		//event handlers for edit view
		public void OnAnimationStart(Animator animator) { }
		public void OnAnimationRepeat(Animator animator) { }
		public void OnAnimationEnd(Animator animator) { }
		public void OnAnimationCancel(Animator animator) { }

		string ToDateClause(DateTime startDateTime, List<DayOfWeek> recurringDays) {
			//returns clause like <weekday(s)> at <time>
			
			string days = "";
			string clause = "";

			if (recurringDays != null) {
				//create days string w/ commas
				for (int i = 0; i < recurringDays.Count; i++) {
					days += recurringDays[i].ToString();
					if (i < recurringDays.Count - 1)
						days += ", ";
				}
			}
			clause = days + " after " + startDateTime.ToShortDateString();

			return clause;
		}
	}
}