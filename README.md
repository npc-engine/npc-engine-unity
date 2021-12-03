# npc-engine Unity integration

This is Unity integration package for NPC-Engine.  
[NPC-Engine](https://github.com/npc-engine/npc-engine) is a deep learning inference engine for designing NPC AI with natural language.


## Features

- Chat-bot dialogue system.
- SoTA tools like text semantic similarity and text to speech.
- Easy, open source deep learning model standard (ONNX with YAML configs).
- GPU accelerated inference with onnxruntime.

## Dependencies

- Welcome window depends on [EditorCoroutines unity package](https://docs.unity3d.com/Packages/com.unity.editorcoroutines@1.0/manual/index.html).  
    You can add this line to your Packages\manifest.json:
    ```
    {
        "dependencies": {
            ...
            "com.unity.editorcoroutines": "1.0.0",
            ...
        }
    }
    
- Advanced demo scene requires these free asset store packages:
  - [VIDE dialogues](https://assetstore.unity.com/packages/tools/ai/vide-dialogues-69932)
  - [Modular First Person Controller](https://assetstore.unity.com/packages/3d/characters/modular-first-person-controller-189884)
  - [Low Poly Modular Armours](https://assetstore.unity.com/packages/3d/characters/lowpoly-modular-armors-free-pack-199890)
  - [RPG Poly Pack - Lite](https://assetstore.unity.com/packages/3d/environments/landscapes/rpg-poly-pack-lite-148410)

## Getting started

- Clone this repository
- Install dependencies
- Move Assets folder to your Unity project.
- Follow welcome window instructions and read [Documentation](https://npc-engine.github.io/npc-engine/)