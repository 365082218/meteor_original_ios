//
//  InAppPurchaseManager.m
//  MoGoSample_iPad
//
//  Created by ShangYu on 3/7/13.
//
//

#import "InAppPurchaseManager.h"

NSString *AppState;
NSString *ObjName;
NSString *FunName;
NSString *ValName;
@implementation InAppPurchaseManager

- (void)requestProductData:(NSString*)state
{
    AppState = state;
    NSSet *productIdentifiers;

    productIdentifiers = [NSSet setWithObject:AppState ];
    
    productsRequest = [[SKProductsRequest alloc] initWithProductIdentifiers:productIdentifiers];
    productsRequest.delegate = self;
    [productsRequest start];
    
    // we will release the request object in the delegate callback
}

#pragma mark -
#pragma mark SKProductsRequestDelegate methods
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
{
    NSLog(@"进入productsRequest");
    NSArray *products = response.products;
    InAppProduct = [products count] == 1 ? [products firstObject] : nil;
    if (InAppProduct)
    {
        NSLog(@"Product title: %@" , InAppProduct.localizedTitle);
        NSLog(@"Product description: %@" , InAppProduct.localizedDescription);
        NSLog(@"Product price: %@" , InAppProduct.price);
        NSLog(@"Product id: %@" , InAppProduct.productIdentifier);
    }
    
    for (NSString *invalidProductId in response.invalidProductIdentifiers)
    {
        NSLog(@"Invalid product id: %@" , invalidProductId);
    }
    
    // finally release the reqest we alloc/init’ed in requestProUpgradeProductData
    //[productsRequest release];
    
    [[NSNotificationCenter defaultCenter] postNotificationName:kInAppPurchaseManagerProductsFetchedNotification object:self userInfo:nil];
}

#pragma -
#pragma Public methods
//
// call this method once on startup
//
- (void)loadStore:(NSString*)state
{
    NSLog(@"进入loadStore");
    // restarts any purchases if they were interrupted last time the app was open
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    // get the product description (defined in early sections)
    [self requestProductData:state];
}
//
// call this before making a purchase
//
- (BOOL)canMakePurchases
{
    return [SKPaymentQueue canMakePayments];
}

- (void)RestoreLevel
{
    NSLog(@"进入RestoreLevel");
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    [[SKPaymentQueue defaultQueue] restoreCompletedTransactions];
}

- (void)paymentQueueRestoreCompletedTransactionsFinished:(SKPaymentQueue *)queue
{
    int i = 0;
    for (SKPaymentTransaction *transaction in queue.transactions)
    {
        [self restoreTransaction:transaction];
        i += 1;
    }
    if( i > 0 ){
        UIAlertView *alertView = [[UIAlertView alloc] initWithTitle:@"Success" message:@"Your add-ons have been restored." delegate:self cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [alertView show];
    }
    else{
        UIAlertView *alertView = [[UIAlertView alloc] initWithTitle:@"Success" message:@"You have no add-ons." delegate:self cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [alertView show];
    }
    
    UnitySendMessage("AppStore","GoOn","");
    return;
}
- (void)paymentQueue:(SKPaymentQueue *)queue
restoreCompletedTransactionsFailedWithError:(NSError *)error
{
    UIAlertView *alertView = [[UIAlertView alloc] initWithTitle:@"Success" message:@"Your add-ons restore fail." delegate:self cancelButtonTitle:@"OK" otherButtonTitles:nil];
    [alertView show];
    UnitySendMessage("AppStore","GoOn","");
}
//
// kick off the upgrade transaction
//

- (void)purchaseLevel:(NSString*)productId:(NSString*)Obj:(NSString*)Fun:(NSString*)Val
{
    AppState = [[NSString alloc]initWithString:productId];
    ObjName = [[NSString alloc]initWithString:Obj];
    FunName = [[NSString alloc]initWithString:Fun];
    ValName = [[NSString alloc]initWithString:Val];
    
    SKPayment *payment;
    payment = [SKPayment paymentWithProductIdentifier:AppState];
    [[SKPaymentQueue defaultQueue] addPayment:payment];
}

#pragma -
#pragma Purchase helpers
//
// saves a record of the transaction by storing the receipt to disk
// 节省了存储收据到磁盘的交易记录
- (void)recordTransaction:(SKPaymentTransaction *)transaction
{
    if ([transaction.payment.productIdentifier isEqualToString:AppState])
    {
        // save the transaction receipt to disk
        [[NSUserDefaults standardUserDefaults] setValue:transaction.transactionReceipt forKey:AppState ];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }
    
}
//
// enable pro features
//
- (void)provideContent:(NSString *)productId
{
    if ([productId isEqualToString:AppState])
    {
        // enable the pro features
        [[NSUserDefaults standardUserDefaults] setBool:YES forKey:AppState ];
        [[NSUserDefaults standardUserDefaults] synchronize];
        UnitySendMessage([ObjName UTF8String],[FunName UTF8String],[ValName UTF8String]);
    }
}
//
// removes the transaction from the queue and posts a notification with the transaction result
// 从队列移除
- (void)finishTransaction:(SKPaymentTransaction *)transaction wasSuccessful:(BOOL)wasSuccessful
{
    // remove the transaction from the payment queue.
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
    
    NSDictionary *userInfo = [NSDictionary dictionaryWithObjectsAndKeys:transaction, @"transaction" , nil];
    if (wasSuccessful)
    {
        // send out a notification that we’ve finished the transaction
        // 发出通知，我们已经完成了交易
        [[NSNotificationCenter defaultCenter] postNotificationName:kInAppPurchaseManagerTransactionSucceededNotification object:self userInfo:userInfo];
    }
    else
    {
        // send out a notification for the failed transaction
        // 发出的交易失败的通知
        [[NSNotificationCenter defaultCenter] postNotificationName:kInAppPurchaseManagerTransactionFailedNotification object:self userInfo:userInfo];
    }
}
//
// called when the transaction was successful
// 交易成功
- (void)completeTransaction:(SKPaymentTransaction *)transaction
{
    UnitySendMessage("AppStore","GoOnSuccess","");
    [self recordTransaction:transaction];
    [self provideContent:transaction.payment.productIdentifier];
    [self finishTransaction:transaction wasSuccessful:YES];
}
//
// called when a transaction has been restored and and successfully completed
// 得到恢复和成功地完成交易时调用
- (void)restoreTransaction:(SKPaymentTransaction *)transaction
{
    UnitySendMessage("AppStore","GoOnRestore","");
    [self recordTransaction:transaction.originalTransaction];
    [self provideContent:transaction.originalTransaction.payment.productIdentifier];
    [self finishTransaction:transaction wasSuccessful:YES];
}
//
// called when a transaction has failed
// 当交易失败
- (void)failedTransaction:(SKPaymentTransaction *)transaction
{
    UnitySendMessage("AppStore","GoOnFail","");
    if (transaction.error.code != SKErrorPaymentCancelled)
    {
        // error!
        [self finishTransaction:transaction wasSuccessful:NO];
    }
    else
    {
        // this is fine, the user just cancelled, so don’t notify
        [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
    }
}
#pragma mark -
#pragma mark SKPaymentTransactionObserver methods
//
// called when the transaction status is updated
//
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions
{
    for (SKPaymentTransaction *transaction in transactions)
    {
        switch (transaction.transactionState)
        {
            case SKPaymentTransactionStatePurchased:
                [self completeTransaction:transaction];
                break;
            case SKPaymentTransactionStateFailed:
                [self failedTransaction:transaction];
                break;
            case SKPaymentTransactionStateRestored:
                [self restoreTransaction:transaction];
                break;
            default:
                break;
        }
    }
}

@end
