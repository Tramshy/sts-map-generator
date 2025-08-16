# Slay the Spire-like Map Generator
This package allows developers to create engaging, procedurally generated maps similar to those found in the game Slay the Spire in Unity.

The basic generation logic was created by [Roican](https://github.com/Roican), then modified by me.

## Features
* Easily created StS-like maps
* Full control over map layout using `MapConfig`
* Fully functioning seed system
* Easy use and setup

## Installation
This repository is installed as a package for Unity.
1. `Open Window` > `Package Manager`.
2. Click `+`.
3. Select Add Package from git URL.
4. Paste `https://github.com/Tramshy/sts-map-generator.git`.
5. Click Add.

NOTE: To do this you need Git installed on your computer.

## Editor Usage
### Creating POIs
In order to use this package, you must first create a new script deriving from `PointOfInterest`. Then you must create a few point of interest (`POI`) prefabs. A `POI` is a UI element with an `Image` component and a component deriving the script `PointOfInterest`.

### MapConfig Fields
You must also create a new `MapConfig` scriptable object, found under "StS Map Generation/Map Config" of the create asset menu. This new scriptable object will let you fully control map generation using the following settings:
* LayerLayout
  - An array of the serializable class `MapLayer`. This array allows you to define the length of the map (for example: 5 elements for a length of 5) as well as the layout of each layer on the map.
* PrefabsForLayerTypes
  - An array of the serializable class `POIPrefabs`. This array defines what POI's can spawn on the map. The array size matches the available `LayerTypes`.
* ChancePathMiddle
  - The chance for a path to spawn in the middle.
* ChancePathSide
  - The chance for a path to spawn to the side.
* MultiplicativeSpaceBetweenLines
  - The space between path lines.
* MultiplicativeNumberOfMinimumConnections
  - The multiplicative minimum allowed connection for a map. This value is multiplied by the map length to determine the minimum number of connections.
* MaxWidth
  -  The max expected nodes per layer.
* CustomNodeTypes
  - An array of string that allows you to easily add custom layer types. Simply add an element and give it a name, you will then find the new custom layer type under PrefabsForLayerTypes.
* AllowPathCrossing

### MapLayer Fields
* LayerType
  - Determines the nodes that spawn on this layer.
* RandomizePosition
  - Multiplier determining how much randomness to add to the position of each node.
* UseRandomRange
  - Determines whether or not to use a random range when spawning nodes for this layer.
* _amountOfNodes
  - If `UseRandomRange` is false, this value will determine how many nodes to spawn. Set to -1 for total randomness.
* _randomNodeRange
  - If `UseRandomRange` is true, this value will determine the random range.
* NodeID
  - A string determining which custom layer type for this layer to use. This field is set using a dropdown menu.
* YPaddingFromPreviousLayer

### POIPrefabs Fields
* PointsOfInterestForThisLayerType
  - The prefabs to spawn for this layer type.

### Creating Base Setup
Navigate to `Tools/StS-Like Generation/Create Basic Setup for Scene` in the top menu. This will create a basic setup for you to adjust. You must create a UI `EventSystem` for this to work. It is also recommended to set the scene's main camera to the `Render Camera` of the `Map Canvas`. 

## Runtime Usage
### PointOfInterest Class
This class handles things like hovering over (using the `OnHoverEnter` and `OnHoverExit` methods) and selecting a node (using the abstract `OnNodeEnter` method).

The `Awake` method is being used by the class to initialize some values. If the `Awake` method is needed, call `base.Awake` in the override method, unless you want to handle initialization yourself. The `OnDisable` method is also already in use, depending on if `DOTween` is installed for a project, and can be overridden in the same way.

#### Methods
* SetAvailability
  - Determines whether or not the node is available to be selected. By default also changes node color based on availability; however, this can be overridden. If you do override, make sure to set the `isAvailable` field properly.
* OnHoverEnter & OnHoverExit
  - Determines behavior when pointer enters and exits the node. By default, it changes scale. Can be overridden.
* OnNodeEnter
  - Determines behavior when node is clicked while available. Is `abstract` and must be set by the user.

#### Fields
* rnd
  - Field of type `System.Random`, created based on `subSeed`. Use this for ALL randomness in a node. This will ensure that all random generation is the same on a per-seed basis.
* deactivatedColor & activatedColor
  - Colors used for the basic animation if a node is available. Can be completely ignored if the `SetAvailability` method has been overridden and now does not use them.
* FloorIndex
  - The layer the node spawned on.
* subSeed
  - The seed created by the master seed during map generation.
* Weight
  - A lower value means lower chance for this node to spawn during map generation.
* isAvailable
  - A bool describing if a node is available. Set in `SetAvailability` methods and used in default `OnHoverEnter` and `OnHoverExit` implementation, as well as in `OnPointerClick` to determine whether or not to let calls pass through. Be sure to set this properly if `SetAvailability` is overridden.
* thisImage

### MapPlayerTracker Class
This class handles the player's position on the map. This class calls `SetAvailability` for all relevant nodes. This class is a `Singleton`

#### Methods
* OnNewMapGenerated
  - Called from `_onMapGenerated` `UnityEvent` from `MapGeneration`
* UpdateCurrentPOI
  - Moves the player to a new POI. By default, updates the availability of the previous floor before moving the player up to the next node. By default also changes the selected POI's `deactivatedColor` to white to create a line of white POI's along the path. Can be overridden.

## License
This package is licensed under the MIT License. For more information read: `LICENSE`.

## Additional Notes About Dependency
The base implementation of `SetAvailability` uses `DOTween` for color and scale changing. However, this package is not dependent on `DOTween`. The package will automatically detect whether you have `DOTween` imported for the project and act accordingly.
