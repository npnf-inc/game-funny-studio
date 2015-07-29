package com.npnf.sdk.npnflocalnotifications;

import java.util.HashMap;
import java.util.Map;

import android.app.Activity;
import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.os.Bundle;
import android.os.SystemClock;
import android.util.Log;

public class NPNFNotificationCenter {

	private static final String TAG = "NPNFNotificationCenter";
	private static NPNFNotificationCenter _instance = null;
	private static String gameObjectName;

	private Activity activity;
	private Map<String, Notification> mNotifications;

	private class Notification {
		private String notificationId;
		private String message;

		public Notification(String notificationId, String message) {
			this.notificationId = notificationId;
			this.message = message;
		}
	}

	public static NPNFNotificationCenter getInstance(Activity currentActivity, String objectName) {
		// Log.d(TAG, "UnityBridgeActivity - getInstance");
		if (_instance == null) {
			// Log.d(TAG, "UnityBridgeActivity - Init");

			_instance = new NPNFNotificationCenter(currentActivity);
			gameObjectName = objectName;
		}

		if (!gameObjectName.equals(objectName)) {
			gameObjectName = objectName;
		}

		return _instance;
	}

	/***
	 * 
	 * @param currentActivity
	 */
	public NPNFNotificationCenter(Activity currentActivity) {
		Log.i(TAG, "Constructor called with currentActivity = " + currentActivity);
		activity = currentActivity;
		mNotifications = new HashMap<String, Notification>();
	}

	public Object getMetaData(Context context, String key) {
		// Log.d(TAG, "getMetaData - context:" + context+ " - key:" + key);
		try {
			ApplicationInfo applicationInfo = context.getPackageManager().getApplicationInfo(context.getPackageName(),
					PackageManager.GET_META_DATA);
			Bundle bundle = applicationInfo.metaData;
			return bundle.get(key);
		} catch (NameNotFoundException e) {
			Log.e(TAG, "An application with the given package name can not be found on the system");
		}
		return null;

	}

	public void npnfScheduleNotifications(String notificationId, String message, String time) {
		Log.d(TAG, "UnityBridgeActivity - npnfScheduleNotifications");

		int seconds = Integer.parseInt(time);

		Notification newNotification = new Notification(notificationId, message);
		Notification oldNotification = mNotifications.put(notificationId, newNotification);
		if (oldNotification != null) {
			CancelAlarm(activity, oldNotification);
		}
		SetAlarm(activity, newNotification, seconds);
	}

	private void SetAlarm(final Context context, final Notification notification, final int seconds) {
		if (seconds > 0) {
			Log.d(TAG, "AlarmReceiver - SetAlarm");
	
			Intent intent = new Intent(context, AlarmReceiver.class);
			intent.putExtra(AlarmReceiver.TITLE_KEY, notification.message);
			intent.putExtra(AlarmReceiver.SUBJECT_KEY, notification.message);
			intent.putExtra(AlarmReceiver.BODY_KEY, "");
			intent.putExtra(AlarmReceiver.PACKAGENAME_KEY, activity.getPackageName());
			intent.putExtra(AlarmReceiver.TAG_KEY, notification.notificationId);
	
			// Schedule the alarm
			AlarmManager alarmMgr = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
			PendingIntent alarmIntent = PendingIntent.getBroadcast(context, 0, intent, 0);
	
			alarmMgr.set(AlarmManager.ELAPSED_REALTIME_WAKEUP, SystemClock.elapsedRealtime() + seconds * 1000, alarmIntent);
		}
	}

	private void CancelAlarm(final Context context, final Notification notification) {
		Log.d(TAG, "AlarmReceiver - CancelAlarm");

		AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);

		Intent intent = new Intent(context, AlarmReceiver.class);
		intent.putExtra(AlarmReceiver.TITLE_KEY, notification.message);
		intent.putExtra(AlarmReceiver.SUBJECT_KEY, notification.message);
		intent.putExtra(AlarmReceiver.BODY_KEY, "");
		intent.putExtra(AlarmReceiver.PACKAGENAME_KEY, activity.getPackageName());
		intent.putExtra(AlarmReceiver.TAG_KEY, notification.notificationId);
		PendingIntent sender = PendingIntent.getBroadcast(context, 0, intent, 0);

		alarmManager.cancel(sender);
	}
}