[![Build status](https://ci.appveyor.com/api/projects/status/qkwacb5jf89oqs71?svg=true)](https://ci.appveyor.com/project/ekwus/primitivebase)
[![NuGet Version](https://img.shields.io/nuget/v/PrimitiveBase.svg)](https://www.nuget.org/packages?q=PrimitiveBase)
[![Nuget Downloads](https://img.shields.io/nuget/dt/PrimitiveBase.svg)](https://www.nuget.org/packages?q=PrimitiveBase)
[![License](https://img.shields.io/github/license/ekwus/PrimitiveBase.svg)](https://raw.githubusercontent.com/ekwus/PrimitiveBase/master/LICENSE)
[![Coverage](https://codecov.io/gh/ekwus/PrimitiveBase/branch/master/graph/badge.svg)](https://codecov.io/gh/ekwus/PrimitiveBase)

# PrimitiveBase

General set of base classes implementing the primitive concepts and useful utilities that are required for all but the basic of libraries and applications.

Currently a few Core base classes are implemented;

* `SafeLock`
* `BaseSyncronised`
* `BaseDisposable`
* `BaseNotifiable`

A start to the Networking namespace has been made with;

* `NetAddress`

Further Networking classes will be added once the unit testable wrappers have been created for `Socket` and `SocketAsyncEventArgs`

There is also a plan to provide logging that will utilise 'ILogger' and the flexibility to use any of the logging frameworks.