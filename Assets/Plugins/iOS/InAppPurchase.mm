#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import "InAppPurchaseManager.h"
extern "C" {
    bool CanPurchase()
    {
        return [SKPaymentQueue canMakePayments];
    }
    InAppPurchaseManager *sharedMgr = nil;
    
    void BuySomething( char State[], char Obj[], char Fun[], char Val[] )
    {
        NSString *nState = [NSString stringWithUTF8String:State];
        NSString *nObj = [NSString stringWithUTF8String:Obj];
        NSString *nFun = [NSString stringWithUTF8String:Fun];
        NSString *nVal = [NSString stringWithUTF8String:Val];
        if (sharedMgr == nil) {
            sharedMgr = [[InAppPurchaseManager alloc]init];
            [sharedMgr loadStore:nState];
        }
        NSLog(@"Invalid product id: %@" , nState);
        [sharedMgr purchaseLevel:nState:nObj:nFun:nVal];
        
    }
    void RestoreBuy()
    {
        if (sharedMgr == nil) {
            sharedMgr = [[InAppPurchaseManager alloc]init];
        }
        [sharedMgr RestoreLevel];
    }
}