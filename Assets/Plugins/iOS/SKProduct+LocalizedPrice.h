//
//  SKProduct+LocalizedPrice.h
//  MoGoSample_iPad
//
//  Created by ShangYu on 3/7/13.
//
//

#import <Foundation/Foundation.h>
//#import <StoreKit/StoreKit.h>

@interface SKProduct (LocalizedPrice)

@property (nonatomic, readonly) NSString *localizedPrice;

@end