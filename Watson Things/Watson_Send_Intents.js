// Example 1: sets up service wrapper, sends initial message, and 
// receives response.

var prompt = require('prompt-sync')();
var AssistantV1 = require('watson-developer-cloud/assistant/v1');

// Set up Assistant service wrapper.
var service = new AssistantV1({
  username: 'e3d0e255-3f04-4a97-a269-ec7af0364113', // replace with service username
  password: 'GviSuuyKJTXY', // replace with service password
  version: '2018-02-16'
});

var workspace_id = '1f521498-79fc-4688-89fc-2b792769008e'; // replace with workspace ID

// Start conversation with empty message.
service.message({
  workspace_id: workspace_id
  }, processResponse);


// Process the service response.
function processResponse(err, response) {
  if (err) {
    console.error(err); // something went wrong
    return;
  }

  console.log(JSON.stringify(response, null, 2));

  var endConversation = false;
  // If an intent was detected, log it out to the console.
  if (response.intents.length > 0) {
    console.log('Detected intent: #' + response.intents[0].intent);
    if (response.intents[0].intent === 'stopApp') {
      // User said goodbye, so we're done.
      endConversation = true;
    } 
  }

  if (response.entities.length > 0) {
    console.log('Detected entities: #' + response.entities[0].entity);
  }
  // Check for action flags.
  // Display the output from dialog, if any.
  if (response.output.text.length != 0) {
      for (var i=0; i<response.output.text.length; ++i){
        console.log(response.output.text[i]);
      } 
  }
  

  // If we're not done, prompt for the next round of input.
  if (!endConversation) {
    var newMessageFromUser = prompt('>> ');
    service.message({
      workspace_id: workspace_id,
      input: { text: newMessageFromUser },
      // Send back the context to maintain state.
      context : response.context,
    }, processResponse)
  }
}