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

using Clans.Fab;

using Java.Lang;

namespace Tomado {
	/// <summary>
	/// Adapter to populate Sessions list.
	/// </summary>
	public class SessionAdapter : BaseAdapter<Session> {
		List<Session> sessions;
		Activity context;
		private DeleteSessionListener deleteSessionListener;
		private SessionClickListener sessionClickListener;
		private ShowDeleteSessionDialogListener showDeleteSessionDialogListener;

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

		/// <summary>
		/// Listener to handle showing delete session dialog
		/// </summary>
		public interface ShowDeleteSessionDialogListener {
			void OnShowDeleteSessionDialog(Session session);
		}

		public SessionAdapter(Activity context, List<Session> sessions, SessionClickListener sessionClickListener, ShowDeleteSessionDialogListener showDeleteSessionDialogListener) {
			this.context = context;
			this.sessions = sessions;
			this.sessionClickListener = sessionClickListener;
			this.showDeleteSessionDialogListener = showDeleteSessionDialogListener;
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
				menu.MenuIconView.SetImageResource(menu.IsOpened ? Resource.Drawable.ic_add_white_24dp : Resource.Drawable.ic_arrow_drop_down_circle_white_24dp);
			};

			set.Play(scaleOutX).With(scaleOutY);
			set.Play(scaleInX).With(scaleInY).After(scaleOutX);
			set.SetInterpolator(new OvershootInterpolator(2));

			menu.IconToggleAnimatorSet = set;
			
			return set;
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
			view.FindViewById<TextView>(Resource.Id.evTime).Text = (dateTime.ToShortTimeString() + "\n" + dateTime.ToShortDateString());

			FloatingActionMenu editMenuButton = view.FindViewById<FloatingActionMenu>(Resource.Id.menuButton_EditSession);
			
			editMenuButton.IconToggleAnimatorSet = CreateCustomAnimationMenuButton(view);
			bool toggled = false;

			if (!editMenuButton.IsOpened)
				editMenuButton.Open(true);

			editMenuButton.MenuToggle += delegate {
				if (!toggled) {
					Toast.MakeText(view.Context, "toggle", ToastLength.Short).Show();
					toggled = true;
				}
				else {
					Toast.MakeText(view.Context, "untoggle", ToastLength.Short).Show();
					toggled = false;
				}
			};

			view.LongClickable = true;

			if (!view.HasOnClickListeners) {
				view.LongClick += delegate {
					//show 'delete session' dialog
					showDeleteSessionDialogListener.OnShowDeleteSessionDialog(session);
				};
			}

			var sessionLayout = view.FindViewById<LinearLayout>(Resource.Id.SessionsListItemLayout);
			if (!sessionLayout.HasOnClickListeners) {
				sessionLayout.Click += delegate {
					sessionClickListener.OnSessionClick(session);
				};
			}

			return view;
		}
	}
}