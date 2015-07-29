//
//  NPNFLocalNotifications.mm
//  
//
//  Created by Ernest Ma on 3/24/14.
//
//

#import "NPNFLocalNotifications.h"
#import <objc/runtime.h>

#ifdef __cplusplus
extern "C" {
#endif
    void UnitySendMessage(const char* obj, const char* method, const char* msg);
#ifdef __cplusplus
}
#endif

@implementation NPNFLocalNotifications

@synthesize gameObjectName = _gameObjectName;

+ (instancetype)sharedInstance
{
    static dispatch_once_t pred;
    static NPNFLocalNotifications *sharedInstance = nil;
    dispatch_once(&pred, ^{
        sharedInstance = [[NPNFLocalNotifications alloc] init];
    });
    return sharedInstance;
}

- (id)init
{
    self = [super init];
    if (self) {
    }
    return self;
}

- (void)dealloc
{
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

- (void) registerForNotifications
{
    if ([UIApplication instancesRespondToSelector:@selector(registerUserNotificationSettings:)])
    {
        UIUserNotificationSettings *settings =
            [UIUserNotificationSettings settingsForTypes:UIUserNotificationTypeAlert |
                                                         UIUserNotificationTypeBadge |
                                                         UIUserNotificationTypeSound categories:nil];
        [[UIApplication sharedApplication] registerUserNotificationSettings:settings];
    }
}

- (void) npnfScheduleLocalNotifications:(NSString*)notificationId message:(NSString*)message time:(int)time
{
    [self npnfRemoveLocalNotifications:notificationId];

    if (time > 0)
    {
        NSDate *nextDate = [[NSDate date] dateByAddingTimeInterval:(NSTimeInterval)time];
        
        UILocalNotification *localNotif = [[UILocalNotification alloc] init];
        if (localNotif == nil)
            return;
        
        localNotif.fireDate = nextDate;
        localNotif.timeZone = [NSTimeZone defaultTimeZone];
        
        localNotif.alertBody = message;
        
        localNotif.soundName = UILocalNotificationDefaultSoundName;
        
        NSDictionary *infoDict = [NSDictionary dictionaryWithObject:notificationId forKey:@"npnfNotificationId"];
        localNotif.userInfo = infoDict;

        [[UIApplication sharedApplication] scheduleLocalNotification:localNotif];
        [localNotif release];
    }
}

- (void) npnfRemoveLocalNotifications:(NSString*)notificationId
{
    NSArray *notificationAry = [[UIApplication sharedApplication] scheduledLocalNotifications];
    for (int i = 0; i<[notificationAry count]; i++)
    {
        UILocalNotification* oneEvent = [notificationAry objectAtIndex:i];
        NSDictionary *userInfoCurrent = oneEvent.userInfo;
        NSString* scheduledNotificationId = [userInfoCurrent objectForKey:@"npnfNotificationId"];
        if ([scheduledNotificationId isEqualToString:notificationId])
        {
            //Cancelling local notification
            [[UIApplication sharedApplication] cancelLocalNotification:oneEvent];
            break;
        }
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

// Convert NSArray into JSON Object
- (NSString *)jsonFromArray:(NSArray *)array key:(NSString *)key
{
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:@{key: array}
                                                       options:NSJSONWritingPrettyPrinted
                                                         error:nil];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    return [jsonString autorelease];
}

@end

extern "C"
{
    void iOSBridgeInitLocalNotifications(const char* gObjName)
    {
        [NPNFLocalNotifications sharedInstance].gameObjectName = [NSString stringWithUTF8String:gObjName];
    }
    
    void npnfScheduleNotifications(const char* notificationId, const char* message, int time)
    {
        [[NPNFLocalNotifications sharedInstance]
         npnfScheduleLocalNotifications:[NSString stringWithUTF8String:notificationId]
         message:[NSString stringWithUTF8String:message]
         time:time];
    }

    void registerForNotifications()
    {
        [[NPNFLocalNotifications sharedInstance] registerForNotifications];
    }
}
