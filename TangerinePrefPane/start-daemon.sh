#!/bin/bash

asmdir=`dirname $0`
export LD_LIBRARY_PATH="$asmdir:$LD_LIBRARY_PATH"
exec -a tangerine mono $asmdir/tangerine-daemon.exe