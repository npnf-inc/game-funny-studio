//
//  CountlyClientImpl.mm
//  
//
//  Created by Sean Lao on 5/5/14.
//
//

#import <Foundation/NSDictionary.h>
#import <Foundation/NSJSONSerialization.h>
#import "CountlyClientImpl.h"
#import "Countly.h"

@implementation CountlyClientImpl

@end

// Converts C style string to NSString
NSString* Countly_CreateNSString (const char* string)
{
	if (string)
		return [NSString stringWithUTF8String: string];
	else
		return [NSString stringWithUTF8String: ""];
}

extern "C"
{
    void _Init(const char* serverUrl, const char* appKey, const char* clientId, const char* accessToken)
    {
        [[Countly sharedInstance] initServerUrl:Countly_CreateNSString(serverUrl)
                                         appKey:Countly_CreateNSString(appKey)
                                       clientId:Countly_CreateNSString(clientId)
                                    accessToken:Countly_CreateNSString(accessToken)];
    }

    void _SetSkieUserId(const char* userId)
    {
        NSLog(@"Countly: skieuserId=%s", userId);
        [[Countly sharedInstance] setUserId:Countly_CreateNSString(userId)];
    }

    void _Start()
    {
        [[Countly sharedInstance] start];
    }

    void _UpdateAccessToken(const char* value)
    {
        [[Countly sharedInstance] updateAccessToken:Countly_CreateNSString(value)];
    }

    void _Stop()
    {
        [[Countly sharedInstance] suspend];
    }

    void _RecordEvent1(const char* key, int count)
    {
        [[Countly sharedInstance] recordEvent:Countly_CreateNSString(key) count:count];
    }

    void _RecordEvent2(const char* key, int count, double sum)
    {
        [[Countly sharedInstance] recordEvent:Countly_CreateNSString(key) count:count sum:sum];
    }

    void _RecordEvent3(const char* key, const char* segmentation, int count)
    {
        NSError *error;
        NSData *data = [Countly_CreateNSString(segmentation) dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary *dictSeg = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&error];
        [[Countly sharedInstance] recordEvent:Countly_CreateNSString(key) segmentation:dictSeg count:count];
    }

    void _RecordEvent4(const char* key, const char* segmentation, int count, double sum)
    {
        NSError *error;
        NSData *data = [Countly_CreateNSString(segmentation) dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary *dictSeg = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&error];
        [[Countly sharedInstance] recordEvent:Countly_CreateNSString(key) segmentation:dictSeg count:count sum:sum];
    }
}
