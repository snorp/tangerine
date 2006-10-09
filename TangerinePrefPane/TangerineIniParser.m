//
//  TangerineIniParser.m
//  TangerinePrefPane
//
//  Created by James Willcox on 10/8/06.
//  Copyright 2006 __MyCompanyName__. All rights reserved.
//

#import "TangerineIniParser.h"

@implementation TangerineIniParser

- (void) doLog:(NSString*) msg
{
    FILE *f = fopen("/Users/snorp/tanglog", "a");
    fprintf(f, [msg cString]);
    fclose(f);
}

- (id) init {
    self = [super init];
    if (self != nil) {
        table = NSCreateMapTable(NSObjectMapKeyCallBacks, NSOwnedPointerMapValueCallBacks, 0);
    }
    return self;
}

- (void) dealloc {
    NSMapTable *section_table;

    NSMapEnumerator enumerator = NSEnumerateMapTable(table);
    while (NSNextMapEnumeratorPair(&enumerator, NULL, (void **)&section_table)) {
        NSFreeMapTable(section_table);
    }

    NSFreeMapTable(table);
    [file release];
    [super dealloc];
}


- (void) setFile:(NSString*) path
{
    [file release];
    file = [path retain];
}

- (NSString*) getFile
{
    return file;
}

- (NSString*) getKey:(NSString*)section key:(NSString*) key
{
    return [NSString stringWithFormat:@"%@:%@", section, key];
}

- (NSMapTable*) getSectionTable:(NSString*) section create:(BOOL) create
{
    NSMapTable *section_table = NSMapGet(table, section);
    if (!section_table && create) {
        section_table = NSCreateMapTable(NSObjectMapKeyCallBacks, NSObjectMapValueCallBacks, 0);
        NSMapInsert(table, section, section_table);
        printf("Creating section: %s\n", [section cString]);
    }
    
    return section_table;
}

- (void) parse
{
    char buf[BUFSIZ];
    NSCharacterSet *stripset = [NSCharacterSet whitespaceAndNewlineCharacterSet];
    
    FILE *input = fopen([file cString], "r");
    if (!input)
        return;
        
    NSString *section = nil;
    NSMapTable *section_table = nil;
    
    while (fgets (buf, BUFSIZ, input)) {
        if (buf[0] == '[') {
            buf[strlen(buf) - 2] = '\0'; /* get rid of "]\n"; */
            section = [NSString stringWithUTF8String:(buf+1)];
            [section retain];
            
            section_table = [self getSectionTable:section create: 1];
            continue;
        } else if (buf[0] == '#') {
            continue;
        }
        
        NSArray *split_line = [[NSString stringWithUTF8String:buf] componentsSeparatedByString:@"="];
            
        if ([split_line count] != 2) {
            continue;
        }
            
        NSString *key = [(NSString *)[split_line objectAtIndex:0] stringByTrimmingCharactersInSet:stripset];
        NSString *value = [(NSString *)[split_line objectAtIndex:1] stringByTrimmingCharactersInSet:stripset];
        
        NSMapInsert(section_table, [key retain], [value retain]);
    }
        
    fclose (input);
}

- (void) save
{
    NSMapTable *section_table;
    NSString *section_name;
    
    FILE *output = fopen([file cString], "w+");
    
    NSMapEnumerator enumerator = NSEnumerateMapTable(table);
    while (NSNextMapEnumeratorPair(&enumerator, (void **)&section_name, (void **)&section_table)) {
        NSMapEnumerator valueEnumerator = NSEnumerateMapTable(section_table);
        NSString *name;
        NSString *value;
        
        fprintf (output, "[%s]\n", [section_name cString]);
        
        while (NSNextMapEnumeratorPair(&valueEnumerator, (void**)&name, (void**)&value)) {
            fprintf (output, "%s = %s\n", [name cString], [value cString]);
        }
        
        fprintf (output, "\n");
    }

    fclose (output);
}


- (NSString*)getValue:(NSString*)key section:(NSString*)section
{
    NSMapTable *section_table = [self getSectionTable: section create: 0];
    if (!section_table) {
        return @"";
    }
        
    NSString *val = (NSString*)NSMapGet(section_table, key);
    if (!val) {
        return @"";
    } else {
        return val;
    }
}

- (void)setValue:(NSString*)key section:(NSString*)section value:(NSString*)value
{
    NSMapTable *section_table = [self getSectionTable: section create: 1];
    NSMapInsert(section_table, key, value);
}

- (void) deleteKey:(NSString*)key section:(NSString*)section
{
    NSMapTable *section_table = [self getSectionTable: section create: 0];
    if (!section_table) {
        return;
    }

    NSMapRemove(section_table, key);
}

@end
