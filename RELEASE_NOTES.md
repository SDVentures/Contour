## 2.2.0-core1
	Upgraded RabbitMQ.Client to 6.1.0 for optimization CPU

## 2.1.0-core9
	Optimized performance for dynamic routing

## 2.1.0-core8
	Fixed CanRoute for dynamic outgoing

## 2.1.0-core7
	Added breadcrumbs and originalMessageId to diagnostic props

## 2.1.0-core5
	Replaced Dictionary with ConcurrentDictionry because AsyncLocal value can be called concurrently

## 2.1.0-core4
    Added opportunity to add custom headers by publishing options for diagnostic reason

## 2.1.0-core3
    Added opportunity to get message label by alias for diagnostic reason

## 2.1.0-core2
    Added diagnostic information about connection connectionString when consuming messages

## 2.1.0-core1
    Changed accessibility level for diagnostic storage

## 2.1.0-core0
    Added opportunity to extract last publish operation connection string for diagnostic reason

## 2.0.1-core5
    Now AppConfigConfigurator is able to accept external configuration.

## 2.0.1-core4
    Now AppConfigConfigurator is able to accept external configuration.

## 2.0.1-core3
    + Changing access level for configuration classes.

## 2.0.1-core2
    + Changing bus configuration logic.

## 2.0.1-core1
    + Migrating from .NET Framework 4.7.1 to .NET Standard 2.1

## 2.0.0-rc2
    + Migrating from .NET Framework 4.7.1 to .NET Standard 2.1
    + Removed Ninject dependency.
    + Updated Common.Logging and Fluent Assertions.

## 2.0.0-rc2
    + Removed FluentValidation dependency.

## 2.0.0-rc1
    + Removed Newtonsoft.Json dependency.
    + Removed support of caching.

## 1.8.0-rc4
    + Upgraded RabbitMQ.Client to 5.0.1

## 1.8.0-rc3
    + Removed circuit breaker from FaultTolerantProducer

## 1.8.0-rc2
    + Connection string provider can now be configured in code
    + Endpoint connection string in now optional

## 1.8.0-rc1
    + A set of specific incoming message headers can now be excluded when being copied to the outgoing message

## 1.7.10
    + Round robin selector made thread-safe; empty collection requirement removed;
    + Request default timeout fixture added;
    + Sender request timeout is set to 30 seconds by default
    + Listener tasks are now created as long running on dedicated threads due to poor application performance while using the thread pool with default configuration;

## 1.7.9
    + All producers will now be re-enlisted on channel failure. This will optimize the time needed to send a message in case of connection drops because the sender will immediatelly switch to the next available producer in queue.

## 1.7.8
    + Producer locking in send operation has been relaxed to use read-write locking, since an ordinary lock may make the send operation in parallel thread (of a listener, for example) to "fail fast" while acquiring a lock which will look like a producer is re-starting. This condition can be observed in a scenario with a huge number of listeners (high number of incoming labels and/or high parallelism level) trying to emit messages via a specific sender. Despite the incoming message rate can be high, an endpoint is not currently able to scale on the outgoing side.

## 1.7.7
    + Listeners are no longer shared between receivers; thus in case of concurrent message consumption by two or more listeners from the same queue the broker will define a listener to handle the message.

## 1.7.6
    - Fault tolerant producer is now able to set and manage retry delays when making send attempts (see https://github.com/SDVentures/Contour/issues/47);
    - Payload logging in sender has been removed;

## 1.7.5
    - Round robin will not rotate if everyone has jumped off (https://github.com/SDVentures/Contour/issues/79)

## 1.7.4
    - Expired response handling (https://github.com/SDVentures/Contour/issues/76)

## 1.7.3
    - Attempt errors aggregation added
    - Listener consuming action fallback changed to take the first action if none of the provided satisfy the accepted labels; Message properties extraction fixed: the source properties of the message are always preserved;
    - Listener sharing fixed: separate listeners are created for separate labels and queues on the same host; a shared listener is created for different labels(exchanges) attached to the same queue on the same host
    - Listener delistment and producer re-enlistment introduced to reconfigure producer on channel shutdown
    - Dynamic routing fixed: a new disposable channel is obtained each time topology is rebuilt

## 1.7.2
    - Dynamic routing: a new disposable channel is obtained each time topology is rebuilt;

## 1.7.1
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
