# Common Lib

The common lib (`lib-common`) contains basic C# utilities and higher-level components that are useful across our Unity apps (like native communication, analytics, promo, state management, purchase, social, and user stuff) and are loosely-coupled at the same time (sort of plug-n-playable if needed).

Responsible for the common lib and its main maintainers are:
- Tine Lesjak
- Dejan Tomažič (passively)
- Matej Romih (passively)

Dependencies:
- _None_

---

## Known Issues

- The native part (common libs) has to be up-to-date for new 'plugin' method calls from Unity to native to work. Otherwise compile errors can emerge on iOS and runtime crashes on Android.

---

## API Changes

Here is the list of API changes with short **instructions** how to overcome backward incompatible API changes. Bug fixes are not listed.

### 1032 | 2017-08-28

MODIFIED
* Moved Facebook like methods (`CanShowLikeDialog`, `ShowLikeDialog` and `Liked`) from `AbstractSocialHelper` to `FacebookHelper`. Also `LikeButtonPressed` was moved from `AbstractSocialPlugin` to `FacebookPlugin` and ~~`ShowLikeDialog`~~ was removed from `AbstractSocialPlugin`. **Use concrete implementation (`FacebookHelper`) to use Facebook like functionality.**
* Moved VKontakte subscribe methods (`SubscribePurchasePack`, `WasSubscribeRewarded`, `SyncSubscribeReward`, `CanSubscribe`, `Subscribe`, `SubscribeCompleted` and `SubscribeFailed`) from `AbstractSocialHelper` to `VKontakteHelper`. Also `SubscribeCompleted` and `SubscribeFailed` were moved from `AbstractSocialPlugin` to `VKontaktePlugin`, ~~`Subscribe`~~ was removed from `AbstractSocialPlugin`. **Use concrete implementation (`VKontakteHelper`) to use VKontakte subscribe functionality. (Like feature is not possible).**

REMOVED
* Completely removed Google Analytics analytics tracker under `Analytics.Google` namespace. **Just delete everything related to Google Analytics. Update the Mini-game lib (lib-minigame)**
* Completey removed video recording (Everyplay) support including `Video.Recording` namespace. **Just delete everything related to video recording.**
* Completely removed `Audio.Repeating` namespace with SoundTouchPlugin in favor of the Talkback library (lib-talkback). **Use the updated Talkback library (lib-talkback).**
* Removed ~~`WebImageLoader`~~. **Use one from the UI library (lib-ui).**
* Removed Facebook like rewarding from `CurrencyState`. ~~`CommonTrackingEventParams.FacebookLike`~~ internal offer item ID and ~~`FacebookLikeRewarded`~~ property were removed. Also a constructor of `CurrencyState` was changed. **Just delete everything related to Facebook like rewarding.**
* Completely removed newsletter subscription support including rewarding. Removed ~~`NewsletterManager`~~ class, ~~`CommonTrackingEventParams.NewsletterRegistration`~~ internal offer item ID, ~~`NewsletterSubscriptionToken`~~ property from `AppPlugin`, ~~`NewsletterSubscribeRewarded`~~ property from `CurrencyState` and constants in `CommonEvents`.
* Removed unused ~~`IsSubscribeEnabled`~~ method from `VKontakteHelper`. **Use `CanSubscribe`.**
* Removed ~~`GpuBenchmark`~~. **Create a local copy if you need it.**
* Removed orphaned ~~`OnCreativeRetrieveSuccess`~~ method from `PromoManager`.
* Removed unused `Gui.Animation` namespace. **Replace ~~`AbstractAnimation`~~ and ~~`EmptyAnimation`~~ with any new animation principles or with `MainExecutor.PostDelayed()`.**

### 1031 | 2017-08-03

ADDED
* Introduced AppsFlyer analytics tracker under `Analytics.AppsFyler` namespace. Basically copied from My Talking Hank and Color Splash.
* Introduced basic messaging system under `Messaging` namespace. Promoted from My Talking Angela and Color Splash.
* Introduced ad providers management from Unity for testing purposes in `AdManager` and `AdPlugin` classes (for Android only). Notable: `AdType` enum, `GetAvailableAdProviders` and `SetAdProvider` methods, `AdShown` events.
* Introduced `FindSelectedSocialNetwork` protected method in `SocialNetwork` which finds the currently or last selected social network.

MODIFIED
* Added `AbstractSocialHelper` parameter to `UpdateSelectedSocialNetwork` method in `SocialNetwork`.

REMOVED
* Removed `GetAvailableInterstitialProviders` and `SetInterstitialProvider` methods from `AdPlugin`. **Use new `GetAvailableAdProviders` and `SetAdProvider` methods.**

### 1030 | 2017-07-04

ADDED
* Introduced support for grid specified allowed scene/state app transitions, where an interstitial can be shown, in `InterstitialAdManager`. Added `AllowedTransitions` public list, `UpdateTransitionsAllowingInterstitials` protected method, `DoStateNamesMatch` protected method and previous state name optional parameter in `CanShowTimeoutedInterstitialAd` method.
* Added `InterstitialPreloading` protected field in `InterstitialAdManager` to reflect if the interstitial is preloading.
* Introduced `DebugNextAdTimeout` protected field in `InterstitialAdManager` to prevent preparing interstitial on every frame while debugging.
* Added Windows support in `AnalyticsPlugin` for enabling/disabling collecting analytics data.
* Introduced `SetAnalyticsCollectionEnabledChange` protected method in `AnalyticsPlugin`.
* Introduced support for removing cross-promo add-on via `RemoveCrossPromoAddOnAppData` abstract method in `CrossPromoAddOnManager` if specified in the grid.
* Introduced `ISocialUserDownloader` interface to be implemented by the class that is responsible for downloading social users from backend. Implement it in a typical class that communicates with backend, usually a UserManager. Call `PostUpdateUsers` method in `SocialUserManager` after social users are received in a response.

MODIFIED
* Redesigned `SocialNetwork` to work with a list of social helpers. **Every social helper has to be created and inited manually outside and registered via `RegisterSocialHelper` method.**
* Made methods & properties virtual in `AbstractSocialHelper`, renamed ~~`FlurryKey`~~ property to `Id`, replaced ~~`TrackingManager`~~ with `BqTracker`.
* Renamed ~~`FlurryKey`~~ property to `Id` in `FacebookSocialHelper` and `VKontakteSocialHelper`.
* Made proper `ClearPrefs` non-static method in `FacebookSocialHelper` and `VKontakteSocialHelper`.
* Made methods & properties virtual in `AbstractSocialPlugin` and `SocialUserPersister`.
* Changed `Tag` constant from private to protected in `AbstractSocialUserUnmarshaller`. Also changed JSON field name constants from public to protected.
* Replaced ~~`AbstractSocialStateWorker`~~ with `SocialUserWorker` class removing all backend communication related code. **Use `SocialUserWorker` class instead of ~~`AbstractSocialStateWorker`~~.**
* Replaced ~~`AbstractSocialStateManager`~~ with `SocialUserManager` without backend communication related code and with a need of new `ISocialUserDownloader` interface. **Use `SocialUserManager` instead of ~~`AbstractSocialStateManager`~~ and set `SocialUserDownloader` property.**
* Changed `PrepareUrl` method in `AbstractUserStateSender` to return a `StringBuilder` instead of a `string`.
* Changed user state call query parameters to new jonified ones in `AbstractUserStateSender`. **Start using the new version of backend REST endpoint, which supports jonified names.**
* Moved checking for special offer action URL from `SpecialOffer` to `AddOnSpecialOffer`.
* Added support for searching multiple items at once in `ReferenceFinder` for Editor.

REMOVED
* Removed ~~`InvitesSentAnalyticsMetric`~~ and ~~`InvitesSentAnalyticsDimension`~~ obsolete abstract properties and deprecated ~~`Tracker`~~ from `AbstractSocialHelper`.
* Removed ~~`FacebookAppId`~~ and ~~`FacebookAppSecret`~~ unused properties from `FacebookHelper`.
* Removed ~~`VKontakteAppId`~~ unused properties from `VKontakteHelper`.
* Removed JSON field name constants from `BasicUserData`, `BasicSocialUser` and `BasicSocialData`. **Each app should define its own constants, because they are bound to the backend user entity for this particular app only forever.**

### 1029 | 2017-04-14

ADDED
* Introduced `SetTimeZoneOffset` and `SetSessionId` methods in `IBigQueryEventBuilder` with implementation in `BigQueryEventBuilder` and `BigQueryTracker`. Therefore the `AppSession` property was added in `BigQueryTracker`. **`AppSession` property has to be set properly.**

REMOVED
* `Promo.News` directory and namespace have been completely removed in favor of the new `lib-promo-news`. **The `lib-promo-news` project has to be included in the app in order to use the News system. First, update this lib, remove `lib-ui-promo` and then add `lib-promo-news`. Do not open Unity before or a complete reimport will be needed!**

### 1028 | 2017-03-30

ADDED
* Extracted `InterstitialAdManager` out of `AdManager` with much improved timing logic. **Properly wire `InterstitialAdManager` into the app.**

MODIFIED
* Revamped `AdManager` by removing interstitial logic. **Replace call to ~~`Setup`~~ method with `Init`.**
* Adapted `AdPlugin` to new `InterstitialAdManager`. Introduced new method `LastO7InterstitialReadyAppId`. **`InterstitialAdManager` property has to be set properly.**
* Introduced `InterstitialAdManager` property in `InterstitialManager`. **`InterstitialAdManager` property has to be set properly.**
* Replaced `AdManager` property with `InterstitialAdManager` in `GameStateManager`. **`InterstitialAdManager` property has to be set properly.**

### 1027 | 2017-03-23

ADDED
* Introduced `UserGender` enum with `SetUserGender` in `AppPlugin` to support reporting user gender to the native.
* Added `GenderQuestion` and `GenderAnswer` BQ event IDs in `CommonTrackingEventParams` to support reporting user gender.

MODIFIED
* Changed `AchievementManager.AreAchievementsSupported` to rely on `GameCenterManager.Available`.
* Changed `BigQueryTracker` to send events regardless grid data.

### 1026 | 2017-02-20

ADDED
* Added `TriggerAdHeightChanged` virtual method in `AdManager` that can be overridden to get info about the banner height change. Also made some private fields protected and some properties virtual.
* Introduced `SetInterstitialProvider` and `GetAvailableInterstitialProviders` methods in `AdPlugin` for selecting and getting ad providers when testing ads.
* Introduced `NativeHeapSize`, `NativeHeapAllocatedSize` and `NativeHeapFreeSize` methods in `AppPlugin` that can be used for detailed debugging or logging. Android only for now.
* Introduced `IsUpdateBlocked`, `IsActionBlocked` and `IsActionAndUpdateBlocked` properties in `StateManager` to fine control blockages and prevent some undesired state change or action fire.
* Added manual-news button impression events and click processing through new `ButtonShown` and `ButtonClicked` methods in `ManualNewsInteraction`.
* Added manual-news button impression support in `ManualNewsManager` and `NewsConfigurableButton`.
* Introduced `AutoNewsFrequencyPeriod` in `AutoNewsData` and `AutoNewsDataParser` to control how to show auto-news in the same session.
* Introduced `IsSpecialButton` in `ManualNewsButtonHandler` with all support for special manual news button.
* Introduced `SpecialButtonImageUrl` and `SpecialButtonTrackingParameters` in `ManualNewsCreativeData`.
* Introduced `SelectionHistoryNavigator` for undo/redo operations in Unity Editor.

MODIFIED
* Improved `AnalyticsPlugin` by offloading analytics collection status to the native side and letting the native side update the status. **`AnalyticsPlugin` is now a MonoBehaviour script and needs to be added to the scene instead of created by the constructor.**
* Added custom stack trace to every log in `O7Log` on Android.
* Moved promo interstitial classes under `News` namespace including some common news stuff. **Some classes were renamed, most have a different namespace now.**
* Refactored promo interstitial classes to inherit common news classes now, including `InterstitialData`, `InterstitialDataParser`, `InterstitialInteraction` and `InterstitialPreparer`.
* Pushed manual-news show policy down to common news.

REMOVED
* Removed `ManualNewsAnimatedButtonHandler`.

### 1025 | 2017-02-01

ADDED
* Introduced `IsPanoramicView` property in `NewsCreativeData` to determine if the creative is panoramic. Added supportive methods in `NewsCreativeDataParser`.
* Introduced `ForceO7InterstitialOverInterstitial` getter in `InterstitialData` to determine if our interstitials should be preferred over others.
* Added `AdManager` property in `InterstitialManager`. **`AdManager` property has to be set properly.**

MODIFIED
* Added support for year of birth in `AgeGateManager` and `AgeGatePlugin`. **Replace `OnDidPass` and `OnDidNotPass` with single method `ApplyBirthYear`.**

REMOVED
* Removed `ButtonImageUri` property and `OpenGridView` method from `GridManager` and corresponding methods from `GridPlugin`. **The old manual-news system is not supported anymore.**
* Removed `Outfit7.News` namespace including `NewsManager` and `NewsPlugin`. **The old auto-news system is not supported anymore.**

### 1024 | 2017-01-03

ADDED
* Introduced `RawData` getter in `StoreIapPack` which holds all JSON data.
* Introduced `IsSubscribed` getter in `StoreIapPack` to find out if a particular subscription IAP pack is active.
* Introduced `FixUrl` method in `RestCall` which should be called on URL before it is being passed to `WWW`. The method switches HTTP protocol to HTTPS if the build is configured so. For iOS only.
* Introduced `SpecialOfferStyleId` getter in `SpecialOffer` for predefined special offer styles.
* Introduced `ButtonEffect` getter in `NewsCreativeData` for manual-news button animation.
* Introduced `IsDebugMode` property in promo `InterstitialManager` to enable debugging for O7 interstitials.

REMOVED
* Removed deprecated getters `Price` and `CurrencyId` from `StoreIapPack`. **They can be accessed via new `RawData` getter in `StoreIapPack`.**
* Removed `AudioRecAndPlay` property from `AudioManager` to get rid of dependency on `Outfit7.Audio.Repeating`. **Override `ToggleMuteSfx` method from `AudioManager` and manually connect sound muting with `AudioRecAndPlay`.**

MODIFIED
* Fixed `CheckAndFixItemsIntegrity` method in `AddOnManager` to disable non-bought add-ons that are somehow enabled.
* Changed `SpecialOffer` to allow null special offer background image URL.
* Changed `IsCreativesCached` method in `SpecialOffer` to virtual.
* Changed `CreativeReady` method in `SpecialOfferManager` to virtual.
* Changed `ToggleMuteSfx` method in `AudioManager` to virtual.

### 1023 | 2016-11-22

ADDED
* Introduced `ServerBaseUrl` protected getter in `AbstractSocialStateManager` to get a custom server base URL from grid data if exists.
* Introduced `PrepareUrl` protected method in `AbstractUserStateSender` and `AbstractSocialStateWorker` to be able to prepare a custom social state URL from various parameters. **`SendToBackend` method now gets the prepared URL instead of preparing it on its own.**

MODIFIED
* Added the UID and the server base URL parameter to `SendState` method in `AbstractUserStateSender` to provide a UID and a custom server base URL from outside.
* Added the server base URL parameter to `PostSendStateAndRetrieveUsers` method in `AbstractSocialStateWorker` to provide a custom server base URL. **Provide the server base URL to this method.**
* Changed `Adjuster` to be extensible-friendly. Also added the server base URL parameter to `FetchNewAdjustments` and other supportive methods to provide a custom server base URL from outside.

REMOVED
* Removed obsolete `Force` BigQuery group ID from `CommonTrackingEventParams`.

### 1022 | 2016-11-21

ADDED
* Introduced app shortcuts with `AppShortcutManager` and `AppShortcutPlugin` to configure 3D touch on iOS and shortcuts on Android 7.1+ and to get notified if the app was opened by a shortcut.
* Introduced `GoToBackground` method in `AppPlugin` to put the app in the background. Only on Android for now.
* Introduced `ShowGameWallWithNotifications` method in `PublisherHelper` and `PublisherPlugin` to support local reminders by Bee7.
* Introduced `UniqueIdConverter` to properly convert UDID to UID.

MODIFIED
* Overhauled `CurrencyManager` to track changes and save them on demand or immediately. Added optional parameters to methods to disallow automatic creation of events and disallow immediate change save. Replaced deprecated `TrackingManager` with `BigQueryTracker`. **`TrackingManager` property has to be replaced with `BigQueryTracker`. You should save the currency state by calling `WriteChanges` method instead of manually saving the `CurrencyState` with `CurrencyStateStore`.**
* Changed `FileLogger` to automatically create new log file on every app start.
* Replaced `SetDebugMode` setter with a proper `IsDebugMode` property in `Admanager`.
* Changed the last parameter (bool forceActive) in `CreateBuilder` method in `BigQueryTracker` to optional (default is false) to ease creating events.
* Changed `Uid` in `AppPlugin` for Unity Editor to create proper UID with new `UniqueUidConverter`.
* Improved `AssetReference` and `AssetReferenceEditor`.
* Improved performance of `ToString` methods in `SimpleJSON` by using StringBuilder.

### 1021 | 2016-11-17

ADDED
* Introduced Unity O7 interstitials under `Interstitial` promo namespace.
* Introduced `PromoLibraryVersion` getter in `AppPlugin` for retrieving the promo library version from native.
* Introduced animated manual-news button mechanism (`ManualNewsAnimatedButtonHandler`).
* Introduced `Firebase` namespace for Firebase analytics tracker.
* Introduced `AnalyticsPlugin` for analytics opt-out support.
* Added `AnalyticsPlugin` property to `BigQueryTracker` and Google Analytics `Tracker` to support explicit analytics opt-out. **`AnalyticsPlugin` property has to be set properly.**
* Added `IsDisabled` property to `BigQueryTracker` and Google Analytics `Tracker` to support explicit shut-down of the tracker.
* Added `FirebaseTracker` property to `CurrencyManager` to create Firebase events on currency change. **`FirebaseTracker` property has to be set properly.**
* Added `AgeGateManager` property to `DreamingOfManager` to check for child-safe action URL. **`AgeGateManager` property has to be set properly.**
* Introduced `ActionUrlChildSafe` property and `GetAgeProperActionUrl` method in `DreamingOfManager` to support a child-safe action URL besides the normal one.
* Introduced `IsInternalVideoGalleryUrl` util method in `VideoGalleryManager`.

### 1020 | 2016-10-25

ADDED
* Introduced `ReferenceFinder` Unity Editor tool, which finds all usages of a particular asset name.
* Introduced `OpenVideoGalleryViewWithUrl` method in `VideoGalleryManager` & `VideoGalleryPlugin` to open a provided URL inside the video gallery.
* Introduced `MANUAL_NEWS_BUTTON_READY` EventBus event to inform listeners that a particular manual-news button becomes ready or not ready.

MODIFIED
* Changed `AddOnCacheHandler` default cache file name to "O7AddOns_{0}.json", where {0} is the app version. This effectively "clears" cache on each app update.

REMOVED
* Removed ~~`AppBuild`~~ from `AppPlugin` since `AppVersion` already contains all the information including the build number. **Use `AppVersion` instead.**

### 1019 | 2016-09-13

ADDED
* Introduced `IsIncremental` getter and constructor parameter in `CollectionAchievement` to define if a collection achievement is simple or incremental.

MODIFED
* Merged `AddToCollectionAndIncrement` and `AddToCollectionAndUnlock` methods into one `AddToCollectionAndSend` method in `AchievementManager`. **See new `IsIncremental` in `CollectionAchievement`.**

### 1018 | 2016-09-01

ADDED
* Introduced `L10nIniter` which reads language code from the native, checks if language is supported (and can be rendered) and sets it as the application language.
* Introduced `LanguageCodes` where all supported languages are listed with some helper methods.
* Introduced `LocalizationHelper` & `LocalizationAsset` to support new localization system.
* Introduced `SignInsCount` property in `GameCenterManager` that persists number of sign-ins locally.
* Introduced `IsGameCenterAppInstalled` getter in `GameCenterManager` & `GameCenterPlugin`.
* Introduced `SetIncrementStepsAndSend` method in `AchievementManager` and `SetCurrentStepsAndSave` method in `IncrementalAchievementPersister<T>` to manually set steps, saves and sends data to the game center.
* Introduced virtual `ShouldOpenAppSettings` method in `NativePermission`.
* Added iOS support for native permissions in `PermissionManager` & `PermissionPlugin`.
* Introduced `StartSendingLogsViaEmail` method in `EmailPlugin` to separate it from more common `StartSendingEmail` method.

MODIFIED
* Completely refactored `LocalizationManager` & `L10n` to get rid of Unity's `SystemLanguage`, to support other languages (added support for traditional Chinese & Hindi) and to improve loading performance. Simplified language handling with language codes only. Added full support for overridden language. Translations are now read from assets (keys & values are separated assets, values having fallbacks to default language built-in), not from JSON file. **Language is not set automatically anymore - you need to set it manually on every app start or use new `L10nIniter`. Translations have to be reimported to assets - a reimporter tool has to be updated. Use `L10n`'s methods (like `Language`, `Is*Language`, `ChangeLanguage`, `ChangeToNextOverriddenLanguage`) to work with languages.**
* Made `IncCurrentSteps` method in `IncrementalAchievement` virtual.
* Changed the return type of `OpenApp` in `GameCenterManager` & `GameCenterPlugin` from void to bool. **The iOS native part of the project needs to be updated to support this change.**
* Made `CanAskForPermission` method in `NativePermission` virtual.
* Changed `SoundTouchPlugin` code for Windows platform to call SoundTouch DLL directly. **SoundTouch DLL for Windows platform is needed - usually in the native part of the project.**
* ConsoleE DLL has been updated from version 2.5.1 to 2.6.0.

REMOVED
* `YouTubeSubscribeManager` & `YouTubeSubscribePlugin` classe has been completely deleted. **YouTube subscription has to be switched to Twitter-like one - opening just the provided URL.**

### 1017 | 2016-08-17

MODIFIED
- Changed ``AddOnItem`` to contain proper localized name & description, introduced min & max versions and black-list platform regex. **Properties of ``AddOnItem`` have been changed.**
- Changed ``AddOnCacheHandler`` to support downloading add-on JSON from specified URL on CDN.
- Added support for ignored & un-ignored add-on list in grid in ``AddOnManager``.

### 1016 | 2016-07-21

ADDED
- Introduced `stopOnSceneChange` optional boolean parameter for `PlayOneShotSoundSFX` method in `MainAudioPlayer`.
- Introduced `CrossPromoAddOnManager` and its companion `CrossPromoAddOnAppData` to manage special add-on items that are used as a cross-promo mechanism.

MODIFIED
- Removed `Geometry` namespace, which is already in `lib-animation`. Moved unit tests to `lib-animation`.

### 1015 | 2016-07-18

MODIFIED
- Fixed `UnityLogHandler` actually not intercepting Unity exceptions on Android.
- Removed `O7Test` in favor of the new `lib-testing`. **Use `lib-testing` for integration tests.**
- Fixed `CurrencyManager.Tag` constant to be protected. **All derivates of `CurrencyManager` should remove their own `Tag` field.**

### 1014 | 2016-07-13

ADDED
- Introduced `Compression` namespace with basic compression stuff. Moved from `lib-editor`.
- Introduced `AreAlmostEqual` methods for double parameters in `FloatingPointUtils`.
- Introduced ConsoleE DLL. Moved from `lib-editor`.
- Introduced `ProjectAnalyzer` class with basic project unit tests. Moved from `lib-editor`.

MODIFIED
- Improved `AssetReferenceEditor` to read resources and streaming assets from any parent directory.
- `lib-editor` should no longer coexist with this lib.

### 1013 | 2016-07-05

ADDED
- Introduced convenient `AddToCollectionAndUnlock` method in `AchievementManager`.
- Introduced convenient `IncCurrentStepsAndSave` method in `IncrementalAchievementPersister<T>`.
- Introduced `AdHeightChanged` event in `AdManager`.
- Introduced `IsCreativesCached` getter in `SpecialOffer`.
- Introduced `OnSpecialOfferActivate` event in `SpecialOfferManager`.
- Introduced `UnQuote` method in `StringUtils` (with unit test) especially to prepare localization table values.

MODIFIED
- Changed to sign in to the game-center in `OpenAchievements` call in `AchievementManager`.
- Fixed to prevent restoring the activated special offer on app start if assets are missing.

### 1012 | 2016-06-21

ADDED
- Introduced `OnAddOnBought` method in `SpecialOfferManager`.
- Introduced `FloatingNotificationPlugin` to control floating notifications on Android only.
- Added `SetReminder` method in `ReminderPlugin` with parameters for setting button texts and actions on expandable notifications on Android only.
- Introduced `Achievements` namespace with manager and various predefined achievements.
- Added full support for achievements on iOS including `ResetAchievements` in `GameCenterPlugin`.

### 1011 | 2016-06-10

ADDED
- Introduced `SetReminder` method in `ReminderPlugin` with texts & actions for two expandable notification buttons (Android only).

MODIFIED
- Removed all dependencies on `lib-editor`.

### 1010 | 2016-05-27

ADDED
- Introduced `SetSplashScreenProgressTextWithColor` method with hex color outline in `AppPlugin`.
- Introduced game center achievements for iOS in `GameCenterManager` and `GameCenterPlugin`. Added BQ events for sign-in and sign-out.

### 1009 | 2016-05-25

ADDED
- Introduced `CheckAndReportInterstitialAvailability` method in `AdManager` that reports a BQ event with source and reason if interstitial is not available. **`TrackingManager` property has to be set properly.**

### 1008 | 2016-05-24

ADDED
- Introduced `GetTimeZoneOffset()`, `CurrentTimeZoneOffset` and `CurrentTimeZoneOffsetSeconds` in `TimeUtils` returning the current time zone's UTC offset (adjusted for DST).
- Introduced `ScheduleReengagementNotifications` in `PublisherHelper` & `PublisherPlugin` to schedule additional Bee7 reminders.
- Introduced `OnLateUpdate` method in `StateManager`. **It should be properly called.**

REMOVED
- Removed old special offer implementation - removed the whole ~~`Outfit7.Purchase.SpecialOffer`~~ namespace.

MODIFIED
- Added current time zone offset "tzo" in seconds as query parameter to user & social backend calls.
- Removed ~~`IapSpecialOfferManager`~~ property and ~~`IsIapSpecialOfferPack`~~ method from `AbstractPurchaseManager` due to removal of old special offer implementation. **A property setter has to be removed.**
- Removed Android special sqlite lib loading in `SQLite`. **Our own-built libsqlite3.so is needed for ARM & x86 for Android.**
- Replaced ~~`CanShowVideo`~~ property with `CheckAndReportVideoAvailability` method in `RewardedVideoClipManager` that reports a BQ event with source and reason if video is not available. **`TrackingManager` property has to be set properly.**

### 1007 | 2016-05-13

ADDED
- Introduced `Permissions` namespace with manager for native permissions for Android 6+.
- Introduced `IapBonusSpecialOffer` for in-app purchase packs with more value for the same price.

MODIFIED
- Introduced `Init` and `OnAppStartOrResume` methods in `VideoClipManager`. **They should be properly called.**
- Changed `OnAppResume` method to `OnAppStartOrResume` in `RewardedVideoClipManager`. **This method should be called on app start, too.**

### 1006 | 2016-05-10

MODIFIED
- Changed `AddOnCacheHandler` to allow `FileName` to be set from the outside. Changed also static methods to non-static. **Now `ClearPrefs` method is not static anymore**.

### 1005 | 2016-04-18

ADDED
- Introduced various helper methods in `L10n` including `IsLanguage`, `CheckAndFixArabic` and `ChangeToNextLanguage` (for Editor only).

MODIFIED
- Improved `LocalizationManager` by adding some helper methods including `IsLanguage`, which replaced ~~`IsArabicLanguage`~~, and `ChangeToNextLanguage` for Editor, which persists current overridden language and restores it on init.

### 1004 | 2016-04-14

ADDED
- Introduced new Unity news promo in `Promo.Creative` & `Promo.News` namespaces.

MODIFIED
- Introduced new BigQuery event tracker in `Analytics.BigQuery` with new event builder and events being routed directly to the native side. Rerouted old `TrackingManager` to use the new tracker. The old tracker still sends all BQ events left in the database, therefore the database is still in read-only use. New database is not created. **`BigQueryPlugin` & `BigQueryTracker` need to be created. `BigQueryTracker` needs to be properly connected, set to `TrackingManager` and inited.** `TrackingManager` and `TrackingWorker` are not needed for new apps and should not be used.

### 1003 | 2016-04-11

MODIFIED
- Changed method signature of `AdManager.BannerHeightInPxNormalized`'s parameter from `Vector2` to `float`. **Just change the calling parameter from `vector` to `vector.y` or similar.**

### 1002 | 2016-04-07

ADDED
- Extracted `EventingAppSession` from `AppSession` with BQ event & GA hit. Introduced forced BQ event for first app install.

MODIFIED
- Simplified `AppSession` by removing BQ event & GA hit (moved to new `EventingAppSession`) to keep it simple and suitable as a utility class. **`TrackingManager` & `Tracker` properties has been removed from `AppSession`.**

### 1001 | 2016-04-06

MODIFIED
- Moved BQ event & GA hit for new session to `AppSession`. They are also created on the first app run and when new session is forcefully created. **Now `TrackingManager` & `Tracker` properties need to be set in `AppSession` before init.**

### 1000 | 2016-03-31

- Introduced this read-me.
