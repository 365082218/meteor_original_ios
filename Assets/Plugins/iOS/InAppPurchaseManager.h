//
//  InAppPurchaseManager.h
//  MoGoSample_iPad
//
//  Created by ShangYu on 3/7/13.
//
//

#import <StoreKit/StoreKit.h>
#define kInAppPurchaseManagerProductsFetchedNotification @"kInAppPurchaseManagerProductsFetchedNotification"
#define kInAppPurchaseManagerTransactionFailedNotification @"kInAppPurchaseManagerTransactionFailedNotification"
#define kInAppPurchaseManagerTransactionSucceededNotification @"kInAppPurchaseManagerTransactionSucceededNotification"
@interface InAppPurchaseManager : NSObject <SKProductsRequestDelegate>
{
    SKProduct *InAppProduct;
    SKProductsRequest *productsRequest;
}

// public methods
- (void)loadStore:(NSString*)state;
- (BOOL)canMakePurchases;
- (void)purchaseLevel:(NSString*)productId:(NSString*)Obj:(NSString*)Fun:(NSString*)Val;
- (void)RestoreLevel;
@end