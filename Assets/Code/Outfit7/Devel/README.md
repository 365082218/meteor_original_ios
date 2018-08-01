# Devel Lib

Summary:
The devel lib (`lib-devel`) contains development utilities that are useful in the Unity Editor and support automatization of several aspects, e.g. debug dialog, L10n import, scene locking, etc.

Dependencies:
* lib-common
* lib-logic

## Changes

### 2017-02-3

ADDED
* Debug dialog: support implemented for int sliders. Show slider's short or long name.

### 2017-02-02

MODIFIED
* Debug dialog: Toggle and Slider must have tag to prevent problems with TouchManager from lib-logic.

### 2017-01-23

MODIFIED
* Debug dialog: Dynamically add new dbg settigns to preexisting UI.

### 2017-01-19

MODIFIED
* Debug dialog: updated the debug menu with the menu from TTGR, added debug label image.

### 2017-01-12

ADDED
* L10nImporter: no hard-coded URL anymore (setting a callback set in the game instead).
* Debug dialog from MTH. Fixes so far: decoupling Main.s from HomeDebugSettings (we can set EmailPlugin’s reference, project name and recipient’s address form outside).

### 2017-01-11

ADDED
* SceneLocking.