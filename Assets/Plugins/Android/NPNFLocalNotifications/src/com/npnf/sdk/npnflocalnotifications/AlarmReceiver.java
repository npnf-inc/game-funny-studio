package com.npnf.sdk.npnflocalnotifications;

import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.graphics.*;
import android.graphics.Bitmap.Config;
import android.graphics.drawable.*;
import android.content.Intent;
import android.support.v4.app.NotificationCompat;
import android.util.Log;

public class AlarmReceiver extends BroadcastReceiver {
    public static final String TITLE_KEY = "TITLE_KEY";
	public static final String BODY_KEY = "BODY_KEY";
	public static final String SUBJECT_KEY = "SUBJECT_KEY";
	public static final String PACKAGENAME_KEY = "PACKAGENAME_KEY";
	public static final String TAG_KEY = "TAG_KEY";

	@Override
	public void onReceive(Context context, Intent intent) {
		Log.d("AlarmReceiver", "AlarmReceiver - onReceive");

		String title = intent.getStringExtra(TITLE_KEY);
		String subject = intent.getStringExtra(SUBJECT_KEY);
		String body = intent.getStringExtra(BODY_KEY);
		String packageName = intent.getStringExtra(PACKAGENAME_KEY);
		String tag = intent.getStringExtra(TAG_KEY);

		Intent launchIntent = context.getPackageManager().getLaunchIntentForPackage(packageName);
		PendingIntent pending = PendingIntent.getActivity(context, 0, launchIntent, Intent.FLAG_ACTIVITY_NEW_TASK);

		Bitmap unityIconBitmapResized = null;
		Bitmap unityIconBitmap = null;

		NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(context)
				.setContentTitle(title)
				.setContentText(body)
				.setTicker(subject)
				.setContentIntent(pending)
				.setAutoCancel(true)
				.setWhen(System.currentTimeMillis());

		int iconId = context.getResources().getIdentifier("app_icon", "drawable", packageName);
		if (iconId == 0) {
			iconId = context.getResources().getIdentifier("ic_launcher", "drawable", packageName);
		}

		if (iconId != 0) {
			Drawable unityIconDrawable = context.getResources().getDrawable(iconId);
			unityIconBitmap = drawableToBitmap(unityIconDrawable);
        	unityIconBitmapResized = resizeBitmap(unityIconBitmap, 48, 48);
			mBuilder.setSmallIcon(android.R.drawable.sym_action_chat);
			mBuilder.setLargeIcon(unityIconBitmapResized);
		}
		
		NotificationManager NM = (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
		NM.notify(tag, 0, mBuilder.build());
	}

	private Bitmap resizeBitmap(Bitmap bm, int newHeight, int newWidth) {
	    int width = bm.getWidth();
	    int height = bm.getHeight();
	    float scaleWidth = ((float) newWidth) / width;
	    float scaleHeight = ((float) newHeight) / height;
	    Matrix matrix = new Matrix();
	    matrix.postScale(scaleWidth, scaleHeight);
	    Bitmap resizedBitmap = Bitmap.createBitmap(bm, 0, 0, width, height, matrix, false);
	    return resizedBitmap;
	}

	private static Bitmap drawableToBitmap(Drawable drawable) {
	    if (drawable instanceof BitmapDrawable) {
	        return ((BitmapDrawable) drawable).getBitmap();
	    }

	    int width = drawable.getIntrinsicWidth();
	    width = width > 0 ? width : 1;
	    int height = drawable.getIntrinsicHeight();
	    height = height > 0 ? height : 1;

	    Bitmap bitmap = Bitmap.createBitmap(width, height, Config.ARGB_8888);
	    Canvas canvas = new Canvas(bitmap);
	    drawable.setBounds(0, 0, canvas.getWidth(), canvas.getHeight());
	    drawable.draw(canvas);

	    return bitmap;
	}
}
