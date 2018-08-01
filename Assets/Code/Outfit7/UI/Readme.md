# UI lib

### Known issues
- UIMaker for `CanvasUiMakerComponent`, `DialogUiMakerComponent` and `ImageUiMakerComponent`(the part for `AtlasImage`) doesn't work
- UIMaker for ProgressBar still needs to be done
- `AbstractActionButton` needs an Editor revamp
- Unity 5.3 sprite not showing sprite images (possible fix: make better sprite picker for `AtlasImage` and `RichText`)
- `FiniteLogic` on `InfiniteScrollView` doesn't work with Snapping and animated centering
- An event should be called from ImageImporter (postprocessor) which would update the `CanvasAtlases` on any atlas texture changed

---

### v1.1.20 - 18.8.2016

#### Change log

##### Added

- Added `BlockTouchesAfterOnCellPerSwipe` which is true by default (for News) in `InfiniteScrollViewController`, when false it can be used with `OneCellPerSwipe` for Albums 

---

### v1.1.19 - 27.7.2016

#### Change log

##### Added

- Added `OnDrawGizmosSelected` for `TouchRectTransform`

---

### v1.1.18 - 8.7.2016

#### Change log

##### Added

- Added `OnTextureDownloaded` event for `WebImageManager`s `Preload` method
- Merged in `CursorController` from TTGR branch

---

### v1.1.17 - 1.7.2016

#### Change log

##### Fixed

- Fixed `InfiniteScrollViewController` data < containers - should only apply to infinite logic

---

### v1.1.16 - 30.6.2016

#### Change log

##### Fixed

- Fixed `InfiniteScrollViewController` crash if data < containers and also dragging was enabled

---

### v1.1.15 - 21.6.2016

#### Change log

##### Added

- Added `OnTextureErrorLoading` actions in `WebImageLoader` and `WebImageManager`

##### Modified

- Improvements in `WebImageLoader` and `WebImageManager`

---

### v1.1.14 - 16.6.2016

#### Change log

##### Modified

- Modified `TouchEventSystemEditor` now inherits from `EvenySystemEditor` which adds back `OnInspectorPreview` functionality (text on the bottom of inspector)

---

### v1.1.13 - 16.6.2016

#### Change log

##### Fixed

- Fixed `ProgressBarController` > 1 value not being clamped

---

### v1.1.12 - 7.6.2016

#### Change log

##### Fixed

- Fixed `RichText` recursive `SetVerticesDirty` calls producing crashes

---

### v1.1.11 - 6.6.2016

#### Change log

##### Fixed

- Fixed possible runtime crash in `OnDisable` in `RichText`

---

### v1.1.10 - 31.5.2016

#### Change log

##### Fixed

- Fixed possible runtime crash in `PositionIcons` in `RichText`

---

### v1.1.9 - 30.5.2016

#### Change log

##### Added

- Added `CallOnRectTransformDimensionsChange` property in `AtlasImage`

##### Fixed

- Fixed possible runtime crash in `OnPopulateMesh` in `RichText`
- Fixed a bug that yelds "Camera not set" in `CanvasUiMakerComponent` when adding a sub`Canvas` in scene

##### Modified

- Modified the layout and fixed a crash bug for `AtlasImageEditor`

---

### v1.1.8 - 26.5.2016

#### Change log

##### Fixed

- Fixed `IsBusy` not taking into account CenterOnLerp variable in `InfiniteScrollViewController`

---

### v1.1.7 - 26.5.2016

#### Change log

##### Added

- Added `SetNativeSize()` call when creating `AtlasImage` or `RawImage` with UI Maker
- Added total size of atlases in `CanvasAtlas`

---

### v1.1.6 - 25.5.2016

#### Change log

##### Fixed

- Fixed `CanvasAtlas` materials and textures not being synced when adding atlas

---

### v1.1.5 - 25.5.2016

#### Change log

##### Fixed

- Removed UIMaker warnings
- Fixed `CanvasAtlas` materials sometimes being null in editor and consequently coudn't add an atlas to it
- UIMaker now closes when opened through the context menu and Create is pressed

---

### v1.1.4 - 25.5.2016

#### Change log

##### Added

- Added the ability to disable `OnRectTransformDimensionsChange` on `AtlasImage`

---

### v1.1.3 - 23.5.2016

#### Change log

##### Added

- Added `NoDeactivations` bool which does what it says - SetActive is never called, only SetParent(null, false) is called -> never rendered outside of canvas

---

### v1.1.2 - 20.5.2016

#### Change log

##### Fixed

- Fixed default namespace to search for `DialogActionButton` and `GameActionButton`
- Fixed `ProgressBarController` using a shared `Material`

---

### v1.1.1 - 20.5.2016

#### Change log

##### Fixed

- Fixed `Update` method in `ProgressBarController` to be overridable if child class has an `Update` method too

---

### v1.1.0 - 20.5.2016

#### Change log

##### Added

- Added context menu of all UIMaker components
- Added a common `ProgressBar` shader and controller script
- ButtonUiMakerComponent now has data where you can specify `DialogActionButton`s and `GameActionButton`s namespaces

---

### v1.0.21 - 12.5.2016

#### Change log

##### Added

- Added `AnimatorEditorUtils` with which you can make stats in `Animator` and then run the script in context menu and it will generate `AnimationClip`s under `Animator` in project view. Used only for Buttons otherwise it is strongly suggested to use ASM.

##### Fixed

- Fixed error when saving textures in `WebImageManager`

---

### v1.0.20 - 10.5.2016

#### Change log

##### Fixed

- Fixed build error on Unity 5.3 in Glow.cs

---

### v1.0.19 - 10.5.2016

#### Change log

##### Added

- Added shader for gradient `UI-Default-Gradient`

---

### v1.0.18 - 10.5.2016

#### Change log

##### Fixed

- Fixed a minor bug where `StopLerping` method was called on `OnBeginDrag` where it shouldn't because dragging wasn't enabled

---

### v1.0.17 - 9.5.2016

#### Change log

##### Fixed

- Removed warnings in `Glow`
- Fixed some `InfiniteScrollViewController` bugs with single cell dragging and drag went through although dragging was disabled
- Fixed `InfiniteScrollViewController` GetComponents LayoutElement getting all child instances instead only on cells

---

### v1.0.16 - 22.4.2016

#### Change log

##### Added

- `ScrollViewController`s `Next` and `Previous` methods now return int - which is the next/previous cell/page index

---

### v1.0.15 - 18.4.2016

#### Change log

##### Fixed

- Renamed `ArabicFixer` method call to `FixArabic`
- Improved `one cell per swipe` for `InfiniteScrollViewController`

---

### v1.0.14 - 15.4.2016

#### Change log

##### Fixed

- Fixed Arabic localization in `LocalizerEditor` not showing properly - !!!MUST BE UP-TO-DATE with Common lib!!!

---

### v1.0.13 - 15.4.2016

#### Change log

##### Added

- Added `SetDragEnabled` method to `InfiniteScrollViewController` which enables/disables only dragging - works if set before `RefreshView` is called

---

### v1.0.12 - 12.4.2016

#### Change log

##### Fixes

- Fixed a wrong condition in `InfiniteScrollViewController` with `FiniteLogic` turned on which didn't set the `BlockToNext` on `InfiniteScrollRect` resulting in going into infinite mode in some cases

---

### v1.0.11 - 12.4.2016

#### Change log

##### Fixes

- Added virtual method `OnSpriteInfoChanged` which is called when changing sprites or characters and it resets previous value of `AtlasInt` and `AtlasFloat` so the same number can be set again

---

### v1.0.10 - 11.4.2016

#### Change log

##### Added

- Added `AtlasText` for setting text characters with `AtlasImage`s with its Editor
- Added `AtlasInt` for setting number characters with `AtlasImage`s  with its Editor
- Added `AtlasFloat` for setting float number characters with `AtlasImage`s  with its Editor

##### Fixed

- Fixed `CenterOn` on a `InfiniteScrollViewController` with `FiniteLogic` behaviour
- Fixed `AtlasImage` not showing null texture as "---" as selected in the inspector enum field
- Fixed `Localizer` not showing the content options if a string was found but there were more options than one

---

### v1.0.9 - 1.4.2016

#### Change log

##### Added

- Added `ModifiedScale` private variable settable through `SetModifiedScale` method which tells the `InfiniteScrollRect` that the content children are scaling

##### Fixed

- Fixed `InfiniteScrollRect` not working as expected if the content children were scaling e.g. with `CenterDistanceResizer` the bug was visible on age gates

---

### v1.0.8 - 29.3.2016

#### Change log

##### Fixed

- Fixed `AtlasImage` not fully supporting custom materials

---

### v1.0.7 - 25.3.2016

#### Change log

##### Removed

- Removed `CanvasAtlas` obsolete methods

##### Modified

- Optimized `RichText` `OnPopulateMesh` method when not parsing images (`ParseImages = false`)

---

### v1.0.6 - 25.3.2016

#### Change log

##### Added

- Added `int occurance = 0` parameter to SetSpriteAndMaterialWithName (`AtlasImage`) and GetSpriteAndMaterial(`CanvasAtlas`) which has to be specified if there is more than one occurance of a specific sprite (name)

---

### v1.0.5 - 24.3.2016

#### Change log

##### Fixed

- Fixed `text` property in `RichText` to be overriden instead of new - fixes the problems like `Localizer` not working with `RichText`

---

### v1.0.4 - 24.3.2016

#### Change log

##### Modified

- Renamed `AbstractScrollViewController` methods used only for editor to have Editor suffix (`SetScrollRectEditor`, `SetContentEditor`, `SetScrollViewRectTransformEditor`)
- Renamed `ShouldRefreshEditor` into `RefreshEditor` in `CanvasAtlas` which now instantly refreshes the atlas and all `AtlasImage`s
- Modified the calculation of the content position change in `InfiniteScrollRect`

##### Added

- Implemented `ForceUpdateCells` method in `InfiniteScrollViewController`
- Added check if application isPlaying in method `LateUpdate` in class `InfiniteScrollRect` method which code is being called after pressing stop
- `CenterOnCell` in `InfiniteScrollViewController` now supports `FiniteLogic`

---

### v1.0.3 - 17.3.2016

#### Change log

##### Fixed

- Fixed `RichText` didn't compile on Unity prior to 5.3 but it is not available there

---

### v1.0.2 - 16.3.2016

##### Change log

##### Added

- Added `FiniteLogic`for `InfiniteScrollView` which stops the scrollview on begining and end

##### Fixed

- Fixed `ScrollView` `Snap` not working correctly with vertical scrollview
- Fixed CanvasAtlas not refreshing OnEnable() in CanvasAtlasEditor -> on editor click

---

### v1.0.1 - 15.3.2016

#### Change log

##### Added

- Added `ClampVelocityValue` serialized variable for `InfiniteScrollViewController` with the default value 0 which means clamp disabled (e.g for AgeGate scrollview is currently set to 1500). The value is used on TouchUp when scrolling

---

### v1.0.0 - 15.3.2016

#### Change log

##### Deprecated

- `AlphaBlend`, `Opaque`, `Additive`, `Custom1` and `Custom2` values are now deprecated in `CanvasAtlas` and `AtlasImage` and a lot of methods and properties that were connected with the mentioned values

##### Modified

- All atlas sprites in `CanvasAtlas` are now in one list
- `ExecuteInEditMode` and the content of `OnEnable` method is temporarily set on `CanvasAtlas` to switch from the old version of the class to the new one
- `CanvasAtlasEditor` doesn't show the sprites anymore (nobody needed them anyways) but shows a nice list with the size of the atlas (e.g. 1024x1024)
- Optimized `RichText` parsing for `AtlasImage`s

##### Added

- Implemented a lot of sprite and material setting methods in `CanvasAtlas`

---
