FTPManager
==========

A simple FTP client for uploading / downloading files to and from an FTP server. Can be used within your .NET project instead of having to use a third party tool. For basic FTP uploads / downloads / deletes / lists.

Usage
=====

You can either compile the solution into an assembly and add a reference to the assembly from your project, or you can simply add the code directly into your project as a source file.

using FTPManager;

FTPManager manager = new FTPManager("myserver", "myusername", "mypassword");

//to upload a file from the local filesystem to the remote FTP server
bool result = manager.Upload("mysourcefile", "destinationfolder/targetfilename", "ascii");

//to download a file from the remote FTP server to the local filesystem
result = manager.Download("mysourcefile", "destinationfolder/targetfilename", "ascii");

//to get a list of files from a specified folder on the remote FTP server
string[] filelist = manager.Getfilelist("remotefolder", "ascii");

//to delete a file on the remote FTP server
result = manager.Deletefile("filetodelete");


