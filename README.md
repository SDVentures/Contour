Contour
========

![Contour status](https://ci.appveyor.com/api/projects/status/github/sdventures/contour?branch=master&svg=true)
[![NuGet Status](http://img.shields.io/nuget/v/Contour.svg?style=flat)](https://www.nuget.org/packages/Contour/)
[![Nuget](https://img.shields.io/nuget/dt/Contour.svg)](http://nuget.org/packages/Contour)

## Documentation

### About

Contour is the message bus implementation to provide communication .NET services via different transport protocols.
There is support AMQP/RabbitMQ transport now.

### Quick start

Create a new endpoint to emit message:

	var bus = new BusFactory().Create(cfg => // create an endpoint of message bus
	{
	    cfg.UseRabbitMq(); // specify transport
	    cfg.SetEndpoint("HelloWorld.Emitter"); // specify an unique endpoint name
	    cfg.SetConnectionString("amqp://"); // specify a connection string to a broker
	    cfg.Route("greeting"); // declare outgoing message route
	});

Send a message by one-way channel:

	bus.Emit("greeting", new { Text = "Hello World!" }); // send message to the service bus

Create a new endpoint to receive messages:

	var bus = new BusFactory().Create(cfg => // create an endpoint of message bus
	{
	    cfg.UseRabbitMq(); // specify transport
	    cfg.SetEndpoint("HelloWorld.Consumer"); // specify an unique endpoint name
	    cfg.SetConnectionString("amqp://"); // specify a connection string to a broker
            cfg.On<Message>( // register message handler for specific message route
                "greeting",
		(Message message) =>
		{
		    Console.WriteLine(message.Text);
		});
	});

And declare incoming message type:

	class Message
	{
	    public string Text { get; set; }
	}

### Build the project

 - clone the repository
 - run "build.cmd" to make sure all unit tests are still passing.
 - run "build.cmd RunAllTests" to make sure all integration and unit tests are still passing. In this case you have to configure access to the RabbitMQ broker.

## Library license

The library is available under MIT. For more information see the [License file][1] in the GitHub repository.

 [1]: https://github.com/SDVentures/Contour/blob/master/LICENSE.md
