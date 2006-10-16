//
//  TangerinePrefPanePref.h
//  TangerinePrefPane
//
//  Created by James Willcox on 10/7/06.
//  Copyright (c) 2006 __MyCompanyName__. All rights reserved.
//

#import <PreferencePanes/PreferencePanes.h>
#import "TangerineIniParser.h"


@interface TangerinePrefPanePref : NSPreferencePane 
{
    IBOutlet NSButton *enabledCheckBox;
    
    IBOutlet NSTextField *shareNameText;
    IBOutlet NSMatrix *radioGroup;
    
    IBOutlet NSButtonCell *automaticRadio;
    IBOutlet NSButtonCell *itunesRadio;
    IBOutlet NSButtonCell *dirRadio;
    IBOutlet NSTextField *dirText;
    IBOutlet NSButton *chooseButton;
    IBOutlet NSTextField *passwordText;
    IBOutlet NSTextField *userLimitText;
    IBOutlet NSStepper *userLimitStepper;
    IBOutlet NSButton *passwordCheckBox;
    IBOutlet NSButton *userLimitCheckBox;
    
@private
    BOOL dirty;
    TangerineIniParser *config;
}

- (void) mainViewDidLoad;
- (void) didUnselect;
- (void) refreshEnabled;
- (void) saveChanges;

- (IBAction)changed:(id)sender;
- (IBAction)chooseButtonPressed:(id)sender;

@end
