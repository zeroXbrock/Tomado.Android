using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.Util;

namespace Tomado {
	public class ProgressCircleView : View, ValueAnimator.IAnimatorUpdateListener, ValueAnimator.IAnimatorListener {
		Context _context;
		ValueAnimator animation;
		float progressPercentage, intervalPercentage, additionalPercentage;
		int updates = 0;

		public bool IsAnimationRunning { get { return animation.IsRunning; } }
		public bool IsAnimationPaused { get { return animation.IsPaused; } }
		public bool IsAnimationStarted { get { return animation.IsStarted; } }

		const int UPDATES_START = 1;

		public int CircleSize { get; set; }

		float startStatic;
		float sweepStatic;

		float startDynamic;
		float sweepDynamic;

		public Paint ProgressPaint { get; set; }

		public ProgressCircleView(Context context)
			: base(context) {
				Init(context);
		}
		public ProgressCircleView(Context context, IAttributeSet attrs)
			: base(context, attrs) {
				Init(context);
		}
		public ProgressCircleView(Context context, IAttributeSet attrs, int defStyleAttr) 
			: base(context, attrs, defStyleAttr){
				Init(context);
		}

		void Init(Context context){
			this._context = context;

			ProgressPaint = new Paint();
			ProgressPaint.Color = Resources.GetColor(Resource.Color.base_app_complementary_color);

			ProgressPaint.AntiAlias = true;
			ProgressPaint.SetStyle(Paint.Style.Stroke);
			ProgressPaint.StrokeWidth = 12;

			animation = ValueAnimator.OfInt(0, 100);//"animate" this value from 0-100
			animation.SetDuration(1000);
			animation.AddUpdateListener(this);
			animation.AddListener(this);
			animation.SetInterpolator(new DecelerateInterpolator());
			animation.RepeatMode = ValueAnimatorRepeatMode.Restart;

			progressPercentage = 0;
			updates = UPDATES_START;

			CircleSize = 500; //default; should be set manually b4 running timer
		}

		protected override void OnDraw(Android.Graphics.Canvas canvas) {
			DrawArc(canvas);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
			//base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			int desiredWidth = CircleSize;
			int desiredHeight = CircleSize;

			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			var widthSize = MeasureSpec.GetSize(widthMeasureSpec);
			var heightSize = MeasureSpec.GetSize(heightMeasureSpec);

			int width, height;

			//Measure Width
			if (widthMode == MeasureSpecMode.Exactly) {
				//Must be this size
				width = widthSize;
			}
			else if (widthMode == MeasureSpecMode.AtMost) {
				//Can't be bigger than...
				width = Math.Min(desiredWidth, widthSize);
			}
			else {
				//Be whatever you want
				width = desiredWidth;
			}

			//Measure Height
			if (heightMode == MeasureSpecMode.Exactly) {
				//Must be this size
				height = heightSize;
			}
			else if (heightMode == MeasureSpecMode.AtMost) {
				//Can't be bigger than...
				height = Math.Min(desiredHeight, heightSize);
			}
			else {
				//Be whatever you want
				height = desiredHeight;
			}

			SetMeasuredDimension(width, height);
		}

		#region animation overrides
		public void OnAnimationUpdate(ValueAnimator animator) {
			UpdateArc(((float)animator.AnimatedValue));

			Log.Debug("progressPercentage_OnAnimationUpdate", progressPercentage.ToString());
			Log.Debug("animator.AnimatedValue_OnAnimationUpdate", animator.AnimatedValue.ToString());
			Log.Debug("interval_OnAnimationUpdate", intervalPercentage.ToString());
		}

		public void OnAnimationStart(Animator animator) {

		}

		public void OnAnimationRepeat(Animator animator) {
			updates++;
		}

		public void OnAnimationEnd(Animator animator) {
			ResetAnimationVars();
		}

		public void OnAnimationCancel(Animator animator) {

		}
		#endregion

		void DrawArc(Canvas canvas) {
			RectF oval = new RectF(10, 10, Width - 10, Height - 10);

			float interval = (intervalPercentage / 100f);
			float addition = ((additionalPercentage) / 100f);

			startStatic = -90f;
			sweepStatic = (intervalPercentage/100f) * 360f * (updates-1);

			startDynamic = sweepStatic + startStatic;
			sweepDynamic = addition * interval * 360f;

			//runs instantly
			if (updates > 1)
				canvas.DrawArc(oval, startStatic, sweepStatic, false, ProgressPaint);
			
			//runs incrementally by animation
			if (sweepDynamic < 360f*(intervalPercentage/100f) && updates >= 1)
				canvas.DrawArc(oval, startDynamic, sweepDynamic, false, ProgressPaint);

			Log.Debug("sweepStatic_DrawArc", sweepStatic.ToString());
			Log.Debug("startDynamic_DrawArc", startDynamic.ToString());
			Log.Debug("sweepDynamic_DrawArc", sweepDynamic.ToString());

			Log.Debug("updates_DrawArc", updates.ToString());
		}

		public void UpdateArc(float additionalPercentage) {
			this.additionalPercentage = additionalPercentage;

			Invalidate();

			Log.Debug("additionalPercentage_UpdateArc", additionalPercentage.ToString());
		}

		public void StartTimerAnimation(float durationInSeconds, float progressPercentage) {
			animation.RepeatCount = (int)durationInSeconds;

			this.progressPercentage = progressPercentage;
			this.intervalPercentage = 100f / durationInSeconds;
			additionalPercentage = 0;
			startDynamic = startStatic;

			animation.Start();

			Log.Debug("progressPercentage_StartTimerAnimation", progressPercentage.ToString());
		}

		public void CancelTimerAnimation() {
			animation.Cancel();
		}

		public void PauseTimerAnimation() {
			animation.Pause();
		}

		public void ResumeTimerAnimation() {
			animation.Resume();
		}

		public void ResetAnimationVars() {
			updates = UPDATES_START;
			this.progressPercentage = 0;
			additionalPercentage = 0;
		}
	}
}