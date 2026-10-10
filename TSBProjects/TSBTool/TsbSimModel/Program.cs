using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TsbSimModel
{
    /// <summary>
    /// TsbSimModel: fits, exports and checks the model behind TSBTool's TSB2/TSB3 Auto Update Sim
    /// (TSB2_TSB3\TSB2_3_Core\TsbSimAutoUpdater.cs). See README.md.
    /// </summary>
    public static class Program
    {
        private const string Usage = @"TsbSimModel -- the model behind TSBTool's TSB2/TSB3 'Auto Update Sim' (see README.md)

  fit <TSB2|TSB3> <stock.smc> <outDir>
        Fits the model to the stock ROM's own sim values; writes <game>_model.json, <game>_report.txt
        (accuracy on data the model wasn't fitted on) and <game>_stock_rosters.txt.
  fitteam <TSB2> <stock.smc> <model.json>
        Fits only the team SimData model (TSB2's rush D / pass D) into an existing model file, leaving its
        player models as they are; replaces the TEAM SIMDATA part of <game>_report.txt next to it.
  fitfav <TSB3> <stock.smc> <model.json>
        Fits only the free agent value model (TSB3: Face's last digit, which its team defense adds up) into an
        existing model file; replaces the FREE AGENT VALUE part of <game>_report.txt next to it.
  constants <tsb2_model.json> <tsb3_model.json> [--patch <TsbSimAutoUpdater.cs>]
        Writes TSBTool's model constants; with --patch, replaces them in TsbSimAutoUpdater.cs.
  expected <TSB2|TSB3> <stock.smc> <model.json> <outDir>
        Writes the test files: <game>_stock_rosters.txt and <game>_stock_rosters_autoupdated.txt (the
        expected Auto Update Sim output; TSBTool_Test\TSB2SimModel, TsbSimAutoUpdaterTests), and for TSB2
        tsb2_stock_team_simdata.txt (the expected team SimData; TsbTeamSimDataTests).
  nodice <TSB2|TSB3> <season> <rom> [<rom> ...]
        What the ROMs' sim bytes do for that season, averaged over the teams, with no dice.
  compare <season.srm> [<season.srm> ...]
        League shapes of simmed TSB2/TSB3 seasons (battery saves), side by side.
  apply <TSB2|TSB3> <rom> <out.smc> [--seasons 1,2,3] [--fix-sim-bugs]
        A test ROM: TSBTool's Auto Update Sim on the seasons' rosters (default: all) -- players, and TSB2's
        team SimData -- optionally the FIX_SIM_BUGS fix, SNES header checksum fixed.
";

        public static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.Write(Usage);
                    return 1;
                }
                switch (args[0].ToLowerInvariant())
                {
                    case "fit": return Fit(args);
                    case "fitteam": return FitTeam(args);
                    case "fitfav": return FitFreeAgentValue(args);
                    case "constants": return Constants(args);
                    case "expected": return Expected(args);
                    case "nodice": Need(args, 4); Console.Write(Analysis.NoDice(Game.Parse(args[1]), int.Parse(args[2]), args.Skip(3).ToArray())); return 0;
                    case "compare": Need(args, 2); Console.Write(Analysis.Compare(args.Skip(1).ToArray())); return 0;
                    case "apply": return Apply(args);
                    default: Console.Write(Usage); return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                return 2;
            }
        }

        private static void Need(string[] args, int count)
        {
            if (args.Length < count)
                throw new ArgumentException("Not enough arguments.\n\n" + Usage);
        }

        private static int Fit(string[] args)
        {
            Need(args, 4);
            Game game = Game.Parse(args[1]);
            string rom = args[2], dir = args[3];
            Directory.CreateDirectory(dir);
            string text = Rosters.RosterText(game, rom);
            List<Team> teams = Rosters.ForFitting(game, Rosters.Parse(game, text));
            byte[] romBytes = File.ReadAllBytes(rom);
            int[] rushTable = Enumerable.Range(0, 16).Select(i => (int)romBytes[game.RushTableOffset + i]).ToArray();
            Fitter fitter = new Fitter(game, teams, rushTable);
            FittedModel model = fitter.Fit();
            string report = fitter.Report;
            if (game.HasFreeAgentValue)
                report += FreeAgentValues.Fit(game, text, model);
            string name = game.Name.ToLowerInvariant();
            File.WriteAllText(Path.Combine(dir, name + "_stock_rosters.txt"), text);
            ModelFile.Write(model, Path.Combine(dir, name + "_model.json"));
            File.WriteAllText(Path.Combine(dir, name + "_report.txt"), report);
            Console.Write(report);
            return 0;
        }

        private static int FitTeam(string[] args)
        {
            Need(args, 4);
            Game game = Game.Parse(args[1]);
            if (!game.HasTeamDefense)
                throw new ArgumentException(game.Name + "'s team SimData isn't a team defense the model fits (see README.md).");
            string rom = args[2], modelPath = args[3];
            List<Team> teams = Rosters.ForFitting(game, Rosters.Parse(game, Rosters.RosterText(game, rom)));
            byte[] romBytes = File.ReadAllBytes(rom);
            int[] rushTable = Enumerable.Range(0, 16).Select(i => (int)romBytes[game.RushTableOffset + i]).ToArray();
            Fitter fitter = new Fitter(game, teams, rushTable);
            FittedModel model = ModelFile.Read(modelPath);
            fitter.FitTeamDefense(model);
            ModelFile.Write(model, modelPath);
            string reportPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(modelPath)), game.Name.ToLowerInvariant() + "_report.txt");
            string report = File.Exists(reportPath) ? File.ReadAllText(reportPath) : "";
            int cut = report.IndexOf("\nTEAM SIMDATA");
            if (cut < 0)
                cut = report.IndexOf("\r\nTEAM SIMDATA");
            if (cut >= 0)
                report = report.Substring(0, cut).TrimEnd('\r', '\n') + "\n";
            File.WriteAllText(reportPath, report + fitter.Report);
            Console.Write(fitter.Report);
            return 0;
        }

        private static int FitFreeAgentValue(string[] args)
        {
            Need(args, 4);
            Game game = Game.Parse(args[1]);
            if (!game.HasFreeAgentValue)
                throw new ArgumentException(game.Name + " has no free agent value model (see README.md).");
            string modelPath = args[3];
            FittedModel model = ModelFile.Read(modelPath);
            string report = FreeAgentValues.Fit(game, Rosters.RosterText(game, args[2]), model);
            ModelFile.Write(model, modelPath);
            ReplaceReportPart(modelPath, game, "FREE AGENT VALUE", report);
            Console.Write(report);
            return 0;
        }

        /// <summary>Replaces the part of '&lt;game&gt;_report.txt' (next to the model file) that starts with 'heading' (to the next part or the end).</summary>
        private static void ReplaceReportPart(string modelPath, Game game, string heading, string part)
        {
            string reportPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(modelPath)), game.Name.ToLowerInvariant() + "_report.txt");
            string report = File.Exists(reportPath) ? File.ReadAllText(reportPath).Replace("\r\n", "\n") : "";
            int cut = report.IndexOf("\n" + heading);
            if (cut >= 0)
            {
                int next = report.IndexOf("\n\n", cut + 1);
                string rest = next >= 0 ? report.Substring(next) : "";
                report = report.Substring(0, cut).TrimEnd('\n') + "\n" + rest.TrimStart('\n');
            }
            File.WriteAllText(reportPath, report.TrimEnd('\n') + "\n" + part.Replace("\r\n", "\n"));
        }

        private static int Constants(string[] args)
        {
            Need(args, 3);
            string block = ConstantsWriter.Block(args[1], args[2]);
            int patch = Array.IndexOf(args, "--patch");
            if (patch > 0 && patch + 1 < args.Length)
            {
                ConstantsWriter.Patch(args[patch + 1], block);
                Console.WriteLine("Patched " + args[patch + 1]);
            }
            else
                Console.Write(block);
            return 0;
        }

        private static int Expected(string[] args)
        {
            Need(args, 5);
            Game game = Game.Parse(args[1]);
            string text = Rosters.RosterText(game, args[2]);
            FittedModel model = ModelFile.Read(args[3]);
            string dir = args[4];
            Directory.CreateDirectory(dir);
            string name = game.Name.ToLowerInvariant();
            File.WriteAllText(Path.Combine(dir, name + "_stock_rosters.txt"), text);
            File.WriteAllText(Path.Combine(dir, name + "_stock_rosters_autoupdated.txt"), ExpectedWriter.AutoUpdated(game, model, text));
            Console.WriteLine("Wrote " + name + "_stock_rosters.txt and " + name + "_stock_rosters_autoupdated.txt to " + dir);
            if (model.TeamDefense.Count > 0)
            {
                File.WriteAllText(Path.Combine(dir, name + "_stock_team_simdata.txt"), ExpectedWriter.TeamSimData(game, model, text));
                Console.WriteLine("Wrote " + name + "_stock_team_simdata.txt to " + dir);
            }
            return 0;
        }

        private static int Apply(string[] args)
        {
            Need(args, 4);
            Game game = Game.Parse(args[1]);
            int[] seasons = game.Seasons;
            int s = Array.IndexOf(args, "--seasons");
            if (s > 0 && s + 1 < args.Length)
                seasons = args[s + 1].Split(',').Select(int.Parse).ToArray();
            Analysis.Apply(game, args[2], args[3], seasons, args.Contains("--fix-sim-bugs"));
            Console.WriteLine("Wrote " + args[3]);
            return 0;
        }
    }
}
