package com.npnf.sdk.npnfsocial;

import java.util.ArrayList;
import java.util.List;

import android.content.Context;
import android.database.ContentObserver;
import android.os.Handler;
import android.provider.ContactsContract;
import android.util.Log;

public class ContactsObserver extends ContentObserver{
	private final static String TAG = ContactsObserver.class.getSimpleName();

	private Context ctx;
	private List<ContactsChangeListener> listeners = new ArrayList<ContactsChangeListener>();

	public ContactsObserver(Context ctx) {
		super(new Handler());
		this.ctx = ctx.getApplicationContext();
		ctx.getContentResolver().registerContentObserver(
				ContactsContract.Contacts.CONTENT_URI, // uri
				false, // notifyForDescendents
				this); // observer
	}

	@Override
	public void onChange(boolean selfChange) {
		Log.i(TAG, "Contacts change");
		for (ContactsChangeListener l : listeners) {
			l.onContactsChange();
		}
		Log.d(TAG, "register contacts change listener");
	}

	public void addContactsChangeListener(ContactsChangeListener l) {
		listeners.add(l);
	}

	public interface ContactsChangeListener {
		void onContactsChange();
	}
}
