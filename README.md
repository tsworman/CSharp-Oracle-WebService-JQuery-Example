# CSharp-Oracle-WebService-JQuery-Example
An example of a C# Webservice that establishes a connection to an Oracle Database and returns JSON to the browser. HTML included to call the Login service using JQuery

Requests a functioning Oracle install on the server. You may get around this by using the Oracle Managed Data Adapter Driver from NuGet.

The pattern:
Web app runs on a single web-server
Client connects to web-server unauthenticated.
Webservice authenticates user to the Oracle database.
Webserver maintains session as long as the app pool is not cycled.
Calls to other functions on this webservice use the stored session.
The oracle database timeout for inactivity is greater than the app pool cycle for time out.

I include examples for login, logout, Oracle stored function calls, stored procedure calls, direct SQL running, and returning JSONP/JSON to browser.

The HTML file included handles login via the webservice. 
It calls the login service via a post using JQuery, passes arguments and then writes the response to the console log. 
The example responds with a response object that has a status of failed and an error message if we know what error occured.
The service call will fail if IIS fails with a 500 or some error itself. I wanted to handle both cases differently in the web app.
Password, invalid server, or invalid parameter data can be handled gracefully. Missing parameters or calling via a HTTPGet when post is expected are different use cases that a user shouldn't encounter. I can handle those differently in the client as they are returned as failures on the AJAX call vs a failed status Response object.

Questions:
Tyler Worman
nova1313@novaslp.net
