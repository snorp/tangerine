#!/bin/bash

echo $$ > ~/.tangerine.pid
asmdir=`dirname $0`
exec mono $asmdir/tangerine-daemon.exe