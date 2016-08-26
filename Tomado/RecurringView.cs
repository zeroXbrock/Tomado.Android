using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Tomado {
	class RecurringView : View {
		Context _context;
		ButtonClickListener buttonClickListener;

		public RecurringView(Context context) 
			: base(context) {
			this._context = context;
		}
		public RecurringView(Context context, IAttributeSet attrs)
			: base(context, attrs) {
			this._context = context;
		}
		public RecurringView(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs, defStyle) {
			this._context = context;
		}

		public interface ButtonClickListener{
			/// <summary>
			/// Event fired when a weekday button is clicked.
			/// </summary>
			/// <param name="index">Index of button pressed: [0-6]</param>
			void OnButtonClick(int index, bool toggled);
		}

		const int NUM_BUTTONS = 7;
		int radius = 70;
		List<Pair> positions = new List<Pair>();
		string[] weekdaysAbbrev = { "S", "M", "T", "W", "T", "F", "S" };
		DayOfWeek[] weekdays = { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
		bool[] toggledButtons = new bool[NUM_BUTTONS];

		public void SetOnButtonClickListener(ButtonClickListener listener) {
			this.buttonClickListener = listener;
		}

		void InitPositions() {
			if (positions.Count == 0) {
				int spacing = Width / NUM_BUTTONS;
				int shift = spacing / 2;

				for (int i = 0; i < NUM_BUTTONS; i++) {
					int x = i * spacing + shift;
					int y = radius * 2;
					positions.Add(new Pair(x, y));
				}
			}
		}

		void DrawWeekdayButtons(Canvas canvas) {
			InitPositions();

			var paintCircleUntoggled = new Paint() { Color = Color.Transparent, AntiAlias = true };
			var paintTextUntoggled = new Paint() { Color = Color.White, TextSize = radius, TextAlign = Paint.Align.Center };

			var paintCircleToggled = new Paint() { Color = Color.White, AntiAlias = true };
			var paintTextToggled = new Paint() { Color = Color.Black, TextSize = radius, TextAlign = Paint.Align.Center };
			
			for (int i = 0; i < NUM_BUTTONS; i++) {
				int x = (int)positions[i].First;
				int y = (int)positions[i].Second;
				
				canvas.DrawCircle(x, y, radius, toggledButtons[i] ? paintCircleToggled : paintCircleUntoggled);
				canvas.DrawText(weekdaysAbbrev[i], x, y + (radius/3), toggledButtons[i] ? paintTextToggled : paintTextUntoggled);
			}
		}

		protected override void OnDraw(Canvas canvas) {
			DrawWeekdayButtons(canvas);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			int height = 4 * radius;
			int width = MeasuredWidth;

			SetMeasuredDimension(width, height);
		}

		public override bool OnTouchEvent(MotionEvent e) {
			int indexHit = IsInsideCircle(e.GetX(), e.GetY());
			if (indexHit > -1) {
				if (buttonClickListener != null)
					buttonClickListener.OnButtonClick(indexHit, !toggledButtons[indexHit]);
				
				toggledButtons[indexHit] = !toggledButtons[indexHit];

				Invalidate();
			}

			return false;
		}

		int IsInsideCircle(float x, float y) {
			for (int i = 0; i < positions.Count; i++) {
				int centerX = (int)positions[i].First;
				int centerY = (int)positions[i].Second;

				if (Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2) < Math.Pow(radius, 2))
					return i;
			}

			return -1;
		}

		public List<DayOfWeek> GetRecurringWeekdays() {
			List<DayOfWeek> recurringWeekdays = new List<DayOfWeek>();

			for (int i = 0; i < toggledButtons.Length; i++) {
				if (toggledButtons[i])
					recurringWeekdays.Add(weekdays[i]);
			}

			return recurringWeekdays;
		}

		public void SetRecurringWeekdays(List<DayOfWeek> recurringDays) {
			//toggle buttons
			foreach (var d in recurringDays){
				for (int i = 0; i < toggledButtons.Length; i++) {
					if (weekdays[i] == d)
						toggledButtons[i] = true;
				}
			}
		}

		public static DayOfWeek IndexToDay(int index) {
			DayOfWeek[] days = {DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};

			if (index < 7 && index >= 0)
				return days[index];
			else
				throw new Exception("Index must be between 0 and 6; inclusively");
		}
	}
}