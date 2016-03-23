# QlikSenseTask
QlikSenseTask

QlikSenseTask.exe (QlikSenseTask.zip) is a tool for starting Qlik Sense tasks from the command line.  Why QlikSenseTask?  There will be many cases where QlikView data models and QVDs need to be loaded in QlikSense applications.  This utility allows QlikView Publisher (and other schedulers) to call QlikSense tasks and load data into Qlik Sense apps.

#h2 Usage

![Usage](https://github.com/marcusspitzmiller/QlikSenseTask/blob/master/Screenshots/usage.JPG)

#h3 Call QlikSenseTask.exe with the following parameters:
-proxy:<proxyserver> - URL of Qlik Sense proxy
-task:<taskname> - name of reload task to execute.  Put task names with spaces in quotes.
-wait:<seconds> - number of seconds the utility should wait for task to execute before timing out.

#h3 Global Settings:
Use config.txt to set any of these parameters at a global level.  The command line values override the settings in config.txt.
For example, use config.txt to set the -proxy and -wait values at a global level so that each time the executable is called, these values to not need to be set from the command line.

#h2 With QlikView Publisher
With QlikSenseTask,exe, QlikView Publisher can be used to call reload tasks in Qlik Sense.  Below, QlikSenseTask.exe is set up as a supporting task in Publisher which reloads a task in Qlik Sense named "Reload Sales Dashboard" and waits 90 seconds before terminating.

![Pub Dependancy](https://github.com/marcusspitzmiller/QlikSenseTask/blob/master/Screenshots/publisher_dependancy.JPG)

 
This supporting task is triggered on the success of the reloaded data model in QlikView.  Not pictured, the script in the Qlik Sense application binary loads the prepared QVW or QVD files.

![Pub Task](https://github.com/marcusspitzmiller/QlikSenseTask/blob/master/Screenshots/publisher_task.JPG)

 
Installation
Download and unzip the QlikSenseTask zip below.  Place in a directory accessible to QlikView Publisher (or any other scheduling tool).
Modify config.txt to point to the Qlik Sense proxy.
 
Qlik Sense 1.0.X - QlikSenseTask.zip
Qlik Sense 1.1 - QlikSenseTask-1.1.zip
Note: two different versions as the task statuses changed.
 
Security
The current version of QlikSenseTask.exe only supports NTLM authentication.  In the content of QlikView Publisher, this means that the service running Publisher will be the account to authenticate to Qlik Sense.  (Note: It is possible but not yet implemented to use SSL certificates as an authentication mechanism.  If there is a need, please let me know.)

The user that calls Qlik Sense needs to have access to the task.  The simplest way to achieve this is to make the user a ContentAdmin in the Qlik Sense QMC.
 
