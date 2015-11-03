# NLight
Toolbox for .NET projects

## Features

### NLight.Collections

* `BufferManager`: maintains a set of large arrays and gives clients segments of these that they can use as buffers
* `OrderedDictionary<TKey, TValue>`: dictionary that also keeps track of item order by implementing `IReadOnlyList<TValue>`
* `Trees.TreeTraversals`: functions to walk any tree in pre/in/post and level order (implemented without using recursion)

### NLight.Core

* `Buffer<T>`: a data buffer that can refill automatically via an user provided function
* `EnumHelper`
  * Validates enums 
  * Allows access to their metadata quickly
* `PreciseDateTime`: returns the current date as a high precision `DateTimeOffset`
* `StringExtensions`: extensions to remove diacritics and add more comparison options
* `StrongRandom`: provides the same functionalities as `System.Random` but uses `System.Security.Cryptography.RandomNumberGenerator`

### NLight.IO

* `IOHelper`
  * Copy and move operations with progress notification
  * Copy will create a symbolic link if possible
  * Provides a safe filesystem enumerator that ignores file access errors
* `Text.DelimitedTextRecordReader/Writer`: a fast and flexible delimited file reader and writer
* `Text.FixedWidthTextRecordReader/Writer`: a fast and flexible fixed width file reader and writer

### NLight.Reactive

* `BehaviorSubjectSlim<T>`: a `SubjectSlim<T>` (see below) that keeps track of the current value
* `SubjectSlim<T>`: an `ISubject<T>` implementation that is still thread-safe but avoids locking internally and stores observers with a `List<T>` with copy on write instead of an `ImmutableList<T>`
* `DeferredSubject<T>`: an `ISubject<T>` wrapper that buffers new elements in a `BlockingCollection<T>`
* `ObservableExtensions`
  * Ignore elements while converting the current one
  * Pairs the current element with the previous

### NLight.Text

* `StringValueConverter`: converts a string to any .NET primitive types, enums or Guid
* `Encoding.FastASCIIEncoding`: a plain ASCII encoding (ignores code pages) that uses loop unrolling to be faster
* `Parsing.EscapedStringParser`: parses backslash escaped strings

### NLight.Threading

* `Tasks.SingleThreadTaskScheduler`: a task scheduler that runs tasks on a single dedicated thread

### NLight.Transactions

* `TransactionManager`: a transaction manager that supports ambient nested transaction contexts

## Contributing Code

To contribute to this project, please follow the standard .NET framework design guidelines and indent your code with tabs. Your code should compile with no warning in release mode.
