#import "QueryHelper.h"

static QueryHelper *helper;

@implementation QueryHelper

- (void) queryNotified:(NSNotification*)notification
{
    if ([notification name] == NSMetadataQueryDidFinishGatheringNotification ||
        [notification name] == NSMetadataQueryDidUpdateNotification) {
        
        [query disableUpdates];
        
        int i;
        for (i = 0; i < [query resultCount]; i++) {
            NSMetadataItem *item = [query resultAtIndex:i];
            
            @try {
                callback (item);
            }
            @catch (NSException *e) {
                NSLog([NSString stringWithFormat:@"Problem with file '%@', dropping", [item valueForAttribute:@"kMDItemPath"]]);
            }
        }
        
        callback (NULL);
        
        [query enableUpdates];
    }
}

- (id) init {
    self = [super init];
    if (self != nil) {
        pool = [[NSAutoreleasePool alloc] init];
        query = [[NSMetadataQuery alloc] init];
        [query setPredicate:[NSPredicate predicateWithFormat:@"kMDItemContentType == 'public.mp3'"]];
        [query setSearchScopes:[NSArray arrayWithObject:NSMetadataQueryUserHomeScope]];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(queryNotified:) name:nil object:query];

    }
    return self;
}

- (void) setCallback:(SpotlightHelperCallback)cb
{
    callback = cb;
}

- (void) run
{
    [query startQuery];
    
    running = YES;
    while (running && [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode beforeDate:[NSDate distantFuture]]) {
        [pool release];
    }
}

- (void) stop
{
    running = NO;
    [query stopQuery];
}

- (void) dealloc {
    [self stop];
    [query release];
    [super dealloc];
}


@end



void
tangerine_run_music_query (SpotlightHelperCallback cb)
{
    helper = [[QueryHelper alloc] init];
    [helper setCallback:cb];
    [helper run];
}

void
tangerine_stop_music_query (void)
{
    if (helper == NULL)
        return;
    
    [helper stop];
    [helper release];
}

static char *
tangerine_item_get_string (NSMetadataItem *item, NSString *key)
{
    const char *val = [[item valueForAttribute:key] cString];
    if (val == NULL) {
        return NULL;
    }
    
    return strdup(val);
}

char *
tangerine_item_get_path (NSMetadataItem *item)
{
    return tangerine_item_get_string (item, @"kMDItemPath");
}

char *
tangerine_item_get_artist (NSMetadataItem *item)
{
    NSArray *array = [item valueForAttribute:@"kMDItemAuthors"];
    if (array && [array count] > 0) {
        return strdup ([[array objectAtIndex:0] cString]);
    } else {
        return NULL;
    }
}

char *
tangerine_item_get_album (NSMetadataItem *item)
{
    return tangerine_item_get_string(item, @"kMDItemAlbum");
}

char *
tangerine_item_get_title (NSMetadataItem *item)
{
    return tangerine_item_get_string (item, @"kMDItemTitle");
}

int
tangerine_item_get_duration (NSMetadataItem *item)
{
    NSNumber *num = [item valueForAttribute:@"kMDItemDurationSeconds"];
    return [num intValue];
}

short
tangerine_item_get_bitrate (NSMetadataItem *item)
{
    NSNumber *num = [item valueForAttribute:@"kMDItemAudioBitRate"];
    return [num shortValue];
}