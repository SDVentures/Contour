## 1.7.8
- Fixed:
  + Producer locking in send operation has been relaxed to use read-write locking, since an ordinary lock may make the send operation in parallel thread (of a listener, for example) to "fail fast" while acquiring a lock which will look like a producer is re-starting. This condition can be observed in a scenario with a huge number of listeners (high number of incoming labels and/or high parallelism level) trying to emit messages via a specific sender. Despite the incoming message rate can be high, an endpoint is not currently able to scale on the outgoing side.

## 1.7.7
- Changes:
  + Listeners are no longer shared between receivers; thus in case of concurrent message consumption by two or more listeners from the same queue the broker will define a listener to handle the message.

## 1.7.6
- Improvements:
  - Fault tolerant producer is now able to set and manage retry delays when making send attempts (see https://github.com/SDVentures/Contour/issues/47);
  - Payload logging in sender has been removed;

## 1.7.5
- Fixed:
  - Round robin will not rotate if everyone has jumped off (https://github.com/SDVentures/Contour/issues/79)

## 1.7.4
- Fixed:
  - Expired response handling (https://github.com/SDVentures/Contour/issues/76)

## 1.7.3
 - Attempt errors aggregation added
 - Listener consuming action fallback changed to take the first action if none of the provided satisfy the accepted labels; Message properties extraction fixed: the source properties of the message are always preserved;
 - Listener sharing fixed: separate listeners are created for separate labels and queues on the same host; a shared listener is created for different labels(exchanges) attached to the same queue on the same host
 - Listener delistment and producer re-enlistment introduced to reconfigure producer on channel shutdown
 - Dynamic routing fixed: a new disposable channel is obtained each time topology is rebuilt

## 1.7.2
 - Fixed:
   - Dynamic routing: a new disposable channel is obtained each time topology is rebuilt;

## 1.7.1
 - Fixed:
   - Topology builder channel usage defect;
   - Rabbit connection operations access serialization;

## 1.7.0
 - Connection failover support introduced;
 - Runtime connection string provider feature added;

## 1.6.0
 - Enables label sharding support (according to feature definition in https://github.com/SDVentures/Contour/issues/27):
   - an endpoint should be able to connect to several broker nodes (with different connection strings) simultaneously;
   - an endpoint should receive messages from all broker nodes (connection strings) specified in the configuration element;
   - an endpoint should send messages to only one broker node (connection string) which should be selected based on a list of supported algorithms (Round Robin);
   - the connection string property specified on the endpoint or incoming/outgoing configuration element should be able to define a list of broker URLs;


## 1.5.0
 - An issue https://github.com/SDVentures/Contour/issues/25 has been fixed. A new correlation id will always be used for outgoing requests.

## 1.4.1
 - NuGet package image added.

## 1.4.0
 - BusProcessingContext used by operators now has reference to used IBusContext.

## 1.3.0
 - Separate connection for each sender and receiver (connection per label) feature introduced. Connection configuration per label (connection string and reuse flag) feature introduced. Connection reuse property switched to bool? due to Maybe<bool>(false) bug.

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
