# AutoGoogleDynamicDNS
An automatic way to update the DNS entries on a domain serviced by Google

## Notes
This program utilizes Google's API to make changes to a DNS entry of a domain name that is hosted by Google's Domain Service.  This was written to help my Domain Name always be up to date with my ISP Provided IP address that isn't static on my homelab.

The XML file, Config.xml, is required by the program to run.  When the program is started the xml will be created in the same run directory if it is not detected and then exit.  Configuring the file will be required for a successful next run.

## XML Configuration
DynDNS API.exe uses Config.xml to allow for multiple entries.  For instance, a subdomain and a top level domain DNS entry can be updated in the same run by having two 'Host' nodes.  Google's API, by design, requires different Username/Passwords for each entry being updated.  The IPAddress section is not required, but can be statically defined if it is different than the public facing IP of the computer that the program is being run on.  If left blank, the program will get the IP of the computer the program is being run on via a simple webscrape and use that one, hence the Dynamic part of Dynamic DNS.

## Logging
The programm is set to automatically log all actions that the program takes.  In the future the functionally may be added to turn logging on/off with a command but it is currently not deemed necessary.
