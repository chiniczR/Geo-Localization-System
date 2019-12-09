# Fall Localization System

This is a bomb-fall (although it could technically be about any sort of user-reported physical event, e.g. bird sightings, etc.) localization system, developed for a course on Windows System Engineering. For more details about how it works, see the about window of the program (no log in necessary).

Users and Falls are supposed to be stored in a local MS MySQL server.

The code follows the MVVM architecture, and makes use of Entity framework to map Fall/User objects to their equivalent entities in a local MS MySQL database. The exact location of a fall for which a geo-tagged photo has not been uploaded yet is estimated with the K-Means algorithm (applied to the coordinates of the address entered in the current report and the most recent reports - up to 10 minutes ago, unless otherwise specified).
