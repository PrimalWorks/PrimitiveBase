[![Build status](https://ci.appveyor.com/api/projects/status/qkwacb5jf89oqs71?svg=true)](https://ci.appveyor.com/project/ekwus/primitivebase)
[![NuGet Version](https://img.shields.io/nuget/v/PrimitiveBase.svg)](https://www.nuget.org/packages?q=PrimitiveBase)
[![Nuget Downloads](https://img.shields.io/nuget/dt/PrimitiveBase.svg)](https://www.nuget.org/packages?q=PrimitiveBase)
[![License](https://img.shields.io/github/license/ekwus/PrimitiveBase.svg)](https://raw.githubusercontent.com/ekwus/PrimitiveBase/master/LICENSE)
[![Coverage](https://codecov.io/gh/ekwus/PrimitiveBase/branch/master/graph/badge.svg)](https://codecov.io/gh/ekwus/PrimitiveBase)

# PrimitiveBase

General set of base classes implementing the primitive concepts and useful utilities that are required for all but the basic of libraries and applications.

Currently a few Core base classes are implemented;

* `SafeLock`
  A lockable object that timeouts to prevent potential deadlock situations
* `BaseSyncronised`
  Providing a SyncRoot of `SafeLock` type for use with `using(SyncRoot.Enter()) { ... }`
* `BaseDisposable`
  Implements a Disposable pattern for quick and simple use
* `BaseNotifiable`
  Implements `INotifyPropertyChanged` for use when your classes are using binding
* `PLog`
  A logging framework based on `ILogger` and `ILoggerFactory` so you can use your favourite Logger such as NLog, Serilog or log4net

A start to the Networking namespace has been made with;

* `NetAddress`
  Wraps up IPEndPoint with helpful string based construction. This will be used throughout the future networking classes

Further Networking classes will be added once the unit testable wrappers have been created for `Socket` and `SocketAsyncEventArgs`
