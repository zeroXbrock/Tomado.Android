using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Animation;
using Android.Views.Animations;

namespace Tomado {
	public class ExpandCollapseAnimation : Animation {
		private View animatedView;
		private int endHeight, type;
		public const int COLLAPSE = 1;
		public const int EXPAND = 0;

		private LinearLayout.LayoutParams layoutParams;

		public ExpandCollapseAnimation(ViewGroup view, int type) {
			animatedView = view;
			endHeight = view.MeasuredHeight;
			layoutParams = new LinearLayout.LayoutParams(view.LayoutParameters);
			this.type = type;

			if (type == EXPAND)
				layoutParams.BottomMargin = -endHeight;
			else
				layoutParams.BottomMargin = 0;

			view.Visibility = ViewStates.Visible;
		}

		protected override void ApplyTransformation(float interpolatedTime, Transformation t) {
			base.ApplyTransformation(interpolatedTime, t);

			if (interpolatedTime < 1.0f) {
				if (type == EXPAND)
					layoutParams.BottomMargin = -endHeight + (int)(endHeight * interpolatedTime);
				else
					layoutParams.BottomMargin = -(int)(endHeight * interpolatedTime);

				animatedView.RequestLayout();
			}
			else {
				if (type == EXPAND) {
					layoutParams.BottomMargin = 0;
					animatedView.RequestLayout();
				}
				else {
					layoutParams.BottomMargin = -endHeight;
					animatedView.Visibility = ViewStates.Gone;
					animatedView.RequestLayout();
				}
			}
		}
	}
}