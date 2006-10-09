//
//  TangerinePrefPanePref.m
//  TangerinePrefPane
//
//  Created by James Willcox on 10/7/06.
//  Copyright (c) 2006 __MyCompanyName__. All rights reserved.
//

#import <Foundation/NSString.h>
#import <Foundation/NSCharacterSet.h>
#import "TangerinePrefPanePref.h"


@implementation TangerinePrefPanePref

- (void)setFromConfig
{
/*
    NSDictionary *loginitems = [NSDictionary dictionaryWithContentsOfFile:
        [@"~/Library/Preferences/loginwindow.plist" stringByExpandingTildeInPath]];
        */
        
        
    [shareNameText setStringValue:[config getValue:@"name" section:@"Tangerine"]];
    [dirText setStringValue:[[config getValue:@"directories" section:@"FilePlugin"] stringByAbbreviatingWithTildeInPath]];
    
    [automaticRadio setState:0];
    [itunesRadio setState:0];
    [dirRadio setState:0];
    
    NSString *password_file = [config getValue:@"password_file" section:@"Tangerine"];
    if (password_file && [password_file length] > 0) {
        FILE *f = fopen([password_file cString], "r");
        if (f) {
            char buf[BUFSIZ];
            fgets(buf, BUFSIZ, f);
            [passwordText setStringValue:[NSString stringWithUTF8String:buf]];
        }
        
        fclose (f);
    }
    
    NSString *plugins = [config getValue:@"plugins" section:@"Tangerine"];
    if ([plugins compare:@"spotlight"] == NSOrderedSame) {
        [automaticRadio setState:1];
    } else if ([plugins compare:@"itunes"] == NSOrderedSame) {
        [itunesRadio setState:1];
    } else {
        [dirRadio setState:1];
    }
}

- (void) mainViewDidLoad
{
    dirty = 0;
    config = [[TangerineIniParser alloc] init];
    [config setFile:[@"~/.tangerine" stringByExpandingTildeInPath]];
    [config parse];
    
    [self setFromConfig];
    [self refreshEnabled];
}

- (void)didUnselect
{
    if (dirty) {
        [self saveChanges];
    }
    
}

- (int) readPid {
    NSString *pidstr = [NSString stringWithContentsOfFile:[@"~/.tangerine.pid" stringByExpandingTildeInPath]];
    
    int pid = [pidstr intValue];
    if (pid <= 0) {
        return -1;
    }
    
    return pid;
}

- (void) startDaemon
{
    NSBundle *bundle = [NSBundle bundleForClass:[self class]];
    NSString * path = [[bundle bundlePath] stringByAppendingPathComponent:@"/Contents/Daemon/start-daemon.sh"];

    [NSTask launchedTaskWithLaunchPath:path arguments:[[[NSArray alloc] init] autorelease]];
}

- (void) stopDaemon:(BOOL)wait
{
    int pid = [self readPid];
    if (pid <= 0) {
        return;
    }
    
    if (kill(pid, SIGTERM) != 0) {
        return;
    }
    
    if (wait) {
        while (kill(pid, 0) == 0) {
            usleep(5000);
        }
    }
}

- (void) restartDaemon
{
    [self stopDaemon: 1];
    [self startDaemon];
}

- (void)saveChanges
{
    [config setValue:@"name" section:@"Tangerine" value:[shareNameText stringValue]];
    [config setValue:@"log_file" section:@"Tangerine" value:[@"~/.tangerine.log" stringByExpandingTildeInPath]];
    /*[config setValue:@"max_users" section:@"Tangerine" value:[userLimitText stringValue]];*/
    
    if ([automaticRadio state]) {
        [config setValue:@"plugins" section:@"Tangerine" value:@"spotlight"];
    } else if ([itunesRadio state]) {
        [config setValue:@"plugins" section:@"Tangerine" value:@"itunes"];
    } else if ([dirRadio state]) {
        [config setValue:@"plugins" section:@"Tangerine" value:@"file"];
        [config setValue:@"directories" section:@"FilePlugin" value:[[dirText stringValue] stringByExpandingTildeInPath]];
    }
    
    NSString *password_file = [@"~/.tangerine.passwd" stringByExpandingTildeInPath];
    NSString *password = [passwordText stringValue];
    if (password && [password length] > 0) {
        FILE *f = fopen([password_file cString], "w+");
        fprintf(f, "%s", [password cString]);
        fclose(f);
        
        [config setValue:@"password_file" section:@"Tangerine" value:password_file];
    } else {
        [config deleteKey:@"password_file" section:@"Tangerine"];
        unlink(password_file);
    }
    
    [config save];
    
    if ([enabledCheckBox state]) {
        [self restartDaemon];
    } else {
        [self stopDaemon: 0];
    }
}

- (IBAction)changed:(id)sender
{
    dirty = 1;
    [self refreshEnabled];
}

- (IBAction)chooseButtonPressed:(id)sender
{
    printf("Choose button pressed!");\
    NSOpenPanel *panel = [NSOpenPanel openPanel];
    [panel setCanChooseFiles:0];
    [panel setCanChooseDirectories:1];
    
    int result = [panel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
    if (result == NSOKButton) {
        [dirText setStringValue:[[panel filename] stringByAbbreviatingWithTildeInPath]];
    }
}

- (void)refreshEnabled
{
    int enabled = [enabledCheckBox state];
    
    [shareNameText setEnabled:enabled];
    [dirText setEnabled:[dirRadio state]];
    [chooseButton setEnabled:(enabled && [dirRadio state])];
    [radioGroup setEnabled:enabled];
    [passwordText setEnabled:enabled];
    [userLimitText setEnabled:enabled];
    [userLimitStepper setEnabled:enabled];
}

@end
