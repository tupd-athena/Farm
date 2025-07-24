using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtils;
using Athena.GameOps;

namespace Atom
{
    public class TrackingManager : SingletonMono<TrackingManager>
    {
        public void TrackEventNormalGameStartDetail(string levelId, string gameMatchId, string root)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_normal_game_start_detail", new Dictionary<string, object>()
            {
                {"level_id", levelId},
                {"game_match_id", gameMatchId},
                {"root", root}
            });
        }

        public void TrackEventNormalGameStartDetail(string levelId, string gameMatchId, double timeSpent, string context, int moves, string retry, int hintUsed)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_normal_game_over_detail", new Dictionary<string, object>()
            {
                {"level_id", levelId},
                {"game_match_id", gameMatchId},
                {"time_spent", timeSpent},
                {"context", context},
                {"moves", moves},
                {"retry", retry},
                {"hint_used", hintUsed}
            });
        }

        public void TrackEventCheckin(int dayChecked, int dayMissed, int dayMakeup, int checkinGap, int clickAmt, int checkpoint0Amt, int checkpoint1Amt, int checkpoint2Amt, int checkpoint3Amt)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_checkin", new Dictionary<string, object>()
            {
                {"day_checked", dayChecked},
                {"day_missed", dayMissed},
                {"day_makeup", dayMakeup},
                {"checkin_gap", checkinGap},
                {"click_amt", clickAmt},
                {"checkpoint0_amt", checkpoint0Amt},
                {"checkpoint1_amt", checkpoint1Amt},
                {"checkpoint2_amt", checkpoint2Amt},
                {"checkpoint3_amt", checkpoint3Amt}
            });
        }

        public void TrackEventDailyCoin(int noadAmt, int adAmt, int clickAmt)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_dailyCoin", new Dictionary<string, object>()
            {
                {"noad_amt", noadAmt},
                {"ad_amt", adAmt},
                {"click_amt", clickAmt}
            });
        }

        public void TrackEventLeaderboard(string leaderboardLog, int clickAmt)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_leaderboard", new Dictionary<string, object>()
            {
                {"leaderboard_log", leaderboardLog},
                {"click_amt", clickAmt}
            });
        }

        public void TrackEventSpend(string itemId, int amount, int price, string source, string currency, string transactionDatetime)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_spend", new Dictionary<string, object>()
            {
                {"item_id", itemId},
                {"amount", amount},
                {"price", price},
                {"source", source},
                {"currency", currency},
                {"transaction_datetime", transactionDatetime}
            });
        }

        public void TrackEventShop(string itemId, int price, int totalClick, string totalBuy)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_shop", new Dictionary<string, object>()
            {
                {"item_id", itemId},
                {"price", price},
                {"total_click", totalClick},
                {"total_buy", totalBuy}
            });
        }

        public void TrackAdImpression(string itemId, int price, int totalClick, string totalBuy)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_ad_impression", new Dictionary<string, object>()
            {
                {"ad_format", itemId},
                {"ad_placement", price}
            });
        }

        public void TrackRate(string rateState, string rateDes, int official)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters("bi_rate", new Dictionary<string, object>()
            {
                {"rate_state", rateState},
                {"rate_des", rateDes},
                {"official", official}
            });
        }

        public void TrackTutorial(int step)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters(Global.TrackingEventName.TUTORIAL, new Dictionary<string, object>()
            {
                {"step", step}
            });
        }

        public void TrackGameStart(string gameMode, string id, string levelId, int attempts)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters(Global.TrackingEventName.GAME_START, new Dictionary<string, object>()
            {
                {"game_mode", gameMode},
                {"ID", id},
                {"level_id", levelId},
                {"attempts", attempts}
            });
        }

        public void TrackGameOver(string gameMode, string id, string levelId, int timeSpent, object result, int levelAbandoned, int attempts, string loseCause)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters(Global.TrackingEventName.GAME_OVER, new Dictionary<string, object>()
            {
                {"game_mode", gameMode},
                {"ID", id},
                {"level_id", levelId},
                {"time_spent", timeSpent},
                {"context", result},
                {"level_abandoned", levelAbandoned},
                {"attempts", attempts},
                {"lose_cause", loseCause},
            });
        }

        public void TrackSourceResourceEvent(string from, string currency, int value, object levelId, int balance)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters(Global.TrackingEventName.BI_RESOURCE_EVENT, new Dictionary<string, object>()
            {
                {"flow_type", "source"},
                {"from", from},
                {"virtual_currency_name", currency},
                {"value", value},
                {"level_id", levelId},
                {"balance", balance}
            });
        }

        public void TrackSinkResourceEvent(string to, string currency, int value, object levelId, int balance)
        {
            AthenaApp.Instance.AnalyticsManager.TrackEventWithParameters(Global.TrackingEventName.BI_RESOURCE_EVENT, new Dictionary<string, object>()
            {
                {"flow_type", "sink"},
                {"to", to},
                {"virtual_currency_name", currency},
                {"value", value},
                {"level_id", levelId},
                {"balance", balance}
            });
        }
    }
}