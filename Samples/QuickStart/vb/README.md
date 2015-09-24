Quick Start
===========

Quick Start application demonstrates how to create a simple imaging application
using the Lumia Imaging SDK. It shows how to apply a filter effect to an 
image and save it to file.

The example has been developed as a Universal app for Windows 10


For more information on implementation, visit  Microsoft Windows Dev Center: [Lumia Imaging SDK](http://go.microsoft.com/fwlink/?LinkID=521939)


1. Usage
-------------------------------------------------------------------------------

This is a simple build-and-run solution. See section 5 for instructions on how
to run the application on your Windows 10 device.


2. Prerequisites
-------------------------------------------------------------------------------

* VB basics
* Windows 10
* Development environment Microsoft Visual Studio Express 2015 for Windows


3. Project structure and implementation
-------------------------------------------------------------------------------

3.1 Folders
-----------

* The root folder contains the project file, the license information and this
  file.
* `QuickStart`: Root folder for the implementation files.  
 * `QuickStart.Shared`: Project shared between Windows Phone and Windows Store apps
 * `QuickStart.Windows`: Windows Store specific project
 * `QuickStart.WindowsPhone`: Windows Phone specific project


3.2 Important files and classes
-------------------------------

| File                                 | Description                                             |
| ------------------------------------ | ------------------------------------------------------- |
| `Quickstart.Shared\MainPage.xaml.vb` | Main page, displays photo rendered with applied filter. |


4. Compatibility
-------------------------------------------------------------------------------

The application works on Windows 10.

Developed with Microsoft Visual Studio Professional 2015.


4.2 Known Issues
----------------

None.


5. Building, installing, and running the application
-------------------------------------------------------------------------------

5.1 Preparations
----------------

Make sure you have the following installed:
 * Windows 10
 * Visual Studio 2015 or 2015 Express for Windows

5.2 Building the solution
---------------------------------

1. Open the solution file:
   FILE -> Open Project, select the file QuickStart.sln
2. Open the NuGet solution package manager (TOOLS -> NuGet Package Manager -> Manage packages for solution)
3. Press 'Restore' to restore missing packages
4. Unload/reload the projects, or close and reopen the solution (this is needed for Visual Studio to refresh the references for the projects)
5. Right click on one of the platform specific projects and select "Set as StartUp project"
6. Select the correct target and platform combination (Device/ARM, Local Machine/x86 etc.)
7. Deploy and launch the app on the selected platform:
   DEBUG -> Start (Without) Debugging


6. License
-------------------------------------------------------------------------------

See the license text file delivered with this project


7. Version history
-------------------------------------------------------------------------------

* 1.0.0.0: First public release of Quick Start for Windows 10

