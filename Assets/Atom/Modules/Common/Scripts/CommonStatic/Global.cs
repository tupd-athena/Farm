using System.Collections;
using System.Collections.Generic;
using Athena.Common;
using Firebase.RemoteConfig;
using Newtonsoft.Json;
using UnityEngine;

namespace Atom
{
    public static class Global
    {
#if UNITY_ANDROID
        public const string STORE_URL = "https://play.google.com/store/apps/details?id=game.puzzle.woody.blockbuster";
#elif UNITY_IOS
        public const string STORE_URL = "https://apps.apple.com/us/app/tasty-blocks-puzzle-adventure/id6468942787";
#endif

        public static class LocalDatabaseKey
        {
            public const string BEST_SCORE = "BestScore";
            public const string IAP_SBUNDLE_EVENT_DATA = "IAPSBundleEventData";
        }

        public static class UIConfig
        {
            public static float DELAY_BLOCK_BREAK_TIME = 0.0f;
            public static float DELAY_BLOCK_REMOVE_TIME = 0.0f;
            public static float FADING_TIME = .5f;
            public static float POPUP_TIME = .3f;
        }

        public static class UserDataConfig
        {
            public static int HEART_REFILL_TIME = 1800; //30 minutes;
        }

        public static class GameConfig
        {
            public const int REFERENCED_BOARD_SIZE = 10;

            public const string IAP_REMOVE_AD_PACK_ID = "blockbuster.puzzle.remove.ads499";
#if UNITY_ANDROID
            public const string IAP_BEGINNER_BUNDLE_ID = "blockbuster.puzzle.sbundle199";
            public const string IAP_DASH_DEAL_BUNDLE_ID = "blockbuster.puzzle.sbundle499";
            public const string IAP_MASTER_BUNDLE_ID = "blockbuster.puzzle.sbundle899";
#elif UNITY_IOS
            public const string IAP_BEGINNER_BUNDLE_ID = "blockbuster.puzzle.Sbundle199";
            public const string IAP_DASH_DEAL_BUNDLE_ID = "blockbuster.puzzle.Sbundle499";
            public const string IAP_MASTER_BUNDLE_ID = "blockbuster.puzzle.Sbundle899";
#endif
            public const string IAP_LAYOUT_KEY_PACK_ICON = "CoinIcon";
            public const string IAP_MORE_COIN_BUNDLE_ID = "blockbuster.puzzle.coin455";
        }

        public static class GameScene
        {
            public static string BOOSTRAP = "Boostrap";
        }

        public static class GameLayer
        {
            public const string ScreenOverlay = "ScreenOverlay";
            public const string UIParticle = "UIParticle";
            public const string UI = "UI";
            public const string Particle = "Particle";
            public const string Block = "Block";
            public const string Tile = "Tile";
            public const string Board = "Board";
            public const string Background = "Background";
        }

        public static class TrackingEventName
        {
            public const string BI_RESOURCE_EVENT = "bi_resource_event";
            public const string BI_PROGRESSION_EVENT = "bi_progression_event";
            public const string BI_NORMAL_GAME_START = "bi_normal_game_start_detail";
            public const string BI_NORMAL_GAME_OVER = "bi_normal_game_over_detail";
            public const string BI_DAILY_GAME_START = "bi_dc_game_start_detail";
            public const string BI_DAILY_GAME_OVER = "bi_dc_game_over_detail";
            public const string BI_DECOR_GAME_START = "bi_decor_game_start_detail";
            public const string BI_DECOR_GAME_OVER = "bi_decor_game_over_detail";
            public const string BUTTON_TAP = "button_tap";
            public const string TUTORIAL = "tutorial";
            public const string GAME_START = "game_start";
            public const string GAME_OVER = "game_over";
        }

        public static class TrackingScreenName
        {
            public const string MENU_HOME = "Menu_Home";
        }

        public static class RemoteConfigGetter
        {
            public static List<IAPBundle> IAP_BUNDLE_CONFIGS
            {
                get
                {
                    return JsonConvert.DeserializeObject<List<IAPBundle>>(FirebaseRemoteConfig.DefaultInstance.GetValue("iap_bundles").StringValue);
                }
            }
        }
    }
}