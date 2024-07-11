/**
 * @version 1.0.0.0
 * @copyright Copyright ©  2020
 * @compiler Bridge.NET 17.10.1
 */
Bridge.assembly("Bridge_TSBTool", function ($asm, globals) {
    "use strict";

    /** @namespace Bridge_TSBTool */

    /**
     * Bridge_TSBTool.App.___
     Public API 
     window.tecmoTool: {
         bool loadFile(string input): Prompts user to load a ROM; returns true when successful
         string getAllContents({options}): Get contents for the loaded ROM
         void applyData(string): Applies the data passed
         void saveFile(): save the current work to a rom.
         string ConvertToTSB1Format(string): Converts data to TSB1 Format; returns the converted data
         string ConvertToTSB2Format(string): Converts data to TSB2 Format; returns the converted data
         string ConvertToTSB3Format(string): Converts data to TSB3 Format; returns the converted data
     }
     *
     * @public
     * @class Bridge_TSBTool.App
     */
    Bridge.define("Bridge_TSBTool.App", {
        main: function Main () {
            var $t;
            var loadButton = ($t = document.createElement("input"), $t.id = "mHiddenLoadButton", $t.type = "file", $t.innerHTML = "Load File", $t.style.display = "none", $t);
            loadButton.onchange = function (ev) {
                Bridge_TSBTool.App.LoadFile(loadButton.files[0]);
            };
            document.body.appendChild(loadButton);
        },
        statics: {
            fields: {
                sButtonHeight: null,
                sButtonMargin: null,
                tool: null,
                mTextBox: null,
                applyButton: null,
                viewButton: null,
                saveButton: null
            },
            ctors: {
                init: function () {
                    this.sButtonHeight = "40px";
                    this.sButtonMargin = "5px";
                }
            },
            methods: {
                /**
                 * https://github.com/bridgedotnet/Bridge/wiki
                 *
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @return  {void}
                 */
                RenderSamplePage: function () {
                    var $t;




                    Bridge_TSBTool.App.mTextBox = ($t = document.createElement("textarea"), $t.id = "mTextBox", $t.style.marginLeft = "5px", $t.style.height = "65vh", $t.style.width = "92%", $t.style.maxWidth = "700px", $t);
                    Bridge_TSBTool.App.mTextBox.setAttribute("spellcheck", "false");
                    document.body.appendChild(Bridge_TSBTool.App.mTextBox);
                    document.body.appendChild(document.createElement("p"));

                    var loadButton = ($t = document.createElement("input"), $t.id = "mLoadButton", $t.type = "file", $t.innerHTML = "Load File", $t.style.margin = Bridge_TSBTool.App.sButtonMargin, $t.style.height = Bridge_TSBTool.App.sButtonHeight, $t);
                    loadButton.onchange = function (ev) {
                        Bridge_TSBTool.App.LoadFile(loadButton.files[0]);
                    };
                    document.body.appendChild(loadButton);

                    Bridge_TSBTool.App.viewButton = ($t = document.createElement("button"), $t.id = "mViewButton", $t.innerHTML = "View Contents", $t.style.margin = Bridge_TSBTool.App.sButtonMargin, $t.style.height = Bridge_TSBTool.App.sButtonHeight, $t);
                    Bridge_TSBTool.App.viewButton.onclick = function (ev) {
                        Bridge_TSBTool.App.ViewContents();
                    };
                    document.body.appendChild(Bridge_TSBTool.App.viewButton);

                    Bridge_TSBTool.App.applyButton = ($t = document.createElement("button"), $t.id = "mApplyButton", $t.innerHTML = "Apply To ROM", $t.style.margin = Bridge_TSBTool.App.sButtonMargin, $t.style.height = Bridge_TSBTool.App.sButtonHeight, $t);
                    Bridge_TSBTool.App.applyButton.onclick = function (ev) {
                        Bridge_TSBTool.App.ApplyToRom();
                    };
                    document.body.appendChild(Bridge_TSBTool.App.applyButton);

                    Bridge_TSBTool.App.saveButton = ($t = document.createElement("button"), $t.id = "mSaveButton", $t.innerHTML = "Save File", $t.style.margin = Bridge_TSBTool.App.sButtonMargin, $t.style.height = Bridge_TSBTool.App.sButtonHeight, $t);
                    Bridge_TSBTool.App.saveButton.onclick = function (ev) {
                        Bridge_TSBTool.App.SaveFile();
                    };
                    document.body.appendChild(Bridge_TSBTool.App.saveButton);
                    Bridge_TSBTool.App.state1();
                },
                LoadROM: function (rom) {
                    Bridge_TSBTool.App.tool = Bridge.as(TSBTool.TecmoToolFactory.GetToolForRom(rom), TSBTool.ITecmoTool);
                    if (Bridge_TSBTool.App.tool != null && Bridge_TSBTool.App.tool.TSBTool$ITecmoTool$OutputRom != null) {
                        System.Console.WriteLine("ROM Loaded; version = " + System.Enum.toString(TSBTool.ROM_TYPE, Bridge_TSBTool.App.tool.TSBTool$ITecmoTool$RomVersion));
                        Bridge_TSBTool.App.state2();
                    } else {
                        Bridge_TSBTool.App.state1();
                    }
                },
                state1: function () {
                    var $t;
                    if (Bridge_TSBTool.App.viewButton != null) {
                        Bridge_TSBTool.App.viewButton.disabled = ($t = (Bridge_TSBTool.App.applyButton.disabled = true, true), Bridge_TSBTool.App.saveButton.disabled = $t, $t);
                    }
                },
                state2: function () {
                    var $t;
                    if (Bridge_TSBTool.App.viewButton != null) {
                        Bridge_TSBTool.App.viewButton.disabled = ($t = (Bridge_TSBTool.App.applyButton.disabled = false, false), Bridge_TSBTool.App.saveButton.disabled = $t, $t);
                    }
                },
                ViewContents: function () {
                    Bridge_TSBTool.App.mTextBox.value = (Bridge_TSBTool.App.tool.TSBTool$ITecmoTool$GetKey() || "") + "\n" + (Bridge_TSBTool.App.tool.TSBTool$ITecmoTool$GetAll() || "");
                },
                ApplyToRom: function () {
                    System.Console.WriteLine("ApplyToRom() called!");
                    if (Bridge_TSBTool.App.tool != null) {
                        var stuff = Bridge_TSBTool.App.mTextBox.value;
                        Bridge_TSBTool.App.tool.TSBTool$ITecmoTool$ProcessText(stuff);
                    }
                },
                /**
                 * used like: loadButton.OnChange = (ev) =&gt; { LoadFile(loadButton.Files[0]);  };
                 Where 'loadButton' is an input element (button)
                 *
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @param   {File}    blob
                 * @return  {void}
                 */
                LoadFile: function (blob) {
                    var reader = new FileReader();

                    reader.onload = function (ev) {
                        var arrayBuffer = reader.result;
                        var arr = new Uint8Array(arrayBuffer);
                        var rom = System.Array.init(arr.byteLength, 0, System.Byte);
                        for (var i = 0; i < arr.byteLength; i = (i + 1) | 0) {
                            rom[System.Array.index(i, rom)] = arr[i];
                        }
                        Bridge_TSBTool.App.LoadROM(rom);
                    };
                    reader.readAsArrayBuffer(blob);
                },
                /**
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @return  {void}
                 */
                PromptAndLoadFile: function () {
                    document.getElementById("mHiddenLoadButton").click();
                },
                /**
                 * Get contents for the loaded ROM
                 *
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @return  {string}
                 */
                GetAllContents: function () {
                    var retVal = "<no data loaded>";
                    if (Bridge_TSBTool.App.tool != null) {
                        retVal = Bridge_TSBTool.App.tool.TSBTool$ITecmoTool$GetAll();
                    } else {
                        System.Console.WriteLine("No Rom Loaded");
                    }
                    return retVal;
                },
                /**
                 * Applies the data passed
                 *
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @param   {string}    data    The data to apply
                 * @return  {void}
                 */
                ApplyData: function (data) {
                    if (Bridge_TSBTool.App.tool != null) {
                        Bridge_TSBTool.App.tool.TSBTool$ITecmoTool$ProcessText(data);
                    } else {
                        System.Console.WriteLine("No Rom Loaded");
                    }
                },
                /**
                 * save the current work to a rom, prompts user for filename.
                 *
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @return  {void}
                 */
                SaveFile: function () {
                    if (Bridge_TSBTool.App.tool != null) {
                        var fileName = window.prompt("Save file name", "");
                        var u8a = new Uint8Array(Bridge_TSBTool.App.tool.TSBTool$ITecmoTool$OutputRom);
                        var blob = window.createBlobFromArrayBuffer(u8a);
                        window.saveFile(fileName, blob);
                    } else {
                        System.Console.WriteLine("No Rom Loaded");
                    }
                },
                /**
                 * Converts data to TSB1 Format; returns the converted data
                 *
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @param   {string}    input
                 * @return  {string}
                 */
                ConvertToTSB1Format: function (input) {
                    var type = TSBTool.StaticUtils.GetContentType(input);
                    return TSBTool2.TecmoConverter.Convert(type, TSBTool.TSBContentType.TSB1, input);
                },
                /**
                 * Converts data to TSB2 Format; returns the converted data
                 *
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @param   {string}    input
                 * @return  {string}
                 */
                ConvertToTSB2Format: function (input) {
                    var type = TSBTool.StaticUtils.GetContentType(input);
                    return TSBTool2.TecmoConverter.Convert(type, TSBTool.TSBContentType.TSB2, input);
                },
                /**
                 * Converts data to TSB3 Format; returns the converted data
                 *
                 * @static
                 * @public
                 * @this Bridge_TSBTool.App
                 * @memberof Bridge_TSBTool.App
                 * @param   {string}    input
                 * @return  {string}
                 */
                ConvertToTSB3Format: function (input) {
                    var type = TSBTool.StaticUtils.GetContentType(input);
                    return TSBTool2.TecmoConverter.Convert(type, TSBTool.TSBContentType.TSB2, input);
                }
            }
        }
    });

    Bridge.define("System.Windows.Forms.DialogResult", {
        $kind: "enum",
        statics: {
            fields: {
                None: 0,
                OK: 1,
                Cancel: 2,
                Abort: 3,
                Retry: 4,
                Ignore: 5,
                Yes: 6,
                No: 7
            }
        }
    });

    Bridge.define("System.Windows.Forms.MessageBox", {
        statics: {
            methods: {
                Show: function (owner, text, caption, buttons, icon) {
                    System.Console.WriteLine("This should not be called!!!");
                    return System.Windows.Forms.DialogResult.Cancel;
                }
            }
        }
    });

    Bridge.define("System.Windows.Forms.MessageBoxButtons", {
        $kind: "enum",
        statics: {
            fields: {
                OK: 0,
                OKCancel: 1,
                AbortRetryIgnore: 2,
                YesNoCancel: 3,
                YesNo: 4,
                RetryCancel: 5
            }
        }
    });

    Bridge.define("System.Windows.Forms.MessageBoxIcon", {
        $kind: "enum",
        statics: {
            fields: {
                None: 0,
                Error: 16,
                Hand: 16,
                Stop: 16,
                Question: 32,
                Exclamation: 48,
                Warning: 48,
                Information: 64,
                Asterisk: 64
            }
        }
    });

    Bridge.define("TSBTool.Conference", {
        $kind: "enum",
        statics: {
            fields: {
                AFC: 0,
                NFC: 1
            }
        }
    });

    /** @namespace TSBTool */

    /**
     * Summary description for SchedulerHelper2.
     *
     * @public
     * @class TSBTool.ScheduleHelper2
     */
    Bridge.define("TSBTool.ScheduleHelper2", {
        fields: {
            weekOneStartLoc: 0,
            end_schedule_section: 0,
            gamesPerWeekStartLoc: 0,
            weekPointersStartLoc: 0,
            teamGames: null,
            total_games_possible: 0,
            gamePerWeekLimit: 0,
            totalGameLimit: 0,
            totalWeeks: 0,
            week: 0,
            week_game_count: 0,
            total_game_count: 0,
            messages: null,
            outputRom: null,
            gameRegex: null
        },
        props: {
            /**
             * @instance
             * @public
             * @readonly
             * @memberof TSBTool.ScheduleHelper2
             * @function TotalGameCount
             * @type number
             */
            TotalGameCount: {
                get: function () {
                    return this.total_game_count;
                }
            }
        },
        ctors: {
            init: function () {
                this.weekOneStartLoc = 207323;
                this.end_schedule_section = 213006;
                this.gamesPerWeekStartLoc = 207305;
                this.weekPointersStartLoc = 207271;
                this.total_games_possible = 238;
                this.gamePerWeekLimit = 14;
                this.totalGameLimit = 224;
                this.totalWeeks = 17;
                this.week = -1;
                this.week_game_count = 0;
                this.total_game_count = 0;
            },
            ctor: function (outputRom) {
                this.$initialize();
                this.outputRom = outputRom;
                this.gameRegex = new System.Text.RegularExpressions.Regex.ctor("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");
            }
        },
        methods: {
            AddMessage: function (message) {
                if (message != null && message.length > 0) {
                    this.messages.add(message);
                }
            },
            /**
             * Applies a schedule to the rom.
             *
             * @instance
             * @public
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @param   {System.Collections.Generic.List$1}    lines    the contents of the schedule file.
             * @return  {void}
             */
            ApplySchedule: function (lines) {
                this.week = -1;
                this.week_game_count = 0;
                this.total_game_count = 0;
                this.messages = new (System.Collections.Generic.List$1(System.String)).$ctor2(50);

                var line;
                for (var i = 0; i < lines.Count; i = (i + 1) | 0) {
                    line = Bridge.toString(lines.getItem(i)).trim().toLowerCase();
                    try {
                        if (System.String.startsWith(line, "#") || line.length < 3) {
                        } else if (System.String.startsWith(line, "week")) {
                            if (this.week > ((this.totalWeeks - 1) | 0)) {
                                this.AddMessage("Error! You can have only 17 weeks in a season.");
                                break;
                            }
                            this.SetupWeek();
                            TSBTool.StaticUtils.WriteError(System.String.format("Scheduleing {0}", [line]));
                        } else {
                            this.ScheduleGame$1(line);
                        }
                    } catch (e) {
                        e = System.Exception.create(e);
                        TSBTool.StaticUtils.WriteError(System.String.format("Exception! with line '{0}' {1}\n{2}", line, e.Message, e.StackTrace));
                        this.AddMessage(System.String.format("Error on line '{0}'", [line]));
                    }
                }
                this.ClosePrevWeek();
                if (this.week < ((this.totalWeeks - 1) | 0)) {
                    this.AddMessage("Warning! You didn't schedule all 17 weeks. The schedule could be messed up.");
                }
                if (this.teamGames != null) {
                    for (var i1 = 0; i1 < this.teamGames.length; i1 = (i1 + 1) | 0) {
                        if (this.teamGames[System.Array.index(i1, this.teamGames)] !== 16) {
                            this.AddMessage(System.String.format("Warning! The {0} have {1} games scheduled.", TSBTool.TecmoTool.GetTeamFromIndex(i1), Bridge.box(this.teamGames[System.Array.index(i1, this.teamGames)], System.Int32)));

                        }
                    }
                }
            },
            SetupWeek: function () {
                this.ClosePrevWeek();
                this.week = (this.week + 1) | 0;
                this.total_game_count = (this.total_game_count + this.week_game_count) | 0;
                this.week_game_count = 0;
                this.SetupPointerForCurrentWeek();
            },
            ClosePrevWeek: function () {
                if (this.week > -1) {
                    var location = (this.gamesPerWeekStartLoc + this.week) | 0;
                    this.outputRom[System.Array.index(location, this.outputRom)] = this.week_game_count & 255;
                    if (this.week_game_count === 0) {
                        this.AddMessage(System.String.format("ERROR! Week {0}. You need at least 1 game in each week.", [Bridge.box(((this.week + 1) | 0), System.Int32)]));
                    }
                }
            },
            SetupPointerForCurrentWeek: function () {
                if (this.week === 0) {
                    return;
                }
                var val = ((Bridge.Int.mul(2, this.total_game_count)) + 35275) | 0;
                var location = (this.weekPointersStartLoc + (Bridge.Int.mul(this.week, 2))) | 0;
                if (this.week < 17) {
                    this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = (val >> 8) & 255;
                    this.outputRom[System.Array.index(location, this.outputRom)] = (val & 255) & 255;
                } else {
                    this.AddMessage(System.String.format("ERROR! To many Weeks {0}", [Bridge.box(((this.week + 1) | 0), System.Int32)]));
                }
            },
            /**
             * Attempts to schedule a game.
             *
             * @instance
             * @protected
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @param   {string}     awayTeam    Away team's name.
             * @param   {string}     homeTeam    Home team's name.
             * @return  {boolean}                true on success, false on failure.
             */
            ScheduleGame: function (awayTeam, homeTeam) {
                var ret = false;
                var awayIndex = TSBTool.TecmoTool.GetTeamIndex(awayTeam);
                var homeIndex = TSBTool.TecmoTool.GetTeamIndex(homeTeam);

                if (awayIndex === -1 || homeIndex === -1) {
                    this.AddMessage(System.String.format("Error! Week {2}: Game '{0} at {1}'", awayTeam, homeTeam, Bridge.box(((this.week + 1) | 0), System.Int32)));
                    return false;
                }

                if (awayIndex === homeIndex) {
                    this.AddMessage(System.String.format("Warning! Week {0}: The {1} are scheduled to play against themselves.", Bridge.box(((this.week + 1) | 0), System.Int32), awayTeam));
                }

                var location = (this.weekOneStartLoc + (Bridge.Int.mul((((this.week_game_count + this.total_game_count) | 0)), 2))) | 0;
                if (location >= this.weekOneStartLoc && location < this.end_schedule_section) {
                    this.outputRom[System.Array.index(location, this.outputRom)] = awayIndex & 255;
                    this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = homeIndex & 255;
                    this.IncrementTeamGames(awayIndex);
                    this.IncrementTeamGames(homeIndex);
                    ret = true;
                }
                return ret;
            },
            /**
             * Attempts to schedule a game.
             *
             * @instance
             * @private
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @param   {string}     line
             * @return  {boolean}            True on success, false on failure.
             */
            ScheduleGame$1: function (line) {
                var ret = false;
                var m = this.gameRegex.match(line);
                var awayTeam, homeTeam;

                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    awayTeam = m.getGroups().get(1).toString();
                    homeTeam = m.getGroups().get(2).toString();
                    if (this.week_game_count > ((this.gamePerWeekLimit - 1) | 0)) {
                        this.AddMessage(System.String.format("Error! Week {0}: You can have no more than {1} games in a week.", Bridge.box(((this.week + 1) | 0), System.Int32), Bridge.box(this.gamePerWeekLimit, System.Int32)));
                        ret = false;
                    } else if (this.ScheduleGame(awayTeam, homeTeam)) {
                        this.week_game_count = (this.week_game_count + 1) | 0;
                        ret = true;
                    } else {
                    }
                }
                if (((this.total_game_count + this.week_game_count) | 0) > this.totalGameLimit) {
                    this.AddMessage(System.String.format("Warning! Week {0}: There are more than {1} games scheduled.", Bridge.box(((this.week + 1) | 0), System.Int32), Bridge.box(this.gamePerWeekLimit, System.Int32)));
                }
                return ret;
            },
            /**
             * Gets the Schedule.
             *
             * @instance
             * @public
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @return  {string}
             */
            GetSchedule: function () {
                var sb = new System.Text.StringBuilder("", 5712);
                for (var i = 0; i < 17; i = (i + 1) | 0) {
                    sb.append(System.String.format("WEEK {0}\n", [Bridge.box((((i + 1) | 0)), System.Int32)]));
                    sb.append((this.GetWeek(i) || "") + "\n");
                }
                return sb.toString();
            },
            /**
             * Gets the schedule for week 'week'.
             *
             * @instance
             * @public
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @param   {number}    week    The week to get.(Zero-based)
             * @return  {string}            The week as a string.
             */
            GetWeek: function (week) {
                if (week < 0 || week > ((this.totalWeeks - 1) | 0)) {
                    this.AddMessage("Programming Error! 'GetWeek' Week must be in the range 0-16.");
                    return null;
                }

                var sb = new System.Text.StringBuilder("", 168);
                var gamesInWeek = this.GetGamesInWeek(week);
                var prevGames = 0;
                for (var i = 0; i < week; i = (i + 1) | 0) {
                    prevGames = (prevGames + (this.GetGamesInWeek(i))) | 0;
                }
                var gameLocation = (this.weekOneStartLoc + (Bridge.Int.mul(2, prevGames))) | 0;
                for (var i1 = 0; i1 < gamesInWeek; i1 = (i1 + 1) | 0) {
                    sb.append(System.String.format("{0}", [this.GetGame(((gameLocation + (Bridge.Int.mul(2, i1))) | 0))]));
                }
                return sb.toString();
            },
            /**
             * Returns
             *
             * @instance
             * @public
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @param   {number}    romLocation
             * @return  {string}
             */
            GetGame: function (romLocation) {
                if (romLocation < this.weekOneStartLoc) {
                    this.AddMessage(System.String.format("Programming ERROR! GetGame Invalid Game Location '0x{0}'. Valid locations are 0x{1}-0x{2}.", Bridge.box(romLocation, System.Int32), Bridge.box(this.weekOneStartLoc, System.Int32), Bridge.box(((this.weekOneStartLoc + 448) | 0), System.Int32)));
                    return null;
                }
                var away = this.outputRom[System.Array.index(romLocation, this.outputRom)];
                var home = this.outputRom[System.Array.index(((romLocation + 1) | 0), this.outputRom)];

                var awayTeam = TSBTool.TecmoTool.GetTeamFromIndex(away);
                var homeTeam = TSBTool.TecmoTool.GetTeamFromIndex(home);

                var ret = System.String.format("{0} at {1}\n", awayTeam, homeTeam);
                return ret;
            },
            /**
             * Returns the number of games in the given week.
             *
             * @instance
             * @public
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @param   {number}    week
             * @return  {number}
             */
            GetGamesInWeek: function (week) {
                if (week < 0 || week > ((this.totalWeeks - 1) | 0)) {
                    this.AddMessage(System.String.format("Programming Error! GetGamesInWeek Week {0} is invalid. Week range = 0-16.", [Bridge.box(week, System.Int32)]));
                    return -1;
                }
                var result = this.outputRom[System.Array.index(((this.gamesPerWeekStartLoc + week) | 0), this.outputRom)];
                return result;
            },
            /**
             * Set a geme in a week.
             To be called by the user.
             *
             * @instance
             * @public
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @param   {number}     week        
             * @param   {number}     game        
             * @param   {string}     awayTeam    
             * @param   {string}     homeTeam
             * @return  {boolean}
             */
            SetGame: function (week, game, awayTeam, homeTeam) {
                if (week < 1 || week > this.totalWeeks) {
                    this.AddMessage("Error! valid week range is 1-17.");
                    return false;
                }
                week = (week - 1) | 0;
                var gamesInweek = this.GetGamesInWeek(week);
                if (game > gamesInweek || game < 1) {
                    this.AddMessage(System.String.format("Error! Game Number invalid. Current range for week {0} is 1 - {1}", Bridge.box(((week + 1) | 0), System.Int32), Bridge.box(gamesInweek, System.Int32)));
                    return false;
                }
                var awayIndex = TSBTool.TecmoTool.GetTeamIndex(awayTeam);
                var homeIndex = TSBTool.TecmoTool.GetTeamIndex(homeTeam);

                if (awayIndex < 0 || homeIndex < 0) {
                    this.AddMessage(System.String.format("Error! Team name invalid. Couldn't schedule game '{0} at {1}'", awayTeam, homeTeam));
                    return false;
                }
                var pointerLocation = (this.weekPointersStartLoc + (Bridge.Int.mul(2, week))) | 0;
                return false;
            },
            IncrementTeamGames: function (teamIndex) {
                if (this.teamGames == null) {
                    this.teamGames = System.Array.init(TSBTool.TecmoTool.Teams.Count, 0, System.Int32);
                }
                this.teamGames[System.Array.index(teamIndex, this.teamGames)] = (this.teamGames[System.Array.index(teamIndex, this.teamGames)] + 1) | 0;
            },
            /**
             * Returns an arraylist of error messages encountered when processing the schedule data.
             *
             * @instance
             * @public
             * @this TSBTool.ScheduleHelper2
             * @memberof TSBTool.ScheduleHelper2
             * @return  {System.Collections.Generic.List$1}
             */
            GetErrorMessages: function () {
                return this.messages;
            }
        }
    });

    Bridge.define("TSBTool.ITecmoContent", {
        $kind: "interface"
    });

    Bridge.define("TSBTool.ITecmoTool", {
        $kind: "interface"
    });

    Bridge.define("TSBTool.IAllStarPlayerControl", {
        $kind: "interface"
    });

    /**
     * Summary description for InputParser.
     *
     * @public
     * @class TSBTool.InputParser
     */
    Bridge.define("TSBTool.InputParser", {
        statics: {
            fields: {
                scheduleState: 0,
                rosterState: 0,
                teamRegex: null,
                weekRegex: null,
                gameRegex: null,
                numberRegex: null,
                posNameFaceRegex: null,
                simDataRegex: null,
                yearRegex: null,
                setRegex: null,
                returnTeamRegex: null,
                offensiveFormationRegex: null,
                playbookRegex: null,
                juiceRegex: null,
                homeRegex: null,
                awayRegex: null,
                divChampRegex: null,
                confChampRegex: null,
                uniformUsageRegex: null,
                replaceStringRegex: null,
                teamStringsRegex: null,
                KickRetMan: null,
                PuntRetMan: null
            },
            ctors: {
                init: function () {
                    this.scheduleState = 0;
                    this.rosterState = 1;
                    this.KickRetMan = new System.Text.RegularExpressions.Regex.ctor("^KR\\s*,\\s*([A-Z1-4]+)$");
                    this.PuntRetMan = new System.Text.RegularExpressions.Regex.ctor("^PR\\s*,\\s*([A-Z1-4]+)$");
                }
            },
            methods: {
                Init: function () {
                    if (TSBTool.InputParser.numberRegex == null) {
                        TSBTool.InputParser.numberRegex = new System.Text.RegularExpressions.Regex.ctor("(#[0-9]{1,2})");
                        TSBTool.InputParser.teamRegex = new System.Text.RegularExpressions.Regex.ctor("TEAM\\s*=\\s*([0-9a-z]+)");
                        TSBTool.InputParser.simDataRegex = new System.Text.RegularExpressions.Regex.ctor("SimData=0[xX]([0-9a-fA-F][0-9a-fA-F])([0-3]?)");
                        TSBTool.InputParser.weekRegex = new System.Text.RegularExpressions.Regex.ctor("WEEK ([1-9][0\t-7]?)");
                        TSBTool.InputParser.gameRegex = new System.Text.RegularExpressions.Regex.ctor("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");
                        TSBTool.InputParser.posNameFaceRegex = new System.Text.RegularExpressions.Regex.ctor("([A-Z]+[1-4]?)\\s*,\\s*([a-zA-Z \\.\\-]+),\\s*(face=0[xX][0-9a-fA-F]+\\s*,\\s*)?");
                        TSBTool.InputParser.yearRegex = new System.Text.RegularExpressions.Regex.ctor("YEAR\\s*=\\s*([0-9]+)");
                        TSBTool.InputParser.returnTeamRegex = new System.Text.RegularExpressions.Regex.ctor("RETURN_TEAM\\s+([A-Z1-4]+)\\s*,\\s*([A-Z1-4]+)\\s*,\\s*([A-Z1-4]+)");
                        TSBTool.InputParser.setRegex = new System.Text.RegularExpressions.Regex.ctor("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
                        TSBTool.InputParser.offensiveFormationRegex = new System.Text.RegularExpressions.Regex.ctor("OFFENSIVE_FORMATION\\s*=\\s*([a-zA-Z1234_]+)");
                        TSBTool.InputParser.playbookRegex = new System.Text.RegularExpressions.Regex.ctor("PLAYBOOK (R[1-8]{4})\\s*,\\s*(P[1-8]{4})");
                        TSBTool.InputParser.juiceRegex = new System.Text.RegularExpressions.Regex.ctor("JUICE\\(\\s*([0-9]{1,2}|ALL)\\s*,\\s*([0-9]{1,2})\\s*\\)");
                        TSBTool.InputParser.homeRegex = new System.Text.RegularExpressions.Regex.ctor("Uniform1\\s*=\\s*0x([0-9a-fA-F]{6})");
                        TSBTool.InputParser.awayRegex = new System.Text.RegularExpressions.Regex.ctor("Uniform2\\s*=\\s*0x([0-9a-fA-F]{6})");
                        TSBTool.InputParser.divChampRegex = new System.Text.RegularExpressions.Regex.ctor("DivChamp\\s*=\\s*0x([0-9a-fA-F]{10})");
                        TSBTool.InputParser.confChampRegex = new System.Text.RegularExpressions.Regex.ctor("ConfChamp\\s*=\\s*0x([0-9a-fA-F]{8})");
                        TSBTool.InputParser.uniformUsageRegex = new System.Text.RegularExpressions.Regex.ctor("UniformUsage\\s*=\\s*0x([0-9a-fA-F]{8})");
                        TSBTool.InputParser.replaceStringRegex = new System.Text.RegularExpressions.Regex.ctor("ReplaceString\\(\\s*\"([A-Za-z0-9 .]+)\"\\s*,\\s*\"([A-Za-z .]+)\"\\s*(,\\s*([0-9]+))*\\s*\\)");
                        TSBTool.InputParser.teamStringsRegex = new System.Text.RegularExpressions.Regex.ctor("TEAM_ABB=([0-9A-Z. ]+),TEAM_CITY=([0-9A-Za-z .]+),TEAM_NAME=([0-9A-Z .]+)");
                    }
                },
                CheckTextForRedundentSetCommands: function (input) {
                    var ret = new System.Text.StringBuilder();
                    var simpleSetRegex = new System.Text.RegularExpressions.Regex.ctor("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
                    var mc = simpleSetRegex.matches(input);
                    var current = null;
                    var m = null;
                    var location1 = System.Int64(0);
                    var location2 = System.Int64(0);
                    var valueLength1 = 0;
                    var valueLength2 = 0;
                    for (var i = 0; i < mc.getCount(); i = (i + 1) | 0) {
                        current = mc.get(i);
                        location1 = TSBTool.StaticUtils.ParseLongFromHexString(current.getGroups().get(1).toString().substr(2));
                        valueLength1 = (Bridge.Int.div(current.getGroups().get(2).getLength(), 2)) | 0;
                        for (var j = (i + 1) | 0; j < mc.getCount(); j = (j + 1) | 0) {
                            m = mc.get(j);
                            location2 = TSBTool.StaticUtils.ParseLongFromHexString(m.getGroups().get(1).toString().substr(2));
                            valueLength2 = (Bridge.Int.div(m.getGroups().get(2).getLength(), 2)) | 0;
                            if ((location2.gte(location1) && location2.lte(location1.add(System.Int64((((valueLength1 - 2) | 0)))))) || (location1.gte(location2) && location1.lte(location2.add(System.Int64((((valueLength2 - 2) | 0))))))) {
                                if (!Bridge.referenceEquals(current.getGroups().get(0).toString(), m.getGroups().get(0).toString())) {
                                    ret.append("WARNING!\n 'SET' Commands modify same locations '");
                                    ret.append(current.getGroups().get(0));
                                    ret.append("' and '");
                                    ret.append(m.getGroups().get(0));
                                    ret.append("'\n");
                                }
                            }
                        }
                    }
                    return ret.toString();
                },
                GetInts: function (input) {
                    return TSBTool.InputParser.GetInts$1(input, false);
                },
                GetInts$1: function (input, useHex) {
                    if (input != null) {
                        var pound = System.String.indexOf(input, "#");
                        var brace = System.String.indexOf(input, "[");
                        if (pound > -1) {
                            input = input.substr(((pound + 3) | 0));
                        }
                        if (brace > -1) {
                            brace = System.String.indexOf(input, "[");
                            input = input.substr(0, brace);
                        }
                        var seps = System.Array.init([32, 44, 9], System.Char);
                        var nums = System.String.split(input, seps.map(function (i) {{ return String.fromCharCode(i); }}));
                        var j, count = 0;
                        for (j = 0; j < nums.length; j = (j + 1) | 0) {
                            if (nums[System.Array.index(j, nums)].length > 0) {
                                count = (count + 1) | 0;
                            }
                        }
                        var result = System.Array.init(count, 0, System.Int32);
                        j = 0;

                        var s = "";
                        var i = 0;
                        try {
                            for (i = 0; i < nums.length; i = (i + 1) | 0) {
                                s = nums[System.Array.index(i, nums)];
                                if (s != null && s.length > 0) {
                                    if (useHex) {
                                        result[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), result)] = TSBTool.StaticUtils.ParseIntFromHexString(s);
                                    } else {
                                        result[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), result)] = System.Int32.parse(s);
                                    }
                                }
                            }
                            return result;
                        } catch (e) {
                            e = System.Exception.create(e);
                            var error = System.String.format("Error with input '{0}', {1}, was jersey number specified?", input, e.Message);
                            TSBTool.StaticUtils.AddError(error);
                        }
                    }
                    return null;
                },
                GetJerseyNumber: function (line) {
                    var ret = -1;
                    var jerseyRegex = new System.Text.RegularExpressions.Regex.ctor("#([0-9]+)");
                    var num = jerseyRegex.match(line).getGroups().get(1).toString();
                    try {
                        ret = TSBTool.StaticUtils.ParseIntFromHexString(num);
                    } catch ($e1) {
                        $e1 = System.Exception.create($e1);
                        ret = -1;
                    }
                    return ret;
                },
                GetFace: function (line) {
                    var ret = -1;
                    var hexRegex = new System.Text.RegularExpressions.Regex.ctor("0[xX]([A-Fa-f0-9]+)");
                    var m = hexRegex.match(line);
                    if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        var num = m.getGroups().get(1).toString();
                        try {
                            ret = TSBTool.StaticUtils.ParseIntFromHexString(num);
                        } catch ($e1) {
                            $e1 = System.Exception.create($e1);
                            ret = -1;
                            TSBTool.StaticUtils.AddError(System.String.format("Face ERROR line '{0}'", [line]));
                        }
                    }

                    return ret;
                },
                GetLastName: function (line) {
                    var ret = "";
                    var m = TSBTool.InputParser.posNameFaceRegex.match(line);
                    if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        var name = m.getGroups().get(2).toString().trim();
                        var index = name.lastIndexOf(" ");
                        ret = name.substr(((index + 1) | 0));
                    }
                    return ret;
                },
                GetFirstName: function (line) {
                    var ret = "";
                    var m = TSBTool.InputParser.posNameFaceRegex.match(line);
                    if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        var name = m.getGroups().get(2).toString().trim();
                        var index = name.lastIndexOf(" ");
                        if (index > -1 && index < name.length) {
                            ret = name.substr(0, index);
                        }
                    }
                    return ret;
                },
                /**
                 * @static
                 * @public
                 * @this TSBTool.InputParser
                 * @memberof TSBTool.InputParser
                 * @param   {string}            byteString    String in the format of a hex string (0123456789ABCDEF), must have
                 an even number of characters.
                 * @return  {Array.<number>}                  The bytes.
                 */
                GetBytesFromString: function (byteString) {
                    var ret = null;
                    var tmp = null;
                    var b;
                    if (byteString != null && byteString.length > 1 && (byteString.length % 2) === 0) {
                        tmp = System.Array.init(((Bridge.Int.div(byteString.length, 2)) | 0), 0, System.Byte);
                        for (var i = 0; i < tmp.length; i = (i + 1) | 0) {
                            b = byteString.substr(Bridge.Int.mul(i, 2), 2);
                            tmp[System.Array.index(i, tmp)] = TSBTool.StaticUtils.ParseByteFromHexString(b);
                        }
                        ret = tmp;
                    }
                    return ret;
                },
                GetHomeUniformColorString: function (line) {
                    TSBTool.InputParser.Init();
                    var tmp = "";
                    var match = TSBTool.InputParser.homeRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                GetAwayUniformColorString: function (line) {
                    TSBTool.InputParser.Init();
                    var tmp = "";
                    var match = TSBTool.InputParser.awayRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                GetConfChampColorString: function (line) {
                    TSBTool.InputParser.Init();
                    var tmp = "";
                    var match = TSBTool.InputParser.confChampRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                GetDivChampColorString: function (line) {
                    TSBTool.InputParser.Init();
                    var tmp = "";
                    var match = TSBTool.InputParser.divChampRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                GetUniformUsageString: function (line) {
                    TSBTool.InputParser.Init();
                    var tmp = "";
                    var match = TSBTool.InputParser.uniformUsageRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                /**
                 * Returns the text string passed, without thr trailing commas.
                 *
                 * @static
                 * @public
                 * @this TSBTool.InputParser
                 * @memberof TSBTool.InputParser
                 * @param   {string}    text
                 * @return  {string}
                 */
                DeleteTrailingCommas: function (text) {
                    var rs = new System.Text.RegularExpressions.Regex.ctor(",+\n");
                    var rrs = new System.Text.RegularExpressions.Regex.ctor(",+$");
                    var ret = rs.replace(text, "\n");
                    ret = rrs.replace(ret, "");

                    return ret;
                }
            }
        },
        fields: {
            tool: null,
            currentState: 0,
            showSimError: false,
            currentTeam: null,
            scheduleList: null
        },
        ctors: {
            init: function () {
                this.currentState = 2;
                this.showSimError = false;
            },
            $ctor1: function (tool) {
                this.$initialize();
                this.tool = tool;
                this.currentTeam = "bills";
                TSBTool.InputParser.Init();
            },
            ctor: function () {
                this.$initialize();
                this.currentTeam = "bills";
                TSBTool.InputParser.Init();
            }
        },
        methods: {
            ProcessFile: function (fileName) {
                try {
                    var sr = new System.IO.StreamReader.$ctor7(fileName);
                    var contents = sr.ReadToEnd();
                    sr.Close();
                    var chars = System.String.toCharArray(("\n\r"), 0, ("\n\r").length);
                    var lines = System.String.split(contents, chars.map(function (i) {{ return String.fromCharCode(i); }}));
                    this.ProcessLines(lines);
                } catch (e) {
                    e = System.Exception.create(e);
                    TSBTool.StaticUtils.ShowError(e.Message);
                }
            },
            ProcessLines: function (lines) {
                var i = 0;
                try {
                    for (i = 0; i < lines.length; i = (i + 1) | 0) {
                        this.ProcessLine(lines[System.Array.index(i, lines)]);
                    }
                    TSBTool.StaticUtils.ShowErrors();
                    this.ApplySchedule();
                } catch (e) {
                    e = System.Exception.create(e);
                    var sb = new System.Text.StringBuilder("", 150);
                    sb.append("Error! ");
                    if (i < lines.length) {
                        sb.append(System.String.format("line #{0}:\t'{1}'", Bridge.box(i, System.Int32), lines[System.Array.index(i, lines)]));
                    }
                    sb.append(e.Message);
                    sb.append("\n");
                    sb.append(e.StackTrace);
                    sb.append("\n\nOperation aborted at this point. Data not applied.");
                    TSBTool.StaticUtils.ShowError(sb.toString());
                }
            },
            ApplySchedule: function () {
                if (this.scheduleList != null) {
                    this.tool.TSBTool$ITecmoTool$ApplySchedule(this.scheduleList);
                    TSBTool.StaticUtils.ShowErrors();
                    this.scheduleList = null;
                }
            },
            ReadFromStdin: function () {
                var line = "";
                var lineNumber = 0;
                System.Console.WriteLine("Reading from standard in...");
                try {
                    while (((line = prompt())) != null) {
                        lineNumber = (lineNumber + 1) | 0;
                        this.ProcessLine(line);
                    }
                    TSBTool.StaticUtils.ShowErrors();
                    this.ApplySchedule();
                } catch (e) {
                    e = System.Exception.create(e);
                    TSBTool.StaticUtils.ShowError(System.String.format("Error Processing line {0}:'{1}'.\n{2}\n{3}", Bridge.box(lineNumber, System.Int32), line, e.Message, e.StackTrace));
                }
            },
            /**
             * @instance
             * @protected
             * @this TSBTool.InputParser
             * @memberof TSBTool.InputParser
             * @param   {string}    line
             * @return  {void}
             */
            ProcessLine: function (line) {
                line = line.trim();
                var m;

                if (System.String.startsWith(line, "#") || Bridge.referenceEquals(line, "") || System.String.startsWith(line.toLowerCase().trim(), "schedule")) {
                    return;
                } else {
                    if (System.String.startsWith(line, "SET")) {
                        this.tool.TSBTool$ITecmoTool$ApplySet(line);
                    } else if (!Bridge.referenceEquals(((m = TSBTool.InputParser.playbookRegex.match(line))), System.Text.RegularExpressions.Match.getEmpty())) {
                        if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                            var runs = m.getGroups().get(1).toString();
                            var passes = m.getGroups().get(2).toString();
                            this.tool.TSBTool$ITecmoTool$SetPlaybook(this.currentTeam, runs, passes);
                        } else {
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR Setting playbook for team {0}. Line '{1}' is Invalid", this.currentTeam, line));
                        }
                    } else if (!Bridge.referenceEquals(((m = TSBTool.InputParser.juiceRegex.match(line))), System.Text.RegularExpressions.Match.getEmpty())) {
                        var juiceWeek = m.getGroups().get(1).toString();
                        var juiceAmt = System.Int32.parse(m.getGroups().get(2).toString());

                        if (Bridge.referenceEquals(juiceWeek, "ALL")) {
                            for (var i = 0; i < 17; i = (i + 1) | 0) {
                                this.tool.TSBTool$ITecmoTool$ApplyJuice(((i + 1) | 0), juiceAmt);
                            }
                        } else {
                            var week = (System.Int32.parse(juiceWeek) - 1) | 0;
                            if (!this.tool.TSBTool$ITecmoTool$ApplyJuice(week, juiceAmt)) {
                                TSBTool.StaticUtils.AddError(System.String.format("ERROR! Line = '{0}'", [line]));
                            }
                        }
                    } else if (!Bridge.referenceEquals(((m = TSBTool.InputParser.replaceStringRegex.match(line))), System.Text.RegularExpressions.Match.getEmpty())) {
                        var find = "";
                        var replace = "";
                        var occur = { v : -1 };
                        if (m.getGroups().getCount() > 1) {
                            find = m.getGroups().get(1).toString();
                            replace = m.getGroups().get(2).toString();
                            if (m.getGroups().getCount() > 3) {
                                System.Int32.tryParse(m.getGroups().get(4).toString(), occur);
                                occur.v = (occur.v - 1) | 0;
                            }
                            var msg = TSBTool.StaticUtils.ReplaceStringInRom(this.tool.TSBTool$ITecmoTool$OutputRom, find, replace, occur.v);
                            if (System.String.startsWith(msg, "Error")) {
                                TSBTool.StaticUtils.AddError(msg);
                            } else {
                                System.Console.WriteLine(msg);
                            }
                        } else {
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR! Not enough info to use 'ReplaceString' function.Line={0}", [line]));
                        }
                    } else if (!Bridge.referenceEquals(((m = TSBTool.InputParser.teamStringsRegex.match(line))), System.Text.RegularExpressions.Match.getEmpty())) {
                        var teamAbb = m.getGroups().get(1).toString();
                        var teamCity = m.getGroups().get(2).toString();
                        var teamName = m.getGroups().get(3).toString();
                        var index = TSBTool.TecmoTool.GetTeamIndex(this.currentTeam);
                        this.tool.TSBTool$ITecmoTool$SetTeamAbbreviation(index, teamAbb);
                        this.tool.TSBTool$ITecmoTool$SetTeamCity(index, teamCity);
                        this.tool.TSBTool$ITecmoTool$SetTeamName(index, teamName);
                    } else if (System.String.startsWith(line, "COLORS")) {
                        var tmp;

                        var home = TSBTool.InputParser.homeRegex.match(line);
                        var away = TSBTool.InputParser.awayRegex.match(line);
                        var confChamp = TSBTool.InputParser.confChampRegex.match(line);
                        var divChamp = TSBTool.InputParser.divChampRegex.match(line);
                        var uniUsage = TSBTool.InputParser.uniformUsageRegex.match(line);
                        if (!Bridge.referenceEquals(home, System.Text.RegularExpressions.Match.getEmpty())) {
                            tmp = home.getGroups().get(1).getValue();
                            this.tool.TSBTool$ITecmoTool$SetHomeUniform(this.currentTeam, tmp);
                        }
                        if (!Bridge.referenceEquals(away, System.Text.RegularExpressions.Match.getEmpty())) {
                            tmp = away.getGroups().get(1).getValue();
                            this.tool.TSBTool$ITecmoTool$SetAwayUniform(this.currentTeam, tmp);
                        }
                        if (!Bridge.referenceEquals(confChamp, System.Text.RegularExpressions.Match.getEmpty())) {
                            tmp = confChamp.getGroups().get(1).getValue();
                            this.tool.TSBTool$ITecmoTool$SetConfChampColors(this.currentTeam, tmp);
                        }
                        if (!Bridge.referenceEquals(divChamp, System.Text.RegularExpressions.Match.getEmpty())) {
                            tmp = divChamp.getGroups().get(1).getValue();
                            this.tool.TSBTool$ITecmoTool$SetDivChampColors(this.currentTeam, tmp);
                        }
                        if (!Bridge.referenceEquals(uniUsage, System.Text.RegularExpressions.Match.getEmpty())) {
                            tmp = uniUsage.getGroups().get(1).getValue();
                            this.tool.TSBTool$ITecmoTool$SetUniformUsage(this.currentTeam, tmp);
                        }
                    } else if (!Bridge.referenceEquals(TSBTool.InputParser.teamRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                        System.Console.WriteLine(System.String.format("'{0}' ", line));
                        this.currentState = TSBTool.InputParser.rosterState;
                        var team = this.GetTeam(line);
                        var ret = this.SetCurrentTeam(team);
                        if (!ret) {
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR with line '{0}'.", [line]));
                            TSBTool.StaticUtils.AddError(System.String.format("Team input must be in the form 'TEAM = team SimData=0x1F'", null));
                            return;
                        }
                        var simData = this.GetSimData(line);
                        if (simData != null) {
                            if (simData[System.Array.index(0, simData)] > -1) {
                                this.tool.TSBTool$ITecmoTool$SetTeamSimData(this.currentTeam, ((simData[System.Array.index(0, simData)]) & 255));
                            } else {
                                TSBTool.StaticUtils.AddError(System.String.format("Warning: No sim data for team {0}", [team]));
                            }

                            if (simData[System.Array.index(1, simData)] > -1) {
                                this.tool.TSBTool$ITecmoTool$SetTeamSimOffensePref(this.currentTeam, simData[System.Array.index(1, simData)]);
                            }
                        } else {
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR with line '{0}'.", [line]));
                        }

                        var oFormMatch = TSBTool.InputParser.offensiveFormationRegex.match(line);
                        if (!Bridge.referenceEquals(oFormMatch, System.Text.RegularExpressions.Match.getEmpty())) {
                            var formation = oFormMatch.getGroups().get(1).toString();
                            this.tool.TSBTool$ITecmoTool$SetTeamOffensiveFormation(team, formation);
                        }
                    } else if (!Bridge.referenceEquals(TSBTool.InputParser.weekRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                        this.currentState = TSBTool.InputParser.scheduleState;
                        if (this.scheduleList == null) {
                            this.scheduleList = new (System.Collections.Generic.List$1(System.String)).$ctor2(300);
                        }
                        this.scheduleList.add(line);
                    } else if (!Bridge.referenceEquals(TSBTool.InputParser.yearRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                        this.SetYear(line);
                    } else if (System.String.startsWith(line, "AFC") || System.String.startsWith(line, "NFC")) {
                        var parts = System.String.split(System.String.replaceAll(line, " ", ""), System.Array.init([44], System.Char).map(function (i) {{ return String.fromCharCode(i); }}), null, 1);
                        if (parts != null && parts.length > 3) {
                            try {
                                this.tool.TSBTool$ITecmoTool$SetProBowlPlayer(System.Nullable.getValue(Bridge.cast(Bridge.unbox(System.Enum.parse(TSBTool.Conference, parts[System.Array.index(0, parts)]), TSBTool.Conference), System.Int32)), parts[System.Array.index(1, parts)], parts[System.Array.index(2, parts)], System.Nullable.getValue(Bridge.cast(Bridge.unbox(System.Enum.parse(TSBTool.TSBPlayer, parts[System.Array.index(3, parts)]), TSBTool.TSBPlayer), System.Int32)));
                            } catch ($e1) {
                                $e1 = System.Exception.create($e1);
                                TSBTool.StaticUtils.AddError("Error processing line > " + (line || ""));
                            }
                        }
                    } else if (this.currentState === TSBTool.InputParser.scheduleState) {
                        if (this.scheduleList != null) {
                            this.scheduleList.add(line);
                        }
                    } else if (this.currentState === TSBTool.InputParser.rosterState) {
                        this.UpdateRoster(line);
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("Garbage/orphin line not applied \"{0}\"", [line]));
                    }
                }
            },
            SetYear: function (line) {
                var m = TSBTool.InputParser.yearRegex.match(line);
                var year = m.getGroups().get(1).toString();
                if (year.length < 1) {
                    TSBTool.StaticUtils.AddError(System.String.format("'{0}' is not valid.", [line]));
                } else {
                    this.tool.TSBTool$ITecmoTool$SetYear(year);
                    System.Console.WriteLine(System.String.format("Year set to '{0}'", year));
                }
            },
            GetTeam: function (line) {
                var m = TSBTool.InputParser.teamRegex.match(line);
                var team = m.getGroups().get(1).toString();
                return team;
            },
            GetSimData: function (line) {
                var m = TSBTool.InputParser.simDataRegex.match(line);
                var data = m.getGroups().get(1).toString();
                var simOffensePref = m.getGroups().get(2).toString();
                var ret = System.Array.init([-1, -1], System.Int32);

                if (data.length > 0) {
                    try {
                        var simData = TSBTool.StaticUtils.ParseIntFromHexString(data);
                        ret[System.Array.index(0, ret)] = simData;
                    } catch ($e1) {
                        $e1 = System.Exception.create($e1);
                        TSBTool.StaticUtils.AddError(System.String.format("Error getting SimData with line '{0}'.", [line]));
                    }
                }

                if (simOffensePref.length > 0) {
                    try {
                        var so = System.Int32.parse(simOffensePref);
                        ret[System.Array.index(1, ret)] = so;
                    } catch ($e2) {
                        $e2 = System.Exception.create($e2);
                        TSBTool.StaticUtils.AddError(System.String.format("Error getting SimData with line '{0}'.", [line]));
                    }
                }
                return ret;
            },
            GetAwayTeam: function (line) {
                var m = TSBTool.InputParser.gameRegex.match(line);
                var awayTeam = m.getGroups().get(1).toString();
                return awayTeam;
            },
            GetHomeTeam: function (line) {
                var m = TSBTool.InputParser.gameRegex.match(line);
                var team = m.getGroups().get(2).toString();
                return team;
            },
            GetWeek: function (line) {
                var m = TSBTool.InputParser.weekRegex.match(line);
                var week_str = m.getGroups().get(1).toString();
                var ret = -1;
                try {
                    ret = System.Int32.parse(week_str);
                    ret = (ret - 1) | 0;
                } catch ($e1) {
                    $e1 = System.Exception.create($e1);
                    TSBTool.StaticUtils.AddError(System.String.format("Week '{0}' is invalid.", [week_str]));
                }
                return ret;
            },
            SetCurrentTeam: function (team) {
                if (TSBTool.TecmoTool.GetTeamIndex(team) < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("Team '{0}' is Invalid.", [team]));
                    return false;
                } else {
                    this.currentTeam = team;
                }
                return true;
            },
            UpdateRoster: function (line) {
                if (System.String.startsWith(line, "KR")) {
                    this.SetKickReturnMan(line);
                } else {
                    if (System.String.startsWith(line, "PR")) {
                        this.SetPuntReturnMan(line);
                    } else {
                        if (System.String.startsWith(line, "RETURN_TEAM")) {
                            var m = TSBTool.InputParser.returnTeamRegex.match(line);
                            if (Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                                TSBTool.StaticUtils.AddError(System.String.format("Error with line '{0}'.\n\tCorrect Syntax ='RETURN_TEAM POS1, POS2, POS3'", [line]));
                            } else {
                                var pos1 = m.getGroups().get(1).toString();
                                var pos2 = m.getGroups().get(2).toString();
                                var pos3 = m.getGroups().get(3).toString();
                                this.tool.TSBTool$ITecmoTool$SetReturnTeam(this.currentTeam, pos1, pos2, pos3);
                            }
                        } else {
                            var m1 = TSBTool.InputParser.posNameFaceRegex.match(line);
                            if (System.String.indexOf(line, "#") > -1) {
                                if (Bridge.referenceEquals(TSBTool.InputParser.numberRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (jersey number) Line  {0}", [line]));
                                    return;
                                }
                            }
                            var p = m1.getGroups().get(1).toString();
                            if (!Bridge.referenceEquals(m1, System.Text.RegularExpressions.Match.getEmpty()) && this.tool.TSBTool$ITecmoTool$IsValidPosition(p)) {
                                if (System.String.startsWith(line, "QB")) {
                                    this.SetQB(line);
                                } else {
                                    if (System.String.startsWith(line, "WR") || System.String.startsWith(line, "RB") || System.String.startsWith(line, "TE")) {
                                        this.SetSkillPlayer(line);
                                    } else {
                                        if (System.String.startsWith(line, "C") || System.String.startsWith(line, "RG") || System.String.startsWith(line, "LG") || System.String.startsWith(line, "RT") || System.String.startsWith(line, "LT")) {
                                            this.SetOLPlayer(line);
                                        } else if (System.String.indexOf(line, "LB") === 2 || System.String.indexOf(line, "CB") === 1 || System.String.startsWith(line, "RE") || System.String.startsWith(line, "LE") || System.String.startsWith(line, "NT") || System.String.startsWith(line, "SS") || System.String.startsWith(line, "FS") || System.String.startsWith(line, "DB")) {
                                            this.SetDefensivePlayer(line);
                                        } else if (System.String.startsWith(line, "P") || System.String.startsWith(line, "K")) {
                                            this.SetKickPlayer(line);
                                        }
                                    }
                                }
                            } else {
                                TSBTool.StaticUtils.AddError(System.String.format("ERROR! With line \"{0}\"     team = {1}", line, this.currentTeam));
                            }
                        }
                    }
                }
            },
            SetQB: function (line) {
                var fname = TSBTool.InputParser.GetFirstName(line);
                var lname = TSBTool.InputParser.GetLastName(line);
                var pos = this.GetPosition(line);
                var face = TSBTool.InputParser.GetFace(line);
                var jerseyNumber = TSBTool.InputParser.GetJerseyNumber(line);
                if (face > -1) {
                    this.tool.TSBTool$ITecmoTool$SetFace(this.currentTeam, pos, face);
                }
                if (jerseyNumber < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error with jersey number for '{0} {1}', setting to 0.", fname, lname));
                    jerseyNumber = 0;
                }
                this.tool.TSBTool$ITecmoTool$InsertPlayer(this.currentTeam, pos, fname, lname, (jerseyNumber & 255));

                var vals = TSBTool.InputParser.GetInts(line);
                var simVals = this.GetSimVals(line);
                if (vals != null && vals.length > 7) {
                    this.tool.TSBTool$ITecmoTool$SetQBAbilities(this.currentTeam, pos, vals[System.Array.index(0, vals)], vals[System.Array.index(1, vals)], vals[System.Array.index(2, vals)], vals[System.Array.index(3, vals)], vals[System.Array.index(4, vals)], vals[System.Array.index(5, vals)], vals[System.Array.index(6, vals)], vals[System.Array.index(7, vals)]);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! could not set ability data for {0} {1},", this.currentTeam, pos));
                }
                if (face > -1) {
                    this.tool.TSBTool$ITecmoTool$SetFace(this.currentTeam, pos, face);
                }
                if (simVals != null) {
                    this.tool.TSBTool$ITecmoTool$SetQBSimData(this.currentTeam, pos, simVals);
                } else {
                    if (this.showSimError) {
                        TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No sim data specified.", [line]));
                    }
                }
            },
            SetSkillPlayer: function (line) {
                var fname = TSBTool.InputParser.GetFirstName(line);
                var lname = TSBTool.InputParser.GetLastName(line);
                var pos = this.GetPosition(line);
                var face = TSBTool.InputParser.GetFace(line);
                var jerseyNumber = TSBTool.InputParser.GetJerseyNumber(line);
                this.tool.TSBTool$ITecmoTool$SetFace(this.currentTeam, pos, face);
                this.tool.TSBTool$ITecmoTool$InsertPlayer(this.currentTeam, pos, fname, lname, (jerseyNumber & 255));

                var vals = TSBTool.InputParser.GetInts(line);
                var simVals = this.GetSimVals(line);
                if (vals != null && vals.length > 5) {
                    this.tool.TSBTool$ITecmoTool$SetSkillPlayerAbilities(this.currentTeam, pos, vals[System.Array.index(0, vals)], vals[System.Array.index(1, vals)], vals[System.Array.index(2, vals)], vals[System.Array.index(3, vals)], vals[System.Array.index(4, vals)], vals[System.Array.index(5, vals)]);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No player data specified.", [line]));
                }
                if (simVals != null && simVals.length > 3) {
                    this.tool.TSBTool$ITecmoTool$SetSkillSimData(this.currentTeam, pos, simVals);
                } else {
                    if (this.showSimError) {
                        TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No sim data specified.", [line]));
                    }
                }
            },
            SetOLPlayer: function (line) {
                var fname = TSBTool.InputParser.GetFirstName(line);
                var lname = TSBTool.InputParser.GetLastName(line);
                var pos = this.GetPosition(line);
                var face = TSBTool.InputParser.GetFace(line);
                var jerseyNumber = TSBTool.InputParser.GetJerseyNumber(line);
                var vals = TSBTool.InputParser.GetInts(line);

                this.tool.TSBTool$ITecmoTool$SetFace(this.currentTeam, pos, face);
                this.tool.TSBTool$ITecmoTool$InsertPlayer(this.currentTeam, pos, fname, lname, (jerseyNumber & 255));

                if (vals != null && vals.length > 3) {
                    this.tool.TSBTool$ITecmoTool$SetOLPlayerAbilities(this.currentTeam, pos, vals[System.Array.index(0, vals)], vals[System.Array.index(1, vals)], vals[System.Array.index(2, vals)], vals[System.Array.index(3, vals)]);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No player data specified.", [line]));
                }

            },
            SetDefensivePlayer: function (line) {
                var fname = TSBTool.InputParser.GetFirstName(line);
                var lname = TSBTool.InputParser.GetLastName(line);
                var pos = this.GetPosition(line);
                var face = TSBTool.InputParser.GetFace(line);
                var jerseyNumber = TSBTool.InputParser.GetJerseyNumber(line);
                var vals = TSBTool.InputParser.GetInts(line);
                var simVals = this.GetSimVals(line);

                this.tool.TSBTool$ITecmoTool$SetFace(this.currentTeam, pos, face);
                this.tool.TSBTool$ITecmoTool$InsertPlayer(this.currentTeam, pos, fname, lname, (jerseyNumber & 255));

                if (vals != null && vals.length > 5) {
                    this.tool.TSBTool$ITecmoTool$SetDefensivePlayerAbilities(this.currentTeam, pos, vals[System.Array.index(0, vals)], vals[System.Array.index(1, vals)], vals[System.Array.index(2, vals)], vals[System.Array.index(3, vals)], vals[System.Array.index(4, vals)], vals[System.Array.index(5, vals)]);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. Invalid player attributes.", [line]));
                }
                if (simVals != null && simVals.length > 1) {
                    this.tool.TSBTool$ITecmoTool$SetDefensiveSimData(this.currentTeam, pos, simVals);
                } else {
                    if (this.showSimError) {
                        TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No sim data specified.", [line]));
                    }
                }
            },
            SetKickPlayer: function (line) {
                var fname = TSBTool.InputParser.GetFirstName(line);
                var lname = TSBTool.InputParser.GetLastName(line);
                var pos = this.GetPosition(line);
                var face = TSBTool.InputParser.GetFace(line);
                var jerseyNumber = TSBTool.InputParser.GetJerseyNumber(line);
                var vals = TSBTool.InputParser.GetInts(line);
                var simVals = this.GetSimVals(line);

                this.tool.TSBTool$ITecmoTool$SetFace(this.currentTeam, pos, face);
                this.tool.TSBTool$ITecmoTool$InsertPlayer(this.currentTeam, pos, fname, lname, (jerseyNumber & 255));
                if (vals != null && vals.length > 5) {
                    this.tool.TSBTool$ITecmoTool$SetKickPlayerAbilities(this.currentTeam, pos, vals[System.Array.index(0, vals)], vals[System.Array.index(1, vals)], vals[System.Array.index(2, vals)], vals[System.Array.index(3, vals)], vals[System.Array.index(4, vals)], vals[System.Array.index(5, vals)]);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No player data specified.", [line]));
                }
                if (simVals != null && Bridge.referenceEquals(pos, "P")) {
                    this.tool.TSBTool$ITecmoTool$SetPuntingSimData(this.currentTeam, simVals[System.Array.index(0, simVals)]);
                } else {
                    if (simVals != null && Bridge.referenceEquals(pos, "K")) {
                        this.tool.TSBTool$ITecmoTool$SetKickingSimData(this.currentTeam, simVals[System.Array.index(0, simVals)]);
                    } else {
                        if (this.showSimError) {
                            TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No sim data specified.", [line]));
                        }
                    }
                }
            },
            SetKickReturnMan: function (line) {
                var m = TSBTool.InputParser.KickRetMan.match(line);
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    var pos = m.getGroups().get(1).toString();
                    if (this.tool.TSBTool$ITecmoTool$IsValidPosition(pos)) {
                        this.tool.TSBTool$ITecmoTool$SetKickReturner(this.currentTeam, pos);
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR with line '{0}'.", [line]));
                    }
                }
            },
            SetPuntReturnMan: function (line) {
                var m = TSBTool.InputParser.PuntRetMan.match(line);
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    var pos = m.getGroups().get(1).toString();
                    if (this.tool.TSBTool$ITecmoTool$IsValidPosition(pos)) {
                        this.tool.TSBTool$ITecmoTool$SetPuntReturner(this.currentTeam, pos);
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR with line '{0}'.", [line]));
                    }
                }
            },
            /**
             * Expect line like '   [8, 9, 0 ]'
             *
             * @instance
             * @public
             * @this TSBTool.InputParser
             * @memberof TSBTool.InputParser
             * @param   {string}            input
             * @return  {Array.<number>}
             */
            GetSimVals: function (input) {
                if (input != null) {
                    var stuff = input.trim();
                    var start = System.String.indexOf(stuff, "[");
                    var end = System.String.indexOf(stuff, "]");
                    if (start > -1 && end > -1) {
                        stuff = stuff.substr(((start + 1) | 0), ((((end - start) | 0) - 1) | 0));
                        return TSBTool.InputParser.GetInts(stuff);
                    }
                }
                return null;
            },
            GetPosition: function (line) {
                var pos = TSBTool.InputParser.posNameFaceRegex.match(line).getGroups().get(1).toString();
                return pos;
            }
        }
    });

    Bridge.define("TSBTool.MainClass", {
        statics: {
            fields: {
                GUI_MODE: false
            },
            ctors: {
                init: function () {
                    this.GUI_MODE = false;
                }
            }
        }
    });

    Bridge.define("TSBTool.ROM_TYPE", {
        $kind: "enum",
        statics: {
            fields: {
                NONE: 0,
                NES_ORIGINAL_TSB: 1,
                CXROM_v105: 2,
                CXROM_v111: 3,
                SNES_TSB1: 4,
                SNES_TSB2: 5,
                SNES_TSB3: 6,
                READ_ONLY_ERROR: 7
            }
        }
    });

    Bridge.define("TSBTool.SimAverages", {
        $kind: "struct",
        statics: {
            methods: {
                getDefaultValue: function () { return new TSBTool.SimAverages(); }
            }
        },
        fields: {
            QB_ave: 0,
            RB_ave: 0,
            WR_ave: 0,
            TE_ave: 0,
            OL_ave: 0,
            DL_run_ave: 0,
            DL_pass_ave: 0,
            LB_run_ave: 0,
            LB_pass_ave: 0,
            DB_run_ave: 0,
            DB_pass_ave: 0,
            TOTAL_RUN_DEFENSE: 0,
            TOTAL_PASS_DEFENSE: 0,
            MIN_RUN_DEFENSE: 0,
            MAX_RUN_DEFENSE: 0,
            MIN_PASS_DEFENSE: 0,
            MAX_PASS_DEFENSE: 0
        },
        ctors: {
            ctor: function () {
                this.$initialize();
            }
        },
        methods: {
            getHashCode: function () {
                var h = Bridge.addHash([2740139056, this.QB_ave, this.RB_ave, this.WR_ave, this.TE_ave, this.OL_ave, this.DL_run_ave, this.DL_pass_ave, this.LB_run_ave, this.LB_pass_ave, this.DB_run_ave, this.DB_pass_ave, this.TOTAL_RUN_DEFENSE, this.TOTAL_PASS_DEFENSE, this.MIN_RUN_DEFENSE, this.MAX_RUN_DEFENSE, this.MIN_PASS_DEFENSE, this.MAX_PASS_DEFENSE]);
                return h;
            },
            equals: function (o) {
                if (!Bridge.is(o, TSBTool.SimAverages)) {
                    return false;
                }
                return Bridge.equals(this.QB_ave, o.QB_ave) && Bridge.equals(this.RB_ave, o.RB_ave) && Bridge.equals(this.WR_ave, o.WR_ave) && Bridge.equals(this.TE_ave, o.TE_ave) && Bridge.equals(this.OL_ave, o.OL_ave) && Bridge.equals(this.DL_run_ave, o.DL_run_ave) && Bridge.equals(this.DL_pass_ave, o.DL_pass_ave) && Bridge.equals(this.LB_run_ave, o.LB_run_ave) && Bridge.equals(this.LB_pass_ave, o.LB_pass_ave) && Bridge.equals(this.DB_run_ave, o.DB_run_ave) && Bridge.equals(this.DB_pass_ave, o.DB_pass_ave) && Bridge.equals(this.TOTAL_RUN_DEFENSE, o.TOTAL_RUN_DEFENSE) && Bridge.equals(this.TOTAL_PASS_DEFENSE, o.TOTAL_PASS_DEFENSE) && Bridge.equals(this.MIN_RUN_DEFENSE, o.MIN_RUN_DEFENSE) && Bridge.equals(this.MAX_RUN_DEFENSE, o.MAX_RUN_DEFENSE) && Bridge.equals(this.MIN_PASS_DEFENSE, o.MIN_PASS_DEFENSE) && Bridge.equals(this.MAX_PASS_DEFENSE, o.MAX_PASS_DEFENSE);
            },
            $clone: function (to) {
                var s = to || new TSBTool.SimAverages();
                s.QB_ave = this.QB_ave;
                s.RB_ave = this.RB_ave;
                s.WR_ave = this.WR_ave;
                s.TE_ave = this.TE_ave;
                s.OL_ave = this.OL_ave;
                s.DL_run_ave = this.DL_run_ave;
                s.DL_pass_ave = this.DL_pass_ave;
                s.LB_run_ave = this.LB_run_ave;
                s.LB_pass_ave = this.LB_pass_ave;
                s.DB_run_ave = this.DB_run_ave;
                s.DB_pass_ave = this.DB_pass_ave;
                s.TOTAL_RUN_DEFENSE = this.TOTAL_RUN_DEFENSE;
                s.TOTAL_PASS_DEFENSE = this.TOTAL_PASS_DEFENSE;
                s.MIN_RUN_DEFENSE = this.MIN_RUN_DEFENSE;
                s.MAX_RUN_DEFENSE = this.MAX_RUN_DEFENSE;
                s.MIN_PASS_DEFENSE = this.MIN_PASS_DEFENSE;
                s.MAX_PASS_DEFENSE = this.MAX_PASS_DEFENSE;
                return s;
            }
        }
    });

    /**
     * Summary description for SimStuff.
     *
     * @public
     * @class TSBTool.SimStuff
     */
    Bridge.define("TSBTool.SimStuff", {
        statics: {
            fields: {
                FRONT_7_SIM_POINT_POOL: 0,
                FRONT_7_MIN_SIM_PASS_RUSH: 0
            },
            ctors: {
                init: function () {
                    this.FRONT_7_SIM_POINT_POOL = 200;
                    this.FRONT_7_MIN_SIM_PASS_RUSH = 13;
                }
            }
        },
        ctors: {
            ctor: function () {
                this.$initialize();
            }
        },
        methods: {
            /**
             * Returns the SimPocket value when passed the QB's
             MS.
             *
             * @instance
             * @public
             * @this TSBTool.SimStuff
             * @memberof TSBTool.SimStuff
             * @param   {number}    MS
             * @return  {number}
             */
            SimPocket: function (MS) {
                var ret = 0;
                switch (MS) {
                    case 100: 
                    case 94: 
                    case 88: 
                    case 81: 
                    case 75: 
                    case 69: 
                    case 63: 
                    case 56: 
                    case 50: 
                        ret = 0;
                        break;
                    case 44: 
                    case 38: 
                        ret = 1;
                        break;
                    case 31: 
                    case 25: 
                        ret = 2;
                        break;
                    default: 
                        ret = 3;
                        break;
                }
                return ret;
            },
            SimPass: function (PC, APB, PS) {
                var ret = 0;

                if (PC > 75) {
                    ret = 13;
                } else {
                    if (PC > 44) {
                        ret = (Bridge.Int.div((((((PS + PC) | 0) + APB) | 0)), 17)) | 0;
                    } else {
                        ret = (Bridge.Int.div((((PC + APB) | 0)), 14)) | 0;
                    }
                }
                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            QbSimRun: function (MS) {
                var ret = (Bridge.Int.div(MS, 5)) | 0;
                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            SimKickRet: function (MS) {
                var ret = (Bridge.Int.div(MS, 4)) | 0;
                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            SimPuntRet: function (MS) {
                var ret = (Bridge.Int.div(MS, 4)) | 0;
                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            RbSimCatch: function (REC) {
                var ret = 0;
                if (REC > 44) {
                    ret = (Bridge.Int.div(REC, 6)) | 0;
                } else {
                    ret = (Bridge.Int.div(REC, 10)) | 0;
                }

                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            RbSimRush: function (MS, HP, BC, RS) {
                var ret = 0;
                if (HP < 50) {
                    ret = ((((Bridge.Int.div((((MS + BC) | 0)), 11)) | 0)) - 2) | 0;
                } else {
                    ret = (Bridge.Int.div((((RS + HP) | 0)), 15)) | 0;
                }
                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            WrTeSimCatch: function (REC) {
                var ret = (Bridge.Int.div(REC, 6)) | 0;
                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            WrTeSimRush: function () {
                return 2;
            },
            PKSimKick: function (KA, AKB) {
                var ret = (Bridge.Int.div((((KA + (((Bridge.Int.div(AKB, 2)) | 0))) | 0)), 11)) | 0;
                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            /**
             * Use PI
             *
             * @instance
             * @public
             * @this TSBTool.SimStuff
             * @memberof TSBTool.SimStuff
             * @param   {number}            rolbInts    
             * @param   {number}            rilbInts    
             * @param   {number}            lilbInts    
             * @param   {number}            lolbInts    
             * @param   {number}            rcbInts     
             * @param   {number}            lcbInts     
             * @param   {number}            fsInts      
             * @param   {number}            ssInts
             * @return  {Array.<number>}
             */
            GetSimPassDefense: function (rolbInts, rilbInts, lilbInts, lolbInts, rcbInts, lcbInts, fsInts, ssInts) {
                var totalInts = (((((((((((((rolbInts + rilbInts) | 0) + lilbInts) | 0) + lolbInts) | 0) + rcbInts) | 0) + lcbInts) | 0) + fsInts) | 0) + ssInts) | 0;
                var totalSimPoints = 254;
                var rolbPoints, rilbPoints, lilbPoints, lolbPoints, rcbPoints, lcbPoints, fsPoints, ssPoints;

                rolbPoints = Bridge.Int.clip32((rolbInts / totalInts) * totalSimPoints);
                rilbPoints = Bridge.Int.clip32((rilbInts / totalInts) * totalSimPoints);
                lolbPoints = Bridge.Int.clip32((lolbInts / totalInts) * totalSimPoints);
                rcbPoints = Bridge.Int.clip32((rcbInts / totalInts) * totalSimPoints);
                lcbPoints = Bridge.Int.clip32((lcbInts / totalInts) * totalSimPoints);
                fsPoints = Bridge.Int.clip32((fsInts / totalInts) * totalSimPoints);
                ssPoints = Bridge.Int.clip32((ssInts / totalInts) * totalSimPoints);

                lilbPoints = (1 + Bridge.Int.clip32((totalSimPoints - (((((((((((((rcbPoints + lcbPoints) | 0) + fsPoints) | 0) + rolbPoints) | 0) + ssPoints) | 0) + rilbPoints) | 0) + lolbPoints) | 0))))) | 0;

                var ret = System.Array.init(8, 0, System.Int32);
                ret[System.Array.index(0, ret)] = rolbPoints;
                ret[System.Array.index(1, ret)] = rilbPoints;
                ret[System.Array.index(2, ret)] = lilbPoints;
                ret[System.Array.index(3, ret)] = lolbPoints;
                ret[System.Array.index(4, ret)] = rcbPoints;
                ret[System.Array.index(5, ret)] = lcbPoints;
                ret[System.Array.index(6, ret)] = fsPoints;
                ret[System.Array.index(7, ret)] = ssPoints;

                return ret;
            },
            /**
             * use HP instead of sacks
             *
             * @instance
             * @public
             * @this TSBTool.SimStuff
             * @memberof TSBTool.SimStuff
             * @param   {number}            reSacks      
             * @param   {number}            ntSacks      
             * @param   {number}            leSacks      
             * @param   {number}            rolbSacks    
             * @param   {number}            rilbSacks    
             * @param   {number}            lilbSacks    
             * @param   {number}            lolbSacks
             * @return  {Array.<number>}
             */
            GetSimPassRush: function (reSacks, ntSacks, leSacks, rolbSacks, rilbSacks, lilbSacks, lolbSacks) {
                var totalSacks = reSacks + ntSacks + leSacks + rolbSacks + rilbSacks + lilbSacks + lolbSacks;

                var totalSimPoints = TSBTool.SimStuff.FRONT_7_SIM_POINT_POOL;
                var minPr = TSBTool.SimStuff.FRONT_7_MIN_SIM_PASS_RUSH;

                var rePoints, ntPoints, lePoints, rolbPoints, rilbPoints, lilbPoints, lolbPoints, ssPoints;
                var dbPoints = 0;
                var cbPoints = 0;
                var front7Points = 0;

                if (totalSacks === 0) {
                    rePoints = (ntPoints = (lePoints = (rolbPoints = (rilbPoints = (lilbPoints = (lolbPoints = (ssPoints = 31)))))));
                    rePoints = (rePoints + 4) | 0;
                } else {
                    rePoints = Math.max(Bridge.Int.clip32((reSacks / totalSacks) * totalSimPoints), minPr);
                    lePoints = Math.max(Bridge.Int.clip32((leSacks / totalSacks) * totalSimPoints), minPr);
                    ntPoints = Math.max(Bridge.Int.clip32((ntSacks / totalSacks) * totalSimPoints), minPr);
                    rolbPoints = Math.max(Bridge.Int.clip32((rolbSacks / totalSacks) * totalSimPoints), minPr);
                    rilbPoints = Math.max(Bridge.Int.clip32((rilbSacks / totalSacks) * totalSimPoints), minPr);
                    lilbPoints = Math.max(Bridge.Int.clip32((lilbSacks / totalSacks) * totalSimPoints), minPr);
                    lolbPoints = Math.max(Bridge.Int.clip32((lolbSacks / totalSacks) * totalSimPoints), minPr);

                    front7Points = (((((((((((rePoints + lePoints) | 0) + ntPoints) | 0) + rolbPoints) | 0) + rilbPoints) | 0) + lilbPoints) | 0) + lolbPoints) | 0;

                    dbPoints = ((255 - front7Points) | 0);

                    cbPoints = (Bridge.Int.div(dbPoints, 4)) | 0;
                    ssPoints = ((255 - ((((Bridge.Int.mul(3, cbPoints)) + front7Points) | 0))) | 0);
                }
                var ret = System.Array.init(8, 0, System.Int32);

                ret[System.Array.index(0, ret)] = rePoints;
                ret[System.Array.index(1, ret)] = ntPoints;
                ret[System.Array.index(2, ret)] = lePoints;
                ret[System.Array.index(3, ret)] = rolbPoints;
                ret[System.Array.index(4, ret)] = rilbPoints;
                ret[System.Array.index(5, ret)] = lilbPoints;
                ret[System.Array.index(6, ret)] = lolbPoints;
                ret[System.Array.index(7, ret)] = ssPoints;

                return ret;
            },
            GetSimOffense: function (QB1SimPass, RB1SimRush, RB2SimRush, WR1SimCatch, WR2SimCatch) {
                var f1, f2;
                if (RB1SimRush > RB2SimRush) {
                    f1 = RB1SimRush;
                } else {
                    f1 = RB2SimRush;
                }
                if (WR1SimCatch > WR2SimCatch) {
                    f2 = WR1SimCatch;
                } else {
                    f2 = WR2SimCatch;
                }

                var ret = (Bridge.Int.div((((((QB1SimPass + f1) | 0) + f2) | 0)), 3)) | 0;
                return ret;
            }
        }
    });

    /**
     * Summary description for ScheduleHelper.
     *
     * @public
     * @class TSBTool.SNES_ScheduleHelper
     */
    Bridge.define("TSBTool.SNES_ScheduleHelper", {
        statics: {
            fields: {
                weekOneStartLoc: 0
            },
            ctors: {
                init: function () {
                    this.weekOneStartLoc = 1438654;
                }
            }
        },
        fields: {
            teamGames: null,
            week: 0,
            week_game_count: 0,
            total_game_count: 0,
            gameRegex: null,
            gamesPerWeek: null,
            outputRom: null
        },
        ctors: {
            init: function () {
                this.gameRegex = new System.Text.RegularExpressions.Regex.ctor("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");
                this.gamesPerWeek = System.Array.init([
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14
                ], System.Int32);
            },
            ctor: function (outputRom) {
                this.$initialize();
                this.outputRom = outputRom;
            }
        },
        methods: {
            CloseWeek: function () {
                if (this.week > -1) {
                    var i = this.week_game_count;
                    while (i < 14) {
                        this.ScheduleGame$2(255, 255, this.week, i);
                        i = (i + 1) | 0;
                    }
                }
                this.week = (this.week + 1) | 0;
                this.total_game_count = (this.total_game_count + this.week_game_count) | 0;
                this.week_game_count = 0;
            },
            /**
             * Applies a schedule to the rom.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_ScheduleHelper
             * @memberof TSBTool.SNES_ScheduleHelper
             * @param   {System.Collections.Generic.List$1}    lines    the contents of the schedule file.
             * @return  {void}
             */
            ApplySchedule: function (lines) {
                this.week = -1;
                this.week_game_count = 0;
                this.total_game_count = 0;

                if (TSBTool.SNES_TecmoTool.AUTO_CORRECT_SCHEDULE) {
                    lines = this.Ensure18Weeks(lines);
                }

                var line;
                for (var i = 0; i < lines.Count; i = (i + 1) | 0) {
                    line = Bridge.toString(lines.getItem(i)).trim().toLowerCase();
                    if (System.String.startsWith(line, "#") || line.length < 3) {
                    } else if (System.String.startsWith(line, "week")) {
                        if (this.week > 18) {
                            TSBTool.StaticUtils.AddError("Error! You can have only 18 weeks in a season.");
                            break;
                        }
                        this.CloseWeek();
                        TSBTool.StaticUtils.WriteError(System.String.format("Scheduleing {0}", [line]));
                    } else {
                        this.ScheduleGame$1(line);
                    }
                }
                this.CloseWeek();

                if (this.week < 18) {
                    TSBTool.StaticUtils.AddError("Warning! You didn't schedule all 18 weeks. The schedule could be messed up.");
                }
                if (this.teamGames != null) {
                    for (var i1 = 0; i1 < this.teamGames.length; i1 = (i1 + 1) | 0) {
                        if (this.teamGames[System.Array.index(i1, this.teamGames)] !== 16) {
                            TSBTool.StaticUtils.AddError(System.String.format("Warning! The {0} have {1} games scheduled.", TSBTool.TecmoTool.GetTeamFromIndex(i1), Bridge.box(this.teamGames[System.Array.index(i1, this.teamGames)], System.Int32)));
                        }
                    }
                }
            },
            /**
             * Attempts to schedule a game.
             *
             * @instance
             * @private
             * @this TSBTool.SNES_ScheduleHelper
             * @memberof TSBTool.SNES_ScheduleHelper
             * @param   {string}     line
             * @return  {boolean}            True on success, false on failure.
             */
            ScheduleGame$1: function (line) {
                var ret = false;
                var m = this.gameRegex.match(line);
                var awayTeam, homeTeam;

                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    awayTeam = m.getGroups().get(1).toString();
                    homeTeam = m.getGroups().get(2).toString();
                    if (this.week_game_count > 13) {
                        TSBTool.StaticUtils.AddError(System.String.format("Error! Week {0}: You can have no more than 14 games in a week.", [Bridge.box(((this.week + 1) | 0), System.Int32)]));
                        ret = false;
                    } else if (this.ScheduleGame(awayTeam, homeTeam, this.week, this.week_game_count)) {
                        this.week_game_count = (this.week_game_count + 1) | 0;
                        ret = true;
                    }

                }
                if (((this.total_game_count + this.week_game_count) | 0) > 224) {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! Week {0}: There are more than 224 games scheduled.", [Bridge.box(((this.week + 1) | 0), System.Int32)]));
                }
                return ret;
            },
            /**
             * @instance
             * @public
             * @this TSBTool.SNES_ScheduleHelper
             * @memberof TSBTool.SNES_ScheduleHelper
             * @param   {string}     awayTeam      
             * @param   {string}     homeTeam      
             * @param   {number}     week          Week is 0-16 (0 = week 1).
             * @param   {number}     gameOfWeek
             * @return  {boolean}
             */
            ScheduleGame: function (awayTeam, homeTeam, week, gameOfWeek) {
                var awayIndex = TSBTool.TecmoTool.GetTeamIndex(awayTeam);
                var homeIndex = TSBTool.TecmoTool.GetTeamIndex(homeTeam);

                if (awayIndex === -1 || homeIndex === -1) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error! Week {2}: Game '{0} at {1}'", awayTeam, homeTeam, Bridge.box(((week + 1) | 0), System.Int32)));
                    return false;
                }

                if (awayIndex === homeIndex && awayIndex < 28) {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! Week {0}: The {1} are scheduled to play against themselves.", Bridge.box(((week + 1) | 0), System.Int32), awayTeam));
                }

                if (week < 0 || week > 17) {
                    TSBTool.StaticUtils.AddError(System.String.format("Week {0} is not valid. Weeks range 1 - 18.", [Bridge.box(((week + 1) | 0), System.Int32)]));
                    return false;
                }
                if (this.GameLocation(week, gameOfWeek) < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("Game {0} for week {1} is not valid. Valid games for week {1} are 0-{2}.", Bridge.box(gameOfWeek, System.Int32), Bridge.box(week, System.Int32), Bridge.box(((this.gamesPerWeek[System.Array.index(week, this.gamesPerWeek)] - 1) | 0), System.Int32)));
                    TSBTool.StaticUtils.AddError(System.String.format("{0} at {1}", awayTeam, homeTeam));
                }

                this.ScheduleGame$2(awayIndex, homeIndex, week, gameOfWeek);

                if (Bridge.referenceEquals(awayTeam, "null") || Bridge.referenceEquals(homeTeam, "null")) {
                    return false;
                }
                return true;
            },
            ScheduleGame$2: function (awayTeamIndex, homeTeamIndex, week, gameOfWeek) {
                var location = this.GameLocation(week, gameOfWeek);
                if (location > 0) {
                    this.outputRom[System.Array.index(location, this.outputRom)] = awayTeamIndex & 255;
                    this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = homeTeamIndex & 255;
                    if (awayTeamIndex < 28) {
                        this.IncrementTeamGames(awayTeamIndex);
                        this.IncrementTeamGames(homeTeamIndex);
                    }
                }
                /* else
                			{
                				StaticUtils.AddError(string.Format("INVALID game for ROM. Week={0} Game of Week ={1}",
                					week,gameOfWeek);
                			}*/
            },
            /**
             * Returns a string like "49ers at giants", for a valid week, game of week combo.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_ScheduleHelper
             * @memberof TSBTool.SNES_ScheduleHelper
             * @param   {number}    week          The week in question.
             * @param   {number}    gameOfWeek    The game to get.
             * @return  {string}                  Returns a string like "49ers at giants", for a valid week, game of week combo, returns null
             upon error.
             */
            GetGame: function (week, gameOfWeek) {
                var location = this.GameLocation(week, gameOfWeek);
                if (location === -1) {
                    return null;
                }
                var awayIndex = this.outputRom[System.Array.index(location, this.outputRom)];
                var homeIndex = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)];
                var ret = "";

                if (awayIndex < 28) {
                    ret = System.String.format("{0} at {1}", TSBTool.TecmoTool.GetTeamFromIndex(awayIndex), TSBTool.TecmoTool.GetTeamFromIndex(homeIndex));
                }
                return ret;
            },
            /**
             * Returns a week from the season.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_ScheduleHelper
             * @memberof TSBTool.SNES_ScheduleHelper
             * @param   {number}    week    The week to get [0-16] (0= week 1).
             * @return  {string}
             */
            GetWeek: function (week) {
                if (week < 0 || week > ((this.gamesPerWeek.length - 1) | 0)) {
                    return null;
                }
                var sb = new System.Text.StringBuilder("", 280);
                sb.append(System.String.format("WEEK {0}\n", [Bridge.box(((week + 1) | 0), System.Int32)]));

                var game;

                for (var i = 0; i < this.gamesPerWeek[System.Array.index(week, this.gamesPerWeek)]; i = (i + 1) | 0) {
                    game = this.GetGame(week, i);
                    if (game != null && game.length > 0) {
                        sb.append(System.String.format("{0}\n", [game]));
                    }
                }
                sb.append("\n");
                return sb.toString();
            },
            GetSchedule: function () {
                var sb = new System.Text.StringBuilder("", 5040);
                for (var week = 0; week < this.gamesPerWeek.length; week = (week + 1) | 0) {
                    sb.append(this.GetWeek(week));
                }

                return sb.toString();
            },
            GameLocation: function (week, gameOfweek) {
                if (week < 0 || week > ((this.gamesPerWeek.length - 1) | 0) || gameOfweek > this.gamesPerWeek[System.Array.index(week, this.gamesPerWeek)] || gameOfweek < 0) {
                    return -1;
                }

                var offset = 0;
                for (var i = 0; i < week; i = (i + 1) | 0) {
                    offset = (offset + (Bridge.Int.mul(this.gamesPerWeek[System.Array.index(i, this.gamesPerWeek)], 2))) | 0;
                }

                offset = (offset + (Bridge.Int.mul(gameOfweek, 2))) | 0;
                var location = (TSBTool.SNES_ScheduleHelper.weekOneStartLoc + offset) | 0;
                return location;
            },
            IncrementTeamGames: function (teamIndex) {
                if (this.teamGames == null) {
                    this.teamGames = System.Array.init(28, 0, System.Int32);
                }
                if (teamIndex < this.teamGames.length) {
                    this.teamGames[System.Array.index(teamIndex, this.teamGames)] = (this.teamGames[System.Array.index(teamIndex, this.teamGames)] + 1) | 0;
                }

            },
            Ensure18Weeks: function (lines) {

                var wks = this.CountWeeks(lines);
                var line1, line2;
                for (var i = (lines.Count - 2) | 0; i > 0; i = (i - 2) | 0) {
                    line1 = Bridge.toString(lines.getItem(i));
                    line2 = Bridge.toString(lines.getItem(((i + 1) | 0)));
                    if (wks > 17) {
                        break;
                    } else if (System.String.indexOf(line1, "at") > -1 && System.String.indexOf(line2, "at") > -1) {
                        lines.insert(((i + 1) | 0), "WEEK ");
                        i = (i - 1) | 0;
                        wks = (wks + 1) | 0;
                    }
                }

                return lines;
            },
            CountWeeks: function (lines) {
                var $t;
                var count = 0;
                $t = Bridge.getEnumerator(lines);
                try {
                    while ($t.moveNext()) {
                        var line = $t.Current;
                        if (System.String.indexOf(line.toLowerCase(), "week") > -1) {
                            count = (count + 1) | 0;
                        }
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
                return count;
            }
        }
    });

    /**
     * Static utility functions that I don't want to clutter up other files with.
     *
     * @static
     * @abstract
     * @public
     * @class TSBTool.StaticUtils
     */
    Bridge.define("TSBTool.StaticUtils", {
        statics: {
            fields: {
                simpleSetRegex: null,
                sErrors: null,
                tsb1QB1Regex: null,
                tsb2QB1Regex: null,
                tsb3QB1Regex: null
            },
            props: {
                RomVersion: {
                    get: function () {
                        return "SNES_TSB2";
                    }
                }
            },
            ctors: {
                init: function () {
                    this.sErrors = new (System.Collections.Generic.List$1(System.String)).ctor();
                    this.tsb1QB1Regex = new System.Text.RegularExpressions.Regex.ctor("^QB1\\s*,[a-zA-Z 0-9]+\\s*,\\s*Face=0x[0-9]{1,2}\\s*,\\s*#[0-9]{1,2}\\s*,(\\s*[0-9]{1,2}\\s*,){7}(\\s*[0-9]{1,2}\\s*,?){1}(\\s*\\[|\\s*$)", 2);
                    this.tsb2QB1Regex = new System.Text.RegularExpressions.Regex.ctor("^QB1\\s*,[a-zA-Z 0-9]+\\s*,\\s*Face=0x[0-9]{1,2}\\s*,\\s*#[0-9]{1,2}\\s*,(\\s*[0-9]{1,2}\\s*,){9}(\\s*[0-9]{1,2}\\s*,?){1}(\\s*\\[|\\s*$)", 2);
                    this.tsb3QB1Regex = new System.Text.RegularExpressions.Regex.ctor("^QB1\\s*,[a-zA-Z 0-9\\.]+\\s*,\\s*Face=0x[08][0-9A-Fa-f]{1}\\s*,\\s*#[0-9]{1,2}\\s*,(\\s*[0-9]{1,2}\\s*,){10}(\\s*[0-9]{1,2}\\s*,?){1}(\\s*\\[|\\s*$)", 2);
                }
            },
            methods: {
                /**
                 * takes a math string, returns a value
                 *
                 * @static
                 * @public
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {string}    formula
                 * @return  {string}
                 */
                Compute: function (formula) {
                    return eval(formula);
                },
                ParseIntFromHexString: function (input) {
                    var retVal = System.Int32.parse(input, 16);
                    return retVal;
                },
                ParseLongFromHexString: function (input) {
                    var retVal = System.Int64(TSBTool.StaticUtils.ParseIntFromHexString(input));
                    return retVal;
                },
                ParseByteFromHexString: function (input) {
                    var retVal = System.Byte.parse(input, 16);
                    return retVal;
                },
                WriteError: function (input) {
                    System.Console.WriteLine(input);
                },
                /**
                 * Returns a string starting with 'Error!' on error condition, the locations of the replacements otherwise.
                 *
                 * @static
                 * @public
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {Array.<number>}    outputRom     
                 * @param   {string}            searchStr     The string to search for.
                 * @param   {string}            replaceStr    The string to replace it with.
                 * @param   {number}            occurence     The occurence you wish to replace, -1 for all occurences.
                 * @return  {string}
                 */
                ReplaceStringInRom: function (outputRom, searchStr, replaceStr, occurence) {
                    if (replaceStr.length > searchStr.length) {
                        return System.String.format("Error! Replace({0},{1}), cannot replace a string with a longer string", searchStr, replaceStr);
                    }
                    while (replaceStr.length < searchStr.length) {
                        replaceStr = (replaceStr || "") + " ";
                    }

                    var locs = TSBTool.StaticUtils.FindStringInFile(searchStr, outputRom, 0, outputRom.length);
                    var builder = new System.Text.StringBuilder();
                    builder.append(System.String.format("Replaced '{0}' with '{1}' at location(s):", searchStr, replaceStr));
                    for (var i = 0; i < locs.Count; i = (i + 1) | 0) {
                        if (occurence < 0 || occurence === i) {
                            builder.append(System.String.format("0x{0:x},", [locs.getItem(i)]));
                            var stringLoc = System.Int64.clip32(locs.getItem(i));
                            for (var j = 0; j < replaceStr.length; j = (j + 1) | 0) {
                                outputRom[System.Array.index(stringLoc, outputRom)] = (replaceStr.charCodeAt(j)) & 255;
                                stringLoc = (stringLoc + 1) | 0;
                            }
                        }
                    }
                    return builder.toString();
                },
                /**
                 * Find string 'str' (unicode string) in the data byte array.
                 *
                 * @static
                 * @public
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {string}                               str      The string to look for
                 * @param   {Array.<number>}                       data     The data to search through.
                 * @param   {number}                               start    where to start in 'data'
                 * @param   {number}                               end      Where to end in 'data'
                 * @return  {System.Collections.Generic.List$1}             a list of addresses
                 */
                FindStringInFile: function (str, data, start, end) {
                    return TSBTool.StaticUtils.FindStringInFile$1(str, data, start, end, false);
                },
                /**
                 * Find string 'str' (unicode string) in the data byte array.
                 *
                 * @static
                 * @public
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {string}                               str         The string to look for
                 * @param   {Array.<number>}                       data        The data to search through.
                 * @param   {number}                               start       where to start in 'data'
                 * @param   {number}                               end         Where to end in 'data'
                 * @param   {boolean}                              nullByte    True to append the null byte at the end.
                 * @return  {System.Collections.Generic.List$1}                a list of addresses
                 */
                FindStringInFile$1: function (str, data, start, end, nullByte) {
                    var $t;
                    var retVal = new (System.Collections.Generic.List$1(System.Int64)).ctor();
                    var length = str.length;
                    if (nullByte) {
                        length = (length + 1) | 0;
                    }

                    var target = System.Array.init(length, 0, System.Byte);
                    var i = 0;
                    System.Array.fill(target, 0, 0, target.length);
                    $t = Bridge.getEnumerator(str);
                    try {
                        while ($t.moveNext()) {
                            var c = $t.Current;
                            target[System.Array.index(Bridge.identity(i, ((i = (i + 1) | 0))), target)] = c & 255;
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    return TSBTool.StaticUtils.FindByesInFile(target, data, start, end);
                },
                /**
                 * Find an array of bytes in the data byte array.
                 *
                 * @static
                 * @public
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {Array.<number>}                       target    
                 * @param   {Array.<number>}                       data      The data to search through.
                 * @param   {number}                               start     where to start in 'data'
                 * @param   {number}                               end       Where to end in 'data'
                 * @return  {System.Collections.Generic.List$1}              a list of addresses
                 */
                FindByesInFile: function (target, data, start, end) {
                    var retVal = new (System.Collections.Generic.List$1(System.Int64)).ctor();

                    if (data != null && data.length > 80) {
                        if (start < 0) {
                            start = 0;
                        }
                        if (end > data.length) {
                            end = (data.length - 1) | 0;
                        }

                        var num = System.Int64(((end - target.length) | 0));
                        for (var num3 = System.Int64(start); num3.lt(num); num3 = num3.add(System.Int64(1))) {
                            if (TSBTool.StaticUtils.Check(target, num3, data)) {
                                retVal.add(num3);
                            }
                        }
                    }
                    return retVal;
                },
                Check: function (target, location, data) {
                    var i;
                    for (i = 0; i < target.length; i = (i + 1) | 0) {
                        if (target[System.Array.index(i, target)] !== data[System.Array.index(System.Int64.toNumber(location.add(System.Int64(i))), data)]) {
                            break;
                        }
                    }
                    return i === target.length;
                },
                MapAttributes: function (attrs) {
                    var $t;
                    var builder = new System.Text.StringBuilder();
                    $t = Bridge.getEnumerator(attrs);
                    try {
                        while ($t.moveNext()) {
                            var b = $t.Current;
                            builder.append(Bridge.toString(TSBTool.StaticUtils.MapAbilityToTSBValue(b)));
                            builder.append(",");
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    return builder.toString();
                },
                GetTsbAbilities: function (abilities) {
                    var retVal = System.Array.init(abilities.length, 0, System.Byte);
                    for (var i = 0; i < retVal.length; i = (i + 1) | 0) {
                        retVal[System.Array.index(i, retVal)] = TSBTool.StaticUtils.GetTSBAbility(abilities[System.Array.index(i, abilities)]);
                    }
                    return retVal;
                },
                /**
                 * Get the index (0-F) ability for the input
                 *
                 * @static
                 * @public
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {number}    ab    the ability
                 * @return  {number}          (0x0-0xF)
                 */
                GetTSBAbility: function (ab) {
                    var ret = 0;
                    switch (ab) {
                        case 6: 
                            ret = 0;
                            break;
                        case 13: 
                            ret = 1;
                            break;
                        case 19: 
                            ret = 2;
                            break;
                        case 25: 
                            ret = 3;
                            break;
                        case 31: 
                            ret = 4;
                            break;
                        case 38: 
                            ret = 5;
                            break;
                        case 44: 
                            ret = 6;
                            break;
                        case 50: 
                            ret = 7;
                            break;
                        case 56: 
                            ret = 8;
                            break;
                        case 63: 
                            ret = 9;
                            break;
                        case 69: 
                            ret = 10;
                            break;
                        case 75: 
                            ret = 11;
                            break;
                        case 81: 
                            ret = 12;
                            break;
                        case 88: 
                            ret = 13;
                            break;
                        case 94: 
                            ret = 14;
                            break;
                        case 100: 
                            ret = 15;
                            break;
                    }
                    return ret;
                },
                /**
                 * takes 0x03 --&gt; 25
                 *
                 * @static
                 * @public
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {number}    ab    0x00 - 0x0F
                 * @return  {number}          A TSB ability (6,13,19,25,31,38,44,50,56,63,69,75,81,88,94,100)
                 */
                MapAbilityToTSBValue: function (ab) {
                    var ret = 0;
                    switch (ab) {
                        case 0: 
                            ret = 6;
                            break;
                        case 1: 
                            ret = 13;
                            break;
                        case 2: 
                            ret = 19;
                            break;
                        case 3: 
                            ret = 25;
                            break;
                        case 4: 
                            ret = 31;
                            break;
                        case 5: 
                            ret = 38;
                            break;
                        case 6: 
                            ret = 44;
                            break;
                        case 7: 
                            ret = 50;
                            break;
                        case 8: 
                            ret = 56;
                            break;
                        case 9: 
                            ret = 63;
                            break;
                        case 10: 
                            ret = 69;
                            break;
                        case 11: 
                            ret = 75;
                            break;
                        case 12: 
                            ret = 81;
                            break;
                        case 13: 
                            ret = 88;
                            break;
                        case 14: 
                            ret = 94;
                            break;
                        case 15: 
                            ret = 100;
                            break;
                    }
                    return ret;
                },
                CombineNibbles: function (first, second) {
                    var retVal = first << 4;
                    retVal = (retVal + second) | 0;
                    return (retVal & 255);
                },
                GetFirstNibble: function (b) {
                    var retVal = (b >> 4) & 255;
                    return retVal;
                },
                GetSecondNibble: function (b) {
                    var retVal = (b & 15) & 255;
                    return retVal;
                },
                CheckTSB2Args: function (season, team) {
                    if (season < 1 || season > 3) {
                        throw new System.ArgumentException.$ctor1("Invalid season! " + season);
                    }
                    if (TSBTool2.TSB2Tool.teams.indexOf(team) < 0) {
                        throw new System.ArgumentException.$ctor1("Invalid team! " + (team || ""));
                    }
                },
                CheckTSB2Args$1: function (season, team, position) {
                    TSBTool.StaticUtils.CheckTSB2Args(season, team);
                    if (TSBTool.TecmoTool.positionNames.indexOf(position) < 0) {
                        throw new System.ArgumentException.$ctor1("Invalid position! " + (position || ""));
                    }
                },
                ApplySet: function (line, tool) {
                    if (TSBTool.StaticUtils.simpleSetRegex == null) {
                        TSBTool.StaticUtils.simpleSetRegex = new System.Text.RegularExpressions.Regex.ctor("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
                    }

                    if (!Bridge.referenceEquals(TSBTool.StaticUtils.simpleSetRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                        TSBTool.StaticUtils.ApplySimpleSet(line, Bridge.as(tool, TSBTool.ITecmoContent));
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR with line \"{0}\"", [line]));
                    }
                },
                ApplySimpleSet: function (line, tool) {
                    if (TSBTool.StaticUtils.simpleSetRegex == null) {
                        TSBTool.StaticUtils.simpleSetRegex = new System.Text.RegularExpressions.Regex.ctor("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
                    }

                    var m = TSBTool.StaticUtils.simpleSetRegex.match(line);
                    if (Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        TSBTool.StaticUtils.ShowError(System.String.format("SET function not used properly. incorrect syntax>'{0}'", [line]));
                        return;
                    }
                    var loc = m.getGroups().get(1).toString().toLowerCase();
                    var val = m.getGroups().get(2).toString().toLowerCase();
                    loc = loc.substr(2);
                    val = val.substr(2);
                    if (val.length % 2 !== 0) {
                        val = "0" + (val || "");
                    }

                    try {
                        var location = TSBTool.StaticUtils.ParseIntFromHexString(loc);
                        var bytes = TSBTool.StaticUtils.GetHexBytes(val);
                        if (((location + bytes.length) | 0) > tool.TSBTool$ITecmoContent$OutputRom.length) {
                            TSBTool.StaticUtils.ShowError(System.String.format("ApplySet:> Error with line {0}. Data falls off the end of rom.\n", [line]));
                        } else if (location < 0) {
                            TSBTool.StaticUtils.ShowError(System.String.format("ApplySet:> Error with line {0}. location is negative.\n", [line]));
                        } else {
                            for (var i = 0; i < bytes.length; i = (i + 1) | 0) {
                                tool.TSBTool$ITecmoContent$SetByte(((location + i) | 0), bytes[System.Array.index(i, bytes)]);
                            }
                        }
                    } catch (e) {
                        e = System.Exception.create(e);
                        TSBTool.StaticUtils.ShowError(System.String.format("ApplySet:> Error with line {0}.\n{1}", line, e.Message));
                    }
                },
                GetHexBytes: function (input) {
                    if (input == null) {
                        return null;
                    }
                    if (input.length > 2 && (System.String.startsWith(input, "0x") || System.String.startsWith(input, "0X"))) {
                        input = input.substr(2);
                    }

                    var ret = System.Array.init(((Bridge.Int.div(input.length, 2)) | 0), 0, System.Byte);
                    var b = "";
                    var tmp = 0;
                    var j = 0;

                    for (var i = 0; i < input.length; i = (i + 2) | 0) {
                        b = input.substr(i, 2);
                        tmp = TSBTool.StaticUtils.ParseIntFromHexString(b);
                        ret[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), ret)] = tmp & 255;
                    }
                    return ret;
                },
                AddError: function (error) {
                    TSBTool.StaticUtils.sErrors.add(error);
                },
                ClearErrors: function () {
                    TSBTool.StaticUtils.sErrors.clear();
                },
                ShowErrors: function () {
                    var $t;
                    if (TSBTool.StaticUtils.sErrors != null && TSBTool.StaticUtils.sErrors.Count > 0) {
                        var sb = new System.Text.StringBuilder("", 500);
                        $t = Bridge.getEnumerator(TSBTool.StaticUtils.sErrors);
                        try {
                            while ($t.moveNext()) {
                                var e = $t.Current;
                                sb.append((e || "") + "\n");
                            }
                        } finally {
                            if (Bridge.is($t, System.IDisposable)) {
                                $t.System$IDisposable$Dispose();
                            }
                        }
                        TSBTool.StaticUtils.ShowError(sb.toString());
                        TSBTool.StaticUtils.ClearErrors();
                    }
                },
                ShowError: function (error) {
                    System.Console.WriteLine(error);
                },
                ReadRom: function (filename) {
                    var outputRom = null;
                    var s1 = null;
                    try {
                        s1 = new System.IO.FileStream.$ctor1(filename, 3);
                        var len = s1.Length;
                        outputRom = System.Array.init(System.Int64.clip32(len), 0, System.Byte);
                        s1.Read(outputRom, 0, System.Int64.clip32(len));
                    } catch (e) {
                        e = System.Exception.create(e);
                        TSBTool.StaticUtils.ShowError(Bridge.toString(e));
                    } finally {
                        if (s1 != null) {
                            s1.Close();
                        }
                    }
                    return outputRom;
                },
                SaveRom: function (filename, outputRom) {
                    if (filename != null) {
                        try {
                            var len = System.Int64(outputRom.length);
                            var s1 = new System.IO.FileStream.$ctor1(filename, 4);
                            s1.Write(outputRom, 0, System.Int64.clip32(len));
                            s1.Close();
                        } catch (e) {
                            e = System.Exception.create(e);
                            TSBTool.StaticUtils.ShowError(Bridge.toString(e));
                        }
                    } else {
                        TSBTool.StaticUtils.AddError("ERROR! You passed a null filename");
                    }
                },
                /**
                 * Updates strng pointers
                 *
                 * @static
                 * @private
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {Array.<number>}    rom                     
                 * @param   {number}            firstPointerLocation    
                 * @param   {number}            change                  the amount of change
                 * @param   {number}            lastPointerLocation
                 * @return  {void}
                 */
                AdjustDataPointers: function (rom, firstPointerLocation, change, lastPointerLocation) {
                    var low, hi;
                    var word;
                    var i = 0;
                    var end = (lastPointerLocation + 1) | 0;
                    for (i = (firstPointerLocation + 2) | 0; i < end; i = (i + 2) | 0) {
                        low = rom[System.Array.index(i, rom)];
                        hi = rom[System.Array.index(((i + 1) | 0), rom)];
                        word = hi;
                        word = word << 8;
                        word = (word + low) | 0;
                        word = (word + change) | 0;
                        low = (word & 255) & 255;
                        word = word >> 8;
                        hi = word & 255;
                        rom[System.Array.index(i, rom)] = low;
                        rom[System.Array.index(((i + 1) | 0), rom)] = hi;
                    }
                },
                ShiftDataUp: function (startPos, endPos, shiftAmount, data) {
                    if (startPos < 0 || endPos < 0) {
                        throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataUp:: either startPos {0} or endPos {1} is invalid.", Bridge.box(startPos, System.Int32), Bridge.box(endPos, System.Int32)));
                    }

                    var i;
                    if (shiftAmount > 0) {
                        System.Console.WriteLine("positive shift amount in ShiftDataUp");
                    }

                    for (i = startPos; i <= endPos; i = (i + 1) | 0) {
                        data[System.Array.index(((i + shiftAmount) | 0), data)] = data[System.Array.index(i, data)];
                    }

                    /* i += shiftAmount;
                    while (outputRom[i] != 0xff && i < 0x300f) { // with this commented out, there will be junk at the end that looks kinda valid, but is just left over
                       SetByte(i, 0xff);
                       i++;
                    }*/
                },
                ShiftDataDown: function (startPos, endPos, shiftAmount, data) {
                    if (startPos < 0 || endPos < 0) {
                        throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataDown:: either startPos {0} or endPos {1} is invalid.", Bridge.box(startPos, System.Int32), Bridge.box(endPos, System.Int32)));
                    }

                    for (var i = (endPos + shiftAmount) | 0; i > startPos; i = (i - 1) | 0) {
                        data[System.Array.index(i, data)] = data[System.Array.index(((i - shiftAmount) | 0), data)];
                    }
                },
                SetStringTableString: function (rom, stringIndex, newValue, firstPointer, offset, numberOfStringsInTable, stringTableSizeInBytes) {
                    var junk = { };
                    var oldValue = TSBTool.StaticUtils.GetStringTableString(rom, stringIndex, firstPointer, offset);
                    if (Bridge.referenceEquals(oldValue, newValue)) {
                        return;
                    }
                    var shiftAmount = (newValue.length - oldValue.length) | 0;
                    if (shiftAmount !== 0) {
                        var currentPointerLocation = (firstPointer + Bridge.Int.mul(2, stringIndex)) | 0;
                        var lastPointerLocation = (firstPointer + Bridge.Int.mul(2, numberOfStringsInTable)) | 0;
                        TSBTool.StaticUtils.AdjustDataPointers(rom, currentPointerLocation, shiftAmount, lastPointerLocation);
                        var startPosition = TSBTool.StaticUtils.GetStringTableStringLocation(rom, ((Bridge.Int.mul((((stringIndex + 1) | 0)), 2) + firstPointer) | 0), junk, offset);
                        var endPosition = (firstPointer + stringTableSizeInBytes) | 0;
                        if (shiftAmount < 0) {
                            TSBTool.StaticUtils.ShiftDataUp(startPosition, endPosition, shiftAmount, rom);
                        } else {
                            if (shiftAmount > 0) {
                                TSBTool.StaticUtils.ShiftDataDown(startPosition, endPosition, shiftAmount, rom);
                            }
                        }
                    }
                    var startLoc = TSBTool.StaticUtils.GetStringTableStringLocation(rom, ((Bridge.Int.mul(stringIndex, 2) + firstPointer) | 0), junk, offset);
                    for (var i = 0; i < newValue.length; i = (i + 1) | 0) {
                        if (newValue.charCodeAt(i) === 42) {
                            rom[System.Array.index(((startLoc + i) | 0), rom)] = 0;
                        } else {
                            rom[System.Array.index(((startLoc + i) | 0), rom)] = (newValue.charCodeAt(i)) & 255;
                        }
                    }
                },
                GetStringTableString: function (rom, string_index, firstPointer, offset) {
                    var retVal = "";
                    var pointer = (Bridge.Int.mul(string_index, 2) + firstPointer) | 0;
                    var length = { v : -1 };

                    var location = TSBTool.StaticUtils.GetStringTableStringLocation(rom, pointer, length, offset);
                    if (length.v > 0) {
                        var stringChars = System.Array.init(length.v, 0, System.Char);
                        for (var i = 0; i < stringChars.length; i = (i + 1) | 0) {
                            stringChars[System.Array.index(i, stringChars)] = rom[System.Array.index(((location + i) | 0), rom)];
                            if (stringChars[System.Array.index(i, stringChars)] === 0) {
                                stringChars[System.Array.index(i, stringChars)] = 42;
                            }
                        }
                        retVal = System.String.fromCharArray(stringChars);
                    }
                    return retVal;
                },
                GetStringTableStringLocation: function (rom, pointerLocation, length, offset) {
                    var pointer_loc = pointerLocation;
                    var b1 = rom[System.Array.index(((pointer_loc + 1) | 0), rom)];
                    var b2 = rom[System.Array.index(pointer_loc, rom)];
                    var b3 = rom[System.Array.index(((pointer_loc + 3) | 0), rom)];
                    var b4 = rom[System.Array.index(((pointer_loc + 2) | 0), rom)];
                    length.v = (((((b3 << 8) + b4) | 0)) - ((((b1 << 8) + b2) | 0))) | 0;
                    var pointerVal = ((b1 << 8) + b2) | 0;
                    var stringStartingLocation = (pointerVal + offset) | 0;
                    return stringStartingLocation;
                },
                AreEqual: function (str1, str2) {
                    var retVal = "";
                    if (!Bridge.referenceEquals(str1, str2)) {
                        retVal = System.String.format("AreEqual:Failure '{0}' and '{1}'\n", str1, str2);
                        System.Diagnostics.Debugger.Log(1, "TEST", retVal);
                    }
                    return retVal;
                },
                IsTSB1Content: function (data) {
                    var retVal = false;
                    var mc = TSBTool.StaticUtils.tsb1QB1Regex.matches(data);
                    if (mc.getCount() > 0) {
                        retVal = true;
                    }
                    return retVal;
                },
                IsTSB2Content: function (data) {
                    var retVal = false;
                    var mc = TSBTool.StaticUtils.tsb2QB1Regex.matches(data);
                    if (mc.getCount() > 0) {
                        retVal = true;
                    }
                    return retVal;
                },
                IsTSB3Content: function (data) {
                    var retVal = false;
                    var mc = TSBTool.StaticUtils.tsb3QB1Regex.matches(data);
                    if (mc.getCount() > 0) {
                        retVal = true;
                    }
                    return retVal;
                },
                /**
                 * Returns the content type (TSB1, TSB2, TSB3, Unknown)
                 *
                 * @static
                 * @this TSBTool.StaticUtils
                 * @memberof TSBTool.StaticUtils
                 * @param   {string}                    data
                 * @return  {TSBTool.TSBContentType}
                 */
                GetContentType: function (data) {
                    if (TSBTool.StaticUtils.IsTSB1Content(data)) {
                        return TSBTool.TSBContentType.TSB1;
                    }
                    if (TSBTool.StaticUtils.IsTSB2Content(data)) {
                        return TSBTool.TSBContentType.TSB2;
                    }
                    if (TSBTool.StaticUtils.IsTSB3Content(data)) {
                        return TSBTool.TSBContentType.TSB3;
                    }
                    return TSBTool.TSBContentType.Unknown;
                }
            }
        }
    });

    Bridge.define("TSBTool.TeamRatings", {
        fields: {
            team: null,
            qbRating: 0,
            rb1Rating: 0,
            rb2Rating: 0,
            wr1Rating: 0,
            wr2Rating: 0,
            teRating: 0,
            olRating: 0,
            dlRunDefenseRating: 0,
            dlPassDefenseRating: 0,
            lbRunDefenseRating: 0,
            lbPassDefenseRating: 0,
            dbRunDefenseRating: 0,
            dbPassDefenseRating: 0,
            totalRunD: 0,
            totalPassD: 0
        }
    });

    /**
     * Summary description for SimStuff.
     *
     * @public
     * @class TSBTool.TecmonsterSimStuff
     */
    Bridge.define("TSBTool.TecmonsterSimStuff", {
        statics: {
            fields: {
                FRONT_7_SIM_POINT_POOL: 0,
                FRONT_7_MIN_SIM_PASS_RUSH: 0
            },
            ctors: {
                init: function () {
                    this.FRONT_7_SIM_POINT_POOL = 200;
                    this.FRONT_7_MIN_SIM_PASS_RUSH = 13;
                }
            }
        },
        ctors: {
            ctor: function () {
                this.$initialize();
            }
        },
        methods: {
            /**
             * Returns the SimPocket value when passed the QB's
             MS.
             *
             * @instance
             * @public
             * @this TSBTool.TecmonsterSimStuff
             * @memberof TSBTool.TecmonsterSimStuff
             * @param   {number}    RS    
             * @param   {number}    MS
             * @return  {number}
             */
            SimPocket: function (RS, MS) {
                var ret = 0;
                if (((RS + MS) | 0) > 99) {
                    ret = 0;
                } else {
                    if (((RS + MS) | 0) > 81) {
                        ret = 1;
                    } else {
                        ret = 3;
                    }
                }
                return ret;
            },
            SimPass: function (PS, PC, PA, APB) {
                var ret = Bridge.Int.clip32(Bridge.Math.round((((PS - 50.0) + (PC - 31.0) + (PA - 31.0) + (APB - 31.0)) / 177) * 15, 0, 6));
                if (ret < 0) {
                    ret = 0;
                }
                return ret;
            },
            QbSimRun: function (RS, RP, MS) {
                var ret = Bridge.Int.clip32(Bridge.Math.round((((RS - 31.0) + (RP - 25.0) + (MS - 13.0)) / 93) * 15, 0, 6));
                if (ret < 0) {
                    ret = 0;
                }
                return ret;
            },
            SimYPC: function (MS, REC) {
                var ret = Bridge.Int.clip32(Bridge.Math.round(((((MS - 31.0) + (REC - 13.0)) / 106.0)) * 15, 0, 6));
                if (ret < 0) {
                    ret = 0;
                }
                return ret;
            },
            SimTargets: function (MS) {
                var ret = (Bridge.Int.div(MS, 4)) | 0;
                if (ret > 15) {
                    ret = 15;
                }
                return ret;
            },
            SimCatch: function (REC) {
                var ret = Bridge.Int.clip32(Bridge.Math.round((((REC - 13.0) / 68)) * 15, 0, 6));
                return ret;
            },
            RbSimRush: function (RS, RP, MS, HP) {
                var ret = 0;
                ret = Bridge.Int.clip32(Bridge.Math.round((((RS - 31.0) + (RP - 19.0) + (MS - 31.0) + (HP - 13.0)) / 176) * 15, 0, 6));
                if (ret < 0) {
                    ret = 0;
                }
                return ret;
            },
            PKSimKick: function (KA, AKB) {
                var ret = Bridge.Int.clip32(Bridge.Math.round(((KA - 38.0) / 43) * 15, 0, 6));
                return ret;
            },
            GetSimPassDefense: function (rolbInts, rilbInts, lilbInts, lolbInts, rcbInts, lcbInts, fsInts, ssInts) {
                var totalInts = (((((((((((((rolbInts + rilbInts) | 0) + lilbInts) | 0) + lolbInts) | 0) + rcbInts) | 0) + lcbInts) | 0) + fsInts) | 0) + ssInts) | 0;
                var totalSimPoints = 254;
                var rolbPoints, rilbPoints, lilbPoints, lolbPoints, rcbPoints, lcbPoints, fsPoints, ssPoints;

                rolbPoints = Bridge.Int.clip32((rolbInts / totalInts) * totalSimPoints);
                rilbPoints = Bridge.Int.clip32((rilbInts / totalInts) * totalSimPoints);
                lolbPoints = Bridge.Int.clip32((lolbInts / totalInts) * totalSimPoints);
                rcbPoints = Bridge.Int.clip32((rcbInts / totalInts) * totalSimPoints);
                lcbPoints = Bridge.Int.clip32((lcbInts / totalInts) * totalSimPoints);
                fsPoints = Bridge.Int.clip32((fsInts / totalInts) * totalSimPoints);
                ssPoints = Bridge.Int.clip32((ssInts / totalInts) * totalSimPoints);

                lilbPoints = (1 + Bridge.Int.clip32((totalSimPoints - (((((((((((((rcbPoints + lcbPoints) | 0) + fsPoints) | 0) + rolbPoints) | 0) + ssPoints) | 0) + rilbPoints) | 0) + lolbPoints) | 0))))) | 0;

                var ret = System.Array.init(8, 0, System.Int32);
                ret[System.Array.index(0, ret)] = rolbPoints;
                ret[System.Array.index(1, ret)] = rilbPoints;
                ret[System.Array.index(2, ret)] = lilbPoints;
                ret[System.Array.index(3, ret)] = lolbPoints;
                ret[System.Array.index(4, ret)] = rcbPoints;
                ret[System.Array.index(5, ret)] = lcbPoints;
                ret[System.Array.index(6, ret)] = fsPoints;
                ret[System.Array.index(7, ret)] = ssPoints;

                return ret;
            },
            GetSimPassRush: function (reSacks, ntSacks, leSacks, rolbSacks, rilbSacks, lilbSacks, lolbSacks) {
                var totalSacks = reSacks + ntSacks + leSacks + rolbSacks + rilbSacks + lilbSacks + lolbSacks;

                var totalSimPoints = TSBTool.TecmonsterSimStuff.FRONT_7_SIM_POINT_POOL;
                var minPr = TSBTool.TecmonsterSimStuff.FRONT_7_MIN_SIM_PASS_RUSH;

                var rePoints, ntPoints, lePoints, rolbPoints, rilbPoints, lilbPoints, lolbPoints, ssPoints;
                var dbPoints = 0;
                var cbPoints = 0;
                var front7Points = 0;

                if (totalSacks === 0) {
                    rePoints = (ntPoints = (lePoints = (rolbPoints = (rilbPoints = (lilbPoints = (lolbPoints = (ssPoints = 31)))))));
                    rePoints = (rePoints + 4) | 0;
                } else {
                    rePoints = Math.max(Bridge.Int.clip32((reSacks / totalSacks) * totalSimPoints), minPr);
                    lePoints = Math.max(Bridge.Int.clip32((leSacks / totalSacks) * totalSimPoints), minPr);
                    ntPoints = Math.max(Bridge.Int.clip32((ntSacks / totalSacks) * totalSimPoints), minPr);
                    rolbPoints = Math.max(Bridge.Int.clip32((rolbSacks / totalSacks) * totalSimPoints), minPr);
                    rilbPoints = Math.max(Bridge.Int.clip32((rilbSacks / totalSacks) * totalSimPoints), minPr);
                    lilbPoints = Math.max(Bridge.Int.clip32((lilbSacks / totalSacks) * totalSimPoints), minPr);
                    lolbPoints = Math.max(Bridge.Int.clip32((lolbSacks / totalSacks) * totalSimPoints), minPr);

                    front7Points = (((((((((((rePoints + lePoints) | 0) + ntPoints) | 0) + rolbPoints) | 0) + rilbPoints) | 0) + lilbPoints) | 0) + lolbPoints) | 0;

                    dbPoints = ((255 - front7Points) | 0);

                    cbPoints = (Bridge.Int.div(dbPoints, 4)) | 0;
                    ssPoints = ((255 - ((((Bridge.Int.mul(3, cbPoints)) + front7Points) | 0))) | 0);
                }
                var ret = System.Array.init(8, 0, System.Int32);

                ret[System.Array.index(0, ret)] = rePoints;
                ret[System.Array.index(1, ret)] = ntPoints;
                ret[System.Array.index(2, ret)] = lePoints;
                ret[System.Array.index(3, ret)] = rolbPoints;
                ret[System.Array.index(4, ret)] = rilbPoints;
                ret[System.Array.index(5, ret)] = lilbPoints;
                ret[System.Array.index(6, ret)] = lolbPoints;
                ret[System.Array.index(7, ret)] = ssPoints;

                return ret;
            }
        }
    });

    Bridge.define("TSBTool.TecmonsterTeamSim", {
        fields: {
            TextData: null
        },
        methods: {
            AutoUpdateSeasonSimData: function (season, textData) {
                this.TextData = textData;
                var teamsForSeason = this.GetTeams(season);
                var ratings = this.GetTeamsRatings(season, teamsForSeason);
                var averages = { v : this.CalculateSimAverages(ratings) };
                this.CalculateSimDefenses(ratings, averages);
                for (var i = 0; i < ratings.Count; i = (i + 1) | 0) {
                    this.UpdateTeamSimData(season, ratings.getItem(i), averages.v.$clone());
                }
                return this.TextData;
            },
            CalculateSimDefenses: function (ratings, averages) {
                var $t;
                $t = Bridge.getEnumerator(ratings);
                try {
                    while ($t.moveNext()) {
                        var rat = $t.Current;
                        rat.totalRunD = (rat.dlRunDefenseRating / averages.v.DL_run_ave) * 0.4 + (rat.lbRunDefenseRating / averages.v.LB_run_ave) * 0.4 + (rat.dbRunDefenseRating / averages.v.DB_run_ave) * 0.2;
                        rat.totalPassD = (rat.dlPassDefenseRating / averages.v.DL_pass_ave) * 0.2 + (rat.lbPassDefenseRating / averages.v.LB_pass_ave) * 0.2 + (rat.dbPassDefenseRating / averages.v.DB_pass_ave) * 0.6;
                        averages.v.TOTAL_RUN_DEFENSE += rat.totalRunD;
                        averages.v.TOTAL_PASS_DEFENSE += rat.totalPassD;

                        if (averages.v.MIN_PASS_DEFENSE === 0 || rat.totalPassD < averages.v.MIN_PASS_DEFENSE) {
                            averages.v.MIN_PASS_DEFENSE = rat.totalPassD;
                        }
                        if (averages.v.MIN_RUN_DEFENSE === 0 || rat.totalRunD < averages.v.MIN_RUN_DEFENSE) {
                            averages.v.MIN_RUN_DEFENSE = rat.totalRunD;
                        }
                        if (rat.totalPassD > averages.v.MAX_PASS_DEFENSE) {
                            averages.v.MAX_PASS_DEFENSE = rat.totalPassD;
                        }
                        if (rat.totalRunD > averages.v.MAX_RUN_DEFENSE) {
                            averages.v.MAX_RUN_DEFENSE = rat.totalRunD;
                        }
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
            },
            UpdateTeamSimData: function (season, ratings, averages) {
                var spread = averages.MAX_RUN_DEFENSE - averages.MIN_RUN_DEFENSE;
                var simRunDefense = Bridge.Math.round(((ratings.totalRunD - averages.MIN_RUN_DEFENSE) / spread) * 15, 0, 6);
                spread = averages.MAX_PASS_DEFENSE - averages.MIN_PASS_DEFENSE;
                var simPassDefense = Bridge.Math.round(((ratings.totalPassD - averages.MIN_PASS_DEFENSE) / spread) * 15, 0, 6);
                if (simRunDefense > 15) {
                    simRunDefense = 15;
                }
                if (simPassDefense > 15) {
                    simPassDefense = 15;
                }
                var sim_def = System.String.format("{0:x}{1:x}", Bridge.box(Bridge.Int.clip32(simRunDefense), System.Int32), Bridge.box(Bridge.Int.clip32(simPassDefense), System.Int32));

                var pattern = System.String.format("TEAM\\s*=\\s*{0}\\s*,?\\s*SimData\\s*=\\s*0x([0-9a-fA-F]{{2}})", [ratings.team]);
                var teamSimRegex = new System.Text.RegularExpressions.Regex.ctor(pattern);
                var seasonIndex = this.GetSeasonIndex(season);
                var m = teamSimRegex.match(this.TextData, seasonIndex);
                var old = m.getGroups().get(1).toString();
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    var start = this.TextData.substr(0, m.getGroups().get(1).getIndex());
                    var last = this.TextData.substr(((m.getGroups().get(1).getIndex() + 2) | 0));
                    var tmp = new System.Text.StringBuilder("", ((this.TextData.length + 20) | 0));
                    tmp.append(start);
                    tmp.append(sim_def);
                    tmp.append(last);
                    this.TextData = tmp.toString();
                }
            },
            GetSeasonIndex: function (season) {
                var retVal = 0;
                if (TSBTool.StaticUtils.IsTSB2Content(this.TextData)) {
                    var pattern = System.String.format("^\\s*SEASON\\s+{0}", [Bridge.box(season, System.Int32)]);
                    var seasonRegex = new System.Text.RegularExpressions.Regex.ctor(pattern);
                    var m = seasonRegex.match(this.TextData);
                    if (m.getSuccess()) {
                        retVal = m.getIndex();
                    }
                }
                return retVal;
            },
            CalculateSimAverages: function (ratings) {
                var $t;
                var retVal = new TSBTool.SimAverages();
                $t = Bridge.getEnumerator(ratings);
                try {
                    while ($t.moveNext()) {
                        var rat = $t.Current;
                        retVal.QB_ave += rat.qbRating;
                        retVal.RB_ave += rat.rb1Rating;
                        retVal.WR_ave += (rat.wr1Rating + rat.wr2Rating);
                        retVal.TE_ave += rat.teRating;
                        retVal.OL_ave += rat.olRating;
                        retVal.DL_run_ave += rat.dlRunDefenseRating;
                        retVal.DL_pass_ave += rat.dlPassDefenseRating;
                        retVal.LB_run_ave += rat.lbRunDefenseRating;
                        retVal.LB_pass_ave += rat.lbPassDefenseRating;
                        retVal.DB_run_ave += rat.dbRunDefenseRating;
                        retVal.DB_pass_ave += rat.dbPassDefenseRating;
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
                retVal.QB_ave = retVal.QB_ave / ratings.Count;
                retVal.RB_ave = retVal.RB_ave / ratings.Count;
                retVal.WR_ave = retVal.WR_ave / (Bridge.Int.mul(ratings.Count, 2));
                retVal.TE_ave = retVal.TE_ave / ratings.Count;
                retVal.OL_ave = retVal.OL_ave / ratings.Count;
                retVal.DL_run_ave = retVal.DL_run_ave / ratings.Count;
                retVal.DL_pass_ave = retVal.DL_pass_ave / ratings.Count;
                retVal.LB_run_ave = retVal.LB_run_ave / ratings.Count;
                retVal.LB_pass_ave = retVal.LB_pass_ave / ratings.Count;
                retVal.DB_run_ave = retVal.DB_run_ave / ratings.Count;
                retVal.DB_pass_ave = retVal.DB_pass_ave / ratings.Count;
                return retVal.$clone();
            },
            GetTeamsRatings: function (season, teams) {
                var $t;
                var retVal = new (System.Collections.Generic.List$1(TSBTool.TeamRatings)).$ctor2(teams.Count);
                var seasonChunk = this.GetSeasonText(season);
                $t = Bridge.getEnumerator(teams);
                try {
                    while ($t.moveNext()) {
                        var team = $t.Current;
                        retVal.add(this.GetTeamRatings(seasonChunk, team));
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
                return retVal;
            },
            GetTeamRatings: function (textData, team) {
                var retVal = new TSBTool.TeamRatings();
                var RS = 0, RP = 1, MS = 2, HP = 3, PS = 4, PC = 5, PA = 6, APB = 7, BC = 4, RC = 5, PI = 4, QU = 5;
                if (TSBTool.StaticUtils.GetContentType(this.TextData) === TSBTool.TSBContentType.TSB2) {
                    PS = 5;
                    PC = 6;
                    PA = 7;
                    APB = 8;
                    BC = 5;
                    RC = 6;
                    PI = 5;
                    QU = 6;
                } else if (TSBTool.StaticUtils.GetContentType(this.TextData) === TSBTool.TSBContentType.TSB3) {
                    PS = 6;
                    PC = 7;
                    PA = 8;
                    APB = 9;
                    BC = 6;
                    RC = 7;
                    PI = 6;
                    QU = 7;
                }

                var qb1 = this.GetPlayerInts(textData, team, "QB1");
                var rb1 = this.GetPlayerInts(textData, team, "RB1");
                var rb2 = this.GetPlayerInts(textData, team, "RB2");
                var wr1 = this.GetPlayerInts(textData, team, "WR1");
                var wr2 = this.GetPlayerInts(textData, team, "WR2");
                var te1 = this.GetPlayerInts(textData, team, "TE1");

                var center = this.GetPlayerInts(textData, team, "C");
                var lg = this.GetPlayerInts(textData, team, "LG");
                var rg = this.GetPlayerInts(textData, team, "RG");
                var lt = this.GetPlayerInts(textData, team, "LT");
                var rt = this.GetPlayerInts(textData, team, "RT");

                var re = this.GetPlayerInts(textData, team, "RE");
                var nt = this.GetPlayerInts(textData, team, "NT");
                var le = this.GetPlayerInts(textData, team, "LE");

                var rolb = this.GetPlayerInts(textData, team, "ROLB");
                var rilb = this.GetPlayerInts(textData, team, "RILB");
                var lilb = this.GetPlayerInts(textData, team, "LILB");
                var lolb = this.GetPlayerInts(textData, team, "LOLB");

                var rcb = this.GetPlayerInts(textData, team, "RCB");
                var lcb = this.GetPlayerInts(textData, team, "LCB");
                var fs = this.GetPlayerInts(textData, team, "FS");
                var ss = this.GetPlayerInts(textData, team, "SS");

                retVal.team = team;
                retVal.qbRating = (qb1[System.Array.index(RS, qb1)] * 0.06) + (qb1[System.Array.index(RP, qb1)] * 0.02) + (qb1[System.Array.index(MS, qb1)] * 0.12) + (qb1[System.Array.index(HP, qb1)] * 0.02) + (qb1[System.Array.index(PS, qb1)] * 0.22) + (qb1[System.Array.index(PC, qb1)] * 0.23) + (qb1[System.Array.index(PA, qb1)] * 0.23) + (qb1[System.Array.index(APB, qb1)] * 0.1);
                retVal.rb1Rating = (rb1[System.Array.index(RS, rb1)] * 0.15) + (rb1[System.Array.index(RP, rb1)] * 0.15) + (rb1[System.Array.index(MS, rb1)] * 0.4) + (rb1[System.Array.index(HP, rb1)] * 0.25) + (rb1[System.Array.index(BC, rb1)] * 0.02) + (rb1[System.Array.index(RC, rb1)] * 0.03);
                retVal.rb2Rating = (rb2[System.Array.index(RS, rb2)] * 0.15) + (rb2[System.Array.index(RP, rb2)] * 0.15) + (rb2[System.Array.index(MS, rb2)] * 0.4) + (rb2[System.Array.index(HP, rb2)] * 0.25) + (rb2[System.Array.index(BC, rb2)] * 0.02) + (rb2[System.Array.index(RC, rb2)] * 0.03);
                retVal.wr1Rating = (wr1[System.Array.index(RS, wr1)] * 0.2) + (wr1[System.Array.index(RP, wr1)] * 0.15) + (wr1[System.Array.index(MS, wr1)] * 0.25) + (wr1[System.Array.index(HP, wr1)] * 0.03) + (wr1[System.Array.index(BC, wr1)] * 0.02) + (wr1[System.Array.index(RC, wr1)] * 0.35);
                retVal.wr2Rating = (wr2[System.Array.index(RS, wr2)] * 0.2) + (wr2[System.Array.index(RP, wr2)] * 0.15) + (wr2[System.Array.index(MS, wr2)] * 0.25) + (wr2[System.Array.index(HP, wr2)] * 0.03) + (wr2[System.Array.index(BC, wr2)] * 0.02) + (wr2[System.Array.index(RC, wr2)] * 0.35);
                retVal.teRating = (te1[System.Array.index(RS, te1)] * 0.18) + (te1[System.Array.index(RP, te1)] * 0.1) + (te1[System.Array.index(MS, te1)] * 0.25) + (te1[System.Array.index(HP, te1)] * 0.25) + (te1[System.Array.index(BC, te1)] * 0.02) + (te1[System.Array.index(RC, te1)] * 0.2);
                retVal.olRating = (center[System.Array.index(RS, center)] * 0.02) + (center[System.Array.index(RP, center)] * 0.01) + (center[System.Array.index(MS, center)] * 0.02) + (center[System.Array.index(HP, center)] * 0.95) + (lg[System.Array.index(RS, lg)] * 0.02) + (lg[System.Array.index(RP, lg)] * 0.01) + (lg[System.Array.index(MS, lg)] * 0.02) + (lg[System.Array.index(HP, lg)] * 0.95) + (rg[System.Array.index(RS, rg)] * 0.02) + (rg[System.Array.index(RP, rg)] * 0.01) + (rg[System.Array.index(MS, rg)] * 0.02) + (rg[System.Array.index(HP, rg)] * 0.95) + (lt[System.Array.index(RS, lt)] * 0.02) + (lt[System.Array.index(RP, lt)] * 0.01) + (lt[System.Array.index(MS, lt)] * 0.02) + (lt[System.Array.index(HP, lt)] * 0.95) + (rt[System.Array.index(RS, rt)] * 0.02) + (rt[System.Array.index(RP, rt)] * 0.01) + (rt[System.Array.index(MS, rt)] * 0.02) + (rt[System.Array.index(HP, rt)] * 0.95);
                retVal.dlRunDefenseRating = (re[System.Array.index(RS, re)] * 0.05) + (re[System.Array.index(RP, re)] * 0.05) + (re[System.Array.index(MS, re)] * 0.05) + (re[System.Array.index(HP, re)] * 0.85) + (nt[System.Array.index(RS, nt)] * 0.05) + (nt[System.Array.index(RP, nt)] * 0.05) + (nt[System.Array.index(MS, nt)] * 0.05) + (nt[System.Array.index(HP, nt)] * 0.85) + (le[System.Array.index(RS, le)] * 0.05) + (le[System.Array.index(RP, le)] * 0.05) + (le[System.Array.index(MS, le)] * 0.05) + (le[System.Array.index(HP, le)] * 0.85);
                retVal.dlPassDefenseRating = (re[System.Array.index(RS, re)] * 0.05) + (re[System.Array.index(RP, re)] * 0.05) + (re[System.Array.index(MS, re)] * 0.05) + (re[System.Array.index(HP, re)] * 0.75) + (re[System.Array.index(PI, re)] * 0.05) + (re[System.Array.index(QU, re)] * 0.05) + (nt[System.Array.index(RS, nt)] * 0.05) + (nt[System.Array.index(RP, nt)] * 0.05) + (nt[System.Array.index(MS, nt)] * 0.05) + (nt[System.Array.index(HP, nt)] * 0.75) + (nt[System.Array.index(PI, nt)] * 0.05) + (nt[System.Array.index(QU, nt)] * 0.05) + (le[System.Array.index(RS, le)] * 0.05) + (le[System.Array.index(RP, le)] * 0.05) + (le[System.Array.index(MS, le)] * 0.05) + (le[System.Array.index(HP, le)] * 0.75) + (le[System.Array.index(PI, le)] * 0.05) + (le[System.Array.index(QU, le)] * 0.05);
                retVal.lbRunDefenseRating = (rolb[System.Array.index(RS, rolb)] * 0.25) + (rolb[System.Array.index(RP, rolb)] * 0.25) + (rolb[System.Array.index(MS, rolb)] * 0.1) + (rolb[System.Array.index(HP, rolb)] * 0.4) + (rilb[System.Array.index(RS, rilb)] * 0.25) + (rilb[System.Array.index(RP, rilb)] * 0.25) + (rilb[System.Array.index(MS, rilb)] * 0.1) + (rilb[System.Array.index(HP, rilb)] * 0.4) + (lilb[System.Array.index(RS, lilb)] * 0.25) + (lilb[System.Array.index(RP, lilb)] * 0.25) + (lilb[System.Array.index(MS, lilb)] * 0.1) + (lilb[System.Array.index(HP, lilb)] * 0.4) + (lolb[System.Array.index(RS, lolb)] * 0.25) + (lolb[System.Array.index(RP, lolb)] * 0.25) + (lolb[System.Array.index(MS, lolb)] * 0.1) + (lolb[System.Array.index(HP, lolb)] * 0.4);
                retVal.lbPassDefenseRating = (rolb[System.Array.index(RS, rolb)] * 0.15) + (rolb[System.Array.index(RP, rolb)] * 0.15) + (rolb[System.Array.index(MS, rolb)] * 0.1) + (rolb[System.Array.index(PI, rolb)] * 0.3) + (rolb[System.Array.index(QU, rolb)] * 0.3) + (rilb[System.Array.index(RS, rilb)] * 0.15) + (rilb[System.Array.index(RP, rilb)] * 0.15) + (rilb[System.Array.index(MS, rilb)] * 0.1) + (rilb[System.Array.index(PI, rilb)] * 0.3) + (rilb[System.Array.index(QU, rilb)] * 0.3) + (lilb[System.Array.index(RS, lilb)] * 0.15) + (lilb[System.Array.index(RP, lilb)] * 0.15) + (lilb[System.Array.index(MS, lilb)] * 0.1) + (lilb[System.Array.index(PI, lilb)] * 0.3) + (lilb[System.Array.index(QU, lilb)] * 0.3) + (lolb[System.Array.index(RS, lolb)] * 0.15) + (lolb[System.Array.index(RP, lolb)] * 0.15) + (lolb[System.Array.index(MS, lolb)] * 0.1) + (lolb[System.Array.index(PI, lolb)] * 0.3) + (lolb[System.Array.index(QU, lolb)] * 0.3);
                retVal.dbRunDefenseRating = (rcb[System.Array.index(RS, rcb)] * 0.25) + (rcb[System.Array.index(RP, rcb)] * 0.25) + (rcb[System.Array.index(MS, rcb)] * 0.1) + (rcb[System.Array.index(HP, rcb)] * 0.4) + (lcb[System.Array.index(RS, lcb)] * 0.25) + (lcb[System.Array.index(RP, lcb)] * 0.25) + (lcb[System.Array.index(MS, lcb)] * 0.1) + (lcb[System.Array.index(HP, lcb)] * 0.4) + (fs[System.Array.index(RS, fs)] * 0.25) + (fs[System.Array.index(RP, fs)] * 0.25) + (fs[System.Array.index(MS, fs)] * 0.1) + (fs[System.Array.index(HP, fs)] * 0.4) + (ss[System.Array.index(RS, ss)] * 0.25) + (ss[System.Array.index(RP, ss)] * 0.25) + (ss[System.Array.index(MS, ss)] * 0.1) + (ss[System.Array.index(HP, ss)] * 0.4);
                retVal.dbPassDefenseRating = (rcb[System.Array.index(RS, rcb)] * 0.15) + (rcb[System.Array.index(RP, rcb)] * 0.15) + (rcb[System.Array.index(MS, rcb)] * 0.1) + (rcb[System.Array.index(PI, rcb)] * 0.3) + (rcb[System.Array.index(QU, rcb)] * 0.3) + (lcb[System.Array.index(RS, lcb)] * 0.15) + (lcb[System.Array.index(RP, lcb)] * 0.15) + (lcb[System.Array.index(MS, lcb)] * 0.1) + (lcb[System.Array.index(PI, lcb)] * 0.3) + (lcb[System.Array.index(QU, lcb)] * 0.3) + (fs[System.Array.index(RS, fs)] * 0.15) + (fs[System.Array.index(RP, fs)] * 0.15) + (fs[System.Array.index(MS, fs)] * 0.1) + (fs[System.Array.index(PI, fs)] * 0.3) + (fs[System.Array.index(QU, fs)] * 0.3) + (ss[System.Array.index(RS, ss)] * 0.15) + (ss[System.Array.index(RP, ss)] * 0.15) + (ss[System.Array.index(MS, ss)] * 0.1) + (ss[System.Array.index(PI, ss)] * 0.3) + (ss[System.Array.index(QU, ss)] * 0.3);
                return retVal;
            },
            /**
             * Gets a player 'line' from m_Data from 'team' playing 'position'.
             *
             * @instance
             * @private
             * @this TSBTool.TecmonsterTeamSim
             * @memberof TSBTool.TecmonsterTeamSim
             * @param   {string}            seasonChunk    
             * @param   {string}            team           
             * @param   {string}            position
             * @return  {Array.<number>}
             */
            GetPlayerInts: function (seasonChunk, team, position) {
                var pattern = "TEAM\\s*=\\s*" + (team || "");
                var findTeamRegex = new System.Text.RegularExpressions.Regex.ctor(pattern);
                var m = findTeamRegex.match(seasonChunk);
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    var teamIndex = m.getIndex();
                    if (teamIndex === -1) {
                        return null;
                    }
                    var playerIndex = -1;
                    var endLineRegex = new System.Text.RegularExpressions.Regex.ctor(System.String.format("\n\\s*{0}\\s*,", [position]));
                    var eol = endLineRegex.match(seasonChunk, teamIndex);
                    if (!Bridge.referenceEquals(eol, System.Text.RegularExpressions.Match.getEmpty())) {
                        playerIndex = eol.getIndex();
                    }
                    playerIndex = (playerIndex + 1) | 0;

                    if (playerIndex === 0) {
                        return null;
                    }
                    var lineEnd = System.String.indexOf(seasonChunk, "\n", playerIndex);
                    var playerLine = seasonChunk.substr(playerIndex, ((lineEnd - playerIndex) | 0));
                    return TSBTool.InputParser.GetInts$1(playerLine, false);
                }
                return null;
            },
            GetTeams: function (season) {
                var $t;
                var seasonChunk = this.GetSeasonText(season);
                var retVal = new (System.Collections.Generic.List$1(System.String)).$ctor2(35);
                var teamRegex = new System.Text.RegularExpressions.Regex.ctor("TEAM\\s*=\\s*([a-z0-9]+)");
                var mc = teamRegex.matches(seasonChunk);
                $t = Bridge.getEnumerator(mc);
                try {
                    while ($t.moveNext()) {
                        var m = Bridge.cast($t.Current, System.Text.RegularExpressions.Match);
                        var team = m.getGroups().get(1).getValue();
                        retVal.add(team);
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
                return retVal;
            },
            GetSeasonText: function (season) {
                var start = 0;
                var end = this.TextData.length;
                var reg = new System.Text.RegularExpressions.Regex.ctor(System.String.format("^\\s*SEASON\\s+({0})", [Bridge.box(season, System.Int32)]));
                var m = reg.match(this.TextData);
                if (m.getSuccess()) {
                    var index = System.String.indexOf(this.TextData, "SEASON", ((m.getIndex() + 50) | 0));
                    if (index > -1) {
                        end = index;
                    }
                }
                var seasonChunk = this.TextData.substr(start, end);
                return seasonChunk;
            }
        }
    });

    Bridge.define("TSBTool.TecmonsterTSB1SimAutoUpdater", {
        statics: {
            fields: {
                PositionNames: null
            },
            ctors: {
                init: function () {
                    this.PositionNames = function (_o1) {
                            _o1.add("QB1");
                            _o1.add("QB2");
                            _o1.add("RB1");
                            _o1.add("RB2");
                            _o1.add("RB3");
                            _o1.add("RB4");
                            _o1.add("WR1");
                            _o1.add("WR2");
                            _o1.add("WR3");
                            _o1.add("WR4");
                            _o1.add("TE1");
                            _o1.add("TE2");
                            _o1.add("C");
                            _o1.add("LG");
                            _o1.add("RG");
                            _o1.add("LT");
                            _o1.add("RT");
                            _o1.add("RE");
                            _o1.add("NT");
                            _o1.add("LE");
                            _o1.add("ROLB");
                            _o1.add("RILB");
                            _o1.add("LILB");
                            _o1.add("LOLB");
                            _o1.add("RCB");
                            _o1.add("LCB");
                            _o1.add("FS");
                            _o1.add("SS");
                            _o1.add("K");
                            _o1.add("P");
                            return _o1;
                        }(new (System.Collections.Generic.List$1(System.String)).ctor());
                }
            },
            methods: {
                AutoUpdatePlayerSimData: function (input) {
                    var tmp = new TSBTool.TecmonsterTSB1SimAutoUpdater();
                    tmp.Data = input;
                    tmp.AutoUpdatePlayerSim();
                    return tmp.Data;
                }
            }
        },
        fields: {
            mTeams: null,
            mSimStuff: null,
            mData: null,
            m_SimRegex: null
        },
        props: {
            /**
             * The text data to work on and retrieve.
             *
             * @instance
             * @protected
             * @memberof TSBTool.TecmonsterTSB1SimAutoUpdater
             * @function Data
             * @type string
             */
            Data: {
                get: function () {
                    return this.mData;
                },
                set: function (value) {
                    var $t;
                    this.mTeams.clear();
                    this.mData = value;
                    var findTeamRegex = new System.Text.RegularExpressions.Regex.ctor("TEAM\\s*=\\s*([a-z49]+)");
                    var mc = findTeamRegex.matches(this.mData);
                    $t = Bridge.getEnumerator(mc);
                    try {
                        while ($t.moveNext()) {
                            var m = Bridge.cast($t.Current, System.Text.RegularExpressions.Match);
                            this.mTeams.add(m.getGroups().get(1).toString());
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                }
            }
        },
        ctors: {
            init: function () {
                this.mTeams = new (System.Collections.Generic.List$1(System.String)).ctor();
                this.mSimStuff = new TSBTool.TecmonsterSimStuff();
                this.mData = "";
            }
        },
        methods: {
            /**
             * Gets a player 'line' from m_Data from 'team' playing 'position'.
             *
             * @instance
             * @private
             * @this TSBTool.TecmonsterTSB1SimAutoUpdater
             * @memberof TSBTool.TecmonsterTSB1SimAutoUpdater
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {string}
             */
            GetPlayerString: function (team, position) {
                var pattern = "TEAM\\s*=\\s*" + (team || "");
                var findTeamRegex = new System.Text.RegularExpressions.Regex.ctor(pattern);
                var m = findTeamRegex.match(this.mData);
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    var teamIndex = m.getIndex();
                    if (teamIndex === -1) {
                        return null;
                    }
                    var playerIndex = -1;
                    var endLineRegex = new System.Text.RegularExpressions.Regex.ctor(System.String.format("\n\\s*{0}\\s*,", [position]));
                    var eol = endLineRegex.match(this.mData, teamIndex);
                    if (!Bridge.referenceEquals(eol, System.Text.RegularExpressions.Match.getEmpty())) {
                        playerIndex = eol.getIndex();
                    }
                    playerIndex = (playerIndex + 1) | 0;

                    if (playerIndex === 0) {
                        return null;
                    }
                    var lineEnd = System.String.indexOf(this.mData, "\n", playerIndex);
                    var playerLine = this.mData.substr(playerIndex, ((lineEnd - playerIndex) | 0));
                    return playerLine;
                }
                return null;
            },
            ReplacePlayer: function (team, oldPlayer, newPlayer) {
                var nextTeamIndex = -1;
                var currentTeamIndex = -1;
                var nextTeam = null;

                var findTeamRegex = new System.Text.RegularExpressions.Regex.ctor("TEAM\\s*=\\s*" + (team || ""));

                var m = findTeamRegex.match(this.mData);
                if (!m.getSuccess()) {
                    return;
                }

                currentTeamIndex = m.getGroups().get(1).getIndex();

                var test = this.mTeams.indexOf(team);

                if (test !== ((this.mTeams.Count - 1) | 0)) {
                    nextTeam = System.String.format("TEAM\\s*=\\s*{0}", [this.mTeams.getItem(((test + 1) | 0))]);
                    var nextTeamRegex = new System.Text.RegularExpressions.Regex.ctor(nextTeam);
                    var nt = nextTeamRegex.match(this.mData);
                    if (nt.getSuccess()) {
                        nextTeamIndex = nt.getIndex();
                    }
                }
                if (nextTeamIndex < 0) {
                    nextTeamIndex = this.mData.length;
                }


                var playerIndex = System.String.indexOf(this.mData, oldPlayer, currentTeamIndex);
                if (playerIndex > -1) {
                    var endLine = System.String.indexOf(this.mData, String.fromCharCode(10), playerIndex);
                    var start = this.mData.substr(0, playerIndex);
                    var last = this.mData.substr(endLine);

                    var tmp = new System.Text.StringBuilder("", ((this.mData.length + 200) | 0));
                    tmp.append(start);
                    tmp.append(newPlayer);
                    tmp.append(last);

                    this.mData = tmp.toString();
                } else {
                    var error = System.String.format("An error occured looking up player\r\n     '{0}'\r\nPlease verify that this player's attributes are correct.", [oldPlayer]);
                    TSBTool.StaticUtils.AddError(error);
                }
            },
            /**
             * Update all players sim attributes.
             *
             * @instance
             * @private
             * @this TSBTool.TecmonsterTSB1SimAutoUpdater
             * @memberof TSBTool.TecmonsterTSB1SimAutoUpdater
             * @return  {void}
             */
            AutoUpdatePlayerSim: function () {
                var $t;
                $t = Bridge.getEnumerator(this.mTeams);
                try {
                    while ($t.moveNext()) {
                        var team = $t.Current;
                        this.AutoUpdatePlayers(team);
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
            },
            /**
             * Auto update a team's players sim attributs.
             *
             * @instance
             * @private
             * @this TSBTool.TecmonsterTSB1SimAutoUpdater
             * @memberof TSBTool.TecmonsterTSB1SimAutoUpdater
             * @param   {string}    team
             * @return  {void}
             */
            AutoUpdatePlayers: function (team) {
                var $t;
                var pattern = "TEAM\\s*=\\s*" + (team || "");
                var findTeamRegex = new System.Text.RegularExpressions.Regex.ctor(pattern);
                var m = findTeamRegex.match(this.mData);
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    $t = Bridge.getEnumerator(TSBTool.TecmonsterTSB1SimAutoUpdater.PositionNames);
                    try {
                        while ($t.moveNext()) {
                            var position = $t.Current;
                            if (Bridge.referenceEquals(position, "C")) {
                                break;
                            }
                            this.AutoUpdatePlayerSimData(team, position);
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    this.AutoUpdatePlayerSimData(team, "P");
                    this.AutoUpdatePlayerSimData(team, "K");
                    this.UpdateTeamSimPassDefense(team);
                    this.UpdateTeamSimPassRush(team);
                }
            },
            /**
             * Auto update a player's sim data.
             *
             * @instance
             * @private
             * @this TSBTool.TecmonsterTSB1SimAutoUpdater
             * @memberof TSBTool.TecmonsterTSB1SimAutoUpdater
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {void}
             */
            AutoUpdatePlayerSimData: function (team, position) {
                var oldValue = this.GetPlayerString(team, position);
                var newValue = null;

                if (oldValue == null || Bridge.referenceEquals(oldValue, "")) {
                    return;
                }
                var fName = TSBTool.InputParser.GetFirstName(oldValue);
                var lName = TSBTool.InputParser.GetLastName(oldValue);
                var face = TSBTool.InputParser.GetFace(oldValue);
                var jerseyNumber = TSBTool.InputParser.GetJerseyNumber(oldValue);

                var attrs = System.Array.init(4, 0, System.Int32);
                try {
                    attrs = TSBTool.InputParser.GetInts$1(oldValue, false);
                } catch (e) {
                    e = System.Exception.create(e);
                    TSBTool.StaticUtils.ShowError("Oh oh!" + (e.Message || ""));
                }
                var simPass, simRush, simPocket, simCatch, simTargets, simYPC, simKA;

                switch (position) {
                    case "QB1": 
                    case "QB2": 
                        if (attrs != null && attrs.length > 7) {
                            simRush = this.mSimStuff.QbSimRun(attrs[System.Array.index(0, attrs)], attrs[System.Array.index(1, attrs)], attrs[System.Array.index(2, attrs)]);
                            simPass = this.mSimStuff.SimPass(attrs[System.Array.index(4, attrs)], attrs[System.Array.index(5, attrs)], attrs[System.Array.index(6, attrs)], attrs[System.Array.index(7, attrs)]);
                            simPocket = this.mSimStuff.SimPocket(attrs[System.Array.index(0, attrs)], attrs[System.Array.index(2, attrs)]);
                            newValue = System.String.format("{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12} ,[{13}, {14}, {15} ]", position, fName, lName, Bridge.box(face, System.Int32), Bridge.box(jerseyNumber, System.Int32), Bridge.box(attrs[System.Array.index(0, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(1, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(2, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(3, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(4, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(5, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(6, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(7, attrs)], System.Int32), Bridge.box(simRush, System.Int32), Bridge.box(simPass, System.Int32), Bridge.box(simPocket, System.Int32));
                        }
                        break;
                    case "RB1": 
                    case "RB2": 
                    case "RB3": 
                    case "RB4": 
                        if (attrs != null && attrs.length > 5) {
                            simRush = this.mSimStuff.RbSimRush(attrs[System.Array.index(0, attrs)], attrs[System.Array.index(1, attrs)], attrs[System.Array.index(3, attrs)], attrs[System.Array.index(4, attrs)]);
                            simCatch = this.mSimStuff.SimCatch(attrs[System.Array.index(5, attrs)]);
                            simYPC = this.mSimStuff.SimYPC(attrs[System.Array.index(2, attrs)], attrs[System.Array.index(5, attrs)]);
                            simTargets = this.mSimStuff.SimTargets(attrs[System.Array.index(2, attrs)]);
                            newValue = System.String.format("{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11}, {12} ,{13}, {14} ]", position, fName, lName, Bridge.box(face, System.Int32), Bridge.box(jerseyNumber, System.Int32), Bridge.box(attrs[System.Array.index(0, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(1, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(2, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(3, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(4, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(5, attrs)], System.Int32), Bridge.box(simRush, System.Int32), Bridge.box(simCatch, System.Int32), Bridge.box(simYPC, System.Int32), Bridge.box(simCatch, System.Int32));
                        }
                        break;
                    case "WR1": 
                    case "WR2": 
                    case "WR3": 
                    case "WR4": 
                    case "TE1": 
                    case "TE2": 
                        if (attrs != null && attrs.length > 5) {
                            simRush = 0;
                            simCatch = this.mSimStuff.SimCatch(attrs[System.Array.index(5, attrs)]);
                            simYPC = this.mSimStuff.SimYPC(attrs[System.Array.index(2, attrs)], attrs[System.Array.index(5, attrs)]);
                            simTargets = this.mSimStuff.SimTargets(attrs[System.Array.index(2, attrs)]);
                            newValue = System.String.format("{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11}, {12} ,{13}, {14} ]", position, fName, lName, Bridge.box(face, System.Int32), Bridge.box(jerseyNumber, System.Int32), Bridge.box(attrs[System.Array.index(0, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(1, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(2, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(3, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(4, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(5, attrs)], System.Int32), Bridge.box(simRush, System.Int32), Bridge.box(simCatch, System.Int32), Bridge.box(simYPC, System.Int32), Bridge.box(simCatch, System.Int32));
                        }
                        break;
                    case "P": 
                    case "K": 
                        if (attrs != null && attrs.length > 5) {
                            simKA = this.mSimStuff.PKSimKick(attrs[System.Array.index(4, attrs)], attrs[System.Array.index(5, attrs)]);
                            newValue = System.String.format("{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11} ]", position, fName, lName, Bridge.box(face, System.Int32), Bridge.box(jerseyNumber, System.Int32), Bridge.box(attrs[System.Array.index(0, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(1, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(2, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(3, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(4, attrs)], System.Int32), Bridge.box(attrs[System.Array.index(5, attrs)], System.Int32), Bridge.box(simKA, System.Int32));
                        }
                        break;
                }
                if (newValue != null) {
                    this.ReplacePlayer(team, oldValue, newValue);
                }
            },
            UpdateTeamSimPassDefense: function (team) {
                var re = this.GetPlayerString(team, "RE");
                var le = this.GetPlayerString(team, "LE");
                var nt = this.GetPlayerString(team, "NT");
                var lolb = this.GetPlayerString(team, "LOLB");
                var lilb = this.GetPlayerString(team, "LILB");
                var rilb = this.GetPlayerString(team, "RILB");
                var rolb = this.GetPlayerString(team, "ROLB");
                var rcb = this.GetPlayerString(team, "RCB");
                var lcb = this.GetPlayerString(team, "LCB");
                var fs = this.GetPlayerString(team, "FS");
                var ss = this.GetPlayerString(team, "SS");

                if (re == null || le == null || nt == null || lolb == null || lilb == null || rilb == null || rolb == null || rcb == null || lcb == null || fs == null || ss == null) {
                    return;
                }
                var reAttrs = TSBTool.InputParser.GetInts$1(re, false);
                var leAttrs = TSBTool.InputParser.GetInts$1(le, false);
                var ntAttrs = TSBTool.InputParser.GetInts$1(nt, false);
                var lolbAttrs = TSBTool.InputParser.GetInts$1(lolb, false);
                var lilbAttrs = TSBTool.InputParser.GetInts$1(lilb, false);
                var rilbAttrs = TSBTool.InputParser.GetInts$1(rilb, false);
                var rolbAttrs = TSBTool.InputParser.GetInts$1(rolb, false);
                var rcbAttrs = TSBTool.InputParser.GetInts$1(rcb, false);
                var lcbAttrs = TSBTool.InputParser.GetInts$1(lcb, false);
                var fsAttrs = TSBTool.InputParser.GetInts$1(fs, false);
                var ssAttrs = TSBTool.InputParser.GetInts$1(ss, false);

                var passDef = this.mSimStuff.GetSimPassDefense(rolbAttrs[System.Array.index(4, rolbAttrs)], rilbAttrs[System.Array.index(4, rilbAttrs)], lilbAttrs[System.Array.index(4, lilbAttrs)], lolbAttrs[System.Array.index(4, lolbAttrs)], rcbAttrs[System.Array.index(4, rcbAttrs)], lcbAttrs[System.Array.index(4, lcbAttrs)], fsAttrs[System.Array.index(4, fsAttrs)], ssAttrs[System.Array.index(4, ssAttrs)]);

                this.ReplacePlayer(team, re, this.ReplaceSimAttr(re, 2, 0));
                this.ReplacePlayer(team, nt, this.ReplaceSimAttr(nt, 2, 0));
                this.ReplacePlayer(team, le, this.ReplaceSimAttr(le, 2, 0));
                this.ReplacePlayer(team, rolb, this.ReplaceSimAttr(rolb, 2, passDef[System.Array.index(0, passDef)]));
                this.ReplacePlayer(team, rilb, this.ReplaceSimAttr(rilb, 2, passDef[System.Array.index(1, passDef)]));
                this.ReplacePlayer(team, lilb, this.ReplaceSimAttr(lilb, 2, passDef[System.Array.index(2, passDef)]));
                this.ReplacePlayer(team, lolb, this.ReplaceSimAttr(lolb, 2, passDef[System.Array.index(3, passDef)]));
                this.ReplacePlayer(team, rcb, this.ReplaceSimAttr(rcb, 2, passDef[System.Array.index(4, passDef)]));
                this.ReplacePlayer(team, lcb, this.ReplaceSimAttr(lcb, 2, passDef[System.Array.index(5, passDef)]));
                this.ReplacePlayer(team, fs, this.ReplaceSimAttr(fs, 2, passDef[System.Array.index(6, passDef)]));
                this.ReplacePlayer(team, ss, this.ReplaceSimAttr(ss, 2, passDef[System.Array.index(7, passDef)]));

                var overallSimPassDef = 0;
                if (reAttrs[System.Array.index(2, reAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (reAttrs[System.Array.index(4, reAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (leAttrs[System.Array.index(2, leAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (leAttrs[System.Array.index(4, leAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (ntAttrs[System.Array.index(2, ntAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (ntAttrs[System.Array.index(4, ntAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (lolbAttrs[System.Array.index(2, lolbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (lolbAttrs[System.Array.index(4, lolbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (lilbAttrs[System.Array.index(2, lilbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (lilbAttrs[System.Array.index(4, lilbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (rilbAttrs[System.Array.index(2, rilbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (rilbAttrs[System.Array.index(4, rilbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (rolbAttrs[System.Array.index(2, rolbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (rolbAttrs[System.Array.index(4, rolbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (rcbAttrs[System.Array.index(2, rcbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (rcbAttrs[System.Array.index(4, rcbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (lcbAttrs[System.Array.index(2, lcbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (lcbAttrs[System.Array.index(4, lcbAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (fsAttrs[System.Array.index(2, fsAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (fsAttrs[System.Array.index(4, fsAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (ssAttrs[System.Array.index(2, ssAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
                if (ssAttrs[System.Array.index(4, ssAttrs)] > 49) {
                    overallSimPassDef = (overallSimPassDef + 1) | 0;
                }
            },
            UpdateTeamSimPassRush: function (team) {
                var re = this.GetPlayerString(team, "RE");
                var le = this.GetPlayerString(team, "LE");
                var nt = this.GetPlayerString(team, "NT");
                var lolb = this.GetPlayerString(team, "LOLB");
                var lilb = this.GetPlayerString(team, "LILB");
                var rilb = this.GetPlayerString(team, "RILB");
                var rolb = this.GetPlayerString(team, "ROLB");
                var rcb = this.GetPlayerString(team, "RCB");
                var lcb = this.GetPlayerString(team, "LCB");
                var fs = this.GetPlayerString(team, "FS");
                var ss = this.GetPlayerString(team, "SS");

                if (re == null || le == null || nt == null || lolb == null || lilb == null || rilb == null || rolb == null || rcb == null || lcb == null || fs == null || ss == null) {
                    return;
                }
                var reAttrs = TSBTool.InputParser.GetInts$1(re, false);
                var leAttrs = TSBTool.InputParser.GetInts$1(le, false);
                var ntAttrs = TSBTool.InputParser.GetInts$1(nt, false);
                var lolbAttrs = TSBTool.InputParser.GetInts$1(lolb, false);
                var lilbAttrs = TSBTool.InputParser.GetInts$1(lilb, false);
                var rilbAttrs = TSBTool.InputParser.GetInts$1(rilb, false);
                var rolbAttrs = TSBTool.InputParser.GetInts$1(rolb, false);
                var rcbAttrs = TSBTool.InputParser.GetInts$1(rcb, false);
                var lcbAttrs = TSBTool.InputParser.GetInts$1(lcb, false);
                var fsAttrs = TSBTool.InputParser.GetInts$1(fs, false);
                var ssAttrs = TSBTool.InputParser.GetInts$1(ss, false);

                var rushDef = this.mSimStuff.GetSimPassRush(((reAttrs[System.Array.index(2, reAttrs)] + reAttrs[System.Array.index(3, reAttrs)]) | 0), ((ntAttrs[System.Array.index(2, ntAttrs)] + ntAttrs[System.Array.index(3, ntAttrs)]) | 0), ((leAttrs[System.Array.index(2, leAttrs)] + leAttrs[System.Array.index(3, leAttrs)]) | 0), ((rolbAttrs[System.Array.index(2, rolbAttrs)] + rolbAttrs[System.Array.index(3, rolbAttrs)]) | 0), ((rilbAttrs[System.Array.index(2, rilbAttrs)] + rilbAttrs[System.Array.index(3, rilbAttrs)]) | 0), ((lilbAttrs[System.Array.index(2, lilbAttrs)] + lilbAttrs[System.Array.index(3, lilbAttrs)]) | 0), ((lolbAttrs[System.Array.index(2, lolbAttrs)] + lolbAttrs[System.Array.index(3, lolbAttrs)]) | 0));

                this.ReplacePlayer(team, re, this.ReplaceSimAttr(re, 1, rushDef[System.Array.index(0, rushDef)]));
                this.ReplacePlayer(team, nt, this.ReplaceSimAttr(nt, 1, rushDef[System.Array.index(1, rushDef)]));
                this.ReplacePlayer(team, le, this.ReplaceSimAttr(le, 1, rushDef[System.Array.index(2, rushDef)]));
                this.ReplacePlayer(team, rolb, this.ReplaceSimAttr(rolb, 1, rushDef[System.Array.index(3, rushDef)]));
                this.ReplacePlayer(team, rilb, this.ReplaceSimAttr(rilb, 1, rushDef[System.Array.index(4, rushDef)]));
                this.ReplacePlayer(team, lilb, this.ReplaceSimAttr(lilb, 1, rushDef[System.Array.index(5, rushDef)]));
                this.ReplacePlayer(team, lolb, this.ReplaceSimAttr(lolb, 1, rushDef[System.Array.index(6, rushDef)]));
                this.ReplacePlayer(team, rcb, this.ReplaceSimAttr(rcb, 1, 0));
                this.ReplacePlayer(team, lcb, this.ReplaceSimAttr(lcb, 1, 0));
                this.ReplacePlayer(team, fs, this.ReplaceSimAttr(fs, 1, 0));
                this.ReplacePlayer(team, ss, this.ReplaceSimAttr(ss, 1, rushDef[System.Array.index(7, rushDef)]));
            },
            /**
             * replaces the sim attribute specified.
             *
             * @instance
             * @private
             * @this TSBTool.TecmonsterTSB1SimAutoUpdater
             * @memberof TSBTool.TecmonsterTSB1SimAutoUpdater
             * @param   {string}    line        Like: 
             "LOLB, trev ALBERTS, Face=0x26, #51, 25, 31, 31, 31, 38, 31 ,[30, 20 ]"
             * @param   {number}    num         1 -&gt; '30', 2-&gt;'20' above.
             * @param   {number}    newValue    the new value
             * @return  {string}                The input string with the specified replacement.
             */
            ReplaceSimAttr: function (line, num, newValue) {
                var ret = line;
                if (this.m_SimRegex == null) {
                    this.m_SimRegex = new System.Text.RegularExpressions.Regex.ctor("\\[\\s*([0-9]+)\\s*,\\s*([0-9]+)\\s*\\]");
                }
                var m = this.m_SimRegex.match(line);
                if (m != null) {
                    var index = m.getGroups().get(num).getIndex();
                    var len = m.getGroups().get(num).toString().length;
                    ret = System.String.format("{0}{1}{2}", line.substr(0, index), Bridge.box(newValue, System.Int32), line.substr(((index + len) | 0)));
                }
                return ret;
            }
        }
    });

    /**
     * Summary description for TecmoToolFactory.
     *
     * @public
     * @class TSBTool.TecmoToolFactory
     */
    Bridge.define("TSBTool.TecmoToolFactory", {
        statics: {
            fields: {
                ORIG_NES_TSB1_LEN: 0,
                CXROM_V105_LEN: 0,
                CXROM_V111_LEN: 0,
                SNES_TSB1_LEN: 0
            },
            ctors: {
                init: function () {
                    this.ORIG_NES_TSB1_LEN = 393232;
                    this.CXROM_V105_LEN = 524304;
                    this.CXROM_V111_LEN = 786448;
                    this.SNES_TSB1_LEN = 1572864;
                }
            },
            methods: {
                GetToolForRom: function (rom) {
                    var tool = null;
                    var type = TSBTool.ROM_TYPE.NONE;
                    try {
                        type = TSBTool.TecmoToolFactory.CheckRomType(rom);
                    } catch ($e1) {
                        $e1 = System.Exception.create($e1);
                        var e;
                        if (Bridge.is($e1, System.UnauthorizedAccessException)) {
                            type = TSBTool.ROM_TYPE.READ_ONLY_ERROR;
                            TSBTool.StaticUtils.ShowError("ERROR opening ROM, Please check ROM to make sure it's not 'read-only'.");
                            return null;
                        } else {
                            e = $e1;
                            TSBTool.StaticUtils.ShowError(System.String.format("ERROR determining ROM type. Exception=\n{0}\n{1}", e.Message, e.StackTrace));
                            return null;
                        }
                    }

                    if (type === TSBTool.ROM_TYPE.CXROM_v105 || type === TSBTool.ROM_TYPE.CXROM_v111) {
                        TSBTool.TecmoTool.Teams = function (_o1) {
                                _o1.add("bills");
                                _o1.add("dolphins");
                                _o1.add("patriots");
                                _o1.add("jets");
                                _o1.add("bengals");
                                _o1.add("browns");
                                _o1.add("ravens");
                                _o1.add("steelers");
                                _o1.add("colts");
                                _o1.add("texans");
                                _o1.add("jaguars");
                                _o1.add("titans");
                                _o1.add("broncos");
                                _o1.add("chiefs");
                                _o1.add("raiders");
                                _o1.add("chargers");
                                _o1.add("redskins");
                                _o1.add("giants");
                                _o1.add("eagles");
                                _o1.add("cowboys");
                                _o1.add("bears");
                                _o1.add("lions");
                                _o1.add("packers");
                                _o1.add("vikings");
                                _o1.add("buccaneers");
                                _o1.add("saints");
                                _o1.add("falcons");
                                _o1.add("panthers");
                                _o1.add("AFC");
                                _o1.add("NFC");
                                _o1.add("49ers");
                                _o1.add("rams");
                                _o1.add("seahawks");
                                _o1.add("cardinals");
                                return _o1;
                            }(new (System.Collections.Generic.List$1(System.String)).ctor());
                        var cxt = new TSBTool.CXRomTSBTool(rom, type);
                        tool = cxt;
                        if (type === TSBTool.ROM_TYPE.CXROM_v111) {
                            var test = cxt.GetName("49ers", "QB1");
                            if (test == null) {
                                tool = new TSBTool.CXRomTSBTool(rom, TSBTool.ROM_TYPE.CXROM_v105);
                            }
                        }
                    } else if (type === TSBTool.ROM_TYPE.SNES_TSB1) {
                        TSBTool.TecmoTool.Teams = function (_o2) {
                                _o2.add("bills");
                                _o2.add("colts");
                                _o2.add("dolphins");
                                _o2.add("patriots");
                                _o2.add("jets");
                                _o2.add("bengals");
                                _o2.add("browns");
                                _o2.add("oilers");
                                _o2.add("steelers");
                                _o2.add("broncos");
                                _o2.add("chiefs");
                                _o2.add("raiders");
                                _o2.add("chargers");
                                _o2.add("seahawks");
                                _o2.add("cowboys");
                                _o2.add("giants");
                                _o2.add("eagles");
                                _o2.add("cardinals");
                                _o2.add("redskins");
                                _o2.add("bears");
                                _o2.add("lions");
                                _o2.add("packers");
                                _o2.add("vikings");
                                _o2.add("buccaneers");
                                _o2.add("falcons");
                                _o2.add("rams");
                                _o2.add("saints");
                                _o2.add("49ers");
                                return _o2;
                            }(new (System.Collections.Generic.List$1(System.String)).ctor());
                        tool = new TSBTool.SNES_TecmoTool(rom);
                    } else if (type === TSBTool.ROM_TYPE.SNES_TSB2) {
                        tool = new TSBTool2.TSB2Tool.$ctor1(rom);
                    } else if (type === TSBTool.ROM_TYPE.SNES_TSB3) {
                        tool = new TSBTool2.TSB3Tool.$ctor1(rom);
                    } else {
                        TSBTool.TecmoTool.Teams = function (_o3) {
                                _o3.add("bills");
                                _o3.add("colts");
                                _o3.add("dolphins");
                                _o3.add("patriots");
                                _o3.add("jets");
                                _o3.add("bengals");
                                _o3.add("browns");
                                _o3.add("oilers");
                                _o3.add("steelers");
                                _o3.add("broncos");
                                _o3.add("chiefs");
                                _o3.add("raiders");
                                _o3.add("chargers");
                                _o3.add("seahawks");
                                _o3.add("redskins");
                                _o3.add("giants");
                                _o3.add("eagles");
                                _o3.add("cardinals");
                                _o3.add("cowboys");
                                _o3.add("bears");
                                _o3.add("lions");
                                _o3.add("packers");
                                _o3.add("vikings");
                                _o3.add("buccaneers");
                                _o3.add("49ers");
                                _o3.add("rams");
                                _o3.add("saints");
                                _o3.add("falcons");
                                return _o3;
                            }(new (System.Collections.Generic.List$1(System.String)).ctor());
                        tool = new TSBTool.TecmoTool.$ctor1(rom);
                    }
                    return tool;
                },
                /**
                 * returns 0 if regular NES TSB rom
                         1 if it's cxrom TSBROM type.
                 Throws exceptions (UnauthorizedAccessException and others)
                 *
                 * @static
                 * @public
                 * @this TSBTool.TecmoToolFactory
                 * @memberof TSBTool.TecmoToolFactory
                 * @param   {Array.<number>}      rom
                 * @return  {TSBTool.ROM_TYPE}
                 */
                CheckRomType: function (rom) {
                    var ret = TSBTool.ROM_TYPE.NONE;
                    var s1 = null;
                    try {
                        var len = System.Int64(rom.length);
                        if (len.equals(System.Int64(TSBTool.TecmoToolFactory.ORIG_NES_TSB1_LEN))) {
                            ret = TSBTool.ROM_TYPE.NES_ORIGINAL_TSB;
                        } else if (len.equals(System.Int64(TSBTool.TecmoToolFactory.CXROM_V105_LEN))) {
                            ret = TSBTool.ROM_TYPE.CXROM_v105;
                        } else if (len.equals(System.Int64(TSBTool.TecmoToolFactory.CXROM_V111_LEN))) {
                            ret = TSBTool.ROM_TYPE.CXROM_v111;
                        } else if (len.equals(System.Int64(TSBTool.TecmoToolFactory.SNES_TSB1_LEN))) {
                            ret = TSBTool.ROM_TYPE.SNES_TSB1;
                        } else if (TSBTool2.TSB2Tool.IsTecmoSuperBowl2Rom(rom)) {
                            ret = TSBTool.ROM_TYPE.SNES_TSB2;
                        } else if (TSBTool2.TSB3Tool.IsTecmoSuperBowl3Rom(rom)) {
                            ret = TSBTool.ROM_TYPE.SNES_TSB3;
                        }
                        TSBTool.StaticUtils.WriteError("ROM Type = " + (System.Enum.toString(TSBTool.ROM_TYPE, ret) || ""));
                    } finally {
                        if (s1 != null) {
                            s1.Close();
                        }
                    }
                    return ret;
                }
            }
        }
    });

    Bridge.define("TSBTool.TSBContentType", {
        $kind: "enum",
        statics: {
            fields: {
                Unknown: 0,
                TSB1: 1,
                TSB2: 2,
                TSB3: 3
            }
        }
    });

    Bridge.define("TSBTool.TSBPlayer", {
        $kind: "enum",
        statics: {
            fields: {
                QB1: 0,
                QB2: 1,
                RB1: 2,
                RB2: 3,
                RB3: 4,
                RB4: 5,
                WR1: 6,
                WR2: 7,
                WR3: 8,
                WR4: 9,
                TE1: 10,
                TE2: 11,
                C: 12,
                LG: 13,
                RG: 14,
                LT: 15,
                RT: 16,
                RE: 17,
                NT: 18,
                LE: 19,
                ROLB: 20,
                RILB: 21,
                LILB: 22,
                LOLB: 23,
                RCB: 24,
                LCB: 25,
                FS: 26,
                SS: 27,
                K: 28,
                P: 29
            }
        }
    });

    Bridge.define("TSBTool2.Conference", {
        $kind: "enum",
        statics: {
            fields: {
                AFC: 0,
                NFC: 1
            }
        }
    });

    /** @namespace TSBTool2 */

    /**
     * Summary description for InputParser.
     *
     * @public
     * @class TSBTool2.InputParser
     */
    Bridge.define("TSBTool2.InputParser", {
        statics: {
            fields: {
                scheduleState: 0,
                rosterState: 0,
                numberRegex: null,
                teamRegex: null,
                simDataRegex: null,
                weekRegex: null,
                gameRegex: null,
                posNameFaceRegex: null,
                yearRegex: null,
                returnTeamRegex: null,
                setRegex: null,
                offensiveFormationRegex: null,
                playbookRegex: null,
                juiceRegex: null,
                homeRegex: null,
                awayRegex: null,
                divChampRegex: null,
                confChampRegex: null,
                uniformUsageRegex: null,
                replaceStringRegex: null,
                teamStringsRegex: null,
                seasonRegex: null,
                KickRetMan: null,
                PuntRetMan: null
            },
            ctors: {
                init: function () {
                    this.scheduleState = 0;
                    this.rosterState = 1;
                    this.numberRegex = new System.Text.RegularExpressions.Regex.ctor("(#[0-9]{1,2})");
                    this.teamRegex = new System.Text.RegularExpressions.Regex.ctor("TEAM\\s*=\\s*([0-9a-zAT]+)");
                    this.simDataRegex = new System.Text.RegularExpressions.Regex.ctor("SimData=0[xX]([0-9a-fA-F][0-9a-fA-F])([0-3]?)");
                    this.weekRegex = new System.Text.RegularExpressions.Regex.ctor("WEEK ([1-9][0\t-7]?)");
                    this.gameRegex = new System.Text.RegularExpressions.Regex.ctor("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");
                    this.posNameFaceRegex = new System.Text.RegularExpressions.Regex.ctor("([A-Z]+[1-5]?)\\s*,\\s*([a-zA-Z \\.\\-]+),\\s*(Face=0[xX][0-9a-fA-F]+\\s*,\\s*)");
                    this.yearRegex = new System.Text.RegularExpressions.Regex.ctor("YEAR\\s*=\\s*([0-9]+)");
                    this.returnTeamRegex = new System.Text.RegularExpressions.Regex.ctor("RETURN_TEAM\\s+([A-Z1-4]+)\\s*,\\s*([A-Z1-4]+)\\s*,\\s*([A-Z1-4]+)");
                    this.setRegex = new System.Text.RegularExpressions.Regex.ctor("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
                    this.offensiveFormationRegex = new System.Text.RegularExpressions.Regex.ctor("OFFENSIVE_FORMATION\\s*=\\s*([a-zA-Z1234_]+)");
                    this.playbookRegex = new System.Text.RegularExpressions.Regex.ctor("PLAYBOOK\\s+(R[0-9A-Fa-f]+)\\s*,\\s*(P[0-9A-Fa-f]+)");
                    this.juiceRegex = new System.Text.RegularExpressions.Regex.ctor("JUICE\\(\\s*([0-9]{1,2}|ALL)\\s*,\\s*([0-9]{1,2})\\s*\\)");
                    this.homeRegex = new System.Text.RegularExpressions.Regex.ctor("Uniform1\\s*=\\s*0x([0-9a-fA-F]{6})");
                    this.awayRegex = new System.Text.RegularExpressions.Regex.ctor("Uniform2\\s*=\\s*0x([0-9a-fA-F]{6})");
                    this.divChampRegex = new System.Text.RegularExpressions.Regex.ctor("DivChamp\\s*=\\s*0x([0-9a-fA-F]{10})");
                    this.confChampRegex = new System.Text.RegularExpressions.Regex.ctor("ConfChamp\\s*=\\s*0x([0-9a-fA-F]{8})");
                    this.uniformUsageRegex = new System.Text.RegularExpressions.Regex.ctor("UniformUsage\\s*=\\s*0x([0-9a-fA-F]{8})");
                    this.replaceStringRegex = new System.Text.RegularExpressions.Regex.ctor("ReplaceString\\(\\s*\"([A-Za-z0-9 .]+)\"\\s*,\\s*\"([A-Za-z .]+)\"\\s*(,\\s*([0-9]+))*\\s*\\)");
                    this.teamStringsRegex = new System.Text.RegularExpressions.Regex.ctor("TEAM_ABB=([0-9A-Za-z. ]+),TEAM_CITY=([0-9A-Za-z .]+),TEAM_NAME=([0-9A-Za-z .]+)");
                    this.seasonRegex = new System.Text.RegularExpressions.Regex.ctor("^\\s*SEASON\\s+([1-3])");
                    this.KickRetMan = new System.Text.RegularExpressions.Regex.ctor("^KR\\s*,\\s*([A-Z1-4]+)$");
                    this.PuntRetMan = new System.Text.RegularExpressions.Regex.ctor("^PR\\s*,\\s*([A-Z1-4]+)$");
                }
            },
            methods: {
                CheckTextForRedundentSetCommands: function (input) {
                    var ret = new System.Text.StringBuilder();
                    var simpleSetRegex = new System.Text.RegularExpressions.Regex.ctor("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
                    var mc = simpleSetRegex.matches(input);
                    var current = null;
                    var m = null;
                    var location1 = System.Int64(0);
                    var location2 = System.Int64(0);
                    var valueLength1 = 0;
                    var valueLength2 = 0;
                    for (var i = 0; i < mc.getCount(); i = (i + 1) | 0) {
                        current = mc.get(i);
                        location1 = TSBTool.StaticUtils.ParseLongFromHexString(current.getGroups().get(1).toString().substr(2));
                        valueLength1 = (Bridge.Int.div(current.getGroups().get(2).getLength(), 2)) | 0;
                        for (var j = (i + 1) | 0; j < mc.getCount(); j = (j + 1) | 0) {
                            m = mc.get(j);
                            location2 = TSBTool.StaticUtils.ParseLongFromHexString(m.getGroups().get(1).toString().substr(2));
                            valueLength2 = (Bridge.Int.div(m.getGroups().get(2).getLength(), 2)) | 0;
                            if ((location2.gte(location1) && location2.lte(location1.add(System.Int64((((valueLength1 - 2) | 0)))))) || (location1.gte(location2) && location1.lte(location2.add(System.Int64((((valueLength2 - 2) | 0))))))) {
                                if (!Bridge.referenceEquals(current.getGroups().get(0).toString(), m.getGroups().get(0).toString())) {
                                    ret.append("WARNING!\n 'SET' Commands modify same locations '");
                                    ret.append(current.getGroups().get(0));
                                    ret.append("' and '");
                                    ret.append(m.getGroups().get(0));
                                    ret.append("'\n");
                                }
                            }
                        }
                    }
                    return ret.toString();
                },
                GetTeam: function (line) {
                    var m = TSBTool2.InputParser.teamRegex.match(line);
                    var team = m.getGroups().get(1).toString();
                    return team;
                },
                GetSimData: function (line) {
                    var m = TSBTool2.InputParser.simDataRegex.match(line);
                    var data = m.getGroups().get(1).toString();
                    var simOffensePref = m.getGroups().get(2).toString();
                    var ret = System.Array.init([-1, -1], System.Int32);

                    if (data.length > 0) {
                        try {
                            var simData = TSBTool.StaticUtils.ParseIntFromHexString(data);
                            ret[System.Array.index(0, ret)] = simData;
                        } catch ($e1) {
                            $e1 = System.Exception.create($e1);
                            TSBTool.StaticUtils.AddError(System.String.format("Error getting SimData with line '{0}'.", [line]));
                        }
                    }

                    if (simOffensePref.length > 0) {
                        try {
                            var so = System.Int32.parse(simOffensePref);
                            ret[System.Array.index(1, ret)] = so;
                        } catch ($e2) {
                            $e2 = System.Exception.create($e2);
                            TSBTool.StaticUtils.AddError(System.String.format("Error getting SimData with line '{0}'.", [line]));
                        }
                    }
                    return ret;
                },
                /**
                 * Expect line like '   [8, 9, 0 ]'
                 *
                 * @static
                 * @public
                 * @this TSBTool2.InputParser
                 * @memberof TSBTool2.InputParser
                 * @param   {string}            input     
                 * @param   {boolean}           useHex
                 * @return  {Array.<number>}
                 */
                GetSimVals: function (input, useHex) {
                    if (input != null) {
                        var stuff = input.trim();
                        var start = System.String.indexOf(stuff, "[");
                        var end = System.String.indexOf(stuff, "]");
                        if (start > -1 && end > -1) {
                            stuff = stuff.substr(((start + 1) | 0), ((((end - start) | 0) - 1) | 0));
                            return TSBTool2.InputParser.GetInts$1(stuff, useHex);
                        }
                    }
                    return null;
                },
                GetInts: function (input) {
                    return TSBTool2.InputParser.GetInts$1(input, false);
                },
                GetInts$1: function (input, useHex) {
                    if (input != null) {
                        var pound = System.String.indexOf(input, "#");
                        var brace = System.String.indexOf(input, "[");
                        if (pound > -1) {
                            input = input.substr(((pound + 3) | 0));
                        }
                        if (brace > -1) {
                            brace = System.String.indexOf(input, "[");
                            input = input.substr(0, brace);
                        }
                        var seps = System.Array.init([32, 44, 9], System.Char);
                        var nums = System.String.split(input, seps.map(function (i) {{ return String.fromCharCode(i); }}));
                        var j, count = 0;
                        for (j = 0; j < nums.length; j = (j + 1) | 0) {
                            if (nums[System.Array.index(j, nums)].length > 0) {
                                count = (count + 1) | 0;
                            }
                        }
                        var result = System.Array.init(count, 0, System.Int32);
                        j = 0;

                        var s = "";
                        var i = 0;
                        try {
                            for (i = 0; i < nums.length; i = (i + 1) | 0) {
                                s = nums[System.Array.index(i, nums)];
                                if (s != null && s.length > 0) {
                                    if (useHex) {
                                        result[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), result)] = TSBTool.StaticUtils.ParseIntFromHexString(s);
                                    } else {
                                        result[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), result)] = System.Int32.parse(s);
                                    }
                                }
                            }
                            return result;
                        } catch (e) {
                            e = System.Exception.create(e);
                            var error = System.String.format("Error with input '{0}', {1}, was jersey number specified?", input, e.Message);
                            TSBTool.StaticUtils.AddError(error);
                        }
                    }
                    return null;
                },
                GetJerseyNumber: function (line) {
                    var ret = -1;
                    var jerseyRegex = new System.Text.RegularExpressions.Regex.ctor("#([0-9]+)");
                    var num = jerseyRegex.match(line).getGroups().get(1).toString();
                    try {
                        ret = TSBTool.StaticUtils.ParseIntFromHexString(num);
                    } catch ($e1) {
                        $e1 = System.Exception.create($e1);
                        ret = -1;
                    }
                    return ret;
                },
                GetFace: function (line) {
                    var ret = -1;
                    var hexRegex = new System.Text.RegularExpressions.Regex.ctor("0[xX]([A-Fa-f0-9]+)");
                    var m = hexRegex.match(line);
                    if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        var num = m.getGroups().get(1).toString();
                        try {
                            ret = TSBTool.StaticUtils.ParseIntFromHexString(num);
                        } catch ($e1) {
                            $e1 = System.Exception.create($e1);
                            ret = -1;
                            TSBTool.StaticUtils.AddError(System.String.format("Face ERROR line '{0}'", [line]));
                        }
                    }

                    return ret;
                },
                GetPosition: function (line) {
                    var pos = TSBTool2.InputParser.posNameFaceRegex.match(line).getGroups().get(1).toString();
                    return pos;
                },
                GetLastName: function (line) {
                    var ret = "";
                    var m = TSBTool2.InputParser.posNameFaceRegex.match(line);
                    if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        var name = m.getGroups().get(2).toString().trim();
                        var index = name.lastIndexOf(" ");
                        ret = name.substr(((index + 1) | 0));
                    }
                    return ret;
                },
                GetFirstName: function (line) {
                    var ret = "";
                    var m = TSBTool2.InputParser.posNameFaceRegex.match(line);
                    if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        var name = m.getGroups().get(2).toString().trim();
                        var index = name.lastIndexOf(" ");
                        if (index > -1 && index < name.length) {
                            ret = name.substr(0, index);
                        }
                    }
                    return ret;
                },
                /**
                 * @static
                 * @public
                 * @this TSBTool2.InputParser
                 * @memberof TSBTool2.InputParser
                 * @param   {string}            byteString    String in the format of a hex string (0123456789ABCDEF), must have
                 an even number of characters.
                 * @return  {Array.<number>}                  The bytes.
                 */
                GetBytesFromString: function (byteString) {
                    var ret = null;
                    var tmp = null;
                    var b;
                    if (byteString != null && byteString.length > 1 && (byteString.length % 2) === 0) {
                        tmp = System.Array.init(((Bridge.Int.div(byteString.length, 2)) | 0), 0, System.Byte);
                        for (var i = 0; i < tmp.length; i = (i + 1) | 0) {
                            b = byteString.substr(Bridge.Int.mul(i, 2), 2);
                            tmp[System.Array.index(i, tmp)] = TSBTool.StaticUtils.ParseByteFromHexString(b);
                        }
                        ret = tmp;
                    }
                    return ret;
                },
                GetHomeUniformColorString: function (line) {
                    var tmp = "";
                    var match = TSBTool2.InputParser.homeRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                GetAwayUniformColorString: function (line) {
                    var tmp = "";
                    var match = TSBTool2.InputParser.awayRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                GetConfChampColorString: function (line) {
                    var tmp = "";
                    var match = TSBTool2.InputParser.confChampRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                GetDivChampColorString: function (line) {
                    var tmp = "";
                    var match = TSBTool2.InputParser.divChampRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                GetUniformUsageString: function (line) {
                    var tmp = "";
                    var match = TSBTool2.InputParser.uniformUsageRegex.match(line);
                    if (!Bridge.referenceEquals(match, System.Text.RegularExpressions.Match.getEmpty())) {
                        tmp = match.getGroups().get(1).getValue();
                    }
                    return tmp;
                },
                /**
                 * Returns the text string passed, without thr trailing commas.
                 *
                 * @static
                 * @public
                 * @this TSBTool2.InputParser
                 * @memberof TSBTool2.InputParser
                 * @param   {string}    text
                 * @return  {string}
                 */
                DeleteTrailingCommas: function (text) {
                    var rs = new System.Text.RegularExpressions.Regex.ctor(",+\n");
                    var rrs = new System.Text.RegularExpressions.Regex.ctor(",+$");
                    var ret = rs.replace(text, "\n");
                    ret = rrs.replace(ret, "");

                    return ret;
                }
            }
        },
        fields: {
            tool: null,
            currentState: 0,
            showSimError: false,
            season: 0,
            currentTeam: null,
            scheduleList: null
        },
        ctors: {
            init: function () {
                this.currentState = 2;
                this.showSimError = false;
                this.season = 1;
            },
            $ctor1: function (tool) {
                this.$initialize();
                this.tool = tool;
                this.currentTeam = "bills";
            },
            ctor: function () {
                this.$initialize();
                this.currentTeam = "bills";
            }
        },
        methods: {
            ProcessFile: function (fileName) {
                try {
                    var sr = new System.IO.StreamReader.$ctor7(fileName);
                    var contents = sr.ReadToEnd();
                    sr.Close();
                    var chars = System.String.toCharArray(("\n\r"), 0, ("\n\r").length);
                    var lines = System.String.split(contents, chars.map(function (i) {{ return String.fromCharCode(i); }}));
                    this.ProcessLines(lines);
                } catch (e) {
                    e = System.Exception.create(e);
                    TSBTool.StaticUtils.ShowError(e.Message);
                }
            },
            ProcessText: function (content) {
                content = System.String.replaceAll(content, "\r\n", "\n");
                var lines = System.String.split(content, System.Array.init([10], System.Char).map(function (i) {{ return String.fromCharCode(i); }}));
                this.ProcessLines(lines);
            },
            ProcessLines: function (lines) {
                var i = 0;
                try {
                    for (i = 0; i < lines.length; i = (i + 1) | 0) {
                        this.ProcessLine(lines[System.Array.index(i, lines)]);
                    }
                    TSBTool.StaticUtils.ShowErrors();
                    this.ApplySchedule();
                } catch (e) {
                    e = System.Exception.create(e);
                    var sb = new System.Text.StringBuilder("", 150);
                    sb.append("Error! ");
                    if (i < lines.length) {
                        sb.append(System.String.format("line #{0}:\t'{1}'", Bridge.box(i, System.Int32), lines[System.Array.index(i, lines)]));
                    }
                    sb.append(e.Message);
                    sb.append("\n");
                    sb.append(e.StackTrace);
                    sb.append("\n\nOperation aborted at this point. Data not applied.");
                    TSBTool.StaticUtils.ShowError(sb.toString());
                }
            },
            ApplySchedule: function () {
                if (this.scheduleList != null) {
                    this.tool.TSBTool2$ITecmoTool$ApplySchedule(this.season, this.scheduleList);
                    TSBTool.StaticUtils.ShowErrors();
                    this.scheduleList = null;
                }
            },
            ReadFromStdin: function () {
                var line = "";
                var lineNumber = 0;
                System.Console.WriteLine("Reading from standard in...");
                try {
                    while (((line = prompt())) != null) {
                        lineNumber = (lineNumber + 1) | 0;
                        this.ProcessLine(line);
                    }
                    TSBTool.StaticUtils.ShowErrors();
                    this.ApplySchedule();
                } catch (e) {
                    e = System.Exception.create(e);
                    TSBTool.StaticUtils.ShowError(System.String.format("Error Processing line {0}:'{1}'.\n{2}\n{3}", Bridge.box(lineNumber, System.Int32), line, e.Message, e.StackTrace));
                }
            },
            /**
             * @instance
             * @protected
             * @this TSBTool2.InputParser
             * @memberof TSBTool2.InputParser
             * @param   {string}    line
             * @return  {void}
             */
            ProcessLine: function (line) {
                line = line.trim();
                var m;

                if (System.String.startsWith(line, "#") || Bridge.referenceEquals(line, "") || System.String.startsWith(line.toLowerCase().trim(), "schedule")) {
                    return;
                } else {
                    if (System.String.startsWith(line, "SET")) {
                        this.tool.TSBTool2$ITecmoTool$ApplySet(line);
                    } else if (!Bridge.referenceEquals(((m = TSBTool2.InputParser.seasonRegex.match(line))), System.Text.RegularExpressions.Match.getEmpty())) {
                        if (this.scheduleList != null && this.scheduleList.Count > 0) {
                            this.ApplySchedule();
                        }
                        System.Int32.tryParse(m.getGroups().get(1).toString(), Bridge.ref(this, "season"));
                    } else if (!Bridge.referenceEquals(((m = TSBTool2.InputParser.playbookRegex.match(line))), System.Text.RegularExpressions.Match.getEmpty())) {
                        var runs = m.getGroups().get(1).toString();
                        var passes = m.getGroups().get(2).toString();
                        this.tool.TSBTool2$ITecmoTool$SetPlaybook(this.season, this.currentTeam, runs, passes);
                    } else if (System.String.startsWith(line, "ReplaceString")) {
                        var repMatch = TSBTool2.InputParser.replaceStringRegex.match(line);
                        var find = "";
                        var replace = "";
                        var occur = { v : -1 };
                        if (repMatch.getGroups().getCount() > 1) {
                            find = repMatch.getGroups().get(1).toString();
                            replace = repMatch.getGroups().get(2).toString();
                            if (repMatch.getGroups().getCount() > 3) {
                                System.Int32.tryParse(repMatch.getGroups().get(4).toString(), occur);
                                occur.v = (occur.v - 1) | 0;
                            }
                            var msg = TSBTool.StaticUtils.ReplaceStringInRom(this.tool.TSBTool2$ITecmoTool$OutputRom, find, replace, occur.v);
                            if (System.String.startsWith(msg, "Error")) {
                                TSBTool.StaticUtils.AddError(msg);
                            } else {
                                System.Console.WriteLine(msg);
                            }
                        } else {
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR! Not enough info to use 'ReplaceString' function.Line={0}", [line]));
                        }
                    } else if (System.String.startsWith(line, "TEAM_ABB")) {
                        var teamStringsMatch = TSBTool2.InputParser.teamStringsRegex.match(line);
                        if (!Bridge.referenceEquals(teamStringsMatch, System.Text.RegularExpressions.Match.getEmpty())) {
                            var teamAbb = teamStringsMatch.getGroups().get(1).toString();
                            var teamCity = teamStringsMatch.getGroups().get(2).toString();
                            var teamName = teamStringsMatch.getGroups().get(3).toString();
                            var index = TSBTool2.TSB2Tool.GetTeamIndex(this.currentTeam);
                            this.tool.TSBTool2$ITecmoTool$SetTeamAbbreviation(index, teamAbb);
                            this.tool.TSBTool2$ITecmoTool$SetTeamCity(index, teamCity);
                            this.tool.TSBTool2$ITecmoTool$SetTeamName(index, teamName);
                        }
                    } else if (!Bridge.referenceEquals(TSBTool2.InputParser.teamRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {

                        this.currentState = TSBTool2.InputParser.rosterState;
                        var team = TSBTool2.InputParser.GetTeam(line);
                        var ret = this.SetCurrentTeam(team);
                        if (!ret) {
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR with line '{0}'.", [line]));
                            TSBTool.StaticUtils.AddError(System.String.format("Team input must be in the form 'TEAM = team SimData=0x1F'", null));
                            return;
                        }
                        this.HandleSimData(line);
                        /* int[] simData = GetSimData(line);
                        if (simData != null)
                        {
                           if (simData[0] > -1)
                               tool.SetTeamSimData(currentTeam, (byte)simData[0]);
                           else
                               TSBTool.StaticUtils.AddError(string.Format("Warning: No sim data for team {0}", team));

                           if (simData[1] > -1)
                               tool.SetTeamSimOffensePref(currentTeam, simData[1]);
                        }
                        else
                           TSBTool.StaticUtils.AddError(string.Format("ERROR with line '{0}'.", line));

                        /*Match oFormMatch = offensiveFormationRegex.Match(line);
                        if (oFormMatch != Match.Empty)
                        {
                           string formation = oFormMatch.Groups[1].ToString();
                           tool.SetTeamOffensiveFormation(team, formation);
                        }*/
                    } else if (!Bridge.referenceEquals(TSBTool2.InputParser.weekRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                        this.currentState = TSBTool2.InputParser.scheduleState;
                        if (this.scheduleList == null) {
                            this.scheduleList = new (System.Collections.Generic.List$1(System.String)).$ctor2(300);
                        }
                        this.scheduleList.add(line);
                    } else if (!Bridge.referenceEquals(TSBTool2.InputParser.yearRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                        this.SetYear(line);
                    } else if (System.String.startsWith(line, "AFC") || System.String.startsWith(line, "NFC")) {
                        var parts = System.String.split(System.String.replaceAll(line, " ", ""), System.Array.init([44], System.Char).map(function (i) {{ return String.fromCharCode(i); }}), null, 1);
                        if (parts != null && parts.length > 3) {
                            try {
                                this.tool.TSBTool2$ITecmoTool$SetProBowlPlayer(this.season, System.Nullable.getValue(Bridge.cast(Bridge.unbox(System.Enum.parse(TSBTool2.Conference, parts[System.Array.index(0, parts)]), TSBTool2.Conference), System.Int32)), parts[System.Array.index(1, parts)], parts[System.Array.index(2, parts)], System.Nullable.getValue(Bridge.cast(Bridge.unbox(System.Enum.parse(TSBTool2.TSBPlayer, parts[System.Array.index(3, parts)]), TSBTool2.TSBPlayer), System.Int32)));
                            } catch ($e1) {
                                $e1 = System.Exception.create($e1);
                                TSBTool.StaticUtils.AddError("Error processing line > " + (line || ""));
                            }
                        }
                    } else if (this.currentState === TSBTool2.InputParser.scheduleState) {
                        if (this.scheduleList != null) {
                            this.scheduleList.add(line);
                        }
                    } else if (this.currentState === TSBTool2.InputParser.rosterState) {
                        this.UpdateRoster(line);
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("Garbage/orphin line not applied \"{0}\"", [line]));
                    }
                }
            },
            SetYear: function (line) {
                var m = TSBTool2.InputParser.yearRegex.match(line);
                var year = m.getGroups().get(1).toString();
                if (year.length < 1) {
                    TSBTool.StaticUtils.AddError(System.String.format("'{0}' is not valid.", [line]));
                } else {
                    this.tool.TSBTool2$ITecmoTool$SetYear(year);
                    System.Console.WriteLine(System.String.format("Year set to '{0}'", year));
                }
            },
            HandleSimData: function (line) {
                var simDataRegex = new System.Text.RegularExpressions.Regex.ctor("SimData\\s*=\\s*0[xX]([0-9a-fA-F]{2})");
                var m = simDataRegex.match(line);
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    var stuff = m.getGroups().get(1).toString();
                    this.tool.TSBTool2$ITecmoTool$SetTeamSimData(this.season, this.currentTeam, stuff);
                }
            },
            GetAwayTeam: function (line) {
                var m = TSBTool2.InputParser.gameRegex.match(line);
                var awayTeam = m.getGroups().get(1).toString();
                return awayTeam;
            },
            GetHomeTeam: function (line) {
                var m = TSBTool2.InputParser.gameRegex.match(line);
                var team = m.getGroups().get(2).toString();
                return team;
            },
            GetWeek: function (line) {
                var m = TSBTool2.InputParser.weekRegex.match(line);
                var week_str = m.getGroups().get(1).toString();
                var ret = -1;
                try {
                    ret = System.Int32.parse(week_str);
                    ret = (ret - 1) | 0;
                } catch ($e1) {
                    $e1 = System.Exception.create($e1);
                    TSBTool.StaticUtils.AddError(System.String.format("Week '{0}' is invalid.", [week_str]));
                }
                return ret;
            },
            SetCurrentTeam: function (team) {
                if (TSBTool2.TSB2Tool.GetTeamIndex(team) < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("Team '{0}' is Invalid.", [team]));
                    return false;
                } else {
                    this.currentTeam = team;
                }
                return true;
            },
            UpdateRoster: function (line) {
                if (System.String.startsWith(line, "KR")) {
                    this.SetKickReturnMan(line);
                } else {
                    if (System.String.startsWith(line, "PR")) {
                        this.SetPuntReturnMan(line);
                    } else {
                        var m = TSBTool2.InputParser.posNameFaceRegex.match(line);
                        if (System.String.indexOf(line, "#") > -1) {
                            if (Bridge.referenceEquals(TSBTool2.InputParser.numberRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                                TSBTool.StaticUtils.AddError(System.String.format("ERROR! (jersey number) Line  {0}", [line]));
                                return;
                            }
                        }
                        var p = m.getGroups().get(1).toString();
                        if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty()) && this.tool.TSBTool2$ITecmoTool$IsValidPosition(p)) {
                            if (System.String.startsWith(line, "QB")) {
                                this.SetQB(line);
                            } else {
                                if (System.String.startsWith(line, "WR") || System.String.startsWith(line, "RB") || System.String.startsWith(line, "TE")) {
                                    this.SetSkillPlayer(line);
                                } else {
                                    if (System.String.startsWith(line, "C") || System.String.startsWith(line, "RG") || System.String.startsWith(line, "LG") || System.String.startsWith(line, "RT") || System.String.startsWith(line, "LT")) {
                                        this.SetOLPlayer(line);
                                    } else if (System.String.indexOf(line, "LB") === 2 || System.String.indexOf(line, "CB") === 1 || System.String.startsWith(line, "RE") || System.String.startsWith(line, "LE") || System.String.startsWith(line, "NT") || System.String.startsWith(line, "SS") || System.String.startsWith(line, "FS") || System.String.startsWith(line, "DB")) {
                                        this.SetDefensivePlayer(line);
                                    } else if (System.String.startsWith(line, "P") || System.String.startsWith(line, "K")) {
                                        this.SetKickPlayer(line);
                                    }
                                }
                            }
                        } else {
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR! With line \"{0}\"     team = {1}", line, this.currentTeam));
                        }
                    }
                }
            },
            SetQB: function (line) {
                var fname = TSBTool2.InputParser.GetFirstName(line);
                var lname = TSBTool2.InputParser.GetLastName(line);
                var pos = TSBTool2.InputParser.GetPosition(line);
                var face = TSBTool2.InputParser.GetFace(line);
                var jerseyNumber = TSBTool2.InputParser.GetJerseyNumber(line);
                if (face > -1) {
                    this.tool.TSBTool2$ITecmoTool$SetFace(this.season, this.currentTeam, pos, face);
                }
                if (jerseyNumber < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error with jersey number for '{0} {1}', setting to 0.", fname, lname));
                    jerseyNumber = 0;
                }
                this.tool.TSBTool2$ITecmoTool$InsertPlayerName(this.season, this.currentTeam, pos, fname, lname, (jerseyNumber & 255));

                var vals = TSBTool.StaticUtils.GetTsbAbilities(TSBTool2.InputParser.GetInts$1(line, false));
                var simVals = TSBTool2.InputParser.GetSimVals(line, true);
                if (vals != null && vals.length > 9) {
                    this.tool.TSBTool2$ITecmoTool$SetQBAbilities(this.season, this.currentTeam, pos, vals);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! could not set ability data for {0} {1},", this.currentTeam, pos));
                }
                if (face > -1) {
                    this.tool.TSBTool2$ITecmoTool$SetFace(this.season, this.currentTeam, pos, face);
                } else {
                    if (this.showSimError) {
                        TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No sim data specified.", [line]));
                    }
                }
            },
            SetSkillPlayer: function (line) {
                var fname = TSBTool2.InputParser.GetFirstName(line);
                var lname = TSBTool2.InputParser.GetLastName(line);
                var pos = TSBTool2.InputParser.GetPosition(line);
                var face = TSBTool2.InputParser.GetFace(line);
                var jerseyNumber = TSBTool2.InputParser.GetJerseyNumber(line);
                this.tool.TSBTool2$ITecmoTool$SetFace(this.season, this.currentTeam, pos, face);
                this.tool.TSBTool2$ITecmoTool$InsertPlayerName(this.season, this.currentTeam, pos, fname, lname, (jerseyNumber & 255));

                var vals = TSBTool.StaticUtils.GetTsbAbilities(TSBTool2.InputParser.GetInts$1(line, false));
                var simVals = TSBTool2.InputParser.GetSimVals(line, true);
                if (vals != null && vals.length > 6) {
                    this.tool.TSBTool2$ITecmoTool$SetSkillPlayerAbilities(this.season, this.currentTeam, pos, vals);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No player data specified.", [line]));
                }
                /* if(simVals!= null&& simVals.Length > 3)
                   tool.SetSkillSimData(currentTeam,pos,simVals);
                else  if(showSimError)
                   TSBTool.StaticUtils.AddError(string.Format("Warning! On line '{0}'. No sim data specified.",line));*/
            },
            SetOLPlayer: function (line) {
                var fname = TSBTool2.InputParser.GetFirstName(line);
                var lname = TSBTool2.InputParser.GetLastName(line);
                var pos = TSBTool2.InputParser.GetPosition(line);
                var face = TSBTool2.InputParser.GetFace(line);
                var jerseyNumber = TSBTool2.InputParser.GetJerseyNumber(line);
                var vals = TSBTool.StaticUtils.GetTsbAbilities(TSBTool2.InputParser.GetInts$1(line, false));

                this.tool.TSBTool2$ITecmoTool$SetFace(this.season, this.currentTeam, pos, face);
                this.tool.TSBTool2$ITecmoTool$InsertPlayerName(this.season, this.currentTeam, pos, fname, lname, (jerseyNumber & 255));

                if (vals != null && vals.length > 3) {
                    this.tool.TSBTool2$ITecmoTool$SetOLPlayerAbilities(this.season, this.currentTeam, pos, vals);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No player data specified.", [line]));
                }
            },
            SetDefensivePlayer: function (line) {
                var fname = TSBTool2.InputParser.GetFirstName(line);
                var lname = TSBTool2.InputParser.GetLastName(line);
                var pos = TSBTool2.InputParser.GetPosition(line);
                var face = TSBTool2.InputParser.GetFace(line);
                var jerseyNumber = TSBTool2.InputParser.GetJerseyNumber(line);
                var vals = TSBTool.StaticUtils.GetTsbAbilities(TSBTool2.InputParser.GetInts$1(line, false));
                var simVals = TSBTool2.InputParser.GetSimVals(line, true);

                this.tool.TSBTool2$ITecmoTool$SetFace(this.season, this.currentTeam, pos, face);
                this.tool.TSBTool2$ITecmoTool$InsertPlayerName(this.season, this.currentTeam, pos, fname, lname, (jerseyNumber & 255));

                if (vals != null && vals.length > 5) {
                    this.tool.TSBTool2$ITecmoTool$SetDefensivePlayerAbilities(this.season, this.currentTeam, pos, vals);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. Invalid player attributes.", [line]));
                }
                if (simVals != null && simVals.length > 1) {
                    this.tool.TSBTool2$ITecmoTool$SetDefensiveSimData(this.season, this.currentTeam, pos, simVals);
                } else {
                    if (this.showSimError) {
                        TSBTool.StaticUtils.AddError(System.String.format("Warning! On line '{0}'. No sim data specified.", [line]));
                    }
                }
            },
            SetKickPlayer: function (line) {
                var fname = TSBTool2.InputParser.GetFirstName(line);
                var lname = TSBTool2.InputParser.GetLastName(line);
                var pos = TSBTool2.InputParser.GetPosition(line);
                var face = TSBTool2.InputParser.GetFace(line);
                var jerseyNumber = TSBTool2.InputParser.GetJerseyNumber(line);
                var vals = TSBTool.StaticUtils.GetTsbAbilities(TSBTool2.InputParser.GetInts$1(line, false));
                var simVals = TSBTool2.InputParser.GetSimVals(line, true);

                this.tool.TSBTool2$ITecmoTool$SetFace(this.season, this.currentTeam, pos, face);
                this.tool.TSBTool2$ITecmoTool$InsertPlayerName(this.season, this.currentTeam, pos, fname, lname, (jerseyNumber & 255));
                if (System.String.startsWith(line, "K")) {
                    if (vals != null && vals.length > 7) {
                        this.tool.TSBTool2$ITecmoTool$SetKickerAbilities(this.season, this.currentTeam, pos, vals);
                    }
                } else {
                    if (vals != null && vals.length > 6) {
                        this.tool.TSBTool2$ITecmoTool$SetPunterAbilities(this.season, this.currentTeam, pos, vals);
                    }
                }

            },
            SetKickReturnMan: function (line) {
                var m = TSBTool2.InputParser.KickRetMan.match(line);
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    var pos = m.getGroups().get(1).toString();
                    if (this.tool.TSBTool2$ITecmoTool$IsValidPosition(pos)) {
                        this.tool.TSBTool2$ITecmoTool$SetKickReturner(this.season, this.currentTeam, pos);
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR with line '{0}'.", [line]));
                    }
                }
            },
            SetPuntReturnMan: function (line) {
                var m = TSBTool2.InputParser.PuntRetMan.match(line);
                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    var pos = m.getGroups().get(1).toString();
                    if (this.tool.TSBTool2$ITecmoTool$IsValidPosition(pos)) {
                        this.tool.TSBTool2$ITecmoTool$SetPuntReturner(this.season, this.currentTeam, pos);
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR with line '{0}'.", [line]));
                    }
                }
            }
        }
    });

    Bridge.define("TSBTool2.ITecmoTool", {
        $kind: "interface"
    });

    /**
     * Summary description for ScheduleHelper.
     *
     * @public
     * @class TSBTool2.SNES_ScheduleHelper
     */
    Bridge.define("TSBTool2.SNES_ScheduleHelper", {
        statics: {
            fields: {
                AUTO_CORRECT_SCHEDULE: false
            },
            ctors: {
                init: function () {
                    this.AUTO_CORRECT_SCHEDULE = true;
                }
            }
        },
        fields: {
            weekOneStartLoc: 0,
            teamGames: null,
            week: 0,
            week_game_count: 0,
            total_game_count: 0,
            gameRegex: null,
            gamesPerWeek: null,
            mTool: null,
            Teams: null
        },
        ctors: {
            init: function () {
                this.weekOneStartLoc = 1438654;
                this.gameRegex = new System.Text.RegularExpressions.Regex.ctor("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");
                this.gamesPerWeek = System.Array.init([
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14, 
                    14
                ], System.Int32);
            },
            ctor: function (tool) {
                this.$initialize();
                this.mTool = tool;
            }
        },
        methods: {
            GetTeamIndex: function (team) {
                return this.Teams.indexOf(team);
            },
            GetTeamFromIndex: function (index) {
                return this.Teams.getItem(index);
            },
            SetWeekOneLocation: function (loc, gamesPerWeek, teams) {
                this.weekOneStartLoc = loc;
                this.gamesPerWeek = gamesPerWeek;
                this.Teams = teams;
            },
            CloseWeek: function () {
                if (this.week > -1) {
                    var i = this.week_game_count;
                    while (i < 14) {
                        this.ScheduleGame$2(255, 255, this.week, i);
                        i = (i + 1) | 0;
                    }
                }
                this.week = (this.week + 1) | 0;
                this.total_game_count = (this.total_game_count + this.week_game_count) | 0;
                this.week_game_count = 0;
            },
            /**
             * Applies a schedule to the rom.
             *
             * @instance
             * @public
             * @this TSBTool2.SNES_ScheduleHelper
             * @memberof TSBTool2.SNES_ScheduleHelper
             * @param   {System.Collections.Generic.List$1}    lines    the contents of the schedule file.
             * @return  {void}
             */
            ApplySchedule: function (lines) {
                this.week = -1;
                this.week_game_count = 0;
                this.total_game_count = 0;

                if (TSBTool2.SNES_ScheduleHelper.AUTO_CORRECT_SCHEDULE && this.gamesPerWeek.length === 18) {
                    lines = this.Ensure18Weeks(lines);
                }

                var line;
                for (var i = 0; i < lines.Count; i = (i + 1) | 0) {
                    line = Bridge.toString(lines.getItem(i)).trim().toLowerCase();
                    if (System.String.startsWith(line, "#") || line.length < 3) {
                    } else if (System.String.startsWith(line, "week")) {
                        if (this.week > 18) {
                            TSBTool.StaticUtils.AddError("Error! You can have a maximum of 18 weeks in a season.");
                            break;
                        }
                        this.CloseWeek();
                        TSBTool.StaticUtils.WriteError(System.String.format("Scheduleing {0}", [line]));
                    } else {
                        this.ScheduleGame$1(line);
                    }
                }
                this.CloseWeek();

                if (this.week < 18 && this.gamesPerWeek.length === 18) {
                    TSBTool.StaticUtils.AddError("Warning! You didn't schedule all 18 weeks. The schedule could be messed up.");
                }
                if (this.teamGames != null) {
                    for (var i1 = 0; i1 < this.teamGames.length; i1 = (i1 + 1) | 0) {
                        if (this.teamGames[System.Array.index(i1, this.teamGames)] !== 16) {
                            TSBTool.StaticUtils.AddError(System.String.format("Warning! The {0} have {1} games scheduled.", this.GetTeamFromIndex(i1), Bridge.box(this.teamGames[System.Array.index(i1, this.teamGames)], System.Int32)));
                        }
                    }
                }
            },
            /**
             * Attempts to schedule a game.
             *
             * @instance
             * @private
             * @this TSBTool2.SNES_ScheduleHelper
             * @memberof TSBTool2.SNES_ScheduleHelper
             * @param   {string}     line
             * @return  {boolean}            True on success, false on failure.
             */
            ScheduleGame$1: function (line) {
                var ret = false;
                var m = this.gameRegex.match(line);
                var awayTeam, homeTeam;

                if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                    awayTeam = m.getGroups().get(1).toString();
                    homeTeam = m.getGroups().get(2).toString();
                    if (this.week_game_count > ((Bridge.Int.div(this.Teams.Count, 2)) | 0)) {
                        TSBTool.StaticUtils.AddError(System.String.format("Error! Week {0}: You can have no more than {1} games in a week.", Bridge.box(((this.week + 1) | 0), System.Int32), Bridge.box(((Bridge.Int.div(this.Teams.Count, 2)) | 0), System.Int32)));
                        ret = false;
                    } else if (this.ScheduleGame(awayTeam, homeTeam, this.week, this.week_game_count)) {
                        this.week_game_count = (this.week_game_count + 1) | 0;
                        ret = true;
                    }

                }
                var total_possible_games = Bridge.Int.mul((((Bridge.Int.div(this.Teams.Count, 2)) | 0)), 16);
                if (((this.total_game_count + this.week_game_count) | 0) > total_possible_games) {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! Week {0}: There are more than {1} games scheduled.", Bridge.box(((this.week + 1) | 0), System.Int32), Bridge.box(total_possible_games, System.Int32)));
                }
                return ret;
            },
            /**
             * @instance
             * @public
             * @this TSBTool2.SNES_ScheduleHelper
             * @memberof TSBTool2.SNES_ScheduleHelper
             * @param   {string}     awayTeam      
             * @param   {string}     homeTeam      
             * @param   {number}     week          Week is 0-16 (0 = week 1).
             * @param   {number}     gameOfWeek
             * @return  {boolean}
             */
            ScheduleGame: function (awayTeam, homeTeam, week, gameOfWeek) {
                var awayIndex = this.GetTeamIndex(awayTeam);
                var homeIndex = this.GetTeamIndex(homeTeam);

                if (awayIndex === -1 || homeIndex === -1) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error! Week {2}: Game '{0} at {1}'", awayTeam, homeTeam, Bridge.box(((week + 1) | 0), System.Int32)));
                    return false;
                }

                if (awayIndex === homeIndex && awayIndex < this.Teams.Count) {
                    TSBTool.StaticUtils.AddError(System.String.format("Warning! Week {0}: The {1} are scheduled to play against themselves.", Bridge.box(((week + 1) | 0), System.Int32), awayTeam));
                }

                if (week < 0 || week > 17) {
                    TSBTool.StaticUtils.AddError(System.String.format("Week {0} is not valid. Weeks range 1 - 18.", [Bridge.box(((week + 1) | 0), System.Int32)]));
                    return false;
                }
                if (this.GameLocation(week, gameOfWeek) < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("Game {0} for week {1} is not valid. Valid games for week {1} are 0-{2}.", Bridge.box(gameOfWeek, System.Int32), Bridge.box(week, System.Int32), Bridge.box(((this.gamesPerWeek[System.Array.index(week, this.gamesPerWeek)] - 1) | 0), System.Int32)));
                    TSBTool.StaticUtils.AddError(System.String.format("{0} at {1}", awayTeam, homeTeam));
                }

                this.ScheduleGame$2(awayIndex, homeIndex, week, gameOfWeek);

                if (Bridge.referenceEquals(awayTeam, "null") || Bridge.referenceEquals(homeTeam, "null")) {
                    return false;
                }
                return true;
            },
            ScheduleGame$2: function (awayTeamIndex, homeTeamIndex, week, gameOfWeek) {
                var location = this.GameLocation(week, gameOfWeek);
                if (location > 0) {
                    this.mTool.TSBTool2$ITecmoTool$SetByte(location, (awayTeamIndex & 255));
                    this.mTool.TSBTool2$ITecmoTool$SetByte(((location + 1) | 0), (homeTeamIndex & 255));
                    if (awayTeamIndex < this.Teams.Count) {
                        this.IncrementTeamGames(awayTeamIndex);
                        this.IncrementTeamGames(homeTeamIndex);
                    }
                }
                /* else
                {
                   TSBTool.StaticUtils.AddError(string.Format("INVALID game for ROM. Week={0} Game of Week ={1}",
                       week,gameOfWeek);
                }*/
            },
            /**
             * Returns a string like "49ers at giants", for a valid week, game of week combo.
             *
             * @instance
             * @public
             * @this TSBTool2.SNES_ScheduleHelper
             * @memberof TSBTool2.SNES_ScheduleHelper
             * @param   {number}    week          The week in question.
             * @param   {number}    gameOfWeek    The game to get.
             * @return  {string}                  Returns a string like "49ers at giants", for a valid week, game of week combo, returns null
             upon error.
             */
            GetGame: function (week, gameOfWeek) {
                var $t, $t1;
                var location = this.GameLocation(week, gameOfWeek);
                if (location === -1) {
                    return null;
                }
                var awayIndex = ($t = this.mTool.TSBTool2$ITecmoTool$OutputRom)[System.Array.index(location, $t)];
                var homeIndex = ($t1 = this.mTool.TSBTool2$ITecmoTool$OutputRom)[System.Array.index(((location + 1) | 0), $t1)];
                var ret = "";

                if (awayIndex < this.Teams.Count) {
                    ret = System.String.format("{0} at {1}", this.GetTeamFromIndex(awayIndex), this.GetTeamFromIndex(homeIndex));
                }
                return ret;
            },
            /**
             * Returns a week from the season.
             *
             * @instance
             * @public
             * @this TSBTool2.SNES_ScheduleHelper
             * @memberof TSBTool2.SNES_ScheduleHelper
             * @param   {number}    week    The week to get [0-16] (0= week 1).
             * @return  {string}
             */
            GetWeek: function (week) {
                if (week < 0 || week > ((this.gamesPerWeek.length - 1) | 0)) {
                    return null;
                }
                var sb = new System.Text.StringBuilder("", 280);
                sb.append(System.String.format("WEEK {0}\n", [Bridge.box(((week + 1) | 0), System.Int32)]));

                var game;

                for (var i = 0; i < this.gamesPerWeek[System.Array.index(week, this.gamesPerWeek)]; i = (i + 1) | 0) {
                    game = this.GetGame(week, i);
                    if (game != null && game.length > 0) {
                        sb.append(System.String.format("{0}\n", [game]));
                    }
                }
                sb.append("\n");
                return sb.toString();
            },
            GetSchedule: function () {
                var sb = new System.Text.StringBuilder("", 5040);
                for (var week = 0; week < this.gamesPerWeek.length; week = (week + 1) | 0) {
                    sb.append(this.GetWeek(week));
                }

                return sb.toString();
            },
            GameLocation: function (week, gameOfweek) {
                if (week < 0 || week > ((this.gamesPerWeek.length - 1) | 0) || gameOfweek > this.gamesPerWeek[System.Array.index(week, this.gamesPerWeek)] || gameOfweek < 0) {
                    return -1;
                }

                var offset = 0;
                for (var i = 0; i < week; i = (i + 1) | 0) {
                    offset = (offset + (Bridge.Int.mul(this.gamesPerWeek[System.Array.index(i, this.gamesPerWeek)], 2))) | 0;
                }

                offset = (offset + (Bridge.Int.mul(gameOfweek, 2))) | 0;
                var location = (this.weekOneStartLoc + offset) | 0;
                return location;
            },
            IncrementTeamGames: function (teamIndex) {
                if (this.teamGames == null) {
                    this.teamGames = System.Array.init(this.Teams.Count, 0, System.Int32);
                }
                if (teamIndex < this.teamGames.length) {
                    this.teamGames[System.Array.index(teamIndex, this.teamGames)] = (this.teamGames[System.Array.index(teamIndex, this.teamGames)] + 1) | 0;
                }

            },
            Ensure18Weeks: function (lines) {

                var wks = this.CountWeeks(lines);
                var line1, line2;
                for (var i = (lines.Count - 2) | 0; i > 0; i = (i - 2) | 0) {
                    line1 = Bridge.toString(lines.getItem(i));
                    line2 = Bridge.toString(lines.getItem(((i + 1) | 0)));
                    if (wks > 17) {
                        break;
                    } else if (System.String.indexOf(line1, "at") > -1 && System.String.indexOf(line2, "at") > -1) {
                        lines.insert(((i + 1) | 0), "WEEK ");
                        i = (i - 1) | 0;
                        wks = (wks + 1) | 0;
                    }
                }

                return lines;
            },
            CountWeeks: function (lines) {
                var $t;
                var count = 0;
                $t = Bridge.getEnumerator(lines);
                try {
                    while ($t.moveNext()) {
                        var line = $t.Current;
                        if (System.String.indexOf(line.toLowerCase(), "week") > -1) {
                            count = (count + 1) | 0;
                        }
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
                return count;
            }
        }
    });

    Bridge.define("TSBTool2.TecmoConverter", {
        statics: {
            methods: {
                Convert: function (from, to, content) {
                    if (from === to) {
                        return content;
                    }
                    var retVal = "";
                    if (from === TSBTool.TSBContentType.TSB1 && to === TSBTool.TSBContentType.TSB2) {
                        retVal = TSBTool2.TSB2Converter.ConvertToTSB2FromTSB1(content);
                    } else if (from === TSBTool.TSBContentType.TSB1 && to === TSBTool.TSBContentType.TSB3) {
                        retVal = TSBTool2.TSB2Converter.ConvertToTSB2FromTSB1(content);
                        retVal = TSBTool2.TSB3Converter.ConvertToTSB3FromTSB2(retVal);
                    } else if (from === TSBTool.TSBContentType.TSB2 && to === TSBTool.TSBContentType.TSB3) {
                        retVal = TSBTool2.TSB3Converter.ConvertToTSB3FromTSB2(content);
                    } else if (from === TSBTool.TSBContentType.TSB2 && to === TSBTool.TSBContentType.TSB1) {
                        retVal = TSBTool2.TSB1Converter.ConvertToTSB1FromTSB2(content);
                    } else if (from === TSBTool.TSBContentType.TSB3 && to === TSBTool.TSBContentType.TSB1) {
                        retVal = TSBTool2.TSB2Converter.ConvertToTSB2FromTSB3(content);
                        retVal = TSBTool2.TSB1Converter.ConvertToTSB1FromTSB2(retVal);
                    } else if (from === TSBTool.TSBContentType.TSB3 && to === TSBTool.TSBContentType.TSB2) {
                        retVal = TSBTool2.TSB2Converter.ConvertToTSB2FromTSB3(content);
                    }
                    return retVal;
                }
            }
        }
    });

    Bridge.define("TSBTool2.TSB1Converter", {
        statics: {
            fields: {
                CONVERT_MSG: null
            },
            ctors: {
                init: function () {
                    this.CONVERT_MSG = "Converting between TSB1, TSB2 and TSB3 data is not exact and one does not 'un-do' the other.\r\nWhen converting from TSB1 --> TSB2 a 'Auto-update' sim data operation is performed (feature taken from TSBToolSupreme).\r\n\r\n";
                }
            },
            methods: {
                ConvertToTSB1FromTSB2: function (input) {
                    var $t;
                    input = System.String.replaceAll(input, "\r\n", "\n");
                    var lines = System.String.split(input, System.String.toCharArray(("\n"), 0, ("\n").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    var builder = new System.Text.StringBuilder("", ((input.length + Bridge.Int.mul(lines.length, 2)) | 0));
                    var tmp = "";
                    $t = Bridge.getEnumerator(lines);
                    try {
                        while ($t.moveNext()) {
                            var line = $t.Current;
                            if (TSBTool2.TSB1Converter.IsPlayerLine(line)) {
                                tmp = TSBTool2.TSB1Converter.ConvertToTSB1Player(line);
                                builder.append(tmp);
                            } else if (System.String.startsWith(line, "SEASON")) {
                                builder.append("#" + (line || ""));
                            } else if (System.String.startsWith(line, "PLAYBOOK")) {
                                tmp = TSBTool2.TSB1Converter.ConvertPlaybook(line);
                                builder.append(tmp);
                            } else if (System.String.startsWith(line, "#COLORS")) {
                                builder.append(line.substr(1));
                            } else if (System.String.startsWith(line, "TEAM_ABB")) {
                                builder.append(line.toUpperCase());
                            } else {
                                builder.append(line);
                            }
                            builder.append("\n");
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    var retVal = TSBTool.TecmonsterTSB1SimAutoUpdater.AutoUpdatePlayerSimData(builder.toString());
                    return retVal;
                },
                ConvertPlaybook: function (line) {
                    return "#" + (line || "");
                },
                IsPlayerLine: function (line) {
                    var index = System.String.indexOf(line, String.fromCharCode(44));
                    var pos = "";
                    if (index > 1) {
                        pos = line.substr(0, index);
                    }
                    return TSBTool2.TSB2Tool.positionNames.indexOf(pos) > -1;
                },
                ConvertToTSB1Player: function (tsb2PlayerLine) {
                    var simIndex = System.String.indexOf(tsb2PlayerLine, "[");
                    var simString = "";
                    var parts = null;
                    if (simIndex > -1) {
                        simString = tsb2PlayerLine.substr(simIndex);
                        parts = System.String.split(tsb2PlayerLine.substr(0, ((simIndex - 1) | 0)), System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    } else {
                        parts = System.String.split(tsb2PlayerLine, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    }

                    var rs = parts[System.Array.index(4, parts)];
                    var rp = parts[System.Array.index(5, parts)];
                    parts[System.Array.index(5, parts)] = rs;
                    parts[System.Array.index(4, parts)] = rp;

                    var attrs = new (System.Collections.Generic.List$1(System.String)).$ctor1(parts);

                    var sb = new System.Text.StringBuilder("", 60);
                    var pos = parts[System.Array.index(0, parts)].trim();
                    switch (pos) {
                        case "QB1": 
                        case "QB2": 
                            attrs.removeAt(13);
                            attrs.removeAt(8);
                            break;
                        case "K": 
                            attrs.removeAt(9);
                            attrs.removeAt(8);
                            break;
                        case "LB5": 
                        case "DB3": 
                        case "RE2": 
                        case "NT2": 
                        case "LE2": 
                            return "#" + (tsb2PlayerLine || "");
                        default: 
                            attrs.removeAt(8);
                            break;
                    }
                    for (var i = 0; i < attrs.Count; i = (i + 1) | 0) {
                        sb.append(attrs.getItem(i));
                        sb.append(",");
                    }
                    if (System.String.indexOf(("RE,NT,LE,ROLB,RILB,LILB,LOLB,RCB,LCB,FS,SS"), pos) > -1) {
                        sb.append("[2,2]");
                    }
                    var retVal = sb.toString();

                    return System.String.replaceAll(retVal, ",,", ",");
                },
                TestQbTSB1Conversion: function () {
                    var retVal = "";
                    var joe = "QB1,joe MONTANA,Face=0x01,#16,25,69,19,13,25,56,81,81,75,81,[51,00,06]";
                    var resultJoe = "QB1,joe MONTANA,Face=0x01,#16,69,25,19,13,56,81,81,75,";

                    var test = TSBTool2.TSB1Converter.ConvertToTSB1Player(joe);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultJoe, test)) || "");

                    var vinny = "QB1,vinny TESTAVERDE,Face=0x03,#14,25,69,31,13,25,31,56,44,44,44,[51,00,06]";
                    var resultVinny = "QB1,vinny TESTAVERDE,Face=0x03,#14,69,25,31,13,31,56,44,44,";
                    test = TSBTool2.TSB1Converter.ConvertToTSB1Player(vinny);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultVinny, test)) || "");
                    return retVal;
                },
                TestRbTSB1Conversion: function () {
                    var retVal = "";
                    var thruman = "RB1,thurman THOMAS,Face=0x83,#34,38,69,63,25,63,75,50,[1A,00,06,05]";
                    var resultThurman = "RB1,thurman THOMAS,Face=0x83,#34,69,38,63,25,75,50,";

                    var test = TSBTool2.TSB1Converter.ConvertToTSB1Player(thruman);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultThurman, test)) || "");

                    var roger = "RB1,roger CRAIG,Face=0x80,#33,38,69,50,25,50,50,44,[1A,00,06,05]";
                    var resultRoger = "RB1,roger CRAIG,Face=0x80,#33,69,38,50,25,50,44,";
                    test = TSBTool2.TSB1Converter.ConvertToTSB1Player(roger);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultRoger, test)) || "");
                    return retVal;
                },
                TestDbTSB1Conversion: function () {
                    var retVal = "";
                    var deion = "RCB,deion SANDERS,Face=0x8e,#21,44,56,75,56,75,56,50,[02,B1,02]";
                    var resultDeion = "RCB,deion SANDERS,Face=0x8e,#21,56,44,75,56,56,50,[2,2]";

                    var test = TSBTool2.TSB1Converter.ConvertToTSB1Player(deion);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultDeion, test)) || "");
                    return retVal;
                },
                TestLbTSB1Conversion: function () {
                    var retVal = "";
                    var ray = "RILB,ray BENTLEY,Face=0x00,#50,25,31,38,38,19,31,56,[1A,1E,1A]";
                    var resultRay = "RILB,ray BENTLEY,Face=0x00,#50,31,25,38,38,31,56,[2,2]";

                    var test = TSBTool2.TSB1Converter.ConvertToTSB1Player(ray);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultRay, test)) || "");
                    return retVal;
                },
                TestDlTSB1Conversion: function () {
                    var retVal = "";
                    var target = "LE,leon SEALS,Face=0x8c,#96,25,31,38,44,13,31,50,[32,36,32]";
                    var resultTarget = "LE,leon SEALS,Face=0x8c,#96,31,25,38,44,31,50,[2,2]";

                    var test = TSBTool2.TSB1Converter.ConvertToTSB1Player(target);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultTarget, test)) || "");
                    return retVal;
                },
                TestOlTSB1Conversion: function () {
                    var retVal = "";
                    var target = "LG,jim RITCHER,Face=0x07,#51,25,69,38,56,13,";
                    var resultTarget = "LG,jim RITCHER,Face=0x07,#51,69,25,38,56,";

                    var test = TSBTool2.TSB1Converter.ConvertToTSB1Player(target);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultTarget, test)) || "");
                    return retVal;
                },
                TestKickerTSB1Conversion: function () {
                    var retVal = "";
                    var target = "K,scott NORWOOD,Face=0x09,#11,56,81,81,31,13,44,44,44,[6]";
                    var resultTarget = "K,scott NORWOOD,Face=0x09,#11,81,56,81,31,44,44,";

                    var test = TSBTool2.TSB1Converter.ConvertToTSB1Player(target);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultTarget, test)) || "");
                    return retVal;
                },
                TestPunterTSB1Conversion: function () {
                    var retVal = "";
                    var target = "P,chris MOHR,Face=0x09,#9,81,25,44,31,13,13,44,69,[8]";
                    var resultTarget = "P,chris MOHR,Face=0x09,#9,25,81,44,31,13,44,69,";

                    var test = TSBTool2.TSB1Converter.ConvertToTSB1Player(target);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultTarget, test)) || "");
                    return retVal;
                }
            }
        }
    });

    /**
     * @public
     * @class TSBTool2.TSB2Converter
     */
    Bridge.define("TSBTool2.TSB2Converter", {
        statics: {
            methods: {
                ConvertToTSB2FromTSB3: function (input) {
                    var $t;
                    input = System.String.replaceAll(input, "\r\n", "\n");
                    var lines = System.String.split(input, System.String.toCharArray(("\n"), 0, ("\n").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    var builder = new System.Text.StringBuilder("", ((input.length + Bridge.Int.mul(lines.length, 2)) | 0));
                    var tmp = "";
                    var line = "";
                    $t = Bridge.getEnumerator(lines);
                    try {
                        while ($t.moveNext()) {
                            var theLine = $t.Current;
                            line = theLine;
                            if (TSBTool2.TSB2Converter.ShouldConvertTSB3Player(line)) {
                                tmp = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB3(line);
                                builder.append(tmp);
                            } else {
                                builder.append(line);
                            }
                            builder.append("\n");
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    TSBTool.StaticUtils.ShowErrors();
                    var retVal = builder.toString();
                    return retVal;
                },
                ConvertToTSB2FromTSB1: function (input) {
                    var $t;
                    input = System.String.replaceAll(input, "\r\n", "\n");
                    input = System.String.replaceAll(input, "titans", "oilers");
                    var lines = System.String.split(input, System.String.toCharArray(("\n"), 0, ("\n").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    var builder = new System.Text.StringBuilder("", ((input.length + Bridge.Int.mul(lines.length, 2)) | 0));
                    var tmp = "";
                    var line = "";

                    $t = Bridge.getEnumerator(lines);
                    try {
                        while ($t.moveNext()) {
                            var theLine = $t.Current;
                            line = theLine;
                            if (System.String.startsWith(line, "#RE2") || System.String.startsWith(line, "#NT2") || System.String.startsWith(line, "#LE2") || System.String.startsWith(line, "#LB5") || System.String.startsWith(line, "#DB3")) {
                                builder.append(line.substr(1));
                            } else if (System.String.startsWith(line, "#PLAYBOOK") && !Bridge.referenceEquals(TSBTool2.InputParser.playbookRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                                builder.append(line.substr(1));
                            } else if (System.String.startsWith(line.trim(), "#")) {
                                builder.append(line);
                            } else if (System.String.startsWith(line, "COLORS")) {
                                builder.append("#" + (line || ""));
                            } else if (TSBTool2.TSB2Converter.ShouldConvertTSB1Player(line)) {
                                tmp = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(line);
                                builder.append(tmp);
                            } else if (System.String.startsWith(line, "PLAYBOOK")) {
                                tmp = TSBTool2.TSB2Converter.ConvertPlaybook(line);
                                builder.append(tmp);
                            } else {
                                builder.append(line);
                            }
                            builder.append("\n");
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    TSBTool.StaticUtils.ShowErrors();
                    var retVal = builder.toString();
                    retVal = TSBTool2.TSBXSimAutoUpdater.AutoUpdatePlayerSimData(retVal, TSBTool.TSBContentType.TSB2);
                    return retVal;
                },
                ShouldConvertTSB1Player: function (line) {
                    var index = System.String.indexOf(line, String.fromCharCode(44));
                    var pos = "";
                    if (index > 0) {
                        pos = line.substr(0, index);
                    }
                    return TSBTool2.TSB2Tool.positionNames.indexOf(pos) > -1;
                },
                ShouldConvertTSB3Player: function (line) {
                    var index = System.String.indexOf(line, String.fromCharCode(44));
                    var pos = "";
                    if (index > 0) {
                        pos = line.substr(0, index);
                    }
                    return TSBTool2.TSB2Tool.positionNames.indexOf(pos) > -1;
                },
                ConvertPlaybook: function (line) {
                    var m = TSBTool2.InputParser.playbookRegex.match(line);
                    var runs = m.getGroups().get(1).toString();
                    var passes = m.getGroups().get(2).toString();

                    runs = runs.substr(1);
                    passes = passes.substr(1);
                    var retVal = System.String.format("PLAYBOOK R{0}{0}, P{1}{1}", runs, passes);

                    return retVal;
                },
                ConvertToTSB2PlayerFromTSB3: function (tsb1PlayerLine) {
                    var simIndex = System.String.indexOf(tsb1PlayerLine, "[");
                    var simString = "";
                    var parts = null;
                    if (simIndex > -1) {
                        simString = tsb1PlayerLine.substr(simIndex);
                        parts = System.String.split(tsb1PlayerLine.substr(0, ((simIndex - 1) | 0)), System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    } else {
                        parts = System.String.split(tsb1PlayerLine, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    }

                    var sb = new System.Text.StringBuilder("", 60);
                    var pos = parts[System.Array.index(0, parts)].trim();
                    for (var i = 0; i < parts.length; i = (i + 1) | 0) {
                        if (i !== 5) {
                            sb.append(parts[System.Array.index(i, parts)].trim());
                            sb.append(",");
                        }
                    }
                    sb.append(simString);
                    return sb.toString();
                },
                /**
                 * @static
                 * @this TSBTool2.TSB2Converter
                 * @memberof TSBTool2.TSB2Converter
                 * @param   {string}    tsb1PlayerLine
                 * @return  {string}
                 */
                ConvertToTSB2PlayerFromTSB1: function (tsb1PlayerLine) {
                    var simIndex = System.String.indexOf(tsb1PlayerLine, "[");
                    var simString = "";
                    var parts = null;
                    if (simIndex > -1) {
                        simString = tsb1PlayerLine.substr(((simIndex - 1) | 0));
                        parts = System.String.split(tsb1PlayerLine.substr(0, ((simIndex - 1) | 0)), System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    } else {
                        parts = System.String.split(tsb1PlayerLine, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    }
                    var sb = new System.Text.StringBuilder("", 60);
                    var pos = parts[System.Array.index(0, parts)].trim();
                    var rs = parts[System.Array.index(4, parts)];
                    var rp = parts[System.Array.index(5, parts)];
                    parts[System.Array.index(5, parts)] = rs;
                    parts[System.Array.index(4, parts)] = rp;
                    for (var i = 0; i < parts.length; i = (i + 1) | 0) {
                        switch (i) {
                            case 2: 
                                sb.append(TSBTool2.TSB2Converter.ConvertFaceToTSB2(parts[System.Array.index(i, parts)].trim()));
                                break;
                            case 7: 
                                sb.append((parts[System.Array.index(i, parts)].trim() || "") + ",");
                                sb.append(TSBTool2.TSB2Converter.GetBB(pos, parts[System.Array.index(6, parts)].trim()));
                                break;
                            case 8: 
                                switch (pos) {
                                    case "K": 
                                        sb.append((parts[System.Array.index(i, parts)].trim() || "") + ",");
                                        sb.append(parts[System.Array.index(i, parts)].trim());
                                        break;
                                    default: 
                                        sb.append(parts[System.Array.index(i, parts)].trim());
                                        break;
                                }
                                break;
                            case 11: 
                                sb.append((parts[System.Array.index(i, parts)].trim() || "") + ",");
                                sb.append(TSBTool2.TSB2Converter.GetCoolness(parts[System.Array.index(1, parts)], parts[System.Array.index(10, parts)].trim()));
                                break;
                            default: 
                                sb.append(parts[System.Array.index(i, parts)].trim());
                                break;
                        }
                        sb.append(",");
                    }
                    TSBTool2.TSB2Converter.AddSimValues(pos, simString, sb);
                    return sb.toString();
                },
                AddSimValues: function (pos, simString, sb) {
                    var vals = TSBTool2.InputParser.GetSimVals(simString, false);
                    var simVals = "";
                    switch (pos) {
                        case "QB1": 
                        case "QB2": 
                            simVals = "[51,00,06]";
                            break;
                        case "RB1": 
                            simVals = "[1A,00,06,05]";
                            break;
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                            simVals = "[00,03,00,00]";
                            break;
                        case "WR1": 
                        case "WR2": 
                            simVals = "[00,07,0A,00]";
                            break;
                        case "WR3": 
                        case "WR4": 
                            simVals = "[0A,00,01,05]";
                            break;
                        case "TE1": 
                        case "TE2": 
                            simVals = "[03,00,00,00]";
                            break;
                        case "RE": 
                        case "NT": 
                        case "LE": 
                        case "RE2": 
                        case "NT2": 
                        case "LE2": 
                        case "ROLB": 
                        case "RILB": 
                        case "LILB": 
                        case "LOLB": 
                        case "LB5": 
                        case "RCB": 
                        case "LCB": 
                        case "DB1": 
                        case "DB2": 
                        case "FS": 
                        case "SS": 
                        case "DB3": 
                            if (simVals.length > 1) {
                                simVals = System.String.format("[{0:X2},{1:X2},{2:X2}]", Bridge.box(Bridge.Int.mul(vals[System.Array.index(0, vals)], 2), System.Int32), Bridge.box(Bridge.Int.mul(vals[System.Array.index(1, vals)], 3), System.Int32), Bridge.box(Bridge.Int.mul(vals[System.Array.index(0, vals)], 2), System.Int32));
                            } else {
                                simVals = "[10,10,10]";
                            }
                            break;
                        case "K": 
                        case "P": 
                            simVals = "[" + vals[System.Array.index(0, vals)] + "]";
                            break;
                    }
                    sb.append(simVals);
                },
                GetCoolness: function (name, pa) {
                    var space = System.String.indexOf(name, String.fromCharCode(32));
                    if (space > -1) {
                        var lastName = name.substr(space).toUpperCase();
                        var coolGuys = " KELLY KOSAR MOON YOUNG ELWAY FAVRE MARINO MONTANA MANNING BRADY BREES MAHOMES RODGERS ROETHLISBERGER BRADSHAW WILSON STAUBACH FOLES UNITAS TARKENTON TESTAVERDE ";
                        if (System.String.indexOf(coolGuys, lastName) > -1) {
                            return "81";
                        }
                    }
                    return pa;
                },
                GetBB: function (pos, ms) {
                    var retVal = "13";
                    var ms_i = System.Int32.parse(ms);
                    switch (pos) {
                        case "RCB": 
                        case "LCB": 
                        case "DB1": 
                        case "DB2": 
                        case "RB1": 
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                            retVal = ms;
                            break;
                        case "LOLB": 
                        case "LILB": 
                        case "RILB": 
                        case "ROLB": 
                        case "LB5": 
                            retVal = "19";
                            break;
                        case "SS": 
                        case "FS": 
                            retVal = "25";
                            break;
                        case "QB1": 
                        case "QB2": 
                        case "WR1": 
                        case "WR2": 
                        case "WR3": 
                        case "WR4": 
                            if (ms_i > 44) {
                                retVal = "44";
                            } else {
                                retVal = "25";
                            }
                            break;
                    }
                    return retVal;
                },
                ConvertFaceToTSB2: function (input) {
                    var tmp = System.String.replaceAll(input, "Face=0x", "");
                    var number = TSBTool.StaticUtils.ParseIntFromHexString(tmp);
                    if (number < 128) {
                        number = number & 15;
                    } else {
                        number = (128 + (number & 15)) | 0;
                    }
                    var retVal = System.String.format("Face=0x{0:x2}", [Bridge.box(number, System.Int32)]);
                    return retVal;
                },
                TestQbTSB2Conversion: function () {
                    var retVal = "";
                    var joe = "QB1,joe MONTANA,Face=0x1, #16, 25, 69, 19, 13, 56, 81, 81, 75 ,[3, 12, 2 ]";
                    var resultJoe = "QB1,joe MONTANA,Face=0x01,#16,69,25,19,13,25,56,81,81,75,81,[51,00,06]";

                    var test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(joe);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultJoe, test)) || "");

                    var vinny = "QB1, vinny TESTAVERDE, Face=0x23, #14, 25, 69, 31, 13, 31, 56, 44, 44 ,[5, 4, 0 ]";
                    var resultVinny = "QB1,vinny TESTAVERDE,Face=0x03,#14,69,25,31,13,25,31,56,44,44,44,[51,00,06]";
                    test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(vinny);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultVinny, test)) || "");
                    return retVal;
                },
                TestRbTSB2Conversion: function () {
                    var retVal = "";
                    var thruman = "RB1, thurman THOMAS, Face=0x83, #34, 38,  69,63,25,75,50 ,[10, 7, 8, 8 ]";
                    var resultThurman = "RB1,thurman THOMAS,Face=0x83,#34,69,38,63,25,63,75,50,[1A,00,06,05]";

                    var test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(thruman);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultThurman, test)) || "");

                    var roger = "RB1, roger CRAIG,Face=0xd0,#33,38,69,50,25,50,44,[6, 7, 7, 2 ]";
                    var resultRoger = "RB1,roger CRAIG,Face=0x80,#33,69,38,50,25,50,50,44,[1A,00,06,05]";
                    test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(roger);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultRoger, test)) || "");
                    return retVal;
                },
                TestDbTSB2Conversion: function () {
                    var retVal = "";
                    var deion = "RCB, deion SANDERS, Face=0x9e, #21,44,56,75,56,56,50,[1, 59 ]";
                    var resultDeion = "RCB,deion SANDERS,Face=0x8e,#21,56,44,75,56,75,56,50,[02,B1,02]";

                    var test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(deion);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultDeion, test)) || "");
                    return retVal;
                },
                TestLbTSB2Conversion: function () {
                    var retVal = "";
                    var ray = "RILB, ray BENTLEY, Face=0x30, #50, 25, 31, 38, 38, 31, 56 ,[13, 10 ]";
                    var resultRay = "RILB,ray BENTLEY,Face=0x00,#50,31,25,38,38,19,31,56,[1A,1E,1A]";

                    var test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(ray);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultRay, test)) || "");
                    return retVal;
                },
                TestDlTSB2Conversion: function () {
                    var retVal = "";
                    var target = "LE, leon SEALS, Face=0xac, #96, 25, 31, 38, 44, 31, 50 ,[25, 18 ]";
                    var resultTarget = "LE,leon SEALS,Face=0x8c,#96,31,25,38,44,13,31,50,[32,36,32]";

                    var test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(target);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultTarget, test)) || "");
                    return retVal;
                },
                TestOlTSB2Conversion: function () {
                    var retVal = "";
                    var target = "LG, jim RITCHER, Face=0x7, #51, 25, 69, 38, 56";
                    var resultTarget = "LG,jim RITCHER,Face=0x07,#51,69,25,38,56,13,";

                    var test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(target);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultTarget, test)) || "");
                    return retVal;
                },
                TestKickerTSB2Conversion: function () {
                    var retVal = "";
                    var target = "K, scott NORWOOD, Face=0x29, #11, 56, 81, 81, 31, 44, 44 ,[6 ]";
                    var resultTarget = "K,scott NORWOOD,Face=0x09,#11,81,56,81,31,13,44,44,44,[6]";

                    var test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(target);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultTarget, test)) || "");
                    return retVal;
                },
                TestPunterTSB2Conversion: function () {
                    var retVal = "";
                    var target = "P,chris MOHR,Face=0x09,#9,81,25,44,31,13,44,69,[8]";
                    var resultTarget = "P,chris MOHR,Face=0x09,#9,25,81,44,31,13,13,44,69,[8]";

                    var test = TSBTool2.TSB2Converter.ConvertToTSB2PlayerFromTSB1(target);
                    retVal = (retVal || "") + ((TSBTool.StaticUtils.AreEqual(resultTarget, test)) || "");
                    return retVal;
                }
            }
        }
    });

    Bridge.define("TSBTool2.TSB3Converter", {
        statics: {
            fields: {
                sFormulaString: null
            },
            props: {
                FormulaString: {
                    get: function () {
                        if (TSBTool2.TSB3Converter.sFormulaString == null) {
                            TSBTool2.TSB3Converter.sFormulaString = "# Free Agent point formulas\r\n\r\nFAP_QB: (PS+PC+AR)/15\r\nFAP_RB: ((MS+BC+RC)-110)/4\r\nFAP_WR: ((RC -38 + MS - 38)/3) -2\r\nFAP_TE: ((RC -25 + MS - 25)/3) +1\r\nFAP_OL: (HP -44)/2\r\nFAP_DL: (MS + HP -50) / 6\r\nFAP_LB: (MS-31 + HP-31)/4\r\nFAP_CB: (MS-31 + PI-25)/5\r\nFAP_S: (MS-31 + PI-25 + HP-31)/7\r\nFAP_K: KA/6\r\nFAP_P: (KP-31)/6\r\n";
                        }
                        return TSBTool2.TSB3Converter.sFormulaString;
                    }
                }
            },
            methods: {
                ReloadFormulas: function () {
                    TSBTool2.TSB3Converter.sFormulaString = null;
                },
                ConvertToTSB3FromTSB2: function (input) {
                    var $t;
                    TSBTool2.TSB3Converter.ReloadFormulas();
                    input = System.String.replaceAll(input, "\r\n", "\n");
                    var lines = System.String.split(input, System.String.toCharArray(("\n"), 0, ("\n").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    var builder = new System.Text.StringBuilder("", ((input.length + Bridge.Int.mul(lines.length, 2)) | 0));
                    var tmp = "";
                    var line = "";
                    $t = Bridge.getEnumerator(lines);
                    try {
                        while ($t.moveNext()) {
                            var theLine = $t.Current;
                            line = theLine;
                            if (TSBTool2.TSB3Converter.ShouldConvertTSB3Player(line)) {
                                tmp = TSBTool2.TSB3Converter.ConvertToTSB3PlayerFromTSB2(line);
                                builder.append(tmp);
                            } else {
                                builder.append(line);
                            }
                            builder.append("\n");
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    TSBTool.StaticUtils.ShowErrors();
                    var retVal = builder.toString();
                    return retVal;
                },
                ShouldConvertTSB3Player: function (line) {
                    var index = System.String.indexOf(line, String.fromCharCode(44));
                    var pos = "";
                    if (index > 0) {
                        pos = line.substr(0, index);
                    }
                    return TSBTool2.TSB2Tool.positionNames.indexOf(pos) > -1;
                },
                ConvertToTSB3PlayerFromTSB2: function (input) {
                    var retVal = "";
                    var simIndex = System.String.indexOf(input, "[");
                    var simString = "";
                    var parts = null;
                    if (simIndex > -1) {
                        simString = input.substr(simIndex);
                        parts = new (System.Collections.Generic.List$1(System.String)).$ctor1(System.String.split(input.substr(0, ((simIndex - 1) | 0)), System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }})));
                    } else {
                        parts = new (System.Collections.Generic.List$1(System.String)).$ctor1(System.String.split(input, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }})));
                    }
                    var sb = new System.Text.StringBuilder("", 60);
                    var pos = parts.getItem(0).trim();
                    var hp = parts.getItem(7).trim();
                    var ms = parts.getItem(6).trim();
                    var ag = TSBTool2.TSB3Converter.GetAG(pos, ms, hp);
                    parts.insert(9, ag);
                    for (var i = 0; i < parts.Count; i = (i + 1) | 0) {
                        sb.append(parts.getItem(i).trim());
                        sb.append(",");
                    }
                    sb.append(simString);
                    retVal = TSBTool2.TSB3Converter.UpdateFreeAgentValue(System.String.replaceAll(sb.toString(), ",,", ","));
                    return retVal;
                },
                UpdateFreeAgentValue: function (input) {
                    var simIndex = System.String.indexOf(input, "[");
                    var simString = "";
                    var parts = null;
                    if (simIndex > -1) {
                        simString = input.substr(simIndex);
                        parts = new (System.Collections.Generic.List$1(System.String)).$ctor1(System.String.split(input.substr(0, ((simIndex - 1) | 0)), System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }})));
                    } else {
                        parts = new (System.Collections.Generic.List$1(System.String)).$ctor1(System.String.split(input, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }})));
                    }

                    var pos = parts.getItem(0).trim();
                    var substitutionString = { v : "" };
                    var formula = TSBTool2.TSB3Converter.GetFreeAgentPointFormula(pos, substitutionString);
                    var sub_parts = System.String.split(substitutionString.v, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    for (var i = 4; i < sub_parts.length; i = (i + 1) | 0) {
                        formula = System.String.replaceAll(formula, sub_parts[System.Array.index(i, sub_parts)].trim(), parts.getItem(i));
                    }
                    var result = 0;
                    try {
                        var r = Bridge.toString(TSBTool.StaticUtils.Compute(formula));
                        result = System.Double.parse(r);
                    } catch ($e1) {
                        $e1 = System.Exception.create($e1);
                        throw new System.ArgumentException.$ctor1(System.String.format("Error calculating FreeAgentPoints for '{0}' Tried to compute:'{1}'", input, formula));
                    }
                    var fap = Bridge.Int.clipu8(Bridge.Math.round(result, 0, 6));
                    if (fap < 0) {
                        fap = 0;
                    } else {
                        if (fap > 15) {
                            fap = 15;
                        }
                    }

                    var face = (parts.getItem(2).trim().substr(0, ((parts.getItem(2).length - 1) | 0)) || "") + (System.Byte.format(fap, "X") || "");
                    return System.String.replaceAll(input, parts.getItem(2), face);
                },
                GetFreeAgentPointFormula: function (position, substitutionString) {
                    var retVal = null;
                    var searchStr = "FAP_OL";
                    substitutionString.v = "OL,name      ,face    , JN,RS,RP,MS,HP,BB";
                    switch (position) {
                        case "QB1": 
                        case "QB2": 
                            searchStr = "FAP_QB";
                            substitutionString.v = "QB,name       ,face    , JN,RS,RP,MS,HP,BB,AG,PS,PC,PA,AR,CO";
                            break;
                        case "RB1": 
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                            searchStr = "FAP_RB";
                            substitutionString.v = "SKILL,name ,face    , JN,RS,RP,MS,HP,BB,AG,BC,RC";
                            break;
                        case "WR1": 
                        case "WR2": 
                        case "WR3": 
                        case "WR4": 
                            searchStr = "FAP_WR";
                            substitutionString.v = "SKILL,name ,face    , JN,RS,RP,MS,HP,BB,AG,BC,RC";
                            break;
                        case "TE1": 
                        case "TE2": 
                            searchStr = "FAP_TE";
                            substitutionString.v = "SKILL,name ,face    , JN,RS,RP,MS,HP,BB,AG,BC,RC";
                            break;
                        case "RE": 
                        case "NT": 
                        case "LE": 
                        case "RE2": 
                        case "NT2": 
                        case "LE2": 
                            searchStr = "FAP_DL";
                            substitutionString.v = "DL,name      ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU";
                            break;
                        case "ROLB": 
                        case "RILB": 
                        case "LILB": 
                        case "LOLB": 
                        case "LB5": 
                            searchStr = "FAP_LB";
                            substitutionString.v = "DEF,name      ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU";
                            break;
                        case "RCB": 
                        case "LCB": 
                        case "DB1": 
                        case "DB2": 
                            searchStr = "FAP_CB";
                            substitutionString.v = "DEF,name      ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU";
                            break;
                        case "FS": 
                        case "SS": 
                        case "DB3": 
                            searchStr = "FAP_S";
                            substitutionString.v = "DEF,name      ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU";
                            break;
                        case "K": 
                            searchStr = "FAP_K";
                            substitutionString.v = "K,name         ,face    , JN,RS,RP,MS,HP,BB,AG,KP,KA,AB";
                            break;
                        case "P": 
                            searchStr = "FAP_P";
                            substitutionString.v = "P,name         ,face    , JN,RS,RP,MS,HP,BB,AG,KP,AB";
                            break;
                    }
                    var pattern = System.String.format("{0}\\s*:\\s*(.*)$", [searchStr]);
                    var reg = new System.Text.RegularExpressions.Regex.ctor(pattern, 2);
                    var m = reg.match(TSBTool2.TSB3Converter.FormulaString);
                    if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        retVal = m.getGroups().get(1).toString();
                    }
                    return retVal;
                },
                GetAG: function (pos, ms, hp) {
                    var retval = "31";
                    var maxSpeed = System.Int32.parse(ms);
                    var msIndex = TSBTool.StaticUtils.GetTSBAbility(maxSpeed);
                    var hpIndex = TSBTool.StaticUtils.GetTSBAbility(System.Int32.parse(hp));

                    switch (pos) {
                        case "QB1": 
                        case "QB2": 
                        case "RB1": 
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                        case "WR1": 
                        case "WR2": 
                        case "WR3": 
                        case "WR4": 
                        case "RE": 
                        case "NT": 
                        case "LE": 
                        case "RE2": 
                        case "LE2": 
                        case "NT2": 
                        case "SS": 
                        case "FS": 
                        case "DB3": 
                            if (msIndex < 14) {
                                retval = Bridge.toString(TSBTool.StaticUtils.MapAbilityToTSBValue(((msIndex + 1) | 0)));
                            } else {
                                retval = Bridge.toString(TSBTool.StaticUtils.MapAbilityToTSBValue(msIndex));
                            }
                            break;
                        case "TE1": 
                        case "TE2": 
                            if (maxSpeed < 38) {
                                retval = "19";
                            }
                            break;
                        case "C": 
                        case "RG": 
                        case "LG": 
                        case "RT": 
                        case "LT": 
                            if (msIndex < 14) {
                                retval = Bridge.toString(TSBTool.StaticUtils.MapAbilityToTSBValue(((hpIndex + 2) | 0)));
                            } else {
                                retval = Bridge.toString(TSBTool.StaticUtils.MapAbilityToTSBValue(hpIndex));
                            }
                            break;
                        case "ROLB": 
                        case "RILB": 
                        case "LILB": 
                        case "LOLB": 
                        case "LB5": 
                            retval = ms;
                            break;
                        case "LCB": 
                        case "RCB": 
                        case "DB1": 
                        case "DB2": 
                            if (msIndex < 14) {
                                retval = Bridge.toString(TSBTool.StaticUtils.MapAbilityToTSBValue(((msIndex + 2) | 0)));
                            } else {
                                retval = Bridge.toString(TSBTool.StaticUtils.MapAbilityToTSBValue(msIndex));
                            }
                            break;
                        case "K": 
                        case "P": 
                            retval = "38";
                            break;
                    }
                    return retval;
                }
            }
        }
    });

    Bridge.define("TSBTool2.TSBPlayer", {
        $kind: "enum",
        statics: {
            fields: {
                QB1: 0,
                QB2: 1,
                RB1: 2,
                RB2: 3,
                RB3: 4,
                RB4: 5,
                WR1: 6,
                WR2: 7,
                WR3: 8,
                WR4: 9,
                TE1: 10,
                TE2: 11,
                C: 12,
                LG: 13,
                RG: 14,
                LT: 15,
                RT: 16,
                RE: 17,
                NT: 18,
                LE: 19,
                RE2: 20,
                NT2: 21,
                LE2: 22,
                ROLB: 23,
                RILB: 24,
                LILB: 25,
                LOLB: 26,
                LB5: 27,
                RCB: 28,
                LCB: 29,
                DB1: 30,
                DB2: 31,
                FS: 32,
                SS: 33,
                DB3: 34,
                K: 35,
                P: 36
            }
        }
    });

    Bridge.define("TSBTool2.TSBXSimAutoUpdater", {
        statics: {
            fields: {
                sSimFormulas: null,
                sSubLines: null,
                sSubLinesTSB3: null,
                sSubLinesTSB2: null,
                sSubLinesTSB1: null
            },
            props: {
                SimFormulas: {
                    get: function () {
                        if (TSBTool2.TSBXSimAutoUpdater.sSimFormulas == null) {
                            TSBTool2.TSBXSimAutoUpdater.sSimFormulas = " SIM_Formulas.txt\r\nQB_SIM_CARY: IIF(MS > 43,10, IIF(MS > 37,8, IIF(MS > 30, 6, IIF(MS > 24, 4,IIF(MS > 18, 2, 0)))))\r\nQB_SIM_RUSHING: MS /5\r\nQB_SIM_PASSING: (PS -38 + PC -31 + AR -38) / 6\r\nQB_SIM_SCRAMBLE: MS /16\r\n\r\nRB_SIM_RUSHING: ((MS - 31)*2 + (HP-31)*2 + (RP-3)*2)+10\r\nRB_SIM_CARRIES: (((MS - 31)*2 + (HP-31)*2 + (RP-3)*2)+10) /10\r\nRB_SIM_RETURN: (MS+HP)/10 -3\r\nRB_SIM_YPC: 4\r\nRB_SIM_CATCH: RC / 5 -1\r\n\r\nWR_SIM_RUSHING: 1\r\nWR_SIM_CARRIES: 2\r\nWR_SIM_RETURN: (MS+HP)/10 -3\r\nWR_SIM_YPC: (MS + RC) / 10\r\nWR_SIM_CATCH:  RC / 5 -1\r\n\r\nTE_SIM_RUSHING: 0\r\nTE_SIM_CARRIES: 2\r\nTE_SIM_RETURN: (MS+HP)/10 -3\r\nTE_SIM_YPC:( MS + RC) / 10\r\nTE_SIM_CATCH:  RC / 5 -1\r\n\r\nDL_SIM_SACKING: (QU-44 + HP-50) * 1.3\r\nDL_SIM_INT: 0\r\nDL_SIM_TACKLING: 1+ (HP /10) \r\n\r\nLB_SIM_SACKING: (HP - 38)*1.3\r\nLB_SIM_INT: (PI - 6) * 0.45\r\nLB_SIM_TACKLING: (HP /7) \r\n\r\nCB_SIM_SACKING: (HP-44)*2\r\nCB_SIM_INT: PI * 0.66\r\nCB_SIM_TACKLING: RP/10\r\n\r\nS_SIM_SACKING: (HP-44)*2\r\nS_SIM_INT: PI * 0.66\r\nS_SIM_TACKLING: QU /10\r\n\r\n\r\nK_SIM_ABILITY: KA/ 7\r\nP_SIM_ABILITY: KP / 7\r\n";
                        }
                        return TSBTool2.TSBXSimAutoUpdater.sSimFormulas;
                    }
                }
            },
            ctors: {
                init: function () {
                    this.sSubLinesTSB3 = function (_o1) {
                            _o1.add("QB", "QB,name       ,face    , JN,RS,RP,MS,HP,BB,AG,PS,PC,PA,AR,CO");
                            _o1.add("SKILL", "SKILL,name ,face    , JN,RS,RP,MS,HP,BB,AG,BC,RC");
                            _o1.add("OL", "OL,name      ,face    , JN,RS,RP,MS,HP,BB,AG");
                            _o1.add("DEF", "DEF,name     ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU");
                            _o1.add("K", "K,name         ,face    , JN,RS,RP,MS,HP,BB,AG,KP,KA,AB");
                            _o1.add("P", "P,name         ,face    , JN,RS,RP,MS,HP,BB,AG,KP,AB");
                            return _o1;
                        }(new (System.Collections.Generic.Dictionary$2(System.String,System.String)).ctor());
                    this.sSubLinesTSB2 = function (_o2) {
                            _o2.add("QB", "QB,name       ,face    , JN,RS,RP,MS,HP,BB,PS,PC,PA,AR,CO");
                            _o2.add("SKILL", "SKILL,name ,face    , JN,RS,RP,MS,HP,BB,BC,RC");
                            _o2.add("OL", "OL,name      ,face    , JN,RS,RP,MS,HP,BB");
                            _o2.add("DEF", "DEF,name     ,face    , JN,RS,RP,MS,HP,BB,PI,QU");
                            _o2.add("K", "K,name         ,face    , JN,RS,RP,MS,HP,BB,KP,KA,AB");
                            _o2.add("P", "P,name         ,face    , JN,RS,RP,MS,HP,BB,KP,AB");
                            return _o2;
                        }(new (System.Collections.Generic.Dictionary$2(System.String,System.String)).ctor());
                    this.sSubLinesTSB1 = function (_o3) {
                            _o3.add("QB", "QB,name       ,face    , JN,RS,RP,MS,HP,PS,PC,PA,AR");
                            _o3.add("SKILL", "SKILL,name ,face    , JN,RS,RP,MS,HP,BC,RC");
                            _o3.add("OL", "OL,name      ,face    , JN,RS,RP,MS,HP");
                            _o3.add("DEF", "DEF,name     ,face    , JN,RS,RP,MS,HP,PI,QU");
                            _o3.add("K", "K,name         ,face    , JN,RS,RP,MS,HP,KP,KA,AB");
                            _o3.add("P", "P,name         ,face    , JN,RS,RP,MS,HP,KP,AB");
                            return _o3;
                        }(new (System.Collections.Generic.Dictionary$2(System.String,System.String)).ctor());
                }
            },
            methods: {
                ReloadFormulas: function () {
                    TSBTool2.TSBXSimAutoUpdater.sSimFormulas = null;
                },
                AutoUpdatePlayerSimData: function (text, gameVersion) {
                    var $t;
                    TSBTool2.TSBXSimAutoUpdater.ReloadFormulas();
                    switch (gameVersion) {
                        case TSBTool.TSBContentType.TSB2: 
                            TSBTool2.TSBXSimAutoUpdater.sSubLines = TSBTool2.TSBXSimAutoUpdater.sSubLinesTSB2;
                            break;
                        case TSBTool.TSBContentType.TSB3: 
                            TSBTool2.TSBXSimAutoUpdater.sSubLines = TSBTool2.TSBXSimAutoUpdater.sSubLinesTSB3;
                            break;
                        default: 
                            throw new System.ArgumentException.$ctor1("TSBXSimAutoUpdater: Incorrect version " + (System.Enum.toString(TSBTool.TSBContentType, gameVersion) || ""));
                    }

                    TSBTool2.TSB3Converter.ReloadFormulas();
                    var builder = new System.Text.StringBuilder("", text.length);
                    var lines = System.String.split(text, System.String.toCharArray(("\n"), 0, ("\n").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    $t = Bridge.getEnumerator(lines);
                    try {
                        while ($t.moveNext()) {
                            var line = $t.Current;
                            if (System.String.startsWith(line, "#")) {
                                builder.append(line);
                            } else {
                                if (TSBTool2.TSBXSimAutoUpdater.IsPlayerLine(line)) {
                                    try {
                                        builder.append(TSBTool2.TSBXSimAutoUpdater.UpdatePlayerSimValues(line, gameVersion));
                                    } catch (ex) {
                                        ex = System.Exception.create(ex);
                                        TSBTool.StaticUtils.ShowError("Error Processing line: " + (line || "") + "\nOperation not applied" + (Bridge.toString(ex) || ""));
                                        return text;
                                    }
                                } else {
                                    builder.append(line);
                                }
                            }

                            builder.append("\n");
                        }
                    } finally {
                        if (Bridge.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    return builder.toString();
                },
                IsPlayerLine: function (line) {
                    if (!Bridge.referenceEquals(TSBTool2.InputParser.posNameFaceRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                        return true;
                    }
                    return false;
                },
                UpdatePlayerSimValues: function (playerLine, gameVersion) {
                    var $t;
                    var simIndex = System.String.indexOf(playerLine, "[");
                    var parts = null;
                    var playerNoSim = null;
                    if (simIndex > -1) {
                        playerNoSim = playerLine.substr(0, ((simIndex - 1) | 0));
                        parts = System.String.split(playerNoSim, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    } else {
                        playerNoSim = playerLine;
                        parts = System.String.split(playerLine, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    }
                    var simVals = new (System.Collections.Generic.List$1(System.Byte)).ctor();
                    var pos = parts[System.Array.index(0, parts)];
                    var sim3, sim4;
                    switch (pos) {
                        case "QB1": 
                        case "QB2": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("QB_SIM_CARY", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("QB"), parts, 0, 28)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("QB_SIM_RUSHING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("QB"), parts, 0, 8)));
                            sim3 = Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("QB_SIM_PASSING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("QB"), parts, 1, 15));
                            sim4 = Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("QB_SIM_SCRAMBLE", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("QB"), parts, 0, 3));
                            simVals.add(((sim3 << ((4 + sim4) | 0)) & 255));
                            break;
                        case "RB1": 
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("RB_SIM_RUSHING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 173)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("RB_SIM_CARRIES", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 11)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("RB_SIM_RETURN", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 255)));
                            sim3 = Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("RB_SIM_YPC", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 15));
                            sim4 = Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("RB_SIM_CATCH", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 15));
                            simVals.add(((sim3 << ((8 + sim4) | 0)) & 255));
                            break;
                        case "WR1": 
                        case "WR2": 
                        case "WR3": 
                        case "WR4": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("WR_SIM_RUSHING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 173)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("WR_SIM_CARRIES", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 11)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("WR_SIM_RETURN", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 255)));
                            sim3 = Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("WR_SIM_YPC", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 15));
                            sim4 = Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("WR_SIM_CATCH", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 15));
                            simVals.add(((sim3 << ((8 + sim4) | 0)) & 255));
                            break;
                        case "TE1": 
                        case "TE2": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("TE_SIM_RUSHING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 173)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("TE_SIM_CARRIES", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 11)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("TE_SIM_RETURN", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 255)));
                            sim3 = Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("TE_SIM_YPC", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 15));
                            sim4 = Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("TE_SIM_CATCH", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("SKILL"), parts, 0, 15));
                            simVals.add(((sim3 << ((8 + sim4) | 0)) & 255));
                            break;
                        case "RE": 
                        case "NT": 
                        case "LE": 
                        case "RE2": 
                        case "NT2": 
                        case "LE2": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("DL_SIM_SACKING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 255)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("DL_SIM_INT", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 255)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("DL_SIM_TACKLING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 10)));
                            break;
                        case "ROLB": 
                        case "RILB": 
                        case "LILB": 
                        case "LOLB": 
                        case "LB5": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("LB_SIM_SACKING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 255)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("LB_SIM_INT", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 255)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("LB_SIM_TACKLING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 10)));
                            break;
                        case "RCB": 
                        case "LCB": 
                        case "DB1": 
                        case "DB2": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("CB_SIM_SACKING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 255)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("CB_SIM_INT", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 255)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("CB_SIM_TACKLING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 10)));
                            break;
                        case "FS": 
                        case "SS": 
                        case "DB3": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("S_SIM_SACKING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 255)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("S_SIM_INT", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 255)));
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("S_SIM_TACKLING", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("DEF"), parts, 0, 10)));
                            break;
                        case "K": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("K_SIM_ABILITY", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("K"), parts, 0, 15)));
                            break;
                        case "P": 
                            simVals.add(Bridge.Int.clipu8(TSBTool2.TSBXSimAutoUpdater.Calculate("P_SIM_ABILITY", TSBTool2.TSBXSimAutoUpdater.sSubLines.getItem("P"), parts, 0, 15)));
                            break;
                    }
                    var sb = new System.Text.StringBuilder("", 30);
                    sb.append(playerNoSim);
                    if (simVals.Count > 0) {
                        sb.append(",[");
                        $t = Bridge.getEnumerator(simVals);
                        try {
                            while ($t.moveNext()) {
                                var b = $t.Current;
                                sb.append(System.Byte.format(b, "X2"));
                                sb.append(",");
                            }
                        } finally {
                            if (Bridge.is($t, System.IDisposable)) {
                                $t.System$IDisposable$Dispose();
                            }
                        }
                        sb.remove(((sb.getLength() - 1) | 0), 1);
                        sb.append("]");
                    }
                    var retVal = sb.toString();
                    if (gameVersion === TSBTool.TSBContentType.TSB3) {
                        retVal = TSBTool2.TSB3Converter.UpdateFreeAgentValue(retVal);
                    }
                    return retVal;
                },
                Calculate: function (formulaName, substString, playerParts, min, max) {
                    var result = 0;
                    var formula = TSBTool2.TSBXSimAutoUpdater.GetFormula(formulaName);
                    var sub_parts = System.String.split(substString, System.String.toCharArray((","), 0, (",").length).map(function (i) {{ return String.fromCharCode(i); }}));
                    for (var i = 4; i < sub_parts.length; i = (i + 1) | 0) {
                        formula = System.String.replaceAll(formula, sub_parts[System.Array.index(i, sub_parts)].trim(), playerParts[System.Array.index(i, playerParts)]);
                    }
                    try {
                        var r = Bridge.toString(TSBTool.StaticUtils.Compute(formula));
                        result = System.Double.parse(r);
                    } catch ($e1) {
                        $e1 = System.Exception.create($e1);
                        throw new System.ArgumentException.$ctor1(System.String.format("Error processing formula '{0}'\nTrying to calculate:'{1}'", formulaName, formula));
                    }
                    if (result < min) {
                        result = min;
                    } else {
                        if (result > max) {
                            result = max;
                        }
                    }
                    return result;
                },
                GetFormula: function (formulaName) {
                    var retVal = "";
                    var formulaRegex = new System.Text.RegularExpressions.Regex.ctor(System.String.format("^\\s*{0}\\s*:\\s*(.*)$", [formulaName]), 2);
                    var m = formulaRegex.match(TSBTool2.TSBXSimAutoUpdater.SimFormulas);
                    if (!Bridge.referenceEquals(m, System.Text.RegularExpressions.Match.getEmpty())) {
                        retVal = m.getGroups().get(1).toString().trim();
                    }
                    return retVal;
                }
            }
        }
    });

    Bridge.define("TSBTool2_UI.IAllStarPlayerControl", {
        $kind: "interface"
    });

    /**
     * Summary description for CXRomScheduleHelper.
     *
     * @public
     * @class TSBTool.CXRomScheduleHelper
     * @augments TSBTool.ScheduleHelper2
     */
    Bridge.define("TSBTool.CXRomScheduleHelper", {
        inherits: [TSBTool.ScheduleHelper2],
        ctors: {
            ctor: function (outputRom) {
                this.$initialize();
                TSBTool.ScheduleHelper2.ctor.call(this, outputRom);
                this.end_schedule_section = 213006;
                this.weekPointersStartLoc = 207271;
                this.total_games_possible = 256;
                this.gamePerWeekLimit = 16;
                this.totalGameLimit = 256;
            }
        },
        methods: {
            AddMessage: function (message) {
                if (System.String.indexOf(message, "AFC") === -1 && System.String.indexOf(message, "NFC") === -1) {
                    TSBTool.ScheduleHelper2.prototype.AddMessage.call(this, message);
                }
            },
            ScheduleGame: function (awayTeam, homeTeam) {
                var ret = false;
                if (this.TotalGameCount < this.total_games_possible) {
                    ret = TSBTool.ScheduleHelper2.prototype.ScheduleGame.call(this, awayTeam, homeTeam);
                } else {
                    this.AddMessage(System.String.format("ERROR! maximum game limit reached ({0}) {1} at {1} will not be scheduled", Bridge.box(this.total_games_possible, System.Int32), awayTeam, homeTeam));
                }
                return ret;
            }
        }
    });

    /**
     * Summary description for TecmoTool.
     Location = pointer - 0x8000 + 0x0010;
     Where pointer is of the 'swapped' format like '0x86dd'
     *
     * @public
     * @class TSBTool.TecmoTool
     * @implements  TSBTool.ITecmoTool
     * @implements  TSBTool.ITecmoContent
     */
    Bridge.define("TSBTool.TecmoTool", {
        inherits: [TSBTool.ITecmoTool,TSBTool.ITecmoContent],
        statics: {
            fields: {
                m2RB_2WR_1TE: null,
                m1RB_3WR_1TE: null,
                m1RB_4WR: null,
                positionNames: null,
                teams: null,
                ShowTeamFormation: false,
                ShowPlaybook: false,
                ShowColors: false,
                ShowTeamStrings: false,
                ShowProBowlRosters: false
            },
            props: {
                PositionNames: {
                    get: function () {
                        return TSBTool.TecmoTool.positionNames;
                    },
                    set: function (value) {
                        TSBTool.TecmoTool.positionNames = value;
                    }
                },
                Teams: {
                    get: function () {
                        return TSBTool.TecmoTool.teams;
                    },
                    set: function (value) {
                        TSBTool.TecmoTool.teams = value;
                    }
                }
            },
            ctors: {
                init: function () {
                    this.m2RB_2WR_1TE = "2RB_2WR_1TE";
                    this.m1RB_3WR_1TE = "1RB_3WR_1TE";
                    this.m1RB_4WR = "1RB_4WR";
                    this.positionNames = function (_o1) {
                            _o1.add("QB1");
                            _o1.add("QB2");
                            _o1.add("RB1");
                            _o1.add("RB2");
                            _o1.add("RB3");
                            _o1.add("RB4");
                            _o1.add("WR1");
                            _o1.add("WR2");
                            _o1.add("WR3");
                            _o1.add("WR4");
                            _o1.add("TE1");
                            _o1.add("TE2");
                            _o1.add("C");
                            _o1.add("LG");
                            _o1.add("RG");
                            _o1.add("LT");
                            _o1.add("RT");
                            _o1.add("RE");
                            _o1.add("NT");
                            _o1.add("LE");
                            _o1.add("ROLB");
                            _o1.add("RILB");
                            _o1.add("LILB");
                            _o1.add("LOLB");
                            _o1.add("RCB");
                            _o1.add("LCB");
                            _o1.add("FS");
                            _o1.add("SS");
                            _o1.add("K");
                            _o1.add("P");
                            return _o1;
                        }(new (System.Collections.Generic.List$1(System.String)).ctor());
                    this.teams = function (_o2) {
                            _o2.add("bills");
                            _o2.add("colts");
                            _o2.add("dolphins");
                            _o2.add("patriots");
                            _o2.add("jets");
                            _o2.add("bengals");
                            _o2.add("browns");
                            _o2.add("oilers");
                            _o2.add("steelers");
                            _o2.add("broncos");
                            _o2.add("chiefs");
                            _o2.add("raiders");
                            _o2.add("chargers");
                            _o2.add("seahawks");
                            _o2.add("redskins");
                            _o2.add("giants");
                            _o2.add("eagles");
                            _o2.add("cardinals");
                            _o2.add("cowboys");
                            _o2.add("bears");
                            _o2.add("lions");
                            _o2.add("packers");
                            _o2.add("vikings");
                            _o2.add("buccaneers");
                            _o2.add("49ers");
                            _o2.add("rams");
                            _o2.add("saints");
                            _o2.add("falcons");
                            return _o2;
                        }(new (System.Collections.Generic.List$1(System.String)).ctor());
                    this.ShowTeamFormation = false;
                    this.ShowPlaybook = false;
                    this.ShowColors = false;
                    this.ShowTeamStrings = false;
                    this.ShowProBowlRosters = false;
                }
            },
            methods: {
                GetTeamIndex: function (teamName) {
                    var ret = -1;
                    if (Bridge.referenceEquals(teamName.toLowerCase(), "null")) {
                        return 255;
                    }
                    for (var i = 0; i < TSBTool.TecmoTool.teams.Count; i = (i + 1) | 0) {
                        if (Bridge.referenceEquals(TSBTool.TecmoTool.teams.getItem(i), teamName)) {
                            ret = i;
                            break;
                        }
                    }
                    return ret;
                },
                /**
                 * Returns the team specified by the index passed. (0= bills).
                 *
                 * @static
                 * @public
                 * @this TSBTool.TecmoTool
                 * @memberof TSBTool.TecmoTool
                 * @param   {number}    index
                 * @return  {string}             team name on success, null on failure
                 */
                GetTeamFromIndex: function (index) {
                    if (index === 255) {
                        return "null";
                    }
                    if (index < 0 || index > ((TSBTool.TecmoTool.teams.Count - 1) | 0)) {
                        return null;
                    }
                    return TSBTool.TecmoTool.teams.getItem(index);
                }
            }
        },
        fields: {
            outputRom: null,
            namePointersStart: 0,
            lastPlayerNamePointer: 0,
            teamSimOffensivePrefStart: 0,
            mBillsPuntKickReturnerPos: 0,
            dataPositionOffset: 0,
            mNameRegex: null,
            mShowOffPref: false,
            maxNameLength: 0,
            gameYearLocations: null,
            billsQB1SimLoc: 0,
            billsRESimLoc: 0,
            billsTeamSimLoc: 0,
            teamSimOffset: 0,
            billsQB1AbilityStart: 0,
            teamAbilityOffset: 0,
            abilityOffsets: null,
            faceOffsets: null,
            faceTeamOffsets: null,
            simpleSetRegex: null,
            mTeamFormationHackLoc: 0,
            mTeamFormationsStartingLoc: 0,
            mTeamFormationsStartingLoc2: 0,
            mPlaybookStartLoc: 0,
            runRegex: null,
            passRegex: null,
            JUICE_LOCATION: 0,
            m_JuiceArray: null,
            mBillsUniformLoc: 0,
            mBillsActionSeqLoc: 0,
            mBillsDivChampLoc: 0,
            mBillsConfChampLoc: 0,
            
            mProwbowlStartingLoc: 0
        },
        props: {
            /**
             * Returns the rom version
             *
             * @instance
             * @public
             * @readonly
             * @memberof TSBTool.TecmoTool
             * @function RomVersion
             * @type TSBTool.ROM_TYPE
             */
            RomVersion: {
                get: function () {
                    return TSBTool.ROM_TYPE.NES_ORIGINAL_TSB;
                }
            },
            OutputRom: {
                get: function () {
                    return this.outputRom;
                },
                set: function (value) {
                    this.outputRom = value;
                }
            },
            ShowOffPref: {
                get: function () {
                    return this.mShowOffPref;
                },
                set: function (value) {
                    this.mShowOffPref = value;
                }
            },
            NameRegex: {
                get: function () {
                    if (this.mNameRegex == null) {
                        this.mNameRegex = new System.Text.RegularExpressions.Regex.ctor("[a-zA-Z \\.]+");
                    }
                    return this.mNameRegex;
                }
            },
            NumberOfStringsInTeamStringTable: {
                get: function () {
                    return 119;
                }
            },
            BillsUniformLoc: {
                get: function () {
                    return this.mBillsUniformLoc;
                },
                set: function (value) {
                    this.mBillsUniformLoc = value;
                }
            },
            BillsActionSeqLoc: {
                get: function () {
                    return this.mBillsActionSeqLoc;
                },
                set: function (value) {
                    this.mBillsActionSeqLoc = value;
                }
            },
            BillsDivChampLoc: {
                get: function () {
                    return this.mBillsDivChampLoc;
                },
                set: function (value) {
                    this.mBillsDivChampLoc = value;
                }
            },
            BillsConfChampLoc: {
                get: function () {
                    return this.mBillsConfChampLoc;
                },
                set: function (value) {
                    this.mBillsConfChampLoc = value;
                }
            }
        },
        alias: [
            "RomVersion", "TSBTool$ITecmoContent$RomVersion",
            "RomVersion", "TSBTool$ITecmoTool$RomVersion",
            "OutputRom", "TSBTool$ITecmoContent$OutputRom",
            "OutputRom", "TSBTool$ITecmoTool$OutputRom",
            "ShowOffPref", "TSBTool$ITecmoContent$ShowOffPref",
            "ShowOffPref", "TSBTool$ITecmoTool$ShowOffPref",
            "SetByte", "TSBTool$ITecmoContent$SetByte",
            "IsValidPosition", "TSBTool$ITecmoTool$IsValidPosition",
            "SaveRom", "TSBTool$ITecmoContent$SaveRom",
            "SaveRom", "TSBTool$ITecmoTool$SaveRom",
            "GetPlayerStuff", "TSBTool$ITecmoTool$GetPlayerStuff",
            "GetSchedule", "TSBTool$ITecmoTool$GetSchedule",
            "SetYear", "TSBTool$ITecmoTool$SetYear",
            "InsertPlayer", "TSBTool$ITecmoTool$InsertPlayer",
            "GetKey", "TSBTool$ITecmoContent$GetKey",
            "GetKey", "TSBTool$ITecmoTool$GetKey",
            "GetTeamPlayers", "TSBTool$ITecmoTool$GetTeamPlayers",
            "GetTeamName", "TSBTool$ITecmoTool$GetTeamName",
            "GetTeamCity", "TSBTool$ITecmoTool$GetTeamCity",
            "GetTeamAbbreviation", "TSBTool$ITecmoTool$GetTeamAbbreviation",
            "GetTeamStringTableString", "TSBTool$ITecmoTool$GetTeamStringTableString",
            "SetTeamStringTableString", "TSBTool$ITecmoTool$SetTeamStringTableString",
            "NumberOfStringsInTeamStringTable", "TSBTool$ITecmoTool$NumberOfStringsInTeamStringTable",
            "SetTeamAbbreviation", "TSBTool$ITecmoTool$SetTeamAbbreviation",
            "SetTeamName", "TSBTool$ITecmoTool$SetTeamName",
            "SetTeamCity", "TSBTool$ITecmoTool$SetTeamCity",
            "GetAll$1", "TSBTool$ITecmoContent$GetAll",
            "GetProBowlPlayers$1", "TSBTool$ITecmoContent$GetProBowlPlayers",
            "GetSchedule$1", "TSBTool$ITecmoContent$GetSchedule",
            "GetAll", "TSBTool$ITecmoTool$GetAll",
            "SetQBAbilities", "TSBTool$ITecmoTool$SetQBAbilities",
            "SetSkillPlayerAbilities", "TSBTool$ITecmoTool$SetSkillPlayerAbilities",
            "SetKickPlayerAbilities", "TSBTool$ITecmoTool$SetKickPlayerAbilities",
            "SetDefensivePlayerAbilities", "TSBTool$ITecmoTool$SetDefensivePlayerAbilities",
            "SetOLPlayerAbilities", "TSBTool$ITecmoTool$SetOLPlayerAbilities",
            "SetTeamSimData", "TSBTool$ITecmoTool$SetTeamSimData",
            "SetTeamSimOffensePref", "TSBTool$ITecmoTool$SetTeamSimOffensePref",
            "SetKickingSimData", "TSBTool$ITecmoTool$SetKickingSimData",
            "SetPuntingSimData", "TSBTool$ITecmoTool$SetPuntingSimData",
            "SetDefensiveSimData", "TSBTool$ITecmoTool$SetDefensiveSimData",
            "SetSkillSimData", "TSBTool$ITecmoTool$SetSkillSimData",
            "SetQBSimData", "TSBTool$ITecmoTool$SetQBSimData",
            "SetFace", "TSBTool$ITecmoTool$SetFace",
            "SetPuntReturner", "TSBTool$ITecmoTool$SetPuntReturner",
            "SetKickReturner", "TSBTool$ITecmoTool$SetKickReturner",
            "ApplySet", "TSBTool$ITecmoContent$ApplySet",
            "ApplySet", "TSBTool$ITecmoTool$ApplySet",
            "SetTeamOffensiveFormation", "TSBTool$ITecmoTool$SetTeamOffensiveFormation",
            "SetPlaybook", "TSBTool$ITecmoTool$SetPlaybook",
            "ApplyJuice", "TSBTool$ITecmoTool$ApplyJuice",
            "ApplySchedule", "TSBTool$ITecmoTool$ApplySchedule",
            "SetHomeUniform", "TSBTool$ITecmoTool$SetHomeUniform",
            "SetAwayUniform", "TSBTool$ITecmoTool$SetAwayUniform",
            "GetGameUniform", "TSBTool$ITecmoTool$GetGameUniform",
            "SetDivChampColors", "TSBTool$ITecmoTool$SetDivChampColors",
            "GetDivChampColors", "TSBTool$ITecmoTool$GetDivChampColors",
            "SetConfChampColors", "TSBTool$ITecmoTool$SetConfChampColors",
            "GetChampColors", "TSBTool$ITecmoTool$GetChampColors",
            "GetConfChampColors", "TSBTool$ITecmoTool$GetConfChampColors",
            "GetUniformUsage", "TSBTool$ITecmoTool$GetUniformUsage",
            "SetUniformUsage", "TSBTool$ITecmoTool$SetUniformUsage",
            "SetReturnTeam", "TSBTool$ITecmoTool$SetReturnTeam",
            "SetProBowlPlayer", "TSBTool$ITecmoTool$SetProBowlPlayer",
            "GetProBowlPlayers", "TSBTool$ITecmoTool$GetProBowlPlayers",
            "ProcessText", "TSBTool$ITecmoContent$ProcessText",
            "ProcessText", "TSBTool$ITecmoTool$ProcessText"
        ],
        ctors: {
            init: function () {
                this.namePointersStart = 72;
                this.lastPlayerNamePointer = 1752;
                this.teamSimOffensivePrefStart = 161062;
                this.mBillsPuntKickReturnerPos = 207059;
                this.dataPositionOffset = -32752;
                this.mShowOffPref = false;
                this.maxNameLength = 16;
                this.gameYearLocations = System.Array.init([
                    50404, 
                    123176, 
                    123530, 
                    123581, 
                    129179, 
                    49449
                ], System.Int32);
                this.billsQB1SimLoc = 98659;
                this.billsRESimLoc = 98683;
                this.billsTeamSimLoc = 98706;
                this.teamSimOffset = 48;
                this.billsQB1AbilityStart = 12304;
                this.teamAbilityOffset = 117;
                this.abilityOffsets = System.Array.init([
                    0, 
                    5, 
                    10, 
                    14, 
                    18, 
                    22, 
                    26, 
                    30, 
                    34, 
                    38, 
                    42, 
                    46, 
                    50, 
                    53, 
                    56, 
                    59, 
                    62, 
                    65, 
                    69, 
                    73, 
                    77, 
                    81, 
                    85, 
                    89, 
                    93, 
                    97, 
                    101, 
                    105, 
                    109, 
                    113
                ], System.Int32);
                this.faceOffsets = System.Array.init([
                    0, 
                    5, 
                    10, 
                    14, 
                    18, 
                    22, 
                    26, 
                    30, 
                    34, 
                    38, 
                    42, 
                    46, 
                    50, 
                    53, 
                    56, 
                    59, 
                    62, 
                    65, 
                    69, 
                    73, 
                    77, 
                    81, 
                    85, 
                    89, 
                    93, 
                    97, 
                    101, 
                    105, 
                    109, 
                    113
                ], System.Int32);
                this.faceTeamOffsets = System.Array.init([
                    12306, 
                    12423, 
                    12540, 
                    12657, 
                    12774, 
                    12891, 
                    13008, 
                    13125, 
                    13242, 
                    13359, 
                    13476, 
                    13593, 
                    13710, 
                    13827, 
                    14412, 
                    14061, 
                    14178, 
                    14295, 
                    13944, 
                    14529, 
                    14646, 
                    14763, 
                    14880, 
                    14997, 
                    15114, 
                    15231, 
                    15348, 
                    15465
                ], System.Int32);
                this.mTeamFormationHackLoc = 136770;
                this.mTeamFormationsStartingLoc = 139232;
                this.mTeamFormationsStartingLoc2 = 204416;
                this.mPlaybookStartLoc = 119568;
                this.JUICE_LOCATION = 122640;
                this.m_JuiceArray = System.Array.init([
                    0, 
                    1, 
                    0, 
                    0, 
                    0, 
                    1, 
                    2, 
                    1, 
                    1, 
                    1, 
                    1, 
                    2, 
                    1, 
                    2, 
                    2, 
                    1, 
                    2, 
                    1, 
                    3, 
                    2, 
                    2, 
                    2, 
                    2, 
                    3, 
                    3, 
                    2, 
                    2, 
                    2, 
                    4, 
                    3, 
                    2, 
                    2, 
                    2, 
                    4, 
                    4, 
                    2, 
                    2, 
                    2, 
                    5, 
                    4, 
                    2, 
                    2, 
                    3, 
                    5, 
                    5, 
                    2, 
                    2, 
                    3, 
                    6, 
                    5, 
                    2, 
                    2, 
                    4, 
                    6, 
                    6, 
                    3, 
                    2, 
                    4, 
                    7, 
                    6, 
                    3, 
                    3, 
                    4, 
                    7, 
                    7, 
                    3, 
                    3, 
                    5, 
                    8, 
                    7, 
                    3, 
                    3, 
                    5, 
                    8, 
                    8, 
                    3, 
                    3, 
                    5, 
                    9, 
                    8, 
                    3, 
                    4, 
                    6, 
                    9, 
                    9
                ], System.Byte);
                this.mBillsUniformLoc = 180964;
                this.mBillsActionSeqLoc = 213720;
                this.mBillsDivChampLoc = 213992;
                this.mBillsConfChampLoc = 214164;
                this.mProwbowlStartingLoc = 206931;
            },
            ctor: function () {
                this.$initialize();
            },
            $ctor1: function (rom) {
                this.$initialize();
                this.Init(rom);
            }
        },
        methods: {
            SetByte: function (location, b) {
                var $t;
                ($t = this.OutputRom)[System.Array.index(location, $t)] = b;
            },
            /**
             * Will ensure that the headder is correct.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @return  {void}
             */
            FixHeadder: function () {
                if (this.outputRom == null) {
                    return;
                }

                var correctHeadder = System.Array.init([
                    78, 
                    69, 
                    83, 
                    26, 
                    16, 
                    16, 
                    66, 
                    0, 
                    0, 
                    0, 
                    0, 
                    0, 
                    0, 
                    0, 
                    0, 
                    0
                ], System.Byte);

                for (var i = 0; i < correctHeadder.length; i = (i + 1) | 0) {
                    this.outputRom[System.Array.index(i, this.outputRom)] = correctHeadder[System.Array.index(i, correctHeadder)];
                }
            },
            IsValidPosition: function (pos) {
                var ret = false;
                for (var i = 0; i < TSBTool.TecmoTool.positionNames.Count; i = (i + 1) | 0) {
                    if (Bridge.referenceEquals(pos, TSBTool.TecmoTool.positionNames.getItem(i))) {
                        ret = true;
                        break;
                    }
                }
                return ret;
            },
            IsValidTeam: function (team) {
                var ret = false;
                for (var i = 0; i < TSBTool.TecmoTool.teams.Count; i = (i + 1) | 0) {
                    if (Bridge.referenceEquals(team, TSBTool.TecmoTool.teams.getItem(i))) {
                        ret = true;
                        break;
                    }
                }
                return ret;
            },
            Init: function (rom) {
                return this.InitRom(rom);
            },
            Test2: function () {
                var team = "bills";
                for (var i = 0; i < TSBTool.TecmoTool.positionNames.Count; i = (i + 1) | 0) {
                    this.InsertPlayer(team, TSBTool.TecmoTool.positionNames.getItem(i), "player", team, ((i % 10) & 255));
                    switch (TSBTool.TecmoTool.positionNames.getItem(i)) {
                        case "QB1": 
                        case "QB2": 
                            this.SetQBAbilities(team, TSBTool.TecmoTool.positionNames.getItem(i), 31, 31, 31, 31, 31, 31, 31, 31);
                            break;
                        case "RB1": 
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                        case "WR1": 
                        case "WR2": 
                        case "WR3": 
                        case "WR4": 
                        case "TE1": 
                        case "TE2": 
                            this.SetSkillPlayerAbilities(team, TSBTool.TecmoTool.positionNames.getItem(i), 31, 31, 31, 31, 31, 31);
                            break;
                        case "C": 
                        case "RG": 
                        case "LG": 
                        case "RT": 
                        case "LT": 
                            this.SetOLPlayerAbilities(team, TSBTool.TecmoTool.positionNames.getItem(i), 31, 31, 31, 31);
                            break;
                        case "RE": 
                        case "NT": 
                        case "LE": 
                        case "LOLB": 
                        case "LILB": 
                        case "RILB": 
                        case "ROLB": 
                        case "RCB": 
                        case "LCB": 
                        case "FS": 
                        case "SS": 
                            this.SetDefensivePlayerAbilities(team, TSBTool.TecmoTool.positionNames.getItem(i), 31, 31, 31, 31, 31, 31);
                            break;
                        case "K": 
                        case "P": 
                            this.SetKickPlayerAbilities(team, TSBTool.TecmoTool.positionNames.getItem(i), 31, 31, 31, 31, 31, 31);
                            break;
                    }
                }
            },
            shiftTest: function () {
                var stuff = System.Array.init([
                    255, 
                    255, 
                    255, 
                    255, 
                    255, 
                    74, 
                    76, 
                    78, 
                    80, 
                    82, 
                    84, 
                    86, 
                    88, 
                    90, 
                    92, 
                    94, 
                    96, 
                    98, 
                    100, 
                    102, 
                    104, 
                    106, 
                    108, 
                    110, 
                    112, 
                    114, 
                    255, 
                    255, 
                    255, 
                    255, 
                    255
                ], System.Byte);
                for (var i = 0; i < stuff.length; i = (i + 1) | 0) {
                    System.Console.Write(System.String.format(" {0:x} ", Bridge.box(stuff[System.Array.index(i, stuff)], System.Byte)));
                }
                System.Console.WriteLine();
                System.Console.WriteLine("shift 3");
                this.ShiftDataDown(6, ((stuff.length - 7) | 0), 3, stuff);
                for (var i1 = 0; i1 < stuff.length; i1 = (i1 + 1) | 0) {
                    System.Console.Write(System.String.format(" {0:x} ", Bridge.box(stuff[System.Array.index(i1, stuff)], System.Byte)));
                }
                System.Console.WriteLine();

            },
            IsValidRomSize: function (len) {
                var ret = false;
                if (len.equals(System.Int64(TSBTool.TecmoToolFactory.ORIG_NES_TSB1_LEN))) {
                    ret = true;
                }
                return ret;
            },
            InitRom: function (rom) {
                var ret = false;
                try {
                    var result = System.Windows.Forms.DialogResult.Yes;
                    var len = System.Int64(rom.length);
                    if (!this.IsValidRomSize(len)) {
                        if (TSBTool.MainClass.GUI_MODE) {
                            result = System.Windows.Forms.MessageBox.Show(null, System.String.format("Warning! \r\n\r\nThe input Rom is not the correct Size. [{0} size = {1}]\r\n\r\nYou should only continue if you know for sure that you are loading a nes TSB ROM.\r\n\r\nSupported sizes are[{2}, {3}, {4}]\r\n\r\nDo you want to continue?", "<file>", len, Bridge.box(TSBTool.TecmoToolFactory.ORIG_NES_TSB1_LEN, System.Int32), Bridge.box(TSBTool.TecmoToolFactory.CXROM_V105_LEN, System.Int32), Bridge.box(TSBTool.TecmoToolFactory.CXROM_V111_LEN, System.Int32)), "WARNING!", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                        } else {
                            var msg = System.String.format("ERROR! ROM '{0}' is not the correct length.  \r\n    Supported sizes are [{1}, {2}, or {3}] bytes long.\r\n    If you know this is really a nes TSB ROM, you can force TSBToolSupreme to load it in GUI mode.", "<file>", Bridge.box(TSBTool.TecmoToolFactory.ORIG_NES_TSB1_LEN, System.Int32), Bridge.box(TSBTool.TecmoToolFactory.CXROM_V105_LEN, System.Int32), Bridge.box(TSBTool.TecmoToolFactory.CXROM_V111_LEN, System.Int32));
                            TSBTool.StaticUtils.AddError(msg);
                        }
                    }

                    if (result === System.Windows.Forms.DialogResult.Yes) {
                        this.outputRom = rom;
                        ret = true;
                    }
                } catch (e) {
                    e = System.Exception.create(e);
                    TSBTool.StaticUtils.ShowError(Bridge.toString(e));
                }
                return ret;
            },
            SaveRom: function (filename) {
                if (filename != null) {
                    try {
                        var len = System.Int64(this.outputRom.length);
                        var s1 = new System.IO.FileStream.$ctor1(filename, 4);
                        s1.Write(this.outputRom, 0, System.Int64.clip32(len));
                        s1.Close();
                    } catch (e) {
                        e = System.Exception.create(e);
                        TSBTool.StaticUtils.ShowError(Bridge.toString(e));
                    }
                } else {
                    TSBTool.StaticUtils.AddError("ERROR! You passed a null filename");
                }
            },
            /**
             * Returns a string consisting of number, name\n for all players in the game.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {boolean}    jerseyNumber_b    
             * @param   {boolean}    name_b            
             * @param   {boolean}    face_b            
             * @param   {boolean}    abilities_b       
             * @param   {boolean}    simData_b
             * @return  {string}
             */
            GetPlayerStuff: function (jerseyNumber_b, name_b, face_b, abilities_b, simData_b) {
                var sb = new System.Text.StringBuilder("", 40320);
                var team = "";
                for (var i = 0; i < TSBTool.TecmoTool.teams.Count; i = (i + 1) | 0) {
                    team = TSBTool.TecmoTool.teams.getItem(i);
                    sb.append(System.String.format("TEAM={0}\n", [team]));
                    for (var j = 0; j < TSBTool.TecmoTool.positionNames.Count; j = (j + 1) | 0) {
                        sb.append((this.GetPlayerData(team, TSBTool.TecmoTool.positionNames.getItem(j), abilities_b, jerseyNumber_b, face_b, name_b, simData_b) || "") + "\n");
                    }
                }
                return sb.toString();
            },
            GetSchedule: function () {
                var ret = "";
                if (this.outputRom != null) {
                    var sh2 = new TSBTool.ScheduleHelper2(this.outputRom);
                    ret = sh2.GetSchedule();
                    TSBTool.StaticUtils.ShowErrors();
                }
                return ret;
            },
            GetSchedule$1: function (season) {
                return this.GetSchedule();
            },
            SetYear: function (year) {
                if (year == null || year.length !== 4) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) {0} is not a valid year.", [year]));
                    return;
                }
                var location;
                for (var i = 0; i < this.gameYearLocations.length; i = (i + 1) | 0) {
                    location = this.gameYearLocations[System.Array.index(i, this.gameYearLocations)];
                    this.outputRom[System.Array.index(location, this.outputRom)] = (year.charCodeAt(0)) & 255;
                    this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = (year.charCodeAt(1)) & 255;
                    this.outputRom[System.Array.index(((location + 2) | 0), this.outputRom)] = (year.charCodeAt(2)) & 255;
                    this.outputRom[System.Array.index(((location + 3) | 0), this.outputRom)] = (year.charCodeAt(3)) & 255;
                }
            },
            GetYear: function () {
                var location = this.gameYearLocations[System.Array.index(0, this.gameYearLocations)];
                var ret = "";
                for (var i = location; i < ((location + 4) | 0); i = (i + 1) | 0) {
                    ret = (ret || "") + String.fromCharCode(this.outputRom[System.Array.index(i, this.outputRom)]);
                }

                return ret;
            },
            InsertPlayer: function (team, position, fname, lname, number) {
                if (!this.IsValidPosition(position) || fname == null || lname == null || fname.length < 1 || lname.length < 1) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) InsertPlayer:: Player name or position invalid. team:'{0}'; position:'{1}'; fname:'{2}'; lname:'{3}'", team, position, fname, lname));
                } else {
                    if (Bridge.referenceEquals(this.NameRegex.match((fname || "") + (lname || "")), System.Text.RegularExpressions.Match.getEmpty())) {
                        TSBTool.StaticUtils.ShowError(System.String.format("Error on name {0} {1}", fname, lname));
                        return;
                    }

                    fname = fname.toLowerCase();
                    lname = lname.toUpperCase();
                    if (((lname.length + fname.length) | 0) > this.maxNameLength) {
                        TSBTool.StaticUtils.AddError(System.String.format("Warning!! There is a 15 character limit for names\n '{0} {1}' is {2} characters long.", fname, lname, Bridge.box(((fname.length + lname.length) | 0), System.Int32)));
                        if (lname.length > ((this.maxNameLength - 2) | 0)) {
                            lname = lname.substr(0, 12);
                            fname = System.String.format("{0}.", [Bridge.box(fname.charCodeAt(0), System.Char, String.fromCharCode, System.Char.getHashCode)]);
                        } else {
                            fname = System.String.format("{0}.", [Bridge.box(fname.charCodeAt(0), System.Char, String.fromCharCode, System.Char.getHashCode)]);
                        }

                        TSBTool.StaticUtils.AddError(System.String.format("Name will be {0} {1}", fname, lname));
                    }
                    if (fname.length < 1) {
                        fname = "Joe";
                    }
                    if (lname.length < 1) {
                        lname = "Nobody";
                    }

                    var oldName = this.GetName(team, position);
                    var bytes = System.Array.init(((((1 + fname.length) | 0) + lname.length) | 0), 0, System.Byte);
                    var change = (bytes.length - oldName.length) | 0;
                    var i = 0;
                    bytes[System.Array.index(0, bytes)] = number;
                    for (i = 1; i < ((fname.length + 1) | 0); i = (i + 1) | 0) {
                        bytes[System.Array.index(i, bytes)] = (fname.charCodeAt(((i - 1) | 0))) & 255;
                    }
                    for (var j = 0; j < lname.length; j = (j + 1) | 0) {
                        bytes[System.Array.index(Bridge.identity(i, ((i = (i + 1) | 0))), bytes)] = (lname.charCodeAt(j)) & 255;
                    }
                    var pos = this.GetPointerPosition(team, position);

                    this.UpdatePlayerData(team, position, bytes, change);
                    this.AdjustDataPointers(pos, change, this.lastPlayerNamePointer);
                }
            },
            /**
             * Updates strng pointers
             *
             * @instance
             * @protected
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {number}    firstPointerLocation    
             * @param   {number}    change                  the amount of change
             * @param   {number}    lastPointerLocation
             * @return  {void}
             */
            AdjustDataPointers: function (firstPointerLocation, change, lastPointerLocation) {
                var low, hi;
                var word;

                var i = 0;
                var end = (lastPointerLocation + 1) | 0;
                for (i = (firstPointerLocation + 2) | 0; i < end; i = (i + 2) | 0) {
                    low = this.outputRom[System.Array.index(i, this.outputRom)];
                    hi = this.outputRom[System.Array.index(((i + 1) | 0), this.outputRom)];
                    word = hi;
                    word = word << 8;
                    word = (word + low) | 0;
                    word = (word + change) | 0;
                    low = (word & 255) & 255;
                    word = word >> 8;
                    hi = word & 255;
                    this.outputRom[System.Array.index(i, this.outputRom)] = low;
                    this.outputRom[System.Array.index(((i + 1) | 0), this.outputRom)] = hi;
                }
            },
            /**
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team        The team the player is assigned to.
             * @param   {string}    position    The player's position ('QB1', 'WR1' ...)
             * @return  {string}
             */
            GetName: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetName:: team '{0}' or position '{1}' is invalid.", team, position));
                    return null;
                }
                var pos = this.GetDataPosition(team, position);
                if (Bridge.referenceEquals(position, "P") && Bridge.referenceEquals(team, "falcons")) {
                    position = position;
                }
                var nextPos = this.GetNextDataPosition(team, position);
                if (nextPos === -1) {
                    var pointerLocation = this.lastPlayerNamePointer;
                    var lowByte = this.outputRom[System.Array.index(pointerLocation, this.outputRom)];
                    var hiByte = this.outputRom[System.Array.index(((pointerLocation + 1) | 0), this.outputRom)];
                    hiByte = hiByte << 8;
                    hiByte = (hiByte + lowByte) | 0;

                    nextPos = (hiByte + this.dataPositionOffset) | 0;
                }
                var name = "";

                if (pos < 0) {
                    return "ERROR!";
                }
                if (nextPos > 0) {
                    for (var i = (pos + 1) | 0; i < nextPos; i = (i + 1) | 0) {
                        name = (name || "") + String.fromCharCode(this.outputRom[System.Array.index(i, this.outputRom)]);
                    }
                }
                var split = 1;
                for (var i1 = 0; i1 < name.length; i1 = (i1 + 1) | 0) {
                    if (((name.charCodeAt(i1)) & 255) > 64 && ((name.charCodeAt(i1)) & 255) < 91) {
                        split = i1;
                        break;
                    }
                }

                var first, last, full;
                full = null;
                try {
                    first = name.substr(0, split);
                    last = name.substr(split);
                    full = (first || "") + " " + (last || "");
                } catch ($e1) {
                    $e1 = System.Exception.create($e1);
                    return full;
                }
                return full;
            },
            GetPlayerData: function (team, position, ability_b, jerseyNumber_b, face_b, name_b, simData_b) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Team {0} is invalid.", [team]));
                    return null;
                } else if (!this.IsValidPosition(position)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) position {0} is invalid.", [position]));
                    return null;
                }

                var result = new System.Text.StringBuilder();

                result.append(System.String.format("{0}, ", [position]));
                if (name_b) {
                    result.append(System.String.format("{0}, ", [this.GetName(team, position)]));
                }
                if (face_b) {
                    result.append(System.String.format("Face=0x{0:x}, ", [Bridge.box(this.GetFace(team, position), System.Int32)]));
                }
                var location = this.GetDataPosition(team, position);

                if (location < 0) {
                    return "Messed Up Pointer";
                }

                var jerseyNumber = System.String.format("#{0:x}, ", [Bridge.box(this.outputRom[System.Array.index(location, this.outputRom)], System.Byte)]);
                if (jerseyNumber_b) {
                    result.append(jerseyNumber);
                }
                if (ability_b) {
                    result.append(this.GetAbilityString(team, position));
                }
                var simData = this.GetPlayerSimData(team, position);
                if (simData != null && simData_b) {
                    result.append(System.String.format(",[{0}]", [this.StringifyArray(simData)]));
                }
                return result.toString();
            },
            GetKey: function () {
                return System.String.format("# TSBTool Forum: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/\r\n# Editing:  Tecmo Super Bowl (nes) [{0}]\r\n# Key\r\n# TEAM:\r\n#  name, SimData  0x<offense><defense><offense preference>\r\n#  Offensive pref values 0-3. \r\n#     0 = Little more rushing, 1 = Heavy Rushing,\r\n#     2 = little more passing, 3 = Heavy Passing.\r\n# credit to Jstout for figuring out 'offense preference'\r\n# -- Quarterbacks:\r\n# Position, First name Last name, FaceID, Jersey number, RS, RP, MS, HP, PS, PC, PA, APB, [Sim rush, Sim pass, Sim Pocket].\r\n# -- Offensive Skill players (non-QB):\r\n# Position, First name Last name, FaceID, Jersey number, RS, RP, MS, HP, BC, REC, [Sim rush, Sim catch, Sim punt Ret, Sim kick ret].\r\n# -- Offensive Linemen:\r\n# Position, First name Last name, FaceID, Jersey number, RS, RP, MS, HP\r\n# -- Defensive Players:\r\n# Position, First name Last name, FaceID, Jersey number, RS, RP, MS, HP, PI, QU, [Sim pass rush, Sim coverage].\r\n# -- Punters and Kickers:\r\n# Position, First name Last name, FaceID, Jersey number, RS, RP, MS, HP, KA, AKB,[ Sim kicking ability].", [Bridge.box(this.RomVersion, TSBTool.ROM_TYPE, System.Enum.toStringFn(TSBTool.ROM_TYPE))]);
            },
            GetTeamPlayers: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetTeamPlayers:: team {0} is invalid.", [team]));
                    return null;
                }

                var result = new System.Text.StringBuilder("", Bridge.Int.mul(41, TSBTool.TecmoTool.positionNames.Count));
                var pos;
                var teamSimData = this.GetTeamSimData(team);
                var data = "";
                if (teamSimData < 15) {
                    data = System.String.format("0{0:x}", [Bridge.box(teamSimData, System.Byte)]);
                } else {
                    data = System.String.format("{0:x}", [Bridge.box(teamSimData, System.Byte)]);
                }
                if (this.ShowOffPref) {
                    data = (data || "") + (this.GetTeamSimOffensePref(team));
                }

                var teamString = System.String.format("TEAM = {0} SimData=0x{1}", team, data);
                result.append(teamString);

                if (TSBTool.TecmoTool.ShowTeamFormation) {
                    result.append(System.String.format(", {0}", [this.GetTeamOffensiveFormation(team)]));
                }
                result.append("\n");

                if (TSBTool.TecmoTool.ShowPlaybook) {
                    result.append(System.String.format("{0}\n", [this.GetPlaybook(team)]));
                }
                if (TSBTool.TecmoTool.ShowTeamStrings) {
                    var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                    result.append(System.String.format("TEAM_ABB={0},TEAM_CITY={1},TEAM_NAME={2}\n", this.GetTeamAbbreviation(teamIndex), this.GetTeamCity(teamIndex), this.GetTeamName(teamIndex)));
                }
                if (TSBTool.TecmoTool.ShowColors) {
                    result.append(System.String.format("COLORS {0}, {1}, {2}\n", this.GetGameUniform(team), this.GetChampColors(team), this.GetUniformUsage(team)));
                }

                for (var i = 0; i < TSBTool.TecmoTool.positionNames.Count; i = (i + 1) | 0) {
                    pos = TSBTool.TecmoTool.positionNames.getItem(i);
                    result.append(System.String.format("{0}\n", [this.GetPlayerData(team, pos, true, true, true, true, true)]));
                }
                result.append(System.String.format("KR, {0}\nPR, {1}\n", this.GetKickReturner(team), this.GetPuntReturner(team)));
                result.append("\n");
                return result.toString();
            },
            GetTeamName: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(((teamIndex + 64) | 0));
                return retVal;
            },
            GetTeamCity: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(((teamIndex + 32) | 0));
                return retVal;
            },
            GetTeamAbbreviation: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(teamIndex);
                return retVal;
            },
            GetTeamStringTableString: function (stringIndex) {
                var $t;
                var length = { v : 0 };
                var stringStartingLocation = this.GetTeamStringTableLocation(stringIndex, length);

                var stringChars = System.Array.init(length.v, 0, System.Char);
                for (var i = 0; i < stringChars.length; i = (i + 1) | 0) {
                    stringChars[System.Array.index(i, stringChars)] = ($t = this.OutputRom)[System.Array.index(((stringStartingLocation + i) | 0), $t)];
                }
                var retVal = System.String.fromCharArray(stringChars);
                return retVal;
            },
            /**
             * Returns the location of the 'Team' string table. This string table 
             contains the city abbreviations, city names and team names.
             *
             * @instance
             * @private
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {number}          stringIndex    The index of the string to get.
             * @param   {System.Int32}    length         out param stores the length.
             * @return  {number}                         Returns the location of the string at the specified index.
             */
            GetTeamStringTableLocation: function (stringIndex, length) {
                var $t, $t1, $t2, $t3;
                var team_string_table_loc = this.GetTeamStringTableStart();
                var pointer_loc = (team_string_table_loc + Bridge.Int.mul(2, stringIndex)) | 0;
                var b1 = ($t = this.OutputRom)[System.Array.index(((pointer_loc + 1) | 0), $t)];
                var b2 = ($t1 = this.OutputRom)[System.Array.index(pointer_loc, $t1)];
                var b3 = ($t2 = this.OutputRom)[System.Array.index(((pointer_loc + 3) | 0), $t2)];
                var b4 = ($t3 = this.OutputRom)[System.Array.index(((pointer_loc + 2) | 0), $t3)];
                length.v = (((((b3 << 8) + b4) | 0)) - ((((b1 << 8) + b2) | 0))) | 0;
                var pointerVal = ((b1 << 8) + b2) | 0;
                pointerVal = (pointerVal - 48128) | 0;
                var stringStartingLocation = (team_string_table_loc + pointerVal) | 0;
                return stringStartingLocation;
            },
            SetTeamStringTableString: function (stringIndex, newValue) {
                var $t;
                var junk = { v : 0 };
                var oldValue = this.GetTeamStringTableString(stringIndex);
                if (Bridge.referenceEquals(oldValue, newValue)) {
                    return;
                }
                var shiftAmount = { v : (newValue.length - oldValue.length) | 0 };
                if (shiftAmount.v !== 0) {
                    var currentPointerLocation = (this.GetTeamStringTableStart() + Bridge.Int.mul(2, stringIndex)) | 0;
                    var lastPointerLocation = (this.GetTeamStringTableStart() + Bridge.Int.mul(2, this.NumberOfStringsInTeamStringTable)) | 0;
                    this.AdjustDataPointers(currentPointerLocation, shiftAmount.v, lastPointerLocation);
                    var startPosition = (this.GetTeamStringTableLocation(((stringIndex + 1) | 0), junk) - 1) | 0;
                    var endPosition = 131072;
                    if (shiftAmount.v < 0) {
                        this.ShiftDataUp(startPosition, endPosition, shiftAmount.v, this.outputRom);
                    } else {
                        if (shiftAmount.v > 0) {
                            this.ShiftDataDown(startPosition, endPosition, shiftAmount.v, this.outputRom);
                        }
                    }
                }
                var startLoc = this.GetTeamStringTableLocation(stringIndex, shiftAmount);
                for (var i = 0; i < newValue.length; i = (i + 1) | 0) {
                    ($t = this.OutputRom)[System.Array.index(((startLoc + i) | 0), $t)] = (newValue.charCodeAt(i)) & 255;
                }
            },
            SetTeamAbbreviation: function (teamIndex, abb) {
                if (abb != null && abb.length === 4) {
                    this.SetTeamStringTableString(teamIndex, abb);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Error setting team abbreviation, teamIndex={0}; value length must == 4; {1}", Bridge.box(teamIndex, System.Int32), abb));
                }
            },
            SetTeamName: function (teamIndex, name) {
                if (name.length > 1) {
                    this.SetTeamStringTableString(((teamIndex + 64) | 0), name);
                } else {
                    TSBTool.StaticUtils.AddError("'SetTeamCity': team name must not be empty");
                }
            },
            SetTeamCity: function (teamIndex, city) {
                if (city.length > 1) {
                    this.SetTeamStringTableString(((teamIndex + 32) | 0), city);
                } else {
                    TSBTool.StaticUtils.AddError("'SetTeamCity': city name must not be empty");
                }
            },
            GetTeamStringTableStart: function () {
                var team_string_table_loc = 130064;
                return team_string_table_loc;
            },
            GetAll$1: function (season) {
                return this.GetAll();
            },
            GetAll: function () {
                var team;
                var all = new System.Text.StringBuilder("", Bridge.Int.mul(1230, TSBTool.TecmoTool.positionNames.Count));
                var year = System.String.format("YEAR={0}\n", [this.GetYear()]);
                all.append(year);
                for (var i = 0; i < TSBTool.TecmoTool.teams.Count; i = (i + 1) | 0) {
                    team = TSBTool.TecmoTool.teams.getItem(i);
                    all.append(this.GetTeamPlayers(team));
                }

                return all.toString();
            },
            GetProBowlPlayers$1: function (season) {
                return this.GetProBowlPlayers();
            },
            GetProBowlPlayers: function () {
                var builder = new System.Text.StringBuilder("", 1000);
                builder.append("# AFC ProBowl players\r\n");
                builder.append(this.GetConferenceProBowlPlayers(TSBTool.Conference.AFC));
                builder.append("\r\n");

                builder.append("# NFC ProBowl players\r\n");
                builder.append(this.GetConferenceProBowlPlayers(TSBTool.Conference.NFC));
                builder.append("\r\n");
                return builder.toString();
            },
            /**
             * Gets the point in the player number name data that a player's data begins.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetDataPosition: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetDataPosition:: either team {0} or position {1} is invalid.", team, position));
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(position);
                var guy = (Bridge.Int.mul(teamIndex, TSBTool.TecmoTool.positionNames.Count) + positionIndex) | 0;
                var pointerLocation = (this.namePointersStart + (Bridge.Int.mul(2, guy))) | 0;
                var lowByte = this.outputRom[System.Array.index(pointerLocation, this.outputRom)];
                var hiByte = this.outputRom[System.Array.index(((pointerLocation + 1) | 0), this.outputRom)];
                hiByte = hiByte << 8;
                hiByte = (hiByte + lowByte) | 0;

                var ret = (hiByte + this.dataPositionOffset) | 0;
                return ret;
            },
            /**
             * Get the starting point of the guy AFTER the one passed to this method.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetNextDataPosition: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetNextDataPosition:: either team {0} or position {1} is invalid.", team, position));
                }

                var ti = TSBTool.TecmoTool.GetTeamIndex(team);
                var pi = this.GetPositionIndex(position);
                pi = (pi + 1) | 0;
                if (Bridge.referenceEquals(position, TSBTool.TecmoTool.positionNames.getItem(((TSBTool.TecmoTool.positionNames.Count - 1) | 0)))) {
                    ti = (ti + 1) | 0;
                    pi = 0;
                }
                if (ti === 28 && Bridge.referenceEquals(position, TSBTool.TecmoTool.positionNames.getItem(((TSBTool.TecmoTool.positionNames.Count - 1) | 0)))) {
                    return -1;
                } else {
                    return this.GetDataPosition(TSBTool.TecmoTool.teams.getItem(ti), TSBTool.TecmoTool.positionNames.getItem(pi));
                }
            },
            GetPointerPosition: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetPointerPosition:: either team {0} or position {1} is invalid.", team, position));
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(position);
                var playerSpot = (Bridge.Int.mul(teamIndex, TSBTool.TecmoTool.positionNames.Count) + positionIndex) | 0;
                if (Bridge.referenceEquals(team, TSBTool.TecmoTool.teams.getItem(((TSBTool.TecmoTool.teams.Count - 1) | 0))) && Bridge.referenceEquals(position, TSBTool.TecmoTool.positionNames.getItem(((TSBTool.TecmoTool.positionNames.Count - 1) | 0)))) {
                    return ((this.lastPlayerNamePointer - 2) | 0);
                }
                if (positionIndex < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Position '{0}' does not exist. Valid positions are:", [position]));
                    for (var i = 1; i <= TSBTool.TecmoTool.positionNames.Count; i = (i + 1) | 0) {
                        TSBTool.StaticUtils.WriteError(System.String.format("{0}\t", [TSBTool.TecmoTool.positionNames.getItem(((i - 1) | 0))]));
                    }
                    return -1;
                }
                var ret = (this.namePointersStart + (Bridge.Int.mul(2, playerSpot))) | 0;
                return ret;
            },
            /**
             * Sets the player data (jersey number, player name) in the data segment.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}            team        The team the player is assigned to.
             * @param   {string}            position    The position the player is assigned to.
             * @param   {Array.<number>}    bytes       The player's number and name data.
             * @param   {number}            change
             * @return  {void}
             */
            UpdatePlayerData: function (team, position, bytes, change) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) UpdatePlayerData:: either team {0} or position {1} is invalid.", team, position));
                }
                if (bytes == null) {
                    return;
                }

                var dataStart = this.GetDataPosition(team, position);
                this.ShiftDataAfter(team, position, change);
                var j = 0;
                for (var i = dataStart; j < bytes.length; i = (i + 1) | 0) {
                    this.outputRom[System.Array.index(i, this.outputRom)] = bytes[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), bytes)];
                }
            },
            ShiftDataAfter: function (team, position, shiftAmount) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataAfter:: either team {0} or position {1} is invalid.", team, position));
                }

                if (Bridge.referenceEquals(team, TSBTool.TecmoTool.teams.getItem(((TSBTool.TecmoTool.teams.Count - 1) | 0))) && Bridge.referenceEquals(position, "P")) {
                    return;
                }

                var endPosition = 12303;
                while (this.outputRom[System.Array.index(endPosition, this.outputRom)] === 255) {
                    endPosition = (endPosition - 1) | 0;
                }

                endPosition = (endPosition + 1) | 0;

                var startPosition = this.GetNextDataPosition(team, position);
                if (shiftAmount < 0) {
                    this.ShiftDataUp(startPosition, endPosition, shiftAmount, this.outputRom);
                } else {
                    if (shiftAmount > 0) {
                        this.ShiftDataDown(startPosition, endPosition, shiftAmount, this.outputRom);
                    }
                }
            },
            ShiftDataUp: function (startPos, endPos, shiftAmount, data) {
                if (startPos < 0 || endPos < 0) {
                    throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataUp:: either startPos {0} or endPos {1} is invalid.", Bridge.box(startPos, System.Int32), Bridge.box(endPos, System.Int32)));
                }

                var i;
                if (shiftAmount > 0) {
                    System.Console.WriteLine("positive shift amount in ShiftDataUp");
                }

                for (i = startPos; i <= endPos; i = (i + 1) | 0) {
                    data[System.Array.index(((i + shiftAmount) | 0), data)] = data[System.Array.index(i, data)];
                }
                /* i--;
                			for(int j=shiftAmount; j < 0; j++) 
                				data[i++] = 0xff; */

                i = (i + shiftAmount) | 0;
                while (this.outputRom[System.Array.index(i, this.outputRom)] !== 255 && i < 12303) {
                    this.outputRom[System.Array.index(i, this.outputRom)] = 255;
                    i = (i + 1) | 0;
                }

            },
            ShiftDataDown: function (startPos, endPos, shiftAmount, data) {
                if (startPos < 0 || endPos < 0) {
                    throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataDown:: either startPos {0} or endPos {1} is invalid.", Bridge.box(startPos, System.Int32), Bridge.box(endPos, System.Int32)));
                }

                for (var i = (endPos + shiftAmount) | 0; i > startPos; i = (i - 1) | 0) {
                    data[System.Array.index(i, data)] = data[System.Array.index(((i - shiftAmount) | 0), data)];
                }
            },
            GetDataAfter: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetDataAfter:: either team {0} or position {1} is invalid.", team, position));
                }

                if (Bridge.referenceEquals(team, TSBTool.TecmoTool.teams.getItem(((TSBTool.TecmoTool.teams.Count - 1) | 0))) && Bridge.referenceEquals(position, "P")) {
                    return null;
                }

                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(position);
                positionIndex = (positionIndex + 1) | 0;
                if (Bridge.referenceEquals(position, "P")) {
                    teamIndex = (teamIndex + 1) | 0;
                    positionIndex = 0;
                }
                var endPosition = 12303;
                while (this.outputRom[System.Array.index(endPosition, this.outputRom)] === 255) {
                    endPosition = (endPosition - 1) | 0;
                }

                endPosition = (endPosition + 1) | 0;
                var startPosition = this.GetDataPosition(TSBTool.TecmoTool.teams.getItem(teamIndex), TSBTool.TecmoTool.positionNames.getItem(positionIndex));
                var retBytes = System.Array.init(((endPosition - startPosition) | 0), 0, System.Byte);

                var j = 0;
                for (var i = startPosition; i < ((endPosition + 1) | 0); i = (i + 1) | 0) {
                    retBytes[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), retBytes)] = this.outputRom[System.Array.index(i, this.outputRom)];
                }

                return retBytes;
            },
            /**
             * @instance
             * @protected
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    positionName    like 'QB1', 'K','P' ...
             * @return  {number}
             */
            GetPositionIndex: function (positionName) {
                var ret = -1;
                for (var i = 0; i < TSBTool.TecmoTool.positionNames.Count; i = (i + 1) | 0) {
                    if (Bridge.referenceEquals(TSBTool.TecmoTool.positionNames.getItem(i), positionName)) {
                        ret = i;
                        break;
                    }
                }
                return ret;
            },
            /**
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team              
             * @param   {string}    qb                Either 'QB1' or 'QB2'
             * @param   {number}    runningSpeed      
             * @param   {number}    rushingPower      
             * @param   {number}    maxSpeed          
             * @param   {number}    hittingPower      
             * @param   {number}    passingSpeed      
             * @param   {number}    passControl       
             * @param   {number}    accuracy          
             * @param   {number}    avoidPassBlock
             * @return  {void}
             */
            SetQBAbilities: function (team, qb, runningSpeed, rushingPower, maxSpeed, hittingPower, passingSpeed, passControl, accuracy, avoidPassBlock) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }
                if (!Bridge.referenceEquals(qb, "QB1") && !Bridge.referenceEquals(qb, "QB2")) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Cannot set qb ablities for {0}", [qb]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);
                passingSpeed = this.GetAbility(passingSpeed);
                passControl = this.GetAbility(passControl);
                accuracy = this.GetAbility(accuracy);
                avoidPassBlock = this.GetAbility(avoidPassBlock);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower) || !this.IsValidAbility(passingSpeed) || !this.IsValidAbility(passControl) || !this.IsValidAbility(accuracy) || !this.IsValidAbility(avoidPassBlock)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Abilities for {0} on {1} were not set.", qb, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, qb, runningSpeed, rushingPower, maxSpeed, hittingPower, passingSpeed, passControl);
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var posIndex = this.GetPositionIndex(qb);
                var location = this.GetAttributeLocation(teamIndex, posIndex);
                var lastByte = accuracy << 4;
                lastByte = (lastByte + avoidPassBlock) | 0;
                this.outputRom[System.Array.index(((location + 4) | 0), this.outputRom)] = lastByte & 255;
                lastByte = passingSpeed << 4;
                lastByte = (lastByte + passControl) | 0;
                this.outputRom[System.Array.index(((location + 3) | 0), this.outputRom)] = lastByte & 255;
            },
            SetSkillPlayerAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, ballControl, receptions) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }

                if (!Bridge.referenceEquals(pos, "RB1") && !Bridge.referenceEquals(pos, "RB2") && !Bridge.referenceEquals(pos, "RB3") && !Bridge.referenceEquals(pos, "RB4") && !Bridge.referenceEquals(pos, "WR1") && !Bridge.referenceEquals(pos, "WR2") && !Bridge.referenceEquals(pos, "WR3") && !Bridge.referenceEquals(pos, "WR4") && !Bridge.referenceEquals(pos, "TE1") && !Bridge.referenceEquals(pos, "TE2")) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Cannot set skill player ablities for {0}.", [pos]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);
                ballControl = this.GetAbility(ballControl);
                receptions = this.GetAbility(receptions);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower) || !this.IsValidAbility(receptions) || !this.IsValidAbility(ballControl)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Invalid attribute. Abilities for {0} on {1} were not set.", pos, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, ballControl, receptions);
            },
            SetKickPlayerAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, kickingAbility, avoidKickBlock) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }

                if (!Bridge.referenceEquals(pos, "K") && !Bridge.referenceEquals(pos, "P")) {
                    TSBTool.StaticUtils.AddError(System.String.format("Cannot set kick player ablities for {0}.", [pos]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);
                kickingAbility = this.GetAbility(kickingAbility);
                avoidKickBlock = this.GetAbility(avoidKickBlock);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower) || !this.IsValidAbility(kickingAbility) || !this.IsValidAbility(avoidKickBlock)) {
                    TSBTool.StaticUtils.AddError(System.String.format("Abilities for {0} on {1} were not set.", pos, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, kickingAbility, avoidKickBlock);
            },
            SetDefensivePlayerAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, passRush, interceptions) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }

                if (!Bridge.referenceEquals(pos, "RE") && !Bridge.referenceEquals(pos, "NT") && !Bridge.referenceEquals(pos, "LE") && !Bridge.referenceEquals(pos, "ROLB") && !Bridge.referenceEquals(pos, "RILB") && !Bridge.referenceEquals(pos, "LILB") && !Bridge.referenceEquals(pos, "LOLB") && !Bridge.referenceEquals(pos, "RCB") && !Bridge.referenceEquals(pos, "LCB") && !Bridge.referenceEquals(pos, "SS") && !Bridge.referenceEquals(pos, "FS")) {
                    TSBTool.StaticUtils.AddError(System.String.format("Cannot set defensive player ablities for {0}.", [pos]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);
                passRush = this.GetAbility(passRush);
                interceptions = this.GetAbility(interceptions);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower) || !this.IsValidAbility(passRush) || !this.IsValidAbility(interceptions)) {
                    TSBTool.StaticUtils.AddError(System.String.format("Abilities for {0} on {1} were not set.", pos, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, passRush, interceptions);
            },
            SetOLPlayerAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }

                if (!Bridge.referenceEquals(pos, "C") && !Bridge.referenceEquals(pos, "RG") && !Bridge.referenceEquals(pos, "LG") && !Bridge.referenceEquals(pos, "RT") && !Bridge.referenceEquals(pos, "LT")) {
                    TSBTool.StaticUtils.AddError(System.String.format("Cannot set OL player ablities for {0}.", [pos]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower)) {
                    TSBTool.StaticUtils.AddError(System.String.format("Abilities for {0} on {1} were not set.", pos, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, -1, -1);
            },
            SaveAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, bc, rec) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SaveAbilities:: team {0} is invalid", [team]));
                    return;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SaveAbilities:: position {0} is invalid", [pos]));
                    return;
                }

                var byte1, byte2, byte3;
                byte1 = rushingPower & 255;
                byte1 = byte1 << 4;
                byte1 = (byte1 + (runningSpeed & 255)) | 0;
                byte2 = maxSpeed & 255;
                byte2 = byte2 << 4;
                byte2 = (byte2 + (hittingPower & 255)) | 0;
                byte3 = bc & 255;
                byte3 = byte3 << 4;
                byte3 = (byte3 + (rec & 255)) | 0;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var posIndex = this.GetPositionIndex(pos);
                var location = this.GetAttributeLocation(teamIndex, posIndex);

                this.outputRom[System.Array.index(location, this.outputRom)] = byte1 & 255;
                this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = byte2 & 255;

                if (bc > -1 && rec > -1) {
                    this.outputRom[System.Array.index(((location + 3) | 0), this.outputRom)] = byte3 & 255;
                }
            },
            IsValidAbility: function (ab) {
                return ab >= 0 && ab <= 15;
            },
            GetAbility: function (ab) {
                var ret = 0;
                switch (ab) {
                    case 6: 
                        ret = 0;
                        break;
                    case 13: 
                        ret = 1;
                        break;
                    case 19: 
                        ret = 2;
                        break;
                    case 25: 
                        ret = 3;
                        break;
                    case 31: 
                        ret = 4;
                        break;
                    case 38: 
                        ret = 5;
                        break;
                    case 44: 
                        ret = 6;
                        break;
                    case 50: 
                        ret = 7;
                        break;
                    case 56: 
                        ret = 8;
                        break;
                    case 63: 
                        ret = 9;
                        break;
                    case 69: 
                        ret = 10;
                        break;
                    case 75: 
                        ret = 11;
                        break;
                    case 81: 
                        ret = 12;
                        break;
                    case 88: 
                        ret = 13;
                        break;
                    case 94: 
                        ret = 14;
                        break;
                    case 100: 
                        ret = 15;
                        break;
                }
                return ret;
            },
            MapAbality: function (ab) {
                var ret = 0;
                switch (ab) {
                    case 0: 
                        ret = 6;
                        break;
                    case 1: 
                        ret = 13;
                        break;
                    case 2: 
                        ret = 19;
                        break;
                    case 3: 
                        ret = 25;
                        break;
                    case 4: 
                        ret = 31;
                        break;
                    case 5: 
                        ret = 38;
                        break;
                    case 6: 
                        ret = 44;
                        break;
                    case 7: 
                        ret = 50;
                        break;
                    case 8: 
                        ret = 56;
                        break;
                    case 9: 
                        ret = 63;
                        break;
                    case 10: 
                        ret = 69;
                        break;
                    case 11: 
                        ret = 75;
                        break;
                    case 12: 
                        ret = 81;
                        break;
                    case 13: 
                        ret = 88;
                        break;
                    case 14: 
                        ret = 94;
                        break;
                    case 15: 
                        ret = 100;
                        break;
                }
                return ret;
            },
            /**
             * Returns an array of ints mapping to a player's abilities.
             Like { 13, 13, 50, 56, 31, 25}. The length of the array returned varies depending
             on position.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}            team        Team name like 'oilers'.
             * @param   {string}            position    Position name like 'RB4'.
             * @return  {Array.<number>}                an array of ints.
             */
            GetAbilities: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    return null;
                }

                var ret = System.Array.init([0], System.Int32);
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var posIndex = this.GetPositionIndex(position);
                var location = this.GetAttributeLocation(teamIndex, posIndex);
                var runningSpeed, rushingPower, maxSpeed, hittingPower, wild1, wild2, accuracy, avoidPassBlock;
                var b1, b2, b3, b4;
                b1 = this.outputRom[System.Array.index(location, this.outputRom)];
                b2 = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)];
                b3 = this.outputRom[System.Array.index(((location + 3) | 0), this.outputRom)];
                b4 = this.outputRom[System.Array.index(((location + 4) | 0), this.outputRom)];
                runningSpeed = b1 & 15;
                runningSpeed = this.MapAbality(runningSpeed);
                rushingPower = b1 & 240;
                rushingPower = this.MapAbality(rushingPower >> 4);
                maxSpeed = b2 & 240;
                maxSpeed = this.MapAbality(maxSpeed >> 4);
                hittingPower = b2 & 15;
                hittingPower = this.MapAbality(hittingPower);
                wild1 = b3 & 240;
                wild1 = this.MapAbality(wild1 >> 4);
                wild2 = b3 & 15;
                wild2 = this.MapAbality(wild2);
                accuracy = b4 & 240;
                accuracy = this.MapAbality(accuracy >> 4);
                avoidPassBlock = b4 & 15;
                avoidPassBlock = this.MapAbality(avoidPassBlock);
                switch (position) {
                    case "C": 
                    case "RG": 
                    case "LG": 
                    case "RT": 
                    case "LT": 
                        ret = System.Array.init(4, 0, System.Int32);
                        break;
                    case "QB1": 
                    case "QB2": 
                        ret = System.Array.init(8, 0, System.Int32);
                        ret[System.Array.index(4, ret)] = wild1;
                        ret[System.Array.index(5, ret)] = wild2;
                        ret[System.Array.index(6, ret)] = accuracy;
                        ret[System.Array.index(7, ret)] = avoidPassBlock;
                        break;
                    default: 
                        ret = System.Array.init(6, 0, System.Int32);
                        ret[System.Array.index(4, ret)] = wild1;
                        ret[System.Array.index(5, ret)] = wild2;
                        break;
                }
                ret[System.Array.index(0, ret)] = runningSpeed;
                ret[System.Array.index(1, ret)] = rushingPower;
                ret[System.Array.index(2, ret)] = maxSpeed;
                ret[System.Array.index(3, ret)] = hittingPower;
                return ret;
            },
            GetAttributeLocation: function (teamIndex, posIndex) {
                var location = ((((Bridge.Int.mul(teamIndex, this.teamAbilityOffset)) + this.abilityOffsets[System.Array.index(posIndex, this.abilityOffsets)]) | 0) + this.billsQB1AbilityStart) | 0;
                return location;
            },
            /**
             * Returns a string consisting of numbers, spaces and commas.
             Like "31, 69, 13, 13, 31, 44"
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {string}
             */
            GetAbilityString: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    return null;
                }
                var abilities = this.GetAbilities(team, position);
                var stuff = new System.Text.StringBuilder();

                for (var i = 0; i < abilities.length; i = (i + 1) | 0) {
                    stuff.append(abilities[System.Array.index(i, abilities)]);
                    stuff.append(", ");
                }
                stuff.remove(((stuff.getLength() - 2) | 0), 1);
                return stuff.toString();
            },
            /**
             * Returns the simulation data for the given team.
             Simulation data is of the form '0xNN' where N is a number 1-F (hex).
             A team's sim data of '0x57' signifies that the team has a simulation figure of
             '5' for offense, and '7' for defense.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team    The team of interest
             * @return  {number}
             */
            GetTeamSimData: function (team) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex >= 0) {
                    var location = (Bridge.Int.mul(teamIndex, this.teamSimOffset) + this.billsTeamSimLoc) | 0;
                    return this.outputRom[System.Array.index(location, this.outputRom)];
                }
                return 0;
            },
            /**
             * Sets the given team's offense and defense sim values.
             Simulation data is of the form '0xNN' where N is a number 1-F (hex).
             A team's sim data of '0x57' signifies that the team has a simulation figure of
             '5' for offense, and '7' for defense.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team      The team to set.
             * @param   {number}    values    The value to set it to.
             * @return  {void}
             */
            SetTeamSimData: function (team, values) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetTeamSimData:: team {0} is invalid ", [team]));
                    return;
                }

                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var location = (Bridge.Int.mul(teamIndex, this.teamSimOffset) + this.billsTeamSimLoc) | 0;
                var currentValue = this.outputRom[System.Array.index(location, this.outputRom)];
                this.outputRom[System.Array.index(location, this.outputRom)] = values;
                currentValue = this.outputRom[System.Array.index(location, this.outputRom)];
            },
            /**
             * Sets the team sim offense tendency . 
             00 = Little more rushing, 01 = Heavy Rushing, 
             02 = little more passing, 03 = Heavy Passing.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}     team    the team name
             * @param   {number}     val     the number to set it to.
             * @return  {boolean}            true if set, fales if could not set it.
             */
            SetTeamSimOffensePref: function (team, val) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (val > -1 && val < 4 && teamIndex !== -1) {
                    var loc = (this.teamSimOffensivePrefStart + teamIndex) | 0;
                    this.outputRom[System.Array.index(loc, this.outputRom)] = val & 255;
                } else {
                    if (teamIndex !== -1) {
                        TSBTool.StaticUtils.AddError(System.String.format("Can't set offensive pref to '{0}' valid values are 0-3.\n", [Bridge.box(val, System.Int32)]));
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("Team '{0}' is invalid\n", [team]));
                    }
                }
                return true;
            },
            /**
             * Sets the team sim offense tendency . 
             00 = Little more rushing, 01 = Heavy Rushing, 
             02 = little more passing, 03 = Heavy Passing.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team    Teh team name.
             * @return  {number}            their sim offense pref (0 - 3)
             */
            GetTeamSimOffensePref: function (team) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var val = -1;
                if (teamIndex > -1) {
                    var loc = (this.teamSimOffensivePrefStart + teamIndex) | 0;
                    val = this.outputRom[System.Array.index(loc, this.outputRom)];
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Team '{0}' is invalid\n", [team]));
                }
                return val;
            },
            GetPlayerSimData: function (team, pos) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetPlayerSimData:: Invalid team {0}", [team]));
                    return null;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetPlayerSimData:: Invalid Position {0}", [pos]));
                    return null;
                }

                switch (pos) {
                    case "QB1": 
                    case "QB2": 
                        return this.GetQBSimData(team, pos);
                    case "RB1": 
                    case "RB2": 
                    case "RB3": 
                    case "RB4": 
                    case "WR1": 
                    case "WR2": 
                    case "WR3": 
                    case "WR4": 
                    case "TE1": 
                    case "TE2": 
                        return this.GetSkillSimData(team, pos);
                    case "RE": 
                    case "NT": 
                    case "LE": 
                    case "LOLB": 
                    case "LILB": 
                    case "RILB": 
                    case "ROLB": 
                    case "RCB": 
                    case "LCB": 
                    case "FS": 
                    case "SS": 
                        return this.GetDefensiveSimData(team, pos);
                    case "K": 
                        return this.GetKickingSimData(team);
                    case "P": 
                        return this.GetPuntingSimData(team);
                    default: 
                        return null;
                }
            },
            GetKickingSimData: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetKickingSimData:: Invalid team {0}", [team]));
                    return null;
                }
                var ret = System.Array.init(1, 0, System.Int32);
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var location = this.GetPunkKickSimDataLocation(teamIndex);
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] >> 4;
                return ret;
            },
            SetKickingSimData: function (team, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetKickingSimData:: Invalid team {0}", [team]));
                    return;
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var location = this.GetPunkKickSimDataLocation(teamIndex);
                var g = this.outputRom[System.Array.index(location, this.outputRom)];
                g = g & 15;
                var g2 = data << 4;
                g = (g + g2) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = g & 255;
            },
            GetPuntingSimData: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetPuntingSimData:: Invalid team {0}", [team]));
                    return null;
                }
                var ret = System.Array.init(1, 0, System.Int32);
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var location = this.GetPunkKickSimDataLocation(teamIndex);
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] & 15;
                return ret;
            },
            SetPuntingSimData: function (team, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetPuntingSimData:: Invalid team {0}", [team]));
                    return;
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var location = this.GetPunkKickSimDataLocation(teamIndex);
                var d = this.outputRom[System.Array.index(location, this.outputRom)];
                d = d & 240;
                d = (d + data) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = d & 255;
            },
            GetPunkKickSimDataLocation: function (teamIndex) {
                var ret = (((Bridge.Int.mul(teamIndex, this.teamSimOffset) + this.billsQB1SimLoc) | 0) + 46) | 0;
                return ret;
            },
            GetDefensiveSimData: function (team, pos) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetDefensiveSimData:: Invalid team {0}", [team]));
                    return null;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetDefensiveSimData:: Invalid Position {0}", [pos]));
                    return null;
                }

                var ret = System.Array.init(2, 0, System.Int32);
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(pos);
                var location = this.GetDefinsivePlayerSimDataLocation(team, pos);
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)];
                ret[System.Array.index(1, ret)] = this.outputRom[System.Array.index(((location + 11) | 0), this.outputRom)];
                return ret;
            },
            /**
             * Sets the simulation data for a defensive player.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}            team    The team the player belongs to.
             * @param   {string}            pos     the position he plays.
             * @param   {Array.<number>}    data    the data to set it to (length = 2).
             * @return  {void}
             */
            SetDefensiveSimData: function (team, pos, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetDefensiveSimData:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetDefensiveSimData:: Invalid Position {0}", [pos]));
                    return;
                } else if (data == null || data.length < 2) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                    return;
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(pos);
                var location = this.GetDefinsivePlayerSimDataLocation(team, pos);
                var byte1, byte2;
                byte1 = (data[System.Array.index(0, data)]) & 255;
                byte2 = (data[System.Array.index(1, data)]) & 255;

                this.outputRom[System.Array.index(location, this.outputRom)] = byte1;
                this.outputRom[System.Array.index(((location + 11) | 0), this.outputRom)] = byte2;
            },
            GetDefinsivePlayerSimDataLocation: function (team, position) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(position);
                var location = (((Bridge.Int.mul(teamIndex, this.teamSimOffset) + (((positionIndex - 17) | 0))) | 0) + this.billsRESimLoc) | 0;
                return location;
            },
            GetSkillSimData: function (team, pos) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetSkillSimData:: Invalid team {0}", [team]));
                    return null;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetSkillSimData:: Invalid Position {0}", [pos]));
                    return null;
                }

                var ret = System.Array.init(4, 0, System.Int32);
                var location = this.GetOffensivePlayerSimDataLocation(team, pos);
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] >> 4;
                ret[System.Array.index(1, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] & 15;
                ret[System.Array.index(2, ret)] = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] >> 4;
                ret[System.Array.index(3, ret)] = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] & 15;
                return ret;
            },
            SetSkillSimData: function (team, pos, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetSkillSimData:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetSkillSimData:: Invalid Position {0}", [pos]));
                    return;
                } else if (data == null || data.length < 4) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                    return;
                }

                var location = this.GetOffensivePlayerSimDataLocation(team, pos);
                var byte1, byte2;
                byte1 = data[System.Array.index(0, data)] << 4;
                byte1 = (byte1 + data[System.Array.index(1, data)]) | 0;
                byte2 = data[System.Array.index(2, data)] << 4;
                byte2 = (byte2 + data[System.Array.index(3, data)]) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = byte1 & 255;
                this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = byte2 & 255;
            },
            GetQBSimData: function (team, pos) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetQBSimData:: Invalid team {0}", [team]));
                    return null;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetQBSimData:: Invalid Position {0}", [pos]));
                    return null;
                }

                var ret = System.Array.init(3, 0, System.Int32);
                var location = this.GetOffensivePlayerSimDataLocation(team, pos);

                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] >> 4;
                ret[System.Array.index(1, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] & 15;
                ret[System.Array.index(2, ret)] = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)];
                return ret;
            },
            GetOffensivePlayerSimDataLocation: function (team, position) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(position);
                var location = (((Bridge.Int.mul(teamIndex, this.teamSimOffset) + (Bridge.Int.mul(positionIndex, 2))) | 0) + this.billsQB1SimLoc) | 0;
                return location;
            },
            SetQBSimData: function (team, pos, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetQBSimData:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetQBSimData:: Invalid Position {0}", [pos]));
                    return;
                } else if (data == null || data.length < 2) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                    return;
                }

                var location = this.GetOffensivePlayerSimDataLocation(team, pos);
                var byte1, byte2;
                byte1 = ((data[System.Array.index(0, data)]) & 255) << 4;
                byte1 = (byte1 + ((data[System.Array.index(1, data)]) & 255)) | 0;
                byte2 = (data[System.Array.index(2, data)]) & 255;
                this.outputRom[System.Array.index(location, this.outputRom)] = byte1 & 255;
                this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = byte2 & 255;
            },
            /**
             * Get the face number from the given team/position
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetFace: function (team, position) {
                var positionOffset = this.GetPositionIndex(position);
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (positionOffset < 0 || teamIndex < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("GetFace Error getting face for {0} {1}", team, position));
                    return -1;
                }
                var loc = (this.faceOffsets[System.Array.index(positionOffset, this.faceOffsets)] + this.faceTeamOffsets[System.Array.index(teamIndex, this.faceTeamOffsets)]) | 0;
                loc = (((12306 + this.faceOffsets[System.Array.index(positionOffset, this.faceOffsets)]) | 0) + Bridge.Int.mul(teamIndex, 117)) | 0;
                var ret = this.outputRom[System.Array.index(loc, this.outputRom)];
                return ret;
            },
            /**
             * Sets the face for the guy at position 'position' on team 'team'.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team        
             * @param   {string}    position    
             * @param   {number}    face
             * @return  {void}
             */
            SetFace: function (team, position, face) {
                var positionOffset = this.GetPositionIndex(position);
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (positionOffset < 0 || teamIndex < 0 || !!(face < 0 | face > 212)) {
                    TSBTool.StaticUtils.AddError(System.String.format("SetFace Error setting face for {0} {1} face={2}", team, position, Bridge.box(face, System.Int32)));
                    if (!!(face < 0 | face > 212)) {
                        TSBTool.StaticUtils.AddError(System.String.format("Valid Face numbers are 0x00 - 0xD4", null));
                    }
                    return;
                }
                var loc = (this.faceOffsets[System.Array.index(positionOffset, this.faceOffsets)] + this.faceTeamOffsets[System.Array.index(teamIndex, this.faceTeamOffsets)]) | 0;
                loc = (((12306 + this.faceOffsets[System.Array.index(positionOffset, this.faceOffsets)]) | 0) + Bridge.Int.mul(teamIndex, 117)) | 0;
                this.outputRom[System.Array.index(loc, this.outputRom)] = face & 255;
            },
            /**
             * Set the punt returner by position.
             Hi nibble.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {void}
             */
            SetPuntReturner: function (team, position) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetPuntReturner:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(position)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetPuntReturner:: Invalid Position {0}", [position]));
                    return;
                }

                var location_1 = (145875 + TSBTool.TecmoTool.teams.indexOf(team)) | 0;
                var location = (207059 + TSBTool.TecmoTool.teams.indexOf(team)) | 0;
                switch (position) {
                    case "QB1": 
                    case "QB2": 
                    case "C": 
                    case "LG": 
                    case "RB1": 
                    case "RB2": 
                    case "RB3": 
                    case "RB4": 
                    case "WR1": 
                    case "WR2": 
                    case "WR3": 
                    case "WR4": 
                    case "TE1": 
                    case "TE2": 
                        var pos = TSBTool.TecmoTool.positionNames.indexOf(position);
                        var b = this.outputRom[System.Array.index(location, this.outputRom)];
                        b = b & 240;
                        b = (b + pos) | 0;
                        this.outputRom[System.Array.index(location, this.outputRom)] = b & 255;
                        this.outputRom[System.Array.index(location_1, this.outputRom)] = b & 255;
                        break;
                    default: 
                        TSBTool.StaticUtils.AddError(System.String.format("Cannot assign '{0}' as a punt returner", [position]));
                        break;
                }

            },
            /**
             * Set the kick returner by position.
             Lo nibble.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {void}
             */
            SetKickReturner: function (team, position) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetKickReturner:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(position)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetKickReturner:: Invalid Position {0}", [position]));
                    return;
                }

                var location_1 = (145875 + TSBTool.TecmoTool.teams.indexOf(team)) | 0;
                var location = (207059 + TSBTool.TecmoTool.teams.indexOf(team)) | 0;
                switch (position) {
                    case "QB1": 
                    case "QB2": 
                    case "C": 
                    case "LG": 
                    case "RB1": 
                    case "RB2": 
                    case "RB3": 
                    case "RB4": 
                    case "WR1": 
                    case "WR2": 
                    case "WR3": 
                    case "WR4": 
                    case "TE1": 
                    case "TE2": 
                        var pos = TSBTool.TecmoTool.positionNames.indexOf(position);
                        var b = this.outputRom[System.Array.index(location, this.outputRom)];
                        b = b & 15;
                        b = (b + (pos << 4)) | 0;
                        this.outputRom[System.Array.index(location, this.outputRom)] = b & 255;
                        this.outputRom[System.Array.index(location_1, this.outputRom)] = b & 255;
                        break;
                    default: 
                        TSBTool.StaticUtils.AddError(System.String.format("Cannot assign '{0}' as a kick returner", [position]));
                        break;
                }

            },
            /**
             * Gets the position who returns punts.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetPuntReturner: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetPuntReturner:: Invalid team {0}", [team]));
                    return null;
                }

                var ret = "";
                var location = (this.mBillsPuntKickReturnerPos + TSBTool.TecmoTool.teams.indexOf(team)) | 0;
                var b = this.outputRom[System.Array.index(location, this.outputRom)];
                b = b & 15;
                ret = TSBTool.TecmoTool.positionNames.getItem(b);
                return ret;
            },
            /**
             * Gets the position who returns kicks.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetKickReturner: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetKickReturner:: Invalid team {0}", [team]));
                    return null;
                }

                var ret = "";
                var location = (207059 + TSBTool.TecmoTool.teams.indexOf(team)) | 0;
                var b = this.outputRom[System.Array.index(location, this.outputRom)];
                b = b & 240;
                b = b >> 4;
                ret = TSBTool.TecmoTool.positionNames.getItem(b);
                return ret;
            },
            ApplySet: function (line) {
                if (this.simpleSetRegex == null) {
                    this.simpleSetRegex = new System.Text.RegularExpressions.Regex.ctor("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
                }

                if (!Bridge.referenceEquals(this.simpleSetRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                    TSBTool.StaticUtils.ApplySimpleSet(line, this);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR with line \"{0}\"", [line]));
                }
            },
            /**
             * Sets the team's offensive formation.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team         
             * @param   {string}    formation
             * @return  {void}
             */
            SetTeamOffensiveFormation: function (team, formation) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex > -1 && teamIndex < 255) {
                    var location = (this.mTeamFormationsStartingLoc + teamIndex) | 0;
                    var location2 = (this.mTeamFormationsStartingLoc2 + teamIndex) | 0;
                    if (this.outputRom[System.Array.index(this.mTeamFormationHackLoc, this.outputRom)] === 160) {
                        TSBTool.StaticUtils.ApplySimpleSet("SET(0x21642, 0x8AA66EBCD09FAA4C5096 ) ", this);
                    }

                    switch (formation) {
                        case TSBTool.TecmoTool.m2RB_2WR_1TE: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 0;
                            this.outputRom[System.Array.index(location2, this.outputRom)] = 0;
                            break;
                        case TSBTool.TecmoTool.m1RB_3WR_1TE: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 2;
                            this.outputRom[System.Array.index(location2, this.outputRom)] = 2;
                            break;
                        case TSBTool.TecmoTool.m1RB_4WR: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 1;
                            this.outputRom[System.Array.index(location2, this.outputRom)] = 1;
                            break;
                        default: 
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR! Formation {0:x} for team {1} is invalid.", formation, team));
                            TSBTool.StaticUtils.AddError(System.String.format("  Valid formations are:\n  {0}\n  {1}\n  {2}", TSBTool.TecmoTool.m2RB_2WR_1TE, TSBTool.TecmoTool.m1RB_3WR_1TE, TSBTool.TecmoTool.m1RB_4WR));
                            break;
                    }
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! Team '{0}' is invalid, Offensive Formation not set", [team]));
                }
            },
            /**
             * Gets the team's offensive formation.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetTeamOffensiveFormation: function (team) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                var ret = "OFFENSIVE_FORMATION = ";
                if (teamIndex > -1 && teamIndex < 255) {
                    var location = (this.mTeamFormationsStartingLoc + teamIndex) | 0;
                    var formation = this.outputRom[System.Array.index(location, this.outputRom)];

                    switch (formation) {
                        case 0: 
                            ret = (ret || "") + (TSBTool.TecmoTool.m2RB_2WR_1TE || "");
                            break;
                        case 2: 
                            ret = (ret || "") + (TSBTool.TecmoTool.m1RB_3WR_1TE || "");
                            break;
                        case 1: 
                            ret = (ret || "") + (TSBTool.TecmoTool.m1RB_4WR || "");
                            break;
                        default: 
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR! Formation {0:x} for team {1} is invalid, ROM FORMATIONS could be messed up.", Bridge.box(formation, System.Int32), team));
                            ret = "";
                            break;
                    }
                } else {
                    ret = "";
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! Team '{0}' is invalid, Offensive Formation get failed.", [team]));
                }
                return ret;
            },
            GetPlaybookLocation: function (team_index) {
                return ((this.mPlaybookStartLoc + (Bridge.Int.mul(team_index, 4))) | 0);
            },
            /**
             * Returns a string like "PLAYBOOK R1, R4, R6, R8, P1, P3, P7, P3"
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetPlaybook: function (team) {
                var ret = "";
                var rSlot1, rSlot2, rSlot3, rSlot4, pSlot1, pSlot2, pSlot3, pSlot4;

                var teamIndex = TSBTool.TecmoTool.teams.indexOf(team);
                if (teamIndex > -1) {
                    var pbLocation = this.GetPlaybookLocation(teamIndex);
                    rSlot1 = this.outputRom[System.Array.index(pbLocation, this.outputRom)] >> 4;
                    rSlot2 = this.outputRom[System.Array.index(pbLocation, this.outputRom)] & 15;
                    rSlot3 = this.outputRom[System.Array.index(((pbLocation + 1) | 0), this.outputRom)] >> 4;
                    rSlot4 = this.outputRom[System.Array.index(((pbLocation + 1) | 0), this.outputRom)] & 15;

                    pSlot1 = this.outputRom[System.Array.index(((pbLocation + 2) | 0), this.outputRom)] >> 4;
                    pSlot2 = this.outputRom[System.Array.index(((pbLocation + 2) | 0), this.outputRom)] & 15;
                    pSlot3 = this.outputRom[System.Array.index(((pbLocation + 3) | 0), this.outputRom)] >> 4;
                    pSlot4 = this.outputRom[System.Array.index(((pbLocation + 3) | 0), this.outputRom)] & 15;

                    ret = System.String.format("PLAYBOOK R{0}{1}{2}{3}, P{4}{5}{6}{7} ", Bridge.box(((rSlot1 + 1) | 0), System.Int32), Bridge.box(((rSlot2 + 1) | 0), System.Int32), Bridge.box(((rSlot3 + 1) | 0), System.Int32), Bridge.box(((rSlot4 + 1) | 0), System.Int32), Bridge.box(((pSlot1 + 1) | 0), System.Int32), Bridge.box(((pSlot2 + 1) | 0), System.Int32), Bridge.box(((pSlot3 + 1) | 0), System.Int32), Bridge.box(((pSlot4 + 1) | 0), System.Int32));
                }

                return ret;
            },
            /**
             * Sets the team's playbook
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team         
             * @param   {string}    runPlays     String like "R1234"
             * @param   {string}    passPlays    String like "P4567"
             * @return  {void}
             */
            SetPlaybook: function (team, runPlays, passPlays) {
                if (this.runRegex == null || this.passRegex == null) {
                    this.runRegex = new System.Text.RegularExpressions.Regex.ctor("R([1-8])([1-8])([1-8])([1-8])");
                    this.passRegex = new System.Text.RegularExpressions.Regex.ctor("P([1-8])([1-8])([1-8])([1-8])");
                }
                var runs = this.runRegex.match(runPlays);
                var pass = this.passRegex.match(passPlays);

                var r1, r2, r3, r4, p1, p2, p3, p4;

                var teamIndex = TSBTool.TecmoTool.teams.indexOf(team);
                if (teamIndex > -1 && !Bridge.referenceEquals(runs, System.Text.RegularExpressions.Match.getEmpty()) && !Bridge.referenceEquals(pass, System.Text.RegularExpressions.Match.getEmpty())) {
                    var pbLocation = this.GetPlaybookLocation(teamIndex);

                    r1 = (System.Int32.parse(runs.getGroups().get(1).toString()) - 1) | 0;
                    r2 = (System.Int32.parse(runs.getGroups().get(2).toString()) - 1) | 0;
                    r3 = (System.Int32.parse(runs.getGroups().get(3).toString()) - 1) | 0;
                    r4 = (System.Int32.parse(runs.getGroups().get(4).toString()) - 1) | 0;

                    p1 = (System.Int32.parse(pass.getGroups().get(1).toString()) - 1) | 0;
                    p2 = (System.Int32.parse(pass.getGroups().get(2).toString()) - 1) | 0;
                    p3 = (System.Int32.parse(pass.getGroups().get(3).toString()) - 1) | 0;
                    p4 = (System.Int32.parse(pass.getGroups().get(4).toString()) - 1) | 0;

                    r1 = ((r1 << 4) + r2) | 0;
                    r3 = ((r3 << 4) + r4) | 0;
                    p1 = ((p1 << 4) + p2) | 0;
                    p3 = ((p3 << 4) + p4) | 0;
                    this.outputRom[System.Array.index(pbLocation, this.outputRom)] = r1 & 255;
                    this.outputRom[System.Array.index(((pbLocation + 1) | 0), this.outputRom)] = r3 & 255;
                    this.outputRom[System.Array.index(((pbLocation + 2) | 0), this.outputRom)] = p1 & 255;
                    this.outputRom[System.Array.index(((pbLocation + 3) | 0), this.outputRom)] = p3 & 255;
                } else {
                    if (teamIndex < 0) {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR! SetPlaybook: Team {0} is Invalid.", [team]));
                    }
                    if (Bridge.referenceEquals(runs, System.Text.RegularExpressions.Match.getEmpty())) {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR! SetPlaybook Run play definition '{0} 'is Invalid", [runPlays]));
                    }
                    if (Bridge.referenceEquals(pass, System.Text.RegularExpressions.Match.getEmpty())) {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR! SetPlaybook Pass play definition '{0} 'is Invalid", [passPlays]));
                    }
                }
            },
            ApplyJuice: function (week, amt) {
                var ret = true;
                if (week > 17 || week < 0 || amt > 17 || amt < 0) {
                    ret = false;
                } else {
                    var rom_location = (this.JUICE_LOCATION + (Bridge.Int.mul(week, 5))) | 0;
                    var index = Bridge.Int.mul((((amt - 1) | 0)), 5);
                    for (var i = 0; i < 5; i = (i + 1) | 0) {
                        this.outputRom[System.Array.index(((rom_location + i) | 0), this.outputRom)] = this.m_JuiceArray[System.Array.index(((index + i) | 0), this.m_JuiceArray)];
                    }
                }
                return ret;
            },
            /**
             * Returns an ArrayList of errors that were encountered during the operation.
             *
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {System.Collections.Generic.List$1}    scheduleList
             * @return  {void}
             */
            ApplySchedule: function (scheduleList) {
                if (scheduleList != null && this.outputRom != null) {
                    var sch = new TSBTool.ScheduleHelper2(this.outputRom);
                    sch.ApplySchedule(scheduleList);
                }
            },
            PrintValidAbilities: function () {
                TSBTool.StaticUtils.AddError(System.String.format("Valid player abilities are 6, 13, 19, 25, 31, 38, 44, 50, 56, 63, 69, 75, 81, 88, 94, 100", null));
            },
            StringifyArray: function (input) {
                if (input == null) {
                    return null;
                }

                var sb = new System.Text.StringBuilder("", 40);
                for (var i = 0; i < input.length; i = (i + 1) | 0) {
                    sb.append(System.String.format("{0}, ", [Bridge.box(input[System.Array.index(i, input)], System.Int32)]));
                }
                sb.remove(((sb.getLength() - 2) | 0), 1);
                return sb.toString();
            },
            SetHomeUniform: function (team, colorString) {
                var $t, $t1, $t2, $t3, $t4;
                var loc = this.GetUniformLoc(team);
                var loc2 = this.GetActionSeqUniformLoc(team);
                var bytes = TSBTool.InputParser.GetBytesFromString(colorString);
                if (loc > -1 && loc2 > -1 && bytes != null && bytes.length > 2) {
                    var pantsColor = bytes[System.Array.index(0, bytes)];
                    var skinColor = bytes[System.Array.index(1, bytes)];
                    var jerseyColor = bytes[System.Array.index(2, bytes)];
                    ($t = this.OutputRom)[System.Array.index(loc, $t)] = pantsColor;
                    ($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)] = skinColor;
                    ($t2 = this.OutputRom)[System.Array.index(((loc + 2) | 0), $t2)] = jerseyColor;
                    ($t3 = this.OutputRom)[System.Array.index(loc2, $t3)] = pantsColor;
                    ($t4 = this.OutputRom)[System.Array.index(((loc2 + 1) | 0), $t4)] = jerseyColor;
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR setting Uniform1 for team {0},'{1}'", team, colorString));
                }
            },
            SetAwayUniform: function (team, colorString) {
                var $t, $t1, $t2, $t3, $t4;
                var loc = this.GetUniformLoc(team);
                var loc2 = this.GetActionSeqUniformLoc(team);

                var bytes = TSBTool.InputParser.GetBytesFromString(colorString);
                if (loc > -1 && loc2 > -1 && bytes != null && bytes.length > 2) {
                    var pantsColor = bytes[System.Array.index(0, bytes)];
                    var skinColor = bytes[System.Array.index(1, bytes)];
                    var jerseyColor = bytes[System.Array.index(2, bytes)];
                    ($t = this.OutputRom)[System.Array.index(((loc + 3) | 0), $t)] = pantsColor;
                    ($t1 = this.OutputRom)[System.Array.index(((loc + 4) | 0), $t1)] = skinColor;
                    ($t2 = this.OutputRom)[System.Array.index(((loc + 5) | 0), $t2)] = jerseyColor;
                    ($t3 = this.OutputRom)[System.Array.index(((loc2 + 2) | 0), $t3)] = pantsColor;
                    ($t4 = this.OutputRom)[System.Array.index(((loc2 + 3) | 0), $t4)] = jerseyColor;
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR setting Uniform2 for team {0},'{1}'", team, colorString));
                }
            },
            GetHomeUniform: function (team) {
                var $t, $t1, $t2;
                var ret = "";
                var loc = this.GetUniformLoc(team);
                if (loc > -1) {
                    ret = System.String.format("Uniform1=0x{0:x2}{1:x2}{2:x2}", Bridge.box(($t = this.OutputRom)[System.Array.index(loc, $t)], System.Byte), Bridge.box(($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)], System.Byte), Bridge.box(($t2 = this.OutputRom)[System.Array.index(((loc + 2) | 0), $t2)], System.Byte));
                }
                return ret;
            },
            GetAwayUniform: function (team) {
                var $t, $t1, $t2;
                var ret = "";
                var loc = this.GetUniformLoc(team);
                if (loc > -1) {
                    ret = System.String.format("Uniform2=0x{0:x2}{1:x2}{2:x2}", Bridge.box(($t = this.OutputRom)[System.Array.index(((loc + 3) | 0), $t)], System.Byte), Bridge.box(($t1 = this.OutputRom)[System.Array.index(((loc + 4) | 0), $t1)], System.Byte), Bridge.box(($t2 = this.OutputRom)[System.Array.index(((loc + 5) | 0), $t2)], System.Byte));
                }
                return ret;
            },
            /**
             * Gets the location of the given team's uniform data.
             *
             * @instance
             * @protected
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team
             * @return  {number}            The location of the given team's uniform data, -1 on error
             */
            GetUniformLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex > -1 && teamIndex < 28) {
                    ret = (this.BillsUniformLoc + (Bridge.Int.mul(teamIndex, 10))) | 0;
                }
                return ret;
            },
            /**
             * Gets the location of the given team's uniform data.
             *
             * @instance
             * @protected
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {string}    team
             * @return  {number}            The location of the given team's uniform data, -1 on error
             */
            GetActionSeqUniformLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex > -1 && teamIndex < 28) {
                    ret = (this.BillsActionSeqLoc + (Bridge.Int.mul(teamIndex, 8))) | 0;
                }
                return ret;
            },
            GetGameUniform: function (team) {
                var ret = "";
                ret = System.String.format("{0}, {1}", this.GetHomeUniform(team), this.GetAwayUniform(team));
                return ret;
            },
            GetDivChampLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex > -1 && teamIndex < 28) {
                    ret = (this.BillsDivChampLoc + (Bridge.Int.mul(teamIndex, 5))) | 0;
                }
                return ret;
            },
            GetConfChampLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex > -1 && teamIndex < 28) {
                    ret = (this.BillsConfChampLoc + (Bridge.Int.mul(teamIndex, 4))) | 0;
                }
                return ret;
            },
            SetDivChampColors: function (team, colorString) {
                var $t, $t1, $t2, $t3, $t4;
                var loc = this.GetDivChampLoc(team);
                var colorBytes = TSBTool.InputParser.GetBytesFromString(colorString);
                if (loc > -1 && colorBytes != null && colorBytes.length > 4) {
                    ($t = this.OutputRom)[System.Array.index(loc, $t)] = colorBytes[System.Array.index(0, colorBytes)];
                    ($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)] = colorBytes[System.Array.index(1, colorBytes)];
                    ($t2 = this.OutputRom)[System.Array.index(((loc + 2) | 0), $t2)] = colorBytes[System.Array.index(2, colorBytes)];
                    ($t3 = this.OutputRom)[System.Array.index(((loc + 3) | 0), $t3)] = colorBytes[System.Array.index(3, colorBytes)];
                    ($t4 = this.OutputRom)[System.Array.index(((loc + 4) | 0), $t4)] = colorBytes[System.Array.index(4, colorBytes)];
                }
            },
            GetDivChampColors: function (team) {
                var $t, $t1, $t2, $t3, $t4;
                var ret = "";
                var loc = this.GetDivChampLoc(team);
                if (loc > -1) {
                    ret = System.String.format("DivChamp=0x{0:x2}{1:x2}{2:x2}{3:x2}{4:x2}", Bridge.box(($t = this.OutputRom)[System.Array.index(loc, $t)], System.Byte), Bridge.box(($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)], System.Byte), Bridge.box(($t2 = this.OutputRom)[System.Array.index(((loc + 2) | 0), $t2)], System.Byte), Bridge.box(($t3 = this.OutputRom)[System.Array.index(((loc + 3) | 0), $t3)], System.Byte), Bridge.box(($t4 = this.OutputRom)[System.Array.index(((loc + 4) | 0), $t4)], System.Byte));
                }
                return ret;
            },
            SetConfChampColors: function (team, colorString) {
                var $t, $t1, $t2, $t3;
                var ret = "";
                var loc = this.GetConfChampLoc(team);
                var colorBytes = TSBTool.InputParser.GetBytesFromString(colorString);
                if (loc > -1 && colorBytes != null && colorBytes.length > 3) {
                    ($t = this.OutputRom)[System.Array.index(loc, $t)] = colorBytes[System.Array.index(3, colorBytes)];
                    ($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)] = colorBytes[System.Array.index(0, colorBytes)];
                    ($t2 = this.OutputRom)[System.Array.index(((loc + 2) | 0), $t2)] = colorBytes[System.Array.index(1, colorBytes)];
                    ($t3 = this.OutputRom)[System.Array.index(((loc + 3) | 0), $t3)] = colorBytes[System.Array.index(2, colorBytes)];
                }
            },
            GetChampColors: function (team) {
                var ret = System.String.format("{0}, {1}", this.GetDivChampColors(team), this.GetConfChampColors(team));
                return ret;
            },
            GetConfChampColors: function (team) {
                var $t, $t1, $t2, $t3;
                var ret = "";
                var loc = this.GetConfChampLoc(team);
                if (loc > -1) {
                    ret = System.String.format("ConfChamp=0x{0:x2}{1:x2}{2:x2}{3:x2}", Bridge.box(($t = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t)], System.Byte), Bridge.box(($t1 = this.OutputRom)[System.Array.index(((loc + 2) | 0), $t1)], System.Byte), Bridge.box(($t2 = this.OutputRom)[System.Array.index(((loc + 3) | 0), $t2)], System.Byte), Bridge.box(($t3 = this.OutputRom)[System.Array.index(loc, $t3)], System.Byte));
                }
                return ret;
            },
            GetUniformUsage: function (team) {
                var $t, $t1, $t2, $t3;
                var ret = "";
                var loc = (this.GetUniformLoc(team) + 6) | 0;
                if (loc > -1) {
                    ret = System.String.format("UniformUsage=0x{0:x2}{1:x2}{2:x2}{3:x2}", Bridge.box(($t = this.OutputRom)[System.Array.index(loc, $t)], System.Byte), Bridge.box(($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)], System.Byte), Bridge.box(($t2 = this.OutputRom)[System.Array.index(((loc + 2) | 0), $t2)], System.Byte), Bridge.box(($t3 = this.OutputRom)[System.Array.index(((loc + 3) | 0), $t3)], System.Byte));
                }
                return ret;
            },
            SetUniformUsage: function (team, usage) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7;
                var loc = (this.GetUniformLoc(team) + 6) | 0;
                var loc2 = (this.GetActionSeqUniformLoc(team) + 4) | 0;
                var colorBytes = TSBTool.InputParser.GetBytesFromString(usage);
                if (loc > -1 && colorBytes != null && colorBytes.length > 3) {
                    ($t = this.OutputRom)[System.Array.index(loc, $t)] = colorBytes[System.Array.index(0, colorBytes)];
                    ($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)] = colorBytes[System.Array.index(1, colorBytes)];
                    ($t2 = this.OutputRom)[System.Array.index(((loc + 2) | 0), $t2)] = colorBytes[System.Array.index(2, colorBytes)];
                    ($t3 = this.OutputRom)[System.Array.index(((loc + 3) | 0), $t3)] = colorBytes[System.Array.index(3, colorBytes)];

                    ($t4 = this.OutputRom)[System.Array.index(loc2, $t4)] = colorBytes[System.Array.index(0, colorBytes)];
                    ($t5 = this.OutputRom)[System.Array.index(((loc2 + 1) | 0), $t5)] = colorBytes[System.Array.index(1, colorBytes)];
                    ($t6 = this.OutputRom)[System.Array.index(((loc2 + 2) | 0), $t6)] = colorBytes[System.Array.index(2, colorBytes)];
                    ($t7 = this.OutputRom)[System.Array.index(((loc2 + 3) | 0), $t7)] = colorBytes[System.Array.index(3, colorBytes)];
                }
            },
            SetReturnTeam: function (team, pos0, pos1, pos2) { },
            
            SetProBowlPlayer: function (conf, proBowlPos, fromTeam, fromTeamPos) {
                var $t, $t1;
                var offset = 0;
                if (conf === TSBTool.Conference.NFC) {
                    offset = (offset + 60) | 0;
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(fromTeam);
                if (teamIndex < 0) {
                    throw new System.Exception(System.String.format("Error, team '{0}' is invalid", [fromTeam]));
                }
                var val1 = teamIndex & 255;
                var val2 = fromTeamPos & 255;

                var posIndex = this.GetPositionIndex(proBowlPos);
                var loc = (((this.mProwbowlStartingLoc + offset) | 0) + (Bridge.Int.mul(2, posIndex))) | 0;
                ($t = this.OutputRom)[System.Array.index(loc, $t)] = val1;
                ($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)] = val2;
            },
            /**
             * @instance
             * @public
             * @this TSBTool.TecmoTool
             * @memberof TSBTool.TecmoTool
             * @param   {TSBTool.Conference}    conf          
             * @param   {string}                proBowlPos
             * @return  {string}
             */
            GetProBowlPlayer: function (conf, proBowlPos) {
                var $t, $t1;
                var ret = "";
                var offset = 0;
                if (conf === TSBTool.Conference.NFC) {
                    offset = (offset + 60) | 0;
                }
                var loc = (((this.mProwbowlStartingLoc + offset) | 0) + (Bridge.Int.mul(2, this.GetPositionIndex(proBowlPos)))) | 0;
                var teamIndex = ($t = this.OutputRom)[System.Array.index(loc, $t)];
                var pos = ($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)];

                var team = TSBTool.TecmoTool.Teams.getItem(teamIndex);
                ret = System.String.format("{0},{1},{2},{3}", System.Enum.toString(TSBTool.Conference, conf), Bridge.toString(proBowlPos), team, System.Enum.toString(TSBTool.TSBPlayer, pos));

                return ret;
            },
            GetConferenceProBowlPlayers: function (conf) {
                var builder = new System.Text.StringBuilder("", 500);
                for (var i = 0; i < TSBTool.TecmoTool.positionNames.Count; i = (i + 1) | 0) {
                    builder.append(this.GetProBowlPlayer(conf, TSBTool.TecmoTool.positionNames.getItem(i)));
                    builder.append("\r\n");
                }
                return builder.toString();
            },
            ProcessText: function (text) {
                var parser = new TSBTool.InputParser.$ctor1(this);
                text = System.String.replaceAll(text, "\r\n", "\n");
                var lines = System.String.split(text, System.String.toCharArray(("\n"), 0, ("\n").length).map(function (i) {{ return String.fromCharCode(i); }}));
                parser.ProcessLines(lines);
            }
        }
    });

    /**
     * Summary description for SNES_TecmoTool.
     Location = pointer - 0x8000 + 0x0010;
     Where pointer is of the 'swapped' format like '0x86dd'
     *
     * @public
     * @class TSBTool.SNES_TecmoTool
     * @implements  TSBTool.ITecmoTool
     * @implements  TSBTool.ITecmoContent
     */
    Bridge.define("TSBTool.SNES_TecmoTool", {
        inherits: [TSBTool.ITecmoTool,TSBTool.ITecmoContent],
        statics: {
            fields: {
                nameNumberSegmentEnd: 0,
                namePointersStart: 0,
                playerNumberNameDataStart: 0,
                teamSimOffensivePrefStart: 0,
                pr_kr_start_offset: 0,
                pr_kr_team_start_offset: 0,
                lastPointer: 0,
                ROM_LENGTH: 0,
                billsQB1SimLoc: 0,
                billsRESimLoc: 0,
                billsTeamSimLoc: 0,
                teamSimOffset: 0,
                billsQB1AbilityStart: 0,
                teamAbilityOffset: 0,
                QUARTER_LENGTH: 0,
                m2RB_2WR_1TE: null,
                m1RB_3WR_1TE: null,
                m1RB_4WR: null,
                mPlaybookStartLoc: 0,
                JUICE_LOCATION: 0,
                mRaceCutsceneStartPos: 0,
                GUI_MODE: false,
                AUTO_CORRECT_SCHEDULE: false,
                teams: null,
                mSimTeams: null
            },
            ctors: {
                init: function () {
                    this.nameNumberSegmentEnd = 1554416;
                    this.namePointersStart = 1540152;
                    this.playerNumberNameDataStart = 1541946;
                    this.teamSimOffensivePrefStart = 89594;
                    this.pr_kr_start_offset = 1510544;
                    this.pr_kr_team_start_offset = 1510576;
                    this.lastPointer = 1541944;
                    this.ROM_LENGTH = 1572864;
                    this.billsQB1SimLoc = 158415;
                    this.billsRESimLoc = 158439;
                    this.billsTeamSimLoc = 158462;
                    this.teamSimOffset = 48;
                    this.billsQB1AbilityStart = 1564672;
                    this.teamAbilityOffset = 125;
                    this.QUARTER_LENGTH = 41198;
                    this.m2RB_2WR_1TE = "2RB_2WR_1TE";
                    this.m1RB_3WR_1TE = "1RB_3WR_1TE";
                    this.m1RB_4WR = "1RB_4WR";
                    this.mPlaybookStartLoc = 1510704;
                    this.JUICE_LOCATION = 158106;
                    this.mRaceCutsceneStartPos = 6127;
                    this.GUI_MODE = false;
                    this.AUTO_CORRECT_SCHEDULE = true;
                    this.teams = System.Array.init([
                        "bills", 
                        "colts", 
                        "dolphins", 
                        "patriots", 
                        "jets", 
                        "bengals", 
                        "browns", 
                        "oilers", 
                        "steelers", 
                        "broncos", 
                        "chiefs", 
                        "raiders", 
                        "chargers", 
                        "seahawks", 
                        "cowboys", 
                        "giants", 
                        "eagles", 
                        "cardinals", 
                        "redskins", 
                        "bears", 
                        "lions", 
                        "packers", 
                        "vikings", 
                        "buccaneers", 
                        "falcons", 
                        "rams", 
                        "saints", 
                        "49ers"
                    ], System.String);
                    this.mSimTeams = System.Array.init([
                        "bills", 
                        "colts", 
                        "dolphins", 
                        "patriots", 
                        "jets", 
                        "bengals", 
                        "browns", 
                        "oilers", 
                        "steelers", 
                        "broncos", 
                        "chiefs", 
                        "raiders", 
                        "chargers", 
                        "seahawks", 
                        "redskins", 
                        "giants", 
                        "eagles", 
                        "cardinals", 
                        "cowboys", 
                        "bears", 
                        "lions", 
                        "packers", 
                        "vikings", 
                        "buccaneers", 
                        "49ers", 
                        "rams", 
                        "saints", 
                        "falcons"
                    ], System.String);
                }
            },
            methods: {
                GetTeamIndex: function (teamName) {
                    var ret = -1;
                    if (Bridge.referenceEquals(teamName.toLowerCase(), "null")) {
                        return 255;
                    }
                    for (var i = 0; i < TSBTool.SNES_TecmoTool.teams.length; i = (i + 1) | 0) {
                        if (Bridge.referenceEquals(TSBTool.SNES_TecmoTool.teams[System.Array.index(i, TSBTool.SNES_TecmoTool.teams)], teamName)) {
                            ret = i;
                            break;
                        }
                    }
                    return ret;
                },
                GetSimTeamIndex: function (teamName) {
                    var ret = -1;
                    if (Bridge.referenceEquals(teamName.toLowerCase(), "null")) {
                        return 255;
                    }
                    for (var i = 0; i < TSBTool.SNES_TecmoTool.mSimTeams.length; i = (i + 1) | 0) {
                        if (Bridge.referenceEquals(TSBTool.SNES_TecmoTool.mSimTeams[System.Array.index(i, TSBTool.SNES_TecmoTool.mSimTeams)], teamName)) {
                            ret = i;
                            break;
                        }
                    }
                    return ret;
                },
                /**
                 * Returns the team specified by the index passed. (0= bills).
                 *
                 * @static
                 * @public
                 * @this TSBTool.SNES_TecmoTool
                 * @memberof TSBTool.SNES_TecmoTool
                 * @param   {number}    index
                 * @return  {string}             team name on success, null on failure
                 */
                GetTeamFromIndex: function (index) {
                    if (index === 255) {
                        return "null";
                    }
                    if (index < 0 || index > ((TSBTool.SNES_TecmoTool.teams.length - 1) | 0)) {
                        return null;
                    }
                    return TSBTool.SNES_TecmoTool.teams[System.Array.index(index, TSBTool.SNES_TecmoTool.teams)];
                }
            }
        },
        fields: {
            outputRom: null,
            dataPositionOffset: 0,
            mShowOffPref: false,
            abilityOffsets: null,
            gameYearLocations: null,
            positionNames: null,
            mTeamFormationsStartingLoc: 0,
            runRegex: null,
            passRegex: null,
            m_JuiceArray: null,
            mProwbowlStartingLoc: 0,
            simpleSetRegex: null,
            mBillsUniformLoc: 0
        },
        props: {
            /**
             * Returns the rom version
             *
             * @instance
             * @public
             * @readonly
             * @memberof TSBTool.SNES_TecmoTool
             * @function RomVersion
             * @type TSBTool.ROM_TYPE
             */
            RomVersion: {
                get: function () {
                    return TSBTool.ROM_TYPE.SNES_TSB1;
                }
            },
            OutputRom: {
                get: function () {
                    return this.outputRom;
                },
                set: function (value) {
                    this.outputRom = value;
                }
            },
            ShowOffPref: {
                get: function () {
                    return this.mShowOffPref;
                },
                set: function (value) {
                    this.mShowOffPref = value;
                }
            },
            NumberOfStringsInTeamStringTable: {
                get: function () {
                    return 30;
                }
            },
            BillsUniformLoc: {
                get: function () {
                    return this.mBillsUniformLoc;
                },
                set: function (value) {
                    this.mBillsUniformLoc = value;
                }
            }
        },
        alias: [
            "RomVersion", "TSBTool$ITecmoContent$RomVersion",
            "RomVersion", "TSBTool$ITecmoTool$RomVersion",
            "OutputRom", "TSBTool$ITecmoContent$OutputRom",
            "OutputRom", "TSBTool$ITecmoTool$OutputRom",
            "ShowOffPref", "TSBTool$ITecmoContent$ShowOffPref",
            "ShowOffPref", "TSBTool$ITecmoTool$ShowOffPref",
            "IsValidPosition", "TSBTool$ITecmoTool$IsValidPosition",
            "SetByte", "TSBTool$ITecmoContent$SetByte",
            "SaveRom", "TSBTool$ITecmoContent$SaveRom",
            "SaveRom", "TSBTool$ITecmoTool$SaveRom",
            "GetPlayerStuff", "TSBTool$ITecmoTool$GetPlayerStuff",
            "GetSchedule", "TSBTool$ITecmoTool$GetSchedule",
            "SetYear", "TSBTool$ITecmoTool$SetYear",
            "InsertPlayer", "TSBTool$ITecmoTool$InsertPlayer",
            "GetKey", "TSBTool$ITecmoContent$GetKey",
            "GetKey", "TSBTool$ITecmoTool$GetKey",
            "GetTeamPlayers", "TSBTool$ITecmoTool$GetTeamPlayers",
            "NumberOfStringsInTeamStringTable", "TSBTool$ITecmoTool$NumberOfStringsInTeamStringTable",
            "SetTeamStringTableString", "TSBTool$ITecmoTool$SetTeamStringTableString",
            "GetTeamStringTableString", "TSBTool$ITecmoTool$GetTeamStringTableString",
            "GetTeamName", "TSBTool$ITecmoTool$GetTeamName",
            "GetTeamCity", "TSBTool$ITecmoTool$GetTeamCity",
            "GetTeamAbbreviation", "TSBTool$ITecmoTool$GetTeamAbbreviation",
            "SetTeamAbbreviation", "TSBTool$ITecmoTool$SetTeamAbbreviation",
            "SetTeamName", "TSBTool$ITecmoTool$SetTeamName",
            "SetTeamCity", "TSBTool$ITecmoTool$SetTeamCity",
            "GetAll$1", "TSBTool$ITecmoContent$GetAll",
            "GetProBowlPlayers$1", "TSBTool$ITecmoContent$GetProBowlPlayers",
            "GetSchedule$1", "TSBTool$ITecmoContent$GetSchedule",
            "GetAll", "TSBTool$ITecmoTool$GetAll",
            "SetQBAbilities", "TSBTool$ITecmoTool$SetQBAbilities",
            "SetSkillPlayerAbilities", "TSBTool$ITecmoTool$SetSkillPlayerAbilities",
            "SetKickPlayerAbilities", "TSBTool$ITecmoTool$SetKickPlayerAbilities",
            "SetDefensivePlayerAbilities", "TSBTool$ITecmoTool$SetDefensivePlayerAbilities",
            "SetOLPlayerAbilities", "TSBTool$ITecmoTool$SetOLPlayerAbilities",
            "SetTeamSimData", "TSBTool$ITecmoTool$SetTeamSimData",
            "SetTeamSimOffensePref", "TSBTool$ITecmoTool$SetTeamSimOffensePref",
            "SetTeamOffensiveFormation", "TSBTool$ITecmoTool$SetTeamOffensiveFormation",
            "SetPlaybook", "TSBTool$ITecmoTool$SetPlaybook",
            "ApplyJuice", "TSBTool$ITecmoTool$ApplyJuice",
            "SetKickingSimData", "TSBTool$ITecmoTool$SetKickingSimData",
            "SetPuntingSimData", "TSBTool$ITecmoTool$SetPuntingSimData",
            "SetDefensiveSimData", "TSBTool$ITecmoTool$SetDefensiveSimData",
            "SetSkillSimData", "TSBTool$ITecmoTool$SetSkillSimData",
            "SetQBSimData", "TSBTool$ITecmoTool$SetQBSimData",
            "SetFace", "TSBTool$ITecmoTool$SetFace",
            "SetReturnTeam", "TSBTool$ITecmoTool$SetReturnTeam",
            "SetPuntReturner", "TSBTool$ITecmoTool$SetPuntReturner",
            "SetKickReturner", "TSBTool$ITecmoTool$SetKickReturner",
            "SetProBowlPlayer", "TSBTool$ITecmoTool$SetProBowlPlayer",
            "ApplySet", "TSBTool$ITecmoContent$ApplySet",
            "ApplySet", "TSBTool$ITecmoTool$ApplySet",
            "ApplySchedule", "TSBTool$ITecmoTool$ApplySchedule",
            "SetHomeUniform", "TSBTool$ITecmoTool$SetHomeUniform",
            "SetAwayUniform", "TSBTool$ITecmoTool$SetAwayUniform",
            "GetGameUniform", "TSBTool$ITecmoTool$GetGameUniform",
            "SetDivChampColors", "TSBTool$ITecmoTool$SetDivChampColors",
            "SetUniformUsage", "TSBTool$ITecmoTool$SetUniformUsage",
            "GetUniformUsage", "TSBTool$ITecmoTool$GetUniformUsage",
            "SetConfChampColors", "TSBTool$ITecmoTool$SetConfChampColors",
            "GetDivChampColors", "TSBTool$ITecmoTool$GetDivChampColors",
            "GetConfChampColors", "TSBTool$ITecmoTool$GetConfChampColors",
            "GetChampColors", "TSBTool$ITecmoTool$GetChampColors",
            "GetProBowlPlayers", "TSBTool$ITecmoTool$GetProBowlPlayers",
            "ProcessText", "TSBTool$ITecmoContent$ProcessText",
            "ProcessText", "TSBTool$ITecmoTool$ProcessText"
        ],
        ctors: {
            init: function () {
                this.dataPositionOffset = 1507328;
                this.mShowOffPref = true;
                this.abilityOffsets = System.Array.init([
                    0, 
                    5, 
                    10, 
                    14, 
                    18, 
                    22, 
                    26, 
                    30, 
                    34, 
                    38, 
                    42, 
                    46, 
                    50, 
                    53, 
                    56, 
                    59, 
                    62, 
                    65, 
                    69, 
                    73, 
                    77, 
                    81, 
                    85, 
                    89, 
                    93, 
                    97, 
                    101, 
                    105, 
                    109, 
                    113, 
                    117, 
                    121
                ], System.Int32);
                this.gameYearLocations = System.Array.init([
                    188779, 
                    1192606, 
                    1192776, 
                    1511529, 
                    1512187, 
                    1512252, 
                    1518724, 
                    1518850, 
                    1518910, 
                    1518939, 
                    1518879
                ], System.Int32);
                this.positionNames = System.Array.init([
                    "QB1", 
                    "QB2", 
                    "RB1", 
                    "RB2", 
                    "RB3", 
                    "RB4", 
                    "WR1", 
                    "WR2", 
                    "WR3", 
                    "WR4", 
                    "TE1", 
                    "TE2", 
                    "C", 
                    "LG", 
                    "RG", 
                    "LT", 
                    "RT", 
                    "RE", 
                    "NT", 
                    "LE", 
                    "ROLB", 
                    "RILB", 
                    "LILB", 
                    "LOLB", 
                    "RCB", 
                    "LCB", 
                    "FS", 
                    "SS", 
                    "K", 
                    "P", 
                    "DB1", 
                    "DB2"
                ], System.String);
                this.mTeamFormationsStartingLoc = 60915;
                this.m_JuiceArray = System.Array.init([
                    0, 
                    1, 
                    0, 
                    0, 
                    0, 
                    1, 
                    2, 
                    1, 
                    1, 
                    1, 
                    1, 
                    2, 
                    1, 
                    2, 
                    2, 
                    1, 
                    2, 
                    1, 
                    3, 
                    2, 
                    2, 
                    2, 
                    2, 
                    3, 
                    3, 
                    2, 
                    2, 
                    2, 
                    4, 
                    3, 
                    2, 
                    2, 
                    2, 
                    4, 
                    4, 
                    2, 
                    2, 
                    2, 
                    5, 
                    4, 
                    2, 
                    2, 
                    3, 
                    5, 
                    5, 
                    2, 
                    2, 
                    3, 
                    6, 
                    5, 
                    2, 
                    2, 
                    4, 
                    6, 
                    6, 
                    3, 
                    2, 
                    4, 
                    7, 
                    6, 
                    3, 
                    3, 
                    4, 
                    7, 
                    7, 
                    3, 
                    3, 
                    5, 
                    8, 
                    7, 
                    3, 
                    3, 
                    5, 
                    8, 
                    8, 
                    3, 
                    3, 
                    5, 
                    9, 
                    8, 
                    3, 
                    4, 
                    6, 
                    9, 
                    9
                ], System.Byte);
                this.mProwbowlStartingLoc = 1510400;
                this.mBillsUniformLoc = 180964;
            },
            ctor: function (rom) {
                this.$initialize();
                this.Init(rom);
            }
        },
        methods: {
            GetTeams: function () {
                return TSBTool.SNES_TecmoTool.teams;
            },
            GetPositionNames: function () {
                return this.positionNames;
            },
            IsValidPosition: function (pos) {
                var ret = false;
                for (var i = 0; i < this.positionNames.length; i = (i + 1) | 0) {
                    if (Bridge.referenceEquals(pos, this.positionNames[System.Array.index(i, this.positionNames)])) {
                        ret = true;
                        break;
                    }
                }
                return ret;
            },
            IsValidTeam: function (team) {
                var ret = false;
                for (var i = 0; i < TSBTool.SNES_TecmoTool.teams.length; i = (i + 1) | 0) {
                    if (Bridge.referenceEquals(team, TSBTool.SNES_TecmoTool.teams[System.Array.index(i, TSBTool.SNES_TecmoTool.teams)])) {
                        ret = true;
                        break;
                    }
                }
                return ret;
            },
            SetByte: function (location, b) {
                var $t;
                ($t = this.OutputRom)[System.Array.index(location, $t)] = b;
            },
            Init: function (rom) {
                return this.ReadRom(rom);
            },
            Test2: function () {
                var team = "bills";
                for (var i = 0; i < this.positionNames.length; i = (i + 1) | 0) {
                    this.InsertPlayer(team, this.positionNames[System.Array.index(i, this.positionNames)], "player", team, ((i % 10) & 255));
                    switch (this.positionNames[System.Array.index(i, this.positionNames)]) {
                        case "QB1": 
                        case "QB2": 
                            this.SetQBAbilities(team, this.positionNames[System.Array.index(i, this.positionNames)], 31, 31, 31, 31, 31, 31, 31, 31);
                            break;
                        case "RB1": 
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                        case "WR1": 
                        case "WR2": 
                        case "WR3": 
                        case "WR4": 
                        case "TE1": 
                        case "TE2": 
                            this.SetSkillPlayerAbilities(team, this.positionNames[System.Array.index(i, this.positionNames)], 31, 31, 31, 31, 31, 31);
                            break;
                        case "C": 
                        case "RG": 
                        case "LG": 
                        case "RT": 
                        case "LT": 
                            this.SetOLPlayerAbilities(team, this.positionNames[System.Array.index(i, this.positionNames)], 31, 31, 31, 31);
                            break;
                        case "RE": 
                        case "NT": 
                        case "LE": 
                        case "LOLB": 
                        case "LILB": 
                        case "RILB": 
                        case "ROLB": 
                        case "RCB": 
                        case "LCB": 
                        case "FS": 
                        case "SS": 
                            this.SetDefensivePlayerAbilities(team, this.positionNames[System.Array.index(i, this.positionNames)], 31, 31, 31, 31, 31, 31);
                            break;
                        case "K": 
                        case "P": 
                            this.SetKickPlayerAbilities(team, this.positionNames[System.Array.index(i, this.positionNames)], 31, 31, 31, 31, 31, 31);
                            break;
                    }
                }
            },
            shiftTest: function () {
                var stuff = System.Array.init([
                    255, 
                    255, 
                    255, 
                    255, 
                    255, 
                    74, 
                    76, 
                    78, 
                    80, 
                    82, 
                    84, 
                    86, 
                    88, 
                    90, 
                    92, 
                    94, 
                    96, 
                    98, 
                    100, 
                    102, 
                    104, 
                    106, 
                    108, 
                    110, 
                    112, 
                    114, 
                    255, 
                    255, 
                    255, 
                    255, 
                    255
                ], System.Byte);
                for (var i = 0; i < stuff.length; i = (i + 1) | 0) {
                    System.Console.Write(System.String.format(" {0:x} ", Bridge.box(stuff[System.Array.index(i, stuff)], System.Byte)));
                }
                System.Console.WriteLine();
                System.Console.WriteLine("shift 3");
                this.ShiftDataDown(6, ((stuff.length - 7) | 0), 3, stuff);
                for (var i1 = 0; i1 < stuff.length; i1 = (i1 + 1) | 0) {
                    System.Console.Write(System.String.format(" {0:x} ", Bridge.box(stuff[System.Array.index(i1, stuff)], System.Byte)));
                }
                System.Console.WriteLine();

            },
            ReadRom: function (rom) {
                var ret = false;
                try {
                    var result = System.Windows.Forms.DialogResult.Yes;
                    var len = System.Int64(rom.length);
                    if (len.ne(System.Int64(TSBTool.SNES_TecmoTool.ROM_LENGTH))) {
                        if (TSBTool.MainClass.GUI_MODE) {
                            result = System.Windows.Forms.MessageBox.Show(null, System.String.format("Warning! \r\n\r\nThe input Rom is not the correct Size ({0} bytes).\r\n\r\nYou should only continue if you know for sure that you are loading a snes TSB1 ROM.\r\n\r\nDo you want to continue?", [Bridge.box(TSBTool.SNES_TecmoTool.ROM_LENGTH, System.Int32)]), "WARNING!", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                        } else {
                            var msg = System.String.format("ERROR! ROM '{0}' is not the correct length.  \r\n    Legit TSB1 snes ROMS are {1} bytes long.\r\n    If you know this is really a snes TSB1 ROM, you can force TSBToolSupreme to load it in GUI mode.", "<filename>", Bridge.box(TSBTool.SNES_TecmoTool.ROM_LENGTH, System.Int32));
                            TSBTool.StaticUtils.AddError(msg);
                        }
                    }

                    if (result === System.Windows.Forms.DialogResult.Yes) {
                        this.outputRom = rom;
                        ret = true;
                    }
                } catch (e) {
                    e = System.Exception.create(e);
                    TSBTool.StaticUtils.ShowError(Bridge.toString(e));
                }
                return ret;
            },
            SaveRom: function (filename) {
                if (filename != null) {
                    try {
                        var len = System.Int64(this.outputRom.length);
                        var s1 = new System.IO.FileStream.$ctor1(filename, 4);
                        s1.Write(this.outputRom, 0, System.Int64.clip32(len));
                        s1.Close();
                    } catch (e) {
                        e = System.Exception.create(e);
                        TSBTool.StaticUtils.ShowError(Bridge.toString(e));
                    }
                } else {
                    TSBTool.StaticUtils.AddError("ERROR! You passed a null filename");
                }
            },
            /**
             * Returns a string consisting of number, name\n for all players in the game.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {boolean}    jerseyNumber_b    
             * @param   {boolean}    name_b            
             * @param   {boolean}    face_b            
             * @param   {boolean}    abilities_b       
             * @param   {boolean}    simData_b
             * @return  {string}
             */
            GetPlayerStuff: function (jerseyNumber_b, name_b, face_b, abilities_b, simData_b) {
                var sb = new System.Text.StringBuilder("", 40320);
                var team = "";
                for (var i = 0; i < TSBTool.SNES_TecmoTool.teams.length; i = (i + 1) | 0) {
                    team = TSBTool.SNES_TecmoTool.teams[System.Array.index(i, TSBTool.SNES_TecmoTool.teams)];
                    sb.append(System.String.format("TEAM={0}\n", [team]));
                    for (var j = 0; j < this.positionNames.length; j = (j + 1) | 0) {
                        sb.append((this.GetPlayerData(team, this.positionNames[System.Array.index(j, this.positionNames)], abilities_b, jerseyNumber_b, face_b, name_b, simData_b) || "") + "\n");
                    }
                }
                return sb.toString();
            },
            GetSchedule: function () {
                var ret = "";
                if (this.outputRom != null) {
                    var sh2 = new TSBTool.SNES_ScheduleHelper(this.outputRom);
                    ret = sh2.GetSchedule();
                    TSBTool.StaticUtils.ShowErrors();
                }
                return ret;
            },
            GetSchedule$1: function (season) {
                return this.GetSchedule();
            },
            SetYear: function (year) {
                if (year == null || year.length !== 4) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) {0} is not a valid year.", [year]));
                    return;
                }
                var location;
                for (var i = 0; i < this.gameYearLocations.length; i = (i + 1) | 0) {
                    location = this.gameYearLocations[System.Array.index(i, this.gameYearLocations)];
                    this.outputRom[System.Array.index(location, this.outputRom)] = (year.charCodeAt(0)) & 255;
                    this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = (year.charCodeAt(1)) & 255;
                    this.outputRom[System.Array.index(((location + 2) | 0), this.outputRom)] = (year.charCodeAt(2)) & 255;
                    this.outputRom[System.Array.index(((location + 3) | 0), this.outputRom)] = (year.charCodeAt(3)) & 255;
                }
                if (!Bridge.referenceEquals(year, "1993")) {
                    this.outputRom[System.Array.index(1518850, this.outputRom)] = 32;
                    this.outputRom[System.Array.index(1518851, this.outputRom)] = 32;
                    this.outputRom[System.Array.index(1518852, this.outputRom)] = 32;
                    this.outputRom[System.Array.index(1518853, this.outputRom)] = 32;
                }
                try {
                    var theYear = System.Int32.parse(year);
                    var superbowlNumber = (theYear - 1965) | 0;
                    if (superbowlNumber < 0) {
                        superbowlNumber = 0;
                    }

                    var sbw;

                    var suffix = "TH";
                    var test = superbowlNumber % 10;

                    switch (test) {
                        case 1: 
                            suffix = "ST";
                            break;
                        case 2: 
                            suffix = "ND";
                            break;
                        case 3: 
                            suffix = "RD";
                            break;
                    }
                    if (superbowlNumber < 10) {
                        sbw = " " + superbowlNumber + (suffix || "");
                    } else if (superbowlNumber < 21) {
                        sbw = " " + superbowlNumber + "TH";
                    } else {
                        sbw = superbowlNumber + (suffix || "");
                    }


                    this.outputRom[System.Array.index(188819, this.outputRom)] = (sbw.charCodeAt(0)) & 255;
                    this.outputRom[System.Array.index(188820, this.outputRom)] = (sbw.charCodeAt(1)) & 255;
                    this.outputRom[System.Array.index(188821, this.outputRom)] = (sbw.charCodeAt(2)) & 255;
                    this.outputRom[System.Array.index(188822, this.outputRom)] = (sbw.charCodeAt(3)) & 255;
                } catch ($e1) {
                    $e1 = System.Exception.create($e1);
                    TSBTool.StaticUtils.AddError("Problem setting superbowl number.");
                }
            },
            GetYear: function () {
                var location = this.gameYearLocations[System.Array.index(0, this.gameYearLocations)];
                var ret = "";
                for (var i = location; i < ((location + 4) | 0); i = (i + 1) | 0) {
                    ret = (ret || "") + String.fromCharCode(this.outputRom[System.Array.index(i, this.outputRom)]);
                }

                return ret;
            },
            InsertPlayer: function (team, position, fname, lname, number) {
                if (!this.IsValidPosition(position) || fname == null || lname == null || fname.length < 1 || lname.length < 1) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) InsertPlayer:: Player name or position invalid", null));
                } else {
                    fname = fname.toLowerCase();
                    lname = lname.toUpperCase();
                    if (((lname.length + fname.length) | 0) > 17) {
                        TSBTool.StaticUtils.AddError(System.String.format("Warning!! There is a 17 character limit for names\n '{0} {1}' is {2} characters long.", fname, lname, Bridge.box(((fname.length + lname.length) | 0), System.Int32)));
                        if (lname.length > 16) {
                            lname = lname.substr(0, 12);
                            fname = System.String.format("{0}.", [Bridge.box(fname.charCodeAt(0), System.Char, String.fromCharCode, System.Char.getHashCode)]);
                        } else {
                            fname = System.String.format("{0}.", [Bridge.box(fname.charCodeAt(0), System.Char, String.fromCharCode, System.Char.getHashCode)]);
                        }

                        TSBTool.StaticUtils.AddError(System.String.format("Name will be {0} {1}", fname, lname));
                    }
                    if (fname.length < 1) {
                        fname = "Joe";
                    }
                    if (lname.length < 1) {
                        lname = "Nobody";
                    }

                    var oldName = this.GetName(team, position);
                    var bytes = System.Array.init(((((1 + fname.length) | 0) + lname.length) | 0), 0, System.Byte);
                    var change = (bytes.length - oldName.length) | 0;
                    var i = 0;
                    bytes[System.Array.index(0, bytes)] = number;
                    for (i = 1; i < ((fname.length + 1) | 0); i = (i + 1) | 0) {
                        bytes[System.Array.index(i, bytes)] = (fname.charCodeAt(((i - 1) | 0))) & 255;
                    }
                    for (var j = 0; j < lname.length; j = (j + 1) | 0) {
                        bytes[System.Array.index(Bridge.identity(i, ((i = (i + 1) | 0))), bytes)] = (lname.charCodeAt(j)) & 255;
                    }
                    var pos = this.GetPointerPosition(team, position);

                    this.UpdatePlayerData(team, position, bytes, change);
                    this.AdjustDataPointers(pos, change, TSBTool.SNES_TecmoTool.lastPointer);
                }
            },
            AdjustDataPointers: function (pos, change, lastPointerLocation) {
                var low, hi;
                var word;

                var i = 0;
                var end = (lastPointerLocation + 1) | 0;
                for (i = (pos + 2) | 0; i < end; i = (i + 2) | 0) {
                    low = this.outputRom[System.Array.index(i, this.outputRom)];
                    hi = this.outputRom[System.Array.index(((i + 1) | 0), this.outputRom)];
                    word = hi;
                    word = word << 8;
                    word = (word + low) | 0;
                    word = (word + change) | 0;
                    low = (word & 255) & 255;
                    word = word >> 8;
                    hi = word & 255;
                    this.outputRom[System.Array.index(i, this.outputRom)] = low;
                    this.outputRom[System.Array.index(((i + 1) | 0), this.outputRom)] = hi;
                }
            },
            /**
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team        The team the player is assigned to.
             * @param   {string}    position    The player's position ('QB1', 'WR1' ...)
             * @return  {string}
             */
            GetName: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetName:: team '{0}' or position '{1}' is invalid.", team, position));
                    return null;
                }
                var pos = this.GetDataPosition(team, position);
                var nextPos = this.GetNextDataPosition(team, position);
                var name = "";

                if (pos < 0) {
                    return "ERROR!";
                }
                if (nextPos > 0) {
                    for (var i = (pos + 1) | 0; i < nextPos; i = (i + 1) | 0) {
                        name = (name || "") + String.fromCharCode(this.outputRom[System.Array.index(i, this.outputRom)]);
                    }
                } else {
                    for (var i1 = (pos + 1) | 0; this.outputRom[System.Array.index(i1, this.outputRom)] !== 255; i1 = (i1 + 1) | 0) {
                        name = (name || "") + String.fromCharCode(this.outputRom[System.Array.index(i1, this.outputRom)]);
                    }
                }
                var split = 1;
                for (var i2 = 0; i2 < name.length; i2 = (i2 + 1) | 0) {
                    if (((name.charCodeAt(i2)) & 255) > 64 && ((name.charCodeAt(i2)) & 255) < 91) {
                        split = i2;
                        break;
                    }
                }

                var first, last, full;
                full = null;
                try {
                    first = name.substr(0, split);
                    last = name.substr(split);
                    full = (first || "") + " " + (last || "");
                } catch ($e1) {
                    $e1 = System.Exception.create($e1);
                    return full;
                }
                return full;
            },
            GetPlayerData: function (team, position, ability_b, jerseyNumber_b, face_b, name_b, simData_b) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Team {0} is invalid.", [team]));
                    return null;
                } else if (!this.IsValidPosition(position)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) position {0} is invalid.", [position]));
                    return null;
                }

                var result = new System.Text.StringBuilder();

                result.append(System.String.format("{0}, ", [position]));
                if (name_b) {
                    result.append(System.String.format("{0}, ", [this.GetName(team, position)]));
                }
                if (face_b) {
                    result.append(System.String.format("Face=0x{0:x}, ", [Bridge.box(this.GetFace(team, position), System.Int32)]));
                }
                var location = this.GetDataPosition(team, position);

                if (location < 0) {
                    return "Messed Up Pointer";
                }

                var jerseyNumber = System.String.format("#{0:x}, ", [Bridge.box(this.outputRom[System.Array.index(location, this.outputRom)], System.Byte)]);
                if (jerseyNumber_b) {
                    result.append(jerseyNumber);
                }
                if (ability_b) {
                    result.append(this.GetAbilityString(team, position));
                }
                var simData = this.GetPlayerSimData(team, position);
                if (simData != null && simData_b) {
                    result.append(System.String.format(",[{0}]", [this.StringifyArray(simData)]));
                }
                return result.toString();
            },
            GetKey: function () {
                return System.String.format("# TSBTool Forum: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/\r\n# Editing: Tecmo Super Bowl I (snes) [{0}]\r\n# \r\n# Double click on a team name (or playbook) to bring up the edit Team GUI.\r\n# Double click on a player to bring up the edit player GUI (Click 'Sim Data'\r\n#   button to find out more on Sim Data). \r\n# Key\r\n# -- Quarterbacks:\r\n# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, PS, PC, PA, APB, [Sim rush, Sim pass, Sim Pocket].\r\n# -- Offensive Skill players (non-QB):\r\n# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, BC, REC, [Sim rush, Sim catch, Sim punt Ret, Sim kick ret].\r\n# -- Offensive Linemen:\r\n# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP\r\n# -- Defensive Players:\r\n# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, PI, QU, [Sim pass rush, Sim coverage].\r\n# -- Punters and Kickers:\r\n# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, KA, AKB,[ Sim kicking ability].\r\n# TEAM:\\n\r\n#  name, SimData  0x<offense><defense><offense preference>\r\n#  Offensive pref values 0-3. \r\n#     0 = Little more rushing, 1 = Heavy Rushing,\r\n#     2 = little more passing, 3 = Heavy Passing.\r\n# credit to elway7 for finding\t'offense preference'", [Bridge.box(this.RomVersion, TSBTool.ROM_TYPE, System.Enum.toStringFn(TSBTool.ROM_TYPE))]);
            },
            GetTeamPlayers: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetTeamPlayers:: team {0} is invalid.", [team]));
                    return null;
                }

                var result = new System.Text.StringBuilder("", Bridge.Int.mul(41, this.positionNames.length));
                var pos;
                var teamSimData = this.GetTeamSimData(team);
                var data = "";
                if (teamSimData < 15) {
                    data = System.String.format("0{0:x}", [Bridge.box(teamSimData, System.Byte)]);
                } else {
                    data = System.String.format("{0:x}", [Bridge.box(teamSimData, System.Byte)]);
                }
                if (this.ShowOffPref) {
                    data = (data || "") + (this.GetTeamSimOffensePref(team));
                }

                var teamString = System.String.format("TEAM = {0} SimData=0x{1}", team, data);
                result.append(teamString);

                if (TSBTool.TecmoTool.ShowTeamFormation) {
                    result.append(System.String.format(", {0}", [this.GetTeamOffensiveFormation(team)]));
                }
                result.append("\n");

                if (TSBTool.TecmoTool.ShowPlaybook) {
                    result.append(System.String.format("{0}\n", [this.GetPlaybook(team)]));
                }
                if (TSBTool.TecmoTool.ShowTeamStrings) {
                    var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                    result.append(System.String.format("TEAM_ABB={0},TEAM_CITY={1},TEAM_NAME={2}\n", this.GetTeamAbbreviation(teamIndex), this.GetTeamCity(teamIndex), this.GetTeamName(teamIndex)));
                }

                for (var i = 0; i < this.positionNames.length; i = (i + 1) | 0) {
                    pos = this.positionNames[System.Array.index(i, this.positionNames)];
                    result.append(System.String.format("{0}\n", [this.GetPlayerData(team, pos, true, true, true, true, true)]));
                }
                result.append(System.String.format("{0}\n", [this.GetReturnTeam(team)]));
                result.append(System.String.format("KR, {0}\nPR, {1}\n", this.GetKickReturner(team), this.GetPuntReturner(team)));
                result.append("\n");
                return result.toString();
            },
            SetTeamStringTableString: function (stringIndex, newValue) {
                var $t, $t1;
                var junk = { v : 0 };
                var oldValue = this.GetTeamStringTableString(stringIndex);
                if (Bridge.referenceEquals(oldValue, newValue)) {
                    return;
                }
                var shiftAmount = { v : (newValue.length - oldValue.length) | 0 };
                if (shiftAmount.v !== 0) {
                    var currentPointerLocation = (this.GetTeamStringTableStart() + Bridge.Int.mul(2, stringIndex)) | 0;
                    var lastPointerLocation = (this.GetTeamStringTableStart() + Bridge.Int.mul(2, this.NumberOfStringsInTeamStringTable)) | 0;
                    this.AdjustDataPointers(currentPointerLocation, shiftAmount.v, lastPointerLocation);
                    var startPosition = (this.GetTeamStringTableLocation(((stringIndex + 1) | 0), junk) - 1) | 0;
                    var endPosition = 29488;
                    if (shiftAmount.v < 0) {
                        this.ShiftDataUp(startPosition, endPosition, shiftAmount.v, this.outputRom);
                    } else {
                        if (shiftAmount.v > 0) {
                            this.ShiftDataDown(startPosition, endPosition, shiftAmount.v, this.outputRom);
                        }
                    }
                }
                var startLoc = this.GetTeamStringTableLocation(stringIndex, shiftAmount);
                for (var i = 0; i < newValue.length; i = (i + 1) | 0) {
                    if (newValue.charCodeAt(i) === 42) {
                        ($t = this.OutputRom)[System.Array.index(((startLoc + i) | 0), $t)] = 0;
                    } else {
                        ($t1 = this.OutputRom)[System.Array.index(((startLoc + i) | 0), $t1)] = (newValue.charCodeAt(i)) & 255;
                    }
                }
            },
            GetTeamStringTableString: function (stringIndex) {
                var $t;
                var length = { v : 0 };
                var stringStartingLocation = this.GetTeamStringTableLocation(stringIndex, length);

                var stringChars = System.Array.init(length.v, 0, System.Char);
                for (var i = 0; i < stringChars.length; i = (i + 1) | 0) {
                    stringChars[System.Array.index(i, stringChars)] = ($t = this.OutputRom)[System.Array.index(((stringStartingLocation + i) | 0), $t)];
                    if (stringChars[System.Array.index(i, stringChars)] === 0) {
                        stringChars[System.Array.index(i, stringChars)] = 42;
                    }
                }
                var retVal = System.String.fromCharArray(stringChars);
                return retVal;
            },
            /**
             * Returns the location of the 'Team' string table. This string table 
             contains the city abbreviations, city names and team names.
             *
             * @instance
             * @private
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {number}          stringIndex    The index of the string to get.
             * @param   {System.Int32}    length         out param stores the length.
             * @return  {number}                         Returns the location of the string at the specified index.
             */
            GetTeamStringTableLocation: function (stringIndex, length) {
                var $t, $t1, $t2, $t3;
                var team_string_table_loc = this.GetTeamStringTableStart();
                var pointer_loc = (team_string_table_loc + Bridge.Int.mul(2, stringIndex)) | 0;
                var b1 = ($t = this.OutputRom)[System.Array.index(((pointer_loc + 1) | 0), $t)];
                var b2 = ($t1 = this.OutputRom)[System.Array.index(pointer_loc, $t1)];
                var b3 = ($t2 = this.OutputRom)[System.Array.index(((pointer_loc + 3) | 0), $t2)];
                var b4 = ($t3 = this.OutputRom)[System.Array.index(((pointer_loc + 2) | 0), $t3)];
                length.v = (((((b3 << 8) + b4) | 0)) - ((((b1 << 8) + b2) | 0))) | 0;
                if (stringIndex === ((this.NumberOfStringsInTeamStringTable - 1) | 0)) {
                    length.v = 9;
                }
                var pointerVal = ((b1 << 8) + b2) | 0;
                var stringStartingLocation = (pointerVal - 32768) | 0;
                return stringStartingLocation;
            },
            GetTeamStringTableStart: function () {
                var team_string_table_loc = 28672;
                return team_string_table_loc;
            },
            GetTeamName: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(teamIndex);
                var lastSpace = retVal.lastIndexOf(String.fromCharCode(32));
                retVal = System.String.replaceAll(retVal.substr(((lastSpace + 1) | 0)), "*", "");
                return retVal;
            },
            GetTeamCity: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(teamIndex).substr(5);
                var lastSpace = retVal.lastIndexOf(String.fromCharCode(32));
                retVal = retVal.substr(0, lastSpace);
                return retVal;
            },
            GetTeamAbbreviation: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(teamIndex);
                retVal = retVal.substr(0, 4);
                return retVal;
            },
            SetTeamAbbreviation: function (teamIndex, abb) {
                if (abb == null || abb.length !== 4) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error Setting TeamAbbreviation; TeamIndex:{0}; abb:{1}", Bridge.box(teamIndex, System.Int32), abb));
                } else {
                    var teamString = System.String.format("{0}*{1} {2}*", abb, this.GetTeamCity(teamIndex), this.GetTeamName(teamIndex));
                    this.SetTeamStringTableString(teamIndex, teamString);
                }
            },
            SetTeamName: function (teamIndex, name) {
                var teamString = System.String.format("{0}*{1} {2}*", this.GetTeamAbbreviation(teamIndex), this.GetTeamCity(teamIndex), name);
                this.SetTeamStringTableString(teamIndex, teamString);
            },
            SetTeamCity: function (teamIndex, city) {
                var teamString = System.String.format("{0}*{1} {2}*", this.GetTeamAbbreviation(teamIndex), city, this.GetTeamName(teamIndex));
                this.SetTeamStringTableString(teamIndex, teamString);
            },
            GetAll$1: function (season) {
                return this.GetAll();
            },
            GetAll: function () {
                var team;
                var all = new System.Text.StringBuilder("", Bridge.Int.mul(1230, this.positionNames.length));
                var year = System.String.format("YEAR={0}\n", [this.GetYear()]);
                all.append(year);
                for (var i = 0; i < TSBTool.SNES_TecmoTool.teams.length; i = (i + 1) | 0) {
                    team = TSBTool.SNES_TecmoTool.teams[System.Array.index(i, TSBTool.SNES_TecmoTool.teams)];
                    all.append(this.GetTeamPlayers(team));
                }
                return all.toString();
            },
            GetProBowlPlayers$1: function (season) {
                return this.GetProBowlPlayers();
            },
            GetProBowlPlayers: function () {
                var builder = new System.Text.StringBuilder("", 1000);
                builder.append("# AFC ProBowl players\r\n");
                builder.append(this.GetConferenceProBowlPlayers(TSBTool.Conference.AFC));
                builder.append("\r\n");

                builder.append("# NFC ProBowl players\r\n");
                builder.append(this.GetConferenceProBowlPlayers(TSBTool.Conference.NFC));
                builder.append("\r\n");
                return builder.toString();
            },
            /**
             * Gets the point in the player number name data that a player's data begins.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetDataPosition: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetDataPosition:: either team {0} or position {1} is invalid.", team, position));
                }
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(position);
                var guy = (Bridge.Int.mul(teamIndex, this.positionNames.length) + positionIndex) | 0;
                var pointerLocation = (TSBTool.SNES_TecmoTool.namePointersStart + (Bridge.Int.mul(2, guy))) | 0;
                var lowByte = this.outputRom[System.Array.index(pointerLocation, this.outputRom)];
                var hiByte = this.outputRom[System.Array.index(((pointerLocation + 1) | 0), this.outputRom)];
                hiByte = hiByte << 8;
                hiByte = (hiByte + lowByte) | 0;

                var ret = (hiByte + this.dataPositionOffset) | 0;
                return ret;
            },
            /**
             * Get the starting point of the guy AFTER the one passed to this method.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetNextDataPosition: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetNextDataPosition:: either team {0} or position {1} is invalid.", team, position));
                }

                var ti = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var pi = this.GetPositionIndex(position);
                pi = (pi + 1) | 0;
                if (Bridge.referenceEquals(position, "DB2")) {
                    ti = (ti + 1) | 0;
                    pi = 0;
                }
                if (Bridge.referenceEquals(team, "49ers") && Bridge.referenceEquals(position, "DB2")) {
                    return -1;
                } else {
                    return this.GetDataPosition(TSBTool.SNES_TecmoTool.teams[System.Array.index(ti, TSBTool.SNES_TecmoTool.teams)], this.positionNames[System.Array.index(pi, this.positionNames)]);
                }
            },
            GetPointerPosition: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetPointerPosition:: either team {0} or position {1} is invalid.", team, position));
                }
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(position);
                var playerSpot = (Bridge.Int.mul(teamIndex, this.positionNames.length) + positionIndex) | 0;
                if (Bridge.referenceEquals(team, "49ers") && Bridge.referenceEquals(position, "DB2")) {
                    return 1541942;
                }

                if (positionIndex < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Position '{0}' does not exist. Valid positions are:", [position]));
                    for (var i = 1; i <= this.positionNames.length; i = (i + 1) | 0) {
                        TSBTool.StaticUtils.WriteError(System.String.format("{0}\t", [this.positionNames[System.Array.index(((i - 1) | 0), this.positionNames)]]));
                    }
                    return -1;
                }
                var loc = (((TSBTool.SNES_TecmoTool.namePointersStart + (Bridge.Int.mul(teamIndex, 64))) | 0) + (Bridge.Int.mul(positionIndex, 2))) | 0;
                return loc;
            },
            /**
             * Sets the player data (jersey number, player name) in the data segment.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}            team        The team the player is assigned to.
             * @param   {string}            position    The position the player is assigned to.
             * @param   {Array.<number>}    bytes       The player's number and name data.
             * @param   {number}            change
             * @return  {void}
             */
            UpdatePlayerData: function (team, position, bytes, change) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) UpdatePlayerData:: either team {0} or position {1} is invalid.", team, position));
                }
                if (bytes == null) {
                    return;
                }

                var dataStart = this.GetDataPosition(team, position);
                this.ShiftDataAfter(team, position, change);
                var j = 0;
                var i;
                for (i = dataStart; j < bytes.length; i = (i + 1) | 0) {
                    this.outputRom[System.Array.index(i, this.outputRom)] = bytes[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), bytes)];
                }

                if (Bridge.referenceEquals(team, "49ers") && Bridge.referenceEquals(position, "DB2")) {
                    while (this.outputRom[System.Array.index(i, this.outputRom)] !== 255) {
                        this.outputRom[System.Array.index(Bridge.identity(i, ((i = (i + 1) | 0))), this.outputRom)] = 255;
                    }
                }
            },
            ShiftDataAfter: function (team, position, shiftAmount) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataAfter:: either team {0} or position {1} is invalid.", team, position));
                }

                if (Bridge.referenceEquals(team, TSBTool.SNES_TecmoTool.teams[System.Array.index(((TSBTool.SNES_TecmoTool.teams.length - 1) | 0), TSBTool.SNES_TecmoTool.teams)]) && Bridge.referenceEquals(position, "DB2")) {
                    return;
                }


                var endPosition = TSBTool.SNES_TecmoTool.nameNumberSegmentEnd;

                while (this.outputRom[System.Array.index(endPosition, this.outputRom)] === 255) {
                    endPosition = (endPosition - 1) | 0;
                }

                endPosition = (endPosition + 1) | 0;

                var startPosition = this.GetNextDataPosition(team, position);
                if (shiftAmount < 0) {
                    this.ShiftDataUp(startPosition, endPosition, shiftAmount, this.outputRom);
                } else {
                    if (shiftAmount > 0) {
                        this.ShiftDataDown(startPosition, endPosition, shiftAmount, this.outputRom);
                    }
                }
            },
            ShiftDataUp: function (startPos, endPos, shiftAmount, data) {
                if (startPos < 0 || endPos < 0) {
                    throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataUp:: either startPos {0} or endPos {1} is invalid.", Bridge.box(startPos, System.Int32), Bridge.box(endPos, System.Int32)));
                }

                var i;
                if (shiftAmount > 0) {
                    System.Console.WriteLine("positive shift amount in ShiftDataUp");
                }

                for (i = startPos; i <= endPos; i = (i + 1) | 0) {
                    data[System.Array.index(((i + shiftAmount) | 0), data)] = data[System.Array.index(i, data)];
                }
                /* i--;
                			for(int j=shiftAmount; j < 0; j++) 
                				data[i++] = 0xff; */

                i = (i + shiftAmount) | 0;
                while (this.outputRom[System.Array.index(i, this.outputRom)] !== 255 && i < TSBTool.SNES_TecmoTool.nameNumberSegmentEnd) {
                    this.outputRom[System.Array.index(i, this.outputRom)] = 255;
                    i = (i + 1) | 0;
                }

            },
            ShiftDataDown: function (startPos, endPos, shiftAmount, data) {
                if (startPos < 0 || endPos < 0) {
                    throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataUp:: either startPos {0} or endPos {1} is invalid.", Bridge.box(startPos, System.Int32), Bridge.box(endPos, System.Int32)));
                }

                for (var i = (endPos + shiftAmount) | 0; i > startPos; i = (i - 1) | 0) {
                    data[System.Array.index(i, data)] = data[System.Array.index(((i - shiftAmount) | 0), data)];
                }
            },
            GetDataAfter: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetDataAfter:: either team {0} or position {1} is invalid.", team, position));
                }

                if (Bridge.referenceEquals(team, TSBTool.SNES_TecmoTool.teams[System.Array.index(((TSBTool.SNES_TecmoTool.teams.length - 1) | 0), TSBTool.SNES_TecmoTool.teams)]) && Bridge.referenceEquals(position, "DB2")) {
                    return null;
                }

                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var positionIndex = this.GetPositionIndex(position);
                positionIndex = (positionIndex + 1) | 0;
                if (Bridge.referenceEquals(position, "DB2")) {
                    teamIndex = (teamIndex + 1) | 0;
                    positionIndex = 0;
                }
                var endPosition = TSBTool.SNES_TecmoTool.nameNumberSegmentEnd;
                while (this.outputRom[System.Array.index(endPosition, this.outputRom)] === 255) {
                    endPosition = (endPosition - 1) | 0;
                }

                endPosition = (endPosition + 1) | 0;
                var startPosition = this.GetDataPosition(TSBTool.SNES_TecmoTool.teams[System.Array.index(teamIndex, TSBTool.SNES_TecmoTool.teams)], this.positionNames[System.Array.index(positionIndex, this.positionNames)]);
                var retBytes = System.Array.init(((endPosition - startPosition) | 0), 0, System.Byte);

                var j = 0;
                for (var i = startPosition; i < ((endPosition + 1) | 0); i = (i + 1) | 0) {
                    retBytes[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), retBytes)] = this.outputRom[System.Array.index(i, this.outputRom)];
                }

                return retBytes;
            },
            /**
             * @instance
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    positionName    like 'QB1', 'K','P' ...
             * @return  {number}
             */
            GetPositionIndex: function (positionName) {
                var ret = -1;
                for (var i = 0; i < this.positionNames.length; i = (i + 1) | 0) {
                    if (Bridge.referenceEquals(this.positionNames[System.Array.index(i, this.positionNames)], positionName)) {
                        ret = i;
                        break;
                    }
                }
                return ret;
            },
            /**
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team              
             * @param   {string}    qb                Either 'QB1' or 'QB2'
             * @param   {number}    runningSpeed      
             * @param   {number}    rushingPower      
             * @param   {number}    maxSpeed          
             * @param   {number}    hittingPower      
             * @param   {number}    passingSpeed      
             * @param   {number}    passControl       
             * @param   {number}    accuracy          
             * @param   {number}    avoidPassBlock
             * @return  {void}
             */
            SetQBAbilities: function (team, qb, runningSpeed, rushingPower, maxSpeed, hittingPower, passingSpeed, passControl, accuracy, avoidPassBlock) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }
                if (!Bridge.referenceEquals(qb, "QB1") && !Bridge.referenceEquals(qb, "QB2")) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Cannot set qb ablities for {0}", [qb]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);
                passingSpeed = this.GetAbility(passingSpeed);
                passControl = this.GetAbility(passControl);
                accuracy = this.GetAbility(accuracy);
                avoidPassBlock = this.GetAbility(avoidPassBlock);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower) || !this.IsValidAbility(passingSpeed) || !this.IsValidAbility(passControl) || !this.IsValidAbility(accuracy) || !this.IsValidAbility(avoidPassBlock)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Abilities for {0} on {1} were not set.", qb, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, qb, runningSpeed, rushingPower, maxSpeed, hittingPower, passingSpeed, passControl);
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var posIndex = this.GetPositionIndex(qb);
                var location = ((((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamAbilityOffset)) + this.abilityOffsets[System.Array.index(posIndex, this.abilityOffsets)]) | 0) + TSBTool.SNES_TecmoTool.billsQB1AbilityStart) | 0;
                var lastByte = accuracy << 4;
                lastByte = (lastByte + avoidPassBlock) | 0;
                this.outputRom[System.Array.index(((location + 4) | 0), this.outputRom)] = lastByte & 255;
                lastByte = passingSpeed << 4;
                lastByte = (lastByte + passControl) | 0;
                this.outputRom[System.Array.index(((location + 3) | 0), this.outputRom)] = lastByte & 255;
            },
            SetSkillPlayerAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, ballControl, receptions) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }

                if (!Bridge.referenceEquals(pos, "RB1") && !Bridge.referenceEquals(pos, "RB2") && !Bridge.referenceEquals(pos, "RB3") && !Bridge.referenceEquals(pos, "RB4") && !Bridge.referenceEquals(pos, "WR1") && !Bridge.referenceEquals(pos, "WR2") && !Bridge.referenceEquals(pos, "WR3") && !Bridge.referenceEquals(pos, "WR4") && !Bridge.referenceEquals(pos, "TE1") && !Bridge.referenceEquals(pos, "TE2")) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Cannot set skill player ablities for {0}.", [pos]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);
                ballControl = this.GetAbility(ballControl);
                receptions = this.GetAbility(receptions);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower) || !this.IsValidAbility(receptions) || !this.IsValidAbility(ballControl)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) Invalid attribute. Abilities for {0} on {1} were not set.", pos, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, ballControl, receptions);
            },
            SetKickPlayerAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, kickingAbility, avoidKickBlock) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }

                if (!Bridge.referenceEquals(pos, "K") && !Bridge.referenceEquals(pos, "P")) {
                    TSBTool.StaticUtils.AddError(System.String.format("Cannot set kick player ablities for {0}.", [pos]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);
                kickingAbility = this.GetAbility(kickingAbility);
                avoidKickBlock = this.GetAbility(avoidKickBlock);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower) || !this.IsValidAbility(kickingAbility) || !this.IsValidAbility(avoidKickBlock)) {
                    TSBTool.StaticUtils.AddError(System.String.format("Abilities for {0} on {1} were not set.", pos, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, kickingAbility, avoidKickBlock);
            },
            SetDefensivePlayerAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, passRush, interceptions) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }

                if (!Bridge.referenceEquals(pos, "RE") && !Bridge.referenceEquals(pos, "NT") && !Bridge.referenceEquals(pos, "LE") && !Bridge.referenceEquals(pos, "ROLB") && !Bridge.referenceEquals(pos, "RILB") && !Bridge.referenceEquals(pos, "LILB") && !Bridge.referenceEquals(pos, "LOLB") && !Bridge.referenceEquals(pos, "RCB") && !Bridge.referenceEquals(pos, "LCB") && !Bridge.referenceEquals(pos, "SS") && !Bridge.referenceEquals(pos, "FS") && !Bridge.referenceEquals(pos, "DB2") && !Bridge.referenceEquals(pos, "DB1")) {
                    TSBTool.StaticUtils.AddError(System.String.format("Cannot set defensive player ablities for {0}.", [pos]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);
                passRush = this.GetAbility(passRush);
                interceptions = this.GetAbility(interceptions);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower) || !this.IsValidAbility(passRush) || !this.IsValidAbility(interceptions)) {
                    TSBTool.StaticUtils.AddError(System.String.format("Abilities for {0} on {1} were not set.", pos, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, passRush, interceptions);
            },
            SetOLPlayerAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) team {0} is invalid", [team]));
                    return;
                }

                if (!Bridge.referenceEquals(pos, "C") && !Bridge.referenceEquals(pos, "RG") && !Bridge.referenceEquals(pos, "LG") && !Bridge.referenceEquals(pos, "RT") && !Bridge.referenceEquals(pos, "LT")) {
                    TSBTool.StaticUtils.AddError(System.String.format("Cannot set OL player ablities for {0}.", [pos]));
                    return;
                }
                runningSpeed = this.GetAbility(runningSpeed);
                rushingPower = this.GetAbility(rushingPower);
                maxSpeed = this.GetAbility(maxSpeed);
                hittingPower = this.GetAbility(hittingPower);

                if (!this.IsValidAbility(runningSpeed) || !this.IsValidAbility(rushingPower) || !this.IsValidAbility(maxSpeed) || !this.IsValidAbility(hittingPower)) {
                    TSBTool.StaticUtils.AddError(System.String.format("Abilities for {0} on {1} were not set.", pos, team));
                    this.PrintValidAbilities();
                    return;
                }
                this.SaveAbilities(team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, -1, -1);
            },
            SaveAbilities: function (team, pos, runningSpeed, rushingPower, maxSpeed, hittingPower, bc, rec) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SaveAbilities:: team {0} is invalid", [team]));
                    return;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SaveAbilities:: position {0} is invalid", [pos]));
                    return;
                }

                var byte1, byte2, byte3;
                byte1 = rushingPower & 255;
                byte1 = byte1 << 4;
                byte1 = (byte1 + (runningSpeed & 255)) | 0;
                byte2 = maxSpeed & 255;
                byte2 = byte2 << 4;
                byte2 = (byte2 + (hittingPower & 255)) | 0;
                byte3 = bc & 255;
                byte3 = byte3 << 4;
                byte3 = (byte3 + (rec & 255)) | 0;
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var posIndex = this.GetPositionIndex(pos);
                var location = ((((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamAbilityOffset)) + this.abilityOffsets[System.Array.index(posIndex, this.abilityOffsets)]) | 0) + TSBTool.SNES_TecmoTool.billsQB1AbilityStart) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = byte1 & 255;
                this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = byte2 & 255;

                if (bc > -1 && rec > -1) {
                    this.outputRom[System.Array.index(((location + 3) | 0), this.outputRom)] = byte3 & 255;
                }
            },
            IsValidAbility: function (ab) {
                return ab >= 0 && ab <= 15;
            },
            GetAbility: function (ab) {
                var ret = 0;
                switch (ab) {
                    case 6: 
                        ret = 0;
                        break;
                    case 13: 
                        ret = 1;
                        break;
                    case 19: 
                        ret = 2;
                        break;
                    case 25: 
                        ret = 3;
                        break;
                    case 31: 
                        ret = 4;
                        break;
                    case 38: 
                        ret = 5;
                        break;
                    case 44: 
                        ret = 6;
                        break;
                    case 50: 
                        ret = 7;
                        break;
                    case 56: 
                        ret = 8;
                        break;
                    case 63: 
                        ret = 9;
                        break;
                    case 69: 
                        ret = 10;
                        break;
                    case 75: 
                        ret = 11;
                        break;
                    case 81: 
                        ret = 12;
                        break;
                    case 88: 
                        ret = 13;
                        break;
                    case 94: 
                        ret = 14;
                        break;
                    case 100: 
                        ret = 15;
                        break;
                }
                return ret;
            },
            MapAbality: function (ab) {
                /* if(abilityMap.ContainsKey(ab))
                				return (byte) abilityMap[ab];
                			else
                				return 0;*/

                var ret = 0;
                switch (ab) {
                    case 0: 
                        ret = 6;
                        break;
                    case 1: 
                        ret = 13;
                        break;
                    case 2: 
                        ret = 19;
                        break;
                    case 3: 
                        ret = 25;
                        break;
                    case 4: 
                        ret = 31;
                        break;
                    case 5: 
                        ret = 38;
                        break;
                    case 6: 
                        ret = 44;
                        break;
                    case 7: 
                        ret = 50;
                        break;
                    case 8: 
                        ret = 56;
                        break;
                    case 9: 
                        ret = 63;
                        break;
                    case 10: 
                        ret = 69;
                        break;
                    case 11: 
                        ret = 75;
                        break;
                    case 12: 
                        ret = 81;
                        break;
                    case 13: 
                        ret = 88;
                        break;
                    case 14: 
                        ret = 94;
                        break;
                    case 15: 
                        ret = 100;
                        break;
                }
                return ret;
            },
            /**
             * Returns an array of ints mapping to a player's abilities.
             Like { 13, 13, 50, 56, 31, 25}. The length of the array returned varies depending
             on position.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}            team        Team name like 'oilers'.
             * @param   {string}            position    Position name like 'RB4'.
             * @return  {Array.<number>}                an array of ints.
             */
            GetAbilities: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    return null;
                }

                var ret = System.Array.init([0], System.Int32);
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var posIndex = this.GetPositionIndex(position);
                var location = ((((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamAbilityOffset)) + this.abilityOffsets[System.Array.index(posIndex, this.abilityOffsets)]) | 0) + TSBTool.SNES_TecmoTool.billsQB1AbilityStart) | 0;
                var runningSpeed, rushingPower, maxSpeed, hittingPower, wild1, wild2, accuracy, avoidPassBlock;
                var b1, b2, b3, b4;
                b1 = this.outputRom[System.Array.index(location, this.outputRom)];
                b2 = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)];
                b3 = this.outputRom[System.Array.index(((location + 3) | 0), this.outputRom)];
                b4 = this.outputRom[System.Array.index(((location + 4) | 0), this.outputRom)];
                runningSpeed = b1 & 15;
                runningSpeed = this.MapAbality(runningSpeed);
                rushingPower = b1 & 240;
                rushingPower = this.MapAbality(rushingPower >> 4);
                maxSpeed = b2 & 240;
                maxSpeed = this.MapAbality(maxSpeed >> 4);
                hittingPower = b2 & 15;
                hittingPower = this.MapAbality(hittingPower);
                wild1 = b3 & 240;
                wild1 = this.MapAbality(wild1 >> 4);
                wild2 = b3 & 15;
                wild2 = this.MapAbality(wild2);
                accuracy = b4 & 240;
                accuracy = this.MapAbality(accuracy >> 4);
                avoidPassBlock = b4 & 15;
                avoidPassBlock = this.MapAbality(avoidPassBlock);
                switch (position) {
                    case "C": 
                    case "RG": 
                    case "LG": 
                    case "RT": 
                    case "LT": 
                        ret = System.Array.init(4, 0, System.Int32);
                        break;
                    case "QB1": 
                    case "QB2": 
                        ret = System.Array.init(8, 0, System.Int32);
                        ret[System.Array.index(4, ret)] = wild1;
                        ret[System.Array.index(5, ret)] = wild2;
                        ret[System.Array.index(6, ret)] = accuracy;
                        ret[System.Array.index(7, ret)] = avoidPassBlock;
                        break;
                    default: 
                        ret = System.Array.init(6, 0, System.Int32);
                        ret[System.Array.index(4, ret)] = wild1;
                        ret[System.Array.index(5, ret)] = wild2;
                        break;
                }
                ret[System.Array.index(0, ret)] = runningSpeed;
                ret[System.Array.index(1, ret)] = rushingPower;
                ret[System.Array.index(2, ret)] = maxSpeed;
                ret[System.Array.index(3, ret)] = hittingPower;
                return ret;
            },
            /**
             * Returns a string consisting of numbers, spaces and commas.
             Like "31, 69, 13, 13, 31, 44"
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {string}
             */
            GetAbilityString: function (team, position) {
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    return null;
                }
                var abilities = this.GetAbilities(team, position);
                var stuff = new System.Text.StringBuilder();

                for (var i = 0; i < abilities.length; i = (i + 1) | 0) {
                    stuff.append(abilities[System.Array.index(i, abilities)]);
                    stuff.append(", ");
                }
                stuff.remove(((stuff.getLength() - 2) | 0), 1);
                return stuff.toString();
            },
            /**
             * Returns the simulation data for the given team.
             Simulation data is of the form '0xNN' where N is a number 1-F (hex).
             A team's sim data of '0x57' signifies that the team has a simulation figure of
             '5' for offense, and '7' for defense.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team    The team of interest
             * @return  {number}
             */
            GetTeamSimData: function (team) {
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                if (teamIndex >= 0) {
                    var location = (Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + TSBTool.SNES_TecmoTool.billsTeamSimLoc) | 0;
                    return this.outputRom[System.Array.index(location, this.outputRom)];
                }
                return 0;
            },
            /**
             * Sets the given team's offense and defense sim values.
             Simulation data is of the form '0xNN' where N is a number 1-F (hex).
             A team's sim data of '0x57' signifies that the team has a simulation figure of
             '5' for offense, and '7' for defense.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team      The team to set.
             * @param   {number}    values    The value to set it to.
             * @return  {void}
             */
            SetTeamSimData: function (team, values) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetTeamSimData:: team {0} is invalid ", [team]));
                    return;
                }

                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var location = (Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + TSBTool.SNES_TecmoTool.billsTeamSimLoc) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = values;
            },
            /**
             * Sets the team sim offense tendency . 
             00 = Little more rushing, 01 = Heavy Rushing, 
             02 = little more passing, 03 = Heavy Passing.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}     team    the team name
             * @param   {number}     val     the number to set it to.
             * @return  {boolean}            true if set, fales if could not set it.
             */
            SetTeamSimOffensePref: function (team, val) {
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                if (val > -1 && val < 4 && teamIndex !== -1) {
                    var loc = (TSBTool.SNES_TecmoTool.teamSimOffensivePrefStart + teamIndex) | 0;
                    this.outputRom[System.Array.index(loc, this.outputRom)] = val & 255;
                } else {
                    if (teamIndex !== -1) {
                        TSBTool.StaticUtils.AddError(System.String.format("Can't set offensive pref to '{0}' valid values are 0-3.\n", [Bridge.box(val, System.Int32)]));
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("Team '{0}' is invalid\n", [team]));
                    }
                }
                return true;
            },
            /**
             * Sets the team sim offense tendency . 
             00 = Little more rushing, 01 = Heavy Rushing, 
             02 = little more passing, 03 = Heavy Passing.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team    Teh team name.
             * @return  {number}            their sim offense pref (0 - 3)
             */
            GetTeamSimOffensePref: function (team) {
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var val = -1;
                if (teamIndex > -1) {
                    var loc = (TSBTool.SNES_TecmoTool.teamSimOffensivePrefStart + teamIndex) | 0;
                    val = this.outputRom[System.Array.index(loc, this.outputRom)];
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Team '{0}' is invalid\n", [team]));
                }
                return val;
            },
            /**
             * Sets the team's offensive formation.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team         
             * @param   {string}    formation
             * @return  {void}
             */
            SetTeamOffensiveFormation: function (team, formation) {
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                if (teamIndex > -1 && teamIndex < 255) {
                    var location = (this.mTeamFormationsStartingLoc + teamIndex) | 0;

                    switch (formation) {
                        case TSBTool.SNES_TecmoTool.m2RB_2WR_1TE: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 0;
                            break;
                        case TSBTool.SNES_TecmoTool.m1RB_3WR_1TE: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 1;
                            break;
                        case TSBTool.SNES_TecmoTool.m1RB_4WR: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 2;
                            break;
                        default: 
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR! Formation {0:x} for team '{1}' is invalid.", formation, team));
                            TSBTool.StaticUtils.AddError(System.String.format("  Valid formations are:\n  {0}\n  {1}\n  {2}", TSBTool.SNES_TecmoTool.m2RB_2WR_1TE, TSBTool.SNES_TecmoTool.m1RB_3WR_1TE, TSBTool.SNES_TecmoTool.m1RB_4WR));
                            break;
                    }
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! Team '{0}' is invalid, Offensive Formation not set", [team]));
                }
            },
            /**
             * Gets the team's offensive formation.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetTeamOffensiveFormation: function (team) {
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                var ret = "OFFENSIVE_FORMATION = ";
                if (teamIndex > -1 && teamIndex < 255) {
                    var location = (this.mTeamFormationsStartingLoc + teamIndex) | 0;
                    var formation = this.outputRom[System.Array.index(location, this.outputRom)];

                    switch (formation) {
                        case 0: 
                            ret = (ret || "") + (TSBTool.SNES_TecmoTool.m2RB_2WR_1TE || "");
                            break;
                        case 1: 
                            ret = (ret || "") + (TSBTool.SNES_TecmoTool.m1RB_3WR_1TE || "");
                            break;
                        case 2: 
                            ret = (ret || "") + (TSBTool.SNES_TecmoTool.m1RB_4WR || "");
                            break;
                        default: 
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR! Formation {0:x} for team {1} is invalid, ROM FORMATIONS could be messed up.", Bridge.box(formation, System.Int32), team));
                            ret = "";
                            break;
                    }
                } else {
                    ret = "";
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! Team '{0}' is invalid, Offensive Formation get failed.", [team]));
                }
                return ret;
            },
            /**
             * Returns a string like "PLAYBOOK R1, R4, R6, R8, P1, P3, P7, P3"
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetPlaybook: function (team) {
                var ret = "";
                var rSlot1, rSlot2, rSlot3, rSlot4, pSlot1, pSlot2, pSlot3, pSlot4;

                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                if (teamIndex > -1) {
                    var pbLocation = (TSBTool.SNES_TecmoTool.mPlaybookStartLoc + (Bridge.Int.mul(teamIndex, 4))) | 0;
                    rSlot1 = this.outputRom[System.Array.index(pbLocation, this.outputRom)] >> 4;
                    rSlot2 = this.outputRom[System.Array.index(pbLocation, this.outputRom)] & 15;
                    rSlot3 = this.outputRom[System.Array.index(((pbLocation + 1) | 0), this.outputRom)] >> 4;
                    rSlot4 = this.outputRom[System.Array.index(((pbLocation + 1) | 0), this.outputRom)] & 15;

                    pSlot1 = this.outputRom[System.Array.index(((pbLocation + 2) | 0), this.outputRom)] >> 4;
                    pSlot2 = this.outputRom[System.Array.index(((pbLocation + 2) | 0), this.outputRom)] & 15;
                    pSlot3 = this.outputRom[System.Array.index(((pbLocation + 3) | 0), this.outputRom)] >> 4;
                    pSlot4 = this.outputRom[System.Array.index(((pbLocation + 3) | 0), this.outputRom)] & 15;

                    ret = System.String.format("PLAYBOOK R{0}{1}{2}{3}, P{4}{5}{6}{7} ", Bridge.box(((rSlot1 + 1) | 0), System.Int32), Bridge.box(((rSlot2 + 1) | 0), System.Int32), Bridge.box(((rSlot3 + 1) | 0), System.Int32), Bridge.box(((rSlot4 + 1) | 0), System.Int32), Bridge.box(((pSlot1 + 1) | 0), System.Int32), Bridge.box(((pSlot2 + 1) | 0), System.Int32), Bridge.box(((pSlot3 + 1) | 0), System.Int32), Bridge.box(((pSlot4 + 1) | 0), System.Int32));
                }

                return ret;
            },
            /**
             * Sets the team's playbook
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team         
             * @param   {string}    runPlays     String like "R1234"
             * @param   {string}    passPlays    String like "P4567"
             * @return  {void}
             */
            SetPlaybook: function (team, runPlays, passPlays) {
                if (this.runRegex == null || this.passRegex == null) {
                    this.runRegex = new System.Text.RegularExpressions.Regex.ctor("R([1-8])([1-8])([1-8])([1-8])");
                    this.passRegex = new System.Text.RegularExpressions.Regex.ctor("P([1-8])([1-8])([1-8])([1-8])");
                }
                var runs = this.runRegex.match(runPlays);
                var pass = this.passRegex.match(passPlays);

                var r1, r2, r3, r4, p1, p2, p3, p4;

                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                if (teamIndex > -1 && !Bridge.referenceEquals(runs, System.Text.RegularExpressions.Match.getEmpty()) && !Bridge.referenceEquals(pass, System.Text.RegularExpressions.Match.getEmpty())) {
                    var pbLocation = (TSBTool.SNES_TecmoTool.mPlaybookStartLoc + (Bridge.Int.mul(teamIndex, 4))) | 0;
                    r1 = (System.Int32.parse(runs.getGroups().get(1).toString()) - 1) | 0;
                    r2 = (System.Int32.parse(runs.getGroups().get(2).toString()) - 1) | 0;
                    r3 = (System.Int32.parse(runs.getGroups().get(3).toString()) - 1) | 0;
                    r4 = (System.Int32.parse(runs.getGroups().get(4).toString()) - 1) | 0;

                    p1 = (System.Int32.parse(pass.getGroups().get(1).toString()) - 1) | 0;
                    p2 = (System.Int32.parse(pass.getGroups().get(2).toString()) - 1) | 0;
                    p3 = (System.Int32.parse(pass.getGroups().get(3).toString()) - 1) | 0;
                    p4 = (System.Int32.parse(pass.getGroups().get(4).toString()) - 1) | 0;

                    r1 = ((r1 << 4) + r2) | 0;
                    r3 = ((r3 << 4) + r4) | 0;
                    p1 = ((p1 << 4) + p2) | 0;
                    p3 = ((p3 << 4) + p4) | 0;
                    this.outputRom[System.Array.index(pbLocation, this.outputRom)] = r1 & 255;
                    this.outputRom[System.Array.index(((pbLocation + 1) | 0), this.outputRom)] = r3 & 255;
                    this.outputRom[System.Array.index(((pbLocation + 2) | 0), this.outputRom)] = p1 & 255;
                    this.outputRom[System.Array.index(((pbLocation + 3) | 0), this.outputRom)] = p3 & 255;
                } else {
                    if (teamIndex < 0) {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR! SetPlaybook: Team {0} is Invalid.", [team]));
                    }
                    if (Bridge.referenceEquals(runs, System.Text.RegularExpressions.Match.getEmpty())) {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR! SetPlaybook Run play definition '{0} 'is Invalid", [runPlays]));
                    }
                    if (Bridge.referenceEquals(pass, System.Text.RegularExpressions.Match.getEmpty())) {
                        TSBTool.StaticUtils.AddError(System.String.format("ERROR! SetPlaybook Pass play definition '{0} 'is Invalid", [passPlays]));
                    }
                }
            },
            ApplyJuice: function (week, amt) {
                var ret = true;
                if (week > 17 || week < 0 || amt > 17 || amt < 0) {
                    ret = false;
                } else {
                    var rom_location = (TSBTool.SNES_TecmoTool.JUICE_LOCATION + (Bridge.Int.mul(week, 5))) | 0;
                    var index = Bridge.Int.mul((((amt - 1) | 0)), 5);
                    for (var i = 0; i < 5; i = (i + 1) | 0) {
                        this.outputRom[System.Array.index(((rom_location + i) | 0), this.outputRom)] = this.m_JuiceArray[System.Array.index(((index + i) | 0), this.m_JuiceArray)];
                    }
                }
                return ret;
            },
            GetPlayerSimData: function (team, pos) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetPlayerSimData:: Invalid team {0}", [team]));
                    return null;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetPlayerSimData:: Invalid Position {0}", [pos]));
                    return null;
                }

                switch (pos) {
                    case "QB1": 
                    case "QB2": 
                        return this.GetQBSimData(team, pos);
                    case "RB1": 
                    case "RB2": 
                    case "RB3": 
                    case "RB4": 
                    case "WR1": 
                    case "WR2": 
                    case "WR3": 
                    case "WR4": 
                    case "TE1": 
                    case "TE2": 
                        return this.GetSkillSimData(team, pos);
                    case "RE": 
                    case "NT": 
                    case "LE": 
                    case "LOLB": 
                    case "LILB": 
                    case "RILB": 
                    case "ROLB": 
                    case "RCB": 
                    case "LCB": 
                    case "FS": 
                    case "SS": 
                        return this.GetDefensiveSimData(team, pos);
                    case "K": 
                        return this.GetKickingSimData(team);
                    case "P": 
                        return this.GetPuntingSimData(team);
                    default: 
                        return null;
                }
            },
            GetKickingSimData: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetKickingSimData:: Invalid team {0}", [team]));
                    return null;
                }
                var ret = System.Array.init(1, 0, System.Int32);
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var location = (((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + TSBTool.SNES_TecmoTool.billsQB1SimLoc) | 0) + 46) | 0;
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] >> 4;
                return ret;
            },
            SetKickingSimData: function (team, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetKickingSimData:: Invalid team {0}", [team]));
                    return;
                }
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var location = (((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + TSBTool.SNES_TecmoTool.billsQB1SimLoc) | 0) + 46) | 0;
                var g = this.outputRom[System.Array.index(location, this.outputRom)];
                g = g & 15;
                var g2 = data << 4;
                g = (g + g2) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = g & 255;
            },
            GetPuntingSimData: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetPuntingSimData:: Invalid team {0}", [team]));
                    return null;
                }
                var ret = System.Array.init(1, 0, System.Int32);
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var location = (((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + TSBTool.SNES_TecmoTool.billsQB1SimLoc) | 0) + 46) | 0;
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] & 15;
                return ret;
            },
            SetPuntingSimData: function (team, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetPuntingSimData:: Invalid team {0}", [team]));
                    return;
                }
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var location = (((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + TSBTool.SNES_TecmoTool.billsQB1SimLoc) | 0) + 46) | 0;
                var d = this.outputRom[System.Array.index(location, this.outputRom)];
                d = d & 240;
                d = (d + data) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = d & 255;
            },
            GetDefensiveSimData: function (team, pos) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetDefensiveSimData:: Invalid team {0}", [team]));
                    return null;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetDefensiveSimData:: Invalid Position {0}", [pos]));
                    return null;
                }

                var ret = System.Array.init(2, 0, System.Int32);
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var positionIndex = this.GetPositionIndex(pos);
                var location = (((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + (((positionIndex - 17) | 0))) | 0) + TSBTool.SNES_TecmoTool.billsRESimLoc) | 0;
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)];
                ret[System.Array.index(1, ret)] = this.outputRom[System.Array.index(((location + 11) | 0), this.outputRom)];
                return ret;
            },
            /**
             * Sets the simulation data for a defensive player.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}            team    The team the player belongs to.
             * @param   {string}            pos     the position he plays.
             * @param   {Array.<number>}    data    the data to set it to (length = 2).
             * @return  {void}
             */
            SetDefensiveSimData: function (team, pos, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetDefensiveSimData:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetDefensiveSimData:: Invalid Position {0}", [pos]));
                    return;
                } else if (data == null || data.length < 2) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                    return;
                }
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var positionIndex = this.GetPositionIndex(pos);
                var location = (((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + (((positionIndex - 17) | 0))) | 0) + TSBTool.SNES_TecmoTool.billsRESimLoc) | 0;
                var byte1, byte2;
                byte1 = (data[System.Array.index(0, data)]) & 255;
                byte2 = (data[System.Array.index(1, data)]) & 255;

                this.outputRom[System.Array.index(location, this.outputRom)] = byte1;
                this.outputRom[System.Array.index(((location + 11) | 0), this.outputRom)] = byte2;
            },
            GetSkillSimData: function (team, pos) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetSkillSimData:: Invalid team {0}", [team]));
                    return null;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetSkillSimData:: Invalid Position {0}", [pos]));
                    return null;
                }

                var ret = System.Array.init(4, 0, System.Int32);
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var positionIndex = this.GetPositionIndex(pos);
                var location = (((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + (Bridge.Int.mul(positionIndex, 2))) | 0) + TSBTool.SNES_TecmoTool.billsQB1SimLoc) | 0;
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] >> 4;
                ret[System.Array.index(1, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] & 15;
                ret[System.Array.index(2, ret)] = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] >> 4;
                ret[System.Array.index(3, ret)] = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] & 15;
                return ret;
            },
            SetSkillSimData: function (team, pos, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetSkillSimData:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetSkillSimData:: Invalid Position {0}", [pos]));
                    return;
                } else if (data == null || data.length < 4) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                    return;
                }

                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);
                var positionIndex = this.GetPositionIndex(pos);
                var location = (((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + (Bridge.Int.mul(positionIndex, 2))) | 0) + TSBTool.SNES_TecmoTool.billsQB1SimLoc) | 0;
                var byte1, byte2;
                byte1 = data[System.Array.index(0, data)] << 4;
                byte1 = (byte1 + data[System.Array.index(1, data)]) | 0;
                byte2 = data[System.Array.index(2, data)] << 4;
                byte2 = (byte2 + data[System.Array.index(3, data)]) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = byte1 & 255;
                this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = byte2 & 255;
            },
            GetQBSimData: function (team, pos) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetQBSimData:: Invalid team {0}", [team]));
                    return null;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetQBSimData:: Invalid Position {0}", [pos]));
                    return null;
                }

                var ret = System.Array.init(3, 0, System.Int32);
                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);

                var location = (Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + TSBTool.SNES_TecmoTool.billsQB1SimLoc) | 0;
                if (Bridge.referenceEquals(pos, "QB2")) {
                    location = (location + 2) | 0;
                }
                ret[System.Array.index(0, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] >> 4;
                ret[System.Array.index(1, ret)] = this.outputRom[System.Array.index(location, this.outputRom)] & 15;
                ret[System.Array.index(2, ret)] = this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)];
                return ret;
            },
            SetQBSimData: function (team, pos, data) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetQBSimData:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(pos)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetQBSimData:: Invalid Position {0}", [pos]));
                    return;
                } else if (data == null || data.length < 2) {
                    TSBTool.StaticUtils.AddError(System.String.format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                    return;
                }

                var teamIndex = TSBTool.SNES_TecmoTool.GetSimTeamIndex(team);

                var location = (Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamSimOffset) + TSBTool.SNES_TecmoTool.billsQB1SimLoc) | 0;
                if (Bridge.referenceEquals(pos, "QB2")) {
                    location = (location + 2) | 0;
                }
                var byte1, byte2;
                byte1 = ((data[System.Array.index(0, data)]) & 255) << 4;
                byte1 = (byte1 + ((data[System.Array.index(1, data)]) & 255)) | 0;
                byte2 = (data[System.Array.index(2, data)]) & 255;
                this.outputRom[System.Array.index(location, this.outputRom)] = byte1 & 255;
                this.outputRom[System.Array.index(((location + 1) | 0), this.outputRom)] = byte2 & 255;
            },
            /**
             * Get the face number from the given team/position
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetFace: function (team, position) {
                var positionOffset = this.GetPositionIndex(position);
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                if (positionOffset < 0 || teamIndex < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("GetFace Error getting face for {0} {1}", team, position));
                    return -1;
                }
                var loc = ((((((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamAbilityOffset)) + this.abilityOffsets[System.Array.index(positionOffset, this.abilityOffsets)]) | 0) + TSBTool.SNES_TecmoTool.billsQB1AbilityStart) | 0) + 2) | 0;

                var ret = this.outputRom[System.Array.index(loc, this.outputRom)];
                return ret;
            },
            /**
             * Sets the face for the guy at position 'position' on team 'team'.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team        
             * @param   {string}    position    
             * @param   {number}    face
             * @return  {void}
             */
            SetFace: function (team, position, face) {
                var positionOffset = this.GetPositionIndex(position);
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                if (positionOffset < 0 || teamIndex < 0 || !!(face < 0 | face > 212)) {
                    TSBTool.StaticUtils.AddError(System.String.format("SetFace Error setting face for {0} {1} face={2}", team, position, Bridge.box(face, System.Int32)));
                    if (!!(face < 0 | face > 212)) {
                        TSBTool.StaticUtils.AddError(System.String.format("Valid Face numbers are 0x00 - 0xD4", null));
                    }
                    return;
                }
                var loc = ((((((Bridge.Int.mul(teamIndex, TSBTool.SNES_TecmoTool.teamAbilityOffset)) + this.abilityOffsets[System.Array.index(positionOffset, this.abilityOffsets)]) | 0) + TSBTool.SNES_TecmoTool.billsQB1AbilityStart) | 0) + 2) | 0;
                var skin = 128;

                if (face < 83) {
                    skin = 0;
                }

                this.SetCutSceneRace(teamIndex, positionOffset, face);
                this.outputRom[System.Array.index(loc, this.outputRom)] = skin & 255;
            },
            SetCutSceneRace: function (teamIndex, positionIndex, color) {
                var pi = (((Bridge.Int.div(positionIndex, 8)) | 0));
                var romPosition = (((TSBTool.SNES_TecmoTool.mRaceCutsceneStartPos + pi) | 0) + Bridge.Int.mul(teamIndex, 4)) | 0;
                var oldValue = this.outputRom[System.Array.index(romPosition, this.outputRom)];
                var newValue = this.GetNewValue(oldValue, positionIndex, color);
                this.outputRom[System.Array.index(romPosition, this.outputRom)] = newValue;
            },
            GetNewValue: function (oldValue, positionIndex, race) {
                var mask = 255;
                var ret = oldValue;
                var bitIndex = positionIndex % 8;
                if (race === 0) {
                    mask = this.GetWhiteMask(positionIndex);
                    ret = (ret & mask) & 255;
                } else {
                    mask = this.GetColorMask(positionIndex);
                    ret = (ret | mask) & 255;
                }
                return ret;
            },
            GetWhiteMask: function (positionIndex) {
                var ret = 255;
                var bitIndex = positionIndex % 8;
                switch (bitIndex) {
                    case 0: 
                        ret = 127;
                        break;
                    case 1: 
                        ret = 191;
                        break;
                    case 2: 
                        ret = 223;
                        break;
                    case 3: 
                        ret = 239;
                        break;
                    case 4: 
                        ret = 247;
                        break;
                    case 5: 
                        ret = 251;
                        break;
                    case 6: 
                        ret = 253;
                        break;
                    case 7: 
                        ret = 254;
                        break;
                }
                return ret;
            },
            GetColorMask: function (positionIndex) {
                var ret = 0;
                var bitIndex = positionIndex % 8;
                switch (bitIndex) {
                    case 0: 
                        ret = 128;
                        break;
                    case 1: 
                        ret = 64;
                        break;
                    case 2: 
                        ret = 32;
                        break;
                    case 3: 
                        ret = 16;
                        break;
                    case 4: 
                        ret = 8;
                        break;
                    case 5: 
                        ret = 4;
                        break;
                    case 6: 
                        ret = 2;
                        break;
                    case 7: 
                        ret = 1;
                        break;
                }
                return ret;
            },
            /**
             * Sets the return team for 'team'
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team    
             * @param   {string}    pos0    
             * @param   {string}    pos1    
             * @param   {string}    pos2
             * @return  {void}
             */
            SetReturnTeam: function (team, pos0, pos1, pos2) {
                if (this.Index(this.positionNames, pos0) > -1 && this.Index(this.positionNames, pos1) > -1 && this.Index(this.positionNames, pos2) > -1) {
                    this.InsertGuyOnReturnTeam(pos0, team, 0);
                    this.InsertGuyOnReturnTeam(pos1, team, 1);
                    this.InsertGuyOnReturnTeam(pos2, team, 2);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! Invalid position on RETURN_TEAM {0} {1} {2}", pos0, pos1, pos2));
                }
            },
            /**
             * Returns a string like "RETURN_TEAM WR3, RB3, RCB"
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetReturnTeam: function (team) {
                var ret = null;
                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                if (teamIndex < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! GetReturnTeam.Invalid team {0}", [team]));
                } else {
                    var teamLocation = (TSBTool.SNES_TecmoTool.pr_kr_team_start_offset + (Bridge.Int.mul(4, teamIndex))) | 0;
                    var pos0, pos1, pos2;

                    pos0 = this.outputRom[System.Array.index(teamLocation, this.outputRom)];
                    pos1 = this.outputRom[System.Array.index(((teamLocation + 1) | 0), this.outputRom)];
                    pos2 = this.outputRom[System.Array.index(((teamLocation + 2) | 0), this.outputRom)];

                    if (pos0 > -1 && pos0 < this.positionNames.length && pos1 > -1 && pos1 < this.positionNames.length && pos2 > -1 && pos2 < this.positionNames.length) {
                        ret = System.String.format("RETURN_TEAM {0}, {1}, {2}", this.positionNames[System.Array.index(pos0, this.positionNames)], this.positionNames[System.Array.index(pos1, this.positionNames)], this.positionNames[System.Array.index(pos2, this.positionNames)]);
                    } else {
                        TSBTool.StaticUtils.AddError("ERROR! Return Team Messed up in ROM.");
                    }
                }
                return ret;
            },
            /**
             * Set the punt returner by position.
             Hi nibble.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {void}
             */
            SetPuntReturner: function (team, position) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetPuntReturner:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(position)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetPuntReturner:: Invalid Position {0}", [position]));
                    return;
                }

                var index = this.IsGuyOnReturnTeam(position, team);
                if (index < 0) {
                    index = 1;
                    this.InsertGuyOnReturnTeam(position, team, index);
                }

                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                var location = (TSBTool.SNES_TecmoTool.pr_kr_start_offset + teamIndex) | 0;
                var kr_pr = this.outputRom[System.Array.index(location, this.outputRom)];

                kr_pr = kr_pr & 240;
                kr_pr = (kr_pr + index) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = kr_pr & 255;
            },
            /**
             * Set the kick returner by position.
             Lo nibble.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {void}
             */
            SetKickReturner: function (team, position) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetKickReturner:: Invalid team {0}", [team]));
                    return;
                } else if (!this.IsValidPosition(position)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetKickReturner:: Invalid Position {0}", [position]));
                    return;
                }

                var index = this.IsGuyOnReturnTeam(position, team);
                if (index < 0) {
                    index = 0;
                    this.InsertGuyOnReturnTeam(position, team, index);
                }
                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                var location = (TSBTool.SNES_TecmoTool.pr_kr_start_offset + teamIndex) | 0;
                var kr_pr = this.outputRom[System.Array.index(location, this.outputRom)];
                kr_pr = kr_pr & 15;
                kr_pr = (kr_pr + (index << 4)) | 0;
                this.outputRom[System.Array.index(location, this.outputRom)] = kr_pr & 255;
            },
            /**
             * Gets the position who returns punts.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetKickReturner: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetKickReturner:: Invalid team {0}", [team]));
                    return null;
                }

                var ret = "";
                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                var returnTeamIndex = this.outputRom[System.Array.index(((TSBTool.SNES_TecmoTool.pr_kr_start_offset + teamIndex) | 0), this.outputRom)] >> 4;
                var teamLocation = (TSBTool.SNES_TecmoTool.pr_kr_team_start_offset + (Bridge.Int.mul(4, teamIndex))) | 0;

                var positionIndex = this.outputRom[System.Array.index(((returnTeamIndex + teamLocation) | 0), this.outputRom)];

                if (positionIndex < this.positionNames.length) {
                    ret = this.positionNames[System.Array.index(positionIndex, this.positionNames)];
                }
                return ret;

                /* 
                			int b = outputRom[loc1];
                			b = b & 0x0F;
                			ret = positionNames[b];
                			return ret;
                			*/
            },
            /**
             * @instance
             * @private
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    guy     the position name of a guy
             * @param   {string}    team    the team
             * @return  {number}
             */
            IsGuyOnReturnTeam: function (guy, team) {
                var ret = -1;
                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                var posIndex = this.Index(this.positionNames, guy);
                var teamLocation = (TSBTool.SNES_TecmoTool.pr_kr_team_start_offset + (Bridge.Int.mul(4, teamIndex))) | 0;

                if (this.outputRom[System.Array.index(teamLocation, this.outputRom)] === posIndex) {
                    ret = 0;
                } else {
                    if (this.outputRom[System.Array.index(((teamLocation + 1) | 0), this.outputRom)] === posIndex) {
                        ret = 1;
                    } else {
                        if (this.outputRom[System.Array.index(((teamLocation + 2) | 0), this.outputRom)] === posIndex) {
                            ret = 2;
                        }
                    }
                }

                return ret;
            },
            InsertGuyOnReturnTeam: function (position, team, index) {
                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                var posIndex = this.Index(this.positionNames, position);

                if (index < 0 || index > 2 || teamIndex < 0 || teamIndex > 27 || posIndex < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("InsertGuyOnReturnTeam: invalid arguments {0}, {1}, {2}", position, team, Bridge.box(index, System.Int32)));
                    return;
                }

                var teamLocation = (TSBTool.SNES_TecmoTool.pr_kr_team_start_offset + (Bridge.Int.mul(4, teamIndex))) | 0;
                this.outputRom[System.Array.index(((teamLocation + index) | 0), this.outputRom)] = posIndex & 255;
            },
            /**
             * Gets the position who returns kicks.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {string}    team
             * @return  {string}
             */
            GetPuntReturner: function (team) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) GetPuntReturner:: Invalid team {0}", [team]));
                    return null;
                }

                var ret = "";
                var teamIndex = this.Index(TSBTool.SNES_TecmoTool.teams, team);
                var returnTeamIndex = this.outputRom[System.Array.index((((TSBTool.SNES_TecmoTool.pr_kr_start_offset + teamIndex) | 0)), this.outputRom)] & 15;
                var teamLocation = (TSBTool.SNES_TecmoTool.pr_kr_team_start_offset + (Bridge.Int.mul(4, teamIndex))) | 0;

                var positionIndex = this.outputRom[System.Array.index(((returnTeamIndex + teamLocation) | 0), this.outputRom)];

                if (positionIndex < this.positionNames.length) {
                    ret = this.positionNames[System.Array.index(positionIndex, this.positionNames)];
                }
                return ret;
                /* 
                			string ret = "";
                			int location = 0x328d3 + Index(teams,team);
                			int b = outputRom[location];
                			b = b & 0xF0;
                			b = b >> 4;
                			ret = positionNames[b];
                			return ret;
                			*/
            },
            SetProBowlPlayer: function (conf, proBowlPos, fromTeam, fromTeamPos) {
                var $t, $t1;
                var offset = 0;
                if (conf === TSBTool.Conference.NFC) {
                    offset = (offset + 72) | 0;
                }
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(fromTeam);
                var ti = teamIndex & 255;
                var pi = fromTeamPos & 255;

                var posIndex = -1;
                switch (proBowlPos) {
                    case "RET1": 
                        posIndex = this.positionNames.length;
                        break;
                    case "RET2": 
                        posIndex = (this.positionNames.length + 1) | 0;
                        break;
                    case "RET3": 
                        posIndex = (this.positionNames.length + 2) | 0;
                        break;
                    default: 
                        posIndex = this.GetPositionIndex(proBowlPos);
                        break;
                }
                var loc = (((this.mProwbowlStartingLoc + offset) | 0) + (Bridge.Int.mul(2, posIndex))) | 0;

                ($t = this.OutputRom)[System.Array.index(loc, $t)] = pi;
                ($t1 = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t1)] = ti;
            },
            SetQuarterLength: function (len) {
                if (this.outputRom != null) {
                    this.outputRom[System.Array.index(TSBTool.SNES_TecmoTool.QUARTER_LENGTH, this.outputRom)] = len;
                }
            },
            GetQuarterLength: function () {
                var ret = 0;
                if (this.outputRom != null) {
                    ret = this.outputRom[System.Array.index(TSBTool.SNES_TecmoTool.QUARTER_LENGTH, this.outputRom)];
                }
                return ret;
            },
            ApplySet: function (line) {
                if (this.simpleSetRegex == null) {
                    this.simpleSetRegex = new System.Text.RegularExpressions.Regex.ctor("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
                }

                if (!Bridge.referenceEquals(this.simpleSetRegex.match(line), System.Text.RegularExpressions.Match.getEmpty())) {
                    TSBTool.StaticUtils.ApplySimpleSet(line, this);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR with line \"{0}\"", [line]));
                }
            },
            GetHexBytes: function (input) {
                if (input == null) {
                    return null;
                }

                var ret = System.Array.init(((Bridge.Int.div(input.length, 2)) | 0), 0, System.Byte);
                var b = "";
                var tmp = 0;
                var j = 0;

                for (var i = 0; i < input.length; i = (i + 2) | 0) {
                    b = input.substr(i, 2);
                    tmp = TSBTool.StaticUtils.ParseIntFromHexString(b);
                    ret[System.Array.index(Bridge.identity(j, ((j = (j + 1) | 0))), ret)] = tmp & 255;
                }
                return ret;
            },
            /**
             * Returns the first index of element that occurs in 'array'. returns
             -1 if 'element' doesn't occur in 'array'.
             *
             * @instance
             * @private
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {Array.<string>}    array      
             * @param   {string}            element
             * @return  {number}
             */
            Index: function (array, element) {
                for (var i = 0; i < array.length; i = (i + 1) | 0) {
                    if (Bridge.referenceEquals(array[System.Array.index(i, array)], element)) {
                        return i;
                    }
                }

                return -1;
            },
            PrintValidAbilities: function () {
                TSBTool.StaticUtils.AddError(System.String.format("Valid player abilities are 6, 13, 19, 25, 31, 38, 44, 50, 56, 63, 69, 75, 81, 88, 94, 100", null));
            },
            StringifyArray: function (input) {
                if (input == null) {
                    return null;
                }

                var sb = new System.Text.StringBuilder("", 40);
                for (var i = 0; i < input.length; i = (i + 1) | 0) {
                    sb.append(System.String.format("{0}, ", [Bridge.box(input[System.Array.index(i, input)], System.Int32)]));
                }
                sb.remove(((sb.getLength() - 2) | 0), 1);
                return sb.toString();
            },
            /**
             * Returns an ArrayList of errors that were encountered during the operation.
             *
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {System.Collections.Generic.List$1}    scheduleList
             * @return  {void}
             */
            ApplySchedule: function (scheduleList) {
                if (scheduleList != null && this.outputRom != null) {
                    var sch = new TSBTool.SNES_ScheduleHelper(this.outputRom);
                    sch.ApplySchedule(scheduleList);
                }
            },
            SetHomeUniform: function (team, colorString) {
                var loc = this.GetUniformLoc(team);
                if (loc > -1) {
                }
            },
            SetAwayUniform: function (team, colorString) {
                var loc = this.GetUniformLoc(team);
                if (loc > -1) {
                }
            },
            GetHomeUniform: function (team) {
                var ret = "";
                var loc = this.GetUniformLoc(team);
                if (loc > -1) {
                }
                return ret;
            },
            GetAwayUniform: function (team) {
                var ret = "";
                var loc = this.GetUniformLoc(team);
                if (loc > -1) {
                }
                return ret;
            },
            GetUniformLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.SNES_TecmoTool.GetTeamIndex(team);
                if (teamIndex > -1 && teamIndex < 28) {
                    ret = (this.BillsUniformLoc + (Bridge.Int.mul(teamIndex, 10))) | 0;
                }
                return ret;
            },
            GetGameUniform: function (team) {
                var ret = "";
                return ret;
            },
            SetDivChampColors: function (team, colorString) { },
            SetUniformUsage: function (team, usage) { },
            GetUniformUsage: function (team) {
                return "";
            },
            SetConfChampColors: function (team, colorString) { },
            GetDivChampColors: function (team) {
                var ret = "";
                return ret;
            },
            GetConfChampColors: function (team) {
                var ret = "";
                return ret;
            },
            GetChampColors: function (team) {
                var ret = "";
                return ret;
            },
            /**
             * @instance
             * @public
             * @this TSBTool.SNES_TecmoTool
             * @memberof TSBTool.SNES_TecmoTool
             * @param   {TSBTool.Conference}    conf          
             * @param   {string}                proBowlPos
             * @return  {string}
             */
            GetProBowlPlayer: function (conf, proBowlPos) {
                var $t, $t1;
                var ret = "";
                var offset = 0;
                if (conf === TSBTool.Conference.NFC) {
                    offset = (offset + 72) | 0;
                }

                var posIndex = -1;
                switch (proBowlPos) {
                    case "RET1": 
                        posIndex = this.positionNames.length;
                        break;
                    case "RET2": 
                        posIndex = (this.positionNames.length + 1) | 0;
                        break;
                    case "RET3": 
                        posIndex = (this.positionNames.length + 2) | 0;
                        break;
                    default: 
                        posIndex = this.GetPositionIndex(proBowlPos);
                        break;
                }
                var loc = (((this.mProwbowlStartingLoc + offset) | 0) + (Bridge.Int.mul(2, posIndex))) | 0;

                var teamIndex = ($t = this.OutputRom)[System.Array.index(((loc + 1) | 0), $t)];
                var pos = ($t1 = this.OutputRom)[System.Array.index(loc, $t1)];

                var team = TSBTool.TecmoTool.Teams.getItem(teamIndex);
                ret = System.String.format("{0},{1},{2},{3}", System.Enum.toString(TSBTool.Conference, conf), Bridge.toString(proBowlPos), team, System.Enum.toString(TSBTool.TSBPlayer, pos));

                return ret;
            },
            GetConferenceProBowlPlayers: function (conf) {
                var builder = new System.Text.StringBuilder("", 500);
                for (var i = 0; i < this.positionNames.length; i = (i + 1) | 0) {
                    builder.append(this.GetProBowlPlayer(conf, this.positionNames[System.Array.index(i, this.positionNames)]));
                    builder.append("\r\n");
                }
                builder.append(this.GetProBowlPlayer(conf, "RET1"));
                builder.append("\r\n");
                builder.append(this.GetProBowlPlayer(conf, "RET2"));
                builder.append("\r\n");
                builder.append(this.GetProBowlPlayer(conf, "RET3"));
                builder.append("\r\n");

                return builder.toString();
            },
            ProcessText: function (text) {
                var parser = new TSBTool.InputParser.$ctor1(this);
                text = System.String.replaceAll(text, "\r\n", "\n");
                var lines = System.String.split(text, System.String.toCharArray(("\n"), 0, ("\n").length).map(function (i) {{ return String.fromCharCode(i); }}));
                parser.ProcessLines(lines);
            }
        }
    });

    Bridge.define("TSBTool2.SNES_TSB3_ScheduleHelper", {
        inherits: [TSBTool2.SNES_ScheduleHelper],
        fields: {
            weeks: null
        },
        ctors: {
            init: function () {
                this.weeks = System.Array.init([
                    1503244, 
                    1503274, 
                    1503304, 
                    1503334, 
                    1503364, 
                    1503394, 
                    1503424, 
                    1503454, 
                    1503484, 
                    1503514, 
                    1503544, 
                    1503574, 
                    1503604, 
                    1503634, 
                    1503664, 
                    1503694, 
                    1503724
                ], System.Int32);
            },
            ctor: function (tool) {
                this.$initialize();
                TSBTool2.SNES_ScheduleHelper.ctor.call(this, tool);
            }
        },
        methods: {
            GameLocation: function (week, gameOfweek) {
                var location = this.weeks[System.Array.index(week, this.weeks)];
                var retVal = (location + (Bridge.Int.mul(2, gameOfweek))) | 0;
                return retVal;
            }
        }
    });

    Bridge.define("TSBTool2.TSB2Tool", {
        inherits: [TSBTool2.ITecmoTool,TSBTool.ITecmoContent],
        statics: {
            fields: {
                BYTES_PER_PLAYER: 0,
                NAME_TABLE_SIZE: 0,
                BANK_1_PLAYER_ATTRIBUTES_START: 0,
                BANK_2_PLAYER_ATTRIBUTES_START: 0,
                BANK_3_PLAYER_ATTRIBUTES_START: 0,
                tsb2_name_string_table_1_offset: 0,
                tsb2_name_string_table_2_first_ptr: 0,
                tsb2_name_string_table_2_offset: 0,
                tsb2_name_string_table_3_first_ptr: 0,
                tsb2_name_string_table_3_offset: 0,
                tsb2_team_name_string_table_first_ptr: 0,
                tsb2_team_name_string_table_offset: 0,
                TEAM_NAME_STRING_TABLE_SIZE: 0,
                schedule_start_season_1: 0,
                schedule_start_season_2: 0,
                schedule_start_season_3: 0,
                team_sim_start_season_1: 0,
                team_sim_start_season_2: 0,
                team_sim_start_season_3: 0,
                team_sim_size: 0,
                bills_kr_loc_season_2: 0,
                bills_kr_loc_season_3: 0,
                playbook_team_size: 0,
                pro_bowl_ptr_location_season_1: 0,
                pro_bowl_ptr_location_season_2: 0,
                pro_bowl_ptr_location_season_3: 0,
                ShowPlayerSimData: false,
                ShowPlaybooks: false,
                ShowSchedule: false,
                positionNames: null,
                teams: null
            },
            ctors: {
                init: function () {
                    this.BYTES_PER_PLAYER = 5;
                    this.NAME_TABLE_SIZE = 18328;
                    this.BANK_1_PLAYER_ATTRIBUTES_START = 2017280;
                    this.BANK_2_PLAYER_ATTRIBUTES_START = 2050048;
                    this.BANK_3_PLAYER_ATTRIBUTES_START = 2082816;
                    this.tsb2_name_string_table_1_offset = 1966080;
                    this.tsb2_name_string_table_2_first_ptr = 2031672;
                    this.tsb2_name_string_table_2_offset = 1998848;
                    this.tsb2_name_string_table_3_first_ptr = 2064440;
                    this.tsb2_name_string_table_3_offset = 2031616;
                    this.tsb2_team_name_string_table_first_ptr = 28672;
                    this.tsb2_team_name_string_table_offset = -32768;
                    this.TEAM_NAME_STRING_TABLE_SIZE = 1472;
                    this.schedule_start_season_1 = 1503244;
                    this.schedule_start_season_2 = 1503748;
                    this.schedule_start_season_3 = 1504280;
                    this.team_sim_start_season_1 = 2023424;
                    this.team_sim_start_season_2 = 2056192;
                    this.team_sim_start_season_3 = 2088960;
                    this.team_sim_size = 102;
                    this.bills_kr_loc_season_2 = 938518;
                    this.bills_kr_loc_season_3 = 938578;
                    this.playbook_team_size = 8;
                    this.pro_bowl_ptr_location_season_1 = 937984;
                    this.pro_bowl_ptr_location_season_2 = 937986;
                    this.pro_bowl_ptr_location_season_3 = 937988;
                    this.ShowPlayerSimData = true;
                    this.ShowPlaybooks = true;
                    this.ShowSchedule = true;
                    this.positionNames = function (_o1) {
                            _o1.add("QB1");
                            _o1.add("QB2");
                            _o1.add("RB1");
                            _o1.add("RB2");
                            _o1.add("RB3");
                            _o1.add("RB4");
                            _o1.add("WR1");
                            _o1.add("WR2");
                            _o1.add("WR3");
                            _o1.add("WR4");
                            _o1.add("TE1");
                            _o1.add("TE2");
                            _o1.add("C");
                            _o1.add("LG");
                            _o1.add("RG");
                            _o1.add("LT");
                            _o1.add("RT");
                            _o1.add("RE");
                            _o1.add("NT");
                            _o1.add("LE");
                            _o1.add("RE2");
                            _o1.add("NT2");
                            _o1.add("LE2");
                            _o1.add("ROLB");
                            _o1.add("RILB");
                            _o1.add("LILB");
                            _o1.add("LOLB");
                            _o1.add("LB5");
                            _o1.add("RCB");
                            _o1.add("LCB");
                            _o1.add("DB1");
                            _o1.add("DB2");
                            _o1.add("FS");
                            _o1.add("SS");
                            _o1.add("DB3");
                            _o1.add("K");
                            _o1.add("P");
                            return _o1;
                        }(new (System.Collections.Generic.List$1(System.String)).ctor());
                    this.teams = function (_o2) {
                            _o2.add("bills");
                            _o2.add("colts");
                            _o2.add("dolphins");
                            _o2.add("patriots");
                            _o2.add("jets");
                            _o2.add("bengals");
                            _o2.add("browns");
                            _o2.add("oilers");
                            _o2.add("steelers");
                            _o2.add("broncos");
                            _o2.add("chiefs");
                            _o2.add("raiders");
                            _o2.add("chargers");
                            _o2.add("seahawks");
                            _o2.add("cowboys");
                            _o2.add("giants");
                            _o2.add("eagles");
                            _o2.add("cardinals");
                            _o2.add("redskins");
                            _o2.add("bears");
                            _o2.add("lions");
                            _o2.add("packers");
                            _o2.add("vikings");
                            _o2.add("buccaneers");
                            _o2.add("falcons");
                            _o2.add("rams");
                            _o2.add("saints");
                            _o2.add("49ers");
                            return _o2;
                        }(new (System.Collections.Generic.List$1(System.String)).ctor());
                }
            },
            methods: {
                IsTecmoSuperBowl2Rom: function (rom) {
                    var retVal = false;
                    if (rom != null && rom.length > 0) {
                        var results = TSBTool.StaticUtils.FindStringInFile("CONSECUTIVE", rom, 1828064, 1830688);
                        if (results.Count > 0) {
                            retVal = true;
                        }
                    }
                    return retVal;
                },
                GetTeamIndex: function (team) {
                    return TSBTool2.TSB2Tool.teams.indexOf(team);
                },
                GetTeamFromIndex: function (i) {
                    return TSBTool2.TSB2Tool.teams.getItem(i);
                }
            }
        },
        fields: {
            OutputRom: null,
            BYTES_PER_QB: 0,
            tsb2_name_string_table_1_first_ptr: 0,
            bills_kr_loc_season_1: 0,
            playbook_start: null,
            pro_bowl_playbook: 0,
            ShowOffPref: false,
            eighteenWeeks: null,
            seventeenWeeks: null
        },
        props: {
            RomVersion: {
                get: function () {
                    return TSBTool.ROM_TYPE.SNES_TSB2;
                }
            }
        },
        alias: [
            "OutputRom", "TSBTool$ITecmoContent$OutputRom",
            "OutputRom", "TSBTool2$ITecmoTool$OutputRom",
            "ShowOffPref", "TSBTool$ITecmoContent$ShowOffPref",
            "RomVersion", "TSBTool$ITecmoContent$RomVersion",
            "SetByte", "TSBTool$ITecmoContent$SetByte",
            "SetByte", "TSBTool2$ITecmoTool$SetByte",
            "IsValidPosition", "TSBTool2$ITecmoTool$IsValidPosition",
            "GetSchedule", "TSBTool$ITecmoContent$GetSchedule",
            "GetSchedule", "TSBTool2$ITecmoTool$GetSchedule",
            "SetQBAbilities", "TSBTool2$ITecmoTool$SetQBAbilities",
            "SetOLPlayerAbilities", "TSBTool2$ITecmoTool$SetOLPlayerAbilities",
            "SetKickerAbilities", "TSBTool2$ITecmoTool$SetKickerAbilities",
            "SetPunterAbilities", "TSBTool2$ITecmoTool$SetPunterAbilities",
            "SetSkillPlayerAbilities", "TSBTool2$ITecmoTool$SetSkillPlayerAbilities",
            "SetDefensivePlayerAbilities", "TSBTool2$ITecmoTool$SetDefensivePlayerAbilities",
            "SetFace", "TSBTool2$ITecmoTool$SetFace",
            "InsertPlayerName", "TSBTool2$ITecmoTool$InsertPlayerName",
            "GetTeams", "TSBTool2$ITecmoTool$GetTeams",
            "GetProBowlPlayers", "TSBTool$ITecmoContent$GetProBowlPlayers",
            "SetProBowlPlayer", "TSBTool2$ITecmoTool$SetProBowlPlayer",
            "SetQBSimData", "TSBTool2$ITecmoTool$SetQBSimData",
            "SetSkillSimData", "TSBTool2$ITecmoTool$SetSkillSimData",
            "SetDefensiveSimData", "TSBTool2$ITecmoTool$SetDefensiveSimData",
            "SetKickingSimData", "TSBTool2$ITecmoTool$SetKickingSimData",
            "SetPuntingSimData", "TSBTool2$ITecmoTool$SetPuntingSimData",
            "SetTeamSimData", "TSBTool2$ITecmoTool$SetTeamSimData",
            "GetTeamPlayers", "TSBTool2$ITecmoTool$GetTeamPlayers",
            "GetTeamName", "TSBTool2$ITecmoTool$GetTeamName",
            "GetTeamCity", "TSBTool2$ITecmoTool$GetTeamCity",
            "GetTeamAbbreviation", "TSBTool2$ITecmoTool$GetTeamAbbreviation",
            "SetTeamAbbreviation", "TSBTool2$ITecmoTool$SetTeamAbbreviation",
            "SetTeamName", "TSBTool2$ITecmoTool$SetTeamName",
            "SetTeamCity", "TSBTool2$ITecmoTool$SetTeamCity",
            "SetPlaybook", "TSBTool2$ITecmoTool$SetPlaybook",
            "SetYear", "TSBTool2$ITecmoTool$SetYear",
            "GetKey", "TSBTool$ITecmoContent$GetKey",
            "GetKey", "TSBTool2$ITecmoTool$GetKey",
            "GetAll", "TSBTool2$ITecmoTool$GetAll",
            "GetAll$1", "TSBTool$ITecmoContent$GetAll",
            "GetAll$1", "TSBTool2$ITecmoTool$GetAll$1",
            "SetKickReturner", "TSBTool2$ITecmoTool$SetKickReturner",
            "SetPuntReturner", "TSBTool2$ITecmoTool$SetPuntReturner",
            "ApplySchedule", "TSBTool2$ITecmoTool$ApplySchedule",
            "ApplySet", "TSBTool$ITecmoContent$ApplySet",
            "ApplySet", "TSBTool2$ITecmoTool$ApplySet",
            "ProcessText", "TSBTool$ITecmoContent$ProcessText",
            "ProcessText", "TSBTool2$ITecmoTool$ProcessText",
            "SaveRom", "TSBTool$ITecmoContent$SaveRom"
        ],
        ctors: {
            init: function () {
                this.BYTES_PER_QB = 6;
                this.tsb2_name_string_table_1_first_ptr = 1998904;
                this.bills_kr_loc_season_1 = 938458;
                this.playbook_start = System.Array.init([
                    941350, 
                    941590, 
                    941830
                ], System.Int32);
                this.pro_bowl_playbook = 24071;
                this.eighteenWeeks = System.Array.init([14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14], System.Int32);
                this.seventeenWeeks = System.Array.init([14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14], System.Int32);
            },
            $ctor1: function (rom) {
                this.$initialize();
                this.OutputRom = rom;
                this.Init();
            },
            ctor: function () {
                this.$initialize();
                this.Init();
            }
        },
        methods: {
            Init: function () {
                this.BYTES_PER_QB = 6;
                this.tsb2_name_string_table_1_first_ptr = 1998904;

                TSBTool2.TSB2Tool.teams = function (_o1) {
                        _o1.add("bills");
                        _o1.add("colts");
                        _o1.add("dolphins");
                        _o1.add("patriots");
                        _o1.add("jets");
                        _o1.add("bengals");
                        _o1.add("browns");
                        _o1.add("oilers");
                        _o1.add("steelers");
                        _o1.add("broncos");
                        _o1.add("chiefs");
                        _o1.add("raiders");
                        _o1.add("chargers");
                        _o1.add("seahawks");
                        _o1.add("cowboys");
                        _o1.add("giants");
                        _o1.add("eagles");
                        _o1.add("cardinals");
                        _o1.add("redskins");
                        _o1.add("bears");
                        _o1.add("lions");
                        _o1.add("packers");
                        _o1.add("vikings");
                        _o1.add("buccaneers");
                        _o1.add("falcons");
                        _o1.add("rams");
                        _o1.add("saints");
                        _o1.add("49ers");
                        return _o1;
                    }(new (System.Collections.Generic.List$1(System.String)).ctor());
            },
            SetByte: function (location, b) {
                var $t;
                ($t = this.OutputRom)[System.Array.index(location, $t)] = b;
            },
            IsValidPosition: function (pos) {
                return TSBTool2.TSB2Tool.positionNames.indexOf(pos) > -1;
            },
            GetSchedule: function (season) {
                var helper = new TSBTool2.SNES_ScheduleHelper(this);
                switch (season) {
                    case 1: 
                        helper.SetWeekOneLocation(TSBTool2.TSB2Tool.schedule_start_season_1, this.seventeenWeeks, TSBTool2.TSB2Tool.teams);
                        break;
                    case 2: 
                        helper.SetWeekOneLocation(TSBTool2.TSB2Tool.schedule_start_season_2, this.eighteenWeeks, TSBTool2.TSB2Tool.teams);
                        break;
                    case 3: 
                        helper.SetWeekOneLocation(TSBTool2.TSB2Tool.schedule_start_season_3, this.seventeenWeeks, TSBTool2.TSB2Tool.teams);
                        break;
                }
                return helper.GetSchedule();
            },
            GetPlayerIndex: function (team, position) {
                var teamIndex = TSBTool2.TSB2Tool.teams.indexOf(team);
                var positionIndex = TSBTool2.TSB2Tool.positionNames.indexOf(position);
                var retVal = (Bridge.Int.mul(teamIndex, TSBTool2.TSB2Tool.positionNames.Count) + positionIndex) | 0;
                return retVal;
            },
            GetPlayerAttributeLocation: function (season, team, position) {
                var retVal = -1;
                var attributeStart = TSBTool2.TSB2Tool.BANK_3_PLAYER_ATTRIBUTES_START;
                switch (season) {
                    case 1: 
                        attributeStart = TSBTool2.TSB2Tool.BANK_1_PLAYER_ATTRIBUTES_START;
                        break;
                    case 2: 
                        attributeStart = TSBTool2.TSB2Tool.BANK_2_PLAYER_ATTRIBUTES_START;
                        break;
                }
                var teamByteSize = 185;
                var teamIndex = TSBTool2.TSB2Tool.teams.indexOf(team);
                var positionIndex = TSBTool2.TSB2Tool.positionNames.indexOf(position);
                var teamStart = (attributeStart + (Bridge.Int.mul(teamIndex, teamByteSize))) | 0;
                switch (positionIndex) {
                    case 0: 
                        retVal = teamStart;
                        break;
                    case 1: 
                        retVal = (teamStart + 7) | 0;
                        break;
                    case 2: 
                    case 3: 
                    case 4: 
                    case 5: 
                    case 6: 
                    case 7: 
                    case 8: 
                    case 9: 
                    case 10: 
                    case 11: 
                        retVal = (((teamStart + Bridge.Int.mul(TSBTool2.TSB2Tool.BYTES_PER_PLAYER, positionIndex)) | 0) + 4) | 0;
                        break;
                    case 12: 
                        retVal = (teamStart + 64) | 0;
                        break;
                    case 13: 
                        retVal = (teamStart + 68) | 0;
                        break;
                    case 14: 
                        retVal = (teamStart + 72) | 0;
                        break;
                    case 15: 
                        retVal = (teamStart + 76) | 0;
                        break;
                    case 16: 
                        retVal = (teamStart + 80) | 0;
                        break;
                    case 36: 
                        retVal = (teamStart + 180) | 0;
                        break;
                    default: 
                        retVal = (((teamStart + 84) | 0) + Bridge.Int.mul(5, (((positionIndex - 17) | 0)))) | 0;
                        break;
                }

                return retVal;
            },
            GetQBAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7, $t8, $t9, $t10, $t11, $t12, $t13;
                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);
                var ps = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t4)]);
                var pc = TSBTool.StaticUtils.GetSecondNibble(($t5 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t5)]);
                var pa = TSBTool.StaticUtils.GetFirstNibble(($t6 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t6)]);
                var ar = TSBTool.StaticUtils.GetSecondNibble(($t7 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t7)]);
                var co = TSBTool.StaticUtils.GetSecondNibble(($t8 = this.OutputRom)[System.Array.index(((location + 5) | 0), $t8)]);
                var bb = TSBTool.StaticUtils.GetFirstNibble(($t9 = this.OutputRom)[System.Array.index(((location + 6) | 0), $t9)]);
                var sp = TSBTool.StaticUtils.GetSecondNibble(($t10 = this.OutputRom)[System.Array.index(((location + 6) | 0), $t10)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb, ps, pc, pa, ar, co], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                if (TSBTool2.TSB2Tool.ShowPlayerSimData) {
                    location = this.GetSimLocation(season, team, position);
                    retVal = (retVal || "") + ((System.String.format("[{0:X2},{1:X2},{2:X2}]", Bridge.box(($t11 = this.OutputRom)[System.Array.index(location, $t11)], System.Byte), Bridge.box(($t12 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t12)], System.Byte), Bridge.box(($t13 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t13)], System.Byte))) || "");
                }
                return retVal;
            },
            SetQBAbilities: function (season, team, qb, abilities) {
                var $t, $t1;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                if (!Bridge.referenceEquals(qb, "QB1") && !Bridge.referenceEquals(qb, "QB2")) {
                    throw new System.ArgumentException.$ctor1("Invalid qb position " + (qb || ""));
                }

                var location = this.GetPlayerAttributeLocation(season, team, qb);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var ps_pc = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(5, abilities)], abilities[System.Array.index(6, abilities)]);
                var pa_ar = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(7, abilities)], abilities[System.Array.index(8, abilities)]);
                var unk1 = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(((location + 5) | 0), $t)]);
                var unk1_co = TSBTool.StaticUtils.CombineNibbles(unk1, abilities[System.Array.index(9, abilities)]);
                var unk2 = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(((location + 6) | 0), $t1)]);
                var bb_unk2 = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], unk2);
                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 3) | 0), ps_pc);
                this.SetByte(((location + 4) | 0), pa_ar);
                this.SetByte(((location + 5) | 0), unk1_co);
                this.SetByte(((location + 6) | 0), bb_unk2);
            },
            GetOLPlayerAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4;
                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);
                var bb = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t4)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                return retVal;
            },
            SetOLPlayerAbilities: function (season, team, pos, abilities) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var posIndex = TSBTool2.TSB2Tool.positionNames.indexOf(pos);
                if (posIndex < 12 || posIndex > 16) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes C,RG,RT,LG,LT) " + (pos || ""));
                }

                var location = this.GetPlayerAttributeLocation(season, team, pos);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var unk1 = TSBTool.StaticUtils.GetSecondNibble(($t = this.OutputRom)[System.Array.index(((location + 3) | 0), $t)]);
                var bb_unk1 = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], unk1);
                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 3) | 0), bb_unk1);
            },
            GetKickerAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7, $t8;
                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);
                var kp = TSBTool.StaticUtils.GetSecondNibble(($t4 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t4)]);
                var ka = TSBTool.StaticUtils.GetFirstNibble(($t5 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t5)]);
                var ab = TSBTool.StaticUtils.GetSecondNibble(($t6 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t6)]);
                var bb = TSBTool.StaticUtils.GetFirstNibble(($t7 = this.OutputRom)[System.Array.index(((location + 5) | 0), $t7)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb, kp, ka, ab], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                if (TSBTool2.TSB2Tool.ShowPlayerSimData) {
                    location = this.GetSimLocation(season, team, position);
                    retVal = (retVal || "") + ((System.String.format("[{0:X}]", [Bridge.box((($t8 = this.OutputRom)[System.Array.index(location, $t8)] >> 4), System.Int32)])) || "");
                }
                return retVal;
            },
            SetKickerAbilities: function (season, team, position, abilities) {
                var $t, $t1;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                if (!Bridge.referenceEquals(position, "K")) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes K) " + (position || ""));
                }

                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var unk1 = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(((location + 3) | 0), $t)]);
                var unk1_kp = TSBTool.StaticUtils.CombineNibbles(unk1, abilities[System.Array.index(5, abilities)]);
                var ka_ab = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(6, abilities)], abilities[System.Array.index(7, abilities)]);
                var unk2 = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(((location + 5) | 0), $t1)]);
                var bb_unk2 = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], unk2);

                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 3) | 0), unk1_kp);
                this.SetByte(((location + 4) | 0), ka_ab);
                this.SetByte(((location + 5) | 0), bb_unk2);
            },
            GetPunterAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7;
                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);
                var kp = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t4)]);
                var ab = TSBTool.StaticUtils.GetSecondNibble(($t5 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t5)]);
                var bb = TSBTool.StaticUtils.GetFirstNibble(($t6 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t6)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb, kp, ab], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                if (TSBTool2.TSB2Tool.ShowPlayerSimData) {
                    location = this.GetSimLocation(season, team, position);
                    retVal = (retVal || "") + ((System.String.format("[{0:X}]", [Bridge.box((($t7 = this.OutputRom)[System.Array.index(location, $t7)] & 15), System.Int32)])) || "");
                }
                return retVal;
            },
            SetPunterAbilities: function (season, team, position, abilities) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                if (!Bridge.referenceEquals(position, "P")) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes P) " + (position || ""));
                }

                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var kp_ab = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(5, abilities)], abilities[System.Array.index(6, abilities)]);
                var unk1 = TSBTool.StaticUtils.GetSecondNibble(($t = this.OutputRom)[System.Array.index(((location + 4) | 0), $t)]);
                var bb_unk1 = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], unk1);

                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 3) | 0), kp_ab);
                this.SetByte(((location + 4) | 0), bb_unk1);
            },
            GetPlayerAbilities: function (season, team, position) {
                switch (position) {
                    case "QB1": 
                    case "QB2": 
                        return this.GetQBAbilities(season, team, position);
                    case "C": 
                    case "RG": 
                    case "LG": 
                    case "RT": 
                    case "LT": 
                        return this.GetOLPlayerAbilities(season, team, position);
                    case "K": 
                        return this.GetKickerAbilities(season, team, position);
                    case "P": 
                        return this.GetPunterAbilities(season, team, position);
                }
                return this.GetSkill_DefPlayerAbilities(season, team, position);
            },
            GetSkill_DefPlayerAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7, $t8, $t9, $t10;
                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);

                var bc = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t4)]);
                var rec = TSBTool.StaticUtils.GetSecondNibble(($t5 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t5)]);
                var bb = TSBTool.StaticUtils.GetFirstNibble(($t6 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t6)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb, bc, rec], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                if (TSBTool2.TSB2Tool.ShowPlayerSimData) {
                    location = this.GetSimLocation(season, team, position);
                    retVal = (retVal || "") + ((System.String.format("[{0:X2},{1:X2},{2:X2}", Bridge.box(($t7 = this.OutputRom)[System.Array.index(location, $t7)], System.Byte), Bridge.box(($t8 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t8)], System.Byte), Bridge.box(($t9 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t9)], System.Byte))) || "");
                    if (TSBTool2.TSB2Tool.positionNames.indexOf(position) < 13) {
                        retVal = (retVal || "") + (("," + (System.Byte.format(($t10 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t10)], "X2") || "")) || "");
                    }
                    retVal = (retVal || "") + "]";
                }
                return retVal;
            },
            SetSkillPlayerAbilities: function (season, team, position, abilities) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var posIndex = TSBTool2.TSB2Tool.positionNames.indexOf(position);
                if (posIndex < 2 || posIndex > 11) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes RB1=TE2)" + (position || ""));
                }

                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var bc_rec = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(5, abilities)], abilities[System.Array.index(6, abilities)]);
                var unk1 = TSBTool.StaticUtils.GetSecondNibble(($t = this.OutputRom)[System.Array.index(((location + 4) | 0), $t)]);
                var bb_unk1 = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], unk1);
                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 3) | 0), bc_rec);
                this.SetByte(((location + 4) | 0), bb_unk1);
            },
            SetDefensivePlayerAbilities: function (season, team, position, abilities) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var posIndex = TSBTool2.TSB2Tool.positionNames.indexOf(position);
                if (posIndex < 17 || posIndex > 34) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes RE-DB3)" + (position || ""));
                }

                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var pi_qu = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(5, abilities)], abilities[System.Array.index(6, abilities)]);
                var unk1 = TSBTool.StaticUtils.GetSecondNibble(($t = this.OutputRom)[System.Array.index(((location + 4) | 0), $t)]);
                var bb_unk1 = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], unk1);
                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 3) | 0), pi_qu);
                this.SetByte(((location + 4) | 0), bb_unk1);
            },
            GetFace: function (season, team, position) {
                var $t;
                var location = (this.GetPlayerAttributeLocation(season, team, position) + 2) | 0;
                var retVal = ($t = this.OutputRom)[System.Array.index(location, $t)];
                return retVal;
            },
            SetFace: function (season, team, position, face) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args$1(season, team, position);
                var location = (this.GetPlayerAttributeLocation(season, team, position) + 2) | 0;
                var lowNibble = TSBTool.StaticUtils.GetSecondNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var highNibble = TSBTool.StaticUtils.GetFirstNibble((face & 255));
                var face_b = TSBTool.StaticUtils.CombineNibbles(highNibble, lowNibble);
                this.SetByte(location, face_b);
            },
            GetPlayerName: function (season, team, position, playerNumber) {
                var retVal = "fakeNEWS!!!";
                var first_ptr = this.tsb2_name_string_table_1_first_ptr;
                var offset = TSBTool2.TSB2Tool.tsb2_name_string_table_1_offset;
                switch (season) {
                    case 2: 
                        first_ptr = TSBTool2.TSB2Tool.tsb2_name_string_table_2_first_ptr;
                        offset = TSBTool2.TSB2Tool.tsb2_name_string_table_2_offset;
                        break;
                    case 3: 
                        first_ptr = TSBTool2.TSB2Tool.tsb2_name_string_table_3_first_ptr;
                        offset = TSBTool2.TSB2Tool.tsb2_name_string_table_3_offset;
                        break;
                }
                var playerIndex = this.GetPlayerIndex(team, position);
                var name = TSBTool.StaticUtils.GetStringTableString(this.OutputRom, playerIndex, first_ptr, offset);
                playerNumber.v = (name.charCodeAt(0)) & 255;
                if (playerNumber.v === 42) {
                    playerNumber.v = 0;
                }
                name = name.substr(1);
                for (var i = 0; i < name.length; i = (i + 1) | 0) {
                    if (Bridge.isUpper(name.charCodeAt(i))) {
                        retVal = (name.substr(0, i) || "") + " " + (name.substr(i) || "");
                        break;
                    }
                }
                return retVal;
            },
            InsertPlayerName: function (season, currentTeam, position, fname, lname, jerseyNumber) {
                var first_ptr = this.tsb2_name_string_table_1_first_ptr;
                var offset = TSBTool2.TSB2Tool.tsb2_name_string_table_1_offset;
                switch (season) {
                    case 2: 
                        first_ptr = TSBTool2.TSB2Tool.tsb2_name_string_table_2_first_ptr;
                        offset = TSBTool2.TSB2Tool.tsb2_name_string_table_2_offset;
                        break;
                    case 3: 
                        first_ptr = TSBTool2.TSB2Tool.tsb2_name_string_table_3_first_ptr;
                        offset = TSBTool2.TSB2Tool.tsb2_name_string_table_3_offset;
                        break;
                }
                var playerIndex = this.GetPlayerIndex(currentTeam, position);
                var insertThis = String.fromCharCode(jerseyNumber) + (fname.toLowerCase() || "") + (lname.toUpperCase() || "");
                var stringsInTable = Bridge.Int.mul(TSBTool2.TSB2Tool.teams.Count, TSBTool2.TSB2Tool.positionNames.Count);
                TSBTool.StaticUtils.SetStringTableString(this.OutputRom, playerIndex, insertThis, first_ptr, offset, stringsInTable, TSBTool2.TSB2Tool.NAME_TABLE_SIZE);
            },
            GetTeams: function (season) {
                var $t;
                var builder = new System.Text.StringBuilder();
                $t = Bridge.getEnumerator(TSBTool2.TSB2Tool.teams);
                try {
                    while ($t.moveNext()) {
                        var team = $t.Current;
                        this.GetTeam(season, team, builder);
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
                var retVal = builder.toString();
                return retVal;
            },
            GetProBowlPlayers: function (season) {
                var builder = new System.Text.StringBuilder();
                this.GetProBowlTeam(season, TSBTool2.Conference.AFC, builder);
                this.GetProBowlTeam(season, TSBTool2.Conference.NFC, builder);
                var retVal = builder.toString();
                return retVal;
            },
            GetTeam: function (season, team, builder) {
                var i = TSBTool2.TSB2Tool.teams.indexOf(team);
                builder.append("TEAM = ");
                builder.append(team);
                builder.append(",");
                builder.append(this.GetTeamSimData(season, team));
                builder.append("\n");
                if (TSBTool2.TSB2Tool.ShowPlaybooks) {
                    builder.append(this.GetPlaybook(season, team));
                    builder.append("\n");
                }
                builder.append(System.String.format("TEAM_ABB={0},TEAM_CITY={1},TEAM_NAME={2}\n", this.GetTeamAbbreviation(i), this.GetTeamCity(i), this.GetTeamName(i)));
                builder.append(this.GetTeamPlayers(season, team));
            },
            GetProBowlTeam: function (season, conf, builder) {
                var $t, $t1, $t2;
                var location = this.GetProbowlTeamLocation(season, conf);
                builder.append(System.String.format("# {0} ProBowl players\n", [System.Enum.toString(TSBTool2.Conference, conf)]));
                var team = "";
                var playerPos = "";
                var playerPositionIndex = -1;
                var playerIndex = -1;
                var teamIndex = -1;
                $t = Bridge.getEnumerator(TSBTool2.TSB2Tool.positionNames);
                try {
                    while ($t.moveNext()) {
                        var pos = $t.Current;
                        playerIndex = ((($t1 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t1)] << 8) + ($t2 = this.OutputRom)[System.Array.index(location, $t2)]) | 0;
                        teamIndex = (Bridge.Int.div(playerIndex, TSBTool2.TSB2Tool.positionNames.Count)) | 0;
                        playerPositionIndex = playerIndex % TSBTool2.TSB2Tool.positionNames.Count;
                        playerPos = TSBTool2.TSB2Tool.positionNames.getItem(playerPositionIndex);
                        team = TSBTool2.TSB2Tool.teams.getItem(teamIndex);
                        builder.append(System.String.format("{0},{1},{2},{3}\n", System.Enum.toString(TSBTool2.Conference, conf), pos, team, playerPos));
                        location = (location + 2) | 0;
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
                builder.append("\n");
            },
            SetProBowlPlayer: function (season, conf, proBowlPos, fromTeam, fromTeamPos) {
                var playerIndex = (Bridge.Int.mul(TSBTool2.TSB2Tool.teams.indexOf(fromTeam), TSBTool2.TSB2Tool.positionNames.Count) + fromTeamPos) | 0;
                var location = (this.GetProbowlTeamLocation(season, conf) + Bridge.Int.mul(2, TSBTool2.TSB2Tool.positionNames.indexOf(proBowlPos))) | 0;
                var b1 = (playerIndex >> 8) & 255;
                var b2 = (playerIndex & 255) & 255;
                this.SetByte(location, b2);
                this.SetByte(((location + 1) | 0), b1);
            },
            GetProbowlTeamLocation: function (season, conf) {
                var $t, $t1;
                var ptr_location = TSBTool2.TSB2Tool.pro_bowl_ptr_location_season_1;
                switch (season) {
                    case 2: 
                        ptr_location = TSBTool2.TSB2Tool.pro_bowl_ptr_location_season_2;
                        break;
                    case 3: 
                        ptr_location = TSBTool2.TSB2Tool.pro_bowl_ptr_location_season_3;
                        break;
                }
                if (conf === TSBTool2.Conference.NFC) {
                    ptr_location = (ptr_location + 6) | 0;
                }
                var location = ((($t = this.OutputRom)[System.Array.index(((ptr_location + 1) | 0), $t)] << 8) + ($t1 = this.OutputRom)[System.Array.index(ptr_location, $t1)]) | 0;
                var offset = 884736;
                location = (location + offset) | 0;
                return location;
            },
            GetSimLocation: function (season, team, position) {
                var $t, $t1;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var index = TSBTool2.TSB2Tool.teams.indexOf(team);
                var ptr_location = Bridge.Int.mul(index, 2);
                switch (season) {
                    case 1: 
                        ptr_location = (ptr_location + TSBTool2.TSB2Tool.team_sim_start_season_1) | 0;
                        break;
                    case 2: 
                        ptr_location = (ptr_location + TSBTool2.TSB2Tool.team_sim_start_season_2) | 0;
                        break;
                    case 3: 
                        ptr_location = (ptr_location + TSBTool2.TSB2Tool.team_sim_start_season_3) | 0;
                        break;
                }
                var location = (((1966080 + (($t = this.OutputRom)[System.Array.index(((ptr_location + 1) | 0), $t)] << 8)) | 0) + ($t1 = this.OutputRom)[System.Array.index(ptr_location, $t1)]) | 0;
                switch (position) {
                    case "QB1": 
                        break;
                    case "QB2": 
                        location = (location + 3) | 0;
                        break;
                    case "RB1": 
                        location = (location + 6) | 0;
                        break;
                    case "RB2": 
                        location = (location + 10) | 0;
                        break;
                    case "RB3": 
                        location = (location + 14) | 0;
                        break;
                    case "RB4": 
                        location = (location + 18) | 0;
                        break;
                    case "WR1": 
                        location = (location + 22) | 0;
                        break;
                    case "WR2": 
                        location = (location + 26) | 0;
                        break;
                    case "WR3": 
                        location = (location + 30) | 0;
                        break;
                    case "WR4": 
                        location = (location + 34) | 0;
                        break;
                    case "TE1": 
                        location = (location + 38) | 0;
                        break;
                    case "TE2": 
                        location = (location + 42) | 0;
                        break;
                    case "RE": 
                        location = (location + 46) | 0;
                        break;
                    case "NT": 
                        location = (location + 49) | 0;
                        break;
                    case "LE": 
                        location = (location + 52) | 0;
                        break;
                    case "RE2": 
                        location = (location + 55) | 0;
                        break;
                    case "NT2": 
                        location = (location + 58) | 0;
                        break;
                    case "LE2": 
                        location = (location + 61) | 0;
                        break;
                    case "LOLB": 
                        location = (location + 64) | 0;
                        break;
                    case "LILB": 
                        location = (location + 67) | 0;
                        break;
                    case "RILB": 
                        location = (location + 70) | 0;
                        break;
                    case "ROLB": 
                        location = (location + 73) | 0;
                        break;
                    case "LB5": 
                        location = (location + 76) | 0;
                        break;
                    case "RCB": 
                        location = (location + 79) | 0;
                        break;
                    case "LCB": 
                        location = (location + 82) | 0;
                        break;
                    case "DB1": 
                        location = (location + 85) | 0;
                        break;
                    case "DB2": 
                        location = (location + 88) | 0;
                        break;
                    case "FS": 
                        location = (location + 91) | 0;
                        break;
                    case "SS": 
                        location = (location + 94) | 0;
                        break;
                    case "DB3": 
                        location = (location + 97) | 0;
                        break;
                    case "K": 
                    case "P": 
                        location = (location + 100) | 0;
                        break;
                }
                return location;
            },
            /**
             * |012345|
              4 = passing
              5 = scramble
             *
             * @instance
             * @public
             * @this TSBTool2.TSB2Tool
             * @memberof TSBTool2.TSB2Tool
             * @param   {number}            season      
             * @param   {string}            team        
             * @param   {string}            position    
             * @param   {Array.<number>}    data        Array of bytes
             * @return  {void}
             */
            SetQBSimData: function (season, team, position, data) {
                var loc = this.GetSimLocation(season, team, position);
                for (var i = 0; i < data.length; i = (i + 1) | 0) {
                    this.SetByte(((loc + i) | 0), ((data[System.Array.index(i, data)]) & 255));
                }
            },
            SetSkillSimData: function (season, team, pos, data) {
                var loc = this.GetSimLocation(season, team, pos);
                for (var i = 0; i < data.length; i = (i + 1) | 0) {
                    this.SetByte(((loc + i) | 0), ((data[System.Array.index(i, data)]) & 255));
                }
            },
            SetDefensiveSimData: function (season, team, pos, data) {
                var loc = this.GetSimLocation(season, team, pos);
                for (var i = 0; i < data.length; i = (i + 1) | 0) {
                    this.SetByte(((loc + i) | 0), ((data[System.Array.index(i, data)]) & 255));
                }
            },
            SetKickingSimData: function (season, team, data) {
                var $t;
                var loc = this.GetSimLocation(season, team, "K");
                var current = (($t = this.OutputRom)[System.Array.index(loc, $t)] & 15) & 255;
                data = (data << 4) & 255;
                data = (data + current) & 255;
                this.SetByte(loc, data);
            },
            SetPuntingSimData: function (season, team, data) {
                var $t;
                var loc = this.GetSimLocation(season, team, "P");
                var current = (($t = this.OutputRom)[System.Array.index(loc, $t)] & 240) & 255;
                data = (data + current) & 255;
                this.SetByte(loc, data);
            },
            GetTeamSimData: function (season, team) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var location = (this.GetSimLocation(season, team, "K") + 1) | 0;
                var retVal = "SimData=0x" + (System.Byte.format(($t = this.OutputRom)[System.Array.index(location, $t)], "X2") || "");
                return retVal;
            },
            SetTeamSimData: function (season, team, data) {
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var theBytes = TSBTool.StaticUtils.GetHexBytes(data);
                var location = (this.GetSimLocation(season, team, "K") + 1) | 0;
                this.SetByte(location, theBytes[System.Array.index(0, theBytes)]);
            },
            GetTeamPlayers: function (season, team) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var builder = new System.Text.StringBuilder();

                $t = Bridge.getEnumerator(TSBTool2.TSB2Tool.positionNames);
                try {
                    while ($t.moveNext()) {
                        var position = $t.Current;
                        this.GetPlayer(season, team, builder, position);
                    }
                } finally {
                    if (Bridge.is($t, System.IDisposable)) {
                        $t.System$IDisposable$Dispose();
                    }
                }
                builder.append("KR,");
                builder.append(this.GetKickReturner(season, team));
                builder.append("\nPR,");
                builder.append(this.GetPuntReturner(season, team));
                builder.append("\n\n");

                var retVal = builder.toString();
                return retVal;
            },
            GetPlayer: function (season, team, builder, position) {
                var playerNumber = { v : 0 };
                var face = 0;
                builder.append(position);
                builder.append(",");
                builder.append(this.GetPlayerName(season, team, position, playerNumber));
                builder.append(",");
                face = this.GetFace(season, team, position);
                builder.append(System.String.format("Face=0x{0:x2},#{1:x},", Bridge.box(face, System.Byte), Bridge.box(playerNumber.v, System.Byte)));
                builder.append(this.GetPlayerAbilities(season, team, position));
                builder.append("\n");
            },
            GetTeamName: function (teamIndex) {
                var retVal = TSBTool.StaticUtils.GetStringTableString(this.OutputRom, teamIndex, TSBTool2.TSB2Tool.tsb2_team_name_string_table_first_ptr, TSBTool2.TSB2Tool.tsb2_team_name_string_table_offset);
                var lastSpace = retVal.lastIndexOf(String.fromCharCode(32));
                retVal = System.String.replaceAll(retVal.substr(((lastSpace + 1) | 0)), "*", "");
                return retVal;
            },
            GetTeamCity: function (teamIndex) {
                var retVal = TSBTool.StaticUtils.GetStringTableString(this.OutputRom, teamIndex, TSBTool2.TSB2Tool.tsb2_team_name_string_table_first_ptr, TSBTool2.TSB2Tool.tsb2_team_name_string_table_offset).substr(5);
                var lastSpace = retVal.lastIndexOf(String.fromCharCode(32));
                if (lastSpace > -1) {
                    retVal = retVal.substr(0, lastSpace);
                }
                return retVal;
            },
            GetTeamAbbreviation: function (teamIndex) {
                var retVal = TSBTool.StaticUtils.GetStringTableString(this.OutputRom, teamIndex, TSBTool2.TSB2Tool.tsb2_team_name_string_table_first_ptr, TSBTool2.TSB2Tool.tsb2_team_name_string_table_offset);
                retVal = retVal.substr(0, 4);
                return retVal;
            },
            SetTeamAbbreviation: function (teamIndex, abb) {
                if (abb != null && abb.length > 0) {
                    var teamString = System.String.format("{0}*{1} {2}*", abb, this.GetTeamCity(teamIndex), this.GetTeamName(teamIndex));
                    TSBTool.StaticUtils.SetStringTableString(this.OutputRom, teamIndex, teamString, TSBTool2.TSB2Tool.tsb2_team_name_string_table_first_ptr, TSBTool2.TSB2Tool.tsb2_team_name_string_table_offset, 30, TSBTool2.TSB2Tool.TEAM_NAME_STRING_TABLE_SIZE);
                }
            },
            SetTeamName: function (teamIndex, name) {
                if (name != null && name.length > 0) {
                    var teamString = System.String.format("{0}*{1} {2}*", this.GetTeamAbbreviation(teamIndex), this.GetTeamCity(teamIndex), name);
                    TSBTool.StaticUtils.SetStringTableString(this.OutputRom, teamIndex, teamString, TSBTool2.TSB2Tool.tsb2_team_name_string_table_first_ptr, TSBTool2.TSB2Tool.tsb2_team_name_string_table_offset, 30, TSBTool2.TSB2Tool.TEAM_NAME_STRING_TABLE_SIZE);
                }
            },
            SetTeamCity: function (teamIndex, city) {
                if (city != null && city.length > 0) {
                    var teamString = System.String.format("{0}*{1} {2}*", this.GetTeamAbbreviation(teamIndex), city, this.GetTeamName(teamIndex));
                    TSBTool.StaticUtils.SetStringTableString(this.OutputRom, teamIndex, teamString, TSBTool2.TSB2Tool.tsb2_team_name_string_table_first_ptr, TSBTool2.TSB2Tool.tsb2_team_name_string_table_offset, 30, TSBTool2.TSB2Tool.TEAM_NAME_STRING_TABLE_SIZE);
                }
            },
            GetPlaybookLocation: function (season, team) {
                var playbookLocation = -1;
                if (Bridge.referenceEquals(team.toUpperCase(), "AFC")) {
                    playbookLocation = (this.playbook_start[System.Array.index(0, this.playbook_start)] + 224) | 0;
                } else {
                    if (Bridge.referenceEquals(team.toUpperCase(), "NFC")) {
                        playbookLocation = (this.playbook_start[System.Array.index(0, this.playbook_start)] + 232) | 0;
                    } else {
                        TSBTool.StaticUtils.CheckTSB2Args(season, team);
                        playbookLocation = (this.playbook_start[System.Array.index(((season - 1) | 0), this.playbook_start)] + Bridge.Int.mul(TSBTool2.TSB2Tool.teams.indexOf(team), TSBTool2.TSB2Tool.playbook_team_size)) | 0;
                    }
                }
                return playbookLocation;
            },
            /**
             * runs = r12345678
             pass = p12345678
             *
             * @instance
             * @public
             * @this TSBTool2.TSB2Tool
             * @memberof TSBTool2.TSB2Tool
             * @param   {number}    season    
             * @param   {string}    team      
             * @param   {string}    runs      
             * @param   {string}    passes
             * @return  {void}
             */
            SetPlaybook: function (season, team, runs, passes) {
                var playbookLocation = this.GetPlaybookLocation(season, team);
                var runBytes = TSBTool.StaticUtils.GetHexBytes(runs.substr(1));
                var passBytes = TSBTool.StaticUtils.GetHexBytes(passes.substr(1));
                if (runBytes.length > 1 && passBytes.length > 1) {
                    this.SetByte(playbookLocation, runBytes[System.Array.index(0, runBytes)]);
                    this.SetByte(((playbookLocation + 1) | 0), runBytes[System.Array.index(1, runBytes)]);
                    this.SetByte(((playbookLocation + 2) | 0), passBytes[System.Array.index(0, passBytes)]);
                    this.SetByte(((playbookLocation + 3) | 0), passBytes[System.Array.index(1, passBytes)]);

                    if (runBytes.length > 3 && passBytes.length > 3) {
                        this.SetByte(((playbookLocation + 4) | 0), runBytes[System.Array.index(2, runBytes)]);
                        this.SetByte(((playbookLocation + 5) | 0), runBytes[System.Array.index(3, runBytes)]);
                        this.SetByte(((playbookLocation + 6) | 0), passBytes[System.Array.index(2, passBytes)]);
                        this.SetByte(((playbookLocation + 7) | 0), passBytes[System.Array.index(3, passBytes)]);
                    }
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR Settings playbook for season:{0} team:{1} data={2} {3}", Bridge.box(season, System.Int32), team, runs, passes));
                }
            },
            GetPlaybook: function (season, team) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7;
                var playbookLocation = this.GetPlaybookLocation(season, team);
                var retVal = System.String.format("PLAYBOOK R{0:X2}{1:X2}{2:X2}{3:X2}, P{4:X2}{5:X2}{6:X2}{7:X2} ", Bridge.box(($t = this.OutputRom)[System.Array.index(playbookLocation, $t)], System.Byte), Bridge.box(($t1 = this.OutputRom)[System.Array.index(((playbookLocation + 1) | 0), $t1)], System.Byte), Bridge.box(($t2 = this.OutputRom)[System.Array.index(((playbookLocation + 4) | 0), $t2)], System.Byte), Bridge.box(($t3 = this.OutputRom)[System.Array.index(((playbookLocation + 5) | 0), $t3)], System.Byte), Bridge.box(($t4 = this.OutputRom)[System.Array.index(((playbookLocation + 2) | 0), $t4)], System.Byte), Bridge.box(($t5 = this.OutputRom)[System.Array.index(((playbookLocation + 3) | 0), $t5)], System.Byte), Bridge.box(($t6 = this.OutputRom)[System.Array.index(((playbookLocation + 6) | 0), $t6)], System.Byte), Bridge.box(($t7 = this.OutputRom)[System.Array.index(((playbookLocation + 7) | 0), $t7)], System.Byte));
                return retVal;
            },
            SetYear: function (year) { },
            GetKey: function () {
                return System.String.format("# TSBTool Forum: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/\r\n# TSBTool2 Forum: https://tecmobowl.org/forums/topic/71072-tsbii-tsbiii-editor-tsbtool2/\r\n# TSB2 Hacking documentation: https://tecmobowl.org/forums/topic/53028-tecmo-super-bowl-ii-hackingresource-documentation/\r\n# Editing: Tecmo Super Bowl II (snes) [{0}]\r\n# Key \r\n# 'SET' commands are supported\r\n# Double click on a team name (or playbook) to bring up the edit Team GUI.\r\n# Double click on a player to bring up the edit player GUI (Click 'Sim Data'\r\n#   button to find out more on Sim Data).\r\n# Attribute Order\r\n# QBs   RS RP MS HP BB PS PC PA AR CO [sim vals]\r\n# Skill RS RP MS HP BB BC RC [sim vals]\r\n# OL    RS RP MS HP BB\r\n# DEF   RS RP MS HP BB PI QU [sim vals]\r\n# K     RS RP MS HP BB KP KA AB [sim val]\r\n# P     RS RP MS HP BB KP AB [sim val]\r\n", [Bridge.box(this.RomVersion, TSBTool.ROM_TYPE, System.Enum.toStringFn(TSBTool.ROM_TYPE))]);
            },
            GetAll: function () {
                var builder = new System.Text.StringBuilder("", 10000);
                builder.append("SEASON 1\n");
                builder.append(this.GetTeams(1));
                if (TSBTool2.TSB2Tool.ShowSchedule) {
                    builder.append(this.GetSchedule(1));
                }

                builder.append("SEASON 2\n");
                builder.append(this.GetTeams(2));
                if (TSBTool2.TSB2Tool.ShowSchedule) {
                    builder.append(this.GetSchedule(2));
                }

                builder.append("SEASON 3\n");
                builder.append(this.GetTeams(3));
                if (TSBTool2.TSB2Tool.ShowSchedule) {
                    builder.append(this.GetSchedule(3));
                }
                return builder.toString();
            },
            GetAll$1: function (season) {
                var builder = new System.Text.StringBuilder("", 5000);
                builder.append("SEASON ");
                builder.append(season);
                builder.append("\n");
                builder.append(this.GetTeams(season));
                if (TSBTool2.TSB2Tool.ShowSchedule) {
                    builder.append(this.GetSchedule(season));
                }
                return builder.toString();
            },
            GetKickReturnLocation: function (season, team) {
                var location = this.bills_kr_loc_season_1;
                switch (season) {
                    case 2: 
                        location = TSBTool2.TSB2Tool.bills_kr_loc_season_2;
                        break;
                    case 3: 
                        location = TSBTool2.TSB2Tool.bills_kr_loc_season_3;
                        break;
                }
                location = (location + (Bridge.Int.mul(2, TSBTool2.TSB2Tool.teams.indexOf(team)))) | 0;
                return location;
            },
            SetKickReturner: function (season, team, position) {
                var location = this.GetKickReturnLocation(season, team);
                var pos_num = TSBTool2.TSB2Tool.positionNames.indexOf(position);
                this.SetByte(location, (pos_num & 255));
            },
            SetPuntReturner: function (season, team, position) {
                var location = (1 + this.GetKickReturnLocation(season, team)) | 0;
                var pos_num = TSBTool2.TSB2Tool.positionNames.indexOf(position);
                this.SetByte(location, (pos_num & 255));
            },
            GetPuntReturner: function (season, team) {
                var $t;
                var location = (1 + this.GetKickReturnLocation(season, team)) | 0;
                var pos_num = ($t = this.OutputRom)[System.Array.index(location, $t)];
                var pos = TSBTool2.TSB2Tool.positionNames.getItem(pos_num);
                return pos;
            },
            GetKickReturner: function (season, team) {
                var $t;
                var location = this.GetKickReturnLocation(season, team);
                var pos_num = ($t = this.OutputRom)[System.Array.index(location, $t)];
                var pos = TSBTool2.TSB2Tool.positionNames.getItem(pos_num);
                return pos;
            },
            ApplySchedule: function (season, scheduleList) {
                var helper = new TSBTool2.SNES_ScheduleHelper(this);
                switch (season) {
                    case 1: 
                        helper.SetWeekOneLocation(TSBTool2.TSB2Tool.schedule_start_season_1, this.seventeenWeeks, TSBTool2.TSB2Tool.teams);
                        break;
                    case 2: 
                        helper.SetWeekOneLocation(TSBTool2.TSB2Tool.schedule_start_season_2, this.eighteenWeeks, TSBTool2.TSB2Tool.teams);
                        break;
                    case 3: 
                        helper.SetWeekOneLocation(TSBTool2.TSB2Tool.schedule_start_season_3, this.seventeenWeeks, TSBTool2.TSB2Tool.teams);
                        break;
                }
                helper.ApplySchedule(scheduleList);
            },
            ApplySet: function (line) {
                TSBTool.StaticUtils.ApplySimpleSet(line, this);
            },
            ProcessText: function (text) {
                var parser = new TSBTool2.InputParser.$ctor1(this);
                parser.ProcessText(text);
            },
            SaveRom: function (fileName) {
                TSBTool.StaticUtils.SaveRom(fileName, this.OutputRom);
            }
        }
    });

    /**
     * Summary description for CXRomTSBTool.
       Still having problems with playbooks.
     Done:
     1. Names and numbers
     2. normal attributes.
     3. Faces
     4. player Sim attributes
     5. team sim attributes
     6. Team Playbooks.
     7. team offensive preference.
     8. team offensive formation
     *
     * @public
     * @class TSBTool.CXRomTSBTool
     * @augments TSBTool.TecmoTool
     */
    Bridge.define("TSBTool.CXRomTSBTool", {
        inherits: [TSBTool.TecmoTool],
        statics: {
            fields: {
                FORTY_NINERS_PLAYBOOK_START: 0
            },
            ctors: {
                init: function () {
                    this.FORTY_NINERS_PLAYBOOK_START = 119696;
                }
            }
        },
        fields: {
            DoSchedule: false,
            fortyNinersQB1SimAttrStart: 0,
            fortyNinersRESimLoc: 0,
            fortyNinersRunPassPreferenceLoc: 0,
            FORTY_NINERS_QB1_POINTER: 0,
            mGetDataPositionOffset: 0,
            FORTY_NINERS_KR_PR_LOC: 0,
            FORTY_NINERS_KR_PR_LOC_1: 0,
            m_ExpansionSegmentEnd: 0,
            m_RomVersionData: null,
            mRomType: 0,
            mAddedFormationRomError: false,
            mFortyNinersUniformLoc: 0,
            mFortyNinersActionSeqLoc: 0,
            m49ersDivChampLoc: 0,
            m49ersConfChampLoc: 0
        },
        props: {
            /**
             * Returns the rom version
             *
             * @instance
             * @public
             * @override
             * @readonly
             * @memberof TSBTool.CXRomTSBTool
             * @function RomVersion
             * @type TSBTool.ROM_TYPE
             */
            RomVersion: {
                get: function () {
                    return this.mRomType;
                }
            },
            NumberOfStringsInTeamStringTable: {
                get: function () {
                    return 123;
                }
            }
        },
        alias: [
            "RomVersion", "TSBTool$ITecmoContent$RomVersion",
            "RomVersion", "TSBTool$ITecmoTool$RomVersion",
            "SetTeamOffensiveFormation", "TSBTool$ITecmoTool$SetTeamOffensiveFormation",
            "SaveRom", "TSBTool$ITecmoContent$SaveRom",
            "SaveRom", "TSBTool$ITecmoTool$SaveRom",
            "SetPuntReturner", "TSBTool$ITecmoTool$SetPuntReturner",
            "SetKickReturner", "TSBTool$ITecmoTool$SetKickReturner",
            "GetAll", "TSBTool$ITecmoTool$GetAll",
            "GetTeamName", "TSBTool$ITecmoTool$GetTeamName",
            "GetTeamCity", "TSBTool$ITecmoTool$GetTeamCity",
            "GetTeamAbbreviation", "TSBTool$ITecmoTool$GetTeamAbbreviation",
            "SetTeamAbbreviation", "TSBTool$ITecmoTool$SetTeamAbbreviation",
            "SetTeamName", "TSBTool$ITecmoTool$SetTeamName",
            "SetTeamCity", "TSBTool$ITecmoTool$SetTeamCity",
            "NumberOfStringsInTeamStringTable", "TSBTool$ITecmoTool$NumberOfStringsInTeamStringTable",
            "SetTeamSimOffensePref", "TSBTool$ITecmoTool$SetTeamSimOffensePref",
            "SetTeamSimData", "TSBTool$ITecmoTool$SetTeamSimData",
            "SetFace", "TSBTool$ITecmoTool$SetFace",
            "ApplySchedule", "TSBTool$ITecmoTool$ApplySchedule",
            "GetSchedule", "TSBTool$ITecmoTool$GetSchedule"
        ],
        ctors: {
            init: function () {
                this.DoSchedule = true;
                this.fortyNinersQB1SimAttrStart = 104897;
                this.fortyNinersRESimLoc = 104921;
                this.fortyNinersRunPassPreferenceLoc = 163803;
                this.FORTY_NINERS_QB1_POINTER = 16048;
                this.mGetDataPositionOffset = 196624;
                this.FORTY_NINERS_KR_PR_LOC = 208050;
                this.FORTY_NINERS_KR_PR_LOC_1 = 260412;
                this.m_ExpansionSegmentEnd = 262128;
                this.mRomType = TSBTool.ROM_TYPE.CXROM_v105;
                this.mAddedFormationRomError = false;
                this.mFortyNinersUniformLoc = 184194;
                this.mFortyNinersActionSeqLoc = 215287;
                this.m49ersDivChampLoc = 215375;
                this.m49ersConfChampLoc = 215503;
            },
            ctor: function (rom, type) {
                this.$initialize();
                TSBTool.TecmoTool.ctor.call(this);
                this.mRomType = type;
                this.Init(rom);
            }
        },
        methods: {
            SetupForCxROM: function () {
                if (this.mRomType === TSBTool.ROM_TYPE.CXROM_v111) {
                    /* Version 1.11*/
                    this.FORTY_NINERS_QB1_POINTER = 254036;
                    this.mGetDataPositionOffset = 221200;
                    this.fortyNinersRunPassPreferenceLoc = 163798;
                    this.FORTY_NINERS_KR_PR_LOC = 208071;
                    this.FORTY_NINERS_KR_PR_LOC_1 = 523601;
                }
                this.mTeamFormationsStartingLoc = 260416;
                this.namePointersStart = 84;
                this.lastPlayerNamePointer = 1764;

                this.faceTeamOffsets = System.Array.init([12306, 12423, 12540, 12657, 12774, 12891, 13008, 13125, 13242, 13359, 13476, 13593, 13710, 13827, 14412, 14061, 14178, 14295, 13944, 14529, 14646, 14763, 14880, 14997, 15114, 15231, 15348, 15465, 14412, 14061, 14178, 14295, 13944, 14529], System.Int32);
            },
            /**
             * Sets the team's offensive formation.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team         
             * @param   {string}    formation
             * @return  {void}
             */
            SetTeamOffensiveFormation: function (team, formation) {
                if (this.RomVersion === TSBTool.ROM_TYPE.CXROM_v111) {
                    if (!this.mAddedFormationRomError) {
                        this.mAddedFormationRomError = true;
                        TSBTool.StaticUtils.AddError("Setting offensive formation on CXROM_v1.11 ROM is not yet supported.");
                    }
                    return;
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex > -1 && teamIndex < 34) {
                    var location = (this.mTeamFormationsStartingLoc + teamIndex) | 0;

                    switch (formation) {
                        case TSBTool.TecmoTool.m2RB_2WR_1TE: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 0;
                            break;
                        case TSBTool.TecmoTool.m1RB_3WR_1TE: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 2;
                            break;
                        case TSBTool.TecmoTool.m1RB_4WR: 
                            this.outputRom[System.Array.index(location, this.outputRom)] = 1;
                            break;
                        default: 
                            TSBTool.StaticUtils.AddError(System.String.format("ERROR! Formation {0} for team '{1}' is invalid.", formation, team));
                            TSBTool.StaticUtils.AddError(System.String.format("  Valid formations are:\n  {0}\n  {1}\n  {2}", TSBTool.TecmoTool.m2RB_2WR_1TE, TSBTool.TecmoTool.m1RB_3WR_1TE, TSBTool.TecmoTool.m1RB_4WR));
                            break;
                    }
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! Team '{0}' is invalid, Offensive Formation not set", [team]));
                }
            },
            IsValidRomSize: function (len) {
                var ret = false;
                if (len.equals(System.Int64(TSBTool.TecmoToolFactory.CXROM_V105_LEN)) || len.equals(System.Int64(TSBTool.TecmoToolFactory.CXROM_V111_LEN))) {
                    ret = true;
                }
                return ret;
            },
            InitRom: function (rom) {
                var ret = false;
                ret = TSBTool.TecmoTool.prototype.InitRom.call(this, rom);
                if (ret) {
                    this.SetupForCxROM();
                    this.m_RomVersionData = System.Array.init(14, 0, System.Byte);
                    for (var i = 0; i < this.m_RomVersionData.length; i = (i + 1) | 0) {
                        this.m_RomVersionData[System.Array.index(i, this.m_RomVersionData)] = this.outputRom[System.Array.index(((i + this.m_ExpansionSegmentEnd) | 0), this.outputRom)];
                    }
                }
                return ret;
            },
            CheckROMVersion: function () {
                var ret = true;
                if (this.outputRom.length > ((this.m_ExpansionSegmentEnd + 20) | 0)) {
                    for (var i = 0; i < this.m_RomVersionData.length; i = (i + 1) | 0) {
                        if (this.outputRom[System.Array.index(((i + this.m_ExpansionSegmentEnd) | 0), this.outputRom)] !== this.m_RomVersionData[System.Array.index(i, this.m_RomVersionData)]) {

                            ret = false;
                            break;
                        }
                    }
                }
                return ret;
            },
            /**
             * Check to see if we overwrote any ROM data after the end of the expansion
             name segment. If we are in GUI mode, prompt the user to confirm that they want to save the
             data.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    filename
             * @return  {void}
             */
            SaveRom: function (filename) {
                if (this.CheckROMVersion()) {
                    TSBTool.TecmoTool.prototype.SaveRom.call(this, filename);
                } else {
                    TSBTool.StaticUtils.ShowError("WARNING!! Expansion team name section has been overwritten, ROM could be messed up.");
                    if (TSBTool.MainClass.GUI_MODE) {
                        if (System.Windows.Forms.MessageBox.Show(null, "ROM could be messed up, do you want to save anyway?", "ERROR!", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) === System.Windows.Forms.DialogResult.Yes) {
                            TSBTool.TecmoTool.prototype.SaveRom.call(this, filename);
                        }
                    } else {
                        TSBTool.TecmoTool.prototype.SaveRom.call(this, filename);
                    }
                }
            },
            /**
             * Gets the position who returns punts.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team
             * @return  {string}
             */
            GetPuntReturner: function (team) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    return TSBTool.TecmoTool.prototype.GetPuntReturner.call(this, team);
                } else {
                    var ret = "";
                    var location = (((this.FORTY_NINERS_KR_PR_LOC + teamIndex) | 0) - 30) | 0;
                    var b = this.outputRom[System.Array.index(location, this.outputRom)];
                    b = b & 15;
                    ret = TSBTool.TecmoTool.positionNames.getItem(b);
                    return ret;
                }
            },
            /**
             * Gets the position who returns kicks.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team
             * @return  {string}
             */
            GetKickReturner: function (team) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    return TSBTool.TecmoTool.prototype.GetKickReturner.call(this, team);
                } else {
                    var ret = "";
                    var location = (((this.FORTY_NINERS_KR_PR_LOC + teamIndex) | 0) - 30) | 0;
                    var b = this.outputRom[System.Array.index(location, this.outputRom)];
                    b = b & 240;
                    b = b >> 4;
                    ret = TSBTool.TecmoTool.positionNames.getItem(b);
                    return ret;
                }
            },
            SetPuntReturner: function (team, position) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    TSBTool.TecmoTool.prototype.SetPuntReturner.call(this, team, position);
                    return;
                } else {
                    var location = (((this.FORTY_NINERS_KR_PR_LOC + TSBTool.TecmoTool.GetTeamIndex(team)) | 0) - 30) | 0;
                    var location1 = (((this.FORTY_NINERS_KR_PR_LOC_1 + TSBTool.TecmoTool.GetTeamIndex(team)) | 0) - 30) | 0;
                    switch (position) {
                        case "QB1": 
                        case "QB2": 
                        case "C": 
                        case "LG": 
                        case "RB1": 
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                        case "WR1": 
                        case "WR2": 
                        case "WR3": 
                        case "WR4": 
                        case "TE1": 
                        case "TE2": 
                            var pos = TSBTool.TecmoTool.positionNames.indexOf(position);
                            var b = this.outputRom[System.Array.index(location, this.outputRom)];
                            b = b & 240;
                            b = (b + pos) | 0;
                            this.outputRom[System.Array.index(location, this.outputRom)] = b & 255;
                            this.outputRom[System.Array.index(location1, this.outputRom)] = b & 255;
                            break;
                        default: 
                            TSBTool.StaticUtils.AddError(System.String.format("Cannot assign '{0}' as a punt returner", [position]));
                            break;
                    }
                }
            },
            SetKickReturner: function (team, position) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    TSBTool.TecmoTool.prototype.SetKickReturner.call(this, team, position);
                    return;
                } else {
                    var location = (((this.FORTY_NINERS_KR_PR_LOC + TSBTool.TecmoTool.GetTeamIndex(team)) | 0) - 30) | 0;
                    var location2 = (((this.FORTY_NINERS_KR_PR_LOC_1 + TSBTool.TecmoTool.GetTeamIndex(team)) | 0) - 30) | 0;
                    switch (position) {
                        case "QB1": 
                        case "QB2": 
                        case "C": 
                        case "LG": 
                        case "RB1": 
                        case "RB2": 
                        case "RB3": 
                        case "RB4": 
                        case "WR1": 
                        case "WR2": 
                        case "WR3": 
                        case "WR4": 
                        case "TE1": 
                        case "TE2": 
                            var pos = TSBTool.TecmoTool.positionNames.indexOf(position);
                            var b = this.outputRom[System.Array.index(location, this.outputRom)];
                            b = b & 15;
                            b = (b + (pos << 4)) | 0;
                            this.outputRom[System.Array.index(location, this.outputRom)] = b & 255;
                            this.outputRom[System.Array.index(location2, this.outputRom)] = b & 255;
                            break;
                        default: 
                            TSBTool.StaticUtils.AddError(System.String.format("Cannot assign '{0}' as a kick returner", [position]));
                            break;
                    }
                }
            },
            GetAll: function () {
                var team;
                var all = new System.Text.StringBuilder("", Bridge.Int.mul(1230, TSBTool.TecmoTool.positionNames.Count));
                var year = System.String.format("YEAR={0}\n", [this.GetYear()]);
                all.append(year);
                var normalTeamEnd = 28;
                for (var i = 0; i < normalTeamEnd; i = (i + 1) | 0) {
                    team = TSBTool.TecmoTool.teams.getItem(i);
                    all.append(this.GetTeamPlayers(team));
                }
                var expansionTeams = this.GetExpansionTeams();
                all.append(expansionTeams);

                return all.toString();
            },
            GetTeamName: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(((teamIndex + 68) | 0));
                return retVal;
            },
            GetTeamCity: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(((teamIndex + 34) | 0));
                return retVal;
            },
            GetTeamAbbreviation: function (teamIndex) {
                var retVal = this.GetTeamStringTableString(teamIndex);
                return retVal;
            },
            SetTeamAbbreviation: function (teamIndex, abb) {
                if (abb != null && abb.length === 4) {
                    this.SetTeamStringTableString(teamIndex, abb);
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Error setting team abbreviation, teamIndex={0}; value length must == 4; {1}", Bridge.box(teamIndex, System.Int32), abb));
                }
            },
            SetTeamName: function (teamIndex, name) {
                this.SetTeamStringTableString(((teamIndex + 68) | 0), name);
            },
            SetTeamCity: function (teamIndex, city) {
                this.SetTeamStringTableString(((teamIndex + 34) | 0), city);
            },
            /**
             * Offensive formation is messed up in CXROM
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team
             * @return  {string}
             */
            GetTeamOffensiveFormation: function (team) {
                var retVal = "";
                if (this.mRomType !== TSBTool.ROM_TYPE.CXROM_v111) {
                    retVal = TSBTool.TecmoTool.prototype.GetTeamOffensiveFormation.call(this, team);
                }
                return retVal;
            },
            GetExpansionTeams: function () {
                var ret = new System.Text.StringBuilder("", 2000);

                ret.append(this.GetTeamPlayers(TSBTool.TecmoTool.teams.getItem(30)));
                ret.append(this.GetTeamPlayers(TSBTool.TecmoTool.teams.getItem(31)));
                ret.append(this.GetTeamPlayers(TSBTool.TecmoTool.teams.getItem(32)));
                ret.append(this.GetTeamPlayers(TSBTool.TecmoTool.teams.getItem(33)));

                var result = ret.toString();
                return result;
            },
            /**
             * Sets the team sim offense tendency . 
             00 = Little more rushing, 01 = Heavy Rushing, 
             02 = little more passing, 03 = Heavy Passing.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}     team    the team name
             * @param   {number}     val     the number to set it to.
             * @return  {boolean}            true if set, fales if could not set it.
             */
            SetTeamSimOffensePref: function (team, val) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    return TSBTool.TecmoTool.prototype.SetTeamSimOffensePref.call(this, team, val);
                }

                if (val > -1 && val < 4 && teamIndex !== -1) {
                    var loc = (((this.fortyNinersRunPassPreferenceLoc + teamIndex) | 0) - 30) | 0;
                    this.outputRom[System.Array.index(loc, this.outputRom)] = val & 255;
                } else {
                    if (teamIndex !== -1) {
                        TSBTool.StaticUtils.AddError(System.String.format("Can't set offensive pref to '{0}' valid values are 0-3.\n", [Bridge.box(val, System.Int32)]));
                    } else {
                        TSBTool.StaticUtils.AddError(System.String.format("Team '{0}' is invalid\n", [team]));
                    }
                }
                return true;
            },
            /**
             * Sets the team sim offense tendency . 
             00 = Little more rushing, 01 = Heavy Rushing, 
             02 = little more passing, 03 = Heavy Passing.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team    Teh team name.
             * @return  {number}            their sim offense pref (0 - 3)
             */
            GetTeamSimOffensePref: function (team) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    return TSBTool.TecmoTool.prototype.GetTeamSimOffensePref.call(this, team);
                }

                var val = -1;
                if (teamIndex > -1) {
                    var loc = (((this.fortyNinersRunPassPreferenceLoc + teamIndex) | 0) - 30) | 0;
                    val = this.outputRom[System.Array.index(loc, this.outputRom)];
                } else {
                    TSBTool.StaticUtils.AddError(System.String.format("Team '{0}' is invalid\n", [team]));
                }
                return val;
            },
            GetOffensivePlayerSimDataLocation: function (team, position) {
                var location = -4;

                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    location = TSBTool.TecmoTool.prototype.GetOffensivePlayerSimDataLocation.call(this, team, position);
                } else if (teamIndex > 29) {

                    var positionIndex = this.GetPositionIndex(position);
                    location = (((Bridge.Int.mul((((teamIndex - 30) | 0)), this.teamSimOffset) + (Bridge.Int.mul(positionIndex, 2))) | 0) + this.fortyNinersQB1SimAttrStart) | 0;
                }
                return location;
            },
            GetDefinsivePlayerSimDataLocation: function (team, position) {
                var location = -4;

                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    location = TSBTool.TecmoTool.prototype.GetDefinsivePlayerSimDataLocation.call(this, team, position);
                } else if (teamIndex > 29) {
                    var positionIndex = this.GetPositionIndex(position);
                    location = (((Bridge.Int.mul((((teamIndex - 30) | 0)), this.teamSimOffset) + (((positionIndex - 17) | 0))) | 0) + this.fortyNinersRESimLoc) | 0;
                }
                return location;
            },
            GetPunkKickSimDataLocation: function (teamIndex) {
                var ret = -1;

                if (teamIndex < 28) {
                    ret = TSBTool.TecmoTool.prototype.GetPunkKickSimDataLocation.call(this, teamIndex);
                } else {
                    ret = (((Bridge.Int.mul((((teamIndex - 30) | 0)), this.teamSimOffset) + this.fortyNinersQB1SimAttrStart) | 0) + 46) | 0;
                }

                return ret;
            },
            /**
             * Returns the simulation data for the given team.
             Simulation data is of the form '0xNN' where N is a number 1-F (hex).
             A team's sim data of '0x57' signifies that the team has a simulation figure of
             '5' for offense, and '7' for defense.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team    The team of interest
             * @return  {number}
             */
            GetTeamSimData: function (team) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    return TSBTool.TecmoTool.prototype.GetTeamSimData.call(this, team);
                }

                if (teamIndex > 29 && teamIndex < 34) {
                    var location = (((Bridge.Int.mul((((teamIndex - 30) | 0)), this.teamSimOffset) + this.fortyNinersQB1SimAttrStart) | 0) + 47) | 0;
                    return this.outputRom[System.Array.index(location, this.outputRom)];
                }
                return 0;
            },
            /**
             * Sets the given team's offense and defense sim values.
             Simulation data is of the form '0xNN' where N is a number 1-F (hex).
             A team's sim data of '0x57' signifies that the team has a simulation figure of
             '5' for offense, and '7' for defense.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team      The team to set.
             * @param   {number}    values    The value to set it to.
             * @return  {void}
             */
            SetTeamSimData: function (team, values) {
                if (!this.IsValidTeam(team)) {
                    TSBTool.StaticUtils.AddError(System.String.format("ERROR! (low level) SetTeamSimData:: team {0} is invalid ", [team]));
                    return;
                }

                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    TSBTool.TecmoTool.prototype.SetTeamSimData.call(this, team, values);
                } else {
                    var location = (((Bridge.Int.mul((((teamIndex - 30) | 0)), this.teamSimOffset) + this.fortyNinersQB1SimAttrStart) | 0) + 47) | 0;
                    var currentValue = this.outputRom[System.Array.index(location, this.outputRom)];
                    this.outputRom[System.Array.index(location, this.outputRom)] = values;
                    currentValue = this.outputRom[System.Array.index(location, this.outputRom)];
                }
            },
            /**
             * Gets the point in the player number name data that a player's data begins.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetDataPosition: function (team, position) {
                var ret = -1;
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetDataPosition:: either team {0} or position {1} is invalid.", team, position));
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    return TSBTool.TecmoTool.prototype.GetDataPosition.call(this, team, position);
                }
                if (teamIndex > 29) {
                    var positionIndex = this.GetPositionIndex(position);
                    var pointerLocation = 0;

                    pointerLocation = (((Bridge.Int.mul((((teamIndex - 30) | 0)), 60) + this.FORTY_NINERS_QB1_POINTER) | 0) + (Bridge.Int.mul(positionIndex, 2))) | 0;

                    var lowByte = this.outputRom[System.Array.index(pointerLocation, this.outputRom)];
                    var hiByte = this.outputRom[System.Array.index(((pointerLocation + 1) | 0), this.outputRom)];
                    hiByte = hiByte << 8;
                    hiByte = (hiByte + lowByte) | 0;

                    ret = (hiByte + this.mGetDataPositionOffset) | 0;
                }
                return ret;
            },
            /**
             * Get the starting point of the guy AFTER the one passed to this method.
             This is hacked up to work with CXROM's rom.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetNextDataPosition: function (team, position) {
                var pointerLocation = 0;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);

                if (teamIndex > 29 && Bridge.referenceEquals(position, "P")) {
                    pointerLocation = (((this.FORTY_NINERS_QB1_POINTER + 60) | 0) + Bridge.Int.mul((((teamIndex - 30) | 0)), 60)) | 0;
                } else if (teamIndex > 29) {
                    var positionIndex = (this.GetPositionIndex(position) + 1) | 0;
                    pointerLocation = (((Bridge.Int.mul((((teamIndex - 30) | 0)), 60) + this.FORTY_NINERS_QB1_POINTER) | 0) + (Bridge.Int.mul(positionIndex, 2))) | 0;
                }

                if (pointerLocation !== 0) {
                    var lowByte = this.outputRom[System.Array.index(pointerLocation, this.outputRom)];
                    var hiByte = this.outputRom[System.Array.index(((pointerLocation + 1) | 0), this.outputRom)];
                    hiByte = hiByte << 8;
                    hiByte = (hiByte + lowByte) | 0;

                    var ret = (hiByte + this.mGetDataPositionOffset) | 0;
                    return ret;
                }

                return TSBTool.TecmoTool.prototype.GetNextDataPosition.call(this, team, position);
            },
            GetPointerPosition: function (team, position) {
                var ret = -4;
                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) GetPointerPosition:: either team {0} or position {1} is invalid.", team, position));
                }
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    return TSBTool.TecmoTool.prototype.GetPointerPosition.call(this, team, position);
                }
                if (teamIndex > 29) {
                    var positionIndex = this.GetPositionIndex(position);
                    var playerSpot = (Bridge.Int.mul((((teamIndex - 30) | 0)), TSBTool.TecmoTool.positionNames.Count) + positionIndex) | 0;
                    if (positionIndex < 0) {
                        var builder = new System.Text.StringBuilder("", 500);
                        builder.append("ERROR! (low level) Position '{0}' does not exist. Valid positions are: [");
                        for (var i = 1; i <= TSBTool.TecmoTool.positionNames.Count; i = (i + 1) | 0) {
                            builder.append(TSBTool.TecmoTool.positionNames.getItem(((i - 1) | 0)));
                            builder.append(",");
                        }
                        builder.remove(((builder.getLength() - 1) | 0), 1);
                        builder.append("]");
                        TSBTool.StaticUtils.AddError(builder.toString());
                        return -1;
                    }
                    ret = (this.FORTY_NINERS_QB1_POINTER + (Bridge.Int.mul(2, playerSpot))) | 0;
                }
                return ret;
            },
            ShiftDataAfter: function (team, position, shiftAmount) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex === 27 && Bridge.referenceEquals(position, "P")) {
                    return;
                }

                if (teamIndex < 28) {
                    TSBTool.TecmoTool.prototype.ShiftDataAfter.call(this, team, position, shiftAmount);
                    return;
                }

                if (!this.IsValidTeam(team) || !this.IsValidPosition(position)) {
                    throw new System.Exception(System.String.format("ERROR! (low level) ShiftDataAfter:: either team {0} or position {1} is invalid.", team, position));
                }

                if (Bridge.referenceEquals(team, TSBTool.TecmoTool.teams.getItem(((TSBTool.TecmoTool.teams.Count - 1) | 0))) && Bridge.referenceEquals(position, "P")) {
                    return;
                }

                var startPosition = this.GetNextDataPosition(team, position);
                var endPosition = (this.m_ExpansionSegmentEnd - 17) | 0;

                if (shiftAmount < 0) {
                    this.ShiftDataUp(startPosition, endPosition, shiftAmount, this.outputRom);
                } else {
                    if (shiftAmount > 0) {
                        this.ShiftDataDown(startPosition, endPosition, shiftAmount, this.outputRom);
                    }
                }
            },
            AdjustDataPointers: function (pos, change, lastPointer) {
                if (pos >= this.GetTeamStringTableStart()) {
                    TSBTool.TecmoTool.prototype.AdjustDataPointers.call(this, pos, change, lastPointer);
                    return;
                }
                if (pos === ((this.lastPlayerNamePointer - 2) | 0)) {
                    var pointerLoc = (pos + 2) | 0;
                    var lo = this.outputRom[System.Array.index(pointerLoc, this.outputRom)];
                    var hi = this.outputRom[System.Array.index(((pointerLoc + 1) | 0), this.outputRom)];
                    var pVal = hi;
                    pVal = pVal << 8;
                    pVal = (pVal + lo) | 0;
                    pVal = (pVal + change) | 0;

                    lo = (pVal & 255) & 255;
                    pVal = pVal >> 8;
                    hi = pVal & 255;
                    this.outputRom[System.Array.index(pointerLoc, this.outputRom)] = lo;
                    this.outputRom[System.Array.index(((pointerLoc + 1) | 0), this.outputRom)] = hi;
                } else if (pos < ((this.lastPlayerNamePointer + 1) | 0)) {
                    TSBTool.TecmoTool.prototype.AdjustDataPointers.call(this, pos, change, this.lastPlayerNamePointer);
                } else {
                    var low, hi1;
                    var word;

                    var start = (pos + 2) | 0;
                    var i = 0;
                    var end = (((((this.FORTY_NINERS_QB1_POINTER + 60) | 0) + 180) | 0) + 2) | 0;

                    for (i = start; i < end; i = (i + 2) | 0) {
                        low = this.outputRom[System.Array.index(i, this.outputRom)];
                        hi1 = this.outputRom[System.Array.index(((i + 1) | 0), this.outputRom)];
                        word = hi1;
                        word = word << 8;
                        word = (word + low) | 0;
                        word = (word + change) | 0;
                        low = (word & 255) & 255;
                        word = word >> 8;
                        hi1 = word & 255;
                        this.outputRom[System.Array.index(i, this.outputRom)] = low;
                        this.outputRom[System.Array.index(((i + 1) | 0), this.outputRom)] = hi1;
                    }
                }
            },
            /**
             * Get the face number from the given team/position
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team        
             * @param   {string}    position
             * @return  {number}
             */
            GetFace: function (team, position) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    return TSBTool.TecmoTool.prototype.GetFace.call(this, team, position);
                }
                var positionOffset = this.GetPositionIndex(position);

                if (positionOffset < 0 || teamIndex < 0) {
                    TSBTool.StaticUtils.AddError(System.String.format("GetFace Error getting face for {0} {1}", team, position));
                    return -1;
                }
                teamIndex = (teamIndex - 2) | 0;
                var loc = (((12306 + this.faceOffsets[System.Array.index(positionOffset, this.faceOffsets)]) | 0) + Bridge.Int.mul(teamIndex, 117)) | 0;
                var ret = this.outputRom[System.Array.index(loc, this.outputRom)];
                return ret;
            },
            /**
             * Sets the face for the guy at position 'position' on team 'team'.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team        
             * @param   {string}    position    
             * @param   {number}    face
             * @return  {void}
             */
            SetFace: function (team, position, face) {
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    TSBTool.TecmoTool.prototype.SetFace.call(this, team, position, face);
                    return;
                }
                var positionOffset = this.GetPositionIndex(position);

                if (positionOffset < 0 || teamIndex < 0 || !!(face < 0 | face > 212)) {
                    TSBTool.StaticUtils.AddError(System.String.format("SetFace Error setting face for {0} {1} face={2}", team, position, Bridge.box(face, System.Int32)));
                    if (!!(face < 0 | face > 212)) {
                        TSBTool.StaticUtils.AddError(System.String.format("Valid Face numbers are 0x00 - 0xD4", null));
                    }
                    return;
                }
                teamIndex = (teamIndex - 2) | 0;
                var loc = (((12306 + this.faceOffsets[System.Array.index(positionOffset, this.faceOffsets)]) | 0) + Bridge.Int.mul(teamIndex, 117)) | 0;
                this.outputRom[System.Array.index(loc, this.outputRom)] = face & 255;
            },
            GetAttributeLocation: function (teamIndex, posIndex) {
                var location = -1;
                if (teamIndex < 28) {
                    location = TSBTool.TecmoTool.prototype.GetAttributeLocation.call(this, teamIndex, posIndex);
                } else {
                    location = TSBTool.TecmoTool.prototype.GetAttributeLocation.call(this, ((teamIndex - 2) | 0), posIndex);
                }
                return location;
            },
            /**
             * Returns an ArrayList of errors that were encountered during the operation.
             *
             * @instance
             * @public
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {System.Collections.Generic.List$1}    scheduleList
             * @return  {void}
             */
            ApplySchedule: function (scheduleList) {
                if (scheduleList != null && this.outputRom != null) {
                    var sch = new TSBTool.CXRomScheduleHelper(this.outputRom);
                    sch.ApplySchedule(scheduleList);
                }
            },
            GetPlaybookLocation: function (team_index) {
                if (team_index < 28) {
                    return TSBTool.TecmoTool.prototype.GetPlaybookLocation.call(this, team_index);
                } else {
                    team_index = (team_index - 30) | 0;
                    return ((TSBTool.CXRomTSBTool.FORTY_NINERS_PLAYBOOK_START + Bridge.Int.mul(team_index, 4)) | 0);
                }
            },
            GetSchedule: function () {
                var ret = "";
                if (this.outputRom != null && this.DoSchedule) {
                    var sh2 = new TSBTool.CXRomScheduleHelper(this.outputRom);
                    ret = sh2.GetSchedule();
                    TSBTool.StaticUtils.ShowErrors();
                }

                return ret;
            },
            GetUniformLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    ret = TSBTool.TecmoTool.prototype.GetUniformLoc.call(this, team);
                } else {
                    teamIndex = (teamIndex - 30) | 0;
                    ret = (this.mFortyNinersUniformLoc + (Bridge.Int.mul(teamIndex, 10))) | 0;
                }
                return ret;
            },
            /**
             * Gets the location of the given team's uniform data.
             *
             * @instance
             * @protected
             * @override
             * @this TSBTool.CXRomTSBTool
             * @memberof TSBTool.CXRomTSBTool
             * @param   {string}    team
             * @return  {number}            The location of the given team's uniform data, -1 on error
             */
            GetActionSeqUniformLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    ret = TSBTool.TecmoTool.prototype.GetActionSeqUniformLoc.call(this, team);
                } else {
                    teamIndex = (teamIndex - 30) | 0;
                    ret = (this.mFortyNinersActionSeqLoc + (Bridge.Int.mul(teamIndex, 8))) | 0;
                }
                return ret;
            },
            GetDivChampLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);
                if (teamIndex < 28) {
                    ret = TSBTool.TecmoTool.prototype.GetDivChampLoc.call(this, team);
                } else {
                    teamIndex = (teamIndex - 30) | 0;
                    ret = (this.m49ersDivChampLoc + (Bridge.Int.mul(teamIndex, 5))) | 0;
                }

                return ret;
            },
            GetConfChampLoc: function (team) {
                var ret = -1;
                var teamIndex = TSBTool.TecmoTool.GetTeamIndex(team);

                if (teamIndex < 28) {
                    ret = TSBTool.TecmoTool.prototype.GetConfChampLoc.call(this, team);
                } else {
                    teamIndex = (teamIndex - 30) | 0;
                    ret = (this.m49ersConfChampLoc + (Bridge.Int.mul(teamIndex, 4))) | 0;
                }
                return ret;
            }
        }
    });

    Bridge.define("TSBTool2.TSB3Tool", {
        inherits: [TSBTool2.TSB2Tool],
        statics: {
            fields: {
                scheduleTeamOrder: null
            },
            ctors: {
                init: function () {
                    this.scheduleTeamOrder = function (_o1) {
                            _o1.add("dolphins");
                            _o1.add("patriots");
                            _o1.add("colts");
                            _o1.add("bills");
                            _o1.add("jets");
                            _o1.add("steelers");
                            _o1.add("browns");
                            _o1.add("bengals");
                            _o1.add("oilers");
                            _o1.add("jaguars");
                            _o1.add("chargers");
                            _o1.add("chiefs");
                            _o1.add("raiders");
                            _o1.add("broncos");
                            _o1.add("seahawks");
                            _o1.add("cowboys");
                            _o1.add("giants");
                            _o1.add("cardinals");
                            _o1.add("eagles");
                            _o1.add("redskins");
                            _o1.add("vikings");
                            _o1.add("packers");
                            _o1.add("lions");
                            _o1.add("bears");
                            _o1.add("buccaneers");
                            _o1.add("49ers");
                            _o1.add("saints");
                            _o1.add("falcons");
                            _o1.add("rams");
                            _o1.add("panthers");
                            return _o1;
                        }(new (System.Collections.Generic.List$1(System.String)).ctor());
                }
            },
            methods: {
                IsTecmoSuperBowl3Rom: function (rom) {
                    var retVal = false;
                    if (rom != null && rom.length > 0) {
                        var results = TSBTool.StaticUtils.FindStringInFile("TECMO SUPERBOWL 3", rom, 1828208, 1833152);
                        if (results.Count > 0) {
                            retVal = true;
                        }
                    }
                    return retVal;
                }
            }
        },
        fields: {
            seventeenWeeks$1: null
        },
        props: {
            RomVersion: {
                get: function () {
                    return TSBTool.ROM_TYPE.SNES_TSB3;
                }
            }
        },
        alias: [
            "GetKey", "TSBTool$ITecmoContent$GetKey",
            "GetKey", "TSBTool2$ITecmoTool$GetKey",
            "SetFace", "TSBTool2$ITecmoTool$SetFace",
            "SetQBAbilities", "TSBTool2$ITecmoTool$SetQBAbilities",
            "SetOLPlayerAbilities", "TSBTool2$ITecmoTool$SetOLPlayerAbilities",
            "SetKickerAbilities", "TSBTool2$ITecmoTool$SetKickerAbilities",
            "SetPunterAbilities", "TSBTool2$ITecmoTool$SetPunterAbilities",
            "SetSkillPlayerAbilities", "TSBTool2$ITecmoTool$SetSkillPlayerAbilities",
            "SetDefensivePlayerAbilities", "TSBTool2$ITecmoTool$SetDefensivePlayerAbilities",
            "GetSchedule", "TSBTool$ITecmoContent$GetSchedule",
            "GetSchedule", "TSBTool2$ITecmoTool$GetSchedule",
            "ApplySchedule", "TSBTool2$ITecmoTool$ApplySchedule",
            "GetAll", "TSBTool2$ITecmoTool$GetAll",
            "GetAll$1", "TSBTool$ITecmoContent$GetAll",
            "GetAll$1", "TSBTool2$ITecmoTool$GetAll$1",
            "RomVersion", "TSBTool$ITecmoContent$RomVersion"
        ],
        ctors: {
            init: function () {
                this.seventeenWeeks$1 = System.Array.init([15, 15, 15, 12, 13, 13, 13, 13, 13, 14, 14, 15, 15, 15, 15, 15, 15], System.Int32);
            },
            $ctor1: function (rom) {
                this.$initialize();
                TSBTool2.TSB2Tool.ctor.call(this);
                this.OutputRom = rom;
                this.Init$1();
            },
            ctor: function () {
                this.$initialize();
                TSBTool2.TSB2Tool.ctor.call(this);
                this.Init$1();
            }
        },
        methods: {
            /**
             * Overrides for setting up locations
             *
             * @instance
             * @private
             * @this TSBTool2.TSB3Tool
             * @memberof TSBTool2.TSB3Tool
             * @return  {void}
             */
            Init$1: function () {
                this.BYTES_PER_QB = 7;
                this.tsb2_name_string_table_1_first_ptr = 1998912;
                this.bills_kr_loc_season_1 = 938154;

                TSBTool2.TSB2Tool.teams = function (_o1) {
                        _o1.add("bills");
                        _o1.add("colts");
                        _o1.add("dolphins");
                        _o1.add("patriots");
                        _o1.add("jets");
                        _o1.add("bengals");
                        _o1.add("browns");
                        _o1.add("oilers");
                        _o1.add("jaguars");
                        _o1.add("steelers");
                        _o1.add("broncos");
                        _o1.add("chiefs");
                        _o1.add("raiders");
                        _o1.add("chargers");
                        _o1.add("seahawks");
                        _o1.add("cardinals");
                        _o1.add("cowboys");
                        _o1.add("giants");
                        _o1.add("eagles");
                        _o1.add("redskins");
                        _o1.add("bears");
                        _o1.add("lions");
                        _o1.add("packers");
                        _o1.add("vikings");
                        _o1.add("buccaneers");
                        _o1.add("falcons");
                        _o1.add("panthers");
                        _o1.add("saints");
                        _o1.add("rams");
                        _o1.add("49ers");
                        _o1.add("freeAgents");
                        _o1.add("allTime");
                        return _o1;
                    }(new (System.Collections.Generic.List$1(System.String)).ctor());
            },
            GetKey: function () {
                return System.String.format("# TSBTool Forum: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/\r\n# TSBTool2 Forum: https://tecmobowl.org/forums/topic/71072-tsbii-tsbiii-editor-tsbtool2/\r\n# Editing: Tecmo Super Bowl III (snes) [{0}]\r\n# TSBIII Hacking documentation: https://tecmobowl.org/forums/topic/53029-tecmo-super-bowl-iii-hackingresource-documentation/\r\n# Key \r\n# 'SET' commands are supported\r\n# Double click on a team name (or playbook) to bring up the edit Team GUI.\r\n# Double click on a player to bring up the edit player GUI (Click 'Sim Data'\r\n#   button to find out more on Sim Data).\r\n# Attribute Order\r\n# QBs   RS RP MS HP BB AG PS PC PA AR CO [sim vals]\r\n# Skill RS RP MS HP BB AG BC RC [sim vals]\r\n# OL    RS RP MS HP BB AG \r\n# DEF   RS RP MS HP BB AG PI QU [sim vals]\r\n# K     RS RP MS HP BB AG KP KA AB [sim val]\r\n# P     RS RP MS HP BB AG KP AB [sim val]\r\n", [Bridge.box(this.RomVersion, TSBTool.ROM_TYPE, System.Enum.toStringFn(TSBTool.ROM_TYPE))]);
            },
            GetPlayer: function (season, team, builder, position) {
                TSBTool2.TSB2Tool.prototype.GetPlayer.call(this, 1, team, builder, position);
            },
            GetFace: function (season, team, position) {
                var $t;
                var location = (this.GetPlayerAttributeLocation(season, team, position) + 3) | 0;
                var retVal = ($t = this.OutputRom)[System.Array.index(location, $t)];
                return retVal;
            },
            SetFace: function (season, team, position, face) {
                TSBTool.StaticUtils.CheckTSB2Args$1(season, team, position);
                var location = (this.GetPlayerAttributeLocation(season, team, position) + 3) | 0;
                this.SetByte(location, (face & 255));
            },
            GetQBAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7, $t8, $t9, $t10, $t11, $t12, $t13, $t14;
                var location = this.GetPlayerAttributeLocation(season, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);
                var bb = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t4)]);
                var ag = TSBTool.StaticUtils.GetSecondNibble(($t5 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t5)]);
                var ps = TSBTool.StaticUtils.GetFirstNibble(($t6 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t6)]);
                var pc = TSBTool.StaticUtils.GetSecondNibble(($t7 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t7)]);
                var pa = TSBTool.StaticUtils.GetFirstNibble(($t8 = this.OutputRom)[System.Array.index(((location + 5) | 0), $t8)]);
                var ar = TSBTool.StaticUtils.GetSecondNibble(($t9 = this.OutputRom)[System.Array.index(((location + 5) | 0), $t9)]);
                var co = TSBTool.StaticUtils.GetFirstNibble(($t10 = this.OutputRom)[System.Array.index(((location + 6) | 0), $t10)]);
                var sp = TSBTool.StaticUtils.GetSecondNibble(($t11 = this.OutputRom)[System.Array.index(((location + 6) | 0), $t11)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb, ag, ps, pc, pa, ar, co], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                if (TSBTool2.TSB2Tool.ShowPlayerSimData) {
                    location = this.GetSimLocation(1, team, position);
                    retVal = (retVal || "") + ((System.String.format("[{0:X2},{1:X2},{2:X2}]", Bridge.box(($t12 = this.OutputRom)[System.Array.index(location, $t12)], System.Byte), Bridge.box(($t13 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t13)], System.Byte), Bridge.box(($t14 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t14)], System.Byte))) || "");
                }
                return retVal;
            },
            SetQBAbilities: function (season, team, qb, abilities) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                if (!Bridge.referenceEquals(qb, "QB1") && !Bridge.referenceEquals(qb, "QB2")) {
                    throw new System.ArgumentException.$ctor1("Invalid qb position " + (qb || ""));
                }

                var location = this.GetPlayerAttributeLocation(1, team, qb);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], abilities[System.Array.index(5, abilities)]);

                var ps_pc = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(6, abilities)], abilities[System.Array.index(7, abilities)]);
                var pa_ar = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(8, abilities)], abilities[System.Array.index(9, abilities)]);
                var unk1 = TSBTool.StaticUtils.GetSecondNibble(($t = this.OutputRom)[System.Array.index(((location + 6) | 0), $t)]);
                var co_unk1 = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(10, abilities)], unk1);

                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 2) | 0), bb_ag);
                this.SetByte(((location + 4) | 0), ps_pc);
                this.SetByte(((location + 5) | 0), pa_ar);
                this.SetByte(((location + 6) | 0), co_unk1);
            },
            GetOLPlayerAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5;
                var location = this.GetPlayerAttributeLocation(1, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);
                var bb = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t4)]);
                var ag = TSBTool.StaticUtils.GetSecondNibble(($t5 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t5)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb, ag], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                return retVal;
            },
            SetOLPlayerAbilities: function (season, team, pos, abilities) {
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var posIndex = TSBTool2.TSB2Tool.positionNames.indexOf(pos);
                if (posIndex < 12 || posIndex > 16) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes C,RG,RT,LG,LT) " + (pos || ""));
                }

                var location = this.GetPlayerAttributeLocation(1, team, pos);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], abilities[System.Array.index(5, abilities)]);
                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 2) | 0), bb_ag);
            },
            GetKickerAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7, $t8, $t9;
                var location = this.GetPlayerAttributeLocation(1, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);

                var bb = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t4)]);
                var ag = TSBTool.StaticUtils.GetSecondNibble(($t5 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t5)]);

                var kp = TSBTool.StaticUtils.GetFirstNibble(($t6 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t6)]);
                var ka = TSBTool.StaticUtils.GetSecondNibble(($t7 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t7)]);
                var ab = TSBTool.StaticUtils.GetFirstNibble(($t8 = this.OutputRom)[System.Array.index(((location + 5) | 0), $t8)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb, ag, kp, ka, ab], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                if (TSBTool2.TSB2Tool.ShowPlayerSimData) {
                    location = this.GetSimLocation(1, team, position);
                    retVal = (retVal || "") + ((System.String.format("[{0:X}]", [Bridge.box((($t9 = this.OutputRom)[System.Array.index(location, $t9)] >> 4), System.Int32)])) || "");
                }
                return retVal;
            },
            SetKickerAbilities: function (season, team, position, abilities) {
                var $t;
                TSBTool.StaticUtils.CheckTSB2Args(1, team);
                if (!Bridge.referenceEquals(position, "K")) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes K) " + (position || ""));
                }

                var location = this.GetPlayerAttributeLocation(1, team, position);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], abilities[System.Array.index(5, abilities)]);
                var kp_ka = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(6, abilities)], abilities[System.Array.index(7, abilities)]);

                var unk1 = TSBTool.StaticUtils.GetSecondNibble(($t = this.OutputRom)[System.Array.index(((location + 5) | 0), $t)]);
                var ab_unk1 = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(8, abilities)], unk1);

                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 2) | 0), bb_ag);
                this.SetByte(((location + 4) | 0), kp_ka);
                this.SetByte(((location + 5) | 0), ab_unk1);
            },
            GetPunterAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7, $t8;
                var location = this.GetPlayerAttributeLocation(1, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);

                var bb = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t4)]);
                var ag = TSBTool.StaticUtils.GetSecondNibble(($t5 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t5)]);

                var kp = TSBTool.StaticUtils.GetFirstNibble(($t6 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t6)]);
                var ab = TSBTool.StaticUtils.GetSecondNibble(($t7 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t7)]);

                var attrs = System.Array.init([rs, rp, ms, hp, bb, ag, kp, ab], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                if (TSBTool2.TSB2Tool.ShowPlayerSimData) {
                    location = this.GetSimLocation(1, team, position);
                    retVal = (retVal || "") + ((System.String.format("[{0:X}]", [Bridge.box((($t8 = this.OutputRom)[System.Array.index(location, $t8)] & 15), System.Int32)])) || "");
                }
                return retVal;
            },
            SetPunterAbilities: function (season, team, position, abilities) {
                TSBTool.StaticUtils.CheckTSB2Args(1, team);
                if (!Bridge.referenceEquals(position, "P")) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes P) " + (position || ""));
                }

                var location = this.GetPlayerAttributeLocation(1, team, position);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], abilities[System.Array.index(5, abilities)]);

                var kp_ab = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(6, abilities)], abilities[System.Array.index(7, abilities)]);

                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 2) | 0), bb_ag);
                this.SetByte(((location + 4) | 0), kp_ab);
            },
            GetPlayerAbilities: function (season, team, position) {
                return TSBTool2.TSB2Tool.prototype.GetPlayerAbilities.call(this, 1, team, position);
            },
            GetSkill_DefPlayerAbilities: function (season, team, position) {
                var $t, $t1, $t2, $t3, $t4, $t5, $t6, $t7, $t8, $t9, $t10, $t11;
                var location = this.GetPlayerAttributeLocation(1, team, position);
                var rs = TSBTool.StaticUtils.GetFirstNibble(($t = this.OutputRom)[System.Array.index(location, $t)]);
                var rp = TSBTool.StaticUtils.GetSecondNibble(($t1 = this.OutputRom)[System.Array.index(location, $t1)]);
                var ms = TSBTool.StaticUtils.GetFirstNibble(($t2 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t2)]);
                var hp = TSBTool.StaticUtils.GetSecondNibble(($t3 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t3)]);

                var bb = TSBTool.StaticUtils.GetFirstNibble(($t4 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t4)]);
                var ag = TSBTool.StaticUtils.GetSecondNibble(($t5 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t5)]);
                var bc = TSBTool.StaticUtils.GetFirstNibble(($t6 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t6)]);
                var rec = TSBTool.StaticUtils.GetSecondNibble(($t7 = this.OutputRom)[System.Array.index(((location + 4) | 0), $t7)]);


                var attrs = System.Array.init([rs, rp, ms, hp, bb, ag, bc, rec], System.Byte);
                var retVal = TSBTool.StaticUtils.MapAttributes(attrs);
                if (TSBTool2.TSB2Tool.ShowPlayerSimData) {
                    location = this.GetSimLocation(1, team, position);
                    retVal = (retVal || "") + ((System.String.format("[{0:X2},{1:X2},{2:X2}", Bridge.box(($t8 = this.OutputRom)[System.Array.index(location, $t8)], System.Byte), Bridge.box(($t9 = this.OutputRom)[System.Array.index(((location + 1) | 0), $t9)], System.Byte), Bridge.box(($t10 = this.OutputRom)[System.Array.index(((location + 2) | 0), $t10)], System.Byte))) || "");
                    if (TSBTool2.TSB2Tool.positionNames.indexOf(position) < 13) {
                        retVal = (retVal || "") + (("," + (System.Byte.format(($t11 = this.OutputRom)[System.Array.index(((location + 3) | 0), $t11)], "X2") || "")) || "");
                    }
                    retVal = (retVal || "") + "]";
                }
                return retVal;
            },
            SetSkillPlayerAbilities: function (season, team, position, abilities) {
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var posIndex = TSBTool2.TSB2Tool.positionNames.indexOf(position);
                if (posIndex < 2 || posIndex > 11) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes RB1=TE2)" + (position || ""));
                }

                var location = this.GetPlayerAttributeLocation(1, team, position);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], abilities[System.Array.index(5, abilities)]);
                var bc_rec = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(6, abilities)], abilities[System.Array.index(7, abilities)]);
                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 2) | 0), bb_ag);
                this.SetByte(((location + 4) | 0), bc_rec);
            },
            SetDefensivePlayerAbilities: function (season, team, position, abilities) {
                TSBTool.StaticUtils.CheckTSB2Args(season, team);
                var posIndex = TSBTool2.TSB2Tool.positionNames.indexOf(position);
                if (posIndex < 17 || posIndex > 34) {
                    throw new System.ArgumentException.$ctor1("Invalid position argument! (takes RE-DB3)" + (position || ""));
                }

                var location = this.GetPlayerAttributeLocation(1, team, position);
                var rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(0, abilities)], abilities[System.Array.index(1, abilities)]);
                var ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(2, abilities)], abilities[System.Array.index(3, abilities)]);
                var bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(4, abilities)], abilities[System.Array.index(5, abilities)]);
                var pi_qu = TSBTool.StaticUtils.CombineNibbles(abilities[System.Array.index(6, abilities)], abilities[System.Array.index(7, abilities)]);
                this.SetByte(location, rs_rp);
                this.SetByte(((location + 1) | 0), ms_hp);
                this.SetByte(((location + 2) | 0), bb_ag);
                this.SetByte(((location + 4) | 0), pi_qu);
            },
            GetSchedule: function (season) {
                var helper = new TSBTool2.SNES_TSB3_ScheduleHelper(this);
                helper.SetWeekOneLocation(TSBTool2.TSB2Tool.schedule_start_season_1, this.seventeenWeeks$1, TSBTool2.TSB3Tool.scheduleTeamOrder);
                return helper.GetSchedule();
            },
            ApplySchedule: function (season, scheduleList) {
                var helper = new TSBTool2.SNES_TSB3_ScheduleHelper(this);
                helper.SetWeekOneLocation(TSBTool2.TSB2Tool.schedule_start_season_1, this.seventeenWeeks$1, TSBTool2.TSB3Tool.scheduleTeamOrder);
                helper.ApplySchedule(scheduleList);
            },
            GetAll: function () {
                return this.GetAll$1(1);
            },
            GetAll$1: function (season) {
                return TSBTool2.TSB2Tool.prototype.GetAll$1.call(this, 1);
            }
        }
    });
});
