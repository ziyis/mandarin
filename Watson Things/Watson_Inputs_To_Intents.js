
var prompt = require('prompt-sync')();
var AssistantV1 = require('watson-developer-cloud/assistant/v1');
var amqp = require('amqplib/callback_api');
var util = require('util');
var workspace_id = '1f521498-79fc-4688-89fc-2b792769008e'; // replace with workspace ID
var exchange_send = 'send_to_unity';
var exchange_recieve = 'send_to_backend';

var connection_url = 'amqp://guest:guest@localhost/mandarin_project';

var service = new AssistantV1({
  username: 'e3d0e255-3f04-4a97-a269-ec7af0364113', // replace with service username
  password: 'GviSuuyKJTXY', // replace with service password
  version: '2018-02-16'
});


//Create a connection that waits for inputs to be passed; when passed it calls processResponse.
amqp.connect(connection_url, function(err, conn) {
  conn.createChannel(function(err, ch) {

    ch.assertQueue(exchange_recieve, {durable: false});
    console.log(" [*] Waiting for messages in %s. To exit press CTRL+C", exchange_recieve);
	
    ch.consume(exchange_recieve, function(msg) {
      console.log(" [x] Received %s", msg.content.toString());
	  msg_content = msg.content.toString();
	  
	service.message({
	  workspace_id: workspace_id,
	  }, processResponse);

	  
	}, {noAck: true});
  });
});

//Get the inputs and wait until Watson returns something. Then handle them.
function processResponse(err, response) {
  if (err) {
	console.error(err); // something went wrong
	return;
  }

  
  var processConversation = true;
  var returnedNull = true;
  
  if(response.input.text != undefined)
  {
	console.log(" [x] Input was " + response.input.text);
	processConversation = false;
  }
  if (response.output.text.length != 0) {
    console.log(' [x] Detected output: #' + response.output.text);
	  returnedNull = false;
  }

  if (response.entities.length > 0) {
	console.log(' [x] Detected entities: #' + response.entities[0].entity);
  }

  //Wait for Watson to return output.
  if (processConversation) {
    //var msg_content = prompt('>> ');
  	service.message({
  	workspace_id: workspace_id,
  	input: { text: msg_content },
  	context : response.context,
  	}, processResponse)
  }
  //If we got output, and it did not return null pass the intents.
  
  else if (!returnedNull)
  {
	amqp.connect(connection_url, function(err, conn) {
	  conn.createChannel(function(err, ch) {  
	    ch.assertExchange(exchange_send, 'topic');
	    ch.publish(exchange_send, '', new Buffer(JSON.stringify(response.output.text)));
      });
	  setTimeout(function() { conn.close(); process.exit(0) }, 111500);
    });
	  console.log(" [o] Sent output : " + response.output.text);
  }

}
	