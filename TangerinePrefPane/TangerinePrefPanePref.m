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

- (BOOL) getDaemonEnabled
{
    NSString *launchdItemPath = [@"~/Library/LaunchAgents/net.snorp.Tangerine.plist" stringByExpandingTildeInPath];
    return [[NSFileManager defaultManager] fileExistsAtPath:launchdItemPath];
}

/* FIXME: I am lame for using NSTask */
- (void) setDaemonEnabled:(BOOL) enabled
{
    NSString *launchdItemPath = [@"~/Library/LaunchAgents/net.snorp.Tangerine.plist" stringByExpandingTildeInPath];
    BOOL launchdItemPathExists = [[NSFileManager defaultManager] fileExistsAtPath:launchdItemPath];
    
    if (enabled && launchdItemPathExists) {
        /* just restart it (by stopping it, launchd will restart it for me) */
        [NSTask launchedTaskWithLaunchPath:@"/bin/launchctl" arguments:[NSArray arrayWithObjects:@"stop", @"Tangerine", nil]];
    } else if (enabled) {
        NSBundle *bundle = [NSBundle bundleForClass:[self class]];
    
        NSString *autostartPath = [[bundle bundlePath] stringByAppendingPathComponent:@"/Contents/Daemon/start-daemon.sh"];
    
        NSMutableString *templateContent = [NSMutableString stringWithContentsOfFile:[bundle pathForResource:@"net.snorp.Tangerine" ofType:@"plist"]];
        [templateContent replaceOccurrencesOfString:@"@PROGRAM@" withString:autostartPath options:0 range:NSMakeRange(0, [templateContent length])];
    
        [[NSFileManager defaultManager] createDirectoryAtPath:[@"~/Library/LaunchAgents" stringByExpandingTildeInPath] attributes:nil];
        [templateContent writeToFile:launchdItemPath atomically:1];
        [NSTask launchedTaskWithLaunchPath:@"/bin/launchctl" arguments:[NSArray arrayWithObjects:@"load", @"-w", launchdItemPath, nil]];
    } else if (!enabled && launchdItemPathExists) {
        NSTask *task = [NSTask launchedTaskWithLaunchPath:@"/bin/launchctl" arguments:[NSArray arrayWithObjects:@"unload", launchdItemPath, nil]];
        [task waitUntilExit];

        [[NSFileManager defaultManager] removeFileAtPath:launchdItemPath handler:nil];
    }
}

- (void)setFromConfig
{
    [enabledCheckBox setState:[self getDaemonEnabled]];
    
    NSString *share_name = [config getValue:@"name" section:@"Tangerine"];
    if (share_name == nil || [share_name length] == 0) {
        share_name = [NSString stringWithFormat:@"%@'s Music", NSFullUserName()];
    }
    
    [shareNameText setStringValue:share_name];
    
    NSString *directory = [[config getValue:@"directories" section:@"FilePlugin"] stringByAbbreviatingWithTildeInPath];
    if (directory == nil || [directory length] == 0) {
        directory = @"~/Music";
    }
    
    [dirText setStringValue:directory];
    
    NSString *max_users = [config getValue:@"max_users" section:@"Tangerine"];
    if (max_users != nil && [max_users length] > 1) {
        [userLimitText setStringValue:max_users];
    } else {
        [userLimitText setIntValue:1];
    }
    
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
    } else if ([plugins compare:@"itunes"] == NSOrderedSame || plugins == nil || [plugins length] == 0) {
        [itunesRadio setState:1];
    } else {
        [dirRadio setState:1];
    }
}

- (BOOL) checkMono
{
    if (![[NSFileManager defaultManager] fileExistsAtPath:@"/usr/bin/mono"]) {
        NSAlert *alert = [NSAlert alertWithMessageText:@"Mono is not installed" defaultButton:@"Close" alternateButton:nil
                                  otherButton:nil informativeTextWithFormat:@"You must have Mono installed in order to use Tangerine.\n\nhttp://www.mono-project.com"];

        [alert beginSheetModalForWindow:[[self mainView] window] modalDelegate:self didEndSelector:nil contextInfo:nil];
        return NO;
    }
    
    return YES;
}

- (void) didSelect
{
    if (![self checkMono]) {
        [enabledCheckBox setState:0];
        [enabledCheckBox setEnabled:NO];
        [self refreshEnabled];
        dirty = 0;
    } else {
        [enabledCheckBox setEnabled:YES];
    }
}

- (void) mainViewDidLoad
{
    config = [[TangerineIniParser alloc] init];
    [config setFile:[@"~/.tangerine" stringByExpandingTildeInPath]];
    [config parse];
    
    [self setFromConfig];
    [self refreshEnabled];
    dirty = NO;
}

- (void) showShareNameError
{
    NSAlert *alert = [NSAlert alertWithMessageText:@"You must specify a share name." defaultButton:@"Close" alternateButton:nil
                              otherButton:nil informativeTextWithFormat:@"A share name must be set in order to enable music sharing."];
    
    [alert beginSheetModalForWindow:[[self mainView] window] modalDelegate:self didEndSelector:nil contextInfo:nil];
}

- (void) showDirectoryError
{
    NSAlert *alert = [NSAlert alertWithMessageText:@"Specified folder is not valid" defaultButton:@"Close" alternateButton:nil
                                       otherButton:nil informativeTextWithFormat:@"A valid folder must be specified when using the 'in Folder' selection."];
    
    [alert beginSheetModalForWindow:[[self mainView] window] modalDelegate:self didEndSelector:nil contextInfo:nil];
}

- (NSPreferencePaneUnselectReply)shouldUnselect
{
    if (![enabledCheckBox state])
        return NSUnselectNow;
    
    if ([[shareNameText stringValue] length] == 0) {
        [self showShareNameError];
        return NSUnselectCancel;
    } else if ([dirRadio state] && ![[NSFileManager defaultManager] fileExistsAtPath:[[dirText stringValue] stringByExpandingTildeInPath]]) {
        [self showDirectoryError];
        return NSUnselectCancel;
    } else {
        return NSUnselectNow;
    }
}

- (void)didUnselect
{
    if (dirty) {
        [self saveChanges];
    }
    
}

- (void)saveChanges
{
    [config setValue:@"name" section:@"Tangerine" value:[shareNameText stringValue]];
    [config setValue:@"log_file" section:@"Tangerine" value:[@"~/.tangerine.log" stringByExpandingTildeInPath]];
    
    NSString *max_users = [userLimitText stringValue];
    if (![userLimitCheckBox state]) {
        max_users = @"0";
    }
    
    [config setValue:@"max_users" section:@"Tangerine" value:max_users];
    
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
    if ([passwordCheckBox state] && password && [password length] > 0) {
        FILE *f = fopen([password_file cString], "w+");
        fprintf(f, "%s", [password cString]);
        fclose(f);
        
        [config setValue:@"password_file" section:@"Tangerine" value:password_file];
    } else {
        [config deleteKey:@"password_file" section:@"Tangerine"];
        unlink(password_file);
    }
    
    [config save];
    
    [self setDaemonEnabled:[enabledCheckBox state]];
}

- (IBAction)changed:(id)sender
{
    dirty = YES;
    [self refreshEnabled];
}

- (void)openPanelDidEnd:(NSOpenPanel *)panel returnCode:(int)returnCode  contextInfo:(void  *)contextInfo
{
    if (returnCode == NSOKButton) {
        [dirText setStringValue:[[panel filename] stringByAbbreviatingWithTildeInPath]];
    }
}

- (IBAction)chooseButtonPressed:(id)sender
{
    NSOpenPanel *panel = [NSOpenPanel openPanel];
    [panel setCanChooseFiles:0];
    [panel setCanChooseDirectories:1];
    
    [panel beginSheetForDirectory:nil file:nil types:nil modalForWindow:[[self mainView] window] modalDelegate:self
                   didEndSelector:@selector(openPanelDidEnd:returnCode:contextInfo:) contextInfo:nil];
}

- (void)refreshEnabled
{
    int enabled = [enabledCheckBox state];
    
    [shareNameText setEnabled:enabled];
    [dirText setEnabled:(enabled && [dirRadio state])];
    [chooseButton setEnabled:(enabled && [dirRadio state])];
    [radioGroup setEnabled:enabled];
    [passwordCheckBox setEnabled:enabled];
    [userLimitCheckBox setEnabled:enabled];
    [passwordText setEnabled:(enabled && [passwordCheckBox state])];
    [userLimitText setEnabled:(enabled && [userLimitCheckBox state])];
    [userLimitStepper setEnabled:(enabled && [userLimitCheckBox state])];
}

@end
