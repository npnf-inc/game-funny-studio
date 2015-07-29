package com.npnf.sdk.npnfsocial;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.app.Activity;
import android.content.ContentResolver;
import android.database.Cursor;
import android.os.AsyncTask;
import android.provider.ContactsContract;
import android.util.Log;

public class ContactReader extends AsyncTask<Activity, Integer, String[]> {
	private final static String TAG = ContactReader.class.getSimpleName();
	private String returnReadContactsPhones;
	private String returnReadContactsEmails;
	private ReadContactCallBack readContactCallBack;
	private final int MIN_PHONE_NUMBER_LENGTH = 6;
	
	public ContactReader(ReadContactCallBack readContactCallBack){
		this.readContactCallBack = readContactCallBack;
		readContactCallBack.before();
	}
	
	/***
	 * Parameters params[0] is Activity using in the method readContactsPhones and
	 * readContactsEmails. When use resource contacts phones and contacts email
	 * we must get ContentResolver from Activity. So parameter in function
	 * doInBackground must is Activity
	 * 
	 * Method doInBackground have a parameters is type array
	 * (http://developer.android.com/reference/android/os/AsyncTask.html#doInBackground(Params...))
	 */
	@Override
	protected String[] doInBackground(Activity... params) {
		if (null != params && params.length >= 0) {
			returnReadContactsPhones = readContactsPhones(params[0]);
			returnReadContactsEmails = readContactsEmails(params[0]);
			Log.i(TAG, "returnReadContactsPhones : " + returnReadContactsPhones);
			Log.i(TAG, "returnReadContactsEmails : " + returnReadContactsEmails);
	 		return new String[]{returnReadContactsPhones, returnReadContactsEmails};
		}else{
			return null;
		}

	}
	
	@Override
	protected void onPostExecute(String[] result) {
 		super.onPostExecute(result);
 		readContactCallBack.after(result);
	}
	/***
	 * 
	 * @author longtq
	 *
	 */
	public interface ReadContactCallBack {

		public void before();

		public void after(String []result);

	}
	
	/***
	 * json format  {"email":[{"f":"abc","e":"a@a.a"}]}
	 * @return json data
	 */
	public String readContactsEmails(Activity activity) {
		JSONObject jsonObjectEmail = new JSONObject();
		JSONArray  jsonArrayEmail  = new JSONArray();
		ContentResolver cr = activity.getContentResolver();
		Cursor cur = cr.query(ContactsContract.Contacts.CONTENT_URI, null,
				null, null, null);
		if (cur.getCount() > 0) {
			while (cur.moveToNext()) {
				String id = cur.getString(cur
						.getColumnIndex(ContactsContract.Contacts._ID));
				String name = cur
						.getString(cur
								.getColumnIndex(ContactsContract.Contacts.DISPLAY_NAME));
				Cursor emailCur = cr.query(
						ContactsContract.CommonDataKinds.Email.CONTENT_URI,
						null, ContactsContract.CommonDataKinds.Email.CONTACT_ID
								+ " = ?", new String[] { id }, null);

				while (emailCur.moveToNext()) {
					String email = emailCur
							.getString(emailCur
									.getColumnIndex(ContactsContract.CommonDataKinds.Email.ADDRESS));
					if (email != null) {
						try{
							JSONObject  jsonElementEmail  = new JSONObject();
							jsonElementEmail.put("f", name);
							jsonElementEmail.put("e", email);
							jsonArrayEmail.put(jsonElementEmail);
						}catch(Exception exception){
							Log.i(TAG, "contacts emails JSONException");
						}

					}
				}
				emailCur.close();
			}
		}
		try {
			jsonObjectEmail.put("emails", jsonArrayEmail);
		} catch (JSONException e) {
			Log.i(TAG, "emails JSONException");
		}
		return jsonObjectEmail.toString();
	}
	
	/***
	 * json format  {"phones":[{"m":"111111","f":"aaaaaa"}]}
	 * @return json data
	 */
	public String readContactsPhones(Activity activity) {
		JSONObject jsonObjectPhones = new JSONObject();
		JSONArray  jsonArrayPhones  = new JSONArray();
		ContentResolver cr = activity.getContentResolver();
		Cursor cur = cr.query(ContactsContract.Contacts.CONTENT_URI, null, null, null, null);
		if (cur.getCount() > 0) {
			while (cur.moveToNext()) {
				String id = cur.getString(cur.getColumnIndex(ContactsContract.Contacts._ID));
				String name = cur.getString(cur.getColumnIndex(ContactsContract.Contacts.DISPLAY_NAME));
				if (Integer.parseInt(cur.getString(cur.getColumnIndex(ContactsContract.Contacts.HAS_PHONE_NUMBER))) > 0) {
					// get the phone number
					Cursor pCur = cr.query(
							ContactsContract.CommonDataKinds.Phone.CONTENT_URI,
							null,
							ContactsContract.CommonDataKinds.Phone.CONTACT_ID
									+ " = ?", new String[] { id }, null);
					while (pCur.moveToNext()) {
 						String phone = pCur
								.getString(pCur
										.getColumnIndex(ContactsContract.CommonDataKinds.Phone.NUMBER));
						phone = phone.replaceAll("[^\\dA-Za-z]", "");
						phone = phone.trim();
						if (phone.length() >= MIN_PHONE_NUMBER_LENGTH) {
							try{
								JSONObject  jsonElementPhones  = new JSONObject();
								jsonElementPhones.put("f", name);
								jsonElementPhones.put("m", phone);
								jsonArrayPhones.put(jsonElementPhones);
							}catch(Exception exception){
								Log.i(TAG, "contacts emails JSONException");
							}
						}
					}
					pCur.close();
				}
			}
		}
		try {
			jsonObjectPhones.put("phones", jsonArrayPhones);
		} catch (JSONException e) {
			Log.i(TAG, "emails JSONException");
		}
		return jsonObjectPhones.toString();
	}
}
