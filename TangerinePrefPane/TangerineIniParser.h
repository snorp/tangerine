//
//  TangerineIniParser.h
//  TangerinePrefPane
//
//  Created by James Willcox on 10/8/06.
//  Copyright 2006 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import <Foundation/NSString.h>


@interface TangerineIniParser : NSObject
{
@private
    NSMapTable *table;
    NSString *file;
}

- (void) setFile:(NSString*) path;
- (void) parse;
- (void) save;
- (NSString*) getFile;
- (NSString*) getValue:(NSString*)key section:(NSString*)section;
- (void) setValue:(NSString*)key section:(NSString*)section value:(NSString*)value;
- (void) deleteKey:(NSString*)key section:(NSString*)section;

@end
