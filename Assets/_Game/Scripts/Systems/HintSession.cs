using System;
using System.Collections.Generic;

namespace Shift.Game
{
    public enum RewardReason { Hint, Undo }
    public interface IRewardedAdService
    {
        bool IsRewardedAdAvailable { get; }
        // Providers must report on the Unity main thread. Only an earned reward is success.
        void ShowRewardedAd(RewardReason reason, Action<bool> onComplete);
    }
    public sealed class NullRewardedAdService : IRewardedAdService
    {
        public bool IsRewardedAdAvailable => false;
        public void ShowRewardedAd(RewardReason reason, Action<bool> onComplete) => onComplete?.Invoke(false);
    }
    // Authored knowledge is session-only; balances are supplied by the shared daily store.
    public sealed class HintSession
    {
        private readonly Dictionary<LevelData,int> stages = new Dictionary<LevelData,int>();
        private readonly DailyAllowanceService allowance;
        public HintSession(DailyAllowanceService allowance = null)
        {
            this.allowance = allowance ?? new DailyAllowanceService(UnityEngine.Resources.Load<DailyAllowanceConfig>("DailyAllowanceConfig"), () => DateTime.Now);
        }
        public int Remaining => allowance.HintsRemaining;
        public bool HasNext(LevelData level) => level != null && Stage(level) < Math.Max(1, HintCatalog.AuthoredCount(level));
        public int Stage(LevelData level) => stages.TryGetValue(level,out int value) ? value : 0;
        public bool TryUse(LevelData level, out string text)
        {
            text = null; if (!HasNext(level) || !allowance.TryConsumeHint()) return false;
            int stage = Stage(level)+1; stages[level] = stage;
            text = HintCatalog.Get(level,stage, GameLanguageService.Shared.HintLanguage); return true;
        }
        public void GrantReward() => allowance.Grant(RewardReason.Hint, allowance.DateKey);
    }
    public enum HintLanguage { English, Turkish }
    // Separate conceptual metadata preserves all serialized puzzle assets and certificates.
    public static class HintCatalog
    {
        private sealed class Entry
        {
            public readonly string[] English, Turkish;
            public Entry(string[] english, string[] turkish) { English = english; Turkish = turkish; }
            public string[] For(HintLanguage language) => language == HintLanguage.Turkish ? Turkish : English;
        }
        private static readonly Dictionary<string, Entry> authored = new Dictionary<string, Entry> {
            { "Level01_FirstTap", new Entry(
                new[] { "Follow the arrow on the colored piece.", "The exit must accept the piece's color.", "A tap moves the piece in the direction it faces." },
                new[] { "Renkli taşın üzerindeki okun yönüne bak.", "Çıkış, taşın rengini kabul etmeli.", "Her dokunuş taşı baktığı yönde ilerletir." }) },
            { "Level02_FindTheExit", new Entry(
                new[] { "Notice the distance between the piece and the exit.", "An ordinary tap advances the piece by one cell.", "Keep following its arrow until it reaches the exit." },
                new[] { "Taş ile çıkış arasındaki uzaklığa dikkat et.", "Normal bir dokunuş taşı bir kare ilerletir.", "Çıkışa ulaşana kadar taşın okunu takip et." }) },
            { "Level03_ClearTheBox", new Entry(
                new[] { "The box matters even though it is not a target.", "A colored piece can push a box aside.", "Red needs the box out of its upward path." },
                new[] { "Hedef olmasa da kutunun yeri önemli.", "Renkli bir taş kutuyu kenara itebilir.", "Kırmızı'nın yukarı ilerlemesi için kutu yolundan çekilmeli." }) },
            { "Level04_FirstChain", new Entry(
                new[] { "Look at the pieces that already form a line.", "A push can travel through the box to Red.", "The push can carry Red sideways, regardless of its arrow." },
                new[] { "Zaten aynı hizada olan taşlara dikkat et.", "Bir itiş kutudan Kırmızı'ya kadar aktarılabilir.", "İtiş, oku başka yöne baksa da Kırmızı'yı yana taşıyabilir." }) },
            { "Level05_BlockedIsFree", new Entry(
                new[] { "A blocked piece does not always need rescuing.", "A fully blocked tap spends no moves.", "Only Red needs to reach the exit here." },
                new[] { "Sıkışan her taşı kurtarman gerekmeyebilir.", "Hiçbir şeyi hareket ettirmeyen dokunuş hamle harcamaz.", "Burada yalnızca Kırmızı'nın çıkışa ulaşması gerekiyor." }) },
            { "Level06_TurnTheCorner", new Entry(
                new[] { "The floor arrow matters as much as the piece's arrow.", "Landing on it changes direction and continues the movement.", "Follow the upward turn, then check the remaining distance." },
                new[] { "Yerdeki ok da taşın oku kadar önemli.", "Üzerine gelince taş yön değiştirir ve ilerlemeye devam eder.", "Yukarı dönüşü takip et, sonra kalan uzaklığa bak." }) },
            { "Level07_MakeRoom", new Entry(
                new[] { "An exit does not accept every color.", "Pushing Blue along Red's line will not clear it.", "Blue can leave the line in its own direction." },
                new[] { "Bir çıkış her rengi kabul etmez.", "Mavi'yi Kırmızı'nın yolunda itmek engeli kaldırmaz.", "Mavi, kendi yönünde ilerleyerek yoldan çekilebilir." }) },
            { "Level08_Clockwise", new Entry(
                new[] { "That circular symbol changes how a piece faces.", "The rotator turns a moving piece clockwise.", "Picture the new direction after entering from below." },
                new[] { "Dairesel simge, taşın baktığı yönü değiştirir.", "Döndürücü, hareket eden taşı saat yönünde çevirir.", "Alttan girince taşın hangi yöne bakacağını düşün." }) },
            { "Level09_PushAndTurn", new Entry(
                new[] { "Consider where the front piece will land after a push.", "The box passes the push along to the arrow tile.", "Red's landing starts another movement toward the exit." },
                new[] { "İtişten sonra öndeki taşın nereye varacağını düşün.", "Kutu, itişi ok karesine kadar aktarır.", "Kırmızı'nın vardığı kare çıkışa doğru yeni bir hareket başlatır." }) },
            { "Level10_SetTheChain", new Entry(
                new[] { "Look for a useful push, not just a clear step.", "Trace what the next piece will do when pushed.", "Prepare the line before starting the reaction." },
                new[] { "Yalnızca boş bir adım değil, işe yarayan bir itiş ara.", "Öndeki taşın itilince ne yapacağını düşün.", "Tepkiyi başlatmadan önce sırayı hazırla." }) },
            { "Level11_OpenTheLane", new Entry(
                new[] { "A nearby piece can make the shortest path unusable.", "Blue's own arrow leads away from the exit lane.", "Let the crossing clear without pushing Blue toward Red's exit." },
                new[] { "Yakındaki bir taş en kısa yolu kullanılamaz kılabilir.", "Mavi'nin oku onu çıkış yolundan uzaklaştırır.", "Mavi'yi Kırmızı'nın çıkışına itmeden kesişimin boşalmasını sağla." }) },
            { "Level12_CrossingOrder", new Entry(
                new[] { "Think about who would reach the corner, not just move.", "The arrow would send either color toward the red exit.", "Keep Blue off that arrow while making room for Red." },
                new[] { "Yalnızca kimin ilerleyeceğini değil, köşeye kimin varacağını düşün.", "Ok, iki rengi de kırmızı çıkışa yönlendirir.", "Kırmızı'ya yer açarken Mavi'yi o oktan uzak tut." }) },
            { "Level13_SecondAct", new Entry(
                new[] { "The first reaction need not reach the exit.", "Being pushed changes Red's direction as well as its position.", "The new facing lets Red continue toward the distant arrow." },
                new[] { "İlk tepkinin çıkışa ulaşması gerekmiyor.", "İtilmek, Kırmızı'nın konumuyla birlikte yönünü de değiştirir.", "Yeni yönü, Kırmızı'nın ilerideki oka yaklaşmasını sağlar." }) },
            { "Level14_TwoCorners", new Entry(
                new[] { "Trace beyond the nearest arrow before committing.", "The two turns lead into Blue's current line.", "Blue must have somewhere else to go before Red arrives." },
                new[] { "İlerlemeden önce en yakın oktan sonrasını da takip et.", "İki dönüş, Mavi'nin bulunduğu sıraya çıkıyor.", "Kırmızı gelmeden Mavi'nin çekilebileceği bir yer olmalı." }) },
            { "Level15_TheLongSetup", new Entry(
                new[] { "The tempting push depends on a distant landing.", "The upward arrow points straight into Blue's cell.", "Free that landing without sending Blue up the exit lane." },
                new[] { "Cazip görünen itiş, ileride varılacak kareye bağlı.", "Yukarı bakan ok doğrudan Mavi'nin karesine yöneliyor.", "Mavi'yi çıkış yoluna itmeden o kareyi boşalt." }) },
            { "Level16_TransformAhead", new Entry(
                new[] { "A clear first turn does not guarantee a clear finish.", "The later rotator sends Red toward Blue's line.", "Check the final landing before starting the earlier turns." },
                new[] { "İlk dönüşün açık olması, sonunun da açık olduğunu göstermez.", "İlerideki döndürücü Kırmızı'yı Mavi'nin bulunduğu sıraya yönlendirir.", "İlk dönüşleri başlatmadan son varış noktasını kontrol et." }) },
            { "Level17_SharedCorridor", new Entry(
                new[] { "Sharing a corner can put the wrong piece in front.", "Pushing Blue upward sends it into Red's delivery lane.", "Blue's own direction offers a way out of the crossing." },
                new[] { "Aynı köşeyi kullanmak yanlış taşı öne geçirebilir.", "Mavi'yi yukarı itmek onu Kırmızı'nın çıkış yoluna sokar.", "Mavi'nin kendi yönü, kesişimden ayrılmasına olanak verir." }) },
            { "Level18_ClearTriggerFinish", new Entry(
                new[] { "The blocker also has a blocker.", "Blue's sideways landing is occupied by Green.", "Give Blue somewhere to wait before the chain reaches it." },
                new[] { "Yolu kapatan taşın da önünde bir engel var.", "Mavi'nin yana ilerleyeceği karede Yeşil duruyor.", "Zincir Mavi'ye ulaşmadan ona bekleyecek bir yer aç." }) },
            { "Level19_SharedDelivery", new Entry(
                new[] { "The two red pieces can help each other.", "The lower Red can push the upper Red toward the exit.", "A clear landing lets that shared delivery begin." },
                new[] { "İki kırmızı taş birbirine yardım edebilir.", "Alttaki Kırmızı, üsttekini çıkışa doğru itebilir.", "Boş bir varış karesi, bu ortak ilerleyişi başlatır." }) },
            { "Level20_TheFinalShift", new Entry(
                new[] { "The finish depends on more than the opening push.", "The early landing and the later turns belong together.", "Make the first landing usable, then predict each new facing." },
                new[] { "Sonuca ulaşmak, ilk itişten daha fazlasına bağlı.", "İlk varış karesi ve sonraki dönüşler birbirine bağlı.", "İlk varış karesini boşaltıp her dönüşteki yeni yönü düşün." }) },
            { "Level21_FirstSwitch", new Entry(
                new[] { "A closed gate does not stay closed forever.", "Match the symbols on the switch and gate.", "Prepare access before committing the target." },
                new[] { "Kapalı bir kapı sonsuza kadar kapalı kalmaz.", "Düğme ile kapının simgelerini eşleştir.", "Hedef taşı ilerletmeden önce geçişi hazırla." }) },
            { "Level22_OpenForRed", new Entry(
                new[] { "A push can change more than a piece's position.", "The crossing occupant faces a switch away from Red's path.", "Let it leave without redirecting it along the delivery lane." },
                new[] { "Bir itiş, taşın konumundan fazlasını değiştirebilir.", "Kesişimdeki taş, Kırmızı'nın yolundan uzaktaki düğmeye bakıyor.", "Taşı çıkış yoluna yönlendirmeden kesişimden ayrılmasını sağla." }) },
            { "Level23_ToggleTwice", new Entry(
                new[] { "An open gate may be open only for now.", "Both gates change together, but start in opposite states.", "The second toggle must wait until the first gate is crossed." },
                new[] { "Açık bir kapı, yalnızca şimdilik açık olabilir.", "İki kapı birlikte değişir ama başlangıç durumları zıttır.", "İlk kapı geçilmeden ikinci kez durum değiştirmemeli." }) },
            { "Level24_ClosedLane", new Entry(
                new[] { "The narrow passage must serve more than one piece.", "Moving its occupant aside can also change the gate.", "Keep the two red pieces cooperating instead of travelling separately." },
                new[] { "Dar geçidi birden fazla taş kullanacak.", "Önündeki taşın yana çekilmesi kapının durumunu da değiştirebilir.", "İki kırmızı taşın ayrı ayrı ilerlemek yerine birlikte çalışmasını sağla." }) },
            { "Level25_BeforeYouGo", new Entry(
                new[] { "The most direct target move may be too early.", "A route can be open and still be occupied.", "Consider the landing spaces before releasing the target." },
                new[] { "Hedefin en doğrudan hamlesi için henüz erken olabilir.", "Bir yol açık olsa da üzerinde taş bulunabilir.", "Hedefi ilerletmeden taşların nereye varacağını düşün." }) },
            { "Level26_ThreeEntries", new Entry(
                new[] { "A small obstruction can have consequences farther away.", "The upper arrows can carry an unwanted color toward the exit.", "The lower blocker needs a clear landing, not an upward shove." },
                new[] { "Küçük bir engel, daha uzakta sorun çıkarabilir.", "Üstteki oklar yanlış rengi çıkışa taşıyabilir.", "Alttaki engelin yukarı itilmesi değil, varacağı karenin boşalması gerekiyor." }) },
            { "Level27_BoxDoesNotPress", new Entry(
                new[] { "A convenient resting place may be needed again.", "The box can block the crossing even outside Red's starting line.", "Choose parking that leaves the downward bend usable." },
                new[] { "Uygun görünen bir bekleme yeri daha sonra gerekebilir.", "Kutu, Kırmızı'nın ilk yolundan çıksa da kesişimi kapatabilir.", "Kutuyu aşağı dönüşü kullanılabilir bırakacak bir yere park et." }) },
            { "Level28_TurnThrough", new Entry(
                new[] { "The direction of arrival matters as much as the turn.", "A clockwise turn follows the direction a piece enters with.", "Red needs a different approach while the final bend stays clear." },
                new[] { "Geliş yönü, dönüşün kendisi kadar önemli.", "Saat yönündeki dönüş, taşın giriş yönüne göre belirlenir.", "Son köşe boş kalırken Kırmızı başka yönden yaklaşmalı." }) },
            { "Level29_ClockworkGate", new Entry(
                new[] { "Reaching an exit and reaching it efficiently are different challenges.", "Moving the front piece alone misses what the others can do.", "Shared pushes can change direction and advance several reds together." },
                new[] { "Çıkışa ulaşmakla az hamlede ulaşmak farklı sorunlar.", "Öndeki taşı tek başına ilerletmek diğerlerinin katkısını boşa çıkarır.", "Ortak itişler yön değiştirip birkaç kırmızı taşı birlikte ilerletebilir." }) },
            { "Level30_ClearThenOpen", new Entry(
                new[] { "A useful opening can also consume important space.", "Think about what must remain reachable after preparation.", "Check both occupancy and gate state before committing." },
                new[] { "İşe yarayan bir açılış, gereken bir yeri de doldurabilir.", "Hazırlıktan sonra nerelere erişebilmen gerektiğini düşün.", "İlerlemeden önce dolu kareleri ve kapının durumunu kontrol et." }) },
            { "Level31_OnePadTwoGates", new Entry(
                new[] { "An obstacle here may depend on work elsewhere.", "Clearing the box alone does not free the lower passage.", "The upper helper must also vacate the lower blocker’s landing." },
                new[] { "Buradaki bir engel, başka yerde yapılacak işe bağlı olabilir.", "Yalnızca kutuyu çekmek alt geçidi boşaltmaya yetmez.", "Üstteki yardımcı da alttaki taşın varacağı kareden çekilmeli." }) },
            { "Level32_OppositeStates", new Entry(
                new[] { "Returning to a familiar place can still be progress.", "The loop changes how Red approaches its starting junction.", "Temporary parking must be cleared for the returning Red." },
                new[] { "Aynı yere dönmek de ilerlemek anlamına gelebilir.", "Döngü, Kırmızı'nın başlangıç kavşağına yaklaşma yönünü değiştirir.", "Geri dönen Kırmızı için geçici bekleme yeri boşaltılmalı." }) },
            { "Level33_SharedDeliveryGate", new Entry(
                new[] { "Separate paths may work better when they meet.", "The waiting Red needs a push from a different approach.", "Let the second Red redirect its partner at the crossing." },
                new[] { "Ayrı yollar birleşince daha iyi işleyebilir.", "Bekleyen Kırmızı'nın farklı yönden gelen bir itişe ihtiyacı var.", "İkinci Kırmızı'nın kesişimde diğerinin yönünü değiştirmesini sağla." }) },
            { "Level34_ClearTheCorridor", new Entry(
                new[] { "The easiest opening can leave the hardest ending.", "The same rotator gives different results from different approaches.", "The outer arc changes Red's approach; keep its return landing clear." },
                new[] { "En kolay başlangıç, en zor sona götürebilir.", "Aynı döndürücü, farklı geliş yönlerinde farklı sonuçlar verir.", "Dış yay Kırmızı'nın geliş yönünü değiştirir; dönüş karesini boş tut." }) },
            { "Level35_TwoChannels", new Entry(
                new[] { "The pieces do not begin as a working chain.", "Look for the row where separated pieces can cooperate.", "Align the supporting piece before releasing the assembled row." },
                new[] { "Taşlar başlangıçta çalışan bir zincir oluşturmuyor.", "Ayrı duran taşların birlikte çalışabileceği sırayı bul.", "Dizilen sırayı harekete geçirmeden yardımcı taşı hizala." }) },
            { "Level36_CrossBeforeClosing", new Entry(
                new[] { "Both colors need the same turning point.", "The receiving space must be clear before the turn is useful.", "A pushed target may end up facing an exit of the wrong color." },
                new[] { "İki rengin de aynı dönüş noktasına ihtiyacı var.", "Dönüşün işe yaraması için varılacak yer boş olmalı.", "İtilen hedef taş, yanlış renkteki çıkışa bakar hâle gelebilir." }) },
            { "Level37_OpenTheSecondPad", new Entry(
                new[] { "Helping another color does not mean following it home.", "Red and Blue share a bay but need different departures.", "The piece that gave the push needs a new way out." },
                new[] { "Diğer renge yardım etmek, onunla aynı çıkışa gitmek değildir.", "Kırmızı ve Mavi aynı alanı kullanır ama farklı yönlerden ayrılmalı.", "İtişi yapan taşın kendine başka bir çıkış yolu bulması gerekiyor." }) },
            { "Level38_LiftAndTurn", new Entry(
                new[] { "A target can help before it leaves.", "Watch how a landing changes access for the other color.", "Delivering a helper too early can remove the push you still need." },
                new[] { "Bir hedef taş, çıkmadan önce yardım edebilir.", "Bir taşın vardığı yerin diğer rengin geçişini nasıl etkilediğine bak.", "Yardımcıyı erken çıkarmak, hâlâ gereken bir itişi kaybettirebilir." }) },
            { "Level39_PrepareToggleRedirect", new Entry(
                new[] { "Two deliveries need not mean two separate efforts.", "Red and Blue can share the same upward push.", "Their meeting point matters before the sideways push begins." },
                new[] { "İki taşı çıkarmak, iki ayrı çaba gerektirmeyebilir.", "Kırmızı ve Mavi aynı yukarı itişten yararlanabilir.", "Yana itiş başlamadan önce buluşacakları yer önemli." }) },
            { "Level40_TheStateOfShift", new Entry(
                new[] { "One shared system needs two different endings.", "Preserve access to the second operation while preparing the first.", "After one color leaves, the waiting target still needs its own accepting route." },
                new[] { "Ortak bir düzen, iki farklı bitişe hizmet ediyor.", "İlk işi hazırlarken ikinci işe erişimi koru.", "Bir renk çıktıktan sonra bekleyen hedefe uygun çıkış yolu hâlâ gerekiyor." }) },
        };
        public static int AuthoredCount(LevelData level) => AuthoredCount(level, HintLanguage.English);
        public static int AuthoredCount(LevelData level, HintLanguage language)
            => level != null && authored.TryGetValue(level.name, out var entry) ? entry.For(language).Length : 0;
        // Explicit-locale overload remains available for content validation.
        public static string Get(LevelData level, int stage) => Get(level, stage, GameLanguageService.Shared.HintLanguage);
        public static string Get(LevelData level, int stage, HintLanguage language)
        {
            if (level != null && authored.TryGetValue(level.name, out var entry))
            {
                var hints = entry.For(language);
                return hints[Math.Max(1, Math.Min(hints.Length, stage)) - 1];
            }
            if (language == HintLanguage.Turkish) return LocalizationCatalog.ForHint("hint.fallback", language);
            return level != null && !string.IsNullOrWhiteSpace(level.Hint) ? level.Hint : LocalizationCatalog.ForHint("hint.fallback", language);
        }
    }
}
