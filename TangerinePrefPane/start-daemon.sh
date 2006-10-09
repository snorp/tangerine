#!/bin/bash

echo $$ > ~/.tangerine.pid
echo `pwd`
exec mono TangerinePrefPane.prefPane/Contents/Daemon/tangerine-daemon.exe