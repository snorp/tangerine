
#include <stdio.h>
#include <stdlib.h>
#include <strings.h>
#include <X11/SM/SMlib.h>
#include <signal.h>

static IceConn iceconn = NULL;
static int pipes[2];
static int closed = 0;

static void
session_died (SmcConn conn, SmPointer client_data)
{
	SmcCloseConnection (conn, 0, NULL);
	kill (getpid (), SIGTERM);
}

static void
save_yourself (SmcConn conn, SmPointer client_data, int save_type, Bool shutdown, int interact_style, Bool fast)
{
	SmcSaveYourselfDone (conn, True);
}

static void
save_complete (SmcConn conn, SmPointer client_data)
{
}

static void
shutdown_cancelled (SmcConn conn, SmPointer client_data)
{
}

static void
ice_watch (IceConn ice_conn, IcePointer client_data, Bool opening, IcePointer *watch_data)
{
	if (opening) {
		iceconn = ice_conn;
	}
}

void
close_session (void)
{
	if (iceconn != NULL) {
		closed = 1;
		write (pipes[1], "closed", 7);
	}
}

void
run_session (void)
{
	SmcConn conn;
	SmcCallbacks callbacks;
	char *id;
	fd_set rfds;

	IceInitThreads ();
	IceAddConnectionWatch (ice_watch, NULL);

	bzero (&callbacks, sizeof (SmcCallbacks));
	callbacks.die.callback = session_died;
	callbacks.save_yourself.callback = save_yourself;
	callbacks.save_complete.callback = save_complete;
	callbacks.shutdown_cancelled.callback = shutdown_cancelled;
	
	conn = SmcOpenConnection (NULL, NULL, 1, 0, SmcDieProcMask | SmcSaveYourselfProcMask |
				  SmcSaveCompleteProcMask | SmcShutdownCancelledProcMask, &callbacks,
				  NULL, &id, 0, NULL);
	IceRemoveConnectionWatch (ice_watch, NULL);

	if (conn == NULL)
		return;
	
        FD_ZERO (&rfds);
        FD_SET (IceConnectionNumber (iceconn), &rfds);

	pipe (pipes);
	FD_SET (pipes[0], &rfds);
	
	
        while (select (pipes[0] + 1, &rfds, NULL, NULL, NULL) > 0) {
		if (closed) {
			SmcCloseConnection (iceconn, 0, NULL);
			break;
		} else if (IceProcessMessages (iceconn, NULL, NULL) == IceProcessMessagesConnectionClosed) {
			break;
		}
	}
}
