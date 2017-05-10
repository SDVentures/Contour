 ![logo](https://cloud.githubusercontent.com/assets/888475/19428671/00fd0fb8-9454-11e6-8d70-a9355ea3bbda.png) Contour
========

![Contour status](https://ci.appveyor.com/api/projects/status/github/sdventures/contour?branch=master&svg=true)
[![NuGet Status](http://img.shields.io/nuget/v/Contour.svg?style=flat)](https://www.nuget.org/packages/Contour/)

## About

Contour is the message bus implementation which provides communication between .NET services via different transport protocols.

**Contour builds all the topology (exchanges, queues with all properties set up) for you.** At this moment it supports only AMQP/RabbitMQ.

## Configuring via xml-file

### Configuration

Configuration can be set in .config file, declaring the 'endpoints' section inside the 'serviceBus' group:

```xml
<sectionGroup name="serviceBus">
    <section name="endpoints" type="Contour.Configurator.EndpointsSection, Contour" />
</sectionGroup>
```

### Endpoints declaration

Each endpoint represents separate service bus instance using which messages are sent and / or received. For example, if application has two endpoints, then from service bus point of view it looks like two different participants (producer or / and consumer). And vice versa, if two different applications use the same endpoint name, then it’s like one interaction participant for service bus (when default routing is used), so you can create concurrent consumers.

Endpoints configuration is defined in 'serviceBus' section:
```xml
<serviceBus>
    <endpoints>
        <endpoint name="point1" connectionString="amqp://localhost:5672/ ">
            <!-- ... -->
        </endpoint>
        <endpoint name="point2" connectionString="amqp://localhost:5672/ ">
            <!-- ... -->
        </endpoint>
    </endpoints>
</serviceBus>
```

It is possible to define the configuration for multiple endpoints. Each endpoint has mandatory parameters: unique name and connection string to the broker.

Additionally, we can pass component name of [IBusLifecycleHandler](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/IBusLifecycleHandler.cs) type in the attribute 'lifecycleHandler', which will be invoked when the state of service bus client is changed. For example, when it starts or stops.

```xml
<serviceBus>
    <endpoints>
        <endpoint name="point1" connectionString="amqp://localhost:5672/" lifecycleHandler="SpecialLifecycleHandler">
            <!-- ... -->
        </endpoint>
    </endpoints>
</serviceBus>
```
It may be helpful, if your component needs to know when service bus was started or stopped.

Every message, that failed to be processed, is added to the fault queue. Fault queue has the same name as the endpoint with additional '.Fault' postfix.
**faultQueueTtl** attribute controls TTL of fault messages in such queues.
**faultQueueLimit** attribute limits maximum number of messages. If number of fault messages reaches this limit, old messages will be substituted.

```xml
<serviceBus>
    <endpoints>
        <endpoint name="point1" connectionString="amqp://localhost:5672/" faultQueueTtl="3.00:00:00" faultQueueLimit="500">
            <!-- ... -->
        </endpoint>
    </endpoints>
</serviceBus>
```
Default value for TTL - 21 days, for maximum size of the fault queue - no limits.

### Declaration of incoming messages

All incoming messages are declared in the 'incoming' collection section. Each message label declares with an individual element 'on'. 'key' and 'label' attributes are mandatory.
For each incoming message topology elements are created if they don't exist:
 - exchange with name equal to label attribute value;
 - queue with name "*&lt;endpoint name>.&lt;label>*"
 - binding from exchange to queue.

Additionally, you can set the parameters (as attributes of the element 'on'):
 - __requiresAccept__ – event processing must be explicitly confirmed in the handler,otherwise message will be returned to the queue (at-least once delivery). False by default (at-most once delivery);
 - __react__ – message handler name (for example, the name of the registered handler in the IoC-container);
 - __type__ – CLR-type of the event body. Can be represented by fully qualified name. In this case, default mechanism of type searching is used. Or you can use a short name type. In this case, it will search for all loaded assemblies in the application domain.If type is not specified, ExpandoObject will be used by default.
 - __validate__ – message validator name (class that implements [IMessageValidator](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/Validation/IMessageValidator.cs)), which operates only within a declared subscription;
 - __lifestyle__ – allows you to specify a handler lifestyle.

Lifestyle possible values:
 - __Normal__ – handler is requested, when you create a bus client (default value);
 - __Lazy__ – handler is requested once, when you get the first message;
 - __Delegated__ – handler is requested every time you get the message, allowing you to manage handler lifetime.
```xml
<incoming>
    <on key="message" label="message.label" react="MessageHandler" />
    <on key="request" label="request.label" react="RequestHandler" type="RequestPayload" validate="InputValidator" requiresAccept="true" />
    <on key="lazy" label="lazy.label" react="LazyHandler" lifestyle="Lazy" />
</incoming>
```

### Declaration of global message validators

Each incoming message passes through global validators, so they can be optionally defined in 'validators' section. Each class of validator must implement [IMessageValidator](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/Validation/IMessageValidator.cs) interface (for more convenience you can inherit from class [FluentPayloadValidatorOf<T>](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/Validation/Fluent/FluentPayloadValidatorOf.cs), which has already implemented this interface).

Also you can set a value of 'group' parameter to 'true'. This would mean that you don’t need to enumerate all validators in component, and just add the name of a validators group, which will include all classes, implementing interface [IMessageValidator](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/Validation/IMessageValidator.cs).

```xml
<endpoint name="point1" connectionString="amqp://localhost:5672/ ">
    <validators>
        <add name="NameValidator" group="true"/>
    </validators>
    <!-- ... -->
</endpoint>
```

### Caching

Tag 'caching' controls the caching of incoming Request/Reply messages

You can enable caching by setting attribute to ‘enabled’. At the requested side response is cached by the key, which is generated on the basis of the request body. Caching time is defined by the responder when calling 'Reply' method.

'Publisher/Subscriber' requests are not cached.

```xml
<endpoint name="point1" connectionString="amqp://localhost:5672/">
    <caching enabled="true"/>
    <!-- ... -->
</endpoint>
```

### Configuring parallel consuming

Endpoint provides ability to setup the necessary number of handler threads, processing incoming messages, in '**parallelismLevel**' attribute. Messages are distributed between the handlers by the Round-Robin principle.
```xml
<endpoints>
    <endpoint name="Emitter" connectionString="amqp://localhost:5672/" parallelismLevel="4">
    </endpoint>
</endpoints>
```
By default, one handler for incoming messages is used.

### Configuring QoS

Endpoint also provides ability to setup a certain number of incoming messages by one access to the broker. This field specifies the prefetch window size.  When the handler finishes processing a message, the following message is already held locally, rather than needing to be sent down the channel. Prefetching gives a performance improvement.
```xml
<endpoints>
    <endpoint name="Consumer" connectionString="amqp://localhost:5672/">
        <qos prefetchCount="8" />
        <incoming />
    </endpoint>
</endpoints>
```
By default, uses the value of 50 messages, which will be read by one access. We strongly recommend starting with 1 message, increasing the number of messages after performance testing.

### Declaration of outgoing messages

All outgoing messages are declared in the 'outgoing' collection section. Each message label is declared with an individual tag 'route'. 'key' and 'label' attributes are mandatory.

Key is a label alias, which can be used from the code, and allows you to separate concrete label (exchange in RabbitMQ) of the message from its representation in code.
Outgoing route may be referenced from the application
-	by key, adding a colon (:) as a prefix.
-	directly by label.

Additionally, you can set the parameters (as attributes of the 'route' element):

 - __persist__ – broker saves the message for reliable delivery (_false_ by default);
 - __confirm__ – broker must confirm message receipt for the processing (message will be stored if persist="true", _false_ by default);
 - __ttl__ – message lifetime (unlimited, if the value is not specified);
 - __timeout__ – response time for requests (default value – 30 seconds).

To support request/reply pattern, you must declare a subscription point for awaiting response messages. At the moment you can declare only default subscription point. For each request / reply route unique incoming queue per application is created.
```xml
<outgoing>
    <route key="message" label="message.label" persist="true" ttl="00:01:00" />
    <route key="request" label="request.label" confirm="true" timeout="00:00:50">
        <callbackEndpoint default="true" />
    </route>
</outgoing>
```

### Sender’s dynamic routing

On the sender’s side you can turn on dynamic routing. It allows you to send messages with labels, which were not configured during the endpoint creation.

Dynamic routing can be useful in cases, when you don’t have enough information about all the labels at the moment of endpoint creation.
```xml
<endpoints>
    <endpoint name="point1" connectionString="amqp://localhost:5672/">
        <dynamic outgoing="true" />
    </endpoint>
</endpoints>
```

### Connection strings and fault tolerance (RabbitMQ)

An endpoint can be configured to use a list of connection strings (URLs) delimited by comma to provide some fault tolerance capabilities. The number of hosts in the connectionString property is not restricted:

```xml
<endpoints>
    <endpoint name="point1" connectionString="amqp://host-1:5672,amqp://host-2:5672,amqp://host-3:5672">
        
    </endpoint>
</endpoints>
```

Since incoming (and outgoing) connection strings can override the endpoint setting, this also applies to the fault tolerant configuration:

```xml
<endpoints>
    <endpoint name="point1">
        <incoming>
            <on connectionString="amqp://host-1:5672,amqp://host-2:5672,amqp://host-3:5672"/>
        </incoming>
    </endpoint>
</endpoints>
```

In both cases the endpoint will listen for incoming messages at each URL which can be useful when message load is split among a number of brokers.

If an endpoint is configured to send messages a specific algorithm will be used to select a URL for each particular message being sent. This capability enables load distrubution and can be utilized in flow sharding scenarios.

*Only Round Robin algorithm is implemented in this version.*

Sending operations can fail if a certain broker specifed in the connection string becomes unavailable or unstable for some reason. To mitigate this condition an endpoint will apply a retry algorithm with delays: when a failure occurres an endpoint incrementally updates a delay set for the failed broker and switches to another one. Next time this broker is chosen a new delay will be in effect.

The broker delays are stored for a certain period of time after which all delays are reset.

*These following defaults apply: the maximum delay value is limited by 30 seconds; 7 attempts will be made to fulfill the send operation; delays are reset after 120 seconds of sender inactivity.*

### Applying configuration

[AppConfigConfigurator](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/Configurator/AppConfigConfigurator.cs) class is used for applying xml-configuration.

Class constructor can take an object, implementing [IDependencyResolver](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/Configurator/IDependencyResolver.cs) interface, which is used for getting bus client dependencies. It is used for searching message handlers, validators or specifying life cycle handler. If concrete client configuration doesn’t require external dependencies, this parameter can be omitted. For simplicity, instead of implementing a particular class, you can pass to the constructor  delegate of [DependencyResolverFunc](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/Configurator/LambdaDependencyResolver.cs) type.

For example, typical creation of [AppConfigConfigurator](https://github.com/SDVentures/Contour/blob/master/Sources/Contour/Configurator/AppConfigConfigurator.cs) object using Ninject container:
```csharp
var configurator = new AppConfigConfigurator((name, type) => kernel.Get(type, name));
```

To apply configuration ‘Configure’ method is used, which must be invoked during the bus instance creation.
```csharp
var bus = new BusFactory().Create(cfg =>
{
    configurator.Configure("point1", cfg);
});
```

Using this method you can combine configuration via code and configuration file.

If endpoints names are unknown (or should not be known) at compile time, they can be accessed through the _Endpoints_ property. Example of creating and configuring all bus clients:

```csharp
var busFactory = new BusFactory();
var busInstances = configurator
                    .Endpoints
                    .Select(e => busFactory.Create(cfg => configurator.Configure(e, cfg)))
                    .ToList();
```

## Configuring via C# code

Endpoint can be completely configured via code except incoming message handler lifestyle.

To use dynamic routing set message label to  _MessageLabel.Any_ value.

```csharp
IBus bus1 = new BusFactory().Create(
    cfg =>
        {
            cfg.UseRabbitMq();
            cfg.SetEndpoint("point1");
            cfg.SetConnectionString("amqp://localhost:5672/ ");
            cfg.HandleLifecycleWith(new SpecialLifecycleHandler());
            cfg.RegisterValidator(new NameValidator());
            cfg.EnableCaching();
            cfg.UseParallelismLevel(4);
            cfg.SetDefaultQoS(8);
            cfg.Route("out.message.label")
                .Persistently()
                .WithTtl(new TimeSpan(0, 1, 0));
            cfg.Route("out.request.label")
                .WithRequestTimeout(new TimeSpan(0, 0, 50))
                .WithConfirmation();
            cfg.On("in.message.label")
                .ReactWith(MessageHandler());
            cfg.On<RequestPayload>("in.request.label")
                .ReactWith(RequestHandler())
                .WhenVerifiedBy(new InputValidator())
                .RequiresAccept();
            cfg.On<RequestPayload>("in.lazy.label")
                .ReactWith(LazyHandler());
        });

IBus bus2 = new BusFactory().Create(
    cfg =>
        {
            cfg.UseRabbitMq();
            cfg.SetEndpoint("point2");
            cfg.SetConnectionString("amqp://localhost:5672/");
        });

```

## Contour message headers
List of used headers is represented in table below.

For message tracking in chain of  application components interaction through service bus several incoming messages headers are copied in the outgoing messages.

Header field name | Description | Copying
----------------- | ----------- |--------
x-correlation-id | The correlation identifier is used to track a set of messages as a single group. It is used to match reply message with the request. New value for header is generated for every request (if not supplied in headers parameter).| Only for one-way messages.
x-expires | Header, which contains the rules of data deterioration. For example: x-expires: at 2016-04-01T22:00:33Z or x-expires: in 100 | No
x-message-type | Message label with which it was sent. Using this header is not recommended | No
x-persist | Marks message persistance (saved on disk) or not | No
x-reply-route | Reply message address for the request. For example, x-reply-route: direct:///amq.gen-n9DsUj1qm4vgCq0MHHPoBQ | No
x-timeout | Response timeout for the request | No
x-ttl | Message TTL | No
x-breadcrumbs | List of all endpoints, through which message has passed, separated by semicolon (;) | Yes (appending new value)
x-original-message-id | Message identifier, that started the messages exchange | Yes

## Channels and pipes in Contour

In Contour you can organize message processing as a set of sequential atomic message transformations based on Pipes and Filters template.

For example, you need to forward messages, that has field 'Tick' and its value is odd. Other messages should be filtered. It can be done with the following set of filters:

```csharp
var configurator = new AppConfigConfigurator((name, type) => kernel.Get(type, name));
configuration.On("document.systemtick")
    .ReactWith(new PipelineConsumerOf<ExpandoObject>(
        new IMessageOperator[]
            {
                new JsonPathFilter("Tick"),
                new Filter(request => ((dynamic)request.Payload).Tick % 2 == 1),
                new StaticRouter("document.oddsystemtick")
            }));
```
Most of the filters (messages transformers) are universal and can be used for processing different types of messages.

### Contour filters

Contour comprises the following set of the universal message filters:
 - Acceptor,
 - ContentBasedRouter,
 - Filter,
 - JsonPathFilter,
 - RecipientList,
 - Reply,
 - Splitter,
 - StaticRouter,
 - Translator,
 - TransparentReply,
 - WireTap.

### Acceptor

Confirms the message processing and forwards it to the next filter.

### ContentBasedRouter

![Content Based Router](https://cloud.githubusercontent.com/assets/888475/19405109/c2778666-927c-11e6-80ae-7b47f2f04bce.png)

Routes each message to the correct recipient based on a message content ([template in EIP](http://www.enterpriseintegrationpatterns.com/patterns/messaging/ContentBasedRouter.html)).

Behavior is defined by the signature:
```csharp
public ContentBasedRouter(Func<IMessage, MessageLabel> routeResolverFunc)
```

### Filter
![Filter](https://cloud.githubusercontent.com/assets/888475/19405163/4d10f456-927d-11e6-8389-68ec1b27fae4.png)

Filters messages by the predicate ([template in EIP](http://www.enterpriseintegrationpatterns.com/patterns/messaging/Filter.html)).

Behavior is defined by the signature:
```csharp
public Filter(Predicate<IMessage> predicateFunc)
```

### JsonPathFilter

Filter checks node availability at the specified JSONPath.

Behavior is defined by the signature:
```csharp
public JsonPathFilter(string jsonPath)
```

### RecipientList

![Recipient List](https://cloud.githubusercontent.com/assets/888475/19405190/9cbe96fc-927d-11e6-9f56-bb5e9a35428c.png)

Inspects an incoming message, determines the list of desired recipients, and forwards the message to all channels associated with the recipients in the list. ([template in EIP](http://www.enterpriseintegrationpatterns.com/patterns/messaging/RecipientList.html)).

Behavior is defined by the signature:
```csharp
public RecipientList(Func<IMessage, MessageLabel[]> determineRecipientList)
```

### Reply

Sends an incoming message as a reply and breaks the message flow. It is terminal filter in the chain.

### Splitter

![Splitter](https://cloud.githubusercontent.com/assets/888475/19405216/d3f6d788-927d-11e6-9a9a-12061093a2e0.png)

Break out the composite message into a series of individual messages, each of them contains data related to one item. ([template in EIP](http://www.enterpriseintegrationpatterns.com/patterns/messaging/Sequencer.html)).

Behavior is defined by the signature:
```csharp
public Splitter(Func<IMessage, IEnumerable<object>> splitterFunc)
```

### StaticRouter

Forwards the incoming messages to the specified recipient.

Behavior is defined by the signature:
```csharp
public StaticRouter(string label)
```

### Translator

![Translator](https://cloud.githubusercontent.com/assets/888475/19405249/22aee5dc-927e-11e6-9eac-1b2e3b32b467.png)

Translate one data format into another ([template in EIP](http://www.enterpriseintegrationpatterns.com/patterns/messaging/MessageTranslator.html)).

Behavior is defined by the signature:
```csharp
public Translator(Func<IMessage, object> translationFunc)
```

### TransparentReply

Sends incoming message as a reply and forwards it to the incoming flow.

### WireTap

![wiretap](https://cloud.githubusercontent.com/assets/888475/19405265/57b0802e-927e-11e6-9a97-f82e886b8ac6.png)

A special case of RecipientList, which allows you to listen to the channel ([template in EIP](http://www.enterpriseintegrationpatterns.com/patterns/messaging/WireTap.html)).

Behavior is defined by the signature:
```csharp
public WireTap(MessageLabel messageLabel)
```

### Features

1. All outgoing messages of the last operation are published to the bus.
2. Synchronous messages processing.
3. Broker is not used to transmit messages between operators.
4. Operations that produce multiple messages per message, publish them in the same outgoing channel.
5. Operations are executed in the same address space and can affect each other. It is not recommended to use Reflection and mutable objects.

## Build the project

 - clone the repository
 - run "build.cmd" to make sure all unit tests are passing.
 - run "build.cmd RunAllTests" to make sure all integration and unit tests are passing. In this case you have to configure access to the RabbitMQ broker.

## Library license

The library is available under MIT. For more information see the [License file][1] in the GitHub repository.

 [1]: https://github.com/SDVentures/Contour/blob/master/LICENSE.md
