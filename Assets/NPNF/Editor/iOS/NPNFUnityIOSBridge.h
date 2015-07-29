//
//  NPNFUnityIOSBridge.h
//  
//
//  Created by Ernest Ma on 3/24/14.
//
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <StoreKit/SKPaymentQueue.h>
#import <StoreKit/SKProductsRequest.h>

@interface NPNFUnityIOSBridge : NSObject<SKPaymentTransactionObserver, SKProductsRequestDelegate>

+ (instancetype)sharedInstance;

@property (nonatomic, strong) NSString* gameObjectName;

@end

