This tutorial explains raw usage of the NPC Engine API from Unity using Basic Demo scene.

## Scene Overview

First lets go through and play around with the basic demo scene.  
Its located under this path: `NPCEngine/Demo/BasicDemo/Basic.unity`

When you start it the first thing you'll see is this screen:
![First thing seen when starting basic demo scene](../resources/basic_scene_first.png)

Since NPC Engine is a server that starts alongside unity and it's startup takes some time you can keep it running between playtests and just connect to existing one.  

When started you should see some Unity logs regarding connecting to the server as well as server console pop up with server logs. This behaviour is debug only and can be turned off by disabling `Server Console` and `Debug Logs` flags in `NPCEngineManager` game object.
![Server console](../resources/npc_engine_console.png)

If NPC Engine starts successfully, menu options will become interactable and you will be able to play around with different APIs.

## Available API Demos

### Text To Speech Demo

This demo shows you the API that allows you to generate speech from text with multiple voices.

![Text to speech](../resources/text_to_speech.png)

### Fantasy Chatbot Demo

This demo shows you the chatbot API. It enables you to describe a fantasy character via the chatbot context and chat with your character.

Right now it's available only in the single style (Fantasy) but we are already working on the other chatbot neural networks with diffirent styles as well as tutorials how to train them yourself.

This demo greets you with a context in which you can fill in different descriptions to simulate different situations.

![Chatbot context demo](../resources/chatbot_context.png)

`Chat` button will take you to chat window where you can talk to the character defined in the context.

![Chatbot chat demo](../resources/chatbot_chat.png)

`Clear history` button will restart the dialogue.


### Semantic Similarity Demo

This demo shows the API to compare two sentences via their meaning.  
When you press `Compute Similarity` the score is shown in range of `[-1,1]`

Where -1 means that phrases are completely unrelated and 1 is that phrases are the same. Usually the most meaningful scores are in the range `[0,1]` 

![Semantic similarity demo](../resources/semantic_similarity.png)

### Speech To Text Demo

This demo shows you the API that allows you to listen to microphone input and 
transcribe it to text.

Just press `Listen` button and say something into the microphone.  
Note that it will only work in low noise environment and with slow articulate speech.

!!! note "Experimental API"
    This API is very WIP and experimental so it's performance is not yet ready
    for any production usage, you should use 
    [UnityEngine.Windows.Speech.DictationRecognizer](https://docs.unity3d.com/ScriptReference/Windows.Speech.DictationRecognizer.html) instead

![Text to speech](../resources/speech_to_text.png)

## Server Lifetime

Two most important NPC Engine classes are: 
- Script that manages NPC Engine core server `NPCEngine.Components.NPCEngineManager`.
- Script that contains configuration `NPCEngine.NPCEngineConfig`

They are attached to `NPCEngineManager` game object in the scene.  
NPC Engine core server's main purpose is to run inference services. 
When started it will start a core service called `control` that allows to control and get information about other services.
Services mentioned as `Load on start` in the configuration are started as well.

Stopped services do not take up any resources (except disk space).
Services configs will take effect on start/restart.
Each service is blocking only for its own thread and does not block other services.