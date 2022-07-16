# npc-engine Unity integration

This is Unity integration package for NPC-Engine.  
[NPC-Engine](https://github.com/npc-engine/npc-engine) is a deep learning inference engine for designing NPC AI with natural language.


## Features

- Chat-bot dialogue system.
- SoTA tools like text semantic similarity and text to speech.
- Easy, open source deep learning model standard (ONNX with YAML configs).
- GPU accelerated inference with onnxruntime.

## Getting started

- Grab the [integration](https://assetstore.unity.com/packages/tools/ai/npc-engine-208498) from Asset Store
- Follow welcome window instructions or do a [manual setup](#manual-setup).
- See tutorials and [API docs](https://npc-engine.github.io/npc-engine-unity/api/NPCEngine.html).

## Tutorials

- [Basic Demo](https://npc-engine.github.io/npc-engine-unity/tutorials/basic_demo.html) tutorial to see the basic usage of the NPC-engine API
- [Advanced Demo](https://npc-engine.github.io/npc-engine-unity/tutorials/advanced_demo.html) to understand how higher-level components work and how to integrate NPC Engine into your game.
- [Scene Setup Tutorial](https://npc-engine.github.io/npc-engine-unity/tutorials/scene_setup.html) to learn how to setup your scene from the ground up.

## Manual setup

This integration package is just a wrapper around python server that actually does the heavy lifting.
These steps are usually done automatically by the welcome dialogue buttons from the unity editor, but they can be done manually if required:

- Download the latest release of [npc-engine](https://github.com/npc-engine/npc-engine/releases)
- Unzip it into the `Assets/StreamingAssets/.npc-engine` folder of your Unity project.
- Create an empty `Assets/StreamingAssets/.models` folder
- Run cmd command from your project folder to download default models:
```
Assets/StreamingAssets/.npc-engine/npc-engine.exe download-default-models --models-path Assets/StreamingAssets/.models
```

## Community

We now have a [Discord server](https://discord.gg/R4zBNmnfrU) :) 