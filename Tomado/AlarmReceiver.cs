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


namespace Tomado {
	[BroadcastReceiver]
	public class AlarmReceiver : BroadcastReceiver {
		public override void OnReceive(Context context, Intent intent) {
			var ID = intent.GetIntExtra("ID", 0);
			var title = intent.GetStringExtra("title");
			var content = intent.GetStringExtra("content");

			var appIntent = new Intent(context, typeof(SwipeActivity));
			appIntent.PutExtra("ID", ID);

			PendingIntent contentIntent = PendingIntent.GetActivity(context, 0, appIntent, PendingIntentFlags.UpdateCurrent);//ID:0 for timer notifications
			var manager = NotificationManager.FromContext(context);

			var builder = new Notification.Builder(context)
				.SetAutoCancel(true)
				.SetSmallIcon(Resource.Drawable.Icon)
				.SetContentTitle(title)
				.SetContentText(content)
				.SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
				.SetContentIntent(contentIntent);

			var notification = builder.Build();

			manager.Notify(ID, notification);
		}
	}
}
