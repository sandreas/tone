#!/bin/sh
VERSION="$1"
if [ "$VERSION" = "" ]; then
  echo "please provide a version as first parameter (e.g. 1.0.0)"
  exit 1  
fi

RELNOTES_FILENAME="release-notes-v$VERSION.md"
ls */doc/release/$RELNOTES_FILENAME 1> /dev/null 2>&1
if [ "$?" = "0" ]; then
  git tag -a "v$VERSION" -m "release $VERSION" && git push origin --tags
else
  echo "Please create $RELNOTES_FILENAME!"
fi 
 
