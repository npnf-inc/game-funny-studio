//
//  NPNFUnityIOSBridge.m
//  
//
//  Created by Ernest Ma on 3/24/14.
//
//

#import "NPNFUnityIOSBridge.h"
#import <AdSupport/ASIdentifierManager.h>
#import <objc/runtime.h>
#import <StoreKit/SKProduct.h>
#import <StoreKit/SKPayment.h>
#import <StoreKit/SKPaymentQueue.h>
#import <StoreKit/SKPaymentTransaction.h>

#ifdef __cplusplus
extern "C" {
#endif
    void UnitySendMessage(const char* obj, const char* method, const char* msg);
#ifdef __cplusplus
}
#endif

@implementation NPNFUnityIOSBridge
{
    NSMutableDictionary *productInfo;
}

@synthesize gameObjectName = _gameObjectName;


+ (instancetype)sharedInstance
{
    static dispatch_once_t pred;
    static NPNFUnityIOSBridge *sharedInstance = nil;
    dispatch_once(&pred, ^{
        sharedInstance = [[NPNFUnityIOSBridge alloc] init];
    });
    return sharedInstance;
}

- (id)init
{
    self = [super init];
    if (self) {
        [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    }
    return self;
}

- (void)dealloc
{
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
    [productInfo release];
    
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


#pragma mark - < SKPaymentTransactionObserver Delegate >

- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions
{
}

- (void)paymentQueue:(SKPaymentQueue *)queue removedTransactions:(NSArray *)transactions
{
    if (!productInfo)
    {
        productInfo = [[NSMutableDictionary alloc] init];
    }

    // Store transaction information
    for (id transaction in transactions)
    {
        SKPaymentTransaction *trans = (SKPaymentTransaction *) transaction;
        //NSLog(@"SLD: Removed Transaction: %@, transState=%d, expected=%d", trans.payment.productIdentifier, trans.transactionState, SKPaymentTransactionStatePurchased);

        if (trans.transactionState == SKPaymentTransactionStatePurchased)
        {
            NSString *quantity = [NSString stringWithFormat:@"%d", trans.payment.quantity];
            NSArray *objects = [NSArray arrayWithObjects:quantity, @"0", @"0", @"", nil];
            NSArray *keys = [NSArray arrayWithObjects:@"quantity", @"price", @"currencyType", @"description", nil];
            NSMutableDictionary *info = [[NSMutableDictionary alloc] initWithObjects:objects forKeys:keys];
            [productInfo setValue:info forKey:trans.payment.productIdentifier];
            [info release];
        }
    }

    // Get Product Info (Price, Currency Type)
    SKProductsRequest *productsRequest = [[SKProductsRequest alloc]
                                          initWithProductIdentifiers:[NSSet setWithArray:[productInfo allKeys]]];
    productsRequest.delegate = self;
    [productsRequest start];
}

- (void)paymentQueue:(SKPaymentQueue *)queue updatedDownloads:(NSArray *)downloads
{
}

#pragma mark - < SKProductsRequestDelegate Delegate >

- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
{
    if (productInfo && productInfo.count > 0 && response.products.count > 0)
    {
        NSMutableDictionary *matches = [[NSMutableDictionary alloc] init];

        for (id product in response.products)
        {
            SKProduct *prod = (SKProduct *) product;
            NSMutableDictionary *prodInfo = [productInfo objectForKey:prod.productIdentifier];
            if (prodInfo)
            {
                // set price
                [prodInfo setValue:[prod.price stringValue] forKey:@"price"];

                // set currency type
                NSNumberFormatter *formatter = [[NSNumberFormatter alloc] init];
                [formatter setLocale:prod.priceLocale];
                [prodInfo setValue:[formatter currencyCode] forKey:@"currencyType"];
                [formatter release];

                // set description
                if (prod.localizedDescription)
                {
                    [prodInfo setValue:prod.localizedDescription forKey:@"description"];
                }

                [matches setValue:prodInfo forKey:prod.productIdentifier];
                [productInfo removeObjectForKey:prod.productIdentifier];
            }
        }

        if (matches.count > 0)
        {
            NSError *error;
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:matches
                                                               options:kNilOptions
                                                                 error:&error];
            NSString *jsonStr = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];

            [self sendMessageToUnity:jsonStr method:@"OnInAppPurchase"];
            [jsonStr release];
        }

        [matches release];
    }
}



@end

extern "C"
{
    void bridgeInit(const char* gObjName)
    {
        [NPNFUnityIOSBridge sharedInstance].gameObjectName = [NSString stringWithUTF8String:gObjName];
    }
    
    const char* bridgeGetIDFA()
    {
        NSString *idfa = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
        return [[NPNFUnityIOSBridge sharedInstance] UTF8StringFormatter:idfa];

    }
    
    const char* bridgeGetIDFV()
    {
        NSString *idfv = [[[UIDevice currentDevice] identifierForVendor] UUIDString];
        return [[NPNFUnityIOSBridge sharedInstance] UTF8StringFormatter:idfv];

    }
    
    const char* bridgeGetUserAgent()
    {
        UIWebView *webView = [[UIWebView alloc]initWithFrame:CGRectZero];
        NSString *uaString = [webView stringByEvaluatingJavaScriptFromString:@"navigator.userAgent"];
        [webView release];
        
        return [[NPNFUnityIOSBridge sharedInstance] UTF8StringFormatter:uaString];
    }
 
    const bool bridgeGetLimitingAdTrackingEnabled()
    {
        return [ASIdentifierManager sharedManager].advertisingTrackingEnabled;
    }
}

