## 1.3.0
 - Separate connection for each sender and receiver (connection per label) feature introduced.

## 1.2.2
 - QoS & PL settings can now be overriden if no endpoint configuration provided.

## 1.2.1
 - QoS & PL default settings are now preserved if not specified in configufation.

## 1.2.0
 - Incoming message listener QoS and parallelism level configuration added.

## 1.1.1
 - Deleted destructor and overloaded Dispose method to leave only one option for endpoint deletion.

## 1.1.0
 - Added ability to configure a TTL of the fault queue and the maximum queue length of the fault queue.

## 1.0.2
 - Fixed OutOfRangeException when Request/Reply timeouts occur.

## 1.0.1
 - Specified only lower bounds of nuget package dependencies.

## 1.0.0
 - Initial commit.
