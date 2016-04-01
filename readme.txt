=== IGoEnchi Readme File ===

About

  IGoEnchi is an Internet Go Server (IGS) client and SGF editor 
  for Windows Mobile. For more information please visit IGoEnchi 
  homepage at

  http://sourceforge.net/projects/igoenchi/

How To Build

  1. Download and install SharpDevelop 3.1, .NET 2.0 SDK,
  	 PowerToys for .NET CompactFramework 3.5 and 
  	 .NET CompactFramework 2.0 SP2.
  2. Start a new C#->Compact Framework->Windows Application project.
  3. Change target runtime to .NET CF 2.0 at 
	 Project->Project Options->Compiling->Target Framework
  4. Remove all the references, then inlude references to  
		System.dll
		System.Data.dll
		System.Drawing.dll
		System.Xml.dll
		System.Windows.Forms.dll
		System.Windows.Forms.DataGrid.dll
		Microsoft.WindowsCE.Forms.dll
		Microsoft.WindowsMobile.DirectX.dll
	 from .NET CF 2.0 install directory. By default it is 
	 C:\Program Files\Microsoft.NET\SDK\CompactFramework\v2.0\WindowsCE\
  5. Add source files to the project.
  6. Add resources from \data\resources to the project.

Things to remember

  If you're making changes to source code or data please note the following:

  1. IGS parser is running in separate thread, so form's controls need to be 
     accessed via Invoke().
  2. IGS message handlers should not alter the message because it is shared 
     between all registered objects.
  3. Graphical objects like Bitmaps, Graphics, Pens, Brushes, etc. use 
     unmanaged memory which should be freed using Dispose(). 
  4. Toolbar icons need to be saved with 256 colors for transparency to work.
