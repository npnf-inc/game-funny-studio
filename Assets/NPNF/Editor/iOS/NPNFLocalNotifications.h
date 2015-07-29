//
//  NPNFLocalNotifications.h
//  
//
//  Created by Ernest Ma on 3/24/14.
//
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface NPNFLocalNotifications : NSObject

+ (instancetype)sharedInstance;

@property (nonatomic, strong) NSString* gameObjectName;

@end

