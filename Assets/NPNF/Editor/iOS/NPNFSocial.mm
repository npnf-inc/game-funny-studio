//
//  NPNFSocial.mm
//  
//
//  Created by Ernest Ma on 3/24/14.
//
//

#import "NPNFSocial.h"
#import <objc/runtime.h>
#import <AddressBook/AddressBook.h>

#ifdef __cplusplus
extern "C" {
#endif
    void UnitySendMessage(const char* obj, const char* method, const char* msg);
#ifdef __cplusplus
}
#endif

@implementation NPNFSocial
{
    ABAddressBookRef addressBook;
    NSMutableArray* phoneNumbers;
    NSMutableArray* emails;
}

@synthesize gameObjectName = _gameObjectName;

+ (instancetype)sharedInstance
{
    static dispatch_once_t pred;
    static NPNFSocial *sharedInstance = nil;
    dispatch_once(&pred, ^{
        sharedInstance = [[NPNFSocial alloc] init];
    });
    return sharedInstance;
}

- (void)dealloc
{
    if (addressBook)
    {
        CFRelease(addressBook);
    }
    
    if (emails)
    {
        CFRelease(emails);
    }
    
    if (phoneNumbers)
    {
        CFRelease(phoneNumbers);
    }
    
    [super dealloc];
}

- (const char*) UTF8StringFormatter:(NSString*) text
{
    if (!text)
        return NULL;
    else {
        const char *str = [text UTF8String];
        if (str == NULL) {
            return NULL;
        }
        char *result = (char *)malloc(strlen(str) + 1);
        strcpy(result, str);
        return result;
    }
}

- (void)sendMessageToUnity:(NSString *)message method:(NSString *)method
{
    if (!message)
    {
        message = @"";
    }
    
    UnitySendMessage([self.gameObjectName UTF8String], [method UTF8String], [message UTF8String]);
}

#pragma mark - < Social Graph >


- (void)setupAddressBook:(bool)addressBookChangeCallback
{
    addressBook = ABAddressBookCreateWithOptions(NULL, NULL);
    
    if(addressBook) {
        if (addressBookChangeCallback)
        {
            ABAddressBookRegisterExternalChangeCallback(addressBook, handleAddressBookChange, (__bridge void *)(self));
        }
    }
}

void handleAddressBookChange(ABAddressBookRef addressBook, CFDictionaryRef info, void *context) {
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0ul), ^{
        [[NPNFSocial sharedInstance] synchronizeContacts];
    });
}


- (void)synchronizeContacts
{
    if (ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusAuthorized) {
        
        [[NPNFSocial sharedInstance] GetContact];
        
        [self sendMessageToUnity:
         [NSString stringWithFormat:@"P<split>%@", [self jsonFromArray:phoneNumbers key:@"phones"]]
                          method:@"OnSynchronizeContactsComplete"];
        [self sendMessageToUnity:
         [NSString stringWithFormat:@"E<split>%@", [self jsonFromArray:emails key:@"emails"]]
                          method:@"OnSynchronizeContactsComplete"];
    }
    else
    {
        [self sendMessageToUnity:@"NotAuthorized" method:@"OnSynchronizeContactsComplete"];
    }
}


- (void)GetContact
{
    if (!addressBook)
    {
        addressBook = ABAddressBookCreateWithOptions(NULL, NULL);
    }
    
    phoneNumbers = [NSMutableArray array];
    emails = [NSMutableArray array];
    
    CFArrayRef allPeople = ABAddressBookCopyArrayOfAllPeople(addressBook);
    
    [(NSArray *)allPeople enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
        id aPerson = obj;
        
        NSString *firstName = (NSString *) ABRecordCopyValue((ABRecordRef)(aPerson), kABPersonFirstNameProperty);
        NSString *lastName = (NSString *) ABRecordCopyValue((ABRecordRef)(aPerson), kABPersonLastNameProperty);
        if (!firstName) {
            firstName = @"";
        }
        if (!lastName) {
            lastName = @"";
        }
        
        // Get phone numbers
        ABMultiValueRef phoneNumbersRef = ABRecordCopyValue((ABRecordRef)(aPerson), kABPersonPhoneProperty);
        for (int i = 0; i < ABMultiValueGetCount(phoneNumbersRef); i++)
        {
            NSString *mobile = (NSString*)ABMultiValueCopyValueAtIndex(phoneNumbersRef, i);
            
            NSCharacterSet *trim = [NSCharacterSet characterSetWithCharactersInString:@"#();$&-+"];
            NSString *mobileModified  = [[[[mobile componentsSeparatedByCharactersInSet: trim] componentsJoinedByString: @""]
                       stringByReplacingOccurrencesOfString:@"\"" withString:@""]
                      stringByReplacingOccurrencesOfString:@" " withString:@""];
            NSDictionary *dict = @{@"f" : [NSString stringWithFormat:@"%@ %@", firstName, lastName],
                                   @"m" : mobileModified};
            [phoneNumbers addObject:dict];
            CFRelease(mobile);
        }
        CFRelease(phoneNumbersRef);

        // Get emails
        ABMultiValueRef emailsRef = ABRecordCopyValue((ABRecordRef)(aPerson), kABPersonEmailProperty);
        for (int i = 0; i < ABMultiValueGetCount(emailsRef); i++)
        {
            NSString *email = (NSString*)ABMultiValueCopyValueAtIndex(emailsRef, i);
            NSDictionary *dict = @{@"f" : [NSString stringWithFormat:@"%@ %@", firstName, lastName],
                                   @"e" : [email stringByReplacingOccurrencesOfString:@" " withString:@""]};
            [emails addObject:dict];
            CFRelease(email);
        }
        CFRelease(firstName);
        CFRelease(lastName);
        CFRelease(emailsRef);
    }];
    
    CFRelease(allPeople);
}


- (void)LoadAllContacts
{
    if (ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusAuthorized) {
        
        [[NPNFSocial sharedInstance] GetContact];
        
        [self sendMessageToUnity:
         [NSString stringWithFormat:@"L<split>%@<split>%@",
          [self jsonFromArray:phoneNumbers key:@"phones"],
          [self jsonFromArray:emails key:@"emails"]]
                          method:@"OnLoadFriendsFromAddressBookComplete"];
        
    }
    else
    {
        [self sendMessageToUnity:@"NotAuthorized" method:@"OnSynchronizeContactsComplete"];
    }
}


// Convert NSArray into JSON Object
- (NSString *)jsonFromArray:(NSArray *)array key:(NSString *)key
{
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:@{key: array}
                                                       options:NSJSONWritingPrettyPrinted
                                                         error:nil];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    return [jsonString autorelease];
}

- (const char *)allPhoneNumbers
{
    return [[NSString stringWithFormat:@"P<split>%@",
             [self jsonFromArray:phoneNumbers key:@"phones"]] UTF8String];
}

- (const char *)allEmailAddresses
{
    return [[NSString stringWithFormat:@"E<split>%@",
             [self jsonFromArray:phoneNumbers key:@"phones"]] UTF8String];
}


@end

extern "C"
{
    void iOSBridgeInitSocial(const char* gObjName)
    {
        [NPNFSocial sharedInstance].gameObjectName = [NSString stringWithUTF8String:gObjName];
    }
    
    // -------------- Social Graph --------------
    
    bool getContactAccessPermissionStatus()
    {
        if (ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusAuthorized)
        {
            return true;
        }
        return false;
    }
    
    
    void askContactAccessPermission()
    {
        if (ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusDenied ||
            ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusRestricted){
            NSLog(@"Denied");
            [[NPNFSocial sharedInstance] sendMessageToUnity:@"False"
                                                             method:@"OnAskPermissionResponsed"];
        } else if (ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusAuthorized){
            NSLog(@"Authorized");
            [[NPNFSocial sharedInstance] sendMessageToUnity:@"True"
                                                             method:@"OnAskPermissionResponsed"];
        } else{ //ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusNotDetermined
            // Contact Modal Pop up
            ABAddressBookRequestAccessWithCompletion(ABAddressBookCreateWithOptions(NULL, nil), ^(bool granted, CFErrorRef error) {
                if (!granted)
                {
                    [[NPNFSocial sharedInstance] sendMessageToUnity:@"False"
                                                                     method:@"OnAskPermissionResponsed"];
                }
                else
                {
                    [[NPNFSocial sharedInstance] sendMessageToUnity:@"True"
                                                                     method:@"OnAskPermissionResponsed"];
                }
            });
        }
    }
    
    void synchronizeContacts(bool addressBookChangeCallback)
    {
        // Setup & Synchronize contacts
        [[NPNFSocial sharedInstance] setupAddressBook:addressBookChangeCallback];
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0ul), ^{
            [[NPNFSocial sharedInstance] synchronizeContacts];
        });
    }
    
    void loadFriendsFromAddressBook()
    {
        if (getContactAccessPermissionStatus())
        {
            dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0ul), ^{
                [[NPNFSocial sharedInstance] LoadAllContacts];
            });
        }
        else
        {
            if (ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusDenied ||
                ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusRestricted){
                NSLog(@"Denied");
            } else if (ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusAuthorized){
                NSLog(@"Authorized");
                dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0ul), ^{
                    [[NPNFSocial sharedInstance] LoadAllContacts];
                });
            } else{ //ABAddressBookGetAuthorizationStatus() == kABAuthorizationStatusNotDetermined
                // Contact Modal Pop up
                ABAddressBookRequestAccessWithCompletion(ABAddressBookCreateWithOptions(NULL, nil), ^(bool granted, CFErrorRef error) {
                    if (granted)
                    {
                        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0ul), ^{
                            [[NPNFSocial sharedInstance] LoadAllContacts];
                        });
                    }
                });
            }
        }
    }
    
    const char * getAllPhoneNumbers()
    {
        return [[NPNFSocial sharedInstance] allPhoneNumbers];
    }
    
    const char * getAllEmailAddresses()
    {
        return [[NPNFSocial sharedInstance] allEmailAddresses];
    }
    
    // -------------- Social Graph --------------
}