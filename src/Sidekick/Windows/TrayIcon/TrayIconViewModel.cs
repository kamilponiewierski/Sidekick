using System.Threading.Tasks;
using System.Windows.Input;
using Sidekick.Business.Apis.Poe.Parser;
using Sidekick.Core.Natives;
using Sidekick.UI.Views;
using Sidekick.Windows.ApplicationLogs;
using Sidekick.Windows.Leagues;
using Sidekick.Windows.Prices;
using Sidekick.Windows.Settings;

namespace Sidekick.Windows.TrayIcon
{
    public class TrayIconViewModel
    {
        private readonly App application;
        private readonly IViewLocator viewLocator;
        private readonly INativeClipboard nativeClipboard;
        private readonly IParserService parserService;

        public TrayIconViewModel(
            App application,
            IViewLocator viewLocator,
            INativeClipboard nativeClipboard,
            IParserService parserService)
        {
            this.application = application;
            this.viewLocator = viewLocator;
            this.nativeClipboard = nativeClipboard;
            this.parserService = parserService;
        }

        public ICommand ShowSettingsCommand => new RelayCommand(_ => viewLocator.Open<SettingsView>());

        public ICommand ShowLogsCommand => new RelayCommand(_ => viewLocator.Open<ApplicationLogsView>());

        public ICommand ExitApplicationCommand => new RelayCommand(_ => application.Shutdown());

        public ICommand DebugPriceCheckCommand => new RelayCommand(async _ => await DebugPriceCheck());

        public ICommand DebugLeagueOverlayCommand => new RelayCommand(_ => viewLocator.Open<LeagueView>());

        private async Task DebugPriceCheck()
        {
            await parserService.ParseItem(@"Rarity: Rare
Vengeance Crest
Eternal Burgonet
--------
Quality: +20% (augmented)
Armour: 470 (augmented)
--------
Requirements:
Level: 69
Str: 138
--------
Sockets: R-R-G R 
--------
Item Level: 72
--------
Explosive Arrow deals 25% increased Damage (enchant)
--------
+19 to Armour
+77 to maximum Life
+16% to Fire Resistance
+19% to Chaos Resistance
+26% to Cold Resistance (crafted)
--------
Note: ~price 1 chaos
");

            await parserService.ParseItem(@"Rarity: Rare
Blood Shelter
Nightmare Bascinet
--------
Armour: 162
Evasion Rating: 233
--------
Requirements:
Level: 67
Str: 62
Dex: 85
--------
Sockets: G W 
--------
Item Level: 69
--------
+1 to Level of Socketed Minion Gems
+30 to Strength
+78 to maximum Life
+35% to Cold Resistance
+17% to Chaos Resistance
--------
Corrupted
");

            await nativeClipboard.SetText(@"Rarity: Unique
Blood of the Karui
Sanctified Life Flask
--------
Quality: +20% (augmented)
Recovers 3504 (augmented) Life over 2.60 (augmented) Seconds
Consumes 15 of 30 Charges on use
Currently has 30 Charges
--------
Requirements:
Level: 50
--------
Item Level: 75
--------
100% increased Life Recovered
15% increased Recovery rate
Recover Full Life at the end of the Flask Effect
--------
""Kaom fought and killed for his people.
Kaom bled for his people.
And so the people gave, the people bled,
So their King might go on.""
- Lavianga, Advisor to Kaom
--------
Right click to drink.Can only hold charges while in belt.Refills as you kill monsters.
");

            viewLocator.Open<PriceView>();
        }
    }
}
