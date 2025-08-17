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
* LayerID
  - A string determining which custom layer type for this layer to use. This field is set using a dropdown menu.
* YPaddingFromPreviousLayer

### POIPrefabs Fields
* PointsOfInterestForThisLayerType
  - The prefabs to spawn for this layer type.

### MapGeneration Fields
* _pathPrefab
  - The prefab for the path used between nodes on the map. The system can handle both a `LineRenderer` and `UI` approach. This means that if this prefab has a `LineRenderer` component, the path will be created using it, otherwise the system will copy this prefab several times and space out as individual path objects.
* _xPadding
  - The padding between the left and right edges of the map background in pixel units.
* The remaining fields can be ignored.

### MapContentResizer Fields
* _finalLayerHeightPadding
  - The extra padding to apply to the final layer of the map in pixel units.
* The remaining fields can be ignored.

### Creating Base Setup
Navigate to `Tools/StS-Like Generation/Create Basic Setup for Scene` in the top menu. This will create a basic setup for you to adjust. You must create a UI `EventSystem` for this to work. It is also recommended to set the scene's main camera to the `Render Camera` of the `Map Canvas`. 

## Runtime Usage
### PointOfInterest Class
This class handles things like hovering over (using the `OnHoverEnter` and `OnHoverExit` methods) and selecting a node (using the abstract `OnNodeEnter` method).

#### Methods
* Awake
  - The `Awake` method is being used by the class to initialize some values. If the `Awake` method is needed, call `base.Awake` in the override method, unless you want to handle initialization yourself. The `OnDisable` method is also already in use, depending on if `DOTween` is installed for a project, and can be overridden in the same way.
* SetAvailability
  - Determines whether or not the node is available to be selected. Simply sets the `isAvailable` field and calls `SetAvailabilityVisuals`.
* SetAvailabilityVisuals
  - By default changes node color based on `isAvailable`; however, this can be overridden.
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
* Awake
  - The `Awake` method is being used by the class to initialize some values and set up `Singleton`. If the `Awake` method is needed, call `base.Awake` in the override method, unless you want to handle initialization yourself.
* OnNewMapGenerated
  - Called from `_onMapGenerated` `UnityEvent` from `MapGeneration`
* UpdateCurrentPOI
  - Updates what nodes are selectable by the player by calling `SetAvailability` on relevant nodes. By default, calls `SetAvailability` of the previous floor to false, before moving up to the next node and calling `SetAvailability` to true for next nodes. By default, calls `UpdateCurrentPOIVisuals` right after updating `currentNode`. Can be overridden.
* UpdateCurrentPOIVisuals
  - By default changes the `currentNode`s `deactivatedColor` to white. This results in a line of white POI's along the selected path. Can be overridden.

#### Fields
* Instance
  - The `Singleton` instance of this class.
* currentNode
  - The current POI selected by the player.

## License
This package is licensed under the MIT License. For more information read: `LICENSE`.

## Additional Notes About Dependency
The base implementation of `SetAvailabilityVisuals` uses `DOTween` for color and scale changing. However, this package is not dependent on `DOTween`. The package will automatically detect whether you have `DOTween` imported for the project and act accordingly.
