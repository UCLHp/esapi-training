# esapi-training
NON-CLINICAL scripts. Purely for practice with ESAPI and C#.  



## Overview
This repo hopefully contains useful information for you to get up and running with the Eclipse Scripting API (ESAPI), C# and Visual Studio.
It also contains some sample scripts to show the range of functionality we might be able to benefit from. As of Eclipse v16.1 we have access to 
write-enabled scripting for proton planning. Script-specific information and learning points can be found in the projects themselves.



## Visual Studio / C#
You don't technically need to use this IDE, but everyone else does and it seems very nice. You can get the free Community edition [here](https://visualstudio.microsoft.com/free-developer-offers/).
I've been using Visual Studio Community 2019 for this work. During installation you will be asked what "Workloads" you want - make sure to tick ".NET desktop development" and
"Universal Windows Platform development". If you find that you don't have these, just open Visual Studio and go to Tools > Get Tools and Features... > Workloads to install them.



## Useful jargon
There's a bunch of jargon that might be new to you, and the way that scripts or applications are structured is confusing at first. Not all of this will be relevant but hopefully it helps.  

- **Solutions & projects**: A *solution* is simply a container that can contain one or more *projects*. An example might be a solution that contains one project with code relating to a specific task, and a second project that contains all the unit testing for it.

Advanced: In the above example we'd give the testing project access to the main code project by adding a *reference* (see later). This action would create a *build dependency*, which means that if you were to build the solution the code project would be built before the test project.

- **Assembly**: these can take the form of .exe or .dll files and are the building blocks of .NET applications. An assembly is a compiled  collection of code (a binary file) for easy sharing.

- **Namespace**: Namespaces are used a lot in C# help organize large code projects. They are used in a similar way to how you might import a python module at the top of your code. Using a namespace allows you to access it's classes without having to type out the complete name each time. In C# we have the *using* command.  

As an example, System is an in-built namespace available to us. Inside that namespace there is a Console class which gives us methods to say WriteLine() to the console. We could access this via `System.Console.WriteLine()` every time, or we could put `using System` right at the top of the code to make that namespace available to us, and then only have to type `Console.WriteLine()` each time.

[Here's](https://zetcode.com/csharp/using/) a good description of how the `using` directive works in C#.

- **References**: If you want to use classes and functions from another project (contained for example in an assembly) we have to manually add the reference to these in our current project.
In Visual Studio this is done from the Solution Explorer tab: right-click on Dependencies and choose Add Reference. You can then select from the options listed, or Browse to the location of your reference.
As an example, you'll see that all of the scripts I have put here contain `using VMS.TPS.Common.Model.API;` and `using VMS.TPS.Common.Model.Types;`. These are required for writing stand-alone scripts, with the
API library essentially connecting Eclipse with C# and the Types library containing all the classes, data structures and functions available to us. You'll need to reference these manually - they can be found on the O: drive.



## Useful links
There are quite a few places to look for code examples and help with ESAPI.
- The Eclipse Scripting API Basics and Reference Guides give a decent overview and simple examples to test out
- Varian [code samples](https://github.com/VarianAPIs/Varian-Code-Samples)
- There are many [Varian webinars](https://www.myvarian.com/s/webinars)
- The ESAPI [reddit page](https://www.reddit.com/r/esapi/) helped me a lot
- [Carlos Anderson's](http://www.carlosjanderson.com/) blog, who developed the [ESAPI Essentials Toolkit](https://www.carlosjanderson.com/introducing-esapi-essentials/) available via NuGet 
- [Rex Cardan](https://github.com/rexcardan) who has done a lot of work on ESAPI, developing the open-source ESAPIX extension library (available via NuGet) as well as the Evil-DICOM library.
- WUSTL [code samples](https://github.com/wustl-ClinicalDev)  




## Instructions to run your first script
1. Todo
2.



## Scripting tips
Todo




## License
```
Copyright (C) 2021 Steven Court

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
```
