#!/bin/bash

# Uninstalls the npnf Platform SDK from your current Unity project
# The npnf Settings asset will not be deleted

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
FILELIST=$DIR/npnf_filelist.txt

if [ -f "$FILELIST" ]; then

( while read -r line
do
	sdkfile=$DIR/$line
	if [ -f "$sdkfile" ] || [ -d "$sdkfile" ]; then
	( echo $sdkfile
	rm -fRv "$sdkfile"
	rm -fRv "$sdkfile.meta" )
	fi

done < "$FILELIST"

rm -fRv "$FILELIST"
rm -fRv "$FILELIST.meta"

if [ ! -d "$DIR/NPNF/Resources" ]; then
	rm -fRv "$DIR/NPNF"
fi

echo
echo "The npnf Platform SDK is uninstalled!" )

else echo "Filelist does not exist!"
echo "Failed to uninstall the npnf Platform SDK. You must manually uninstall the SDK."
fi