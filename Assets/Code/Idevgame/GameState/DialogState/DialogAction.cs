using System;

namespace Idevgame.GameState.DialogState {
    public enum DialogAction {
        None,

        //�׳䰴ť��֧��
        FirstBuyPay,

        // common
        BackButton,
        Confirm,
        Cancel,
        Close,
        Open,

        // shop menu
        OpenFood,
        OpenFurniture,
        OpenWardrobe,
        OpenDiamonds,
        OpenWhatsNew,

        Next,
        Previous,

        // sticker album
        FindSellers,

        // Age gate
        Up,
        Down,
        ArrowUpReleased,
        ArrowUpPressed,
        ArrowDownReleased,
        ArrowDownPressed,

        // Restore
        ConfirmUserRestore,
        StartNewGame,

        // gender dialog
        GenderSelected,

        // Purchase dialog
        IapPurchase,
        Coupons,
        FreeCoins,
        RestorePurchase,
        ProcessPurchaseConnectionNeeded,
        //LENTODO ��ʾ/������ʾ���Action
        showHint,
        hideHint,

        // Rate this app
        SetScore,
        RateIt,
        RateNotNow,

        // Upgrade
        BuyWithCoins,
        UpgradeInstantly,

        // Wait for upgrade
        UpgradeDone,

        // minigames
        Restart,
        DoubleCoinsPack,
        SocialConnect,
        InviteFriends,
        SinglePlayer,
        Multiplayer,

        // Fullscreen reward
        RedirectHalloween,
        RedirectUnicorn,
        RedirectCleopatra,

        // on focus
        OnFocus,

        // food for video
        InstantFood,

        OpenLeaderboard,

        OpenDreamingOfPromo,
        OpenUrl,
        PrivacyPolicyPressed,

        WatchVideo,
        BuyItem,
        InstantBuyout,
    }
}