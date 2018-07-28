#!/bin/sh

while [ "$1" != "" ]; do
    case $1 in
        -i | --image)
            IMAGE=$2
            SET="$SET --set image=$2"
            ;;
        -r | --release)
            RELEASE=$2
            ;;
        -c | --chart)
            CHART=$2
            ;;
        -s | --set)
            SET="$SET --set $2"
            ;;
        *)
            echo "ERROR: unknown parameter \"$1\""
            exit 1
            ;;
    esac
    shift 2
done
echo "IMAGE = $IMAGE; RELEASE = $RELEASE; CHART = $CHART"
helm upgrade --install$SET $RELEASE $CHART