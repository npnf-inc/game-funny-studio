package com.npnf.sdk.npnfsocial;


import com.unity3d.player.UnityPlayer;

import android.app.Activity;
import android.provider.ContactsContract;
import android.util.Log;

public class ContactListController {
	   private static final String TAG = "ContactListController";
	   private static ContactListController _instance = null;
	   private static String gameObjectName;

	   private Activity activity;

	   private ContactsObserver contactsObserver;
	      
	   private String phoneString;
	   private String emailString;

	   
	   public static ContactListController getInstance(Activity currentActivity, String objectName) {
			//Log.d("AndroidAPI", "ContactListController - getInstance");
			if (_instance == null)
			{
				//Log.d("AndroidAPI", "ContactListController - Init");

				_instance =  new ContactListController( currentActivity);
				gameObjectName = objectName;
			}
			
			if (!gameObjectName.equals(objectName))
			{
				gameObjectName = objectName;
			}
			
			return _instance;
	   }
	   
	   /***
	    *
	    * @param currentActivity
	    */
	   public ContactListController(Activity currentActivity)
	   {
	       activity = currentActivity;
	   }
	   
	   
	  public static void sendMessageToUnity(String methodName, String message)
	  { 
		  //Log.d(TAG,	"sendMessageToUnity");
		  UnityPlayer.UnitySendMessage(gameObjectName, methodName, message);   
	  }

	  
	  public void synchronizeContacts(boolean enableContactListChangeCallback)		 
	  {
		  GetContactInfo(true, true, false);
		  
		  if (enableContactListChangeCallback)
		  {
				contactsObserver = new ContactsObserver(activity.getApplicationContext());
				contactsObserver.addContactsChangeListener(new ContactsObserver.ContactsChangeListener() {
							public void onContactsChange() {
								GetContactInfo(true, true, false);
							}
						});
				activity.getApplication().getContentResolver().registerContentObserver(ContactsContract.Contacts.CONTENT_URI, true, contactsObserver);
		  }
	 }
	  
	  
	 public void loadFriendsFromAddressBook()
	 {
		 GetContactInfo(true, true, true);
	 }
	  
	  
	 public void getAddressBookPhoneList()
	 {
		 if (phoneString != null)
		 {
			 ContactListController.sendMessageToUnity("OnSynchronizeContactsComplete", "P<split>" + phoneString);
		 }
		 else
		 {
			 GetContactInfo(true, false, false);
		 }
			 
	 }
	 
	 public void getAddressBookEmailList()
	 {
		 if (emailString != null)
		 {
			 ContactListController.sendMessageToUnity("OnSynchronizeContactsComplete", "E<split>" + emailString);
		 }
		 else
		 {
			GetContactInfo(true, false, false);
		 }
	 }
	  
	 private void GetContactInfo(final boolean phone, final boolean email, final boolean allLoad)
	 {		 
		 new ContactReader(new ContactReader.ReadContactCallBack() {
			
			public void before() {
				
			}

			public void after(String[] result) {
				if (result != null) {
					
					phoneString = result[0];
					emailString = result[1];
					
					if (allLoad)
					{
						ContactListController.sendMessageToUnity("OnLoadFriendsFromAddressBookComplete", "L<split>" + result[0] + "<split>" + result[1]);

					}
					else
					{
						if (phone)
						{
							ContactListController.sendMessageToUnity("OnSynchronizeContactsComplete", "P<split>" + result[0]);
						}
						if (email)
						{
							ContactListController.sendMessageToUnity("OnSynchronizeContactsComplete", "E<split>" + result[1]);
						}
					}
				}
			}
		  }).execute(activity);
	 }
	    
}
