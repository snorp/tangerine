#import <Cocoa/Cocoa.h>

typedef void (*SpotlightHelperCallback) (NSMetadataItem *item);

@interface QueryHelper : NSObject {
    NSMetadataQuery *query;
    SpotlightHelperCallback callback;
    BOOL running;
    NSAutoreleasePool *pool;
}

- (void) setCallback:(SpotlightHelperCallback)cb;
- (void) run;
- (void) stop;

@end

void tangerine_run_music_query (SpotlightHelperCallback cb);
void tangerine_stop_music_query (void);

char *tangerine_item_get_path (NSMetadataItem *item);
char *tangerine_item_get_artist (NSMetadataItem *item);
char *tangerine_item_get_album (NSMetadataItem *item);
char *tangerine_item_get_title (NSMetadataItem *item);

int tangerine_item_get_duration (NSMetadataItem *item);
short tangerine_item_get_bitrate (NSMetadataItem *item);

const char *tangerine_get_string_value (NSMetadataItem *item, const char *key);