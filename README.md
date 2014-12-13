# Overview

This is a simlified version of the nice [ApiChange](http://apichange.codeplex.com/) tool. Here are changes that I made:
* Mono.Cecil was updated to the latest version to support .NET 4 assemblies
* If breaking change is found then app exists with -1 code (useful for automated checks)
* A lot of code not required for backward compatibility checks was removed
