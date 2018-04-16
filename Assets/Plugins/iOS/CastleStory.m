//
//  CastleStory.m
//  CastleStory
//
//  Created by sure on 2016/10/29.
//  Copyright © 2016年 sure. All rights reserved.
//

#import "CastleStory.h"

@implementation CastleStory
@end

#if defined (__cplusplus)
extern "C"
{
#endif
    extern void UnitySendMessage(const char * , const char *, const char *);
    void appstart()
    {
        UnitySendMessage("Main Camera", "GameStart", "开始");
    }
#if defined (__cplusplus)
}
#endif
