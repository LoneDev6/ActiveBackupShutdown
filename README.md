# ActiveBackupShutdown
Automatically hibernate PC when ActiveBackupForBusiness finished all its backup scheduled.

This is useful if you have backups running in night and you want the PC to suspend once all backups finished completely.

It won't hibernate your PC if the login form is not visible, so it won't interrupt your work in case you're using the PC on night.

## How to install this
- compile it
- save the built folder somewhere
- create a Windows scheduled task to lauch this exe and mark it to be executed on each start of your PC (even if no one is logged in)
- done

#### Notes
This was tested only with 2 scheduled backups and both had successful state.
It's important to test it also with failed backups and maybe also with more than 2 scheduled backups.

Tested only on Windows 10 2004
