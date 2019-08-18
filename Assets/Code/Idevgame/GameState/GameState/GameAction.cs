public enum GameAction {
    None,

    /// <summary>
    /// 自增首冲按钮事件
    /// </summary>
    OpenFirstBuyPanel,

    Close,
    Skip,
    BackButton,

    HeaderButton,

    // Main
    OpenTerrace,
    OpenBedroom,
    OpenKitchen,
    OpenBathroom,
    ShowManualNews,
    ShowLevelTree,
    ShowVideoGallery,
    ShowXPBoosterPopup,
    ShowShopMenu,
    CloseShopMenu,
    OpenMessages,
    OpenSpecialOffer,
    OpenMerchandiseSpecialOffer,
    LevelUpForVideo,
    RestoreGameState,
    OpenWhatsNew,
    OpenWebShop,
    Coupons,

    // Debug
    OpenDebugMenu,

    // Wardrobe / Furniture
    OpenWardrobe,
    OpenRoomEdit,
    OpenSpecificRoomEdit,
    SelectCategory,
    SelectItem,
    CategoryChanged,
    ToggleAddOn,
    SelectColorPicker,
    CloseColorPicker,
    SelectColor,
    OpenUpgradeDialog,
    Upgrade,
    UpgradeInstantly,
    BuyItem,
    SelectAndBuyItem,
    ItemForVideoCompleted,
    ItemBeingPreparedCompleted,
    InstantItemBuyout,
    ShowVideo,
    ShowVideoForLevelUp,
    OpenMakeUp,
    NotifyUnavailableIAP,
    OpenCustomizeDress,
    CameraStart,
    TakePicture,

    //Common
    Next,
    Previous,
    OpenSpecialOfferRewards,

    // Minigames
    OpenGameWall,
    Bee7OfferButton,
    ShowMoreGames,
    OpenRegularMinigame,

    // Food store
    OpenFoodstore,
    OpenFoodStoreCategory,

    // Livingroom
    OpenStickerAlbum,

    // Bathroom
    OpenTeethCleaning,
    OpenShowerScene,

    // GrowUp
    ShowGrowUp,

    // Sticker Album
    OpenPageNavigation,
    ShowOpenStickerPacks,
    ShowStickerDuplicates,
    ShowStickerTrading,
    SeeInWardrobe,
    SocialConnect,
    //    OpenMissingSticker,
    RewardButton,
    StickerAlbumPageCompleted,
    OpenStickerAlbumCollectionPage,
    OpenStickerCollectionCompleted,
    StickedStickerPress,
    StickedStickerDragEnd,
    RewardDialogClosed,

    // Sticker Album Duplicates
    GiveToAFriend,
    SellForGoldCoins,

    // Sticker Album Trade
    InviteFriends,
    RefreshRandomPlayers,

    // Sticker Album Banner Left
    StickerAlbumBannerLeftButtonPromo,
    StickerAlbumBannerLeftButtonClose,

    // Level tree - old
    ShowLevelUp,
    OpenAchievements,
    OpenGameSettings,
    OpenHowToPlay,
    OpenPreferences,
    OpenLegal,
    OpenEnterNameDialog,

    // Level tree
    EditName,
    HowToPlay,
    Website,
    Options,
    LevelInfo,
    DailyChallenge,
    XpBooster,
    Achievements,
    OpenLevelUp,
    Settings,
    LegalTerms,

    // legal terms
    Eula,
    PrivacyPolicy,
    Contact,
    Coppa,
    ToggleUID,
    OutfitLink,
    OthersLink,
    Privo,
    Eprivacy,
    EprivacyLink,

    // preferences
    ReminderSound,
    MusicInGames,
    Login,
    PushNotification,
    DisableInterestBasedAds,
    SettingsAnalyticsOptOut,
    SettingsConnectWithGameCenter,
    FloatingNotifications,

    // level up
    Bee7Collect,

    // level up effect
    StartLevelUpEffect,

    // Messages
    CollectGoldCoins,
    SayThankYou,

    // Scroll views
    ScrollViewCell,
    ScrollViewButton,
    StickerFriend,

    NextTutorialState,
    FinishTutorial,

    // Purchases
    OpenPurchaseDiamonds,
    OpenPurchaseStickers,
    PurchaseDialogClosed,
    FreePack,
    LockedByLevelConfirmed,

    PurchasePack,
    PurchaseStickerVideoPack,

    CloseButton,

    OpenConfirmation,
    // Age gate scroll
    Up,
    Down,

    ConsumeOrBuyEnergyPotion,

    // minigames
    Play,
    Restart,
    Pause,
    Help,
    GameOver,
    ExtendGame,
    ResumeGame,
    ExitGame,
    MiniGameNextLevel,
    MiniGameNext10Levels,
    MiniGameMoreOptions,

    // makeUp
    LockedMakeUp,

    // url handling
    OpenUrl,

    OpenState,

    // coin to diamond conversion
    DiamondConversionDone,

}


