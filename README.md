# DDNS
QaD DDNS client and server using route53

## Overview
This consists of two components - a webapi that is meant to be published out in the world and answer with the NATed (or not I guess, 
the assumption is that client can't see its own publically routable IP) IP address - and a client console app that is meant to run via
a scredule and manitains all its relevent config on the json file.

## Notes
This is really only meant to handle one A record and IP, though if it needs expansion it is easy enough.  Also it can run with an
app id and secret in the config leaving those around isn't great, it will also run under a profile just as easily (not configured,
it will use default).
